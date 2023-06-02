using XIVPainter.Element2D;

namespace XIVPainter.Element3D;

public class Drawing3DPoly : Drawing3D
{
    public IDrawing3D[] SubItems { get; set; }

    public override IEnumerable<IDrawing2D> To2D(XIVPainter owner)
    {
        return SubItems.SelectMany(i =>  i.To2D(owner));
    }

    public override void UpdateOnFrame(XIVPainter painter)
    {
        base.UpdateOnFrame(painter);
        foreach (var item in SubItems)
        {
            item.Color = Color;
            item.WarningRatio = WarningRatio;
            item.WarningTime = WarningTime;
            item.WarningType = WarningType;
            item.DeadTime = DeadTime;
            item.TimeToDisappear = TimeToDisappear;
            item.DisappearType = DisappearType;
            item.AlphaRatio = AlphaRatio;
        }
    }
}
