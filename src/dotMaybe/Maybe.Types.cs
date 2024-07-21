namespace DotMaybe;

public readonly partial record struct Maybe<T>
{
    /// <summary>
    /// Marker interface for a maybe type.
    /// </summary>
    private interface IMaybe;

    private readonly record struct SomeType(T Value) : IMaybe
    {
        public T Value { get; } = Value;
    }

    private readonly record struct NoneType : IMaybe;
}
