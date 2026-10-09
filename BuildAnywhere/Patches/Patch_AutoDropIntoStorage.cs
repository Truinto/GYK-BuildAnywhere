using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

namespace BuildAnywhere
{
    [HarmonyPatch]
    public static class Patch_AutoDropIntoStorage3
    {
        public static Regex BlackListItems = new("^(water|faith|pail_water|jug_milk|drop_|foodstuff_|goo)", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        public static Regex BlackListBuilding = new("^(refugee_|tavern_)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.DropItems))]
        [HarmonyPrepare]
        public static bool Prepare()
        {
            return Settings.Instance.AutoDropIntoStorage;
        }

        private static List<Item> _Items = new();
        private static List<Item> _CantInsert = new();
        [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.DropItems))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        public static bool StoreDropItems(List<Item> items, Direction direction, WorldGameObject __instance)
        {
            _Items.Clear();
            _CantInsert.Clear();

            // remove blacklisted _items
            _Items.AddRange(items);
            for (int i = _Items.Count - 1; i >= 0; i--)
            {
                var item = _Items[i];
                if (item.is_tech_point)
                {
                    _CantInsert.Add(item);
                    _Items.RemoveAt(i);
                }
                else if (BlackListItems.IsMatch(item.id))
                {
                    _CantInsert.Add(item);
                    _Items.RemoveAt(i);
                    //Debug.Log($"Patch_AutoDropIntoStorage3 blacklisted item {item.id}");
                }
                else if (BlackListBuilding.IsMatch(__instance.obj_id))
                {
                    _CantInsert.Add(item);
                    _Items.RemoveAt(i);
                    //Debug.Log($"Patch_AutoDropIntoStorage3 blacklisted building {item.id}");
                }
            }

            // try store in chests
            //Debug.Log($"Patch_AutoDropIntoStorage3 try put {_items.Join()}");
            __instance.PutToAllPossibleInventories(_Items, out var cant_insert);
            if (cant_insert != null && cant_insert.Count > 0)
            {
                _CantInsert.AddRange(cant_insert);
                //Main.ShowMessage("", Vector3.zero, EffectBubblesManager.BubbleColor.Relation, 3f, true, SpeechBubbleGUI.SpeechBubbleType.Think, SmartSpeechEngine.VoiceID.None);
            }

            // drop leftovers
            if (_CantInsert.Count > 0)
            {
                var pos = __instance.GetDropPos();
                foreach (var item in _CantInsert)
                {
                    //Debug.Log($"Patch_AutoDropIntoStorage3 manual {item.id}");
                    __instance.DropItem(item, direction, pos);
                }
            }

            return false;
        }


        [HarmonyPatch(typeof(CraftComponent), nameof(CraftComponent.ProcessFinishedCraft))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> StoreMultiQualityItems(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var code = instr.ToList();

            var mi_drop = AccessTools.Method(typeof(CraftComponent), nameof(CraftComponent.DropMultiqualityOutput));

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Calls(mi_drop))
                {
                    code[i].opcode = OpCodes.Call;
                    code[i].operand = ((Delegate)Patch1).Method;
                    break;
                }
            }

            return code;
            //var tool = new TranspilerTool(instr, generator, original);
            //tool.ReplaceAllCalls(typeof(CraftComponent), nameof(CraftComponent.DropMultiqualityOutput), Patch1);
            //return tool;

        }
        public static List<Item> Patch1(CraftComponent __instance, bool do_not_really_drop)
        {
            if (BlackListBuilding.IsMatch(__instance.wgo.obj_id))
                return __instance.DropMultiqualityOutput(do_not_really_drop);

            var items = __instance.DropMultiqualityOutput(true);
            __instance.wgo.DropItems(items);
            return new();
        }
    }
}
