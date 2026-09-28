namespace DotMaybe.PropertyTests;

/// <summary>
/// The four kinds of awaitable maybe the extensions must handle alike. FsCheck generates the enum.
/// </summary>
public enum SourceKind
{
    CompletedTask,
    PendingTask,
    CompletedValueTask,
    PendingValueTask,
}

/// <summary>
/// Runs the same pipeline on a <c>Task&lt;Maybe&lt;T&gt;&gt;</c> or a <c>ValueTask&lt;Maybe&lt;T&gt;&gt;</c> source.
/// Extension methods bind at compile time, so each test passes the pipeline twice: once typed for each source.
/// </summary>
internal static class Sources
{
    public static ValueTask<TResult> Apply<T, TResult>(
        Maybe<T> maybe,
        SourceKind kind,
        Func<Task<Maybe<T>>, ValueTask<TResult>> onTask,
        Func<ValueTask<Maybe<T>>, ValueTask<TResult>> onValueTask)
        where T : notnull =>
        kind switch
        {
            SourceKind.CompletedTask => onTask(Task.FromResult(maybe)),
            SourceKind.PendingTask => onTask(Deferred.Lift(maybe, yields: true)),
            SourceKind.CompletedValueTask => onValueTask(ValueTask.FromResult(maybe)),
            SourceKind.PendingValueTask => onValueTask(new ValueTask<Maybe<T>>(Deferred.Lift(maybe, yields: true))),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown source kind."),
        };
}
