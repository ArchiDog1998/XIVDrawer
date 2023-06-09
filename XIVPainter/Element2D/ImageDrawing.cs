namespace XIVPainter.Element2D;

internal class ImageDrawing : IDrawing2D
{
    nint _textureId;
    Vector2 _pt1, _pt2;

    public ImageDrawing(nint textureId, Vector2 pt1, Vector2 pt2)
    {
        _textureId = textureId;
        _pt1 = pt1;
        _pt2 = pt2;
    }
    public void Draw()
    {
        ImGui.GetWindowDrawList().AddImage(_textureId, _pt1, _pt2);
    }
}
