namespace SharpTests.Models;

public class Options
{
    public static readonly Options Default = new();

    public int Iterations { get; set; } = 1;
    public Chunk[] Chunks { get; set; } = [];
}

public record Chunk(long Percentual, int VirtualUsers);