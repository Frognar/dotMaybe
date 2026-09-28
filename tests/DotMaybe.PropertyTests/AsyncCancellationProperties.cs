namespace DotMaybe.PropertyTests;

/// <summary>
/// D33: cancellation inside a delegate gives a canceled result in every async method, whether the delegate throws
/// <see cref="OperationCanceledException"/> before returning a task (<c>token.ThrowIfCancellationRequested()</c>)
/// or returns a canceled task. Awaiting the result throws <see cref="OperationCanceledException"/>.
/// </summary>
public sealed class AsyncCancellationProperties : IDisposable
{
    private readonly CancellationTokenSource _source = new();

    public AsyncCancellationProperties() => _source.Cancel();

    private CancellationToken Canceled => _source.Token;

    public void Dispose() => _source.Dispose();

    [Fact]
    public async Task MatchAsync_is_canceled_by_its_some_branch()
    {
        var some = Maybes.Some(1);

        await AssertCanceled(() => some.MatchAsync(ThrowingStep<int, int>(), () => Task.FromResult(0)));
        await AssertCanceled(() => some.MatchAsync(CanceledStep<int, int>(), () => Task.FromResult(0)));
        await AssertCanceled(() => some.MatchAsync(ThrowingStep<int, int>(), () => 0));
        await AssertCanceled(() => some.MatchAsync(CanceledStep<int, int>(), () => 0));
    }

    [Fact]
    public async Task MatchAsync_is_canceled_by_its_none_branch()
    {
        var empty = Maybes.Empty<int>();
        Func<int, Task<int>> some = value => Task.FromResult(value);
        Func<int> throwingSyncNone = () =>
        {
            Canceled.ThrowIfCancellationRequested();
            return 0;
        };

        await AssertCanceled(() => empty.MatchAsync(some, ThrowingStep<int>()));
        await AssertCanceled(() => empty.MatchAsync(some, CanceledStep<int>()));
        await AssertCanceled(() => empty.MatchAsync(some, throwingSyncNone));
    }

    [Fact]
    public async Task OrDefaultAsync_is_canceled_by_its_fallback()
    {
        var empty = Maybes.Empty<int>();

        await AssertCanceled(() => empty.OrDefaultAsync(ThrowingStep<int>()));
        await AssertCanceled(() => empty.OrDefaultAsync(CanceledStep<int>()));
    }

    [Fact]
    public async Task MapAsync_BindAsync_and_FilterAsync_are_canceled_by_their_step()
    {
        var some = Maybes.Some(1);

        await AssertCanceled(() => some.MapAsync(ThrowingStep<int, string>()));
        await AssertCanceled(() => some.MapAsync(CanceledStep<int, string>()));
        await AssertCanceled(() => some.BindAsync(ThrowingStep<int, Maybe<string>>()));
        await AssertCanceled(() => some.BindAsync(CanceledStep<int, Maybe<string>>()));
        await AssertCanceled(() => some.FilterAsync(ThrowingStep<int, bool>()));
        await AssertCanceled(() => some.FilterAsync(CanceledStep<int, bool>()));
    }

    private static async Task AssertCanceled<TResult>(Func<ValueTask<TResult>> call)
    {
        ValueTask<TResult> pending = default;

        var thrownByTheCall = Record.Exception(() => { pending = call(); });

        Assert.Null(thrownByTheCall);
        Assert.True(pending.IsCanceled, "Expected the result to be canceled, not faulted or successful.");
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pending);
    }

    private Func<TArg, Task<TResult>> ThrowingStep<TArg, TResult>() => _ =>
    {
        Canceled.ThrowIfCancellationRequested();
        return Task.FromResult<TResult>(default!);
    };

    private Func<TArg, Task<TResult>> CanceledStep<TArg, TResult>() => _ => Task.FromCanceled<TResult>(Canceled);

    private Func<Task<TResult>> ThrowingStep<TResult>() => () =>
    {
        Canceled.ThrowIfCancellationRequested();
        return Task.FromResult<TResult>(default!);
    };

    private Func<Task<TResult>> CanceledStep<TResult>() => () => Task.FromCanceled<TResult>(Canceled);
}
