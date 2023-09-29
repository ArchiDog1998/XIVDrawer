namespace XIVPainter.Element2D;

/// <summary>
/// Text drawing with font.
/// </summary>
public readonly struct TextDrawingFont : IDrawing2D
{
    readonly Vector2 _pt;
    readonly uint _color;
    readonly string _text;
    readonly ImFontPtr _font;
    readonly float _fontSize;

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
