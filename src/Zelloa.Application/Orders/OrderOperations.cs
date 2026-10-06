using System.Globalization;
using Zelloa.Application.Identity;
using Zelloa.Domain.Catalog;
using Zelloa.Domain.Orders;

namespace Zelloa.Application.Orders;

public sealed class OrderOperations(
    IOrderStore store, ICurrentUser currentUser, ITenantContext tenantContext, TimeProvider timeProvider)
{
    public async Task<OrderCutoffResponse> SetCutoffAsync(
        string shift, string cutoffTime, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(shift) || shift.Trim().Length > 50
            || !TimeOnly.TryParseExact(cutoffTime, "HH:mm", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var time))
            throw new OrderRuleException("InvalidCutoff", "Informe turno e horário válido no formato HH:mm.");

        var existing = await store.GetCutoffAsync(shift, cancellationToken);
        if (existing is null)
            store.Add(new OrderCutoff(Guid.NewGuid(), tenantContext.TenantId, shift, time));
        else
            existing.Update(time);
        await store.SaveChangesAsync(cancellationToken);
        return new(shift.Trim(), time);
    }

    public async Task<IReadOnlyList<OrderCutoffResponse>> GetCutoffsAsync(CancellationToken cancellationToken) =>
        (await store.GetCutoffsAsync(cancellationToken))
            .Select(cutoff => new OrderCutoffResponse(cutoff.Shift, cutoff.CutoffTime)).ToArray();

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } guardianId || request.StudentId == Guid.Empty
            || request.Items is null || request.Items.Count == 0 || request.Items.Count > 50
            || request.Items.Any(item => item.ProductId == Guid.Empty || item.Quantity is < 1 or > 99)
            || request.Items.Select(item => item.ProductId).Distinct().Count() != request.Items.Count)
            throw new OrderRuleException("InvalidOrder", "O pedido contém dados inválidos.");
        if (!await store.GuardianHasStudentAsync(guardianId, request.StudentId, cancellationToken))
            throw new OrderRuleException("StudentNotLinked", "O aluno não está vinculado ao responsável.");
        var context = await store.GetStudentContextAsync(request.StudentId, cancellationToken)
            ?? throw new OrderRuleException("StudentUnavailable", "O aluno ou sua turma não está disponível.");
        if (!context.Student.IsActive || !context.Classroom.IsActive || !context.School.IsActive)
            throw new OrderRuleException("StudentUnavailable", "O aluno ou sua turma não está disponível.");
        var cutoff = await store.GetCutoffAsync(context.Classroom.Shift, cancellationToken)
            ?? throw new OrderRuleException("OrderCutoffNotConfigured", "A escola ainda não configurou o limite para este turno.");
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById(context.School.TimeZone); }
        catch (TimeZoneNotFoundException) { throw new OrderRuleException("InvalidSchoolTimeZone", "O fuso da escola é inválido."); }
        catch (InvalidTimeZoneException) { throw new OrderRuleException("InvalidSchoolTimeZone", "O fuso da escola é inválido."); }

        var localNow = TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), zone);
        var localTime = TimeOnly.FromDateTime(localNow.DateTime);
        if (localTime > cutoff.CutoffTime)
            throw new OrderRuleException("OrderCutoffPassed", "O horário limite para pedidos deste turno já passou.");

        var products = await store.GetProductsAsync(request.Items.Select(item => item.ProductId).ToArray(), cancellationToken);
        if (products.Count != request.Items.Count)
            throw new OrderRuleException("ProductNotFound", "Um ou mais produtos não foram encontrados.");
        if (products.Any(product => !product.IsActive || !product.IsAvailable))
            throw new OrderRuleException("ProductUnavailable", "Um ou mais produtos não estão disponíveis.");
        var byId = products.ToDictionary(product => product.Id);
        var lines = request.Items.Select(item => new OrderItem(
            item.ProductId, byId[item.ProductId].Name, byId[item.ProductId].Price, item.Quantity));
        var order = new Order(Guid.NewGuid(), tenantContext.TenantId, guardianId, context.Student.Id,
            context.Classroom.Id, context.Classroom.Name, context.Classroom.Shift,
            DateOnly.FromDateTime(localNow.DateTime), timeProvider.GetUtcNow(), lines);
        store.Add(order);
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(order);
    }

    public async Task<OrderResponse?> RepeatAsync(Guid orderId, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } guardianId) return null;
        var prior = await store.GetOrderForGuardianAsync(orderId, guardianId, cancellationToken);
        if (prior is null) return null;
        return await CreateAsync(new(prior.StudentId,
            prior.Items.Select(item => new OrderLineRequest(item.ProductId, item.Quantity)).ToArray()), cancellationToken);
    }

    public async Task<OrderResponse?> CancelAsync(Guid orderId, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } guardianId) return null;
        var order = await store.GetOrderForGuardianAsync(orderId, guardianId, cancellationToken);
        if (order is null) return null;
        try { order.Cancel(); }
        catch (InvalidOperationException) { throw new OrderRuleException("OrderCannotBeCancelled", "Somente pedidos não pagos podem ser cancelados."); }
        await store.SaveChangesAsync(cancellationToken);
        return ToResponse(order);
    }

    public async Task<OrderResponse?> GetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId) return null;
        var order = currentUser.Roles.Contains(ZelloaRoles.Guardian)
            ? await store.GetOrderForGuardianAsync(orderId, userId, cancellationToken)
            : currentUser.Roles.Contains(ZelloaRoles.SchoolAdmin)
                ? await store.GetOrderAsync(orderId, cancellationToken)
                : null;
        return order is null ? null : ToResponse(order);
    }

    public async Task<IReadOnlyList<OrderResponse>> ListMineAsync(CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } guardianId) return [];
        return (await store.GetOrdersForGuardianAsync(guardianId, cancellationToken)).Select(ToResponse).ToArray();
    }

    private static OrderResponse ToResponse(Order order) => new(order.Id, order.StudentId, order.ClassroomName,
        order.Shift, order.OperationalDate, order.Status.ToString(), order.CreatedAt,
        order.Items.Select(item => new OrderItemResponse(item.ProductId, item.ProductName,
            item.UnitPrice, item.Quantity, item.Subtotal)).ToArray(), order.Total);
}
