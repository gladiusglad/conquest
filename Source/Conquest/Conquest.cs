using HarmonyLib;
using Verse;

namespace Conquest
{
    [StaticConstructorOnStartup]
    public static class Conquest
    {
        static Conquest()
        {
            var harmony = new Harmony("Glad.Conquest");
            harmony.PatchAll();
        }
    }
}
