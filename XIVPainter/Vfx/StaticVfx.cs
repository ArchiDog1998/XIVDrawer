namespace XIVPainter.Vfx;

/// <summary>
/// 
/// </summary>
public class StaticVfx : BaseVfx
{
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
        VfxManager.StaticVfxRun?.Invoke(Handle, 0.0f, 0xFFFFFFFF);

        UpdatePosition(position);
        UpdateRotation(rotation);
        UpdateScale(scale);
        Update();
    }

    private protected override void Remove() => VfxManager.StaticVfxRemove?.Invoke(Handle);
}
