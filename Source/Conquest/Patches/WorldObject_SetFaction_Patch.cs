using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace Conquest.Patches
{
    [HarmonyPatch(typeof(WorldObject), nameof(WorldObject.SetFaction))]
    public class WorldObject_SetFaction_Patch
    {
        public static void Postfix(Faction newFaction, WorldObject __instance)
        {
            if (__instance is Settlement settlement)
            {
                FactionData factionData = FactionUtility.GetFactionData(newFaction);
                if (factionData.capital == null)
                {
                    factionData.capital = settlement;
                }
            }
        }
    }
}
