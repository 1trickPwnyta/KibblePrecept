using HarmonyLib;
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

            Log.Message($"[{PACKAGE_NAME}] Loaded.");
        }
    }
}
