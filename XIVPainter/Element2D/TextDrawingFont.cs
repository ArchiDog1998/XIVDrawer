namespace XIVPainter.Element2D;

/// <summary>
/// Text drawing with font.
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="pt"></param>
/// <param name="color"></param>
/// <param name="text"></param>
/// <param name="font"></param>
/// <param name="fontSize"></param>
public readonly struct TextDrawingFont(Vector2 pt, uint color, string text, ImFontPtr font, float fontSize) : IDrawing2D
{
    readonly Vector2 _pt = pt;
    readonly uint _color = color;
    readonly string _text = text;
    readonly ImFontPtr _font = font;
    readonly float _fontSize = fontSize;

    /// <summary>
    /// Draw on the <seealso cref="ImGui"/>
    /// </summary>
    public void Draw()
    {
        ImGui.GetWindowDrawList().AddText(_font, _fontSize, _pt, _color, _text);
    }
}
