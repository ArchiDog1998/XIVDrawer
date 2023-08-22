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
    /// 
    /// </summary>
    public float AlphaRatio { get; set; }

    /// <summary>
    /// The time that it will disappear.
    /// </summary>
    public DateTime DeadTime { get; set; }

    /// <summary>
    /// What should it do every frame.
    /// </summary>
    public Action UpdateEveryFrame { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected float AnimationRatio { get; set; } = 0;

    /// <summary>
    /// Draw this element with height.
    /// </summary>
    public bool DrawWithHeight { get; set; } = true;

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

        if (time > painter.TimeToDisappear) return;
        if (time > 0) //Disappear.
        {
            var method = DrawingExtensions.EaseFuncRemap(EaseFuncType.None, painter.DisappearType);
            AlphaRatio = (float)(1 - method(time / painter.TimeToDisappear));
        }
        else if (time > -painter.WarningTime) //Warning.
        {
            AnimationRatio = (painter.WarningTime + (float)time) % 1;

            var percent = ((int)(-time + 1) / (float)(painter.WarningTime + 1)) / 3;
            var inFunc = DrawingExtensions.EaseFuncRemap(painter.WarningType, EaseFuncType.None);
            var outFunc = DrawingExtensions.EaseFuncRemap(painter.WarningType, painter.WarningType);

            if (painter.WarningRatio <= 0)
            {
                AlphaRatio = (float)outFunc(AnimationRatio) * (1 - percent) + percent;
            }
            else if (painter.WarningRatio >= 1)
            {
                AlphaRatio = 1 - (float)inFunc(AnimationRatio) * (1 - percent);
            }
            else if (AnimationRatio < painter.WarningRatio)
            {
                AlphaRatio = 1 - (float)inFunc(AnimationRatio / painter.WarningRatio) * (1 - percent);
            }
            else
            {
                AlphaRatio = (float)outFunc((AnimationRatio - painter.WarningRatio) / (1 - painter.WarningRatio)) * (1 - percent)
                    + percent;
            }
        }
    }
}
