namespace XIVPainter.Vfx;

/// <summary>
/// "vfx/lockon/eff/{Name}.avfx".
/// </summary>
public struct ActorOmen
{
    public const string
        Share4 = "com_share0c", //com_share3t ?
        Share2 = "m0618trg_a0k1",
        Single = "lockon5_t0h";
}

public static class OmenHelper
{

    public static string LockOn(this string str) => $"vfx/lockon/eff/{str}.avfx";
    public static string Omen(this string str) => $"vfx/omen/eff/{str}.avfx";
}
