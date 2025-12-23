using FullstackProject.Models;

namespace FullstackProject.Services;

public sealed class MapStore
{
    private readonly object _lock = new();
    private GraphDto? _graph;
    private Dictionary<string, List<(string to, int w)>> _adj = new();

    public bool HasMap
    {
        get { lock (_lock) return _graph is not null; }
    }

    public GraphDto? GetGraph()
    {
        lock (_lock) return _graph;
    }

    public Dictionary<string, List<(string to, int w)>> GetAdjacency()
    {
        lock (_lock)
        {
            // return a safe copy (small graph so OK)
            return _adj.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToList()
            );
        }
    }

    public void SetGraph(GraphDto graph)
    {
        // Normalize + build adjacency (undirected)
        var adj = new Dictionary<string, List<(string to, int w)>>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in graph.Edges)
        {
            var from = (e.From ?? "").Trim();
            var to = (e.To ?? "").Trim();

            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Edge has missing node name.");

            if (e.Distance <= 0)
                throw new ArgumentException("Distance must be > 0.");

            if (!adj.ContainsKey(from)) adj[from] = new();
            if (!adj.ContainsKey(to)) adj[to] = new();

            adj[from].Add((to, e.Distance));
            adj[to].Add((from, e.Distance));
        }

        lock (_lock)
        {
            _graph = graph;
            _adj = adj;
        }
    }

    public bool NodeExists(string node)
    {
        lock (_lock) return _adj.ContainsKey(node.Trim());
    }
}
