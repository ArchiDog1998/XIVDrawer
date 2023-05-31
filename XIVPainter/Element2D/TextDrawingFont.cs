namespace XIVPainter.Element2D;

internal class TextDrawingFont : IDrawing2D
{
    Vector2 _pt;
    uint _color;
    string _text;
    ImFontPtr _font;
    float _fontSize;

    public TextDrawingFont(Vector2 pt, uint color, string text, ImFontPtr font, float fontSize)
    {
        _text = text;
        _pt = pt;
        _color = color;
        _font = font;
        _fontSize = fontSize;
    }
    public void Draw()
    {
        ImGui.GetWindowDrawList().AddText(_font, _fontSize, _pt, _color, _text);
    }
}
