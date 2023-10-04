using Clipper2Lib;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using XIVPainter.Element2D;
using XIVPainter.Element3D;
using XIVPainter.ElementSpecial;

namespace XIVPainter;

/// <summary>
/// The painter for FFXIV in Dalamud.
/// </summary>
public class XIVPainter : IDisposable
{
    internal readonly string _name;

    internal readonly List<IDrawing> _drawingElements = new List<IDrawing>();
    internal readonly List<Drawing3DHighlightLine> _outLineGo =new List<Drawing3DHighlightLine>();

    private readonly WindowSystem windowSystem;

    #region Config

    /// <summary>
    /// Enable this <seealso cref="XIVPainter"/>
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// if the points of the polyline is not on the ground, remove it.
    /// </summary>
    public bool RemovePtsNotOnGround { get; set; } = false;

    /// <summary>
    /// The height of drawing in the world for making the polyline on the ground.
    /// </summary>
    public float DrawingHeight { get; set; } = 3;

    /// <summary>
    /// The length of sample, please don't set this too low!
    /// </summary>
    public float SampleLength { get; set; } = 0.2f;

    /// <summary>
    /// How long should the animation of disappearing be in second.
    /// </summary>
    public float TimeToDisappear { get; set; } = 1f;

    /// <summary>
    /// The ease function type of disappearing.
    /// </summary>
    public EaseFuncType DisappearType { get; set; } = EaseFuncType.Back;

    /// <summary>
    /// The time of warning.
    /// </summary>
    public byte WarningTime { get; set; } = 3;

    /// <summary>
    /// How much alpha value should be changed when warning.
    /// </summary>
    public float WarningRatio { get; set; } = 0.8f;

    /// <summary>
    /// The ease function type for warning.
    /// </summary>
    public EaseFuncType WarningType { get; set; } = EaseFuncType.Cubic;

    /// <summary>
    /// The color of moving suggestion.
    /// </summary>
    public uint MovingSuggestionColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.8f, 0.2f, 1));

    /// <summary>
    /// Should it show the moving suggestions.
    /// </summary>
    public bool MovingSuggestion { get; set; } = true;

    /// <summary>
    /// The offset of the polygon that it should go.
    /// </summary>
    public float MovingSuggestionOffset { get; set; } = 0.1f;
    #endregion

    /// <summary>
    /// The way to create this.
    /// </summary>
    /// <param name="pluginInterface"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static XIVPainter Create(DalamudPluginInterface pluginInterface, string name)
    {
        pluginInterface.Create<Service>();
        return new XIVPainter(name);
    }

    private XIVPainter(string name)
    {
        _name = name;
        windowSystem = new WindowSystem(name);
        windowSystem.AddWindow(new OverlayWindow(this));

        Service.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Service.Framework.Update += Update;

        RaycastManager.Enable();
    }

    /// <summary>
    /// Don't forget to dispose this!
    /// </summary>
    public void Dispose()
    {
        Service.PluginInterface.UiBuilder.Draw -= windowSystem.Draw;
        Service.Framework.Update -= Update;

        RaycastManager.Dispose();
    }

    private void Draw()
    {
        if (!Enable || Service.ClientState == null || Service.ClientState.LocalPlayer == null) return;
        try
        {
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);

            using var windowStyle = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            if (ImGui.Begin(_name, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav
            ))
            {
                ImGui.GetStyle().AntiAliasedFill = false;

                try
                {
                    IEnumerable<IDrawing2D> result = Array.Empty<IDrawing2D>();

                    if (_drawingElements != null)
                    {
                        foreach (var item in _drawingElements)
                        {
                            result = result.Concat(item.To2D(this));
                        }
                    }

                    foreach (var item in _outLineGo)
                    {
                        result = result.Concat(item.To2D(this));
                    }

                    foreach (var item in result.OrderBy(drawing =>
                    {
                        if (drawing is PolylineDrawing poly)
                        {
                            return poly._thickness == 0 ? 0 : 1;
                        }
                        else
                        {
                            return 2;
                        }
                    }))
                    {
                        item.Draw();
                    }
                }
                catch (Exception ex)
                {
                    Service.Log.Warning(ex, $"{_name} failed to draw on Screen.");
                }
                ImGui.End();
            }
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, $"{_name} failed to draw.");
        }
    }

    private void Update(IFramework framework)
    {
        if (!Enable || Service.ClientState == null || Service.ClientState.LocalPlayer == null) return;

        if (_started) return;
        _started = true;

        Task.Run(UpdateData);
    }

    bool _started = false;
    private async void UpdateData()
    {
        try
        {
            IDrawing2D[] elements = Array.Empty<IDrawing2D>();
            IEnumerable<IDrawing2D> relay = elements;
            List<Task> tasks;
            List<Drawing3DPolyline> movingPoly;

            //lock (_drawing3DLock)
            {
                var length = _drawingElements.Count;
                var remove = new List<IDrawing>(length);
                tasks = new(length + 8);
                movingPoly = new(length);
                for (int i = 0; i < length; i++)
                {
                    var ele = _drawingElements[i];
                    if (ele is IDrawing3D draw && draw.DeadTime != DateTime.MinValue)
                    {
                        var time = (DateTime.Now - draw.DeadTime).TotalSeconds;
                        if (time > TimeToDisappear) //Remove
                        {
                            remove.Add(ele);
                            continue;
                        }

                        if (time <= 0 && ele is Drawing3DPolyline poly)
                        {
                            switch (poly.PolylineType)
                            {
                                case Enum.PolylineType.ShouldGoIn:
                                case Enum.PolylineType.ShouldGoOut:
                                    movingPoly.Add(poly);
                                    break;
                            }
                        }
                    }

                    tasks.Add(Task.Run(() =>
                    {
                        ele.UpdateOnFrame(this);
                    }));
                }
                foreach (var r in remove)
                {
                    _drawingElements.Remove(r);
                }
            }

            if (MovingSuggestion) tasks.Add(Task.Run(() =>
            {
                _outLineGo.Clear();

                if (Service.ClientState == null || Service.ClientState.LocalPlayer == null) return;

                Vector3 start = Service.ClientState.LocalPlayer.Position;
                var color = ImGui.ColorConvertU32ToFloat4(MovingSuggestionColor);

                foreach (var pair in movingPoly.GroupBy(poly => poly.DeadTime).OrderBy(p => p.Key))
                {
                    var c = pair.Count();
                    List<Drawing3DPolyline> outPoly = new List<Drawing3DPolyline>(c),
                                            inPoly = new List<Drawing3DPolyline>(c);

                    foreach (var p in pair)
                    {
                        if(p.PolylineType == Enum.PolylineType.ShouldGoIn) inPoly.Add(p);
                        else if (p.PolylineType == Enum.PolylineType.ShouldGoOut) outPoly.Add(p);
                    }

                    var outPts = GetUnion(outPoly);
                    var inPts = GetUnion(inPoly);


                    if ((outPts == null || !DrawingExtensions.IsPointInside(start, outPts))
                    && (inPts == null || DrawingExtensions.IsPointInside(start, inPts))) continue;

                    outPts = DrawingExtensions.OffSetPolyline(outPts, -MovingSuggestionOffset);
                    inPts = DrawingExtensions.OffSetPolyline(inPts, MovingSuggestionOffset);

                    Vector3 to = Vector3.Zero;
                    if(outPts != null && inPts != null)
                    {
                        var o = DrawingExtensions.Vec3ToPathsD(outPts);
                        var i = DrawingExtensions.Vec3ToPathsD(inPts);
                        var r = Clipper.Difference(i, o, FillRule.NonZero);
                        if(r != null)
                        {
                            var h1 = outPts.Sum(poly => poly.Sum(p => p.Y) / poly.Length) / outPts.Count();
                            var h2 = inPts.Sum(poly => poly.Sum(p => p.Y) / poly.Length) / inPts.Count();

                            var pts = DrawingExtensions.PathsDToVec3(r, (h1 + h2) /2);
                            if(pts != null && pts.Any())
                            {
                                to = DrawingExtensions.GetClosestPoint(start, pts);
                            }
                        }
                    }

                    if (to == Vector3.Zero)
                    {
                        if (outPts != null) to = DrawingExtensions.GetClosestPoint(start, outPts);
                        else if (inPts != null) to = DrawingExtensions.GetClosestPoint(start, inPts);
                        else continue;
                    }

                    var go = new Drawing3DHighlightLine(start, to, 0.5f, ImGui.ColorConvertFloat4ToU32(color), 4);
                    start = to;
                    color.W *= 0.8f;

                    go.UpdateOnFrame(this);
                    _outLineGo.Add(go);
                }
            }));

            await Task.WhenAll(tasks.ToArray());
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, "Something wrong with drawing");
        }

        _started = false;
    }

    private static IEnumerable<Vector3[]> GetUnion(IEnumerable<Drawing3DPolyline> polys)
    {
        if (polys == null || !polys.Any())
        {
            return null;
        }

        PathsD result = null;
        float height = 0;

        foreach (var p in polys)
        {
            height += p.BorderPoints.Sum(poly => poly.Sum(p => p.Y) / poly.Count()) / p.BorderPoints.Count();
            var path = DrawingExtensions.Vec3ToPathsD(p.BorderPoints);
            if (path == null) continue;

            result = result == null ? Clipper.Union(path, FillRule.NonZero)
                : Clipper.Union(result, path, FillRule.NonZero);
        }

        height /= polys.Count();
        return DrawingExtensions.PathsDToVec3(result, height);
    }

    #region Add Remove
    /// <summary>
    /// Add the drawings to the list.
    /// </summary>
    /// <param name="drawings">drawings</param>
    public void AddDrawings(params IDrawing[] drawings)
    {
        _drawingElements.AddRange(drawings);
    }


    /// <summary>
    /// Remove the drawings from the list.
    /// </summary>
    /// <param name="drawings">drawings</param>
    public void RemoveDrawings(params IDrawing[] drawings)
    {
        foreach (var drawing in drawings)
        {
            if (drawing is IDrawing3D draw)
            {
                if (draw.DeadTime == DateTime.MinValue)
                {
                    draw.DeadTime = DateTime.Now;
                }
            }
            else
            {
                _drawingElements.Remove(drawing);
            }
        }
    }
    #endregion

    #region Trasform
    /// <summary>
    /// Make the world point project into the screen.
    /// </summary>
    /// <param name="pts"></param>
    /// <param name="isClosed">Is pts closed</param>
    /// <param name="inScreen">Must be draw in the screen.</param>
    /// <param name="withHeight">With draw height.</param>
    /// <returns></returns>
    public Vector2[] GetPtsOnScreen(IEnumerable<Vector3> pts, bool isClosed, bool inScreen, bool withHeight)
    {
        var cameraPts = ProjectPtsOnGround(DivideCurve(pts, SampleLength, isClosed), withHeight ? DrawingHeight : 0)
            .Select(WorldToCamera).ToArray();
        var changedPts = ChangePtsBehindCamera(cameraPts);

        return changedPts.Select(p => CameraToScreen(p, inScreen)).ToArray();
    }

    IEnumerable<Vector3> DivideCurve(IEnumerable<Vector3> worldPts, float length, bool isClosed)
    {
        if(worldPts == null || worldPts.Count() < 2 || length <= 0.01f) return worldPts;

        IEnumerable<Vector3> pts = Array.Empty<Vector3>();

        DrawingExtensions.SegmentAction(worldPts, (a, b) =>
        {
            pts = pts.Union(DashPoints(a, b, length));
        }, isClosed);

        if(!isClosed) pts = pts.Append(worldPts.Last());

        return pts;
    }

    Vector3[] DashPoints(Vector3 previous, Vector3 next, float length)
    {
        var dir = next - previous;
        var count = Math.Max(1, (int)(dir.Length() / length));
        var points = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            points[i] = previous + dir * i / count;
        }
        return points;
    }

    IEnumerable<Vector3> ChangePtsBehindCamera(Vector3[] cameraPts)
    {
        var changedPts = new List<Vector3>(cameraPts.Length * 2);

        for (int i = 0; i < cameraPts.Length; i++)
        {
            var pt1 = cameraPts[(i - 1 + cameraPts.Length) % cameraPts.Length];
            var pt2 = cameraPts[i];

            if (pt1.Z > 0 && pt2.Z <= 0)
            {
                GetPointOnPlane(pt1, ref pt2);
            }
            if (pt2.Z > 0 && pt1.Z <= 0)
            {
                GetPointOnPlane(pt2, ref pt1);
            }

            if (changedPts.Count > 0 && Vector3.Distance(pt1, changedPts[changedPts.Count - 1]) > 0.001f)
            {
                changedPts.Add(pt1);
            }

            changedPts.Add(pt2);
        }

        return changedPts.Where(p => p.Z > 0);
    }

    unsafe IEnumerable<Vector3> ProjectPtsOnGround(IEnumerable<Vector3> pts, float height)
    {
        if (!RemovePtsNotOnGround && height <= 0) 
            return pts;

        var result = new List<Vector3>(pts.Count());
        foreach (var pt in pts)
        {
            if (RaycastManager.Raycast(in pt, in height, out var territoryPt))
            {
                result.Add(territoryPt);
            }
            else if (!RemovePtsNotOnGround)
            {
                result.Add(pt - Vector3.UnitY * height);
            }
        }
        return result;
    }

    const float PLANE_Z = 0.001f;
    void GetPointOnPlane(Vector3 front, ref Vector3 back)
    {
        if (front.Z <= 0) return;
        if (back.Z > 0) return;

        var ratio = (PLANE_Z - back.Z) / (front.Z - back.Z);
        back.X = (front.X - back.X) * ratio + back.X;
        back.Y = (front.Y - back.Y) * ratio + back.Y;
        back.Z = PLANE_Z;
    }

    unsafe Vector3 WorldToCamera(Vector3 worldPos)
    {
        var camera = CameraManager.Instance()->CurrentCamera;
        return Vector3.Transform(worldPos, camera->ViewMatrix * camera->RenderCamera->ProjectionMatrix);
    }

    unsafe Vector2 CameraToScreen(Vector3 cameraPos, bool inScreen)
    {
        var screenPos = new Vector2(cameraPos.X / MathF.Abs(cameraPos.Z), cameraPos.Y / MathF.Abs(cameraPos.Z));
        var windowPos = ImGuiHelpers.MainViewport.Pos;

        var device = Device.Instance();
        float width = device->Width;
        float height = device->Height;

        screenPos.X = (0.5f * width * (screenPos.X + 1f)) + windowPos.X;
        screenPos.Y = (0.5f * height * (1f - screenPos.Y)) + windowPos.Y;

        if (inScreen)
        {
            screenPos = GetPtInRect(windowPos, new Vector2(width, height), screenPos);
        }
        return screenPos;
    }

    /// <summary>
    /// Make the <paramref name="pt"/> into the Rectange.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <param name="pt"></param>
    /// <returns></returns>
    public static Vector2 GetPtInRect(Vector2 pos, Vector2 size, Vector2 pt)
    {
        var rec = size / 2;
        var center = pos + rec;
        return GetPtInRect(rec, pt - center) + center;
    }

    private static Vector2 GetPtInRect(Vector2 rec, Vector2 pt)
    {
        if (rec.X <= 0 || rec.Y <= 0) return pt;
        return GetPtIn1Rect(pt / rec) * rec;
    }

    private static Vector2 GetPtIn1Rect(Vector2 pt)
    {
        if (pt.X is >= -1 and <= 1 && pt.Y is >= -1 and <= 1) return pt;

        var rate = Math.Max(Math.Abs(pt.X), Math.Abs(pt.Y));
        if (rate == 0) return pt;

        return new Vector2(pt.X / rate, pt.Y / rate);
    }
    #endregion

    internal Vector3[] SectorPlots(Vector3 center, float radius, float rotation, float round)
    {
        var circleSegment = (int)(MathF.Tau * radius / SampleLength);
        return SectorPlots(center, radius, rotation, round, circleSegment);
    }

    internal Vector3[] SectorPlots(Vector3 center, float radius, float rotation, float round, int circleSegment)
    {
        if (radius <= 0) return Array.Empty<Vector3>();
        circleSegment = Math.Max(circleSegment, 16);

        var seg = (int)(circleSegment * round / MathF.Tau);
        var step = round / seg;

        if (round == MathF.Tau) seg--;
        var pts = new Vector3[seg + 1];

        for (int i = 0; i <= seg; i++)
        {
            pts[i] = RoundPoint(center, radius, rotation + i * step);
        }
        return pts;
    }

    static Vector3 RoundPoint(Vector3 pt, double radius, float rotation)
    {
        var x = Math.Sin(rotation) * radius + pt.X;
        var z = Math.Cos(rotation) * radius + pt.Z;
        return new Vector3((float)x, pt.Y, (float)z);
    }
}
