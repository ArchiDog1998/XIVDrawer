using Dalamud.Utility;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiScene;
using Lumina.Data.Files;
using XIVPainter.Element2D;

namespace XIVPainter.ElementSpecial;

public class DrawingHighlightHotbar : IDrawing
{
    static TexFile _tex = null;
    static readonly Vector2 _uv1 = new Vector2(96 * 5 / 852f, 0), 
        _uv2 = new Vector2((96 * 5 + 144) / 852f, 0.5f);

    TextureWrap _texture = null;

    public uint ActionId { get; set; } = 0;

    Vector4 _color = new Vector4(0.8f, 0.5f, 0.3f, 1);
    public Vector4 Color 
    {
        get => _color;
        set
        {
            _color = value;
            SetTexture();
        }
    }

    public DrawingHighlightHotbar()
    {
        _tex ??= XIVPainter.Data.GetFile<TexFile>("ui/uld/icona_frame_hr1.tex");
        SetTexture();
    }

    void SetTexture()
    {
        _texture = XIVPainter._pluginInterface.UiBuilder.LoadImageRaw(GetImageData(_tex), _tex!.Header.Width, _tex!.Header.Height, 4);
    }

    byte[] GetImageData(TexFile texFile)
    {
        byte[] imageData = texFile.ImageData;
        byte[] array = new byte[imageData.Length];
        for (int i = 0; i < array.Length; i += 4)
        {
            array[i] = (byte)(byte.MaxValue * Color.X);
            array[i + 1] = (byte)(byte.MaxValue * Color.Y);
            array[i + 2] = (byte)(byte.MaxValue * Color.Z);
            array[i + 3] = (byte) (imageData[i + 3] * Color.W);
        }

        return array;
    }

    public unsafe IEnumerable<IDrawing2D> To2D(XIVPainter owner)
    {
        if(_texture == null) return Array.Empty<IDrawing2D>();

        List<IDrawing2D> result = new List<IDrawing2D>();

        var hotBarIndex = 0;
        foreach (var intPtr in GetAddons<AddonActionBar>()
            .Union(GetAddons<AddonActionBarX>())
            .Union(GetAddons<AddonActionCross>())
            .Union(GetAddons<AddonActionDoubleCrossBase>()))
        {
            if (intPtr == IntPtr.Zero) continue;
            var actionBar = (AddonActionBarBase*)intPtr;
            var s = actionBar->AtkUnitBase.Scale;
            if (actionBar->AtkUnitBase.IsVisible)
            {
                var hotBar = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule()->HotBar[hotBarIndex];
                var slotIndex = 0;
                foreach (var slot in actionBar->Slot)
                {
                    var iconAddon = slot.Icon;
                    if ((IntPtr)iconAddon != IntPtr.Zero && iconAddon->AtkResNode.IsVisible)
                    {
                        AtkResNode node = default;
                        HotBarSlot* bar;

                        if (hotBarIndex > 9)
                        {
                            if (slot.Icon->AtkResNode.ParentNode->ParentNode->IsVisible)
                            {
                                var manager = slot.Icon->AtkResNode.ParentNode->GetAsAtkComponentNode()->Component->UldManager.NodeList[2]->GetAsAtkComponentNode()->Component->UldManager;

                                for (var i = 0; i < manager.NodeListCount; i++)
                                {
                                    node = *manager.NodeList[i];
                                    if (node.Width == 72) break;
                                }
                            }

                            bar = null;
                        }
                        else
                        {
                            node = *slot.Icon->AtkResNode.ParentNode->ParentNode;
                            bar = hotBar->Slot[slotIndex];
                        }

                        if (IsActionSlotRight(slot, bar))
                        {
                            var pt1 = new Vector2(node.ScreenX, node.ScreenY);
                            var pt2 = pt1 + new Vector2(node.Width * s, node.Height * s);

                            result.Add(new ImageDrawing(_texture.ImGuiHandle, pt1, pt2, _uv1, _uv2));
                        }
                    }

                    slotIndex++;
                }
            }
            hotBarIndex++;
        }

        return result;
    }

    public void UpdateOnFrame(XIVPainter painter)
    {
        return;
    }

    unsafe static IEnumerable<IntPtr> GetAddons<T>() where T : struct
    {
        if (typeof(T).GetCustomAttribute<Addon>() is not Addon on) return Array.Empty<nint>();

        return on.AddonIdentifiers
            .Select(str => XIVPainter.GameGui.GetAddonByName(str, 1))
            .Where(ptr => ptr != IntPtr.Zero);
    }

    unsafe bool IsActionSlotRight(ActionBarSlot slot, HotBarSlot* hot)
    {
        if ((IntPtr)hot != IntPtr.Zero)
        {
            if (hot->IconTypeA != HotbarSlotType.CraftAction && hot->IconTypeA != HotbarSlotType.Action) return false;
            if (hot->IconTypeB != HotbarSlotType.CraftAction && hot->IconTypeB != HotbarSlotType.Action) return false;
        }

        return ActionId == ActionManager.Instance()->GetAdjustedActionId((uint)slot.ActionId);
    }
}
