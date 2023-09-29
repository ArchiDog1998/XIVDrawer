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
    /// What should it do every frame.
    /// </summary>
    Action UpdateEveryFrame { get; set; }

    /// <summary>
    /// The time that it will disappear.
    /// </summary>
    public DateTime DeadTime { get; set; }
}