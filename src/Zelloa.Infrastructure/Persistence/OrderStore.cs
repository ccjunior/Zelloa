using Microsoft.EntityFrameworkCore;
using Zelloa.Application.Orders;
using Zelloa.Domain.Catalog;
using Zelloa.Domain.Orders;
using Zelloa.Domain.Schools;

namespace Zelloa.Infrastructure.Persistence;

public sealed class OrderStore(ZelloaDbContext dbContext) : IOrderStore
{
    public async Task<OrderStudentContext?> GetStudentContextAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await dbContext.Students.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == studentId, cancellationToken);
        if (student is null) return null;
        var classroom = await dbContext.Classrooms.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == student.ClassroomId, cancellationToken);
        var school = await dbContext.Schools.AsNoTracking()
            .FirstOrDefaultAsync(item => item.TenantId == student.TenantId, cancellationToken);
        return classroom is null || school is null ? null : new(student, classroom, school);
    }

    public Task<bool> GuardianHasStudentAsync(Guid guardianId, Guid studentId, CancellationToken cancellationToken) =>
        dbContext.GuardianStudents.AnyAsync(link => link.GuardianId == guardianId
            && link.StudentId == studentId, cancellationToken);

    public Task<OrderCutoff?> GetCutoffAsync(string shift, CancellationToken cancellationToken)
    {
        var shiftKey = shift.Trim().ToUpperInvariant();
        return dbContext.OrderCutoffs.FirstOrDefaultAsync(cutoff => cutoff.ShiftKey == shiftKey, cancellationToken);
    }

    public async Task<IReadOnlyList<OrderCutoff>> GetCutoffsAsync(CancellationToken cancellationToken) =>
        await dbContext.OrderCutoffs.AsNoTracking().OrderBy(cutoff => cutoff.Shift)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> GetProductsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        await dbContext.Products.Where(product => ids.Contains(product.Id)).ToListAsync(cancellationToken);

    public Task<Order?> GetOrderForGuardianAsync(Guid orderId, Guid guardianId, CancellationToken cancellationToken) =>
        dbContext.Orders.Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == orderId && order.GuardianId == guardianId, cancellationToken);

    public Task<Order?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.Orders.Include(order => order.Items)
            .FirstOrDefaultAsync(order => order.Id == orderId, cancellationToken);

    public async Task<IReadOnlyList<Order>> GetOrdersForGuardianAsync(
        Guid guardianId, CancellationToken cancellationToken) =>
        await dbContext.Orders.AsNoTracking().Include(order => order.Items)
            .Where(order => order.GuardianId == guardianId)
            .OrderByDescending(order => order.CreatedAt).Take(100).ToListAsync(cancellationToken);

    public void Add(Order order) => dbContext.Orders.Add(order);
    public void Add(OrderCutoff cutoff) => dbContext.OrderCutoffs.Add(cutoff);
    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
