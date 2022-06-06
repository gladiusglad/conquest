using RimWorld;
using UnityEngine;
using Verse;

namespace Conquest
{
    public static class FactionAttitudeTypeUtility
    {
        public static Color GetColor(this FactionAttitudeType type)
        {
            switch (type)
            {
                case FactionAttitudeType.Neutral:
                    return Color.white;
                case FactionAttitudeType.Hostile:
                    return ColorLibrary.Orange;
                case FactionAttitudeType.Furious:
                    return ColorLibrary.RedReadable;
                case FactionAttitudeType.Threatened:
                    return ColorLibrary.Yellow;
                case FactionAttitudeType.Friendly:
                    return new Color(0f, 0.75f, 1f);
                case FactionAttitudeType.Ally:
                    return Color.green;
                case FactionAttitudeType.Overlord:
                    return ColorLibrary.Lavender;
                case FactionAttitudeType.Loyal:
                    return new Color(0f, 0.75f, 1f);
                case FactionAttitudeType.Disloyal:
                    return ColorLibrary.Orange;
                default:
                    return Color.white;
            }
        }
        public static string GetLabel(this FactionAttitudeType type)
        {
            switch (type)
            {
                case FactionAttitudeType.Neutral:
                    return "neutral";
                case FactionAttitudeType.Hostile:
                    return "hostile";
                case FactionAttitudeType.Furious:
                    return "furious";
                case FactionAttitudeType.Threatened:
                    return "threatened";
                case FactionAttitudeType.Friendly:
                    return "friendly";
                case FactionAttitudeType.Ally:
                    return "ally";
                case FactionAttitudeType.Overlord:
                    return "overlord";
                case FactionAttitudeType.Loyal:
                    return "loyal";
                case FactionAttitudeType.Disloyal:
                    return "disloyal";
                default:
                    return "error";
            }
        }

        public static string GetLabelCap(this FactionAttitudeType type)
        {
            switch (type)
            {
                case FactionAttitudeType.Neutral:
                    return "Neutral";
                case FactionAttitudeType.Hostile:
                    return "Hostile";
                case FactionAttitudeType.Furious:
                    return "Furious";
                case FactionAttitudeType.Threatened:
                    return "Threatened";
                case FactionAttitudeType.Friendly:
                    return "Friendly";
                case FactionAttitudeType.Ally:
                    return "Ally";
                case FactionAttitudeType.Overlord:
                    return "Overlord";
                case FactionAttitudeType.Loyal:
                    return "Loyal";
                case FactionAttitudeType.Disloyal:
                    return "Disloyal";
                default:
                    return "error";
            }
        }

        public static bool IsHostile(this FactionAttitudeType type)
        {
            switch (type)
            {
                case FactionAttitudeType.Hostile:
                case FactionAttitudeType.Furious:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNeutral(this FactionAttitudeType type)
        {
            switch (type)
            {
                case FactionAttitudeType.Neutral:
                case FactionAttitudeType.Threatened:
                case FactionAttitudeType.Friendly:
                    return true;
                default:
                    return false;
            }
        }

        public static FactionRelationKind ToRelationKind(this FactionAttitudeType type)
        {
            if (IsHostile(type)) return FactionRelationKind.Hostile;
            if (IsNeutral(type)) return FactionRelationKind.Neutral;
            return FactionRelationKind.Ally;
        }
    }
}
