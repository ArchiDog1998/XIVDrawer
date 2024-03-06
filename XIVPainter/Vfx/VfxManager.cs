// From https://github.com/0ceal0t/Dalamud-VFXEditor/tree/main/VFXEditor/Interop

using Dalamud.Hooking;
using Dalamud.Utility;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using XIVPainter.Enum;

namespace XIVPainter.Vfx;
internal static class VfxManager
{
    //From https://github.com/0ceal0t/Dalamud-VFXEditor/blob/main/VFXEditor/Interop/Constants.cs
    private const string StaticVfxCreateSig = "E8 ?? ?? ?? ?? F3 0F 10 35 ?? ?? ?? ?? 48 89 43 08";
    private const string StaticVfxRunSig = "E8 ?? ?? ?? ?? 8B 4B 7C 85 C9";
    private const string StaticVfxRemoveSig = "40 53 48 83 EC 20 48 8B D9 48 8B 89 ?? ?? ?? ?? 48 85 C9 74 28 33 D2 E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 85 C9";

    private const string ActorVfxCreateSig = "40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8";
    private const string ActorVfxRemoveSig = "0F 11 48 10 48 8D 05"; // the weird one

    //====== STATIC ===========
    public delegate nint StaticVfxCreateDelegate(string path, string pool);

    public static StaticVfxCreateDelegate? StaticVfxCreate;

    public delegate nint StaticVfxRunDelegate(nint vfx, float a1, uint a2);

    public static StaticVfxRunDelegate? StaticVfxRun;

    public delegate nint StaticVfxRemoveDelegate(nint vfx);

    public static StaticVfxRemoveDelegate? StaticVfxRemove;

    // ======= STATIC HOOKS ========
    public static Hook<StaticVfxRemoveDelegate>? StaticVfxRemoveHook { get; private set; }

    // ======== ACTOR =============
    public delegate nint ActorVfxCreateDelegate(string path, nint a2, nint a3, float a4, char a5, ushort a6, char a7);

    public static ActorVfxCreateDelegate? ActorVfxCreate;

    public delegate nint ActorVfxRemoveDelegate(nint vfx, char a2);

    public static ActorVfxRemoveDelegate? ActorVfxRemove;

    // ======== ACTOR HOOKS =============

    public static Hook<ActorVfxRemoveDelegate>? ActorVfxRemoveHook { get; private set; }

    public static HashSet<BaseVfx> AddedVfxStructs { get; } = [];

#if DEBUG
    private unsafe delegate void* GetResourceSyncPrototype(IntPtr pFileManager, uint* pCategoryId, char* pResourceType, uint* pResourceHash, char* pPath, void* pUnknown);

    private unsafe delegate void* GetResourceAsyncPrototype(IntPtr pFileManager, uint* pCategoryId, char* pResourceType, uint* pResourceHash, char* pPath, void* pUnknown, bool isUnknown);

    static Hook<GetResourceSyncPrototype>? _getResourceSyncHook;
    static Hook<GetResourceAsyncPrototype>? _getResourceAsyncHook;
#endif

    public static void Init()
    {
        var staticVfxCreateAddress = Service.SigScanner.ScanText(StaticVfxCreateSig);
        var staticVfxRemoveAddress = Service.SigScanner.ScanText(StaticVfxRemoveSig);
        var actorVfxCreateAddress = Service.SigScanner.ScanText(ActorVfxCreateSig);
        var actorVfxRemoveAddressTemp = Service.SigScanner.ScanText(ActorVfxRemoveSig) + 7;
        var actorVfxRemoveAddress = Marshal.ReadIntPtr(actorVfxRemoveAddressTemp + Marshal.ReadInt32(actorVfxRemoveAddressTemp) + 4);

        ActorVfxCreate = Marshal.GetDelegateForFunctionPointer<ActorVfxCreateDelegate>(actorVfxCreateAddress);
        ActorVfxRemove = Marshal.GetDelegateForFunctionPointer<ActorVfxRemoveDelegate>(actorVfxRemoveAddress);
        StaticVfxRemove = Marshal.GetDelegateForFunctionPointer<StaticVfxRemoveDelegate>(staticVfxRemoveAddress);
        StaticVfxRun = Marshal.GetDelegateForFunctionPointer<StaticVfxRunDelegate>(Service.SigScanner.ScanText(StaticVfxRunSig));
        StaticVfxCreate = Marshal.GetDelegateForFunctionPointer<StaticVfxCreateDelegate>(staticVfxCreateAddress);

        StaticVfxRemoveHook = Service.Hook.HookFromAddress<StaticVfxRemoveDelegate>(staticVfxRemoveAddress, StaticVfxRemoveHandler);
        ActorVfxRemoveHook = Service.Hook.HookFromAddress<ActorVfxRemoveDelegate>(actorVfxRemoveAddress, ActorVfxRemoveHandler);

        StaticVfxRemoveHook.Enable();
        ActorVfxRemoveHook.Enable();

        Service.ClientState.Logout += ClearAllVfx;
#if DEBUG
        unsafe
        {
            _getResourceSyncHook = Service.Hook.HookFromSignature<GetResourceSyncPrototype>("E8 ?? ?? ?? ?? 48 8D 8F ?? ?? ?? ?? 48 89 87 ?? ?? ?? ?? 48 8D 54 24", GetResourceSyncDetour);
            _getResourceSyncHook.Enable();

            _getResourceAsyncHook = Service.Hook.HookFromSignature<GetResourceAsyncPrototype>("E8 ?? ?? ?? ?? 48 8B D8 EB 07 F0 FF 83", GetResourceAsyncDetour);
            _getResourceAsyncHook.Enable();
        }
#endif
    }

    public static void ClearAllVfx()
    {
        lock (AddedVfxStructs)
        {
            foreach (var item in AddedVfxStructs)
            {
                item.Dispose();
            }
        }
    }

    public static void Dispose()
    {
        ClearAllVfx();

        Service.ClientState.Logout -= ClearAllVfx;

        StaticVfxRemoveHook?.Dispose();
        ActorVfxRemoveHook?.Dispose();
#if DEBUG
        _getResourceAsyncHook?.Dispose();
        _getResourceSyncHook?.Dispose();
#endif
    }

    private static nint StaticVfxRemoveHandler(nint vfx)
    {
        RemoveVfx(vfx);
        return StaticVfxRemoveHook!.Original(vfx);
    }

    private static nint ActorVfxRemoveHandler(nint vfx, char a2)
    {
        RemoveVfx(vfx);
        return ActorVfxRemoveHook!.Original(vfx, a2);
    }

    private static void RemoveVfx(nint vfx)
    {
        BaseVfx? item;

        lock (AddedVfxStructs)
        {
            item = AddedVfxStructs.FirstOrDefault(x => x.Handle == vfx);
        }

        if (item == null) return;

#if DEBUG
        Service.Log.Debug($"!!Remove the vfx at hook to use dispose at {vfx:x}");
#endif
        try
        {
            lock (AddedVfxStructs)
            {
                AddedVfxStructs.Remove(item);
            }
            item?.Dispose();
        }
        catch (Exception ex)
        {
            Service.Log.Error(ex, "Failed to dispose the vfx.");
        }
    }

#if DEBUG
    private unsafe static void* GetResourceSyncDetour(IntPtr pFileManager, uint* pCategoryId, char* pResourceType, uint* pResourceHash, char* pPath, void* pUnknown)
    {
        DrawDebug(pPath);
        return _getResourceSyncHook!.Original(pFileManager, pCategoryId, pResourceType, pResourceHash, pPath, pUnknown);
        }

    private unsafe static void* GetResourceAsyncDetour(IntPtr pFileManager, uint* pCategoryId, char* pResourceType, uint* pResourceHash, char* pPath, void* pUnknown, bool isUnknown)
    {
        DrawDebug(pPath);
        return _getResourceAsyncHook!.Original(pFileManager, pCategoryId, pResourceType, pResourceHash, pPath, pUnknown, isUnknown);
    }

    private unsafe static void DrawDebug(char* pPath)
    {
        var path = ReadTerminatedString((byte*)pPath);

        if (!path.EndsWith("avfx")) return;

        else if (!path.StartsWith("vfx/common/eff/")
            && !path.StartsWith("vfx/omen/eff/"))
        {
            Service.Log.Verbose("Object Unknown: " + path);
        }
        static unsafe string ReadTerminatedString(byte* ptr)
        {
            if (ptr == null)
            {
                return string.Empty;
            }

            var bytes = new List<byte>();
            while (*ptr != 0)
            {
                bytes.Add(*ptr);
                ptr += 1;
            }

            return Encoding.UTF8.GetString([.. bytes]);
        }
    }
#endif
}
