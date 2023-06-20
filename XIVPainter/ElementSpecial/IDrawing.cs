using XIVPainter.Element2D;

namespace XIVPainter.ElementSpecial;

public interface IDrawing
{
    void UpdateOnFrame(XIVPainter painter);
    IEnumerable<IDrawing2D> To2D(XIVPainter owner);
}
