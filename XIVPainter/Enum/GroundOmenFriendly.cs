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
    [VfxPath("vfx/omen/eff/general_1bpxf.avfx")]
    Circle1,
    #endregion

    #region Rectangle
    /// <summary>
    /// Width 2 Length 1
    /// </summary>
    [VfxPath("vfx/omen/eff/general02pxf.avfx")]
    Rectangle01,

    /// <summary>
    /// Width 2 Length -1 to 1
    /// </summary>
    [VfxPath("vfx/omen/eff/general_x02pf.avfx")]
    Rectangle02,

    #endregion

    #region Circular Sector
    /// <summary>
    /// Cone Radius 1, Angle 20
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan020_0pt.avfx")]
    CircularSector020,

    /// <summary>
    /// Cone Radius 1, Angle 30
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan030_1bpf.avfx")]
    CircularSector030,

    /// <summary>
    /// Cone Radius 1, Angle 45
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan045_1bpxf.avfx")]
    CircularSector045,

    /// <summary>
    /// Cone Radius 1, Angle 60
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan060_1bpf.avfx")]
    CircularSector060,

    /// <summary>
    /// Cone Radius 1, Angle 90
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan090_1bpxf.avfx")]
    CircularSector090,

    /// <summary>
    /// Cone Radius 1, Angle 120
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan120_1bpxf.avfx")]
    CircularSector120,

    /// <summary>
    /// Cone Radius 1, Angle 150
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan150_1bpf.avfx")]
    CircularSector150,
    #endregion
}
