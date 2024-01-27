namespace Compiler.Common;

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
}