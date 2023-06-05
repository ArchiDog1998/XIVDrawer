using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

namespace XIVPainter;

#if DEBUG
internal class RaycastManager
#else
internal static class RaycastManager
#endif
{
    class Vector2Comparer : IComparer<Vector2>
    {
        public int Compare(Vector2 x, Vector2 y)
        {
            var xX = (int)x.X;
            var xY = (int)x.Y;

            var yX = (int)y.X;
            var yY = (int)y.Y;

            var xCom = (xX / 10).CompareTo(yX / 10);
            if (xCom != 0) return xCom;
            xCom = (xY / 10).CompareTo(yY / 10);
            if (xCom != 0) return xCom;

            xCom = xX.CompareTo(yX);
            if (xCom != 0) return xCom;
            xCom = xY.CompareTo(yY);
            if (xCom != 0) return xCom;

            xCom = x.X.CompareTo(y.X);
            if(xCom != 0) return xCom;
            return x.Y.CompareTo(y.Y);
        }
    }

    static readonly Vector2Comparer _comparer = new Vector2Comparer();

    static readonly object _rayRelayLock = new object();
    static readonly SortedList<Vector2, float> _rayRelay = new (_comparer);

    static readonly object _calculatingPtsLock = new object();
    static readonly Queue<Vector3> _calculatingPts = new ();
    static bool _canAdd = false;


#if DEBUG
    unsafe delegate bool RayCastDelegate(Vector3 origin, Vector3 direction, float maxDistance, RaycastHit* hitInfo, int* flags);
    unsafe delegate bool RayCastExDelegate(RaycastHit* hitInfo, Vector3 origin, Vector3 direction, float maxDistance, int layerMask, int* flags);

    [Signature("E8 ?? ?? ?? ?? 44 0F B6 F0 84 C0 74 ?? 40 38 BD", DetourName = nameof(RayCastH))]
    static Hook<RayCastDelegate> RayCastHook = null;

    [Signature("48 83 EC 48 48 8B 05 ?? ?? ?? ?? 4D 8B D1", DetourName = nameof(RayCastH2))]
    static Hook<RayCastDelegate> RayCastHook2 = null;

    [Signature("E8 ?? ?? ?? ?? 84 C0 41 0F B6 D6", DetourName = nameof(RayCastEx))]
    static Hook<RayCastExDelegate> RayCastExHook = null;

    static unsafe bool RayCastH(Vector3 origin, Vector3 direction, float maxDistance, RaycastHit* hitInfo, int* flags)
    {
        PluginLog.Information($"RayCast1: {GetArray(flags)}");
        return RayCastHook.Original(origin, direction, maxDistance, hitInfo, flags);
    }

    static unsafe bool RayCastH2(Vector3 origin, Vector3 direction, float maxDistance, RaycastHit* hitInfo, int* flags)
    {
        PluginLog.Information($"RayCast2: {GetArray(flags)}");
        return RayCastHook2.Original(origin, direction, maxDistance, hitInfo, flags);
    }

    static unsafe bool RayCastEx(RaycastHit* hitInfo, Vector3 origin, Vector3 direction, float maxDistance, int layerMask, int* flags)
    {
        PluginLog.Information($"RayCastEx: {GetArray(flags)}; {layerMask}");
        return RayCastExHook.Original(hitInfo, origin, direction, maxDistance, layerMask, flags);
    }

    unsafe static string GetArray(int* flags)
    {
        int count = 4;
        var result = "";
        for (int i = 0; i < count; i++)
        {
            result += flags[i].ToString() + ", ";
        }
        return result;
    }
#endif
    public static void Enable()
    {
        if(XIVPainter._clientState != null)
        {
            XIVPainter._clientState.TerritoryChanged += TerritoryChanged;
        }
        else
        {
            PluginLog.Warning("Failed to find ClientState!");
        }

        if(XIVPainter._framework != null)
        {
            XIVPainter._framework.Update += Update;
        }

#if DEBUG
        SignatureHelper.Initialise(new RaycastManager());
        RayCastHook?.Enable();
        RayCastHook2?.Enable();
        RayCastExHook?.Enable();
#endif
    }

    public static void Dispose()
    {
        if (XIVPainter._clientState != null) 
            XIVPainter._clientState.TerritoryChanged -= TerritoryChanged;

        if (XIVPainter._framework != null)
            XIVPainter._framework.Update -= Update;

#if DEBUG
        RayCastHook?.Dispose();
        RayCastHook2?.Dispose();
        RayCastExHook?.Dispose();
#endif
    }

    private static void Update(Framework framework)
    {
        _canAdd = !_calculatingPts.Any();
    }

    private static void TerritoryChanged(object sender, ushort e)
    {
        lock (_rayRelayLock)
        {
            _rayRelay.Clear();
        }
    }

    public static bool Raycast(Vector3 point, float height, out Vector3 territoryPt)
    {
        var xy = GetKey(point);
        territoryPt = point;

        //Start RayCasting!
        if (_canAdd)
        {
            lock (_calculatingPtsLock)
            {
                _calculatingPts.Enqueue(point);
            }
            RunRaycast();
        }

        if (!GetHeight(xy, out var vector)) vector = territoryPt.Y;
        if (float.IsNaN(vector))
        {
            return false;
        }
        else
        {
            territoryPt.Y = vector;
            territoryPt.Y = Math.Max(territoryPt.Y, point.Y - height);
            territoryPt.Y = Math.Min(territoryPt.Y, point.Y + height);
            return true;
        }
    }

    static FieldInfo _keyInfo;
    private static bool GetHeight(Vector2 xy, out float height)
    {
        lock (_rayRelayLock)
        {
            if(_rayRelay.Count > 0)
            {
                _keyInfo ??= _rayRelay.GetType().GetRuntimeFields().First(f => f.Name == "keys");
                var index = Array.BinarySearch((Vector2[])_keyInfo.GetValue(_rayRelay), xy, _comparer);
                if (index < 0) index = -1 - index;
                height = _rayRelay.Values[index % _rayRelay.Count];
                return true;
            }
            height = 0;
            return false;
        }
    }

    private static Vector2 GetKey(Vector3 point) 
        => new Vector2(float.Round(point.X, 1), float.Round(point.Z, 1));

    static bool _isRun = false;
    static void RunRaycast()
    {
        if(_isRun) return;
        _isRun = true;

        Task.Run(() =>
        {
            while(Dequeue(out var vector))
            {
                var key = GetKey(vector);
                var value = Raycast(vector);

                lock (_rayRelayLock)
                {
                    _rayRelay[key] = value;
                }
            }
            _isRun = false;
        });
    }

    static bool Dequeue(out Vector3 vector)
    {
        lock (_calculatingPtsLock)
        {
            return _calculatingPts.TryDequeue(out vector);
        }
    }

    static unsafe float Raycast(Vector3 point)
    {
        int* unknown = stackalloc int[3] { 16384, 16384, 0 };

        RaycastHit hit = default;

        return BGCollisionModule.Raycast2(point + Vector3.UnitY * 5, -Vector3.UnitY, 100, &hit, unknown) ? hit.Point.Y : float.NaN;
    }
}