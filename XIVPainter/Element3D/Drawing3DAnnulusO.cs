using Dalamud.Game.ClientState.Objects.Types;
using XIVPainter.Enum;

namespace XIVPainter.Element3D;

/// <summary>
/// The annulus drawing binding by a <seealso cref="GameObject"/>.
/// </summary>
public class Drawing3DAnnulusO : Drawing3DAnnulus
{
    /// <summary>
    /// The type of hit radius adding.
    /// </summary>
    public RadiusInclude Including { get; set; }

    /// <summary>
    /// The radius 1
    /// </summary>
    public float RadiusTarget1 { get; set; }

    /// <summary>
    /// The radius 2
    /// </summary>
    public float RadiusTarget2 { get; set; }

    /// <summary>
    /// The binding target
    /// </summary>
    public GameObject Target { get; set; }

    /// <summary>
    /// Arc start, span.
    /// </summary>
    public Vector2[] ArcStartSpanTarget { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="radius1"></param>
    /// <param name="radius2"></param>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    /// <param name="include"></param>
    /// <param name="arcStartSpan"></param>
    public Drawing3DAnnulusO(GameObject target, float radius1, float radius2, uint color, float thickness, RadiusInclude include = RadiusInclude.IncludeBoth, params Vector2[] arcStartSpan) 
        : base(target?.Position ?? default, include.GetRadius(target, radius1), include.GetRadius(target, radius2), color, thickness)
    {
        RadiusTarget1 = radius1;
        RadiusTarget2 = radius2;
        Target = target;
        Including = include;
        ArcStartSpanTarget = arcStartSpan;
        if (arcStartSpan == null || arcStartSpan.Length == 0)
        {
            ArcStartSpanTarget = new Vector2[] { new Vector2(0, MathF.Tau) };
        }
    }


    /// <summary>
    /// The things that can be done in the task.
    /// </summary>
    /// <param name="painter"></param>
    public override void UpdateOnFrame(XIVPainter painter)
    {
        Center = Target?.Position ?? default;
        if (Target == null)
        {
            Radius1 = Radius2 = 0;
        }
        else
        {
            Radius1 = Including.GetRadius(Target, RadiusTarget1);
            Radius2 = Including.GetRadius(Target, RadiusTarget2);
            ArcStartSpan = ArcStartSpanTarget.Select(pt => new Vector2(pt.X + Target?.Rotation ?? 0, pt.Y)).ToArray();
        }
        
        base.UpdateOnFrame(painter);
    }
}
