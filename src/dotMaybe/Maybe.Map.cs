﻿using System;

namespace DotMaybe;

public readonly partial record struct Maybe<T>
{
    /// <summary>
    /// Transforms the value inside the Maybe if it exists.
    /// </summary>
    /// <typeparam name="TResult">The type of the result after applying the transformation.</typeparam>
    /// <param name="map">The function to apply to the value if it exists.</param>
    /// <returns>
    /// A new Maybe instance containing the result of the transformation if the original Maybe had a value;
    /// otherwise, returns an empty Maybe of the new type.
    /// </returns>
    /// <remarks>
    /// This method is useful for transforming the value inside a Maybe without having to manually check
    /// if the value exists.
    /// </remarks>
    public Maybe<TResult> Map<TResult>(Func<T, TResult> map)
    {
        return Match(Maybe<TResult>.None, v => Maybe<TResult>.Some(map(v)));
    }
}