using XIVPainter.Element2D;

namespace XIVPainter.Element3D;

public class Drawing3DPolyline : Drawing3D
{
    uint showColor;
    public uint InsideColor { get; set; }
    public float Thickness { get; set; }
    public bool IsFill { get; set; } = true;
    public uint MovingColor { get; set; }

    public IEnumerable<IEnumerable<Vector3>> BorderPoints { get; protected set; }
    public IEnumerable<IEnumerable<Vector3>> FillPoints { get; protected set; }

    public Drawing3DPolyline(IEnumerable<Vector3> pts, uint color, float thickness)
        : this(new IEnumerable<Vector3>[] { pts ?? Array.Empty<Vector3>() }, color, thickness)
    {
    }

    public Drawing3DPolyline(IEnumerable<IEnumerable<Vector3>> borderPts, uint color, float thickness, IEnumerable<IEnumerable<Vector3>> fillPoints = null)
    {
        BorderPoints = borderPts ?? Array.Empty<Vector3[]>();
        FillPoints = fillPoints;
        AlphaRatio = 1;
        MovingColor = showColor = InsideColor = Color = color;
        Thickness = thickness;
    }

    public override IEnumerable<IDrawing2D> To2D(XIVPainter owner)
    {
        var baseColor = ImGui.ColorConvertU32ToFloat4(showColor);

        if(baseColor.W == 0) return Array.Empty<IDrawing2D>();

        baseColor.W *= AlphaRatio;
        var fillColor = ImGui.ColorConvertFloat4ToU32(baseColor);
        baseColor.W = AlphaRatio;
        var boarderColor = ImGui.ColorConvertFloat4ToU32(baseColor);

        IEnumerable<IDrawing2D> result = Array.Empty<IDrawing2D>();
        var hasFill = FillPoints != null && FillPoints.Any();
        var hasBorder = Thickness > 0;
        foreach (var points in BorderPoints)
        {
            var pts = owner.GetPtsOnScreen(points, true);

            if(hasBorder)
            {
                if (IsFill)
                {
                    result = result.Append(new PolylineDrawing(pts, boarderColor, Thickness));

                    if(AnimationRatio != 0)
                    {
                        var offset = owner.GetPtsOnScreen(DrawingHelper.OffSetPolyline(points.ToArray(), AnimationRatio * 2), true);

                        baseColor.W *= 1 - AnimationRatio;

                        result = result.Append(new PolylineDrawing(offset, ImGui.ColorConvertFloat4ToU32(baseColor), Thickness));
                    }
                }
                else
                {
                    result = result.Append(new PolylineDrawing(pts, fillColor, Thickness));
                }
            }

            if(!hasFill && IsFill) result = result.Union(DrawingHelper.ConvexPoints(pts).Select(p => new PolylineDrawing(p, fillColor, 0)));
        }

        if (hasFill && IsFill)
        {
            foreach (var points in FillPoints)
            {
                var pts = owner.GetPtsOnScreen(points, true);

                result = result.Union(DrawingHelper.ConvexPoints(pts).Select(p => new PolylineDrawing(p, fillColor, 0)));
            }
        }

        return result;
    }

    public override void UpdateOnFrame(XIVPainter painter)
    {
        base.UpdateOnFrame(painter);

        if (DeadTime != DateTime.MinValue && DateTime.Now > DeadTime) return;
        var inside = XIVPainter._clientState.LocalPlayer != null && DrawingHelper.IsPointInside(XIVPainter._clientState.LocalPlayer.Position, BorderPoints);

        showColor = Color;
        if (XIVPainter._clientState?.LocalPlayer != null)
        {
            if (Color != InsideColor)
            {
                if (inside)
                {
                    showColor = InsideColor;
                }
            }
        }
    }
}
