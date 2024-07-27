using System;

namespace DotMaybe;

/// <summary>
/// Provides static methods for working with Maybe types.
/// </summary>
public static class Maybe
{
    /// <summary>
    /// Combines two Maybe instances using a mapping function.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the first Maybe.</typeparam>
    /// <typeparam name="T2">The type of the value in the second Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="maybe1">The first Maybe instance.</param>
    /// <param name="maybe2">The second Maybe instance.</param>
    /// <param name="map">A function that combines the values from both Maybes if they exist.</param>
    /// <returns>
    /// A new Maybe instance containing the result of applying the map function to the values of both input Maybes
    /// if both contain values; otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method is useful for combining two independent Maybe values into a single result.
    /// If either of the input Maybes is empty, the result will be an empty Maybe.
    /// </remarks>
    public static Maybe<TResult> Map2<T1, T2, TResult>(
        Maybe<T1> maybe1,
        Maybe<T2> maybe2,
        Func<T1, T2, TResult> map)
    {
        return
            from v1 in maybe1
            from v2 in maybe2
            select map(v1, v2);
    }

    /// <summary>
    /// Combines three Maybe instances using a mapping function.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the first Maybe.</typeparam>
    /// <typeparam name="T2">The type of the value in the second Maybe.</typeparam>
    /// <typeparam name="T3">The type of the value in the third Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="maybe1">The first Maybe instance.</param>
    /// <param name="maybe2">The second Maybe instance.</param>
    /// <param name="maybe3">The third Maybe instance.</param>
    /// <param name="map">A function that combines the values from all three Maybes if they exist.</param>
    /// <returns>
    /// A new Maybe instance containing the result of applying the map function to the values of all three input Maybes
    /// if all contain values; otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method is useful for combining three independent Maybe values into a single result.
    /// If any of the input Maybes is empty, the result will be an empty Maybe.
    /// </remarks>
    public static Maybe<TResult> Map3<T1, T2, T3, TResult>(
        Maybe<T1> maybe1,
        Maybe<T2> maybe2,
        Maybe<T3> maybe3,
        Func<T1, T2, T3, TResult> map)
    {
        return
            from v1 in maybe1
            from v2 in maybe2
            from v3 in maybe3
            select map(v1, v2, v3);
    }
}
