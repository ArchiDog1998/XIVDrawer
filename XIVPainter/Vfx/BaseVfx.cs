//From https://github.com/0ceal0t/Dalamud-VFXEditor/blob/main/VFXEditor/Interop/Structs/Vfx/BaseVfx.cs

using System.Runtime.InteropServices;

namespace XIVPainter.Vfx;

[StructLayout(LayoutKind.Explicit)]

internal unsafe struct VfxStruct
{
    [FieldOffset(0x38)] public byte Flags;
    [FieldOffset(0x50)] public Vector3 Position;
    [FieldOffset(0x60)] public Vector4 Rotation;
    [FieldOffset(0x70)] public Vector3 Scale;

    [FieldOffset(0x128)] public int ActorCaster;
    [FieldOffset(0x130)] public int ActorTarget;

    [FieldOffset(0x1B8)] public int StaticCaster;
    [FieldOffset(0x1C0)] public int StaticTarget;
}

/// <summary>
/// The basic vfx
/// </summary>
public abstract unsafe class BaseVfx : IDisposable
{
    private VfxStruct* Vfx;

    internal IntPtr Handle 
    { 
        get =>(IntPtr)Vfx;
        init
        {
            Vfx = (VfxStruct*)value;
            if (Vfx == null) return;
            VfxManager.AddedVfxStructs.Add(this);
        }
    }

    /// <inheritdoc/>>
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (!VfxManager.AddedVfxStructs.Remove(this)) return;
        if (Handle == IntPtr.Zero) return;
        Remove();

        Vfx = null;
    }

    private protected abstract void Remove();

    /// <summary>
    /// 
    /// </summary>
    public void Update()
    {
        if (Vfx == null) return;

        Vfx->Flags |= 0x2;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    public void UpdatePosition(Vector3 position)
    {
        if (Vfx == null) return;
        Vfx->Position = new Vector3
        {
            X = position.X,
            Y = position.Y,
            Z = position.Z,
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="scale"></param>
    public void UpdateScale(Vector3 scale)
    {
        if (Vfx == null) return;

        Vfx->Scale = new Vector3
        {
            X = scale.X,
            Y = scale.Y,
            Z = scale.Z
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rotation"></param>
    public void UpdateRotation(Vector3 rotation)
    {
        if (Vfx == null) return;

        var q = Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
        Vfx->Rotation = new Vector4
        {
            X = q.X,
            Y = q.Y,
            Z = q.Z,
            W = q.W
        };
    }
}
