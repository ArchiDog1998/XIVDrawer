using XIVPainter.Element2D;

namespace XIVPainter.ElementSpecial;

/// <summary>
/// Drawing element
/// </summary>
public abstract class  IDrawing : BasicDrawing
{
    private protected IDrawing()
    {
        XIVPainterMain._drawingElements.Add(this);
    }

    internal void UpdateOnFrameMain()
    {
        if (!Enable) return;
        UpdateOnFrame();
    }

    /// <summary>
    /// The things that it should upate on every frame.
    /// </summary>
    protected abstract void UpdateOnFrame();

    internal IEnumerable<IDrawing2D> To2DMain()
    {
        if (!Enable) return [];
        return To2D();
    }

    private protected abstract IEnumerable<IDrawing2D> To2D();

    private protected override void AdditionalDispose()
    {
        XIVPainterMain._drawingElements.Remove(this);
    }
}
