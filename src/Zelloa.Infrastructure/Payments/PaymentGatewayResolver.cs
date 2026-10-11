using Zelloa.Application.Payments;
using Microsoft.Extensions.Configuration;

namespace Zelloa.Infrastructure.Payments;

public sealed class PaymentGatewayResolver(
    IConfiguration configuration,
    DevelopmentSimulationPaymentGateway developmentSimulation,
    AsaasSandboxPaymentGateway asaasSandbox) : IPaymentGatewayResolver
{
    public IPaymentGateway ResolveConfigured() => Resolve(configuration["Payments:Provider"] ?? "");

    public IPaymentGateway Resolve(string provider) => provider switch
    {
        "DevelopmentSimulation" => developmentSimulation,
        "AsaasSandbox" => asaasSandbox,
        _ => throw new InvalidOperationException($"Payment provider '{provider}' is not registered.")
    };
}
