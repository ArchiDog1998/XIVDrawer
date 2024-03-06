using XIVPainter.ElementSpecial;

namespace XIVPainter.Element3D;

/// <summary>
/// Basic drawing 2d element.
/// </summary>
public abstract class Drawing3D : IDrawing
{
    /// <summary>
    /// The color of drawing. It always be the foreground color.
    /// </summary>
    public uint Color { get; set; }

    /// <summary>
    /// What should it do every frame.
    /// </summary>
    public Action? UpdateEveryFrame { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public float AlphaRatio { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected float AnimationRatio { get; set; } = 0;

    /// <summary>
    /// The things that can be done in the task.
    /// </summary>
    protected override void UpdateOnFrame()
    {
        UpdateEveryFrame?.Invoke();

        if (DeadTime == DateTime.MinValue) return;
        var time = (DateTime.Now - DeadTime).TotalSeconds;

        if (time > -XIVPainterMain.WarningTime) //Warning.
        {
            AnimationRatio = (XIVPainterMain.WarningTime + (float)time) % 1;

            var percent = ((int)(-time + 1) / (float)(XIVPainterMain.WarningTime + 1)) / 3;
            var inFunc = DrawingExtensions.EaseFuncRemap(XIVPainterMain.WarningType, EaseFuncType.None);
            var outFunc = DrawingExtensions.EaseFuncRemap(XIVPainterMain.WarningType, XIVPainterMain.WarningType);

            if (XIVPainterMain.WarningRatio <= 0)
            {
                AlphaRatio = (float)outFunc(AnimationRatio) * (1 - percent) + percent;
            }
            else if (XIVPainterMain.WarningRatio >= 1)
            {
                AlphaRatio = 1 - (float)inFunc(AnimationRatio) * (1 - percent);
            }
            else if (AnimationRatio < XIVPainterMain.WarningRatio)
            {
                AlphaRatio = 1 - (float)inFunc(AnimationRatio / XIVPainterMain.WarningRatio) * (1 - percent);
            }
            else
            {
                AlphaRatio = (float)outFunc((AnimationRatio - XIVPainterMain.WarningRatio) / (1 - XIVPainterMain.WarningRatio)) * (1 - percent)
                    + percent;
            }
        }
    }
}
