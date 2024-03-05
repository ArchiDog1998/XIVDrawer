using Dalamud.Game.ClientState.Objects.Types;

namespace XIVPainter.Vfx;

/// <summary>
/// 
/// </summary>
public class ActorVfx : BaseVfx
{
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
    public ActorVfx(string path, IntPtr caster, IntPtr target)
    {
        Handle = VfxManager.ActorVfxCreate?.Invoke(path, caster, target, -1, (char)0, 0, (char)0) ?? IntPtr.Zero;

#if DEBUG
        Service.Log.Debug($"Created Actor {Handle:x}");
#endif
    }

    private protected override void Remove()
    {
        VfxManager.ActorVfxRemoveHook?.Original.Invoke(Handle, (char)1);
    }
}
