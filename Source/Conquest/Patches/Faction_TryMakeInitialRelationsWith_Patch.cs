using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Conquest.Patches
{
    [HarmonyPatch(typeof(Faction), nameof(Faction.TryMakeInitialRelationsWith))]
    public class Faction_TryMakeInitialRelationsWith_Patch
    {
        public static bool Prefix(Faction other, Faction __instance, List<FactionRelation> ___relations)
        {
            if (__instance.RelationWith(other, allowNull: true) == null)
            {
                int a2 = GetInitialGoodwill(__instance, other);
                int b2 = GetInitialGoodwill(other, __instance);
                int num = Mathf.Min(a2, b2);

                FactionData factionData = GetFactionData(__instance);
                FactionData factionData2 = GetFactionData(other);

                FactionRelationKind kind = FactionRelationKind.Neutral;
                FactionAttitudeType type = FactionAttitudeType.Neutral;
                int trust = __instance.IsPlayer || other.IsPlayer ? 30 : 50;
                if (num < -10)
                {
                    kind = FactionRelationKind.Hostile;
                    type = FactionAttitudeType.Hostile;
                    trust = 0;
                }
                else if (num > 75 && !__instance.Hidden && !other.Hidden)
                {
                    kind = FactionRelationKind.Ally;
                    type = FactionAttitudeType.Ally;
                    factionData.allies.Add(other);
                    factionData2.allies.Add(__instance);
                    trust = 90;
                }
                else if (num > 10 && type != FactionAttitudeType.Ally)
                {
                    type = FactionAttitudeType.Friendly;
                    trust = 60;
                }

                FactionRelation factionRelation = new FactionRelation();
                factionRelation.other = other;
                factionRelation.baseGoodwill = num;
                factionRelation.kind = kind;
                ___relations.Add(factionRelation);

                FactionRelation factionRelation2 = new FactionRelation();
                factionRelation2.other = __instance;
                factionRelation2.baseGoodwill = num;
                factionRelation2.kind = kind;
                FieldInfo relationsField = AccessTools.Field(typeof(Faction), "relations");
                List<FactionRelation> relations = (List<FactionRelation>)relationsField.GetValue(other);
                relations.Add(factionRelation2);

                FactionAttitude factionAttitude = new FactionAttitude();
                factionAttitude.other = factionData2;
                factionAttitude.type = type;
                factionAttitude.trust = trust;
                factionData.AddAttitude(factionAttitude);

                FactionAttitude factionAttitude2 = new FactionAttitude();
                factionAttitude2.other = factionData;
                factionAttitude2.type = type;
                factionAttitude2.trust = trust;
                factionData2.AddAttitude(factionAttitude2);
            }
            return false;
        }

        private static int GetInitialGoodwill(Faction a, Faction b)
        {
            if (a.def.permanentEnemy)
            {
                return -100;
            }

            if (a.def.permanentEnemyToEveryoneExceptPlayer && !b.IsPlayer)
            {
                return -100;
            }

            if (a.def.permanentEnemyToEveryoneExcept != null && !a.def.permanentEnemyToEveryoneExcept.Contains(b.def))
            {
                return -100;
            }

            if (!a.IsPlayer && !b.IsPlayer)
            {
                float chance = 0f;
                if (a.def == b.def)
                {
                    chance += 0.4f;
                }
                if (a.def.naturalEnemy)
                {
                    chance += 0.2f;
                }
                else
                {
                    chance += 0.6f;
                }

                if (UnityEngine.Random.value < chance)
                {
                    int bonus = a.def == b.def ? 50 : 20;
                    return Mathf.Min((int)Math.Round(UnityEngine.Random.value * 10) * 10 + bonus, 100);
                }
            }

            if (a.def.naturalEnemy)
            {
                return -80;
            }

            return 0;
        }

        private static FactionData GetFactionData(Faction faction)
        {
            FactionData factionData = FactionUtility.GetWC().GetFactionData(faction);

            if (factionData == null)
            {
                factionData = FactionUtility.GetWC().AddFactionData(faction);
            }

            return factionData;
        }
    }
}
