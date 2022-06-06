using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Conquest
{
    public class FactionUtility
    {

        private static readonly List<int> tmpTicks = new List<int>();

        private static readonly List<int> tmpCustomGoodwill = new List<int>();

        public static WorldComponent_Conquest GetWC() => Find.World.GetComponent<WorldComponent_Conquest>();

        public static FactionData GetFactionData(Faction faction)
        {
            FactionData factionData = GetWC().GetFactionData(faction);

            if (factionData == null)
            {
                factionData = GetWC().AddNewFactionData(faction);
            }

            return factionData;
        }

        public static FactionData GetPlayerFactionData()
        {
            return GetFactionData(Faction.OfPlayer);
        }

        public static List<Settlement> GetSettlements(Faction faction)
        {
            return Find.WorldObjects.SettlementBases.Where(settlement => settlement.Faction == faction).ToList();
        }

        public static void DrawAttitudeRect(Rect rect, Rect highlightRect, Faction faction, FactionData factionData, bool highlight = true)
        {
            Rect natRect = rect.ContractedBy(25f);
            natRect.y = rect.y + 50f;
            natRect.height = 20f;

            string text = factionData.PlayerAttitudeType.GetLabelCap();
            if (faction.defeated)
            {
                text = text.Colorize(ColorLibrary.Grey);
            }

            GUI.color = factionData.PlayerAttitudeType.GetColor();
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, text);
            if (faction.HasGoodwill && !faction.def.permanentEnemy)
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(rect.x, rect.y + 20f, rect.width, rect.height), faction.PlayerGoodwill.ToStringWithSign());
                Text.Font = GameFont.Small;
            }

            GenUI.ResetLabelAlign();
            GUI.color = Color.white;
            if (Mouse.IsOver(highlightRect))
            {
                if (!Mouse.IsOver(natRect))
                {
                    TaggedString taggedString = "";
                    if (faction.def.permanentEnemy)
                    {
                        taggedString = "CurrentGoodwillTip_PermanentEnemy".Translate();
                    }
                    else if (faction.HasGoodwill)
                    {
                        taggedString = "Goodwill".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + (faction.PlayerGoodwill.ToStringWithSign() + ", " + factionData.PlayerAttitudeType.GetLabel()).Colorize(factionData.PlayerAttitudeType.GetColor());
                        TaggedString ongoingEvents = GetOngoingEvents(faction);
                        if (!ongoingEvents.NullOrEmpty())
                        {
                            taggedString += "\n\n" + "OngoingEvents".Translate().Colorize(ColoredText.TipSectionTitleColor) + ":\n" + ongoingEvents;
                        }

                        TaggedString recentEvents = GetRecentEvents(faction);
                        if (!recentEvents.NullOrEmpty())
                        {
                            taggedString += "\n\n" + "RecentEvents".Translate().Colorize(ColoredText.TipSectionTitleColor) + ":\n" + recentEvents;
                        }

                        string s = "";
                        switch (faction.PlayerRelationKind)
                        {
                            case FactionRelationKind.Ally:
                                s = "CurrentGoodwillTip_Ally".Translate(0.ToString("F0"));
                                break;
                            case FactionRelationKind.Neutral:
                                s = "CurrentGoodwillTip_Neutral".Translate((-75).ToString("F0"), 75.ToString("F0"));
                                break;
                            case FactionRelationKind.Hostile:
                                s = "CurrentGoodwillTip_Hostile".Translate(0.ToString("F0"));
                                break;
                        }

                        taggedString += "\n\n" + s.Colorize(ColoredText.SubtleGrayColor);
                    }

                    if ((string)taggedString != "")
                    {
                        TooltipHandler.TipRegion(highlightRect, taggedString);
                    }
                }

                if (highlight)
                {
                    Widgets.DrawHighlight(highlightRect);
                }
            }

            // Natural goodwill
            if (!faction.IsPlayer && faction.HasGoodwill && !faction.def.permanentEnemy)
            {
                FactionAttitudeType attitudeTypeForGoodwill = AttitudeTypeForGoodwill(faction.NaturalGoodwill);
                GUI.color = attitudeTypeForGoodwill.GetColor();
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.DrawRectFast(natRect, Color.black);
                Widgets.Label(natRect, faction.NaturalGoodwill.ToStringWithSign());
                GenUI.ResetLabelAlign();
                GUI.color = Color.white;

                if (Mouse.IsOver(natRect))
                {
                    TaggedString str = "NaturalGoodwill".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + faction.NaturalGoodwill.ToStringWithSign().Colorize(attitudeTypeForGoodwill.GetColor());
                    int goodwill = Mathf.Clamp(faction.NaturalGoodwill - 50, -100, 100);
                    int goodwill2 = Mathf.Clamp(faction.NaturalGoodwill + 50, -100, 100);
                    str += "\n" + "NaturalGoodwillRange".Translate().Colorize(ColoredText.TipSectionTitleColor) + ": " + goodwill.ToString().Colorize(AttitudeTypeForGoodwill(goodwill).GetColor()) + " " + "RangeTo".Translate() + " " + goodwill2.ToString().Colorize(AttitudeTypeForGoodwill(goodwill2).GetColor());
                    TaggedString naturalGoodwillExplanation = GetNaturalGoodwillExplanation(faction);
                    if (!naturalGoodwillExplanation.NullOrEmpty())
                    {
                        str += "\n\n" + "AffectedBy".Translate().Colorize(ColoredText.TipSectionTitleColor) + "\n" + naturalGoodwillExplanation;
                    }

                    str += "\n\n" + "NaturalGoodwillDescription".Translate(1.25f.ToStringPercent()).Colorize(ColoredText.SubtleGrayColor);
                    TooltipHandler.TipRegion(natRect, str);
                    Widgets.DrawHighlight(natRect);
                }
            }
        }

        public static float DrawRelationRow(float x, float rowY, float width, float size, int i, Faction[] factions, string label)
        {
            if (factions.Count() == 0) return 0;
            float allyHeight = (float)Math.Ceiling(factions.Count() * size / (width - 90f)) * size + 8f;
            Rect allyRect = new Rect(x, rowY, width, allyHeight);

            if (i % 2 == 0)
            {
                Widgets.DrawLightHighlight(allyRect);
            }

            Rect allyLabelRect = new Rect(allyRect);
            allyLabelRect.x += 15f;
            allyLabelRect.width = 70f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(allyLabelRect, label);
            GenUI.ResetLabelAlign();

            float allyX = allyLabelRect.xMax,
                relationY = rowY + 4f;
            for (int j = 0; j < factions.Length; j++)
            {
                if (allyX >= allyRect.xMax - size)
                {
                    allyX = allyLabelRect.xMax;
                    relationY += size;
                }

                FactionUtility.DrawFactionIconWithTooltip(new Rect(allyX, relationY + 2f, size * 5 / 6, size * 5 / 6), factions[j]);
                allyX += size;
            }

            return allyHeight;
        }

        private static TaggedString GetRecentEvents(Faction other)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<HistoryEventDef> allDefsListForReading = DefDatabase<HistoryEventDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                int recentCountWithinTicks = Find.HistoryEventsManager.GetRecentCountWithinTicks(allDefsListForReading[i], 3600000, other);
                if (recentCountWithinTicks <= 0)
                {
                    continue;
                }

                Find.HistoryEventsManager.GetRecent(allDefsListForReading[i], 3600000, tmpTicks, tmpCustomGoodwill, other);
                int num = 0;
                for (int j = 0; j < tmpTicks.Count; j++)
                {
                    num += tmpCustomGoodwill[j];
                }

                if (num != 0)
                {
                    string text = "- " + allDefsListForReading[i].LabelCap;
                    if (recentCountWithinTicks != 1)
                    {
                        text = text + " x" + recentCountWithinTicks;
                    }

                    text = text + ": " + num.ToStringWithSign().Colorize((num >= 0) ? FactionRelationKind.Ally.GetColor() : FactionRelationKind.Hostile.GetColor());
                    stringBuilder.AppendInNewLine(text);
                }
            }

            return stringBuilder.ToString();
        }

        public static FactionAttitudeType AttitudeTypeForGoodwill(int goodwill)
        {
            if (goodwill <= -75)
            {
                return FactionAttitudeType.Hostile;
            }

            if (goodwill >= 75)
            {
                return FactionAttitudeType.Friendly;
            }

            return FactionAttitudeType.Neutral;
        }

        private static TaggedString GetOngoingEvents(Faction other)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<GoodwillSituationManager.CachedSituation> situations = Find.GoodwillSituationManager.GetSituations(other);
            for (int i = 0; i < situations.Count; i++)
            {
                if (situations[i].maxGoodwill < 100)
                {
                    string str = "- " + situations[i].def.Worker.GetPostProcessedLabelCap(other);
                    str = str + ": " + (situations[i].maxGoodwill.ToStringWithSign() + " " + "max".Translate()).Colorize(FactionRelationKind.Hostile.GetColor());
                    stringBuilder.AppendInNewLine(str);
                }
            }

            return stringBuilder.ToString();
        }

        private static TaggedString GetNaturalGoodwillExplanation(Faction other)
        {
            StringBuilder stringBuilder = new StringBuilder();
            List<GoodwillSituationManager.CachedSituation> situations = Find.GoodwillSituationManager.GetSituations(other);
            for (int i = 0; i < situations.Count; i++)
            {
                if (situations[i].naturalGoodwillOffset != 0)
                {
                    string str = "- " + situations[i].def.Worker.GetPostProcessedLabelCap(other);
                    str = str + ": " + situations[i].naturalGoodwillOffset.ToStringWithSign().Colorize((situations[i].naturalGoodwillOffset >= 0) ? FactionRelationKind.Ally.GetColor() : FactionRelationKind.Hostile.GetColor());
                    stringBuilder.AppendInNewLine(str);
                }
            }

            return stringBuilder.ToString();
        }

        public static void DrawFactionIconWithTooltip(Rect r, Faction faction)
        {
            FactionData factionData = GetFactionData(faction);
            FactionAttitude attitudeToPlayer = factionData.TryGetAttitudeTowards(Faction.OfPlayer);
            GUI.color = faction.Color;
            GUI.DrawTexture(r, faction.def.FactionIcon);
            GUI.color = Color.white;
            if (Mouse.IsOver(r))
            {

                TipSignal tip = new TipSignal(() => faction.Name.Colorize(ColoredText.TipSectionTitleColor) + "\n" + faction.def.LabelCap.Resolve() + "\n" +
                        (faction.IsPlayer ? "This is you" : $"{attitudeToPlayer.type.GetLabelCap()} ({faction.PlayerGoodwill.ToStringWithSign()})".Colorize(attitudeToPlayer.type.GetColor())),
                        faction.loadID ^ 0x738AC053);
                TooltipHandler.TipRegion(r, tip);
                Widgets.DrawHighlight(r);
            }

            if (Widgets.ButtonInvisible(r, doMouseoverSound: false))
            {
                Find.WindowStack.Add(new Dialog_Faction(factionData));
            }
        }
    }
}
