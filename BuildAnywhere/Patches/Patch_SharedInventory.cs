using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BuildAnywhere
{
    [HarmonyPatch]
    public static class Patch_SharedInventory
    {
        public static MultiInventory GlobalInventory = new();
        public static bool IsRecalculateAll;

        [HarmonyPatch(typeof(WorldZone), nameof(WorldZone.RecalculateAllZones))]
        [HarmonyPrefix]
        public static void RecalculateAllPre()
        {
            IsRecalculateAll = true;
        }

        [HarmonyPatch(typeof(WorldZone), nameof(WorldZone.RecalculateAllZones))]
        [HarmonyPostfix]
        public static void RecalculateAllPost()
        {
            IsRecalculateAll = false;
            Recalculate();
        }

        [HarmonyPatch(typeof(WorldZone), nameof(WorldZone.Recalculate))]
        [HarmonyPostfix]
        public static void Recalculate()
        {
            if (IsRecalculateAll)
                return;

            var list = GlobalInventory._inventories;
            list.Clear();

            foreach (var zone in WorldZone._all_zones)
            {
                foreach (var wgo in zone._wgos)
                {
                    if (wgo.IsWorker() || wgo.is_player)
                        continue;

                    if (wgo.obj_def.open_in_multiinventory)
                        list.Add(new Inventory(wgo) { _name = $"{zone.id}:{wgo.obj_id}", _name_is_set = true });
                }
            }

            var player_inventory = new Inventory(MainGame.me.player);
            list.Add(player_inventory);
            foreach (var item in player_inventory.data.inventory)
            {
                if (item != null && !item.IsEmpty() && item.is_bag)
                    list.Add(new Inventory(item, "player_bag"));
            }

            Debug.Log($"Recalc shared inventory: {list.Join(a => a.name)}");
        }

        [HarmonyPatch(typeof(CraftComponent), nameof(CraftComponent.CanStartCraftFromQueue))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> OverrideInventoryFormQueue(IEnumerable<CodeInstruction> instr) // TODO: check do I need to patch CraftReally?
        {
            var code = instr.ToList();

            var mi = AccessTools.Method(typeof(WorldGameObject), nameof(WorldGameObject.GetMultiInventory));

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(mi))
                {
                    code[i].opcode = OpCodes.Call;
                    code[i].operand = ((Delegate)Patch).Method;
                    Debug.Log($"Patched CanStartCraftFromQueue");
                }
            }

            return code;
        }

        public static MultiInventory Patch(WorldGameObject instance, List<WorldGameObject> exceptions = null, string force_world_zone = "", MultiInventory.PlayerMultiInventory player_mi = MultiInventory.PlayerMultiInventory.DontChange, bool include_toolbelt = false, bool sortWGOS = false, bool include_bags = false)
        {
            //Debug.Log($"OverrideInventoryFormQueue returning {GlobalInventory._inventories.Join(a => a.name)}");
            return GlobalInventory;
        }
    }
}
