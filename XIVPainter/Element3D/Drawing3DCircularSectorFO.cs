using Dalamud.Game.ClientState.Objects.Types;
using XIVPainter.Enum;

namespace XIVPainter.Element3D;

public class Drawing3DCircularSectorFO : Drawing3DCircularSectorF
{
    public RadiusInclude Including { get; set; }
    public float RadiusTarget { get; set; }
    public GameObject Target { get; set; }
    public float RotationTarget { get; set; }

    public Drawing3DCircularSectorFO(GameObject target, float radius, uint color, float thickness, float rotation = 0, float round = MathF.Tau, RadiusInclude include = RadiusInclude.IncludeBoth) 
        : base(target?.Position ?? default, include.GetRadius(target, radius), color, thickness, rotation + target?.Rotation ?? 0, round)
    {
        RadiusTarget = radius;
        Target = target;
        Including = include;
        RotationTarget = rotation;
    }

    public override void UpdateOnFrame(XIVPainter painter)
    {
        Center = Target?.Position ?? default;
        Radius = Including.GetRadius(Target, RadiusTarget);
        Rotation = RotationTarget + Target?.Rotation ?? 0;
        base.UpdateOnFrame(painter);
    }
}
