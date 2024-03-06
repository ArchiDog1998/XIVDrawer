using XIVPainter.Element2D;

namespace XIVPainter.Element3D;

/// <summary>
/// A set of drawings
/// </summary>
public class Drawing3DPoly : Drawing3D
{
    /// <summary>
    /// The sub items.
    /// </summary>
    public Drawing3D[] SubItems { get; set; } = [];

    /// <summary>
    /// Convert this to the 2d elements.
    /// </summary>
    /// <returns></returns>
    private protected override IEnumerable<IDrawing2D> To2D()
    {
        return SubItems.SelectMany(i => i.To2DMain()) ?? [];
    }

    /// <summary>
    /// The things that can be done in the task.
    /// </summary>
    protected override void UpdateOnFrame()
    {
        base.UpdateOnFrame();
        foreach (var item in SubItems)
        {
            item.DeadTime = DeadTime;
            item.UpdateOnFrameMain();
        }
    }
}
