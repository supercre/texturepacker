using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using TexturePacker.Models;
using TexturePacker.Generator;
using System.Text;
using Point = SixLabors.ImageSharp.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Color = SixLabors.ImageSharp.Color;

namespace TexturePacker.Packing;

public class TexturePacker
{
    public static async Task<TexturePackerResult> PackTexturesAsync(TexturePackerOptions options)
    {
        try
        {
            // Check input directory
            if (!Directory.Exists(options.InputDirectory))
            {
                return new TexturePackerResult { ErrorMessage = "Input directory does not exist." };
            }

            // Get list of image files
            var imageFiles = Directory.GetFiles(options.InputDirectory, "*.*")
                .Where(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                              file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                              file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!imageFiles.Any())
            {
                return new TexturePackerResult { ErrorMessage = "No image files found in the input directory." };
            }

            // Load and sort images
            var images = new List<(string name, Image<Rgba32> image)>();
            foreach (var file in imageFiles)
            {
                var image = await Image.LoadAsync<Rgba32>(file);
                images.Add((Path.GetFileNameWithoutExtension(file), image));
            }

            // Sort by height
            images = images.OrderByDescending(x => x.image.Height).ToList();

            // Calculate total area based on rotation
            int totalArea = 0;
            foreach (var (_, image) in images)
            {
                if (options.EnableRotation && image.Width < image.Height)
                {
                    // When rotated, swap width and height for calculation
                    totalArea += (image.Height + options.Padding) * (image.Width + options.Padding);
                }
                else
                {
                    totalArea += (image.Width + options.Padding) * (image.Height + options.Padding);
                }
            }
            
            // Calculate estimated size based on area
            int estimatedSize = (int)Math.Ceiling(Math.Sqrt(totalArea));
            Console.WriteLine($"Estimated texture size based on area: {estimatedSize}x{estimatedSize}");

            // Initial texture size estimate
            int initialWidth = Math.Min(estimatedSize, options.MaxWidth);
            int initialHeight = Math.Min(estimatedSize, options.MaxHeight);

            // Image packing
            var positions = new Dictionary<string, (Point position, bool rotated)>();
            var rotatedImages = new Dictionary<string, Image<Rgba32>>();

            int maxX = 0;
            int maxY = 0;
            int posX = 0;
            int posY = 0;
            int rowHeight = 0;

            foreach (var (name, image) in images)
            {
                // Determine if rotation is needed
                bool rotated = false;
                
                if (options.EnableRotation && image.Width < image.Height)
                {
                    // Rotate tall images
                    rotated = true;
                }
                
                int imgWidth = rotated ? image.Height : image.Width;
                int imgHeight = rotated ? image.Width : image.Height;
                
                // Move to next row if needed
                if (posX + imgWidth > initialWidth)
                {
                    posX = 0;
                    posY += rowHeight + options.Padding;
                    rowHeight = 0;
                }
                
                Point position = new Point(posX, posY);
                positions[name] = (position, rotated);
                
                if (rotated)
                {
                    var rotatedImage = image.Clone();
                    rotatedImage.Mutate(x => x.Rotate(90));
                    rotatedImages[name] = rotatedImage;
                }
                
                // Calculate next image position
                posX += imgWidth + options.Padding;
                rowHeight = Math.Max(rowHeight, imgHeight);
                
                // Track maximum X and Y positions
                maxX = Math.Max(maxX, posX - options.Padding); // Exclude last padding
                maxY = Math.Max(maxY, posY + imgHeight);
            }

            // Adjust texture size to exactly match used area
            int finalWidth = maxX;
            int finalHeight = maxY;
            
            Console.WriteLine($"Final texture size (exact area): {finalWidth}x{finalHeight}");

            // Create result image
            using var atlas = new Image<Rgba32>(finalWidth, finalHeight);
            atlas.Mutate(x => x.BackgroundColor(Color.Transparent));

            foreach (var (name, image) in images)
            {
                var (position, rotated) = positions[name];
                
                if (rotated)
                {
                    atlas.Mutate(x => x.DrawImage(rotatedImages[name], position, 1f));
                }
                else
                {
                    atlas.Mutate(x => x.DrawImage(image, position, 1f));
                }
            }

            // Create output directory
            Directory.CreateDirectory(Path.GetDirectoryName(options.OutputFile) ?? string.Empty);

            // Save atlas
            await atlas.SaveAsPngAsync(options.OutputFile);

            // Generate additional output files if requested
            var spriteRectangles = new Dictionary<string, (Rectangle rectangle, bool rotated)>();
            foreach (var (name, image) in images)
            {
                var (position, rotated) = positions[name];
                var width_height = rotated ? (image.Height, image.Width) : (image.Width, image.Height);
                
                spriteRectangles[name] = (
                    new Rectangle(position.X, position.Y, width_height.Item1, width_height.Item2),
                    rotated
                );
            }

            if (options.GeneratePlist)
            {
                await GeneratePlistFile(options.OutputFile, spriteRectangles, options.Padding);
            }

            if (options.GenerateAtlas)
            {
                await GenerateAtlasFile(options.OutputFile, spriteRectangles, options.Padding);
            }

            return new TexturePackerResult
            {
                Success = true,
                TextureSize = new Size(finalWidth, finalHeight),
                SpritePositions = positions
            };
        }
        catch (Exception ex)
        {
            return new TexturePackerResult { ErrorMessage = $"Error: {ex.Message}" };
        }
    }

    private static async Task GeneratePlistFile(string texturePath, Dictionary<string, (Rectangle Rectangle, bool Rotated)> spritePositions, int padding)
    {
        var plistPath = Path.ChangeExtension(texturePath, ".plist");
        var textureFileName = Path.GetFileName(texturePath);

        var plistContent = new StringBuilder();
        plistContent.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        plistContent.AppendLine("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
        plistContent.AppendLine("<plist version=\"1.0\">");
        plistContent.AppendLine("<dict>");
        plistContent.AppendLine($"\t<key>metadata</key>");
        plistContent.AppendLine("\t<dict>");
        plistContent.AppendLine("\t\t<key>format</key>");
        plistContent.AppendLine("\t\t<integer>2</integer>");
        plistContent.AppendLine("\t\t<key>textureFileName</key>");
        plistContent.AppendLine($"\t\t<string>{textureFileName}</string>");
        plistContent.AppendLine("\t\t<key>size</key>");
        plistContent.AppendLine($"\t\t<string>{{{spritePositions.Values.Max(p => p.Rectangle.Right)}, {spritePositions.Values.Max(p => p.Rectangle.Bottom)}}}</string>");
        plistContent.AppendLine("\t\t<key>textureFileName</key>");
        plistContent.AppendLine($"\t\t<string>{textureFileName}</string>");
        plistContent.AppendLine("\t</dict>");
        plistContent.AppendLine("\t<key>frames</key>");
        plistContent.AppendLine("\t<dict>");

        foreach (var kvp in spritePositions)
        {
            var fileName = kvp.Key;
            var (rect, rotated) = kvp.Value;

            plistContent.AppendLine($"\t\t<key>{fileName}</key>");
            plistContent.AppendLine("\t\t<dict>");
            plistContent.AppendLine("\t\t\t<key>frame</key>");
            plistContent.AppendLine($"\t\t\t<string>{{{{{rect.X}, {rect.Y}}}, {{{rect.Width}, {rect.Height}}}}}</string>");
            plistContent.AppendLine("\t\t\t<key>offset</key>");
            plistContent.AppendLine($"\t\t\t<string>{{0, 0}}</string>");
            plistContent.AppendLine("\t\t\t<key>rotated</key>");
            plistContent.AppendLine($"\t\t\t<{rotated.ToString().ToLower()}/>");
            plistContent.AppendLine("\t\t\t<key>sourceColorRect</key>");
            plistContent.AppendLine($"\t\t\t<string>{{{{0, 0}}, {{{rect.Width}, {rect.Height}}}}}</string>");
            plistContent.AppendLine("\t\t\t<key>sourceSize</key>");
            plistContent.AppendLine($"\t\t\t<string>{{{rect.Width}, {rect.Height}}}</string>");
            plistContent.AppendLine("\t\t</dict>");
        }

        plistContent.AppendLine("\t</dict>");
        plistContent.AppendLine("</dict>");
        plistContent.AppendLine("</plist>");

        await File.WriteAllTextAsync(plistPath, plistContent.ToString());
    }

    private static async Task GenerateAtlasFile(string texturePath, Dictionary<string, (Rectangle Rectangle, bool Rotated)> spritePositions, int padding)
    {
        var atlasPath = Path.ChangeExtension(texturePath, ".atlas");
        var textureFileName = Path.GetFileName(texturePath);

        var atlasContent = new StringBuilder();
        atlasContent.AppendLine(textureFileName);
        atlasContent.AppendLine($"size: {spritePositions.Values.Max(p => p.Rectangle.Right)}, {spritePositions.Values.Max(p => p.Rectangle.Bottom)}");
        atlasContent.AppendLine("format: RGBA8888");
        atlasContent.AppendLine("filter: Linear,Linear");
        atlasContent.AppendLine("repeat: none");
        atlasContent.AppendLine();

        foreach (var kvp in spritePositions)
        {
            var fileName = kvp.Key;
            var (rect, rotated) = kvp.Value;

            atlasContent.AppendLine(fileName);
            atlasContent.AppendLine($"  rotate: {rotated.ToString().ToLower()}");
            atlasContent.AppendLine($"  xy: {rect.X}, {rect.Y}");
            atlasContent.AppendLine($"  size: {rect.Width}, {rect.Height}");
            atlasContent.AppendLine($"  orig: {rect.Width}, {rect.Height}");
            atlasContent.AppendLine($"  offset: 0, 0");
            atlasContent.AppendLine($"  index: -1");
            atlasContent.AppendLine();
        }

        await File.WriteAllTextAsync(atlasPath, atlasContent.ToString());
    }
} 