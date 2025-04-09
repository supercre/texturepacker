using SixLabors.ImageSharp;
using System.Xml.Linq;

namespace TexturePacker.Generator;

public static class OutputGenerator
{
    public static string GenerateCocos2dPlist(string texturePath, Dictionary<string, (Rectangle Rectangle, bool Rotated)> spritePositions)
    {
        var doc = new XDocument(
            new XElement("plist",
                new XAttribute("version", "1.0"),
                new XElement("dict",
                    new XElement("key", "frames"),
                    new XElement("dict",
                        spritePositions.Select(kvp =>
                            new XElement("dict",
                                new XElement("key", kvp.Key),
                                new XElement("dict",
                                    new XElement("key", "frame"),
                                    new XElement("string", $"{{{{{kvp.Value.Rectangle.X},{kvp.Value.Rectangle.Y},{kvp.Value.Rectangle.Width},{kvp.Value.Rectangle.Height}}}}}"),
                                    new XElement("key", "offset"),
                                    new XElement("string", "{0,0}"),
                                    new XElement("key", "rotated"),
                                    new XElement(kvp.Value.Rotated.ToString().ToLower()),
                                    new XElement("key", "sourceColorRect"),
                                    new XElement("string", $"{{{{{kvp.Value.Rectangle.X},{kvp.Value.Rectangle.Y},{kvp.Value.Rectangle.Width},{kvp.Value.Rectangle.Height}}}}}"),
                                    new XElement("key", "sourceSize"),
                                    new XElement("string", $"{{{kvp.Value.Rectangle.Width},{kvp.Value.Rectangle.Height}}}")
                                )
                            )
                        )
                    ),
                    new XElement("key", "metadata"),
                    new XElement("dict",
                        new XElement("key", "format"),
                        new XElement("integer", "2"),
                        new XElement("key", "textureFileName"),
                        new XElement("string", Path.GetFileName(texturePath)),
                        new XElement("key", "size"),
                        new XElement("string", $"{{{spritePositions.Values.Max(r => r.Rectangle.Right)},{spritePositions.Values.Max(r => r.Rectangle.Bottom)}}}")
                    )
                )
            )
        );

        return doc.ToString();
    }

    public static string GenerateSpineAtlas(string texturePath, Dictionary<string, (Rectangle Rectangle, bool Rotated)> spritePositions, string version)
    {
        var textureFileName = Path.GetFileName(texturePath);
        var sb = new System.Text.StringBuilder();
        
        // Add texture information
        sb.AppendLine(textureFileName);
        sb.AppendLine($"size: {spritePositions.Values.Max(r => r.Rectangle.Right)}, {spritePositions.Values.Max(r => r.Rectangle.Bottom)}");
        sb.AppendLine("format: RGBA8888");
        sb.AppendLine("filter: Linear,Linear");
        sb.AppendLine("repeat: none");
        sb.AppendLine();
        
        // Add region information for each sprite
        foreach (var kvp in spritePositions)
        {
            var spriteName = Path.GetFileNameWithoutExtension(kvp.Key);
            var rect = kvp.Value.Rectangle;
            var rotated = kvp.Value.Rotated;
            
            sb.AppendLine(spriteName);
            
            if (version == "3.x")
            {
                // Spine 3.x format
                sb.AppendLine($"  rotate: {rotated.ToString().ToLower()}");
                sb.AppendLine($"  xy: {rect.X}, {rect.Y}");
                sb.AppendLine($"  size: {rect.Width}, {rect.Height}");
                sb.AppendLine($"  orig: {rect.Width}, {rect.Height}");
                sb.AppendLine($"  offset: 0, 0");
                sb.AppendLine($"  index: -1");
            }
            else
            {
                // Spine 2.x format
                sb.AppendLine($"  rotate: {rotated.ToString().ToLower()}");
                sb.AppendLine($"  xy: {rect.X}, {rect.Y}");
                sb.AppendLine($"  size: {rect.Width}, {rect.Height}");
                sb.AppendLine($"  orig: {rect.Width}, {rect.Height}");
                sb.AppendLine($"  offset: 0, 0");
                sb.AppendLine($"  index: -1");
            }
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
} 