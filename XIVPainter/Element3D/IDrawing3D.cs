using XIVPainter.Element2D;
using XIVPainter.ElementSpecial;

namespace XIVPainter.Element3D;

public interface IDrawing3D : IDrawing
{
    uint Color { get; set; }
    Action UpdateEveryFrame { get; set; }
    internal byte WarningTime { get; set; }
    internal float AlphaRatio { get; set; }
    public DateTime DeadTime { get; set; }
    internal EaseFuncType DisappearType { get; set; }
    internal float TimeToDisappear { get; set; }
    internal float WarningRatio { get; set; }
    internal EaseFuncType WarningType { get; set; }
}