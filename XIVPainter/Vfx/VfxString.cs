using Dalamud.Utility;

namespace XIVPainter.Vfx;
public struct VfxString
{
    private string _string;

    public static implicit operator VfxString(string str) => new() { _string = str };
    public static implicit operator string(VfxString str) => str._string;

    public static implicit operator VfxString(System.Enum str) => str.GetAttribute<VfxPathAttribute>()?.Path ?? string.Empty;
}
