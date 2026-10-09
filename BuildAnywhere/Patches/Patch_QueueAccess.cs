
using System.Reflection;
using System.Reflection.Emit;

namespace BuildAnywhere
{
    [HarmonyPatch]
    public static class Patch_QueueAccess
    {
        // queue multi-quality:
        // CraftDefinition.CanEnqueue
        // CraftDefinition.CanCraftMultiple

        // CraftComponent.CanStartCraftFromQueue
        // CraftQueueGUI
        // CraftQueueItemGUI
        // CraftComponent.CraftQueueItem
        // GUIElements.OpenCraftGUI(WorldGameObject)
        // CraftGUI.Open(WorldGameObject craftery_wgo, CraftsInventory crafts_inventory, string add_custom_tab = null)
        // CraftItemGUI.OnCraftPressed()
        // CraftComponent.EnqueueCraft(CraftDefinition craft, List<string> multiquality_ids, int amount, bool can_use_player_inventory)

        [HarmonyTargetMethod]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(CraftComponent), nameof(CraftComponent.CanInteractCraft));
            yield return AccessTools.Method(typeof(GUIElements), nameof(GUIElements.OpenCraftGUI));
            yield return AccessTools.Method(typeof(CraftControlItem), nameof(CraftControlItem.Redraw));
            yield return AccessTools.Method(typeof(CraftItemGUI), nameof(CraftItemGUI.OnCraftPressed));
            yield return AccessTools.Method(typeof(CraftComponent), nameof(CraftComponent.GetCraftAnmountCounter));

            yield return AccessTools.Method(typeof(CraftComponent), nameof(CraftComponent.ReallyUpdateComponent));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, MethodBase original, ILGenerator generator)
        {
            var code = instr.ToList();

            //// TODO: replace this with code that forces the crafting list
            //// edits the switch instruction to ignore ResourcesBasedCraft and AlchemyDecompose; results in decompose to show prayer gui and others an empty craft list?
            //if (original.DeclaringType == typeof(GUIElements))
            //{
            //    for (int i = 0; i < code.Count; i++)
            //    {
            //        if (code[i].opcode == OpCodes.Switch && code[i].operand is Label[] labels)
            //        {
            //            Debug.Log($"Patched GUIElements at {i} in {original.FullDescription()}");
            //            var label = (Label)code[i + 1].operand;
            //            labels[0] = label;
            //            labels[4] = label;
            //        }
            //    }
            //}

            // fix auto crafts not starting automatically after material becomes available by pretending HasLinkedWorker is always true
            if (original.DeclaringType == typeof(CraftComponent) && original.Name == nameof(CraftComponent.ReallyUpdateComponent))
            {
                var mi = AccessTools.Method(typeof(CraftComponent), nameof(CraftComponent.HasLinkedWorker));
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].Calls(mi))
                    {
                        Debug.Log($"Patched HasLinkedWorker at {i} in {original.FullDescription()}");
                        code.Insert(++i, new CodeInstruction(OpCodes.Call, ((Delegate)Patch0).Method));
                        return code;
                    }
                }
                return code;
            }

            // craft GUI can always be opened by pretending can_insert_zombie is always true
            var fi = AccessTools.Field(typeof(ObjectDefinition), nameof(ObjectDefinition.can_insert_zombie));
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && object.Equals(code[i].operand, fi))
                {
                    Debug.Log($"Patched is_crafting at {i} in {original.FullDescription()}");
                    code.Insert(++i, new CodeInstruction(OpCodes.Call, ((Delegate)Patch0).Method));
                }
            }
            return code;
        }

        public static bool Patch0(bool stack0)
        {
            return true;
        }
    }

    [HarmonyPatch]
    public static class Patch_QueueMultiquality
    {
        /*
         * - finishing multi quality clears all queue
         * - craftqueueitem always show bronze quality
         * 
         * CraftComponent.ProcessFinishedCraft
         * CraftComponent.DropMultiqualityOutput
         * CraftComponent.CraftReally - _multiquality_craft_result
         * 
         * class CraftComponent.CraftQueueItem
         * class CraftDefinition.MultiqualityCraftResult
         */

        //[HarmonyPatch(typeof(CraftDefinition), nameof(CraftDefinition.CanEnqueue))]
        //[HarmonyPrefix]
        public static bool CanEnqueue(ref bool __result)
        {
            __result = true;
            return false;
        }

        //[HarmonyPatch(typeof(CraftDefinition), nameof(CraftDefinition.CanCraftMultiple))]
        //[HarmonyPrefix]
        public static bool CanCraftMultiple(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}
