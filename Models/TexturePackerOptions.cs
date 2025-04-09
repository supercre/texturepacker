using System;

namespace TexturePacker.Models;

public class TexturePackerOptions
{
    public string InputDirectory { get; set; } = string.Empty;
    public string OutputFile { get; set; } = string.Empty;
    public int MaxWidth { get; set; } = 2048;
    public int MaxHeight { get; set; } = 2048;
    public int Padding { get; set; } = 2;
    public bool GenerateCocos2dPlist { get; set; } = false;
    public bool GenerateSpineAtlas { get; set; } = false;
    public string? SpineVersion { get; set; } = null; // "2.x" or "3.x"
    public bool EnableRotation { get; set; }
    public bool FindOptimalSize { get; set; }
    public bool GeneratePlist { get; set; }
    public bool GenerateAtlas { get; set; }
} 