namespace FullstackProject.Models;

public sealed class GraphDto
{
    public List<EdgeDto> Edges { get; set; } = new();
}

public sealed class EdgeDto
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public int Distance { get; set; }
}
