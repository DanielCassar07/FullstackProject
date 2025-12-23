using FullstackProject.Models;
using FullstackProject.Security;
using FullstackProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace FullstackProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MapController : ControllerBase
    {
        private readonly MapStore _store;

        public MapController(MapStore store)
        {
            _store = store;
        }

        [HttpPost("[action]")]
        [RequireApiKey("ReadWrite")]
        public IActionResult SetMap([FromBody] GraphDto graph)
        {
            try
            {
                if (graph is null || graph.Edges is null || graph.Edges.Count == 0)
                    return BadRequest("Missing map data.");

                _store.SetGraph(graph);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpGet("[action]")]
        [RequireApiKey("Read")]
        public IActionResult GetMap()
        {
            try
            {
                if (!_store.HasMap)
                    return BadRequest("Map has not been set.");

                var graph = _store.GetGraph();
                return Ok(graph);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpGet("[action]")]
        [RequireApiKey("Read")]
        public IActionResult ShortestRoute([FromQuery] string from, [FromQuery] string to)
        {
            try
            {
                if (!_store.HasMap)
                    return BadRequest("Map has not been set.");

                if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                    return BadRequest("Missing parameters.");

                from = from.Trim();
                to = to.Trim();

                if (!_store.NodeExists(from) || !_store.NodeExists(to))
                    return BadRequest("Unknown node name.");

                var (path, _) = Dijkstra(from, to);
                var pathString = string.Join("", path);
                return Ok(pathString);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpGet("[action]")]
        [RequireApiKey("Read")]
        public IActionResult ShortestDistance([FromQuery] string from, [FromQuery] string to)
        {
            try
            {
                if (!_store.HasMap)
                    return BadRequest("Map has not been set.");

                if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                    return BadRequest("Missing parameters.");

                from = from.Trim();
                to = to.Trim();

                if (!_store.NodeExists(from) || !_store.NodeExists(to))
                    return BadRequest("Unknown node name.");

                var (_, dist) = Dijkstra(from, to);
                return Ok(dist);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        // --- Dijkstra ---
        private (List<string> path, int distance) Dijkstra(string start, string goal)
        {
            var adj = _store.GetAdjacency();

            var dist = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var prev = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (var node in adj.Keys)
            {
                dist[node] = int.MaxValue;
                prev[node] = null;
            }

            dist[start] = 0;

            var pq = new PriorityQueue<string, int>();
            pq.Enqueue(start, 0);

            while (pq.Count > 0)
            {
                var current = pq.Dequeue();

                if (string.Equals(current, goal, StringComparison.OrdinalIgnoreCase))
                    break;

                foreach (var (next, w) in adj[current])
                {
                    if (dist[current] == int.MaxValue) continue;

                    var alt = dist[current] + w;
                    if (alt < dist[next])
                    {
                        dist[next] = alt;
                        prev[next] = current;
                        pq.Enqueue(next, alt);
                    }
                }
            }

            if (dist[goal] == int.MaxValue)
            {
                // If disconnected graph, treat as invalid request (no route)
                throw new ArgumentException("No route exists between these nodes.");
            }

            var path = new List<string>();
            string? step = goal;
            while (step is not null)
            {
                path.Add(step);
                step = prev[step];
            }
            path.Reverse();

            return (path, dist[goal]);
        }
    }
}
