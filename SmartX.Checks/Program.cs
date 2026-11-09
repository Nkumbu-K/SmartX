using PROG7312_SmartX.Algorithms;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

foreach (int[] keys in new[] { new[] { 3, 2, 1 }, new[] { 1, 2, 3 }, new[] { 3, 1, 2 }, new[] { 1, 3, 2 } })
{
    var tree = new AvlTree<int, int>();
    foreach (int key in keys) tree.Add(key, key);
    Check(tree.Height == 2, "Each AVL rotation must balance three nodes.");
    Check(tree.All().SequenceEqual(new[] { 1, 2, 3 }), "AVL traversal must sort keys.");
}
var large = new AvlTree<int, int>();
for (int i = 0; i < 100_000; i++) large.Add(i, i);
Check(large.Height < 25, "Sorted input must not degenerate into a chain.");
large.Add(50_000, -1);
Check(large.Range(50_000, 50_000, out int visited).SequenceEqual(new[] { 50_000, -1 }), "Equal timestamps must retain both packets.");
Check(visited < 25, "Narrow queries must prune unrelated branches.");
Check(large.Range(-5, -1).Count == 0, "Out-of-range search must be empty.");
Check(large.Range(10, 12).SequenceEqual(new[] { 10, 11, 12 }), "Bounds must be inclusive.");
try { large.Range(2, 1); throw new Exception("Reversed range accepted."); }
catch (ArgumentException) { }
Console.WriteLine($"AVL checks passed: 100,001 readings, height {large.Height}, exact query visited {visited} nodes.");
