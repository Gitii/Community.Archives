namespace Community.Archives.Core;

public static class DictionaryExtensions
{
    /// <summary>
    /// Checks if two <seealso cref="Dictionary{TKey,TValue}"/> are equal.
    /// All key & value pairs will be compared using <seealso cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of value.</typeparam>
    /// <param name="x">The left value.</param>
    /// <param name="y">The right value.</param>
    /// <returns><c>true</c> if both are equal, <c>false</c> is the are different.</returns>
    public static bool AreEqual<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue>? x,
        IReadOnlyDictionary<TKey, TValue>? y
    )
    {
        // early-exit checks
        if (y == null)
        {
            return x == null;
        }

        if (x == null)
        {
            return false;
        }

        if (object.ReferenceEquals(x, y))
        {
            return true;
        }

        if (x.Count != y.Count)
        {
            return false;
        }

        // check keys are the same
        foreach (TKey k in x.Keys)
        {
            if (!y.ContainsKey(k))
            {
                return false;
            }
        }

        var cmp = EqualityComparer<TValue>.Default;

        // check values are the same
        foreach (TKey k in x.Keys)
        {
            var leftValue = x[k];
            var rightValue = y[k];

            if (leftValue == null && rightValue == null)
            {
                continue;
            }
            else if (leftValue == null || rightValue == null)
            {
                return false;
            }

            if (!cmp.Equals(leftValue, rightValue))
            {
                return false;
            }
        }

        return true;
    }
}
