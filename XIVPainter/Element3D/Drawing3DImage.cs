using ImGuiScene;
using XIVPainter.Element2D;
namespace XIVPainter.Element3D;

public class Drawing3DImage : Drawing3D
{
    public Vector3 Position { get; set; }
    public nint ImageID { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public bool HideIfInvisiable { get; set; }
    public Drawing3DImage(TextureWrap wrap, Vector3 position, float size = 1)
       : this(wrap.ImGuiHandle, position, wrap.Width * size, wrap.Height * size)
    {
        
    }

    public Drawing3DImage(nint imageId, Vector3 position, float width, float height)
    {
        ImageID = imageId;
        Position = position;
        Width = width;
        Height = height;
    }

    public override IEnumerable<IDrawing2D> To2D(XIVPainter owner)
    {
        if (HideIfInvisiable && !Position.CanSee() || ImageID == 0) return Array.Empty<IDrawing2D>();

        var pts = owner.GetPtsOnScreen(new Vector3[] { Position }, false);
        if (pts.Length == 0) return Array.Empty<IDrawing2D>();
        var pt = pts[0];

        var half = new Vector2(Width / 2, Height / 2);
        return new IDrawing2D[] { new ImageDrawing(ImageID, pt - half, pt + half) };
    }
}
