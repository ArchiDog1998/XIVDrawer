using XIVPainter.Element2D;
using XIVPainter.Enum;

namespace XIVPainter.Element3D;

/// <summary>
/// The polygon element.
/// </summary>
public class Drawing3DPolyline : Drawing3D
{
    uint showColor;

    /// <summary>
    /// If the player is inside of this, what the color it should be.
    /// </summary>
    public uint InsideColor { get; set; }

    /// <summary>
    /// The thickness of curve.
    /// </summary>
    public float Thickness { get; set; }

    /// <summary>
    /// Should it be filled.
    /// </summary>
    public bool IsFill { get; set; } = true;

    /// <summary>
    /// Whether the mouse is within the drawing range.
    /// </summary>
    public bool IsMouseInside { get; private set; } = false;

    /// <summary>
    /// The type of it, you can set it for moving suggestion.
    /// </summary>
    public PolylineType PolylineType { get; set; } = PolylineType.None;

    /// <summary>
    /// The border of polyline.
    /// </summary>
    public IEnumerable<IEnumerable<Vector3>> BorderPoints { get; protected set; }

    /// <summary>
    /// The fill of polygon.
    /// </summary>
    public IEnumerable<IEnumerable<Vector3>> FillPoints { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pts"></param>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    public Drawing3DPolyline(IEnumerable<Vector3> pts, uint color, float thickness)
        : this(new IEnumerable<Vector3>[] { pts ?? Array.Empty<Vector3>() }, color, thickness)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="borderPts"></param>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    /// <param name="fillPoints"></param>
    public Drawing3DPolyline(IEnumerable<IEnumerable<Vector3>> borderPts, uint color, float thickness, IEnumerable<IEnumerable<Vector3>> fillPoints = null)
    {
        BorderPoints = borderPts ?? Array.Empty<Vector3[]>();
        FillPoints = fillPoints;
        AlphaRatio = 1;
        showColor = InsideColor = Color = color;
        Thickness = thickness;
    }

    /// <summary>
    /// Convert this to the 2d elements.
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
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
        var hasBorder = Thickness != 0;

        var screenPts = new List<Vector2[]>(BorderPoints.Count());
        foreach (var points in BorderPoints)
        {
            var pts = owner.GetPtsOnScreen(points, Thickness > 0, false, DrawWithHeight);
            screenPts.Add(pts);

            if (hasBorder)
            {
                if (IsFill)
                {
                    result = result.Append(new PolylineDrawing(pts, boarderColor, Thickness));

                    if(AnimationRatio != 0)
                    {
                        foreach (var item in DrawingExtensions.OffSetPolyline(points.ToArray(), -AnimationRatio))
                        {
                            var offset = owner.GetPtsOnScreen(item, true, false, DrawWithHeight);

                            baseColor.W *= 1 - AnimationRatio;

                            result = result.Append(new PolylineDrawing(offset, ImGui.ColorConvertFloat4ToU32(baseColor), Thickness));
                        }
                    }
                }
                else
                {
                    result = result.Append(new PolylineDrawing(pts, fillColor, Thickness));
                }
            }

            if(!hasFill && IsFill) result = result.Concat(DrawingExtensions.ConvexPoints(pts)
                .Select(p => new PolylineDrawing(p, fillColor, 0) as IDrawing2D));
        }

        IsMouseInside = DrawingExtensions.IsPointInside(ImGui.GetMousePos(), screenPts);

        if (hasFill && IsFill)
        {
            foreach (var points in FillPoints)
            {
                var pts = owner.GetPtsOnScreen(points, true, false, DrawWithHeight);

                result = result.Concat(DrawingExtensions.ConvexPoints(pts)
                    .Select(p => new PolylineDrawing(p, fillColor, 0) as IDrawing2D));
            }
        }

        return result;
    }

    /// <summary>
    /// The things that can be done in the task.
    /// </summary>
    /// <param name="painter"></param>
    public override void UpdateOnFrame(XIVPainter painter)
    {
        base.UpdateOnFrame(painter);

        if (DeadTime != DateTime.MinValue && DateTime.Now > DeadTime) return;
        var inside = Service.ClientState.LocalPlayer != null && DrawingExtensions.IsPointInside(Service.ClientState.LocalPlayer.Position, BorderPoints);

        showColor = Color;
        if (Service.ClientState?.LocalPlayer != null)
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
