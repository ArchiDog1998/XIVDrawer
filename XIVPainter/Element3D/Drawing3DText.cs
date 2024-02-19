using XIVPainter.Element2D;

namespace XIVPainter.Element3D;

/// <summary>
/// The text drawing.
/// </summary>
/// <param name="text"></param>
/// <param name="position"></param>
public class Drawing3DText(string text, Vector3 position) : Drawing3D
{
    /// <summary>
    /// The text it should show.
    /// </summary>
    public string Text { get; set; } = text;

    /// <summary>
    /// The location of showing.
    /// </summary>
    public Vector3 Position { get; set; } = position;

    /// <summary>
    /// Should it hides if the <seealso cref="Position"/> can't be seen by the active camera.
    /// </summary>
    public bool HideIfInvisible { get; set; }

    /// <summary>
    /// Convert this to the 2d elements.
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public override IEnumerable<IDrawing2D> To2D(XIVPainter owner)
    {
        if(HideIfInvisible && !Position.CanSee() || string.IsNullOrEmpty(Text)) return Array.Empty<IDrawing2D>();

        var pts = owner.GetPtsOnScreen(new Vector3[] { Position }, false, false, DrawWithHeight);
        if(pts.Length == 0) return Array.Empty<IDrawing2D>();
        var pt = pts[0];

        return new IDrawing2D[] { new TextDrawing(pt, Color, Text) };
    }
}
