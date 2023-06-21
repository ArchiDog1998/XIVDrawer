using Dalamud.Game.ClientState.Objects.Types;
using XIVPainter.Enum;

namespace XIVPainter.Element3D;

/// <summary>
/// The circular sector drawing binding by a <seealso cref="GameObject"/>.
/// </summary>
public class Drawing3DCircularSectorO : Drawing3DCircularSector
{
    /// <summary>
    /// The type of hit radius adding.
    /// </summary>
    public RadiusInclude Including { get; set; }

    /// <summary>
    /// The radius of target.
    /// </summary>
    public float RadiusTarget { get; set; }

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
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <param name="thickness"></param>
    /// <param name="include"></param>
    /// <param name="arcStartSpan"></param>
    public Drawing3DCircularSectorO(GameObject target, float radius, uint color, float thickness, RadiusInclude include = RadiusInclude.IncludeBoth, params Vector2[] arcStartSpan) 
        : base(target?.Position ?? default, include.GetRadius(target, radius), color, thickness)
    {
        RadiusTarget = radius;
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
        Radius = Including.GetRadius(Target, RadiusTarget);

        ArcStartSpan = ArcStartSpanTarget.Select(pt => new Vector2(pt.X + Target?.Rotation ?? 0, pt.Y)).ToArray();

        base.UpdateOnFrame(painter);
    }
}
