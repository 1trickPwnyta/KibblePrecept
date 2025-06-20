using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace KibblePrecept
{
    [HarmonyPatch(typeof(FoodUtility))]
    [HarmonyPatch(nameof(FoodUtility.ThoughtsFromIngesting))]
    public static class Patch_FoodUtility_ThoughtsFromIngesting
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
                    yield return new CodeInstruction(OpCodes.Call, typeof(PatchUtility_FoodUtility).Method(nameof(PatchUtility_FoodUtility.DoesNotMindBecauseKibble)));
                    yield return new CodeInstruction(OpCodes.Brtrue, instruction.operand);
                    continue;
                }

                yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility))]
    [HarmonyPatch(nameof(FoodUtility.FoodOptimality))]
    public static class Patch_FoodUtility_FoodOptimality
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool foundOptimality = false;
            bool finished = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldfld && (FieldInfo)instruction.operand == typeof(IngestibleProperties).Field(nameof(IngestibleProperties.optimalityOffsetHumanlikes)))
                {
                    foundOptimality = true;
                }
                if (foundOptimality && !finished && instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloca, 0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, typeof(PatchUtility_FoodUtility).Method(nameof(PatchUtility_FoodUtility.AdjustOptimalityOffsetForKibble)));
                    finished = true;
                    continue;
                }

                yield return instruction;
            }
        }
    }

    // Patched manually in mod constructor
    public static class Patch_FoodUtility_BestFoodSourceOnMap
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            int ingestibleCount = 0;
            Label bypassLabel = il.DefineLabel();

            int insertIndex = 0;
            List<CodeInstruction> insertInstructions = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, typeof(FoodUtility).GetNestedType("<>c__DisplayClass16_0", BindingFlags.NonPublic).Field("eater")),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldfld, typeof(Thing).Field(nameof(Thing.def))),
                new CodeInstruction(OpCodes.Call, typeof(PatchUtility_FoodUtility).Method(nameof(PatchUtility_FoodUtility.DoesNotMindBecauseKibble))),
                new CodeInstruction(OpCodes.Brtrue, bypassLabel)
            };

            List<CodeInstruction> instructionsList = instructions.ToList();
            for (int i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                if (instruction.opcode == OpCodes.Ldfld && (FieldInfo)instruction.operand == typeof(ThingDef).Field(nameof(ThingDef.ingestible)))
                {
                    if (++ingestibleCount == 3)
                    {
                        insertIndex = i - 2;
                    }
                }
                if (ingestibleCount >= 3 && instruction.opcode == OpCodes.Bgt)
                {
                    instructionsList[i + 1].labels.Add(bypassLabel);
                    break;
                }
            }

            insertInstructions[0].labels.Add(instructionsList[insertIndex].labels[0]);
            instructionsList[insertIndex].labels.Clear();
            instructionsList.InsertRange(insertIndex, insertInstructions);
            return instructionsList;
        }
    }

    public static class PatchUtility_FoodUtility
    {
        public static bool DoesNotMindBecauseKibble(this Pawn ingester, ThingDef foodDef)
        {
            return foodDef == ThingDefOf.Kibble && ingester.Ideo != null && ingester.Ideo.HasPrecept(DefDatabase<PreceptDef>.GetNamed("KibbleEating_DontMind"));
        }

        public static void AdjustOptimalityOffsetForKibble(ref float score, Pawn eater, ThingDef foodDef)
        {
            if (eater.DoesNotMindBecauseKibble(foodDef))
            {
                score -= foodDef.ingestible.optimalityOffsetHumanlikes;
            }
        }
    }
}
