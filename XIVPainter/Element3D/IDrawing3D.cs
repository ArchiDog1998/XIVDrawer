using XIVPainter.Element2D;
using XIVPainter.ElementSpecial;

namespace XIVPainter.Element3D;

/// <summary>
/// 3D drawing element.
/// </summary>
public interface IDrawing3D : IDrawing
{
    /// <summary>
    /// The color of drawing. It always be the foreground color.
    /// </summary>
    uint Color { get; set; }

    /// <summary>
    /// What should it do everyframe.
    /// </summary>
    Action UpdateEveryFrame { get; set; }
    internal byte WarningTime { get; set; }
    internal float AlphaRatio { get; set; }

    /// <summary>
    /// The time that it will disapear.
    /// </summary>
    public DateTime DeadTime { get; set; }
    internal EaseFuncType DisappearType { get; set; }
    internal float TimeToDisappear { get; set; }
    internal float WarningRatio { get; set; }
    internal EaseFuncType WarningType { get; set; }
}