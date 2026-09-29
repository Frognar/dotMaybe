using BenchmarkDotNet.Attributes;
using static DotMaybe.Prelude;

namespace DotMaybe.Benchmarks;

/// <summary>
/// The same small pipeline (halve even numbers, keep positive halves, triple them) written with plain ifs,
/// with <see cref="Nullable{T}"/>, and with maybes. D18: the maybe versions allocate nothing.
/// </summary>
[MemoryDiagnoser]
public class ChainBenchmarks
{
    private int[] _inputs = [];

    [GlobalSetup]
    public void Setup() => _inputs = [.. Enumerable.Range(-500, 1000)];

    [Benchmark(Baseline = true)]
    public int Imperative()
    {
        var total = 0;
        foreach (var input in _inputs)
        {
            if (input % 2 == 0 && input / 2 > 0)
            {
                total += input / 2 * 3;
            }
        }

        return total;
    }

    [Benchmark]
    public int NullableChain()
    {
        var total = 0;
        foreach (var input in _inputs)
        {
            int? half = input % 2 == 0 ? input / 2 : null;
            int? positive = half > 0 ? half : null;
            total += positive * 3 ?? 0;
        }

        return total;
    }

    [Benchmark]
    public int MethodChain()
    {
        var total = 0;
        foreach (var input in _inputs)
        {
            total += Half(input)
                .Filter(static half => half > 0)
                .Map(static half => half * 3)
                .OrDefault(0);
        }

        return total;
    }

    [Benchmark]
    public int QuerySyntax()
    {
        var total = 0;
        foreach (var input in _inputs)
        {
            Maybe<int> start = input;
            var tripled =
                from value in start
                from half in Half(value)
                select half > 0 ? half * 3 : 0;
            total += tripled.OrDefault(0);
        }

        return total;
    }

    [Benchmark]
    public int PatternMatching()
    {
        var total = 0;
        foreach (var input in _inputs)
        {
            total += Half(input) switch
            {
                int half when half > 0 => half * 3,
                int => 0,
                None => 0,
            };
        }

        return total;
    }

    private static Maybe<int> Half(int value) => value % 2 == 0 ? value / 2 : none;
}
