namespace XIVPainter.Enum;

/// <summary>
/// Type of polygon for moving suggestion.
/// </summary>
public enum PolylineType : byte
{
    /// <summary>
    /// It is a normal polygon.
    /// </summary>
    None,

    /// <summary>
    /// The player should go into this polygon.
    /// </summary>
    ShouldGoIn,

    /// <summary>
    /// The player should go out from this polygon.
    /// </summary>
    ShouldGoOut,
}
