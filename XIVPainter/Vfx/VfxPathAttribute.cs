namespace XIVPainter.Vfx;

[AttributeUsage(AttributeTargets.Field)]
public class VfxPathAttribute(string path) : Attribute
{
    public string Path => path;
}
