using XIVPainter.Element2D;

namespace XIVPainter.ElementSpecial;

/// <summary>
/// Drawing element
/// </summary>
public interface IDrawing
{
    /// <summary>
    /// The things that can be done in the task.
    /// </summary>
    /// <param name="painter"></param>
    void UpdateOnFrame(XIVPainter painter);

    /// <summary>
    /// Convert this to the 2d elements.
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    IEnumerable<IDrawing2D> To2D(XIVPainter owner);
}
