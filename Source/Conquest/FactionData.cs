using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Conquest
{
    public class FactionData : IExposable, ILoadReferenceable
    {
        public Faction faction;

        public int randomKey;
        public int loadID = -1;

        public List<Faction> vassals = new List<Faction>();
        public List<Faction> allies = new List<Faction>();
        public List<FactionAttitude> attitudes = new List<FactionAttitude>();

        public Settlement capital;

        public float aggressiveExpansion = 0;

        public float MilitaryPower => 0;

        public float RulerBravery
        {
            get
            {
                if (faction.leader != null)
                {
                    float rulerBravery = 0.5f;
                    foreach (Trait trait in faction.leader.story.traits.allTraits)
                    {
                        switch (trait.def.defName)
                        {
                            case "Wimp":
                                rulerBravery -= 0.5f;
                                break;
                            case "NaturalMood":
                                rulerBravery += trait.Degree * 0.1f;
                                break;
                            case "Nerves":
                                rulerBravery += trait.Degree * 0.15f;
                                break;
                        }
                    }
                    return Math.Min(Math.Max(rulerBravery, 0), 1);
                }

                return 0;
            }
        }

        public FactionData()
        {
            randomKey = Rand.Range(0, int.MaxValue);
        }

        public FactionData(Faction faction) : this()
        {
            this.faction = faction;
            loadID = faction.loadID;
        }
        public bool IsOverlordOf(Faction vassal) => vassals.Contains(vassal);

        public bool IsAlliedTo(Faction ally) => allies.Contains(ally);

        public bool IsHostileTo(Faction enemy) => TryGetAttitudeTowards(enemy).type.IsHostile();

        public bool IsAThreat(FactionData threat) => threat.MilitaryPower > MilitaryPower * 2 * (RulerBravery + 0.5);

        public FactionAttitude GetAttitudeTowards(FactionData other)
        {
            if (other == this) return null;

            foreach (FactionAttitude attitude in attitudes)
            {
                if (attitude.other == other)
                {
                    return attitude;
                }
            }

            return null;
        }

        public FactionAttitude TryGetAttitudeTowards(Faction other)
        {
            if (other == faction) return null;
            FactionAttitude attitude = GetAttitudeTowards(FactionUtility.GetFactionData(other));

            if (attitude == null)
            {
                attitude = AddAttitude(FactionUtility.GetFactionData(other));
            }

            return attitude;
        }

        public void UpdateAttitudeTowards(Faction other) => TryGetAttitudeTowards(other).UpdateAttitude(this);

        public FactionAttitudeType PlayerAttitudeType => TryGetAttitudeTowards(Faction.OfPlayer).type;

        public FactionAttitude AddAttitude(FactionData other)
        {
            if (other == this) return null;

            FactionAttitude attitude = new FactionAttitude();
            attitude.other = other;

            if (!attitudes.Contains(attitude))
            {
                attitudes.Add(attitude);
                attitude.UpdateAttitude(this);

                return attitude;
            }
            else
            {
                return GetAttitudeTowards(other);
            }
        }

        public void AddAttitude(FactionAttitude attitude)
        {
            if (attitude.other == this) return;

            if (!attitudes.Contains(attitude))
            {
                attitudes.Add(attitude);
            }
        }

        public void UpdateAllAttitudes()
        {
            List<FactionData> allFactions = FactionUtility.GetWC().Factions;

            if (allFactions.Count > attitudes.Count)
            {
                foreach (FactionData factionData in allFactions)
                {
                    if (factionData == this) continue;

                    FactionAttitude attitude = GetAttitudeTowards(factionData);

                    if (attitude == null)
                    {
                        AddAttitude(factionData);
                    }
                    else
                    {
                        attitude.UpdateAttitude(this);
                    }
                }
            }
            else
            {
                foreach (FactionAttitude attitude in attitudes)
                {
                    attitude.UpdateAttitude(this);
                }
            }
        }

        public void Notify_AttitudeChanged(FactionData other, FactionAttitudeType previousType, FactionAttitudeType attitudeType, bool canSendLetter, string reason, GlobalTargetInfo lookTarget, out bool sentLetter)
        {
            if (Current.ProgramState != ProgramState.Playing || other.faction != Faction.OfPlayer)
            {
                canSendLetter = false;
            }

            sentLetter = false;
            ColoredText.ClearCache();

            if (attitudeType.IsHostile())
            {
                if (Current.ProgramState == ProgramState.Playing)
                {
                    foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_Alive.ToList())
                    {
                        if ((item.Faction == faction && item.HostFaction == other.faction) || (item.Faction == other.faction && item.HostFaction == faction))
                        {
                            item.guest.SetGuestStatus(item.HostFaction, GuestStatus.Prisoner);
                        }
                    }
                }

                if (other.faction == Faction.OfPlayer)
                {
                    QuestUtility.SendQuestTargetSignals(faction.questTags, "BecameHostileToPlayer", this.Named("SUBJECT"));
                }
            }

            if (other.faction == Faction.OfPlayer && !IsHostileTo(Faction.OfPlayer))
            {
                List<Site> list = new List<Site>();
                List<Site> sites = Find.WorldObjects.Sites;
                for (int i = 0; i < sites.Count; i++)
                {
                    if (sites[i].factionMustRemainHostile && sites[i].Faction == faction && !sites[i].HasMap)
                    {
                        list.Add(sites[i]);
                    }
                }

                if (list.Any())
                {
                    string str;
                    string str2;
                    if (list.Count == 1)
                    {
                        str = "LetterLabelSiteNoLongerHostile".Translate();
                        str2 = "LetterSiteNoLongerHostile".Translate(faction.NameColored, list[0].Label);
                    }
                    else
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        for (int j = 0; j < list.Count; j++)
                        {
                            if (stringBuilder.Length != 0)
                            {
                                stringBuilder.AppendLine();
                            }

                            stringBuilder.Append("  - " + list[j].LabelCap);
                            ImportantPawnComp component = list[j].GetComponent<ImportantPawnComp>();
                            if (component != null && component.pawn.Any)
                            {
                                stringBuilder.Append(" (" + component.pawn[0].LabelCap + ")");
                            }
                        }

                        str = "LetterLabelSiteNoLongerHostileMulti".Translate();
                        str2 = (string)("LetterSiteNoLongerHostileMulti".Translate(faction.NameColored) + ":\n\n") + stringBuilder;
                    }

                    Find.LetterStack.ReceiveLetter(str, str2, LetterDefOf.NeutralEvent, new LookTargets(list.Select((Site x) => new GlobalTargetInfo(x.Tile))));
                    for (int k = 0; k < list.Count; k++)
                    {
                        list[k].Destroy();
                    }
                }
            }

            if (other.faction == Faction.OfPlayer && IsHostileTo(Faction.OfPlayer))
            {
                List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
                for (int l = 0; l < allWorldObjects.Count; l++)
                {
                    if (allWorldObjects[l].Faction == faction)
                    {
                        TradeRequestComp component2 = allWorldObjects[l].GetComponent<TradeRequestComp>();
                        if (component2 != null && component2.ActiveRequest)
                        {
                            component2.Disable();
                        }
                    }
                }

                foreach (Map map in Find.Maps)
                {
                    map.passingShipManager.RemoveAllShipsOfFaction(faction);
                }
            }

            if (canSendLetter)
            {
                TaggedString text = "";
                TryAppendRelationKindChangedInfo(ref text, previousType, attitudeType, reason);
                if (attitudeType.IsHostile())
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_Hostile".Translate(faction.Name), text, LetterDefOf.NegativeEvent, lookTarget, faction);
                    sentLetter = true;
                }
                else if (attitudeType == FactionAttitudeType.Ally)
                {
                    Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_Ally".Translate(faction.Name), text, LetterDefOf.PositiveEvent, lookTarget, faction);
                    sentLetter = true;
                }
                else if (attitudeType.IsNeutral())
                {
                    if (previousType.IsHostile())
                    {
                        Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_NeutralFromHostile".Translate(faction.Name), text, LetterDefOf.PositiveEvent, lookTarget, faction);
                        sentLetter = true;
                    }
                    else
                    {
                        Find.LetterStack.ReceiveLetter("LetterLabelRelationsChange_NeutralFromAlly".Translate(faction.Name), text, LetterDefOf.NeutralEvent, lookTarget, faction);
                        sentLetter = true;
                    }
                }
            }

            if (Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            List<Map> maps = Find.Maps;
            for (int m = 0; m < maps.Count; m++)
            {
                maps[m].attackTargetsCache.Notify_FactionHostilityChanged(faction, other.faction);
                LordManager lordManager = maps[m].lordManager;
                for (int n = 0; n < lordManager.lords.Count; n++)
                {
                    Lord lord = lordManager.lords[n];
                    if (lord.faction == other.faction)
                    {
                        lord.Notify_FactionRelationsChanged(faction, previousType.ToRelationKind());
                    }
                    else if (lord.faction == faction)
                    {
                        lord.Notify_FactionRelationsChanged(other.faction, previousType.ToRelationKind());
                    }
                }
            }
        }

        public void TryAppendRelationKindChangedInfo(ref TaggedString text, FactionAttitudeType previousType, FactionAttitudeType newType, string reason = null)
        {
            if (previousType == newType)
            {
                return;
            }

            if (!text.NullOrEmpty())
            {
                text += "\n\n";
            }

            if (newType.IsHostile())
            {
                text += "LetterRelationsChange_Hostile".Translate(faction.NameColored);
                if (faction.HasGoodwill)
                {
                    text += "\n\n" + "LetterRelationsChange_HostileGoodwillDescription".Translate(faction.PlayerGoodwill.ToStringWithSign(), (-75).ToStringWithSign(), 0.ToStringWithSign());
                }

                if (!reason.NullOrEmpty())
                {
                    text += "\n\n" + "FinalStraw".Translate(reason.CapitalizeFirst());
                }
            }
            else if (newType == FactionAttitudeType.Ally)
            {
                text += "LetterRelationsChange_Ally".Translate(faction.NameColored);
                if (faction.HasGoodwill)
                {
                    text += "\n\n" + "LetterRelationsChange_AllyGoodwillDescription".Translate(faction.PlayerGoodwill.ToStringWithSign(), 75.ToStringWithSign(), 0.ToStringWithSign());
                }

                if (!reason.NullOrEmpty())
                {
                    text += "\n\n" + "LastFactionRelationsEvent".Translate() + ": " + reason.CapitalizeFirst();
                }
            }
            else if (newType.IsNeutral())
            {
                if (previousType.IsHostile())
                {
                    text += "LetterRelationsChange_NeutralFromHostile".Translate(faction.NameColored);
                    if (faction.HasGoodwill)
                    {
                        text += "\n\n" + "LetterRelationsChange_NeutralFromHostileGoodwillDescription".Translate(faction.NameColored, faction.PlayerGoodwill.ToStringWithSign(), 0.ToStringWithSign(), (-75).ToStringWithSign(), 75.ToStringWithSign());
                    }

                    if (!reason.NullOrEmpty())
                    {
                        text += "\n\n" + "LastFactionRelationsEvent".Translate() + ": " + reason.CapitalizeFirst();
                    }
                }
                else
                {
                    text += "LetterRelationsChange_NeutralFromAlly".Translate(faction.NameColored);
                    if (faction.HasGoodwill)
                    {
                        text += "\n\n" + "LetterRelationsChange_NeutralFromAllyGoodwillDescription".Translate(faction.NameColored, faction.PlayerGoodwill.ToStringWithSign(), 0.ToStringWithSign(), (-75).ToStringWithSign(), 75.ToStringWithSign());
                    }

                    if (!reason.NullOrEmpty())
                    {
                        text += "\n\n" + "Reason".Translate() + ": " + reason.CapitalizeFirst();
                    }
                }
            }
        }

        public void MakeAlly(FactionData other)
        {
            if (!IsAlliedTo(other.faction))
            {
                allies.Add(other.faction);
            }
            if (!other.IsAlliedTo(faction))
            {
                other.allies.Add(faction);
            }

            GetAttitudeTowards(other).UpdateAttitude(this, true, "declared alliance", GlobalTargetInfo.Invalid, out _);
            other.GetAttitudeTowards(this).UpdateAttitude(other, true, "declared alliance", GlobalTargetInfo.Invalid, out _);
        }

        public bool WantAllianceWith(FactionData other, out Dictionary<String, float> reasons)
        {
            reasons = new Dictionary<String, float>();
            if (IsAlliedTo(other.faction) || IsOverlordOf(other.faction) || other.IsOverlordOf(faction))
            {
                return false;
            }

            switch (GetAttitudeTowards(other).type)
            {
                case FactionAttitudeType.Friendly:
                    reasons.Add("Friendly towards " + other.faction.Name, 30);
                    break;
                case FactionAttitudeType.Threatened:
                    reasons.Add("Threatened by " + other.faction.Name, 10);
                    break;
                case FactionAttitudeType.Hostile:
                    reasons.Add("Hostile towards " + other.faction.Name, -50);
                    break;
                case FactionAttitudeType.Furious:
                    reasons.Add("Furious towards " + other.faction.Name, -300);
                    break;
                default:
                    reasons.Add("Neutral towards " + other.faction.Name, -10);
                    break;
            }

            if (faction.GoodwillWith(other.faction) != 0)
            {
                reasons.Add("Goodwill towards " + other.faction.Name, faction.GoodwillWith(other.faction) * 0.2f);
            }

            if (MilitaryPower != other.MilitaryPower)
            {
                float milDiff = Mathf.Clamp((other.MilitaryPower - MilitaryPower) * 0.1f, -10, 10);
                if (milDiff < 0)
                {

                }
                else
                {

                }
            }

            return false;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref randomKey, "randomKey", 0);
            Scribe_Values.Look(ref loadID, "loadID", 0);
            Scribe_Collections.Look(ref allies, "allies", LookMode.Reference);
            Scribe_Collections.Look(ref vassals, "vassals", LookMode.Reference);
            Scribe_Values.Look(ref aggressiveExpansion, "aggresiveExpansion", 0);
            Scribe_References.Look(ref capital, "capital");
            Scribe_References.Look(ref faction, "faction");
            Scribe_Collections.Look(ref attitudes, "attitudes", LookMode.Deep);
            BackCompatibility.PostExposeData(this);
        }

        public override bool Equals(object obj)
        {
            return obj is FactionData data &&
                   randomKey == data.randomKey;
        }

        public override int GetHashCode()
        {
            return randomKey.GetHashCode();
        }

        public string GetUniqueLoadID()
        {
            return "FactionData_" + loadID;
        }
    }
}
