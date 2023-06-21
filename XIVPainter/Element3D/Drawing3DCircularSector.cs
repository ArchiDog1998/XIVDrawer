namespace XIVPainter.Element3D;

/// <summary>
/// The circular sector drawing.
/// </summary>
public class Drawing3DCircularSector : Drawing3DPolyline
{
    /// <summary>
    /// The drawing center.
    /// </summary>
    public Vector3 Center { get; set; }

    /// <summary>
    /// The radius of circular sector.
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// The arc start span.
    /// </summary>
    public Vector2[] ArcStartSpan { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    /// <param name="arcStartSpan"></param>
    public Drawing3DCircularSector(Vector3 center, float radius, uint color, float thickness, params Vector2[] arcStartSpan)
        : base(null, color, thickness)
    {
        Center = center;
        Radius = radius;
        ArcStartSpan = arcStartSpan;
        if(arcStartSpan == null || arcStartSpan.Length == 0)
        {
            ArcStartSpan = new Vector2[] { new Vector2(0, MathF.Tau)};
        }
    }

    /// <summary>
    /// The things that can be done in the task.
    /// </summary>
    /// <param name="painter"></param>
    public override void UpdateOnFrame(XIVPainter painter)
    {
        base.UpdateOnFrame(painter);

        BorderPoints = ArcStartSpan.Select(pair =>
        {
            IEnumerable<Vector3> pts = painter.SectorPlots(Center, Radius, pair.X, pair.Y);
            if (pair.Y != MathF.Tau && pts.Any() && IsFill)
            {
                pts = pts.Append(Center);
            }
            return pts;
        });
    }
}