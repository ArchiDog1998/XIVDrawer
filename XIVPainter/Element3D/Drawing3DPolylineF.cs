using XIVPainter.Element2D;
using XIVPainter.Enum;

namespace XIVPainter.Element3D;

public class Drawing3DPolylineF : Drawing3D
{
    uint showColor;
    Drawing3DCircularSectorF _drawingCir;
    public uint InsideColor { get; set; }
    public float Thickness { get; set; }
    public bool IsFill { get; set; } = true;

    public float ClosestPtDis { get; set; } = 0;
    public uint MovingColor { get; set; }

    public IEnumerable<IEnumerable<Vector3>> BorderPoints { get; protected set; }
    public IEnumerable<IEnumerable<Vector3>> FillPoints { get; protected set; }

    public Drawing3DPolylineF(IEnumerable<Vector3> pts, uint color, float thickness)
        : this(new IEnumerable<Vector3>[] { pts ?? Array.Empty<Vector3>() }, color, thickness)
    {
    }

    public Drawing3DPolylineF(IEnumerable<IEnumerable<Vector3>> borderPts, uint color, float thickness, IEnumerable<IEnumerable<Vector3>> fillPoints = null)
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
                result = result.Append(new PolylineDrawing(pts, boarderColor, Thickness));

                var offset = owner.GetPtsOnScreen(DrawingHelper.OffSetPolyline(points.ToArray(), AnimationRatio * 2), true);

                baseColor.W *= 1 - AnimationRatio;

                result = result.Append(new PolylineDrawing(offset, ImGui.ColorConvertFloat4ToU32(baseColor), Thickness));
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

        if(_drawingCir != null)
        {
            result = result.Union(_drawingCir.To2D(owner));
        }

        return result;
    }

    public override void UpdateOnFrame(XIVPainter painter)
    {
        base.UpdateOnFrame(painter);

        if (DeadTime != DateTime.MinValue && DateTime.Now > DeadTime) return;
        var inside = DrawingHelper.IsPointInside(XIVPainter._clientState.LocalPlayer.Position, BorderPoints);

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

            if (ClosestPtDis != 0 && ClosestPtDis > 0 != inside)
            {
                var pts = BorderPoints.Select(pt => DrawingHelper.OffSetPolyline(pt.ToArray(), ClosestPtDis));
                var loc = DrawingHelper.GetClosestPoint(XIVPainter._clientState.LocalPlayer.Position, pts);

                var r = MathF.Abs(ClosestPtDis);
                var d = DateTime.Now.Millisecond / 1000f;
                r *= (float)DrawingHelper.EaseFuncRemap(EaseFuncType.None, EaseFuncType.Cubic)(d);
                _drawingCir = new Drawing3DCircularSectorF(loc, r, MovingColor, 2);
                _drawingCir.UpdateOnFrame(painter);
                return;
            }
        }
        _drawingCir = null;
    }
}
