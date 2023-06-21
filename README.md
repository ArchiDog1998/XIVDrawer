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

