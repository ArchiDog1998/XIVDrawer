using Dalamud.Game.ClientState.Objects.Types;

namespace XIVDrawer.Vfx;

/// <summary>
/// 
/// </summary>
public class ActorVfx : BasicDrawing
{
    private bool _shouldRemove;
    private readonly nint _handle;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caster"></param>
    /// <param name="target"></param>
    /// <param name="path"></param>
    public ActorVfx(string path, GameObject caster, GameObject target)
        : this(path, caster.Address, target.Address) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caster"></param>
    /// <param name="target"></param>
    /// <param name="path"></param>
    public ActorVfx(string path, nint caster, nint target)
    {
        _handle = VfxManager.ActorVfxCreate?.Invoke(path, caster, target, -1, (char)0, 0, (char)0) ?? nint.Zero;

#if DEBUG
        Service.Log.Debug($"Created Actor {_handle:x}");
#endif
        _shouldRemove = !path.StartsWith("vfx/lockon/eff/");

        VfxManager.AddedActorVfxStructs.Add(this);
    }

    private protected override void AdditionalDispose()
    {
        if (_shouldRemove)
        {
            VfxManager.ActorVfxRemove?.Invoke(_handle, (char)1);
        }
        _shouldRemove = false;
    }
}
