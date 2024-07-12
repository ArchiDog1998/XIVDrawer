using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;

namespace XIVDrawer.ElementSpecial;

/// <summary>
/// The Hot bar ID
/// </summary>
public readonly record struct HotbarID(HotbarSlotType SlotType, uint Id)
{
    ///// <summary>
    ///// Convert from a action id.
    ///// </summary>
    ///// <param name="actionId"></param>
    //public static implicit operator HotbarID(uint actionId) => new(HotbarSlotType.Action, actionId);
}
