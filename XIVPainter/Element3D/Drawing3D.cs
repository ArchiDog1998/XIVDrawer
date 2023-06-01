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

    protected float AnimationRatio = 0;
    public abstract IEnumerable<IDrawing2D> To2D(XIVPainter owner);

    public virtual void UpdateOnFrame(XIVPainter painter)
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
            AnimationRatio = (WarningTime + (float)time) % 1;
            var inFunc = DrawingHelper.EaseFuncRemap(WarningType, EaseFuncType.None);
            var outFunc = DrawingHelper.EaseFuncRemap(WarningType, WarningType);
            AlphaRatio = (float)AnimationRatio;
            if (WarningRatio <= 0)
            {
                AlphaRatio = (float)outFunc(AnimationRatio) * (1 - percent) + percent;
            }
            else if (WarningRatio >= 1)
            {
                AlphaRatio = 1 - (float)inFunc(AnimationRatio) * (1 - percent);
            }
            else if (AnimationRatio < WarningRatio)
            {
                AlphaRatio = 1 - (float)inFunc(AnimationRatio / WarningRatio) * (1 - percent);
            }
            else
            {
                AlphaRatio = (float)outFunc((AnimationRatio - WarningRatio) / (1 - WarningRatio)) * (1 - percent)
                    + percent;
            }
        }
    }
}
