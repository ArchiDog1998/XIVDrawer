using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace XIVPainter;

internal class Service 
{
    [PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService] internal static IDataManager Data { get; private set; } = null!;

    [PluginService] internal static ICondition Condition { get; private set; } = null!;

    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    [PluginService] internal static IClientState ClientState { get; private set; } = null!;

    [PluginService] internal static IGameGui GameGui { get; private set; } = null!;

    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
}