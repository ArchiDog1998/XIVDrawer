using Dalamud.Game.ClientState.Objects.Types;

namespace XIVPainter.Enum;

/// <summary>
/// The type of the radius addition.
/// </summary>
[Flags]
public enum RadiusInclude: byte
{
    /// <summary>
    /// Do not add anything.
    /// </summary>
    None = 0,

    /// <summary>
    /// Add the hit radius of the target.
    /// </summary>
    IncludeTarget = 1 << 0,

    /// <summary>
    /// Add the hit radius of the player.
    /// </summary>
    IncludePlayer = 1 << 1,

    /// <summary>
    /// Add the hit radius of the target and the player.
    /// </summary>
    IncludeBoth = IncludeTarget | IncludePlayer,
}

internal static class RadiusIncludeExtension
{
    internal static float GetRadius(this RadiusInclude include, GameObject target, float radius)
    {
        if(target == null) return 0;
        if (include.HasFlag(RadiusInclude.IncludePlayer))
        {
            radius += Service.ClientState?.LocalPlayer?.HitboxRadius ?? 0;
        }
        if (include.HasFlag(RadiusInclude.IncludeTarget))
        {
            radius += target.HitboxRadius;
        }
        return radius;
    }
}
