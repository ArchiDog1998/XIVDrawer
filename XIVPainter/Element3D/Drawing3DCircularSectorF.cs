namespace XIVPainter.Element3D;

public class Drawing3DCircularSectorF : Drawing3DPolylineF
{
    public Vector3 Center { get; set; }
    public float Radius { get; set; }
    public Vector2[] ArcStartSpan { get; set; }
    public Drawing3DCircularSectorF(Vector3 center, float radius, uint color, float thickness, params Vector2[] arcStartSpan)
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

    public override void UpdateOnFrame(XIVPainter painter)
    {
        base.UpdateOnFrame(painter);

        BorderPoints = ArcStartSpan.Select(pair =>
        {
            IEnumerable<Vector3> pts = painter.SectorPlots(Center, Radius, pair.X, pair.Y);
            if (pair.Y != MathF.Tau && pts.Any())
            {
                pts = pts.Append(Center);
            }
            return pts;
        });
    }
}