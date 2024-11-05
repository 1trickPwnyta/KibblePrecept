using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace KibblePrecept
{
    [HarmonyPatch(typeof(Alert_NeedMealSource))]
    [HarmonyPatch("NeedMealSource")]
    public static class Patch_Alert_NeedMealSource
    {
        public static void Postfix(Map map, ref bool __result)
        {
            if (__result)
            {
                if (map.mapPawns.FreeColonistsAndPrisonersSpawned.Where(p => !p.DevelopmentalStage.Baby()).All(p => p.Ideo != null && p.Ideo.HasPrecept(DefDatabase<PreceptDef>.GetNamed("KibbleEating_DontMind"))))
                {
                    if (map.listerBuildings.allBuildingsColonist.Any(b => b.def.recipes != null && b.def.recipes.Contains(DefDatabase<RecipeDef>.GetNamed("Make_Kibble"))))
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}
