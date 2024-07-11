using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures.TextureWraps;

namespace XIVDrawer.Element2D;

/// <summary>
/// Drawing the image.
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="texture"></param>
/// <param name="pt1"></param>
/// <param name="pt2"></param>
/// <param name="col"></param>
public readonly struct ImageDrawing(IDalamudTextureWrap texture, Vector2 pt1, Vector2 pt2, uint col = uint.MaxValue) : IDrawing2D
{
    private readonly IDalamudTextureWrap _texture = texture;
    private readonly Vector2 _pt1 = pt1, _pt2 = pt2, _uv1 = default, _uv2 = Vector2.One;
    private readonly uint _col = col;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="pt1"></param>
    /// <param name="pt2"></param>
    /// <param name="uv1"></param>
    /// <param name="uv2"></param>
    /// <param name="col"></param>
    public ImageDrawing(IDalamudTextureWrap texture, Vector2 pt1, Vector2 pt2,
        Vector2 uv1, Vector2 uv2, uint col = uint.MaxValue)
        : this(texture, pt1, pt2, col)
    {
        _uv1 = uv1;
        _uv2 = uv2;
    }

    /// <summary>
    /// Draw on the <seealso cref="ImGui"/>
    /// </summary>
    public void Draw()
    {
        ImGui.GetWindowDrawList().AddImage(_texture.ImGuiHandle, _pt1, _pt2, _uv1, _uv2, _col);
    }
}
