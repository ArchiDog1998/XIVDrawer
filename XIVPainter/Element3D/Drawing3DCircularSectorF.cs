namespace XIVPainter.Element3D;

public class Drawing3DCircularSectorF : Drawing3DPolylineF
{
    public Vector3 Center { get; set; }
    public float Radius { get; set; }
    public float Rotation { get; set; }
    public float Round { get; set; }
    public ushort Segments { get; set; }
    public Drawing3DCircularSectorF(Vector3 center, float radius, ushort segments, uint color, float thickness, float rotation = 0, float round = MathF.Tau)
        : base(null, color, thickness)
    {
        Center = center;
        Radius = radius;
        Rotation = rotation;
        Round = round;
        Segments = segments;
    }

    public override void UpdateOnFrame()
    {
        base.UpdateOnFrame();
        BorderPoints = new IEnumerable<Vector3>[] { DrawingHelper.SectorPlots(Center, Radius, Rotation, Round, Segments) };
    }
}