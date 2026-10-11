using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Zelloa.Application.Payments;

namespace Zelloa.Infrastructure.Payments;

public sealed class AsaasSandboxPaymentGateway(HttpClient httpClient, IConfiguration configuration)
    : IPaymentGateway
{
    private const string ProviderName = "AsaasSandbox";

    public async Task<PixChargeResult> CreatePixChargeAsync(Guid paymentId, decimal amount,
        DateTimeOffset expiresAt, CancellationToken cancellationToken)
    {
        var customerId = RequiredSetting("Payments:Asaas:SandboxCustomerId");
        using var createResponse = await httpClient.PostAsJsonAsync("payments", new CreateChargeRequest(
            customerId,
            "PIX",
            amount,
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(expiresAt,
                TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows()
                    ? "E. South America Standard Time"
                    : "America/Sao_Paulo")).DateTime)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            $"Pedido Zelloa {paymentId:N}",
            paymentId.ToString("D")), cancellationToken);
        await EnsureSuccessAsync(createResponse, cancellationToken);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateChargeResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Asaas Sandbox returned an empty charge response.");

        using var qrResponse = await httpClient.GetAsync($"payments/{Uri.EscapeDataString(created.Id)}/pixQrCode",
            cancellationToken);
        await EnsureSuccessAsync(qrResponse, cancellationToken);
        var qrCode = await qrResponse.Content.ReadFromJsonAsync<PixQrCodeResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Asaas Sandbox returned an empty Pix QR response.");
        if (string.IsNullOrWhiteSpace(qrCode.Payload) || string.IsNullOrWhiteSpace(qrCode.EncodedImage))
            throw new InvalidOperationException("Asaas Sandbox did not return the Pix payload and QR image.");

        var actualExpiry = ParseProviderDate(qrCode.ExpirationDate);
        return new PixChargeResult(ProviderName, created.Id, qrCode.Payload,
            $"data:image/png;base64,{qrCode.EncodedImage}", actualExpiry, IsSimulated: false);
    }

    public async Task<string> GetPaymentStatusAsync(string externalTransactionId,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(
            $"payments/{Uri.EscapeDataString(externalTransactionId)}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        var payment = await response.Content.ReadFromJsonAsync<PaymentStatusResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Asaas Sandbox returned an empty payment status response.");
        return payment.Status;
    }

    private string RequiredSetting(string key) => configuration[key] is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"Required payment setting '{key}' is missing.");

    private static DateTimeOffset ParseProviderDate(string value)
    {
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None,
                out var explicitOffset) && (value.EndsWith('Z') || value.Contains('+')
                    || value.LastIndexOf('-') > value.IndexOf('T')))
            return explicitOffset;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var localDate))
            return new DateTimeOffset(DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified),
                TimeSpan.FromHours(-3));
        throw new InvalidOperationException("Asaas Sandbox returned an invalid Pix expiration date.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        var code = (int)response.StatusCode;
        throw new HttpRequestException($"Asaas Sandbox request failed with HTTP {code}.", null,
            response.StatusCode);
    }

    private sealed record CreateChargeRequest(
        [property: JsonPropertyName("customer")] string Customer,
        [property: JsonPropertyName("billingType")] string BillingType,
        [property: JsonPropertyName("value")] decimal Value,
        [property: JsonPropertyName("dueDate")] string DueDate,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("externalReference")] string ExternalReference);

    private sealed record CreateChargeResponse([property: JsonPropertyName("id")] string Id);

    private sealed record PixQrCodeResponse(
        [property: JsonPropertyName("encodedImage")] string EncodedImage,
        [property: JsonPropertyName("payload")] string Payload,
        [property: JsonPropertyName("expirationDate")] string ExpirationDate);

    private sealed record PaymentStatusResponse([property: JsonPropertyName("status")] string Status);
}
