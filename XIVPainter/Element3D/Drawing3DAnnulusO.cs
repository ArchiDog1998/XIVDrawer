using Dalamud.Game.ClientState.Objects.Types;
using XIVPainter.Enum;

namespace XIVPainter.Element3D;

public class Drawing3DAnnulusO : Drawing3DAnnulus
{
    public RadiusInclude Including { get; set; }
    public float RadiusTarget1 { get; set; }
    public float RadiusTarget2 { get; set; }
    public GameObject Target { get; set; }
    public Vector2[] ArcStarSpanTarget { get; set; }

    public Drawing3DAnnulusO(GameObject target, float radius1, float radius2, uint color, float thickness, RadiusInclude include = RadiusInclude.IncludeBoth, params Vector2[] arcStartSpan) 
        : base(target?.Position ?? default, include.GetRadius(target, radius1), include.GetRadius(target, radius2), color, thickness)
    {
        RadiusTarget1 = radius1;
        RadiusTarget2 = radius2;
        Target = target;
        Including = include;
        ArcStarSpanTarget = arcStartSpan;
        if (arcStartSpan == null || arcStartSpan.Length == 0)
        {
            ArcStarSpanTarget = new Vector2[] { new Vector2(0, MathF.Tau) };
        }
    }

    public override void UpdateOnFrame(XIVPainter painter)
    {
        Center = Target?.Position ?? default;
        Radius1 = Including.GetRadius(Target, RadiusTarget1);
        Radius2 = Including.GetRadius(Target, RadiusTarget2);
        ArcStartSpan = ArcStarSpanTarget.Select(pt => new Vector2(pt.X + Target?.Rotation ?? 0, pt.Y)).ToArray();
        
        base.UpdateOnFrame(painter);
    }
}
