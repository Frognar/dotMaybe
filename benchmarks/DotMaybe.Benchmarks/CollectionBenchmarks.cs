using BenchmarkDotNet.Attributes;

namespace DotMaybe.Benchmarks;

/// <summary>
/// Sequences of maybes against sequences of nullables: picking the values out, and all-or-nothing.
/// These allocate like LINQ does (enumerators, lists); the point is to stay close to the nullable versions.
/// </summary>
[MemoryDiagnoser]
public class CollectionBenchmarks
{
    private Maybe<int>[] _maybes = [];
    private int?[] _nullables = [];
    private Maybe<int>[] _complete = [];

    [Params(10, 1_000)]
    public int Size { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _maybes = [.. Enumerable.Range(0, Size).Select(static i => i % 3 == 0 ? default(Maybe<int>) : i)];
        _nullables = [.. Enumerable.Range(0, Size).Select(static i => i % 3 == 0 ? (int?)null : i)];
        _complete = [.. Enumerable.Range(0, Size).Select(static i => (Maybe<int>)i)];
    }

    [Benchmark(Baseline = true)]
    public int NullableValues() =>
        _nullables.Where(static n => n.HasValue).Select(static n => n.GetValueOrDefault()).Sum();

    [Benchmark]
    public int Choose() => _maybes.Choose().Sum();

    [Benchmark]
    public int ManualAllOrNothing()
    {
        var values = new List<int>(_complete.Length);
        foreach (var maybe in _complete)
        {
            if (maybe is not int value)
            {
                return -1;
            }

            values.Add(value);
        }

        return values.Count;
    }

    [Benchmark]
    public int Sequence() => _complete.Sequence().Map(static values => values.Count).OrDefault(-1);
}
