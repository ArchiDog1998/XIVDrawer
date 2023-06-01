namespace XIVPainter.Element3D;

public class Drawing3DCircularSectorF : Drawing3DPolylineF
{
    public Vector3 Center { get; set; }
    public float Radius { get; set; }
    public float Rotation { get; set; }
    public float Round { get; set; }
    public Drawing3DCircularSectorF(Vector3 center, float radius, uint color, float thickness, float rotation = 0, float round = MathF.Tau)
        : base(null, color, thickness)
    {
        Center = center;
        Radius = radius;
        Rotation = rotation;
        Round = round;
    }

    public override void UpdateOnFrame(XIVPainter painter)
    {
        base.UpdateOnFrame(painter);
        IEnumerable<Vector3> pts = painter.SectorPlots(Center, Radius, Rotation, Round);
        if(Round != MathF.Tau && pts.Any())
        {
            pts = pts.Append(Center);
        }
        BorderPoints = new IEnumerable<Vector3>[] { pts };
    }
}