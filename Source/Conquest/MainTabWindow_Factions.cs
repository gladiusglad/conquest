using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Conquest
{
    internal class MainTabWindow_Factions : MainTabWindow
    {

        private Vector2 scrollPosition;

        private float scrollViewHeight;

        private Faction scrollToFaction;

        private static bool showAll;

        private static readonly List<Faction> visibleFactions = new List<Faction>();

        public override void PreOpen()
        {
            base.PreOpen();
            this.scrollToFaction = null;
        }

        public void ScrollToFaction(Faction faction)
        {
            this.scrollToFaction = faction;
        }

        public override void DoWindowContents(Rect fillRect)
        {
            DoWindowContents(fillRect, ref this.scrollPosition, ref this.scrollViewHeight, this.scrollToFaction);
            if (this.scrollToFaction != null)
            {
                this.scrollToFaction = null;
            }
        }
        public static void DoWindowContents(Rect fillRect, ref Vector2 scrollPosition, ref float scrollViewHeight, Faction scrollToFaction = null)
        {
            Rect rect = new Rect(0f, 0f, fillRect.width, fillRect.height);
            GUI.BeginGroup(rect);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            if (Prefs.DevMode)
            {
                Widgets.CheckboxLabeled(new Rect(rect.width - 120f, 0f, 120f, 24f), "Dev: Show all", ref showAll);
            }
            else
            {
                showAll = false;
            }

            Rect outRect = new Rect(0f, 50f, rect.width, rect.height - 50f);
            Rect rect2 = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
            visibleFactions.Clear();
            foreach (Faction item in Find.FactionManager.AllFactionsInViewOrder)
            {
                if ((!item.IsPlayer && !item.Hidden) || showAll)
                {
                    visibleFactions.Add(item);
                }
            }

            if (visibleFactions.Count > 0)
            {
                outRect.yMin += Text.LineHeight;
                Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
                float num = 0f;
                foreach (Faction visibleFaction in visibleFactions)
                {
                    if ((!visibleFaction.IsPlayer && !visibleFaction.Hidden) || showAll)
                    {
                        GUI.color = new Color(1f, 1f, 1f, 0.2f);
                        Widgets.DrawLineHorizontal(0f, num, rect2.width);
                        GUI.color = Color.white;
                        if (visibleFaction == scrollToFaction)
                        {
                            scrollPosition.y = num;
                        }

                        num += DrawFactionRow(visibleFaction, num, rect2);
                    }
                }

                if (Event.current.type == EventType.Layout)
                {
                    scrollViewHeight = num;
                }

                Widgets.EndScrollView();
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "NoFactions".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
            }

            GUI.EndGroup();
        }

        private static float DrawFactionRow(Faction faction, float rowY, Rect fillRect)
        {
            FactionData factionData = FactionUtility.GetFactionData(faction);

            float startY = rowY + 10f;

            // Faction info
            Rect infoRect = new Rect(120f, startY, 250f, 80f);
            Rect iconRect = new Rect(24f, startY + 4f, 42f, 42f);

            // Faction icon
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            FactionUtility.DrawFactionIconWithTooltip(iconRect, faction);

            if (faction.leader != null)
            {
                Rect leaderRect = new Rect(iconRect.xMax, startY + 5f, 50f, 50f);
                GUI.DrawTexture(leaderRect, PortraitsCache.Get(faction.leader, new Vector2(42f, 42f), Rot4.South, default, 1.22f));
            }

            // Faction info label
            Text.Font = GameFont.Medium;
            string factionName = faction.Name.CapitalizeFirst();
            Widgets.Label(infoRect, factionName);
            Text.Font = GameFont.Small;
            Rect labelRect = new Rect(120f, startY + 30f, 250f, 80f);
            string label = faction.def.LabelCap + "\n" + ((faction.leader != null) ? ("Leader: " + faction.leader.Name.ToStringFull) : "");
            Widgets.Label(labelRect, label);

            // Ideologion
            Rect ideoRect = new Rect(infoRect.xMax, startY, 60f, 80f);
            if (ModsConfig.IdeologyActive && faction.ideos != null)
            {
                float num3 = ideoRect.x;
                float num4 = ideoRect.y;
                if (faction.ideos.PrimaryIdeo != null)
                {
                    if (num3 + 40f > ideoRect.xMax)
                    {
                        num3 = ideoRect.x;
                        num4 += 45f;
                    }

                    Rect rect4 = new Rect(num3, num4, 40f, 40f);
                    IdeoUIUtility.DoIdeoIcon(rect4, faction.ideos.PrimaryIdeo, doTooltip: true, delegate
                    {
                        IdeoUIUtility.OpenIdeoInfo(faction.ideos.PrimaryIdeo);
                    });
                    num3 += rect4.width + 5f;
                    num3 = ideoRect.x;
                    num4 += 45f;
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

            // Attitude
            Rect attiHighlightRect = new Rect(ideoRect.xMax, rowY, 100f, 90f);
            Rect attiRect = new Rect(ideoRect.xMax, startY, 100f, 80f);
            if (!faction.IsPlayer)
            {
                FactionUtility.DrawAttitudeRect(attiRect, attiHighlightRect, faction, factionData);
            }

            // Action buttons
            Rect diploRect = new Rect(attiRect.xMax + 10f, startY, 100f, 28f);
            if (Widgets.ButtonText(diploRect, "Diplomacy", true, true, true))
            {
                Find.WindowStack.Add(new Dialog_Faction(factionData));
            }

            Text.Anchor = TextAnchor.MiddleLeft;
            float relationY = rowY,
                relationWidth = fillRect.width - diploRect.xMax;

            Faction[] allies = Find.FactionManager.AllFactionsInViewOrder.Where((Faction f) =>
            {
                return f != faction && factionData.IsAlliedTo(f) && (!f.Hidden || showAll);
            }).ToArray();

            Faction[] enemies = Find.FactionManager.AllFactionsInViewOrder.Where((Faction f) =>
            {
                return f != faction && FactionUtility.GetFactionData(f).IsHostileTo(faction) && (!f.Hidden || showAll);
            }).ToArray();

            // Allies
            if (allies.Count() > 0)
            {
                relationY += FactionUtility.DrawRelationRow(diploRect.xMax + 20f, relationY, relationWidth, 26f, 1, allies, "Allies:");
            }

            // Enemies
            if (enemies.Count() > 0)
            {
                relationY += FactionUtility.DrawRelationRow(diploRect.xMax + 20f, relationY, relationWidth, 26f, allies.Count() > 0 ? 0 : 1, enemies, "Enemies:");
            }

            float rowHeight = Mathf.Max(90f, relationY - rowY);

            // Highlight
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            Widgets.DrawHighlightIfMouseover(new Rect(0f, rowY, fillRect.width, rowHeight));

            GenUI.ResetLabelAlign();

            return rowHeight;
        }

        public static void DrawRelatedFactionInfo(Rect rect, Faction faction, ref float curY)
        {
            Text.Anchor = TextAnchor.LowerRight;
            curY += 10f;
            FactionRelationKind playerRelationKind = faction.PlayerRelationKind;
            string text = faction.Name.CapitalizeFirst() + "\n" + "goodwill".Translate().CapitalizeFirst() + ": " + faction.PlayerGoodwill.ToStringWithSign();
            GUI.color = Color.gray;
            Rect rect2 = new Rect(rect.x, curY, rect.width, Text.CalcHeight(text, rect.width));
            Widgets.Label(rect2, text);
            curY += rect2.height;
            GUI.color = playerRelationKind.GetColor();
            Rect rect3 = new Rect(rect2.x, curY - 7f, rect2.width, 25f);
            Widgets.Label(rect3, playerRelationKind.GetLabelCap());
            curY += rect3.height;
            GUI.color = Color.white;
            GenUI.ResetLabelAlign();
        }
    }
}
