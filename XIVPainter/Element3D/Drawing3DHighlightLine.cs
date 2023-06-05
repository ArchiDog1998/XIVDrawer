namespace XIVPainter.Element3D;

public class Drawing3DHighlightLine: Drawing3DPoly
{
    public Vector3 From { get; set; }
    public Vector3 To { get; set; }
    public float Radius { get; set; }
    public float Thickness { get; set; }
    Drawing3DCircularSector cir1, cir2;

    public Drawing3DHighlightLine(Vector3 from, Vector3 to, float radius, uint color, float thickness)
    {
        SubItems = new IDrawing3D[]
        {
            cir1 = new Drawing3DCircularSector(from, 0, color, thickness)
            {
                IsFill = false,
            },
            cir2 = new Drawing3DCircularSector(to, 0, color, thickness)
            {
                IsFill = false,
            },
        };

        From = from;
        To = to;
        Radius = radius;
        Thickness = thickness;
        Color = color;
    }

    public override void UpdateOnFrame(XIVPainter painter)
    {
        var d = DateTime.Now.Millisecond / 1000f;
        var ratio = (float)DrawingHelper.EaseFuncRemap(EaseFuncType.None, EaseFuncType.Cubic)(d);
        if(Radius == 0)
        {
            cir1.Radius = cir2.Radius = 0;
        }
        else
        {
            cir1.Radius = MathF.Max(0.01f, (1 - ratio) * Radius / 2);
            cir2.Radius = MathF.Max(0.01f, ratio * Radius);
        }
        cir1.Thickness = cir2.Thickness = Thickness;
        cir1.Color = cir2.Color = Color;

        cir1.Center = From + (To -  From) * ratio;
        cir2.Center = To;

        base.UpdateOnFrame(painter);
    }
}
