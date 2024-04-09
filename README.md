# XIVDrawer

> [!IMPORTANT]
>
> Due to the `XIVPainter` is already taken in Nuget, I have to change its name to `XIVDrawer`!

XIVDrawer is a drawing library in Imgui designed to work within Dalamud Plugins. It can draw polygons, images, text, and so on to your world.

## Getting Started

Add XIVPainter as a submodule to your project:

```shell
git submodule add https://github.com/ArchiDog1998/XIVDrawer
```

Add it to your plugin's CSProj file:

```xml
<ItemGroup>
	<ProjectReference Include="..\XIVDrawer\XIVDrawer\XIVDrawer.csproj" />
</ItemGroup>
```

Then, in the entry point of your plugin:

```c#
XIVDrawerMain.Init(pluginInterface, "%NAME%");
```

where pluginInterface is a **DalamudPluginInterface**.

 And there is a good example of it in [Rotation Solver](https://github.com/ArchiDog1998/RotationSolver/blob/main/RotationSolver/UI/PainterManager.cs).

Don't forget to **dispose** it!

## Usage

All things need to be disposed to close it.

### VFX stuff

<iframe width="560" height="315" src="https://www.youtube.com/embed/wE8VVTmQyxQ?si=Lnjw9O4yCyY-I9kO" title="YouTube video player" frameborder="0"</iframe>

Use this code to show the Vfx stuff.

```c#
_ = XIVDrawerMain.ShowOff();
```

There are two things called `ActorVfx` and `StaticVfx` for you to use in your own project.

### Hotbar highlighting

```c#
new DrawingHighlightHotbar(new(0f, 1f, 0.8f, 1f), 7411);
```

![highlight](assets/1687487480217.png)

### Drawing stuff

``` c#
new Drawing3DCircularSectorO(Player.Object, 5, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.5f, 0.4f, 0.15f)), 5);
```

### Animation stuff

``` c#
var deadTime = DateTime.Now.AddSeconds(10);
var r = new Random();
var col = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.5f, 0.2f, 0.15f));
var colIn = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.5f, 0.2f, 0.5f));
    new Drawing3DAnnulus(Player.Object.Position + new Vector3((float)r.NextDouble() * 3, 0, (float)r.NextDouble() * 3), 3, 5, col, 2)
    {
        DeadTime = deadTime,
        InsideColor = colIn,
    };

    new Drawing3DCircularSector(Player.Object.Position + new Vector3((float)r.NextDouble() * 3, 0, (float)r.NextDouble() * 3), 3, col, 2)
    {
        DeadTime = deadTime,
        InsideColor = colIn,
    };
```

`DeadTime` will make an animation about disappear.

`PolylineType` will show the moving suggestion for you.
