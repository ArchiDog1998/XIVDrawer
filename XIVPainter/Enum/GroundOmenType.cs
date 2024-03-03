using XIVPainter.Vfx;

namespace XIVPainter.Enum;

/// <summary>
/// 
/// </summary>
public enum GroundOmenType
{
    #region Circle
    /// <summary>
    /// Radius 1
    /// </summary>
    [VfxPath("vfx/omen/eff/general_1bf.avfx")]
    Circle1,

    /// <summary>
    /// Radius 1
    /// </summary>
    [VfxPath("vfx/omen/eff/er_general_1f.avfx")]
    Circle1_Er,
    #endregion

    #region Rectangle
    /// <summary>
    /// Width 2 Length 1
    /// </summary>
    [VfxPath("vfx/omen/eff/general02f.avfx")]
    Rectangle02,
    #endregion

    #region Annulus

    /// <summary>
    /// Scale 40 to Outer 40, Inner 12.
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_sircle_4012c.avfx")]
    Annulus4012,

    /// <summary>
    /// Scale 08 to Outer 08, Inner 03.
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_sircle_0803c.avfx")]
    Annulus0803,
    #endregion

    #region Circular Sector
    /// <summary>
    /// Cone Radius 1, Angle 20
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan020_0f.avfx")]
    CircularSector020,

    /// <summary>
    /// Cone Radius 1, Angle 60
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan060_1bf.avfx")]
    CircularSector060,

    /// <summary>
    /// Cone Radius 1, Angle 60
    /// </summary>
    [VfxPath("vfx/omen/eff/er_gl_fan060_1bf.avfx")]
    CircularSector060_Er,

    /// <summary>
    /// Cone Radius 1, Angle 90
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan090_1bf.avfx")]
    CircularSector090,

    /// <summary>
    /// Cone Radius 1, Angle 90, Er
    /// </summary>
    [VfxPath("vfx/omen/eff/er_gl_fan090_1bf.avfx")]
    CircularSector090_Er,

    /// <summary>
    /// Cone Radius 1, Angle 120
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan120_1bf.avfx")]
    CircularSector120,

    /// <summary>
    /// Cone Radius 1, Angle 120
    /// </summary>
    [VfxPath("vfx/omen/eff/er_gl_fan120_1bf.avfx")]
    CircularSector120_Er,

    /// <summary>
    /// Cone Radius 1, Angle 180
    /// </summary>
    [VfxPath("vfx/omen/eff/gl_fan180_1bf.avfx")]
    CircularSector180,

    /// <summary>
    /// Cone Radius 1, Angle 180
    /// </summary>
    [VfxPath("vfx/omen/eff/er_gl_fan180_1bf.avfx")]
    CircularSector180_Er,
    #endregion


    /// <summary>
    /// Angle 180, Scale 26 Outer 26, Inner 20.
    /// </summary>
    [VfxPath("vfx/omen/eff/m0055_fan240r_2620_e1.avfx")]
    AnnularSector240_2620,
}
