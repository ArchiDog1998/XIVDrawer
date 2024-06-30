using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using XIVDrawer.Element2D;
using XIVDrawer.ElementSpecial;
using XIVDrawer.Vfx;

namespace XIVDrawer;

/// <summary>
/// The drawer for FFXIV in Dalamud.
/// </summary>
public static class XIVDrawerMain
{
    internal static string _name = "XIV Drawer";

    internal static readonly List<IDrawing> _drawingElements = [];
    internal static IDrawing2D[] _drawingElements2D = [];

    private static WindowSystem? windowSystem;

    #region Config
    /// <summary>
    /// The height scale for the vfx things.
    /// </summary>
    public static float HeightScale { get; set; } = 5;

    /// <summary>
    /// Enable this <seealso cref="XIVDrawerMain"/>
    /// </summary>
    public static bool Enable { get; set; } = true;

    /// <summary>
    /// The length of sample, please don't set this too low!
    /// </summary>
    public static float SampleLength { get; set; } = 1;

    /// <summary>
    /// The time of warning.
    /// </summary>
    public static byte WarningTime { get; set; } = 3;

    /// <summary>
    /// How much alpha value should be changed when warning.
    /// </summary>
    public static float WarningRatio { get; set; } = 0.8f;

    /// <summary>
    /// The ease function type for warning.
    /// </summary>
    public static EaseFuncType WarningType { get; set; } = EaseFuncType.Cubic;

    /// <summary>
    /// To make it faster
    /// </summary>
    public static bool UseTaskToAccelerate { get; set; } = false;

    /// <summary>
    /// The view Padding range.
    /// </summary>
    public static Vector4 ViewPadding { get; set; } = Vector4.One * 50;
    #endregion

    private static bool _inited = false;

    /// <summary>
    /// The way to create this.
    /// </summary>
    /// <param name="pluginInterface"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static void Init(IDalamudPluginInterface pluginInterface, string name)
    {
        if (_inited) return;
        _inited = true;

        _name = name;
        pluginInterface.Create<Service>();
        windowSystem = new WindowSystem(_name);
        windowSystem.AddWindow(new OverlayWindow());

        Service.PluginInterface.UiBuilder.Draw += OnDraw;
        Service.Framework.Update += Update;

        VfxManager.Init();
    }

    private static void OnDraw()
    {
        if (Service.GameGui.GameUiHidden) return;
        windowSystem?.Draw();
    }

    /// <summary>
    /// Don't forget to dispose this!
    /// </summary>
    public static void Dispose()
    {
        if (!_inited) return;
        _inited = false;

        foreach (var item in new List<IDrawing>(_drawingElements))
        {
            item.Dispose();
        }

        Service.PluginInterface.UiBuilder.Draw -= OnDraw;
        Service.Framework.Update -= Update;

        VfxManager.Dispose();
    }

    private static void Update(IFramework framework)
    {
        if (!Enable || Service.ClientState == null || Service.ClientState.LocalPlayer == null) return;

        if (_started) return;
        _started = true;

        Task.Run(UpdateData);
    }

    static bool _started = false;
    private static async void UpdateData()
    {
        try
        {
            List<Task> tasks = [];

            foreach (var ele in new List<IDrawing>(_drawingElements))
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        ele.UpdateOnFrameMain();
                    }
                    catch (Exception ex)
                    {
                        Service.Log.Warning(ex, "Something wrong with " + nameof(IDrawing.UpdateOnFrameMain));
                    }
                }));
            }

            await Task.WhenAll([.. tasks]);

            if (UseTaskToAccelerate)
            {
                _drawingElements2D = await To2DAsync();
            }
        }
        catch (Exception ex)
        {
            Service.Log.Warning(ex, "Something wrong with drawing");
        }

        _started = false;
    }

    internal static async Task<IDrawing2D[]> To2DAsync()
    {
        List<Task<IEnumerable<IDrawing2D>>> drawing2Ds = [];

        if (_drawingElements != null)
        {
            drawing2Ds.AddRange(_drawingElements.Select(item => Task.Run(() =>
            {
                return item.To2DMain();
            })));
        }

        await Task.WhenAll([.. drawing2Ds]);
        return drawing2Ds.SelectMany(i => i.Result).ToArray();
    }

    #region Trasform
    /// <summary>
    /// Make the world point project into the screen.
    /// </summary>
    /// <param name="pts"></param>
    /// <param name="isClosed">Is pts closed</param>
    /// <param name="inScreen">Must be draw in the screen.</param>
    /// <returns></returns>
    public static Vector2[] GetPtsOnScreen(IEnumerable<Vector3> pts, bool isClosed, bool inScreen)
    {
        var cameraPts = DivideCurve(pts, SampleLength, isClosed)
            .Select(WorldToCamera).ToArray();
        var changedPts = ChangePtsBehindCamera(cameraPts);

        return changedPts.Select(p => CameraToScreen(p, inScreen)).ToArray();
    }

    static IEnumerable<Vector3> DivideCurve(IEnumerable<Vector3> worldPts, float length, bool isClosed)
    {
        if (worldPts.Count() < 2 || length <= 0.01f) return worldPts;

        IEnumerable<Vector3> pts = [];

        DrawingExtensions.SegmentAction(worldPts, (a, b) =>
        {
            pts = pts.Union(DashPoints(a, b, length));
        }, isClosed);

        if (!isClosed) pts = pts.Append(worldPts.Last());

        return pts;
    }

    static Vector3[] DashPoints(Vector3 previous, Vector3 next, float length)
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

    static IEnumerable<Vector3> ChangePtsBehindCamera(Vector3[] cameraPts)
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

            if (changedPts.Count > 0 && Vector3.Distance(pt1, changedPts[^1]) > 0.001f)
            {
                changedPts.Add(pt1);
            }

            changedPts.Add(pt2);
        }

        return changedPts.Where(p => p.Z > 0);
    }

    const float PLANE_Z = 0.001f;
    static void GetPointOnPlane(Vector3 front, ref Vector3 back)
    {
        if (front.Z <= 0) return;
        if (back.Z > 0) return;

        var ratio = (PLANE_Z - back.Z) / (front.Z - back.Z);
        back.X = (front.X - back.X) * ratio + back.X;
        back.Y = (front.Y - back.Y) * ratio + back.Y;
        back.Z = PLANE_Z;
    }

    static unsafe Vector3 WorldToCamera(Vector3 worldPos)
    {
        var camera = CameraManager.Instance()->CurrentCamera;
        return Vector3.Transform(worldPos, camera->ViewMatrix * camera->RenderCamera->ProjectionMatrix);
    }

    static unsafe Vector2 CameraToScreen(Vector3 cameraPos, bool inScreen)
    {
        var screenPos = new Vector2(cameraPos.X / MathF.Abs(cameraPos.Z), cameraPos.Y / MathF.Abs(cameraPos.Z));
        var windowPos = ImGuiHelpers.MainViewport.Pos;

        var device = Device.Instance();
        float width = device->Width;
        float height = device->Height;

        screenPos.X = 0.5f * width * (screenPos.X + 1f) + windowPos.X;
        screenPos.Y = 0.5f * height * (1f - screenPos.Y) + windowPos.Y;

        if (inScreen)
        {
            screenPos = GetPtInRect(windowPos, new Vector2(width, height), screenPos);
        }
        return screenPos;
    }

    /// <summary>
    /// Make the <paramref name="pt"/> into the Rectangle.
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
        if (rec.X > 0)
        {
            pt.X /= rec.X;
        }
        else
        {
            pt.X = 0;
        }

        if (rec.Y > 0)
        {
            pt.Y /= rec.Y;
        }
        else
        {
            pt.Y = 0;
        }

        return GetPtIn1Rect(pt) * rec;
    }

    private static Vector2 GetPtIn1Rect(Vector2 pt)
    {
        if (pt.X is >= -1 and <= 1 && pt.Y is >= -1 and <= 1) return pt;

        var rate = Math.Max(Math.Abs(pt.X), Math.Abs(pt.Y));
        if (rate == 0) return pt;

        return new Vector2(pt.X / rate, pt.Y / rate);
    }
    #endregion

    internal static Vector3[] SectorPlots(Vector3 center, float radius, float rotation, float round)
    {
        var circleSegment = (int)(MathF.Tau * radius / SampleLength);
        return SectorPlots(center, radius, rotation, round, circleSegment);
    }

    internal static Vector3[] SectorPlots(Vector3 center, float radius, float rotation, float round, int circleSegment)
    {
        if (radius <= 0) return [];
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

#if DEBUG
    /// <summary>
    /// Show off the vfx for test.
    /// </summary>
    /// <returns></returns>
    public static async Task ShowOff()
    {
        if (Service.ClientState.LocalPlayer is not IPlayerCharacter player) return;

        await ShowLockOnElements(player);
        await Task.Delay(3000);

        await ShowChannelingElements(player);
        await Task.Delay(3000);

        await ShowGroundHostile(player);
        await Task.Delay(3000);

        await ShowGroundFriendly(player);
        await Task.Delay(3000);
    }

    private static async Task ShowGroundHostile(IPlayerCharacter player)
    {
        foreach (var item in typeof(GroundOmenHostile).GetRuntimeFields())
        {
            if (item.GetValue(null) is not string str) continue;

            using var i = new StaticVfx(str.Omen(), player, new Vector3(3, HeightScale, 3));
            await MessageDelay(item.Name);
        }

        ShowQuest("That's all Hostile Omen!");
    }

    private static async Task ShowGroundFriendly(IPlayerCharacter player)
    {
        foreach (var item in typeof(GroundOmenNone).GetRuntimeFields()
            .Concat(typeof(GroundOmenFriendly).GetRuntimeFields()))
        {
            if (item.GetValue(null) is not string str) continue;

            using var i = new StaticVfx(str.Omen(), player, new Vector3(3, HeightScale, 3));
            await MessageDelay(item.Name);
        }

        ShowQuest("That's all Friendly Omen!");
    }

    private static async Task ShowLockOnElements(IPlayerCharacter player)
    {
        foreach (var item in typeof(LockOnOmen).GetRuntimeFields())
        {
            if (item.GetValue(null) is not string str) continue;

            using var i = new ActorVfx(str.LockOn(), player, player);
            await MessageDelay(item.Name);
        }

        ShowQuest("That's all lockOn Element!");
    }

    private static async Task ShowChannelingElements(IPlayerCharacter player)
    {
        foreach (var item in typeof(ChannelingOmen).GetRuntimeFields())
        {
            if (item.GetValue(null) is not string str) continue;

            using var i = new ActorVfx(str.Channeling(), player, player);
            await MessageDelay(item.Name);
        }

        ShowQuest("That's all Channeling Element!");
    }

    private static async Task MessageDelay(string name)
    {
        Service.Toasts.ShowError(name);
        await Task.Delay(5000);
    }

    private static void ShowQuest(string str)
    {
        Service.Toasts.ShowQuest(str, new Dalamud.Game.Gui.Toast.QuestToastOptions()
        {
            DisplayCheckmark = true,
            PlaySound = true,
        });
    }
#endif
}
