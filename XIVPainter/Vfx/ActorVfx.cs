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
    public ActorVfx(GameObject caster, GameObject target, string path) : this(caster.Address, target.Address, path) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caster"></param>
    /// <param name="target"></param>
    /// <param name="path"></param>
    public ActorVfx(IntPtr caster, IntPtr target, string path)
    {
        Handle = VfxManager.ActorVfxCreate?.Invoke(path, caster, target, -1, (char)0, 0, (char)0) ?? IntPtr.Zero;
    }

    private protected override void Remove()
    {
        VfxManager.ActorVfxRemove?.Invoke(Handle, (char)1);
    }
}
