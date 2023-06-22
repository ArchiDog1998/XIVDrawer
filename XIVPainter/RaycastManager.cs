using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using System.Drawing;

namespace XIVPainter;

internal static class RaycastManager
{
    const int MaxDistance = 80;
    const int compacity = MaxDistance * MaxDistance * 400;
    class Vector2Comparer : IComparer<Vector2>
    {
        public int Compare(Vector2 x, Vector2 y)
        {
            var xX = (int)x.X;
            var xY = (int)x.Y;

            var yX = (int)y.X;
            var yY = (int)y.Y;

            int com;

            //com = (xX / 10).CompareTo(yX / 10);
            //if (com != 0) return com;
            //com = (xY / 10).CompareTo(yY / 10);
            //if (com != 0) return com;

            com = xX.CompareTo(yX);
            if (com != 0) return com;
            com = xY.CompareTo(yY);
            if (com != 0) return com;

            com = x.X.CompareTo(y.X);
            if(com != 0) return com;
            return x.Y.CompareTo(y.Y);
        }
    }

    static readonly Vector2Comparer _comparer = new Vector2Comparer();

    readonly static SortedList<Vector2, float> _rayRelay = new (compacity + 2000, _comparer);

    static readonly object _calculatingPtsLock = new object();
    static readonly Queue<Vector3> _calculatingPts = new ();
    static bool _canAdd = false;

    public static void Enable()
    {
        if(Service.Framework != null)
        {
            Service.Framework.Update += Update;
        }
    }

    public static void Dispose()
    {

        if (Service.Framework != null)
            Service.Framework.Update -= Update;
    }

    static bool _isUpdateRun = false;
    private static void Update(Framework framework)
    {
        if (_isUpdateRun) return;
        _isUpdateRun = true;
        Task.Run(() =>
        {
            _canAdd = !_calculatingPts.Any();

            if (Service.ClientState != null && Service.ClientState.LocalPlayer != null)
            {
                var loc = Service.ClientState.LocalPlayer.Position;
                var pos = GetKey(loc);
                while (_rayRelay.Count > compacity)
                {
                    var removed = _rayRelay.MaxBy(p => Vector2.Distance(p.Key, pos));
                    _rayRelay.Remove(removed.Key);
                }
            }

            _isUpdateRun = false;
        });
    }

    private static void AddCalculatingPts(Vector3 loc, float distance, int maxCount)
    {
        var pt = default(Vector2);

        int count = 0;

        if (!_rayRelay.ContainsKey(pt + GetKey(loc)))
        {
            count++;
            lock (_calculatingPtsLock)
            {
                _calculatingPts.Enqueue(loc + new Vector3(pt.X, 0, pt.Y));
            }
        }
        while (count < maxCount && Vector2.Distance(pt, default) < distance)
        {
            var xAy = pt.X + pt.Y;
            var xSy = pt.X - pt.Y;

            if (xAy >= 0 && xSy >= 0) pt += new Vector2(0, 0.1f);
            else if (xAy > 0 && xSy < 0) pt += new Vector2(-0.1f, 0);
            else if (xAy <= 0 && xSy < 0) pt += new Vector2(0, -0.1f);
            else pt += new Vector2(0.1f, 0);

            if (!_rayRelay.ContainsKey(pt + GetKey(loc)))
            {
                count++;
                lock (_calculatingPtsLock)
                {
                    _calculatingPts.Enqueue(loc + new Vector3(pt.X, 0, pt.Y));
                }
            }
        }
        if (count == 0)
        {
            lock (_calculatingPtsLock)
            {
                _calculatingPts.Enqueue(loc + new Vector3(pt.X, 0, pt.Y));
            }
        }

        RunRaycast();
    }

    public static bool Raycast(Vector3 point, float height, out Vector3 territoryPt)
    {
        var xy = GetKey(point);
        territoryPt = point;

        //Start RayCasting!
        if (_canAdd)
        {
            Task.Run(() =>
            {
                AddCalculatingPts(point, 5, 1);
            });
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
        height = 0;

        if (_rayRelay.Count > 0)
        {
            _keyInfo ??= _rayRelay.GetType().GetRuntimeFields().First(f => f.Name == "keys");
            var keys = (Vector2[])_keyInfo.GetValue(_rayRelay);
            var index = Array.BinarySearch(keys, 0, _rayRelay.Count, xy, _comparer);
            if (index < 0) index = -1 - index;
            index %= _rayRelay.Count;

            if (Vector2.Distance(keys[index], xy) > 3) return false;
            height = _rayRelay.Values[index];
            return true;
        }
        return false;
    }

    private static Vector2 GetKey(Vector3 point) 
        => new Vector2(float.Round(point.X, 1), float.Round(point.Z, 1));

    static bool _isRaycastRun = false;
    static void RunRaycast()
    {
        if(_isRaycastRun) return;
        _isRaycastRun = true;

        Task.Run(() =>
        {
            while (TryGetCalPt(out var vector))
            {
                var key = GetKey(vector);
                var value = Raycast(vector);

                _rayRelay[key] = value;
            }
            _isRaycastRun = false;
        });
    }

    static bool TryGetCalPt(out Vector3 pt)
    {
        lock (_calculatingPtsLock)
        {
            return _calculatingPts.TryDequeue(out pt);
        }
    }

    static unsafe float Raycast(Vector3 point)
    {
        int* unknown = stackalloc int[] { 0x4000, 0, 0x4000, 0 };

        RaycastHit hit = default;

        return FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->BGCollisionModule
            ->RaycastEx(&hit, point + Vector3.UnitY * 5, -Vector3.UnitY, 100, 1, unknown) ? hit.Point.Y : float.NaN;
    }
}