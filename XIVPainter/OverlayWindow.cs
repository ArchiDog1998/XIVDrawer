using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using XIVPainter.Element2D;

namespace XIVPainter;

internal class OverlayWindow : Window
{
    readonly XIVPainter _owner;
    public OverlayWindow(XIVPainter owner) : base(owner._name, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav, true)
    {
        _owner = owner;
        IsOpen = true;
    }

    public override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
        ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);

        base.PreDraw();
    }

    public override void Draw()
    {
        if (!_owner.Enable || Service.ClientState == null || Service.ClientState.LocalPlayer == null) return;

        ImGui.GetStyle().AntiAliasedFill = false;

        try
        {
            if (!_owner.UseTaskToAccelerate)
            {
                _owner._drawingElements2D = _owner.To2DAsync().Result;
            }

            foreach (var item in _owner._drawingElements2D.OrderBy(drawing =>
            {
                if (drawing is PolylineDrawing poly)
                {
                    return poly._thickness == 0 ? 0 : 1;
                }
                else
                {
                    return 2;
                }
            }))
            {
                item.Draw();
            }
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, $"{_owner._name} failed to draw on Screen.");
        }
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar();
        base.PostDraw();
    }
}
