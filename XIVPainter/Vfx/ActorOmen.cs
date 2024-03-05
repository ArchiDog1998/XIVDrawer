namespace XIVPainter.Vfx;

/// <summary>
/// "vfx/lockon/eff/{Name}.avfx".
/// </summary>
public struct ActorOmen
{
    /// <summary>
    /// Use it with <see cref="OmenHelper.LockOn(string)"/>.
    /// </summary>
    public const string
        Share4 = "com_share0c", //com_share3t ?
        Share2 = "m0618trg_a0k1",
        Single = "lockon5_t0h";
}

/// <summary>
/// 
/// </summary>
public static class OmenHelper
{
    /// <summary>
    /// Make name to lock on path.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string LockOn(this string str) => $"vfx/lockon/eff/{str}.avfx";

    /// <summary>
    /// Make name to omen path.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Omen(this string str) => $"vfx/omen/eff/{str}.avfx";
}
