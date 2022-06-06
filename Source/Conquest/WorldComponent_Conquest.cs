using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Conquest
{
    public class WorldComponent_Conquest : WorldComponent
    {
        private bool setup;

        private List<FactionData> factions;

        public List<FactionData> Factions
        {
            get
            {
                if (factions == null)
                {
                    factions = new List<FactionData>();
                }
                return factions;
            }
            set
            {
                factions.Clear();
                foreach (FactionData faction in value)
                {
                    factions.Add(faction);
                }
            }
        }

        public WorldComponent_Conquest(World world) : base(world) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref setup, "setup", false);
            Scribe_Collections.Look(ref factions, "factionData", LookMode.Deep);
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            int ticks = Find.TickManager.TicksGame;

            if (setup == false)
            {
                if (ticks > 5)
                {
                    Setup();
                    setup = true;
                }
            }
            else
            {
                if (ticks % 120 == 0)
                {
                    CheckNewFactions();
                }
            }
        }

        public void Setup()
        {
            List<Faction> allFactions = Find.World.factionManager.AllFactions.ToList<Faction>();

            foreach (Faction faction in allFactions)
            {
                AddFactionData(faction);
            }

            UpdateAllFactionAttitudes();
        }

        public void CheckNewFactions()
        {
            List<Faction> allFactions = Find.World.factionManager.AllFactions.ToList<Faction>();
            if (allFactions.Count > factions.Count)
            {
                foreach (Faction faction in allFactions)
                {
                    if (GetFactionData(faction) == null)
                    {
                        AddNewFactionData(faction);
                    }
                }
            }
        }

        public FactionData AddNewFactionData(Faction faction)
        {
            FactionData factionData = AddFactionData(faction);
            UpdateAllFactionAttitudes();
            return factionData;
        }

        public FactionData AddFactionData(Faction faction)
        {
            FactionData factionData = GetFactionData(faction);

            if (factionData == null)
            {
                factionData = new FactionData(faction);
                factions.Add(factionData);
            }

            return factionData;
        }

        public FactionData GetFactionData(Faction faction)
        {
            foreach (FactionData factionData in Factions)
            {
                if (factionData.faction == faction)
                {
                    return factionData;
                }
            }
            return null;
        }

        public void UpdateAllFactionAttitudes()
        {
            foreach (FactionData factionData in factions)
            {
                factionData.UpdateAllAttitudes();
            }
        }
    }
}
