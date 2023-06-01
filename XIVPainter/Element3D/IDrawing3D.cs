using XIVPainter.Element2D;

namespace XIVPainter.Element3D;

public interface IDrawing3D
{
    public uint Color { get; set; }
    public Action UpdateEveryFrame { get; set; }
    internal byte WarningTime { get; set; }
    internal float AlphaRatio { get; set; }
    public DateTime DeadTime { get; set; }
    internal EaseFuncType DisappearType { get; set; }
    internal float TimeToDisappear { get; set; }
    internal float WarningRatio { get; set; }
    internal EaseFuncType WarningType { get; set; }
    void UpdateOnFrame(XIVPainter painter);
    IEnumerable<IDrawing2D> To2D(XIVPainter owner);
}