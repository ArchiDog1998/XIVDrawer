namespace XIVDrawer.Vfx;

/// <summary>
/// "vfx/lockon/eff/{Name}.avfx".
/// </summary>
public struct LockOnOmen
{
    /// <summary>
    /// Use it with <see cref="OmenHelper.LockOn(string)"/>, or <see cref="OmenHelper.Channeling(string)"/>
    /// </summary>
    public const string
        Share4 = "com_share0c", //com_share3t ?
        Share2 = "m0618trg_a0k1",
        Single = "lockon5_t0h";
}

/// <summary>
///  "vfx/channeling/eff/{Name}.avfx".
/// </summary>
public struct ChannelingOmen
{
    /// <summary>
    /// Use it with <see cref="OmenHelper.Channeling(string)"/>, or <see cref="OmenHelper.UnChanneling(string)"/>
    /// </summary>
    public const string
        ChannelingDark = "chn_dark001f";
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
    /// Un lock on the string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string UnLockOn(this string str) => str.Length > 20 ? str[15..^5] : string.Empty;

    /// <summary>
    /// Make name to omen path.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Omen(this string str) => $"vfx/omen/eff/{str}.avfx";

    /// <summary>
    /// Un omen the string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string UnOmen(this string str) => str.Length > 18 ? str[13..^5] : string.Empty;

    /// <summary>
    /// channeling the str.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Channeling(this string str) => $"vfx/channeling/eff/{str}.avfx";

    /// <summary>
    /// Un channeling the string.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string UnChanneling(this string str) => str.Length > 18 ? str[19..^5] : string.Empty;
}
