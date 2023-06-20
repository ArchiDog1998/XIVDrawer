using Clipper2Lib;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using XIVPainter.Element2D;
using XIVPainter.Element3D;

namespace XIVPainter;

public class XIVPainter
{
    readonly string _name;

    //readonly object _drawing3DLock = new object();
    List<IDrawing3D> _drawing3DElements = new List<IDrawing3D>();

    readonly List<Drawing3DHighlightLine> _outLineGo =new List<Drawing3DHighlightLine>();

    [PluginService]
    internal static DalamudPluginInterface _pluginInterface { get; set; }

    [PluginService]
    internal static Framework _framework { get; set; }

    [PluginService]
    internal static ClientState _clientState { get; set; }

    #region Config
    public bool Enable { get; set; } = true;
    public bool RemovePtsNotOnGround { get; set; } = false;
    public float DrawingHeight { get; set; } = 3;
    public float SampleLength { get; set; } = 0.2f;
    public float TimeToDisappear { get; set; } = 1f;
    public EaseFuncType DisappearType { get; set; } = EaseFuncType.Back;
    public byte DefaultWarningTime { get; set; } = 3;
    public float WarningRatio { get; set; } = 0.8f;
    public EaseFuncType WarningType { get; set; } = EaseFuncType.Cubic;
    public uint MovingSuggestionColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.8f, 0.2f, 1));
    public bool MovingSuggestion { get; set; } = true;
    public float MovingSuggestionRadius { get; set; } = 0.1f;
    #endregion

    /// <summary>
    /// Do not use it! Please use DalamudPluginInterface.Create<XIVPainter>(string name) to create this.
    /// </summary>
    /// <param name="name"></param>
    public XIVPainter(string name)
    {
        _name = name;

        _pluginInterface.UiBuilder.Draw += Draw;
        _framework.Update += Update;

        RaycastManager.Enable();
    }

    public void Dispose()
    {
        _pluginInterface.UiBuilder.Draw -= Draw;
        _framework.Update -= Update;

        RaycastManager.Dispose();
    }

    private void Draw()
    {
        if(!Enable) return;
        try
        {
            ImGuiHelpers.ForceNextWindowMainViewport();
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(Vector2.Zero);
            ImGui.SetNextWindowSize(ImGuiHelpers.MainViewport.Size);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            if (ImGui.Begin(_name, ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav
            ))
            {
                ImGui.GetStyle().AntiAliasedFill = false;

                try
                {
                    IEnumerable<IDrawing2D> result = Array.Empty<IDrawing2D>();

                    if (_drawing3DElements != null)
                    {
                        foreach (var item in _drawing3DElements)
                        {
                            result = result.Union(item.To2D(this));
                        }
                    }

                    foreach (var item in _outLineGo)
                    {
                        result = result.Union(item.To2D(this));
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
                    PluginLog.Warning(ex, $"{_name} failed to draw on Screen.");
                }
                ImGui.End();
            }

            ImGui.PopStyleVar();
        }
        catch (Exception ex)
        {
            PluginLog.Warning(ex, $"{_name} failed to draw.");
        }
    }

    private void Update(Framework framework)
    {
        if (!Enable) return;
        Task.Run(UpdateData);
    }

    bool _started = false;
    private async void UpdateData()
    {
        if (_started) return;
        _started = true;

        try
        {
            IDrawing2D[] elements = Array.Empty<IDrawing2D>();
            IEnumerable<IDrawing2D> relay = elements;
            List<Task> tasks;
            List<Drawing3DPolyline> movingPoly;

            //lock (_drawing3DLock)
            {
                var length = _drawing3DElements.Count;
                var remove = new List<IDrawing3D>(length);
                tasks = new(length + 8);
                movingPoly = new(length);
                for (int i = 0; i < length; i++)
                {
                    var ele = _drawing3DElements[i];
                    if (ele.DeadTime != DateTime.MinValue)
                    {
                        var time = (DateTime.Now - ele.DeadTime).TotalSeconds;
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
                    _drawing3DElements.Remove(r);
                }
            }

            if (MovingSuggestion) tasks.Add(Task.Run(() =>
            {
                _outLineGo.Clear();

                Vector3 start = _clientState.LocalPlayer.Position;
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


                    if ((outPts == null || !DrawingHelper.IsPointInside(start, outPts))
                    && (inPts == null || DrawingHelper.IsPointInside(start, inPts))) continue;

                    outPts = DrawingHelper.OffSetPolyline(outPts, -MovingSuggestionRadius);
                    inPts = DrawingHelper.OffSetPolyline(inPts, MovingSuggestionRadius);

                    Vector3 to = Vector3.Zero;
                    if(outPts != null && inPts != null)
                    {
                        var o = DrawingHelper.Vec3ToPathsD(outPts);
                        var i = DrawingHelper.Vec3ToPathsD(inPts);
                        var r = Clipper.Difference(i, o, FillRule.NonZero);
                        if(r != null)
                        {
                            var h1 = outPts.Sum(poly => poly.Sum(p => p.Y) / poly.Length) / outPts.Count();
                            var h2 = inPts.Sum(poly => poly.Sum(p => p.Y) / poly.Length) / inPts.Count();

                            var pts = DrawingHelper.PathsDToVec3(r, (h1 + h2) /2);
                            if(pts != null && pts.Any())
                            {
                                to = DrawingHelper.GetClosestPoint(start, pts);
                            }
                        }
                    }

                    if (to == Vector3.Zero)
                    {
                        if (outPts != null) to = DrawingHelper.GetClosestPoint(start, outPts);
                        else if (inPts != null) to = DrawingHelper.GetClosestPoint(start, inPts);
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
            PluginLog.Warning(ex, "Something wrong with drawing");
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
            var path = DrawingHelper.Vec3ToPathsD(p.BorderPoints);
            if (path == null) continue;

            result = result == null ? Clipper.Union(path, FillRule.NonZero)
                : Clipper.Union(result, path, FillRule.NonZero);
        }

        height /= polys.Count();
        return DrawingHelper.PathsDToVec3(result, height);
    }

    #region Add Remove
    public void AddDrawings(params IDrawing3D[] drawings)
    {
        foreach (var drawing in drawings)
        {
            drawing.DisappearType = DisappearType;
            drawing.TimeToDisappear = TimeToDisappear;
            drawing.WarningRatio = WarningRatio;
            drawing.WarningType = WarningType;
            drawing.WarningTime = DefaultWarningTime;
        }

        //lock (_drawing3DLock)
        {
            _drawing3DElements.AddRange(drawings);
        }
    }

    public void RemoveDrawings(params IDrawing3D[] drawings)
    {
        foreach (var drawing in drawings)
        {
            if(drawing.DeadTime == DateTime.MinValue)
            {
                drawing.DeadTime = DateTime.Now;
            }
        }
    }
    #endregion

    #region Trasform
    public unsafe Vector2[] GetPtsOnScreen(IEnumerable<Vector3> pts, bool isClosed)
    {
        var cameraPts = ProjectPtsOnGround(DivideCurve(pts, SampleLength, isClosed), DrawingHeight)
            .Select(WorldToCamera).ToArray();
        var changedPts = ChangePtsBehindCamera(cameraPts);

        return changedPts.Select(CameraToScreen).ToArray();
    }

    IEnumerable<Vector3> DivideCurve(IEnumerable<Vector3> worldPts, float length, bool isClosed)
    {
        if(worldPts == null || worldPts.Count() < 2 || length <= 0.01f) return worldPts;

        IEnumerable<Vector3> pts = Array.Empty<Vector3>();

        DrawingHelper.SegmentAction(worldPts, (a, b) =>
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
            if (RaycastManager.Raycast(pt, height, out var territoryPt))
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

    unsafe Vector2 CameraToScreen(Vector3 cameraPos)
    {
        var screenPos = new Vector2(cameraPos.X / MathF.Abs(cameraPos.Z), cameraPos.Y / MathF.Abs(cameraPos.Z));
        var windowPos = ImGuiHelpers.MainViewport.Pos;

        var device = Device.Instance();
        float width = device->Width;
        float height = device->Height;

        screenPos.X = (0.5f * width * (screenPos.X + 1f)) + windowPos.X;
        screenPos.Y = (0.5f * height * (1f - screenPos.Y)) + windowPos.Y;

        return screenPos;
    }
    #endregion

    public Vector3[] SectorPlots(Vector3 center, float radius, float rotation, float round)
    {
        var circleSegment = (int)(MathF.Tau * radius / SampleLength);
        return SectorPlots(center, radius, rotation, round, circleSegment);
    }

    public Vector3[] SectorPlots(Vector3 center, float radius, float rotation, float round, int circleSegment)
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
