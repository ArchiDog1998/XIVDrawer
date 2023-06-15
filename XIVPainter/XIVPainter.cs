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

    readonly object _drawing2DLock = new object();
    IDrawing2D[] _drawing2DElements;

    readonly object _drawing3DLock = new object();
    List<IDrawing3D> _drawing3DElements = new List<IDrawing3D>();

    [PluginService]
    internal static DalamudPluginInterface _pluginInterface { get; set; }

    [PluginService]
    internal static Framework _framework { get; set; }

    [PluginService]
    internal static ClientState _clientState { get; set; }

    #region Config
    public bool Enable { get; set; } = true;
    public bool UseTaskForAccelerate { get; set; } = false;
    public bool RemovePtsNotOnGround { get; set; } = false;
    public float DrawingHeight { get; set; } = 3;
    public float SampleLength { get; set; } = 0.2f;
    public float TimeToDisappear { get; set; } = 1f;
    public EaseFuncType DisappearType { get; set; } = EaseFuncType.Back;
    public byte DefaultWarningTime { get; set; } = 3;
    public float WarningRatio { get; set; } = 0.8f;
    public EaseFuncType WarningType { get; set; } = EaseFuncType.Cubic;
    public uint MovingSuggestionColor { get; set; } = ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.8f, 0.2f, 0.15f));
    public bool MovingSuggestion { get; set; } = true;
    public float MovingSuggestionRadius { get; set; } = 0.5f;
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

        RaycastManager.Enable(Directory.GetParent(_pluginInterface.ConfigDirectory.FullName)?.FullName);
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
                try
                {
                    lock(_drawing2DLock)
                    {
                        if (_drawing2DElements != null)
                        {
                            foreach (var element in _drawing2DElements)
                            {
                                element.Draw();
                            }
                        }
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

        if (UseTaskForAccelerate)
        {
            Task.Run(UpdateData);
        }
        else
        {
            UpdateData();
        }
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
            List<Task<IEnumerable<IDrawing2D>>> tasks, outLineTasks = new List<Task<IEnumerable<IDrawing2D>>>(8);
            List<Drawing3DPolyline> outPoly;
            List<Drawing3DPolyline> inPoly;

            lock (_drawing3DLock)
            {
                var length = _drawing3DElements.Count;
                var remove = new List<IDrawing3D>(length);
                tasks = new(length);
                outPoly = new(length);
                inPoly = new(length);
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

                        if (ele is Drawing3DPolyline poly)
                        {
                            switch (poly.PolylineType)
                            {
                                case Enum.PolylineType.ShouldGoOut:
                                    outPoly.Add(poly);
                                    break;

                                case Enum.PolylineType.ShouldGoIn:
                                    inPoly.Add(poly);
                                    break;
                            }
                        }
                    }

                    tasks.Add(Task.Run(() =>
                    {
                        ele.UpdateOnFrame(this);
                        return ele.To2D(this);
                    }));
                }
                foreach (var r in remove)
                {
                    _drawing3DElements.Remove(r);
                }
            }

            if (MovingSuggestion)
            {
                outLineTasks.Add(Task.Run(() =>
                {
                    IEnumerable<IDrawing2D> result = Array.Empty<IDrawing2D>();

                    Vector3 start = _clientState.LocalPlayer.Position;
                    foreach (var pair in outPoly.GroupBy(poly => poly.DeadTime).OrderBy(p => p.Key))
                    {
                        var pts = DrawingHelper.OffSetPolyline(GetUnion(pair), MovingSuggestionRadius);

                        if (!DrawingHelper.IsPointInside(start, pts)) continue;

#if DEBUG
                        var poly = new Drawing3DPolyline(pts, MovingSuggestionColor, 2)
                        {
                            IsFill = false,
                        };
                        poly.UpdateOnFrame(this);
                        result = result.Union(poly.To2D(this));
#endif

                        var to = DrawingHelper.GetClosestPoint(start, pts);
                        var line = new Drawing3DHighlightLine(start, to, MovingSuggestionRadius, MovingSuggestionColor, 2);
                        line.UpdateOnFrame(this);
                        result = result.Union(line.To2D(this));
                    }
                    return result;
                }));

                //outLineTasks.Add(Task.Run(() =>
                //{
                //    DrawingHelper.OffSetPolyline(GetUnion(inPoly), -MovingSuggestionRadius);
                //}));
            }

            await Task.WhenAll(tasks.ToArray());

            foreach (var task in tasks)
            {
                relay = relay.Union(task.Result);
            }

            relay = relay.OrderBy(drawing =>
            {
                if (drawing is PolylineDrawing poly)
                {
                    return poly._thickness == 0 ? 0 : 1;
                }
                else
                {
                    return 2;
                }
            });

            await Task.WhenAll(outLineTasks.ToArray());

            foreach (var task in outLineTasks)
            {
                relay = relay.Union(task.Result);
            }

            elements = relay.ToArray();

            lock (_drawing2DLock)
            {
                _drawing2DElements = elements;
            }
        }
        catch (Exception ex)
        {
            PluginLog.Warning(ex, "Something wrong with drawing");
        }

        _started = false;
    }

    private static IEnumerable<Vector3[]> GetUnion(IEnumerable<Drawing3DPolyline> polys)
    {
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

        lock (_drawing3DLock)
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
