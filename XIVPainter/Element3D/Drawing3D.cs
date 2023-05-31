using XIVPainter.Element2D;

namespace XIVPainter.Element3D;

public abstract class Drawing3D : IDrawing3D
{
    public uint Color { get; set; }

    public byte WarningTime { get; set; }

    public float AlphaRatio { get; set; }

    public DateTime DeadTime { get; set; }

    public float TimeToDisappear { get; set; }

    public EaseFuncType DisappearType { get; set; }
    public float WarningRatio { get; set; }
    public EaseFuncType WarningType { get; set; }
    public abstract IEnumerable<IDrawing2D> To2D(XIVPainter owner);

    public virtual void UpdateOnFrame()
    {
        if (DeadTime == DateTime.MinValue) return;
        var time = (DateTime.Now - DeadTime).TotalSeconds;

        if (time > TimeToDisappear) return;
        if (time > 0) //Disappear.
        {
            var method = DrawingHelper.EaseFuncRemap(EaseFuncType.None, DisappearType);
            AlphaRatio = (float)(1 - method(time / TimeToDisappear));
        }
        else if (time > -WarningTime) //Warning.
        {
            var percent = ((int)(-time + 1) / (float)(WarningTime + 1)) / 3;
            var inOne = (WarningTime + time) % 1;
            var inFunc = DrawingHelper.EaseFuncRemap(WarningType, EaseFuncType.None);
            var outFunc = DrawingHelper.EaseFuncRemap(WarningType, WarningType);
            AlphaRatio = (float)inOne;
            if (WarningRatio <= 0)
            {
                AlphaRatio = (float)outFunc(inOne) * (1 - percent) + percent;
            }
            else if (WarningRatio >= 1)
            {
                AlphaRatio = 1 - (float)inFunc(inOne) * (1 - percent);
            }
            else if (inOne < WarningRatio)
            {
                AlphaRatio = 1 - (float)inFunc(inOne / WarningRatio) * (1 - percent);
            }
            else
            {
                AlphaRatio = (float)outFunc((inOne - WarningRatio) / (1 - WarningRatio)) * (1 - percent)
                    + percent;
            }
        }
    }
}
