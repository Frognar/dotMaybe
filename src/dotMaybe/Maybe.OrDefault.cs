using System;

namespace DotMaybe;

public readonly partial record struct Maybe<T>
{
    /// <summary>
    /// Returns the value if the Maybe instance contains one; otherwise, returns the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value to return if the Maybe instance is empty.</param>
    /// <returns>
    /// The value contained in the Maybe instance if it exists; otherwise, the specified default value.
    /// </returns>
    public T OrDefault(T defaultValue)
    {
        return _maybe switch
        {
            SomeType some => some.Value,
            _ => defaultValue,
        };
    }
}
