using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

namespace XIVPainter;

internal static class RaycastManager
{
    static readonly object _rayRelayLock = new object();
    static readonly Dictionary<Vector2, HitResult> _rayRelay = new ();

    static readonly object _calculatingPtsLock = new object();
    static readonly Queue<Vector3> _calculatingPts = new ();
    static bool _canAdd = false;

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
    }

    public static void Dispose()
    {
        if (XIVPainter._clientState != null) 
            XIVPainter._clientState.TerritoryChanged -= TerritoryChanged;

        if (XIVPainter._framework != null)
            XIVPainter._framework.Update -= Update;
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

        lock (_rayRelayLock)
        {
            if (_rayRelay.TryGetValue(xy, out var vector))
            {
                territoryPt = vector.Point;
                territoryPt.Y = Math.Max(territoryPt.Y, point.Y - height);
                territoryPt.Y = Math.Min(territoryPt.Y, point.Y + height);
                return vector.HasValue;
            }
        }

        //Start RayCasting!
        if (_canAdd)
        {
            lock (_calculatingPtsLock)
            {
                _calculatingPts.Enqueue(point);
            }
            RunRaycast();
        }
        territoryPt = point;
        return true;
    }

    private static Vector2 GetKey(Vector3 point) 
        => new Vector2(float.Round(point.X, 2), float.Round(point.Z, 2));

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

    static unsafe HitResult Raycast(Vector3 point)
    {
        int* unknown = stackalloc int[3] { 16384, 16384, 0 };

        HitResult result = default;
        RaycastHit hit = default;

        result.HasValue = BGCollisionModule.Raycast2(point + Vector3.UnitY * 10, -Vector3.UnitY, 20, &hit, unknown);
        result.Point = hit.Point;
        return result;
    }

    struct HitResult
    {
        public bool HasValue;
        public Vector3 Point;
    }
}