using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using XIVDrawer.Element2D;

namespace XIVDrawer.Element3D;

/// <summary>
/// The 3d drawing element for image.
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="texture"></param>
/// <param name="position">position</param>
/// <param name="size"></param>
public class Drawing3DImage(IDalamudTextureWrap? texture, Vector3 position, float size = 1) : Drawing3D
{
    /// <summary>
    /// The position to draw.
    /// </summary>
    public Vector3 Position { get; set; } = position;

    /// <summary>
    /// <seealso cref="ImGui"/> for texture.
    /// </summary>
    public IDalamudTextureWrap? Image { get; set; } = texture;

    /// <summary>
    /// Drawing Height
    /// </summary>
    public float Size { get; set; } = size;

    /// <summary>
    /// The Image must be in range.
    /// </summary>
    public bool MustInViewRange { get; set; }

    /// <summary>
    /// If the <see cref="Position"/> can't be seen, it'll not be shown.
    /// </summary>
    public bool HideIfInvisible { get; set; }


    /// <summary>
    /// Convert this to the 2d elements.
    /// </summary>
    /// <returns></returns>
    private protected override IEnumerable<IDrawing2D> To2D()
    {
        if (HideIfInvisible && !Position.CanSee() || Image == null || Size == 0)
            return [];

        var pts = XIVDrawerMain.GetPtsOnScreen([Position], false, false);
        if (pts.Length == 0) return [];
        var pt = pts[0];

        var half = new Vector2(Image.Width * Size / 2, Image.Height * Size / 2);

        if (MustInViewRange) unsafe
            {
                var windowPos = ImGuiHelpers.MainViewport.Pos;

                var device = Device.Instance();
                float width = device->Width;
                float height = device->Height;

                pt = XIVDrawerMain.GetPtInRect(windowPos + half, new Vector2(width, height) - 2 * half, pt);
            }

        return [new ImageDrawing(Image, pt - half, pt + half)];
    }
}
