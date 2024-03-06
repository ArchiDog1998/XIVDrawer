//From https://github.com/0ceal0t/Dalamud-VFXEditor/blob/main/VFXEditor/Interop/Structs/Vfx/BaseVfx.cs

using System.Runtime.InteropServices;

namespace XIVPainter.Vfx;

[StructLayout(LayoutKind.Explicit)]

internal unsafe struct VfxStruct
{
    [FieldOffset(0x38)] public byte Flags;
    [FieldOffset(0x50)] public Vector3 Position;
    [FieldOffset(0x60)] public Quat Rotation;
    [FieldOffset(0x70)] public Vector3 Scale;

    [FieldOffset(0x128)] public int ActorCaster;
    [FieldOffset(0x130)] public int ActorTarget;

    [FieldOffset(0x1B8)] public int StaticCaster;
    [FieldOffset(0x1C0)] public int StaticTarget;
}

[StructLayout(LayoutKind.Sequential)]
internal struct Quat
{
    public float X;
    public float Z;
    public float Y;
    public float W;

    public static implicit operator System.Numerics.Vector4(Quat pos) => new(pos.X, pos.Y, pos.Z, pos.W);

    public static implicit operator SharpDX.Vector4(Quat pos) => new(pos.X, pos.Z, pos.Y, pos.W);
}

/// <summary>
/// The basic vfx
/// </summary>
public abstract unsafe class BaseVfx() : BasicDrawing()
{
    private protected VfxStruct* Vfx;

    private float _height;
    private bool _enable = true;

    /// <inheritdoc/>
    public override bool Enable 
    {
        get => _enable;
        set
        {
            if (_enable == value) return;
            _enable = value;

            unsafe
            {
                if (_enable)
                {
                    Vfx->Scale.Y = _height;
                }
                else
                {
                    _height = Vfx->Scale.Y;
                    Vfx->Scale.Y = 0;
                }
            }

            Update();
        }
    }
    internal IntPtr Handle 
    { 
        get =>(IntPtr)Vfx;
        init
        {
            Vfx = (VfxStruct*)value;
            if (Vfx == null) return;

            lock (VfxManager.AddedVfxStructs)
            {
                VfxManager.AddedVfxStructs.Add(this);
            }
        }
    }

    private protected sealed override void AdditionalUpdate()
    {
        CustomUpdate();
        if (!Enable)
        {
            Vfx->Scale.Y = 0;
        }
        Update();
    }

    private protected virtual void CustomUpdate()
    {

    }

    /// <inheritdoc/>>
    private protected override void AdditionalDispose()
    {
        try
        {
            if (Handle == IntPtr.Zero) return;
            lock (VfxManager.AddedVfxStructs)
            {
                if (!VfxManager.AddedVfxStructs.Remove(this)) return;
            }
#if DEBUG
            Service.Log.Debug($"Dispose the vfx from Dispose at {Handle:x}");
#endif
            Remove();
        }
        finally
        {
            Vfx = null;
        }
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
    public void UpdateRotation(float rotation) 
        => UpdateRotation(new Vector3(0, 0, rotation));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rotation"></param>
    public void UpdateRotation(Vector3 rotation)
    {
        if (Vfx == null) return;

        var q = Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
        Vfx->Rotation = new Quat
        {
            X = q.X,
            Y = q.Y,
            Z = q.Z,
            W = q.W
        };
    }
}
