namespace XIVPainter.Element2D;

internal class PolylineDrawing : IDrawing2D
{
    Vector2[] _pts;
    uint _color;
    internal float _thickness;

    public PolylineDrawing(Vector2[] pts, uint color, float thickness)
    {
        _pts = pts;
        _color = color;
        _thickness = thickness;
    }

    public void Draw()
    {
        if (_pts == null || _pts.Length < 2) return;

        //int index = 0;
        foreach (var pt in _pts)
        {
            ImGui.GetWindowDrawList().PathLineTo(pt);
            //ImGui.GetWindowDrawList().AddText(pt, _color, (index++).ToString());
        }

        if (_thickness == 0)
        {
            var flags = ImGui.GetWindowDrawList().Flags;
            ImGui.GetWindowDrawList().Flags |= ImDrawListFlags.AntiAliasedFill;
            ImGui.GetWindowDrawList().PathFillConvex(_color);
            ImGui.GetWindowDrawList().Flags = flags;
        }
        else if (_thickness < 0)
        {
            ImGui.GetWindowDrawList().PathStroke(_color, ImDrawFlags.RoundCornersAll, -_thickness);
        }
        else
        {
            ImGui.GetWindowDrawList().PathStroke(_color, ImDrawFlags.Closed | ImDrawFlags.RoundCornersAll, _thickness);
        }
    }
}
