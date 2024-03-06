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
    /// The padding of the bg.
    /// </summary>
    public Vector2 Padding { get; set; } = Vector2.One * 5;

    /// <summary>
    /// The size of the text.
    /// </summary>
    public float Scale { get; set; } = 1;

    /// <summary>
    /// The background Color.
    /// </summary>
    public uint BackgroundColor { get; set; } = 0x00000080;

    /// <summary>
    /// The corner of the background.
    /// </summary>
    public float Corner { get; set; } = 5;

    /// <summary>
    /// Convert this to the 2d elements.
    /// </summary>
    /// <returns></returns>
    private protected override IEnumerable<IDrawing2D> To2D()
    {
        if (HideIfInvisible && !Position.CanSee() || string.IsNullOrEmpty(Text)) return [];

        var pts = XIVPainterMain.GetPtsOnScreen([Position], false, false);
        if(pts.Length == 0) return [];
        var pt = pts[0];

        return [new TextDrawing(pt, Color, Text, Padding, Scale, BackgroundColor, Corner, GetHashCode())];
    }
}
