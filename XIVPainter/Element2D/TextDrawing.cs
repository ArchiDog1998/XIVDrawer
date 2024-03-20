using Dalamud.Interface.Utility.Raii;

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
/// <param name="padding"></param>
/// <param name="scale"></param>
/// <param name="bgColor"></param>
/// <param name="corner"></param>
/// <param name="id"></param>
public readonly struct TextDrawing(Vector2 pt, uint color, string text, Vector2 padding, float scale, uint bgColor, float corner, int id = 0) : IDrawing2D
{
    readonly Vector2 _pt = pt;
    readonly uint _color = color;
    readonly uint _bgColor = bgColor;
    readonly string _text = text;
    readonly float _scale = scale;
    readonly Vector2 _padding = padding;
    readonly float _corner = corner;

    /// <summary>
    /// Draw on the <seealso cref="ImGui"/>
    /// </summary>
    public void Draw()
    {
        using var padding = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, _padding);
        using var rounding = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, _corner);
        using var bgColor = ImRaii.PushColor(ImGuiCol.ChildBg, _bgColor);
        using var textColor = ImRaii.PushColor(ImGuiCol.Text, _color);
        using var font = ImRaii.PushFont(DrawingExtensions.GetFont(ImGui.GetFontSize() * _scale));

        var size = ImGui.CalcTextSize(_text);
        size += _padding * 2;

        ImGui.SetNextWindowPos(new Vector2(_pt.X - size.X / 2, _pt.Y - size.Y / 2));

        using var child = ImRaii.Child("##TextChild" + (id == 0 ? GetHashCode() : id).ToString(), size, false,
            ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav
            | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysUseWindowPadding);

        ImGui.Text(_text);
    }
}
