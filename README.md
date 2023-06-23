# XIVPainter

XIVPainter is a drawing library in Imgui designed to work within Dalamud Plugins. It can draw polygons, images, text, and so on to your world.

## Getting Started

Add XIVPainter as a submodule to your project:

```shell
git submodule add https://github.com/ArchiDog1998/XIVPainter
```

Add it to your plugin's CSProj file:

```xml
<ItemGroup>
	<ProjectReference Include="..\XIVPainter\XIVPainter\XIVPainter.csproj" />
</ItemGroup>
```

Then, in the entry point of your plugin:

```c#
var painter = XIVPainter.XIVPainter.Create(pluginInterface, "%NAME%");
```

where pluginInterface is a **DalamudPluginInterface**.

 And there is a good example of it in [Rotation Solver](https://github.com/ArchiDog1998/RotationSolver/blob/main/RotationSolver/UI/PainterManager.cs).

Don't forget to **dispose** it!

## Usage

### Hotbar highlighting

```c#
painter.AddDrawings(new DrawingHighlightHotbar(new(0f, 1f, 0.8f, 1f), 7411));
```

![highlight](assets/1687487480217.png)

### Drawing stuff

``` c#
painter.AddDrawings(new Drawing3DCircularSectorO(Player.Object, 5, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.5f, 0.4f, 0.15f)), 5));
```

![Moving](assets/Moving.gif)

### Animation stuff

``` c#
var deadTime = DateTime.Now.AddSeconds(10);
var r = new Random();
var col = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.5f, 0.2f, 0.15f));
var colIn = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.5f, 0.2f, 0.5f));
painter.AddDrawings(
    new Drawing3DAnnulus(Player.Object.Position + new Vector3((float)r.NextDouble() * 3, 0, (float)r.NextDouble() * 3), 3, 5, col, 2)
    {
        DeadTime = deadTime,
        InsideColor = colIn,
        PolylineType = XIVPainter.Enum.PolylineType.ShouldGoOut,
    },

    new Drawing3DCircularSector(Player.Object.Position + new Vector3((float)r.NextDouble() * 3, 0, (float)r.NextDouble() * 3), 3, col, 2)
    {
        DeadTime = deadTime,
        InsideColor = colIn,
        PolylineType = XIVPainter.Enum.PolylineType.ShouldGoOut,
    });
```

`DeadTime` will make an animation about disappear.

`PolylineType` will show the moving suggestion for you.

![Suggestion](assets/Suggestion.gif)
