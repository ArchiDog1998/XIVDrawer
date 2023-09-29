namespace XIVPainter.Element2D;

/// <summary>
/// Polyline drawing.
/// </summary>
public readonly struct PolylineDrawing : IDrawing2D
{
    readonly Vector2[] _pts;
    readonly uint _color;
    readonly internal float _thickness;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pts"></param>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    public PolylineDrawing(Vector2[] pts, uint color, float thickness)
    {
        _pts = pts;
        _color = color;
        _thickness = thickness;
    }

    /// <summary>
    /// Draw on the <seealso cref="ImGui"/>
    /// </summary>
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
            ImGui.GetWindowDrawList().PathFillConvex(_color);
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
