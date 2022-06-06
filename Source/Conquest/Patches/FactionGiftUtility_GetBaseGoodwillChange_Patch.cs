using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Conquest.Patches
{
    [HarmonyPatch(typeof(FactionGiftUtility), "GetBaseGoodwillChange")]
    public class FactionGiftUtility_GetBaseGoodwillChange_Patch
    {
        public static bool Prefix(Thing anyThing, int count, float singlePrice, Faction theirFaction, ref float __result)
        {
            if (count <= 0)
            {
                __result = 0f;
                return false;
            }

            float num = singlePrice * (float)count;
            Pawn pawn = anyThing as Pawn;
            if (pawn != null && pawn.IsPrisoner && pawn.Faction == theirFaction)
            {
                num *= 2f;
            }

            __result = num / 100f;
            return false;
        }
    }
}
