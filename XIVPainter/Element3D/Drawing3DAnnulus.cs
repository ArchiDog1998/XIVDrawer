namespace XIVPainter.Element3D;

/// <summary>
/// The annulus drawing.
/// </summary>
public class Drawing3DAnnulus : Drawing3DPolyline
{
    /// <summary>
    /// The drawing center.
    /// </summary>
    public Vector3 Center { get; set; }

    /// <summary>
    /// The radius 1 of annulus.
    /// </summary>
    public float Radius1 { get; set; }

    /// <summary>
    /// The radius 2 of annulus.
    /// </summary>
    public float Radius2 { get; set; }

    /// <summary>
    /// The arc start span.
    /// </summary>
    public Vector2[] ArcStartSpan { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius1"></param>
    /// <param name="radius2"></param>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    /// <param name="arcStartSpan"></param>
    public Drawing3DAnnulus(Vector3 center, float radius1, float radius2, uint color,
        float thickness, params Vector2[] arcStartSpan)
        : base(null, color, thickness)
    {
        Center = center;
        Radius1 = radius1;
        Radius2 = radius2;
        ArcStartSpan = arcStartSpan;
        if (arcStartSpan == null || arcStartSpan.Length == 0)
        {
            ArcStartSpan = new Vector2[] { new Vector2(0, MathF.Tau) };
        }
    }

    /// <summary>
    /// The things that can be done in the task.
    /// </summary>
    /// <param name="painter"></param>
    public override void UpdateOnFrame(XIVPainter painter)
    {
        base.UpdateOnFrame(painter);

        if (Radius1 == 0 || Radius2 == 0)
        {
            BorderPoints = FillPoints = Array.Empty<Vector3[]>();
            return;
        }

        IEnumerable<IEnumerable<Vector3>> boarder = Array.Empty<IEnumerable<Vector3>>(),
            fill = Array.Empty<IEnumerable<Vector3>>();
        foreach (var pair in ArcStartSpan)
        {
            var circleSegment = (int)(MathF.Tau * MathF.Max(Radius1, Radius2) / painter.SampleLength);
            circleSegment = Math.Min(circleSegment, 72);

            var sect1 = painter.SectorPlots(Center, Radius1, pair.X, pair.Y, circleSegment).Reverse().ToArray();
            var sect2 = painter.SectorPlots(Center, Radius2, pair.X, pair.Y, circleSegment);
            boarder = boarder.Append(sect1);
            boarder = boarder.Append(sect2);

            var bound = new List<Vector3>(sect1);
            bound.AddRange(sect2);

            fill = fill.Append(bound);
            
            if(pair.Y == MathF.Tau)
            {
                fill = fill.Append(new Vector3[]
                {
                    sect1.First(),
                    sect1.Last(),
                    sect2.First(),
                    sect2.Last(),
                });
            }
        }
        BorderPoints = boarder;
        FillPoints = fill;
    }
}
