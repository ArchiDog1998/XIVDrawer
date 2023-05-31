namespace XIVPainter.Element2D;

internal class TextDrawing : IDrawing2D
{
    Vector2 _pt;
    uint _color;
    string _text;

    public TextDrawing(Vector2 pt, uint color, string text)
    {
        _text = text;
        _pt = pt;
        _color = color;
    }
    public void Draw()
    {
        ImGui.GetWindowDrawList().AddText(_pt, _color, _text);
    }
}
