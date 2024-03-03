using XIVPainter.Vfx;

namespace XIVPainter.Enum;

/// <summary>
/// The Omen Type for the Object.
/// </summary>
public enum ObjectOmenType
{
    /// <summary>
    /// Radius 2, 5 Seconds.
    /// </summary>
    [VfxPath("vfx/lockon/eff/tank_lockon02k1.avfx")]
    TankLock02,

    /// <summary>
    /// Radius 6, Times 3, 5 Seconds.
    /// </summary>
    [VfxPath("vfx/lockon/eff/com_share6_5s0c.avfx")]
    TankShare03_05,

    /// <summary>
    /// Radius 7, 5 Seconds.
    /// </summary>
    [VfxPath("vfx/lockon/eff/com_share3t.avfx")]
    Share07,

    /// <summary>
    /// Radius 5, 5 Seconds
    /// </summary>
    [VfxPath("vfx/lockon/eff/loc05sp_05af.avfx")]
    Separator05,

    /// <summary>
    /// Radius 6, 5 Seconds
    /// </summary>
    [VfxPath("vfx/lockon/eff/target_ae_s5f.avfx")]
    Separator06,
}
