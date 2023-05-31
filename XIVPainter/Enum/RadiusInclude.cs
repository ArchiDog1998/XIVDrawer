using Dalamud.Game.ClientState.Objects.Types;

namespace XIVPainter.Enum;

[Flags]
public enum RadiusInclude: byte
{
    None = 0,
    IncludeTarget = 1 << 0,
    IncludePlayer = 1 << 1,
    IncludeBoth = IncludeTarget | IncludePlayer,
}

public static class RadiusIncludeExtension
{
    public static float GetRadius(this RadiusInclude include, GameObject target, float radius)
    {
        if(target == null) return 0;
        if (include.HasFlag(RadiusInclude.IncludePlayer))
        {
            radius += XIVPainter._clientState?.LocalPlayer?.HitboxRadius ?? 0;
        }
        if (include.HasFlag(RadiusInclude.IncludeTarget))
        {
            radius += target.HitboxRadius;
        }
        return radius;
    }
}
