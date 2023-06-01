using Dalamud.Game.ClientState.Objects.Types;
using XIVPainter.Enum;

namespace XIVPainter.Element3D;

public class Drawing3DAnnulusFO : Drawing3DAnnulusF
{
    public RadiusInclude Including { get; set; }
    public float RadiusTarget1 { get; set; }
    public float RadiusTarget2 { get; set; }
    public GameObject Target { get; set; }
    public float RotationTarget { get; set; }

    public Drawing3DAnnulusFO(GameObject target, float radius1, float radius2, uint color, float thickness, float rotation = 0, float round = MathF.Tau, RadiusInclude include = RadiusInclude.IncludeBoth) 
        : base(target?.Position ?? default, include.GetRadius(target, radius1), include.GetRadius(target, radius2), color, thickness, rotation + target?.Rotation ?? 0, round)
    {
        RadiusTarget1 = radius1;
        RadiusTarget2 = radius2;
        Target = target;
        Including = include;
        RotationTarget = rotation;
    }

    public override void UpdateOnFrame(XIVPainter painter)
    {
        Center = Target?.Position ?? default;
        Radius1 = Including.GetRadius(Target, RadiusTarget1);
        Radius2 = Including.GetRadius(Target, RadiusTarget2);
        Rotation = RotationTarget + Target?.Rotation ?? 0;
        base.UpdateOnFrame(painter);
    }
}
