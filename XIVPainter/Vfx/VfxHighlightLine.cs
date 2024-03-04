namespace XIVPainter.Vfx;

/// <summary>
/// The element that can show the location that the player should go.
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="from"></param>
/// <param name="to"></param>
/// <param name="radius"></param>
public class VfxHighlightLine(Vector3 from, Vector3 to, float radius) : BasicDrawing
{
    /// <summary>
    /// The position where it from
    /// </summary>
    public Vector3 From { get; set; } = from;

    /// <summary>
    /// The position where it to
    /// </summary>
    public Vector3 To { get; set; } = to;

    /// <summary>
    /// Highlight circle radius
    /// </summary>
    public float Radius { get; set; } = radius;

    /// <summary>
    /// What should it do every frame.
    /// </summary>
    public Action? UpdateEveryFrame { get; set; }

    readonly StaticVfx
        cir1 = new ("vfx/omen/eff/general_1bpf.avfx", from, 0, Vector3.One * radius), 
        cir2 = new ("vfx/omen/eff/general_1bpf.avfx", to, 0, Vector3.One * radius);

    /// <inheritdoc/>
    public override bool Enable 
    { 
        get => base.Enable; 
        set
        {
            cir1.Enable = cir2.Enable = base.Enable = value;
        }
    }

    private protected override void AdditionalUpdate()
    {
        UpdateEveryFrame?.Invoke();

        var d = DateTime.Now.Millisecond / 1000f;
        var ratio = (float)DrawingExtensions.EaseFuncRemap(EaseFuncType.None, EaseFuncType.Cubic)(d);
        if (Radius == 0)
        {
            cir1.UpdateScale(Vector3.Zero);
            cir2.UpdateScale(Vector3.Zero);
        }
        else
        {
            cir1.UpdateScale(Vector3.One * MathF.Max(0.01f, (1 - ratio) * Radius / 2));
            cir2.UpdateScale(Vector3.One * MathF.Max(0.01f, ratio * Radius));
        }

        cir1.UpdatePosition(From + (To - From) * ratio);
        cir2.UpdatePosition(To);
    }
}
