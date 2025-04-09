using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace TexturePacker.Models;

public class TexturePackerResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Size TextureSize { get; set; }
    public Dictionary<string, (Point position, bool rotated)> SpritePositions { get; set; } = new();
}

public class AtlasInfo
{
    public string OutputFile { get; set; } = string.Empty;
    public Dictionary<string, (SixLabors.ImageSharp.Rectangle Rectangle, bool Rotated)> SpritePositions { get; set; } = new();
    public (int Width, int Height) TextureSize { get; set; }
} 