using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace KibblePrecept
{
    [HarmonyPatch(typeof(FoodUtility))]
    [HarmonyPatch(nameof(FoodUtility.ThoughtsFromIngesting))]
    public static class Patch_FoodUtility
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool foundTasteThought = false;
            bool finished = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldfld && (FieldInfo)instruction.operand == typeof(IngestibleProperties).Field(nameof(IngestibleProperties.tasteThought)))
                {
                    foundTasteThought = true;
                }
                if (!finished && foundTasteThought && instruction.opcode == OpCodes.Brfalse_S)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, typeof(PatchUtility_FoodUtility).Method(nameof(PatchUtility_FoodUtility.ShouldIgnoreKibbleTasteThought)));
                    yield return new CodeInstruction(OpCodes.Brtrue, instruction.operand);
                    continue;
                }

                yield return instruction;
            }
        }
    }

    public static class PatchUtility_FoodUtility
    {
        public static bool ShouldIgnoreKibbleTasteThought(Pawn ingester, ThingDef foodDef)
        {
            return foodDef == ThingDefOf.Kibble && ingester.Ideo != null && ingester.Ideo.HasPrecept(DefDatabase<PreceptDef>.GetNamed("KibbleEating_DontMind"));
        }
    }
}
