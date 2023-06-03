using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

namespace XIVPainter;

internal static class RaycastManager
{
    class Vector2Comparer : IComparer<Vector2>
    {
        public int Compare(Vector2 x, Vector2 y)
        {
            var xCom = x.X.CompareTo(y.X);
            if(xCom != 0) return xCom;
            return y.Y.CompareTo(x.Y);
        }
    }

    static readonly Vector2Comparer _comparer = new Vector2Comparer();

    static readonly object _rayRelayLock = new object();
    static readonly SortedList<Vector2, float> _rayRelay = new (_comparer);

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

        var vector = GetHeight(xy);
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
    private static float GetHeight(Vector2 xy)
    {
        lock (_rayRelayLock)
        {
            _keyInfo ??= _rayRelay.GetType().GetRuntimeFields().First(f => f.Name == "keys");
            var index = Array.BinarySearch((Vector2[])_keyInfo.GetValue(_rayRelay), xy, _comparer);
            if (index < 0) index = -1 - index;
            return _rayRelay.Values[index % _rayRelay.Count];
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

        return BGCollisionModule.Raycast2(point + Vector3.UnitY * 10, -Vector3.UnitY, 20, &hit, unknown) ? hit.Point.Y : float.NaN;
    }
}