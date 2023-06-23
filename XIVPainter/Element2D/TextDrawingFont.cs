namespace XIVPainter.Element2D;

/// <summary>
/// Text drawing with font.
/// </summary>
public class TextDrawingFont : IDrawing2D
{
    Vector2 _pt;
    uint _color;
    string _text;
    ImFontPtr _font;
    float _fontSize;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pt"></param>
    /// <param name="color"></param>
    /// <param name="text"></param>
    /// <param name="font"></param>
    /// <param name="fontSize"></param>
    public TextDrawingFont(Vector2 pt, uint color, string text, ImFontPtr font, float fontSize)
    {
        _text = text;
        _pt = pt;
        _color = color;
        _font = font;
        _fontSize = fontSize;
    }

    /// <summary>
    /// Draw on the <seealso cref="ImGui"/>
    /// </summary>
    public void Draw()
    {
        ImGui.GetWindowDrawList().AddText(_font, _fontSize, _pt, _color, _text);
    }
}
