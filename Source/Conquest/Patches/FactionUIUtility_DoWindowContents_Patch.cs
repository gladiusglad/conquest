using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace Conquest.Patches
{
    [HarmonyPatch(typeof(FactionUIUtility), nameof(FactionUIUtility.DoWindowContents))]
    public class FactionUIUtility_DoWindowContents_Patch
    {
        public static bool Prefix(Rect fillRect, ref Vector2 scrollPosition, ref float scrollViewHeight, Faction scrollToFaction = null)
        {
            MainTabWindow_Factions.DoWindowContents(fillRect, ref scrollPosition, ref scrollViewHeight, scrollToFaction);
            return false;
        }
    }
}
