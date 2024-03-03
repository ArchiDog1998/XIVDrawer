using Dalamud.Game.ClientState.Objects.Types;

namespace XIVPainter.Vfx;

/// <summary>
/// 
/// </summary>
public class StaticVfx : BaseVfx
{
    public GameObject? Owner { get; set; }

    public GameObject? Target { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="owner"></param>
    /// <param name="scale"></param>
    public StaticVfx(VfxString path, GameObject owner, Vector3 scale)
        : this(path,owner.Position, owner.Rotation, scale)
    {
        Owner = owner;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="scale"></param>
    public unsafe StaticVfx(VfxString path, Vector3 position, float rotation, Vector3 scale)
        :this(path, position, new Vector3(0, 0, rotation), scale)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="scale"></param>
    public unsafe StaticVfx(VfxString path, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Handle = VfxManager.StaticVfxCreate?.Invoke(path, "Client.System.Scheduler.Instance.VfxObject") ?? IntPtr.Zero;
        VfxManager.StaticVfxRun?.Invoke(Handle, 0f, 0xFFFFFFFF);

        UpdatePosition(position);
        UpdateRotation(rotation);
        UpdateScale(scale);
    }

    private protected override void CustomUpdate()
    {
        if (Owner == null) return;

        UpdatePosition(Owner.Position);

        if (Target == null)
        {
            UpdateRotation(Owner.Rotation);
        }
        else
        {
            var dir = Target.Position - Owner.Position;
            UpdateRotation(MathF.Atan2(dir.X , dir.Z));
        }
    }

    private protected override void Remove() => VfxManager.StaticVfxRemove?.Invoke(Handle);
}
