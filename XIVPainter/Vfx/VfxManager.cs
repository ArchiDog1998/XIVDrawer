// From https://github.com/0ceal0t/Dalamud-VFXEditor/tree/main/VFXEditor/Interop

using Dalamud.Hooking;
using System.Runtime.InteropServices;
using System.Text;

namespace XIVPainter.Vfx;
internal static class VfxManager
{
    //From https://github.com/0ceal0t/Dalamud-VFXEditor/blob/main/VFXEditor/Interop/Constants.cs
    private const string StaticVfxCreateSig = "E8 ?? ?? ?? ?? F3 0F 10 35 ?? ?? ?? ?? 48 89 43 08";
    private const string StaticVfxRunSig = "E8 ?? ?? ?? ?? 8B 4B 7C 85 C9";
    private const string StaticVfxRemoveSig = "40 53 48 83 EC 20 48 8B D9 48 8B 89 ?? ?? ?? ?? 48 85 C9 74 28 33 D2 E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 85 C9";

    private const string ActorVfxCreateSig = "40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8";

    //====== STATIC ===========
    public delegate nint StaticVfxCreateDelegate(string path, string pool);

    public static StaticVfxCreateDelegate? StaticVfxCreate;

    public delegate nint StaticVfxRunDelegate(nint vfx, float a1, uint a2);

    public static StaticVfxRunDelegate? StaticVfxRun;

    public delegate nint StaticVfxRemoveDelegate(nint vfx);

    public static StaticVfxRemoveDelegate? StaticVfxRemove;

    // ======== ACTOR =============
    public delegate nint ActorVfxCreateDelegate(string path, nint a2, nint a3, float a4, char a5, ushort a6, char a7);

    public static ActorVfxCreateDelegate? ActorVfxCreate;

    public static HashSet<StaticVfx> AddedVfxStructs { get; } = [];
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

        ActorVfxCreate = Marshal.GetDelegateForFunctionPointer<ActorVfxCreateDelegate>(actorVfxCreateAddress);
        StaticVfxRemove = Marshal.GetDelegateForFunctionPointer<StaticVfxRemoveDelegate>(staticVfxRemoveAddress);
        StaticVfxRun = Marshal.GetDelegateForFunctionPointer<StaticVfxRunDelegate>(Service.SigScanner.ScanText(StaticVfxRunSig));
        StaticVfxCreate = Marshal.GetDelegateForFunctionPointer<StaticVfxCreateDelegate>(staticVfxCreateAddress);

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
        foreach (var item in AddedVfxStructs)
        {
            item.Dispose();
        }
    }

    public static void Dispose()
    {
        ClearAllVfx();

        Service.ClientState.Logout -= ClearAllVfx;
#if DEBUG
        _getResourceAsyncHook?.Dispose();
        _getResourceSyncHook?.Dispose();
#endif
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
