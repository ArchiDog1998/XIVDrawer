using Clipper2Lib;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

namespace XIVPainter;

/// <summary>
/// A static class for drawing extension.
/// </summary>
public static class DrawingExtensions
{
    /// <summary>
    /// Normalize the <seealso cref="Vector2"/> safely.
    /// </summary>
    /// <param name="vec">the vector to normalize</param>
    /// <returns>Is succeed.</returns>
    public static bool Normalize(ref this Vector2 vec)
    {
        var length = vec.Length();
        if(length == 0) return false;
        vec /= length;
        return true;
    }

    /// <summary>
    /// Can the point be seen by the active camera.
    /// </summary>
    /// <param name="point">testing point in world.</param>
    /// <returns>can be seen.</returns>
    public unsafe static bool CanSee(in this Vector3 point)
    {
        var camera = (Vector3)CameraManager.Instance()->CurrentCamera->Object.Position;

        var vec = point - camera;
        var dis = vec.Length() - 0.1f;

        int* unknown = stackalloc int[] { 0x4000, 0, 0x4000, 0 };

        RaycastHit hit = default;

        return !FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->BGCollisionModule
            ->RaycastEx(&hit, camera, vec, dis, 1, unknown);
    }

    /// <summary>
    /// Is the point inside the polygon
    /// </summary>
    /// <param name="pt">testing point</param>
    /// <param name="pts">polygon</param>
    /// <returns>is Inside</returns>
    public static bool IsPointInside(in Vector3 pt, IEnumerable<IEnumerable<Vector3>> pts)
        => IsPointInside(new Vector2(pt.X, pt.Z), pts.Select(lt => lt.Select(p => new Vector2(p.X, p.Z))));

    /// <summary>
    /// Is the point inside the polygon
    /// </summary>
    /// <param name="pt">testing point</param>
    /// <param name="pts">polygon</param>
    /// <returns>is Inside</returns>
    public static bool IsPointInside(Vector2 pt, IEnumerable<IEnumerable<Vector2>> pts)
    {
        var count = 0;

        foreach (var partPts in pts)
        {
            SegmentAction(partPts, (a, b) =>
            {
                if ((pt.Y < a.Y) != (pt.Y < b.Y) &&
                    pt.X < a.X + (pt.Y - a.Y) / (b.Y - a.Y) * (b.X - a.X))
                    count++;
            });
        }

        return count % 2 == 1;
    }

    /// <summary>
    /// Get the closest point to the polygon.
    /// </summary>
    /// <param name="pt">testing point</param>
    /// <param name="pts">polygon</param>
    /// <returns>closest point</returns>
    public static Vector3 GetClosestPoint(Vector3 pt, IEnumerable<IEnumerable<Vector3>> pts)
    {
        float minDis = float.MaxValue;
        Vector3 result = default;

        foreach (var partPts in pts)
        {
            SegmentAction(partPts, (a, b) =>
            {
                var dis = PointSegment(pt, a, b, out var res);
                if(dis < minDis)
                {
                    minDis = dis;
                    result = res;
                }
            });
        }

        return result;
    }

    static float PointSegment(in Vector3 p, in Vector3 a, in Vector3 b, out Vector3 cp)
    {
        Vector3 ab = b - a;
        Vector3 ap = p - a;

        float proj = Vector3.Dot(ap, ab);
        float abLenSq = ab.LengthSquared();
        float d = proj / abLenSq;

        cp = d switch
        {
            <= 0 => a,
            >= 1 => b,
            _ => a + ab * d,
        };
        return Vector3.Distance(p, cp);
    }

    /// <summary>
    /// Offset the polygon
    /// </summary>
    /// <param name="pts">polygon</param>
    /// <param name="offset">distance to offset</param>
    /// <returns>offseted polygon</returns>
    public static IEnumerable<Vector3[]> OffSetPolyline(in IEnumerable<Vector3[]> pts, float offset)
        => pts?.SelectMany(p => OffSetPolyline(p, offset));

    /// <summary>
    /// Offset the polygon
    /// </summary>
    /// <param name="pts">polygon</param>
    /// <param name="offset">distance to offset</param>
    /// <returns>offseted polygon</returns>
    public static IEnumerable<Vector3[]> OffSetPolyline(in Vector3[] pts, float offset)
    {
        if (pts.Length < 3) return new Vector3[][] { pts };

        if (!IsOrdered(pts.Select(p => new Vector2(p.X, p.Z)).ToArray()))
            offset = -offset;

        var path = Vec3ToPathD(pts);
        var result = Clipper.InflatePaths(new PathsD(new PathD[] { path }), offset, JoinType.Round, EndType.Polygon);

        float height = 0;
        foreach (var p in pts)
        {
            height += p.Y;
        }
        height /= pts.Length;

        return result.Select(p => PathDToVec3(p, height));
    }

    internal static PathsD Vec3ToPathsD(IEnumerable<IEnumerable<Vector3>> pts)
        => new PathsD(pts.Select(Vec3ToPathD));

    internal static PathD Vec3ToPathD(IEnumerable<Vector3> pts)
    {
        if(pts == null) return null;
        return new PathD(pts.Select(p => new PointD(p.X, p.Z)));
    }
    internal static IEnumerable<Vector3[]> PathsDToVec3(PathsD path, float height)
    => path?.Select(p => PathDToVec3(p, height));

    internal static Vector3[] PathDToVec3(PathD path, in float height)
    {
        var result = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            var p = path[i];
            result[i] = new Vector3((float)p.x, height, (float)p.y);
        }
        return result;
    }

    internal static void SegmentAction<T>(IEnumerable<T> pts, Action<T, T> pairAction, in bool closed = true)
    {
        if (pairAction == null) return;
        if(pts == null || !pts.Any()) return;

        T prePt = default;
        bool isFirst = true;
        foreach (var pt in pts)
        {
            if (isFirst)
            {
                isFirst = false;
                prePt = pt;
                continue;
            }
            pairAction(prePt, pt);
            prePt = pt;
        }

        if(closed) pairAction(prePt, pts.First());
    }

    /// <summary>
    /// ConvexDecomposition
    /// </summary>
    /// <param name="points">concave polygon</param>
    /// <returns>convex polygons</returns>
    public static IEnumerable<Vector2[]> ConvexPoints(in Vector2[] points)
    {
        if (points == null || points.Length < 3) 
            return new Vector2[][] { points };

        var tess = new LibTessDotNet.Tess();
        tess.AddContour(points.Select(p => new LibTessDotNet.ContourVertex(new LibTessDotNet.Vec3(p.X, p.Y, 0))).ToArray(), LibTessDotNet.ContourOrientation.CounterClockwise);

        tess.Tessellate();

        int numTriangles = tess.ElementCount;
        var result = new Vector2[numTriangles][];
        for (int i = 0; i < numTriangles; i++)
        {
            var v0 = tess.Vertices[tess.Elements[i * 3]].Position;
            var v1 = tess.Vertices[tess.Elements[i * 3 + 1]].Position;
            var v2 = tess.Vertices[tess.Elements[i * 3 + 2]].Position;
            result[i] = new Vector2[]
            {
                new Vector2(v0.X, v0.Y),
                new Vector2(v1.X, v1.Y),
                new Vector2(v2.X, v2.Y),
            };
        }
        return result;
    }

    static bool IsOrdered(in Vector2[] points)
    {
        int index = 0;
        float leftBottom = float.MaxValue;
        for (int i = 0; i < points.Length; i++)
        {
            var pt = points[i];
            var value = pt.X + pt.Y;
            if (value < leftBottom)
            {
                index = i;
                leftBottom = value;
            }
        }

        return PointCross(points, index, out _, out _) <= 0.01f;
    }

    private static float PointCross(in Vector2[] pts, in int index, out Vector2 vec1, out Vector2 vec2)
    {
        var length = pts.Length;
        var prePt = pts[(index - 1 + length) % length];
        var midPt = pts[index];
        var nextPt = pts[(index + 1) % length];

        vec1 = midPt - prePt;
        vec2 = nextPt - midPt;

        vec1.Normalize();
        vec2.Normalize();

        return Vector3.Cross(new Vector3(vec1.X, vec1.Y, 0), new Vector3(vec2.X, vec2.Y, 0)).Z;
    }

    /// <summary>
    /// Get the ease function by <seealso cref="EaseFuncType"/>
    /// </summary>
    /// <param name="inType">input type</param>
    /// <param name="outType">output type</param>
    /// <returns>function</returns>
    public static Func<double, double> EaseFuncRemap(EaseFuncType inType, EaseFuncType outType)
    {
        if (inType == EaseFuncType.None)
        {
            if (outType == EaseFuncType.None)
            {
                return (x) => x;
            }
            else
            {
                return (x) => FindOutFuction(outType)(x);
            }
        }
        else
        {
            if (outType == EaseFuncType.None)
            {
                return (x) => 1 - FindOutFuction(inType)(1 - x);
            }
            else
            {
                return (x) => x < 0.5 ? (1 - FindOutFuction(inType)(1 - (2 * x))) / 2
                                      : (1 + FindOutFuction(outType)((2 * x) - 1)) / 2;
            }
        }
    }

    const double c1 = 1.70158;
    const double c3 = c1 + 1;
    const double c4 = (2 * Math.PI) / 3;
    const double n1 = 7.5625;
    const double d1 = 2.75;

    static Func<double, double> FindOutFuction(in EaseFuncType outType) => outType switch 
    {
        EaseFuncType.Sine => x => Math.Sin(x * Math.PI / 2),
        EaseFuncType.Quad => x => 1 - ((1 - x) * (1 - x)),
        EaseFuncType.Cubic => x => 1 - Math.Pow(1 - x, 3),
        EaseFuncType.Quart => x => 1 - Math.Pow(1 - x, 4),
        EaseFuncType.Quint => x => 1 - Math.Pow(1 - x, 5),
        EaseFuncType.Expo => x => x == 1 ? 1 : 1 - Math.Pow(2, -10 * x),
        EaseFuncType.Circ => x => Math.Sqrt(1 - Math.Pow(x - 1, 2)),
        EaseFuncType.Back => x => 1 + (c3 * Math.Pow(x - 1, 3)) + (c1 * Math.Pow(x - 1, 2)),
        EaseFuncType.Elastic => x => x == 0 ? 0 : x == 1 ? 1 : (Math.Pow(2, -10 * x) * Math.Sin(((x * 10) - 0.75) * c4)) + 1,
        EaseFuncType.Bounce => x =>
        {
            if (x < 1 / d1)
            {
                return n1 * x * x;
            }
            else if (x < 2 / d1)
            {
                return (n1 * (x -= 1.5 / d1) * x) + 0.75;
            }
            else if (x < 2.5 / d1)
            {
                return (n1 * (x -= 2.25 / d1) * x) + 0.9375;
            }
            else
            {
                return (n1 * (x -= 2.625 / d1) * x) + 0.984375;
            }
        } ,
        _ => x => x,
    };
}

/// <summary>
/// Ease function type.
/// </summary>
public enum EaseFuncType
{
    /// <summary>
    /// Line
    /// </summary>
    None,

    /// <summary>
    /// <see href="https://easings.net#easeInSine">In</see>,
    /// <see href="https://easings.net#easeOutSine">Out</see>
    /// </summary>
    Sine,

    /// <summary>
    /// <see href="https://easings.net#easeInQuad">In</see>,
    /// <see href="https://easings.net#easeOutQuad">Out</see>
    /// </summary>
    Quad,

    /// <summary>
    /// <see href="https://easings.net#easeInCubic">In</see>,
    /// <see href="https://easings.net#easeOutCubic">Out</see>
    /// </summary>
    Cubic,

    /// <summary>
    /// <see href="https://easings.net#easeInQuart">In</see>,
    /// <see href="https://easings.net#easeOutQuart">Out</see>
    /// </summary>
    Quart,

    /// <summary>
    /// <see href="https://easings.net#easeInQuint">In</see>,
    /// <see href="https://easings.net#easeOutQuint">Out</see>
    /// </summary>
    Quint,

    /// <summary>
    /// <see href="https://easings.net#easeInExpo">In</see>,
    /// <see href="https://easings.net#easeOutExpo">Out</see>
    /// </summary>
    Expo,

    /// <summary>
    /// <see href="https://easings.net#easeInCirc">In</see>,
    /// <see href="https://easings.net#easeOutCirc">Out</see>
    /// </summary>
    Circ,

    /// <summary>
    /// <see href="https://easings.net#easeInBack">In</see>,
    /// <see href="https://easings.net#easeOutBack">Out</see>
    /// </summary>
    Back,

    /// <summary>
    /// <see href="https://easings.net#easeInElastic">In</see>,
    /// <see href="https://easings.net#easeOutElastic">Out</see>
    /// </summary>
    Elastic,

    /// <summary>
    /// <see href="https://easings.net#easeInBounce">In</see>,
    /// <see href="https://easings.net#easeOutBounce">Out</see>
    /// </summary>
    Bounce,
}