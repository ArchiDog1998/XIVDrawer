using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using KdTree;
using KdTree.Math;

namespace XIVPainter;

internal static class RaycastManager
{
    //const int Compacity = 80 * 80 * 400;

    readonly static KdTree<float, (DateTime addTime, float value)> _rayRelay = new (2, new FloatMath(), AddDuplicateBehavior.Update);

    static readonly Queue<Vector3> _calculatingPts = new ();
    static bool _canAdd = false;

    static readonly object _addingPtsLock = new ();
    static readonly Queue<Vector3> _addingPts = new ();


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
            try
            {
                //Add
                while (TryGetCalPt(out var pt))
                {
                    var p = GetKey(pt);
                    if (!_rayRelay.TryFindValueAt(new float[] { p.X, p.Y }, out var pair) || DateTime.Now - pair.addTime > reCalTimePt)
                    {
                        _calculatingPts.Enqueue(pt);
                    }
                }

                ////Remove
                //if (Service.ClientState != null && Service.ClientState.LocalPlayer != null)
                //{
                //    var loc = Service.ClientState.LocalPlayer.Position;
                //    var pos = GetKey(in loc);
                //    while (_rayRelay.Count > Compacity)
                //    {
                //        var removed = _rayRelay.MaxBy(p => Vector2.Distance(new Vector2(p.Point[0], p.Point[1]), pos));
                //        _rayRelay.RemoveAt(removed.Point);
                //    }
                //}

                //Calculation
                while (!Service.Condition[ConditionFlag.BetweenAreas]
                    && !Service.Condition[ConditionFlag.BetweenAreas51]
                    && _calculatingPts.TryDequeue(out var vector))
                {
                    var key = GetKey(in vector);
                    var value = Raycast(in vector);

                    _rayRelay.Add(new float[] { key.X, key.Y }, (DateTime.Now, value));
                }
            }
            catch (Exception ex)
            {
                Service.Log.Warning(ex, "Failed to calculating the raycast");
            }
            finally
            {
                _isUpdateRun = false;
            }
        });
    }

    static bool TryGetCalPt(out Vector3 pt)
    {
        lock (_addingPtsLock)
        {
            return _addingPts.TryDequeue(out pt);
        }
    }

    static readonly TimeSpan reCalTimePt = TimeSpan.FromSeconds(1);

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

    private static bool GetHeight(in Vector2 xy, out float height)
    {
        height = 0;

        var result = _rayRelay.GetNearestNeighbours(new float[] { xy.X, xy.Y }, 1);

        if (result != null && result.Length > 0)
        {
            var p = result[0].Point;

            if((new Vector2(p[0], p[1]) - xy).LengthSquared() <= 1)
            {
                height = result[0].Value.value;
                return true;
            }
        }
        return false;
    }

    private static Vector2 GetKey(in Vector3 point) 
        => new (float.Round(point.X, 1), float.Round(point.Z, 1));

    static unsafe float Raycast(in Vector3 point)
    {
        int* unknown = stackalloc int[] { 0x4000, 0, 0x4000, 0 };

        RaycastHit hit = default;

        return FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->BGCollisionModule
            ->RaycastEx(&hit, point + Vector3.UnitY * 5, -Vector3.UnitY, 100, 1, unknown) ? hit.Point.Y : float.NaN;
    }
}