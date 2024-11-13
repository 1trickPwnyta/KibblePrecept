using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace KibblePrecept
{
    public class KibblePreceptMod : Mod
    {
        public const string PACKAGE_ID = "kibbleprecept.1trickPwnyta";
        public const string PACKAGE_NAME = "Kibble Precept";

        public KibblePreceptMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony(PACKAGE_ID);
            harmony.PatchAll();
            harmony.Patch(typeof(FoodUtility).GetNestedType("<>c__DisplayClass14_0", BindingFlags.NonPublic).Method("<BestFoodSourceOnMap>b__0"), null, null, typeof(Patch_FoodUtility_BestFoodSourceOnMap).Method(nameof(Patch_FoodUtility_BestFoodSourceOnMap.Transpiler)));

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }
    }
}
