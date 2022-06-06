using HarmonyLib;
using RimWorld;

namespace Conquest.Patches
{
    [HarmonyPatch(typeof(Faction), nameof(Faction.TryAffectGoodwillWith))]
    public class Faction_TryAffectGoodwillWith_Patch
    {
        public static void Postfix(Faction other, Faction __instance, bool __result)
        {
            if (__result)
            {
                FactionUtility.GetFactionData(__instance).UpdateAttitudeTowards(other);
            }
        }
    }
}
