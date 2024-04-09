using XIVDrawer.Element2D;

namespace XIVDrawer.ElementSpecial;

/// <summary>
/// Drawing element
/// </summary>
public abstract class IDrawing : BasicDrawing
{
    private protected IDrawing()
    {
        XIVDrawerMain._drawingElements.Add(this);
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
        XIVDrawerMain._drawingElements.Remove(this);
    }
}
