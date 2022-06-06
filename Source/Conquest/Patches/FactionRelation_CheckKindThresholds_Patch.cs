using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace Conquest.Patches
{
    [HarmonyPatch(typeof(FactionRelation), nameof(FactionRelation.CheckKindThresholds))]
    public class FactionRelation_CheckKindThresholds_Patch
    {
        public static bool Prefix(Faction faction, bool canSendLetter, string reason, GlobalTargetInfo lookTarget, out bool sentLetter, FactionRelation __instance)
        {
            FactionData factionData = FactionUtility.GetFactionData(faction);
            FactionAttitude attitude = factionData.TryGetAttitudeTowards(__instance.other);
            attitude.UpdateAttitude(factionData, canSendLetter, reason, lookTarget, out sentLetter);
            return false;
        }
    }
}
