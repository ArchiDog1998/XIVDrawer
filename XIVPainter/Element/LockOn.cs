using Dalamud.Game.ClientState.Objects.Types;
using XIVPainter.Vfx;

namespace XIVPainter.Element;
public abstract class LockOnBase : IDisposable
{
    readonly StaticVfx _area;
    readonly ActorVfx _actor;

    protected LockOnBase(GameObject target, float range, string path)
    {
        _area = new(GroundOmenNone.Circle.Omen(), target, new Vector3(range, XIVPainterMain.HeightScale, range))
        {
            DeadTime = DateTime.Now.AddSeconds(5),
        };
        _actor = new(path.LockOn(), target, target)
        {
            DeadTime = DateTime.Now.AddSeconds(5),
        };
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _actor.Dispose();
        _area.Dispose();
    }
}

public class Share2(GameObject target, float range)
    : LockOnBase(target, range, ActorOmen.Share2)
{
}

public class Share4(GameObject target, float range)
    : LockOnBase(target, range, ActorOmen.Share4)
{
}

public class Single1(GameObject target, float range)
    : LockOnBase(target, range, ActorOmen.Single)
{
}