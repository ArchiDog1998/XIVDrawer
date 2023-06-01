namespace XIVPainter.Element3D;

public class Drawing3DAnnulusF : Drawing3DPolylineF
{
    public Vector3 Center { get; set; }
    public float Radius1 { get; set; }
    public float Radius2 { get; set; }
    public float Rotation { get; set; }
    public float Round { get; set; }
    public Drawing3DAnnulusF(Vector3 center, float radius1, float radius2, uint color,
        float thickness, float rotation = 0, float round = MathF.Tau)
        : base(null, color, thickness)
    {
        Center = center;
        Radius1 = radius1;
        Radius2 = radius2;
        Rotation = rotation;
        Round = round;
    }

    public override void UpdateOnFrame(XIVPainter painter)
    {
        base.UpdateOnFrame(painter);

        var sect1 = painter.SectorPlots(Center, Radius1, Rotation, Round);
        var sect2 = painter.SectorPlots(Center, Radius2, Rotation, Round);
        BorderPoints = new IEnumerable<Vector3>[] { sect1, sect2 };
        FillPoints = GetAnnulusFill(sect1, sect2);
    }

    private static IEnumerable<IEnumerable<Vector3>> GetAnnulusFill(Vector3[] ptsA, Vector3[] ptsB)
    {
        if (ptsA.Length == ptsB.Length)
        {
            var length = ptsA.Length;
            for (int i = 0; i < length; i++)
            {
                var p1 = ptsA[i];
                var p2 = ptsB[i];
                var p3 = ptsB[(i + 1) % length];
                var p4 = ptsA[(i + 1) % length];

                yield return new Vector3[] { p1, p2, p3, p4 };
            }
        }
    }
}
