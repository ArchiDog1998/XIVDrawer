using Dalamud.Game.ClientState.Objects.Types;
using XIVPainter.Enum;

namespace XIVPainter.Element3D;

internal class Drawing3DCircularSectorFO : Drawing3DCircularSectorF
{
    public RadiusInclude Including { get; set; }
    public float RadiusTarget { get; set; }
    public GameObject Target { get; set; }
    public Drawing3DCircularSectorFO(GameObject target, float radius, RadiusInclude include, ushort segments, uint color, float thickness, float round = MathF.Tau) 
        : base(target?.Position ?? default, include.GetRadius(target, radius), segments, color, thickness, target?.Rotation ?? 0, round)
    {
        RadiusTarget = radius;
        Target = target;
        Including = include;
    }

    public override void UpdateOnFrame()
    {
        Center = Target?.Position ?? default;
        Radius = Including.GetRadius(Target, RadiusTarget);
        Rotation = Target?.Rotation ?? 0;
        base.UpdateOnFrame();
    }
}
