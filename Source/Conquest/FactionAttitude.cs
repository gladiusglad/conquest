using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace Conquest
{
    public class FactionAttitude : IExposable
    {
        public FactionAttitudeType type = FactionAttitudeType.Neutral;

        public FactionData other;

        public int trust = 50;

        public void ExposeData()
        {
            Scribe_References.Look(ref other, "other");
            Scribe_Values.Look(ref type, "attitudeType", FactionAttitudeType.Neutral);
            Scribe_Values.Look(ref trust, "trust", 50);
            BackCompatibility.PostExposeData(this);
        }

        public override int GetHashCode()
        {
            return -677964483 + EqualityComparer<FactionData>.Default.GetHashCode(other);
        }

        public void UpdateAttitude(FactionData factionData, bool canSendLetter, string reason, GlobalTargetInfo lookTarget, out bool sentLetter)
        {
            int num = factionData.faction.GoodwillWith(other.faction);
            FactionAttitudeType previous = type;
            sentLetter = false;

            if (factionData.IsOverlordOf(other.faction))
            {
                if (type != FactionAttitudeType.Disloyal && num <= -50)
                {
                    type = FactionAttitudeType.Disloyal;
                    factionData.Notify_AttitudeChanged(other, previous, type, canSendLetter, reason, lookTarget, out sentLetter);
                }
                if (type != FactionAttitudeType.Loyal && num >= 50)
                {
                    type = FactionAttitudeType.Loyal;
                    factionData.Notify_AttitudeChanged(other, previous, type, canSendLetter, reason, lookTarget, out sentLetter);
                }
            }
            else if (other.IsOverlordOf(factionData.faction))
            {
                if (type != FactionAttitudeType.Overlord)
                {
                    type = FactionAttitudeType.Overlord;
                    factionData.Notify_AttitudeChanged(other, previous, type, canSendLetter, reason, lookTarget, out sentLetter);
                }
            }
            else if (factionData.IsAlliedTo(other.faction))
            {
                if (type != FactionAttitudeType.Ally)
                {
                    type = FactionAttitudeType.Ally;
                    factionData.Notify_AttitudeChanged(other, previous, type, canSendLetter, reason, lookTarget, out sentLetter);
                }
            }
            else if (factionData.IsAThreat(other))
            {
                if (!(type == FactionAttitudeType.Hostile || type == FactionAttitudeType.Furious || type == FactionAttitudeType.Threatened))
                {
                    type = FactionAttitudeType.Threatened;
                    factionData.Notify_AttitudeChanged(other, previous, type, canSendLetter, reason, lookTarget, out sentLetter);
                }
            }
            else
            {
                if (!(type == FactionAttitudeType.Hostile || type == FactionAttitudeType.Furious) && num <= -75)
                {
                    type = FactionAttitudeType.Hostile;
                    factionData.Notify_AttitudeChanged(other, previous, type, canSendLetter, reason, lookTarget, out sentLetter);
                }
                if ((type == FactionAttitudeType.Hostile || type == FactionAttitudeType.Furious) && num >= 0)
                {
                    type = FactionAttitudeType.Neutral;
                    factionData.Notify_AttitudeChanged(other, previous, type, canSendLetter, reason, lookTarget, out sentLetter);
                }
                if (type != FactionAttitudeType.Friendly && num >= 75)
                {
                    type = FactionAttitudeType.Friendly;
                    factionData.Notify_AttitudeChanged(other, previous, type, canSendLetter, reason, lookTarget, out sentLetter);
                }
                if (type == FactionAttitudeType.Friendly && num <= 0)
                {
                    type = FactionAttitudeType.Neutral;
                    factionData.Notify_AttitudeChanged(other, previous, type, canSendLetter, reason, lookTarget, out sentLetter);
                }
            }
        }

        public void UpdateAttitude(FactionData factionData)
        {
            UpdateAttitude(factionData, false, null, GlobalTargetInfo.Invalid, out _);
        }

        public override string ToString()
        {
            return string.Concat(new object[]
               {
                "(",
                other,
                ", type=",
                type,
                ")"
               });
        }
    }
}
