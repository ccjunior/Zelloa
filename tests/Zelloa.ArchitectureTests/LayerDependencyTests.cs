using System.Reflection;

namespace Zelloa.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Theory]
    [InlineData("Zelloa.Domain", "Zelloa.Application")]
    [InlineData("Zelloa.Domain", "Zelloa.Infrastructure")]
    [InlineData("Zelloa.Domain", "Zelloa.Api")]
    [InlineData("Zelloa.Application", "Zelloa.Infrastructure")]
    [InlineData("Zelloa.Application", "Zelloa.Api")]
    [InlineData("Zelloa.Infrastructure", "Zelloa.Api")]
    public void Layer_must_not_reference_forbidden_assembly(string layerName, string forbiddenName)
    {
        var layer = Assembly.Load(layerName);
        var references = layer.GetReferencedAssemblies().Select(assembly => assembly.Name);

        Assert.DoesNotContain(forbiddenName, references);
    }
}
