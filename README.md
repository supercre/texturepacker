# TexturePacker

TexturePacker is a .NET tool that easily creates image atlases (sprite sheets) for use in games and graphic applications. It efficiently packs multiple individual images into a single texture atlas, reducing memory usage and improving rendering performance.

## Key Features

- **Efficient Texture Packing**: Optimized packing of multiple images into a single texture atlas
- **Image Rotation Support**: Automatically rotates tall images to improve space utilization
- **Optimal Size Calculation**: Creates texture atlases that exactly match the required area size
- **Various Output Formats**: 
  - Image (PNG) 
  - Cocos2d-x compatible plist files
  - Spine compatible atlas files
- **Customization Options**: Support for various settings like padding, maximum width/height, etc.

## Requirements

- .NET 6.0 or higher
- SixLabors.ImageSharp library

## Installation

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/TexturePacker.git
   ```

2. Build the project:
   ```
   cd TexturePacker
   dotnet build
   ```

## Usage

### Command Line Options

```
dotnet run -- [options]
```

### Main Options

| Option             | Description                                    | Default    |
|--------------------|------------------------------------------------|------------|
| --input            | Directory path with input images               | (Required) |
| --output           | Output texture atlas file path                 | (Required) |
| --enable-rotation  | Enable image rotation optimization             | false      |
| --max-width        | Maximum texture width                          | 4096       |
| --max-height       | Maximum texture height                         | 4096       |
| --padding          | Padding between images                         | 2          |
| --generate-plist   | Generate Cocos2d-x plist file                  | false      |
| --generate-atlas   | Generate Spine atlas file                      | false      |

### Usage Examples

Basic usage:
```
dotnet run -- --input images --output Output/atlas.png
```

Rotation optimization and additional file formats:
```
dotnet run -- --input images --output Output/atlas.png --enable-rotation --generate-plist --generate-atlas
```

Custom padding and maximum size:
```
dotnet run -- --input images --output Output/atlas.png --padding 5 --max-width 2048 --max-height 2048
```

## Programmatic Usage

```csharp
var options = new TexturePackerOptions
{
    InputDirectory = "path/to/image/folder",
    OutputFile = "path/to/output.png",
    EnableRotation = true,
    GeneratePlist = true,
    GenerateAtlas = true
};

var result = await TexturePacker.PackTexturesAsync(options);

if (result.Success)
{
    Console.WriteLine($"Texture atlas created successfully.");
    Console.WriteLine($"Texture size: {result.TextureSize.Width}x{result.TextureSize.Height}");
    Console.WriteLine($"Number of sprites: {result.SpritePositions.Count}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

## License

This project is provided under the MIT License. For more details, see the [LICENSE.txt](LICENSE.txt) file. 