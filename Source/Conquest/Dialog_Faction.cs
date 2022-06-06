using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Conquest
{
    public class Dialog_Faction : Window
    {
        public enum FactionTab
        {
            Diplomacy,
            Intel
        }

        private FactionData factionData;
        private FactionTab tab;
        private bool showAll;

        protected override float Margin => 0f;
        public override Vector2 InitialSize => new Vector2(600f, 760f);

        public Dialog_Faction(FactionData factionData)
        {
            this.factionData = factionData;

            draggable = true;
            doCloseX = true;
            preventCameraMotion = false;
            soundAppear = SoundDefOf.InfoCard_Open;
            soundClose = SoundDefOf.InfoCard_Close;
            StatsReportUtility.Reset();
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.InfoCard, KnowledgeAmount.Total);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Faction faction = factionData.faction;
            Rect marginRect = inRect.ContractedBy(18f);

            // Icon
            Rect iconRect = new Rect(marginRect.x, marginRect.y, 60f, 60f);
            GUI.color = faction.Color;
            GUI.DrawTexture(iconRect, faction.def.FactionIcon);
            GUI.color = Color.white;

            if (Mouse.IsOver(iconRect))
            {
                TipSignal infoTip = new TipSignal(() => faction.def.description, faction.loadID ^ 0x738AC053);
                TooltipHandler.TipRegion(iconRect, infoTip);
                Widgets.DrawHighlight(iconRect);
            }

            // Title
            Rect titleRect = new Rect(iconRect.xMax + 10f, marginRect.y + 5f, marginRect.width, 34f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, faction.Name);

            // Info
            Rect infoRect = new Rect(titleRect.x, titleRect.y + 30f, 250f, 45f);
            Text.Font = GameFont.Small;
            string info = faction.def.LabelCap + "\nCapital: " + factionData.capital.Name;
            Widgets.Label(infoRect, info);

            // Attitude
            Rect attiRect = new Rect(marginRect.xMax - 90f, titleRect.y, 90f, 75f);
            if (!faction.IsPlayer)
            {
                FactionUtility.DrawAttitudeRect(attiRect, attiRect, faction, factionData);
            }

            // Settlements
            List<Settlement> settlements = FactionUtility.GetSettlements(faction);
            if (settlements.Count() > 0)
            {
                Rect settleRect = new Rect(attiRect.x - 50f, titleRect.y, 50f, attiRect.height);

                Rect settleIconRect = new Rect(settleRect.x + 10f, settleRect.y, 30f, 30f);
                GUI.color = Color.grey;
                GUI.DrawTexture(settleIconRect, faction.def.FactionIcon);
                GUI.color = Color.white;

                Rect settleNumRect = new Rect(settleIconRect);
                settleNumRect.y = settleIconRect.yMax + 5f;
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(settleNumRect, settlements.Count().ToString());
                Text.Font = GameFont.Small;
                GenUI.ResetLabelAlign();

                if (Mouse.IsOver(settleRect))
                {
                    TooltipHandler.TipRegion(settleRect, "Settlements: " + settlements.Count());
                    Widgets.DrawHighlight(settleRect);
                }

                if (Widgets.ButtonInvisible(settleRect))
                {
                    if (Find.World.renderer.wantedMode == WorldRenderMode.None)
                    {
                        Find.World.renderer.wantedMode = WorldRenderMode.Planet;
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.FormCaravan, OpportunityType.Important);
                        Find.MainTabsRoot.EscapeCurrentTab(false);
                        SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
                    }
                    else if (Find.MainTabsRoot.OpenTab != null && Find.MainTabsRoot.OpenTab != MainButtonDefOf.Inspect)
                    {
                        Find.MainTabsRoot.EscapeCurrentTab(false);
                        SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
                    }
                    Close(false);
                    Find.WorldSelector.ClearSelection();
                    Find.WorldCameraDriver.JumpTo(settlements[0].Tile);

                    foreach (var settlement in settlements)
                    {
                        Find.WorldSelector.Select(settlement);
                    }
                }
            }

            float contentY = 0f,
                contentWidth = marginRect.width / 2 - 10f;

            // Leader
            if (faction.leader != null)
            {
                float curY = attiRect.yMax + 10f;
                ListSeparator(marginRect.x, ref curY, contentWidth, "Leader - " + faction.LeaderTitle);
                Rect leaderRect = new Rect(iconRect.x - 5f, curY + 5f, 70f, 70f);
                // Pawn portrait
                GUI.DrawTexture(leaderRect, PortraitsCache.Get(faction.leader, new Vector2(80f, 80f), Rot4.South, default, 1.22f));

                // Name
                Rect leaderNameRect = new Rect(infoRect.x, leaderRect.y, contentWidth - leaderRect.width + 5f, 25f);
                string leaderName = faction.leader.Name.ToStringFull;
                Widgets.Label(leaderNameRect, leaderName);

                // Info
                Rect leaderInfoRect = new Rect(leaderNameRect.x, leaderNameRect.yMax, leaderNameRect.width, 70f);
                leaderInfoRect.y = leaderNameRect.yMax;
                string leaderInfo = string.Join(", ", faction.leader.story.traits.allTraits.Select(t => t.LabelCap));
                Widgets.Label(leaderInfoRect, leaderInfo);

                if (leaderInfoRect.yMax > contentY)
                {
                    contentY = leaderInfoRect.yMax;
                }
            }

            // Ideo
            if (ModsConfig.IdeologyActive && faction.ideos != null)
            {
                float curY = attiRect.yMax + 10f,
                    ideoX = inRect.x + (inRect.width / 2) + 10f;
                ListSeparator(ideoX, ref curY, contentWidth, "Ideologion");
                Rect ideoRect = new Rect(ideoX, curY + 5f, 50f, 50f);
                float num3 = ideoRect.x;
                float num4 = ideoRect.y;
                if (faction.ideos.PrimaryIdeo != null)
                {
                    if (num3 + 40f > ideoRect.xMax)
                    {
                        num3 = ideoRect.x;
                        num4 += 45f;
                    }

                    Rect ideoIconRect = new Rect(num3, num4, 50f, 50f);
                    IdeoUIUtility.DoIdeoIcon(ideoIconRect, faction.ideos.PrimaryIdeo, doTooltip: true, delegate
                    {
                        IdeoUIUtility.OpenIdeoInfo(faction.ideos.PrimaryIdeo);
                    });
                    num3 += ideoIconRect.width + 5f;
                    num3 = ideoRect.x;
                    num4 += 45f;

                    // Name
                    Rect ideoNameRect = new Rect(ideoRect.xMax + 10f, ideoRect.y, contentWidth - ideoRect.width - 10f, 25f);
                    Widgets.Label(ideoNameRect, faction.ideos.PrimaryIdeo.name);

                    // Info
                    Rect ideoInfoRect = new Rect(ideoNameRect.x, ideoNameRect.yMax, ideoNameRect.width, 70f);
                    string ideoInfo = string.Join(", ", faction.ideos.PrimaryIdeo.memes.Select(m => m.LabelCap.ToString())) + "\nCulture: " + faction.ideos.PrimaryCulture.LabelCap;
                    Widgets.Label(ideoInfoRect, ideoInfo);

                    if (ideoInfoRect.yMax > contentY)
                    {
                        contentY = ideoInfoRect.yMax;
                    }
                }

                List<Ideo> minor = faction.ideos.IdeosMinorListForReading;
                int i;
                for (i = 0; i < minor.Count; i++)
                {
                    if (num3 + 22f > ideoRect.xMax)
                    {
                        num3 = ideoRect.x;
                        num4 += 27f;
                    }

                    if (num4 + 22f > ideoRect.yMax)
                    {
                        break;
                    }

                    Rect rect5 = new Rect(num3, num4, 22f, 22f);
                    IdeoUIUtility.DoIdeoIcon(rect5, minor[i], doTooltip: true, delegate
                    {
                        IdeoUIUtility.OpenIdeoInfo(minor[i]);
                    });
                    num3 += rect5.width + 5f;
                }
            }

            // Tabs
            Rect tabRect = new Rect(inRect);
            tabRect.yMin = Math.Max(contentY, attiRect.yMax) + 35f;

            TabRecord diploTab = new TabRecord("Diplomacy", delegate
            {
                tab = FactionTab.Diplomacy;
            }, tab == FactionTab.Diplomacy);
            TabRecord intelTab = new TabRecord("Intel", delegate
            {
                tab = FactionTab.Intel;
            }, tab == FactionTab.Intel);

            List<TabRecord> tabList = new List<TabRecord> { diploTab, intelTab };
            TabDrawer.DrawTabs(tabRect, tabList, maxTabWidth: 500f);

            switch (tab)
            {
                case FactionTab.Intel:
                    IntelTab(tabRect);
                    break;
                case FactionTab.Diplomacy:
                    DiploTab(tabRect);
                    break;
            }
        }

        private void DiploTab(Rect tabRect)
        {
            Faction faction = factionData.faction;

            Rect relationsLabelRect = new Rect(tabRect);
            relationsLabelRect.height = 38f;
            relationsLabelRect.xMin += 15f;
            Text.Anchor = TextAnchor.LowerLeft;
            GUI.color = Widgets.SeparatorLabelColor;
            Widgets.Label(relationsLabelRect, "Relations");

            Rect actionsLabelRect = new Rect(tabRect);
            actionsLabelRect.height = 38f;
            actionsLabelRect.xMin = (tabRect.xMin + tabRect.xMax) / 2 + 15f;
            Widgets.Label(actionsLabelRect, "Actions");
            GenUI.ResetLabelAlign();

            GUI.color = Widgets.SeparatorLineColor;
            Widgets.DrawLineHorizontal(tabRect.x + 1f, tabRect.y + 38f, tabRect.width - 2f);
            GUI.color = Color.white;

            // Relations
            Faction[] allies = factionData.allies.ToArray();
            Faction[] enemies = Find.FactionManager.AllFactionsInViewOrder.Where((Faction f) =>
            {
                return f != faction && FactionUtility.GetFactionData(f).IsHostileTo(faction) && (!f.Hidden || showAll);
            }).ToArray();
            Dictionary<string, Faction[]> relations = new Dictionary<string, Faction[]>
            {
                { "Allies:", allies },
                { "Enemies:", enemies }
            };

            Rect relationsRect = new Rect(tabRect);
            relationsRect.yMin += 38f;
            relationsRect.width = tabRect.width / 2;
            float rowY = relationsRect.y;
            int i = 0;

            foreach (var relation in relations)
            {
                if (relation.Value.Count() > 0)
                {
                    rowY += FactionUtility.DrawRelationRow(relationsRect.x, rowY, relationsRect.width, 30f, i, relation.Value, relation.Key);
                    i++;
                }
            }

            while (rowY < tabRect.yMax)
            {
                if (i % 2 == 0)
                {
                    Widgets.DrawLightHighlight(new Rect(relationsRect.x, rowY, relationsRect.width, 38f));
                }
                rowY += 38f;
                i++;
            }

            GUI.color = Widgets.SeparatorLineColor;
            Widgets.DrawLineVertical((tabRect.xMin + tabRect.xMax) / 2, tabRect.y + 39f, tabRect.height - 40f);
            GUI.color = Color.white;

            // Diplomatic actions
            Dictionary<string, Action> actions = new Dictionary<string, Action>
            {
                { "Offer Alliance", () => factionData.MakeAlly(FactionUtility.GetPlayerFactionData()) }
            };

            Rect actionsRect = new Rect(tabRect);
            actionsRect.yMin += 38f;
            actionsRect.xMin = (tabRect.xMin + tabRect.xMax) / 2;
            float actionY = actionsRect.y;

            foreach (var action in actions)
            {
                actionY += DrawActionButton(actionsRect, actionY, action.Value, action.Key);
            }
        }

        private void IntelTab(Rect tabRect)
        {

        }

        private static float DrawActionButton(Rect actionsRect, float rowY, Action action, string label)
        {
            Rect buttonRect = new Rect(actionsRect.x, rowY, actionsRect.width, 38f);
            Rect labelRect = new Rect(buttonRect);
            labelRect.xMin += 15f;

            if (Mouse.IsOver(buttonRect))
            {
                Rect highlightRect = buttonRect.ContractedBy(1f);
                highlightRect.yMax += 1;
                labelRect.xMin += 4f;
                GUI.DrawTexture(highlightRect, SolidColorMaterials.NewSolidColorTexture(new ColorInt(29, 45, 50).ToColor));
            }

            if (Widgets.ButtonInvisible(buttonRect))
            {
                action.Invoke();
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            GenUI.ResetLabelAlign();

            GUI.color = Widgets.SeparatorLineColor;
            Widgets.DrawLineHorizontal(buttonRect.x, buttonRect.yMax, buttonRect.width - 1f);
            GUI.color = Color.white;

            return buttonRect.height;
        }

        private static void ListSeparator(float x, ref float curY, float width, string label)
        {
            Color color = GUI.color;
            curY += 3f;
            GUI.color = Widgets.SeparatorLabelColor;
            Rect rect = new Rect(x, curY, width, 30f);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(rect, label);
            curY += 20f;
            GUI.color = Widgets.SeparatorLineColor;
            Widgets.DrawLineHorizontal(x, curY, width);
            curY += 2f;
            GUI.color = color;
        }
    }
}
