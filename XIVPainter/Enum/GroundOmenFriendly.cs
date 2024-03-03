using XIVPainter.Vfx;

namespace XIVPainter.Enum;

/// <summary>
/// 
/// </summary>
public enum GroundOmenFriendly
{
    #region Circle
    /// <summary>
    /// Radius 1
    /// </summary>
    [VfxPath("vfx/omen/eff/general_1bpf.avfx")]
    Circle1,
    #endregion

    #region Rectangle
    /// <summary>
    /// Width 2 Length 1
    /// </summary>
    [VfxPath("vfx/omen/eff/general02_pf.avfx")]
    Rectangle02,
    #endregion

    #region Circular Sector
    /// <summary>
    /// Cone Radius 1, Angle 60
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan060_1bpf.avfx")]
    CircularSector060,

    /// <summary>
    /// Cone Radius 1, Angle 90
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan090_1bpf.avfx")]
    CircularSector090,

    /// <summary>
    /// Cone Radius 1, Angle 120
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan120_1bpf.avfx")]
    CircularSector120,
    #endregion
}
