using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using ImGuiScene;
using XIVPainter.Element2D;

namespace XIVPainter.Element3D;

/// <summary>
/// The 3d drawing element for image.
/// </summary>
public class Drawing3DImage : Drawing3D
{
    /// <summary>
    /// The position to draw.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// <seealso cref="ImGui"/> handle for texture.
    /// </summary>
    public nint ImageID { get; set; }

    /// <summary>
    /// Drawing width
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Drawing Height
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// The Image must be in range.
    /// </summary>
    public bool MustInViewRange { get; set; }

    /// <summary>
    /// If the <see cref="Position"/> can't be seen, it'll not be shown.
    /// </summary>
    public bool HideIfInvisible { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wrap">texture</param>
    /// <param name="position">position</param>
    /// <param name="size">size ratio</param>
    public Drawing3DImage(IDalamudTextureWrap wrap, Vector3 position, float size = 1)
       : this(wrap?.ImGuiHandle ?? IntPtr.Zero, position,
             wrap?.Width * size ?? 0, wrap?.Height * size ?? 0)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="imageId">Imgui handle</param>
    /// <param name="position">position</param>
    /// <param name="width">drawing width</param>
    /// <param name="height">drawing height</param>
    public Drawing3DImage(nint imageId, Vector3 position, float width, float height)
    {
        ImageID = imageId;
        Position = position;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Set the new texture.
    /// </summary>
    /// <param name="wrap">texture</param>
    /// <param name="size">size ratio</param>
    public void SetTexture(IDalamudTextureWrap wrap, float size = 1)
    {
        ImageID = wrap?.ImGuiHandle ?? IntPtr.Zero;
        Width = wrap?.Width * size ?? 0;
        Height = wrap?.Height * size ?? 0;
    }


    /// <summary>
    /// Convert this to the 2d elements.
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public override IEnumerable<IDrawing2D> To2D(XIVPainter owner)
    {
        if (HideIfInvisible && !Position.CanSee() || ImageID == 0 || Height == 0 || Width == 0) return Array.Empty<IDrawing2D>();

        var pts = owner.GetPtsOnScreen(new Vector3[] { Position }, false, false, DrawWithHeight);
        if (pts.Length == 0) return Array.Empty<IDrawing2D>();
        var pt = pts[0];

        var half = new Vector2(Width / 2, Height / 2);

        if (MustInViewRange) unsafe
        {
            var windowPos = ImGuiHelpers.MainViewport.Pos;

            var device = Device.Instance();
            float width = device->Width;
            float height = device->Height;

            pt = XIVPainter.GetPtInRect(windowPos + half, new Vector2(width, height) - 2 * half, pt);
        }

        return new IDrawing2D[] { new ImageDrawing(ImageID, pt - half, pt + half) };
    }
}
