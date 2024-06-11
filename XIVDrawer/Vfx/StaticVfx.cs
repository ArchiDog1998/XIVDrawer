//From https://github.com/0ceal0t/Dalamud-VFXEditor/blob/main/VFXEditor/Interop/Structs/Vfx/BaseVfx.cs

using Dalamud.Game.ClientState.Objects.Types;
using System.Runtime.InteropServices;

namespace XIVDrawer.Vfx;

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

    public static implicit operator Vector4(Quat pos) => new(pos.X, pos.Y, pos.Z, pos.W);

    public static implicit operator SharpDX.Vector4(Quat pos) => new(pos.X, pos.Z, pos.Y, pos.W);
}

/// <summary>
/// 
/// </summary>
public unsafe class StaticVfx : BasicDrawing
{
    internal VfxStruct* Vfx = null;
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
            if (Vfx == null) return;

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

    /// <summary>
    /// The owner of this static vfx
    /// </summary>
    public GameObject? Owner { get; set; }

    /// <summary>
    /// The rotation to the target.
    /// </summary>
    public GameObject? Target { get; set; }

    /// <summary>
    /// The rotate addition.
    /// </summary>
    public float RotateAddition { get; init; }

    /// <summary>
    /// The location offset.
    /// </summary>
    public Vector3 LocationOffset { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="owner"></param>
    /// <param name="scale"></param>
    public StaticVfx(string path, GameObject owner, Vector3 scale)
        : this(path, owner.Position, owner.Rotation, scale)
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
        : this(path, position, new Vector3(0, 0, rotation), scale)
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
        Vfx = (VfxStruct*)(VfxManager.StaticVfxCreate?.Invoke(path, "Client.System.Scheduler.Instance.VfxObject") ?? nint.Zero);
        if (Vfx == null) return;

        VfxManager.StaticVfxRun?.Invoke((nint)Vfx, 0f, 0xFFFFFFFF);

        VfxManager.AddedVfxStructs.Add(this);

        UpdatePosition(position);
        UpdateRotation(rotation);
        UpdateScale(scale);
    }

    private protected override void AdditionalUpdate()
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
        }
        catch (Exception e)
        {
            Service.Log.Error(e, "sth wrong");
        }
        finally
        {
            if (!Enable)
            {
                Vfx->Scale.Y = 0;
            }
            Update();
        }
    }

    private protected sealed override void AdditionalDispose()
    {
        try
        {
            if (Vfx == null) return;
            if (!VfxManager.AddedVfxStructs.Remove(this)) return;
            VfxManager.StaticVfxRemoveHook?.Original((nint)Vfx);
        }
        finally
        {
            Vfx = null;
        }
    }


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
