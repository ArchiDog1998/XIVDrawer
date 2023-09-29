using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

namespace XIVPainter;

internal static class RaycastManager
{
    const int MaxDistance = 80,
              Compacity = MaxDistance * MaxDistance * 400;
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

    readonly static SortedList<Vector2, (DateTime addTime, float value)> _rayRelay = new (Compacity + 2000, _comparer);

    static readonly Queue<Vector3> _calculatingPts = new ();
    static bool _canAdd = false;

    static readonly object _addingPtsLock = new object();
    static readonly Queue<Vector3> _addingPts = new Queue<Vector3>();


    public static void Enable()
    {
        if(Service.Framework != null)
        {
            Service.Framework.Update -= Update;
            Service.Framework.Update += Update;
        }
    }

    public static void Dispose()
    {
        if (Service.Framework != null)
        {
            Service.Framework.Update -= Update;
        }
    }

    static bool _isUpdateRun = false;
    private static void Update(IFramework framework)
    {
        _canAdd = !_addingPts.Any();

        if (_isUpdateRun) return;
        _isUpdateRun = true;

        Task.Run(() =>
        {
            //Add
            while(TryGetCalPt(out var pt))
            {
                AddCalculatingPts(in pt, 5, 1);
            }

            //Remove
            if (Service.ClientState != null && Service.ClientState.LocalPlayer != null)
            {
                var loc = Service.ClientState.LocalPlayer.Position;
                var pos = GetKey(in loc);
                while (_rayRelay.Count > Compacity)
                {
                    var removed = _rayRelay.MaxBy(p => Vector2.Distance(p.Key, pos));
                    _rayRelay.Remove(removed.Key);
                }
            }

            //Calculation
            while (!Service.Condition[ConditionFlag.BetweenAreas] 
                && !Service.Condition[ConditionFlag.BetweenAreas51]
                && _calculatingPts.TryDequeue(out var vector))
            {
                var key = GetKey(in vector);
                var value = Raycast(in vector);

                _rayRelay[key] = (DateTime.Now, value);
            }

            _isUpdateRun = false;
        });
    }

    static bool TryGetCalPt(out Vector3 pt)
    {
        lock (_addingPtsLock)
        {
            return _addingPts.TryDequeue(out pt);
        }
    }

    static readonly TimeSpan reCalTime = TimeSpan.FromSeconds(10),
                             reCalTimePt = TimeSpan.FromSeconds(1);

    private static void AddCalculatingPts(in Vector3 loc, in float distance, in int maxCount)
    {
        var pt = default(Vector2);

        int count = 0;

        if (!_rayRelay.TryGetValue(pt + GetKey(loc), out var pair) || DateTime.Now - pair.addTime > reCalTimePt)
        {
            count++;
            _calculatingPts.Enqueue(loc + new Vector3(pt.X, 0, pt.Y));
        }
        while (count < maxCount && Vector2.Distance(pt, default) < distance)
        {
            var xAy = pt.X + pt.Y;
            var xSy = pt.X - pt.Y;

            if (xAy >= 0 && xSy >= 0) pt += new Vector2(0, 0.1f);
            else if (xAy > 0 && xSy < 0) pt += new Vector2(-0.1f, 0);
            else if (xAy <= 0 && xSy < 0) pt += new Vector2(0, -0.1f);
            else pt += new Vector2(0.1f, 0);

            if (!_rayRelay.TryGetValue(pt + GetKey(in loc), out pair) || DateTime.Now -  pair.addTime > reCalTime)
            {
                count++;
                _calculatingPts.Enqueue(loc + new Vector3(pt.X, 0, pt.Y));
            }
        }
        if (count == 0)
        {
            _calculatingPts.Enqueue(loc + new Vector3(pt.X, 0, pt.Y));
        }
    }

    public static bool Raycast(in Vector3 point, in float height, out Vector3 territoryPt)
    {
        var xy = GetKey(in point);
        territoryPt = point;

        //Start RayCasting!
        if (_canAdd)
        {
            lock (_addingPtsLock)
            {
                _addingPts.Enqueue(point);
            }
        }

        if (!GetHeight(in xy, out var vector)) vector = territoryPt.Y;
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
    private static bool GetHeight(in Vector2 xy, out float height)
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
            height = _rayRelay.Values[index].value;
            return true;
        }
        return false;
    }

    private static Vector2 GetKey(in Vector3 point) 
        => new Vector2(float.Round(point.X, 1), float.Round(point.Z, 1));

    static unsafe float Raycast(in Vector3 point)
    {
        int* unknown = stackalloc int[] { 0x4000, 0, 0x4000, 0 };

        RaycastHit hit = default;

        return FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->BGCollisionModule
            ->RaycastEx(&hit, point + Vector3.UnitY * 5, -Vector3.UnitY, 100, 1, unknown) ? hit.Point.Y : float.NaN;
    }
}