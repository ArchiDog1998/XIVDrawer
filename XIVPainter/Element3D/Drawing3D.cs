using XIVPainter.Element2D;

namespace XIVPainter.Element3D;

/// <summary>
/// Basic drawing 2d element.
/// </summary>
public abstract class Drawing3D : IDrawing3D
{
    /// <summary>
    /// The color of drawing. It always be the foreground color.
    /// </summary>
    public uint Color { get; set; }

    /// <summary>
    /// The count of warning time, 1 means 1 time for 1 second.
    /// </summary>
    public byte WarningTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public float AlphaRatio { get; set; }

    /// <summary>
    /// The time that it will disapear.
    /// </summary>
    public DateTime DeadTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public float TimeToDisappear { get; set; }

    /// <summary>
    /// What should it do everyframe.
    /// </summary>
    public Action UpdateEveryFrame { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public EaseFuncType DisappearType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public float WarningRatio { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public EaseFuncType WarningType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected float AnimationRatio { get; set; } = 0;

    /// <summary>
    /// Convert this to the 2d elements.
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public abstract IEnumerable<IDrawing2D> To2D(XIVPainter owner);

    /// <summary>
    /// The things that can be done in the task.
    /// </summary>
    /// <param name="painter"></param>
    public virtual void UpdateOnFrame(XIVPainter painter)
    {
        UpdateEveryFrame?.Invoke();

        if (DeadTime == DateTime.MinValue) return;
        var time = (DateTime.Now - DeadTime).TotalSeconds;

        if (time > TimeToDisappear) return;
        if (time > 0) //Disappear.
        {
            var method = DrawingExtensions.EaseFuncRemap(EaseFuncType.None, DisappearType);
            AlphaRatio = (float)(1 - method(time / TimeToDisappear));
        }
        else if (time > -WarningTime) //Warning.
        {
            AnimationRatio = (WarningTime + (float)time) % 1;

            var percent = ((int)(-time + 1) / (float)(WarningTime + 1)) / 3;
            var inFunc = DrawingExtensions.EaseFuncRemap(WarningType, EaseFuncType.None);
            var outFunc = DrawingExtensions.EaseFuncRemap(WarningType, WarningType);

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
