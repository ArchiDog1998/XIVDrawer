﻿using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using XIVDrawer.Element2D;

namespace XIVDrawer;

internal class OverlayWindow : Window
{
    public OverlayWindow() : base(XIVDrawerMain._name, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav, true)
    {
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
        if (!XIVDrawerMain.Enable || Service.ClientState == null || Service.ClientState.LocalPlayer == null) return;

        ImGui.GetStyle().AntiAliasedFill = false;

        try
        {
            if (!XIVDrawerMain.UseTaskToAccelerate)
            {
                XIVDrawerMain._drawingElements2D = XIVDrawerMain.To2DAsync().Result;
            }

            foreach (var item in XIVDrawerMain._drawingElements2D.OrderBy(drawing =>
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
            Service.Log.Warning(ex, $"{XIVDrawerMain._name} failed to draw on Screen.");
        }

        try
        {
            XIVDrawerMain.AdditionDraw();
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, $"{XIVDrawerMain._name} failed to draw the additional.");
        }
    }

    public override void PostDraw()
    {
        ImGui.PopStyleVar();
        base.PostDraw();
    }
}
