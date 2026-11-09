namespace PROG7312_SmartX.Algorithms;

/// <summary>Append-only balanced index. Equal keys share a bucket.</summary>
public sealed class AvlTree<TKey, TValue> where TKey : IComparable<TKey>
{
    private sealed class Node(TKey key, TValue value)
    {
        public TKey Key = key;
        public List<TValue> Values = [value];
        public Node? Left, Right;
        public int Height = 1;
    }

    private Node? root;
    public int Count { get; private set; }
    public int Height => GetHeight(root);

    public void Add(TKey key, TValue value)
    {
        root = Insert(root, key, value);
        Count++;
    }

    // Visits only intersecting branches: O(log n + k) for k returned values.
    public List<TValue> Range(TKey from, TKey to) => Range(from, to, out _);

    public List<TValue> Range(TKey from, TKey to, out int visitedNodes)
    {
        if (from.CompareTo(to) > 0) throw new ArgumentException("Start must precede end.");
        var result = new List<TValue>();
        int visited = 0;
        void Visit(Node? node)
        {
            if (node is null) return;
            visited++;
            int lower = from.CompareTo(node.Key), upper = to.CompareTo(node.Key);
            if (lower < 0) Visit(node.Left);
            if (lower <= 0 && upper >= 0) result.AddRange(node.Values);
            if (upper > 0) Visit(node.Right);
        }
        Visit(root);
        visitedNodes = visited;
        return result;
    }

    public List<TValue> All()
    {
        var result = new List<TValue>();
        void Visit(Node? node)
        {
            if (node is null) return;
            Visit(node.Left);
            result.AddRange(node.Values);
            Visit(node.Right);
        }
        Visit(root);
        return result;
    }

    private static int GetHeight(Node? node) => node?.Height ?? 0;
    private static void Update(Node node) =>
        node.Height = 1 + Math.Max(GetHeight(node.Left), GetHeight(node.Right));

    private static Node RotateRight(Node node)
    {
        var pivot = node.Left!;
        node.Left = pivot.Right;
        pivot.Right = node;
        Update(node);
        Update(pivot);
        return pivot;
    }

    private static Node RotateLeft(Node node)
    {
        var pivot = node.Right!;
        node.Right = pivot.Left;
        pivot.Left = node;
        Update(node);
        Update(pivot);
        return pivot;
    }

    private static Node Insert(Node? node, TKey key, TValue value)
    {
        if (node is null) return new Node(key, value);
        int comparison = key.CompareTo(node.Key);
        if (comparison == 0) { node.Values.Add(value); return node; }
        if (comparison < 0) node.Left = Insert(node.Left, key, value);
        else node.Right = Insert(node.Right, key, value);
        Update(node);
        int balance = GetHeight(node.Left) - GetHeight(node.Right);
        if (balance > 1)
        {
            if (key.CompareTo(node.Left!.Key) > 0) node.Left = RotateLeft(node.Left);
            return RotateRight(node);
        }
        if (balance < -1)
        {
            if (key.CompareTo(node.Right!.Key) < 0) node.Right = RotateRight(node.Right);
            return RotateLeft(node);
        }
        return node;
    }
}
