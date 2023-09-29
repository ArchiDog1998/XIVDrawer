namespace XIVPainter.Element2D;

/// <summary>
/// Text drawing.
/// </summary>
public readonly struct TextDrawing : IDrawing2D
{
    readonly Vector2 _pt;
    readonly uint _color;
    readonly string _text;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pt"></param>
    /// <param name="color"></param>
    /// <param name="text"></param>
    public TextDrawing(Vector2 pt, uint color, string text)
    {
        _text = text;
        _pt = pt;
        _color = color;
    }

    /// <summary>
    /// Draw on the <seealso cref="ImGui"/>
    /// </summary>
    public void Draw()
    {
        ImGui.GetWindowDrawList().AddText(_pt, _color, _text);
    }
}
