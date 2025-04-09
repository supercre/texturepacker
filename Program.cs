using System;
using System.Threading.Tasks;
using TexturePacker.Models;
using TexturePacker.Packing;

namespace TexturePacker;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowHelp();
            return;
        }

        var options = new TexturePackerOptions();
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--help":
                case "-h":
                    ShowHelp();
                    return;
                case "--input":
                case "-i":
                    if (i + 1 < args.Length)
                        options.InputDirectory = args[++i];
                    break;
                case "--output":
                case "-o":
                    if (i + 1 < args.Length)
                        options.OutputFile = args[++i];
                    break;
                case "--max-width":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int maxWidth))
                        options.MaxWidth = maxWidth;
                    break;
                case "--max-height":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int maxHeight))
                        options.MaxHeight = maxHeight;
                    break;
                case "--padding":
                    if (i + 1 < args.Length && int.TryParse(args[++i], out int padding))
                        options.Padding = padding;
                    break;
                case "--generate-plist":
                    options.GeneratePlist = true;
                    break;
                case "--generate-atlas":
                    options.GenerateAtlas = true;
                    break;
                case "--spine-version":
                    if (i + 1 < args.Length)
                        options.SpineVersion = args[++i];
                    break;
                case "--enable-rotation":
                    options.EnableRotation = true;
                    break;
                case "--find-optimal-size":
                    options.FindOptimalSize = true;
                    break;
            }
        }

        if (string.IsNullOrEmpty(options.InputDirectory) || string.IsNullOrEmpty(options.OutputFile))
        {
            Console.WriteLine("Error: Input directory and output file are required.");
            ShowHelp();
            return;
        }

        var result = await Packing.TexturePacker.PackTexturesAsync(options);

        if (result.Success)
        {
            Console.WriteLine($"Texture atlas created successfully.");
            Console.WriteLine($"Texture size: {result.TextureSize.Width}x{result.TextureSize.Height}");
            Console.WriteLine($"Number of sprites: {result.SpritePositions.Count}");
        }
        else
        {
            Console.WriteLine($"Failed to create texture atlas: {result.ErrorMessage}");
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("TexturePacker - A tool for packing textures into atlases");
        Console.WriteLine();
        Console.WriteLine("Usage: TexturePacker [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --help, -h              Show this help message");
        Console.WriteLine("  --input, -i <dir>       Input directory containing images");
        Console.WriteLine("  --output, -o <file>     Output file path for the texture atlas");
        Console.WriteLine("  --max-width <pixels>    Maximum width of the texture atlas (default: 2048)");
        Console.WriteLine("  --max-height <pixels>   Maximum height of the texture atlas (default: 2048)");
        Console.WriteLine("  --padding <pixels>      Padding between sprites (default: 2)");
        Console.WriteLine("  --generate-plist        Generate Cocos2d-x .plist file");
        Console.WriteLine("  --generate-atlas        Generate Spine .atlas file");
        Console.WriteLine("  --spine-version <ver>   Spine version (2.x or 3.x)");
        Console.WriteLine("  --enable-rotation       Enable rotation for better packing");
        Console.WriteLine("  --find-optimal-size     Find the optimal texture size");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  TexturePacker --input sprites --output atlas.png --generate-plist --enable-rotation");
    }
} 