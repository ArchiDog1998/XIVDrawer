using XIVPainter.Element2D;

namespace XIVPainter.Element3D;

public class Drawing3DText : Drawing3D
{
    public string Text { get; set; }
    public Vector3 Position { get; set; }
    public bool HideIfInvisiable { get; set; }
    public Drawing3DText(string text, Vector3 position)
    {
        Text = text;
        Position = position;
    }
    public override IEnumerable<IDrawing2D> To2D(XIVPainter owner)
    {
        if(HideIfInvisiable && !Position.CanSee() || string.IsNullOrEmpty(Text)) return Array.Empty<IDrawing2D>();

        var pts = owner.GetPtsOnScreen(new Vector3[] { Position }, false);
        if(pts.Length == 0) return Array.Empty<IDrawing2D>();
        var pt = pts[0];

        return new IDrawing2D[] { new TextDrawing(pt, Color, Text) };
    }
}
