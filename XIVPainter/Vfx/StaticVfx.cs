using Dalamud.Game.ClientState.Objects.Types;

namespace XIVPainter.Vfx;

/// <summary>
/// 
/// </summary>
public class StaticVfx : BaseVfx
{
    public GameObject? Owner { get; set; }

    public GameObject? Target { get; set; }

    public float RotateAddition { get; init; }

    public Vector3 LocationOffset { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="owner"></param>
    /// <param name="scale"></param>
    public StaticVfx(string path, GameObject owner, Vector3 scale)
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
    public unsafe StaticVfx(string path, Vector3 position, float rotation, Vector3 scale)
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
    public unsafe StaticVfx(string path, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        Handle = VfxManager.StaticVfxCreate?.Invoke(path, "Client.System.Scheduler.Instance.VfxObject") ?? IntPtr.Zero;
        VfxManager.StaticVfxRun?.Invoke(Handle, 0f, 0xFFFFFFFF);

        VfxManager.AddedVfxStructs.Add(this);

        UpdatePosition(position);
        UpdateRotation(rotation);
        UpdateScale(scale);
    }

    private protected override void CustomUpdate()
    {
        try
        {
            if (Owner == null) return;

            var rotation = Owner.Rotation;
            if (Target != null)
            {
                var dir = Target.Position - Owner.Position;
                rotation = MathF.Atan2(dir.X, dir.Z);
            }
            UpdateRotation(rotation += RotateAddition);

            var locOff = new Vector3(
                LocationOffset.X * MathF.Sin(rotation),
                LocationOffset.Y,
                LocationOffset.Z * MathF.Cos(rotation));

            UpdatePosition(Owner.Position + locOff);
        }catch (Exception e)
        {
            Service.Log.Error(e, "sth wrong");
        }

    }

    private protected override void Remove()
    {
        if (!VfxManager.AddedVfxStructs.Remove(this)) return;
        VfxManager.StaticVfxRemoveHook?.Original(Handle);
    }
}
