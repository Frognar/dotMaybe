using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// Proves that FsCheck runs properties and that the assembly-level configuration (including
/// <see cref="MaybeArbitraries"/>) is accepted. Remove once real properties exist (TP-01).
/// </summary>
public sealed class InfrastructureCanary
{
    [Property]
    public void Properties_run_with_generated_inputs(int a, int b) =>
        Assert.Equal(a + b, b + a);
}
