namespace XIVPainter.Element2D;

/// <summary>
/// Text drawing.
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="pt"></param>
/// <param name="color"></param>
/// <param name="text"></param>
public readonly struct TextDrawing(Vector2 pt, uint color, string text) : IDrawing2D
{
    readonly Vector2 _pt = pt;
    readonly uint _color = color;
    readonly string _text = text;

    /// <summary>
    /// Draw on the <seealso cref="ImGui"/>
    /// </summary>
    public void Draw()
    {
        ImGui.GetWindowDrawList().AddText(_pt, _color, _text);
    }
}
