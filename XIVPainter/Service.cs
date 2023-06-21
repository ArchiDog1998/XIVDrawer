using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace XIVPainter;

internal class Service 
{
    [PluginService] internal static DataManager Data { get; private set; }

    [PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; }

    [PluginService] internal static Framework Framework { get; private set; }

    [PluginService] internal static ClientState ClientState { get; private set; }

    [PluginService] internal static GameGui GameGui { get; private set; }
}