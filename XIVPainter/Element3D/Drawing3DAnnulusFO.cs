using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XIVPainter.Enum;

namespace XIVPainter.Element3D;

public class Drawing3DAnnulusFO : Drawing3DAnnulusF
{
    public RadiusInclude Including { get; set; }
    public float RadiusTarget1 { get; set; }
    public float RadiusTarget2 { get; set; }
    public GameObject Target { get; set; }

    public Drawing3DAnnulusFO(GameObject target, float radius1, float radius2, RadiusInclude include, ushort segments, uint color, float thickness, float round = MathF.Tau) 
        : base(target?.Position ?? default, include.GetRadius(target, radius1), include.GetRadius(target, radius2), segments, color, thickness, target?.Rotation ?? 0, round)
    {
        RadiusTarget1 = radius1;
        RadiusTarget2 = radius2;
        Target = target;
        Including = include;
    }

    public override void UpdateOnFrame()
    {
        Center = Target?.Position ?? default;
        Radius1 = Including.GetRadius(Target, RadiusTarget1);
        Radius2 = Including.GetRadius(Target, RadiusTarget2);
        Rotation = Target?.Rotation ?? 0;
        base.UpdateOnFrame();
    }
}
