using System.Collections;

namespace LanguageParser.Common;

public static class Extensions
{
    public static ScopeNode? FindDescendant(this ScopeNode root, Func<ScopeNode, bool> predicate)
    {
        if (predicate(root))
            return root;

        foreach (var child in root.Children)
        {
            if (predicate(child))
                return child;

            var node = child.FindDescendant(predicate);

            if (node is not null)
                return node;
        }

        return null;
    }
    
    public static TAcc Aggregate<TAcc, T>(this IEnumerable<T>? source, Func<T, TAcc>? seedSelector,
        Func<TAcc, T, TAcc>? selector)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));

        if (seedSelector is null) throw new ArgumentNullException(nameof(seedSelector));

        if (selector is null) throw new ArgumentException(nameof(selector));

        var (head, tail) = source.GetHeadAndTail();
        var seed = seedSelector.Invoke(head);
        var result = tail.Aggregate(seed, selector);
        return result;
    }

    public static (T Head, IEnumerable<T> Tail) GetHeadAndTail<T>(this IEnumerable<T>? seq)
    {
        if (seq is null)
            throw new ArgumentNullException(nameof(seq));

        var it = seq.GetEnumerator();
        if (!it.MoveNext())
            throw new ArgumentNullException(nameof(seq), "Sequence is empty");

        var head = it.Current;
        var tail = new EnumerableFromEnumerator<T>(it);
        return (head, tail);
    }

    private class EnumerableFromEnumerator<T> : IEnumerable<T>
    {
        private readonly IEnumerator<T> _it;

        public EnumerableFromEnumerator(IEnumerator<T> it)
        {
            _it = it;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _it;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _it;
        }
    }
}