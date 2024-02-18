using Dalamud.Game.ClientState.Conditions;

namespace XIVPainter.Element2D;

/// <summary>
/// Drawing the image.
/// </summary>
/// <remarks>
/// 
/// </remarks>
/// <param name="textureId"></param>
/// <param name="pt1"></param>
/// <param name="pt2"></param>
/// <param name="col"></param>
public readonly struct ImageDrawing(nint textureId, Vector2 pt1, Vector2 pt2, uint col = uint.MaxValue) : IDrawing2D
{
    readonly nint _textureId = textureId;
    readonly Vector2 _pt1 = pt1, _pt2 = pt2, _uv1 = default, _uv2 = Vector2.One;
    readonly uint _col = col;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textureId"></param>
    /// <param name="pt1"></param>
    /// <param name="pt2"></param>
    /// <param name="uv1"></param>
    /// <param name="uv2"></param>
    /// <param name="col"></param>
    public ImageDrawing(nint textureId, Vector2 pt1, Vector2 pt2, 
        Vector2 uv1, Vector2 uv2, uint col = uint.MaxValue)
        :this(textureId, pt1, pt2, col)
    {
        _uv1 = uv1;
        _uv2 = uv2;
    }

    private static DateTime _time = DateTime.Now;

    /// <summary>
    /// Draw on the <seealso cref="ImGui"/>
    /// </summary>
    public void Draw()
    {
        var canDraw = !Service.Condition[ConditionFlag.BetweenAreas]
        && !Service.Condition[ConditionFlag.BetweenAreas51]
        && !Service.Condition[ConditionFlag.OccupiedInCutSceneEvent];

        if (!canDraw)
        {
            _time = DateTime.Now;
            return;
        }

        if (DateTime.Now - _time < TimeSpan.FromSeconds(5))
        {
            return;
        }

        ImGui.GetWindowDrawList().AddImage(_textureId, _pt1, _pt2, _uv1, _uv2, _col);
    }
}
