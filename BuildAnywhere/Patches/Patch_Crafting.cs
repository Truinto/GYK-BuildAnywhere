using System.Reflection;
using System.Reflection.Emit;

namespace BuildAnywhere
{
    [HarmonyPatch]
    public static class Patch_Crafting
    {
        [HarmonyPatch(typeof(GameBalance), nameof(GameBalance.LoadGameBalance))]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void EditCrafting()
        {
            try
            {
                // TODO: check dont_close_window_on_craft
                //craft.dont_close_window_on_craft = true;

                // change crafts
                for (int i = 0; i < GameBalance.me.craft_data.Count; i++)
                {
                    CraftDefinition craft = GameBalance.me.craft_data[i];
                    if (craft.id == "peat_from_waste")
                    {
                        craft.craft_type = CraftDefinition.CraftType.None;
                    }

                    else if (craft.id.StartsWith("marble_plate_1"))
                    {
                        craft.needs.RemoveAll(a => a.id == "spike_1");
                    }

                    else if (craft.id == "garden_wheat_grow_desk_planting")
                    {
                        var clone = craft.Clone();
                        clone.can_craft_always = true;
                        clone.id = "garden_cannabis_grow_desk_planting";
                        clone.icon = "i_hamp_crop";

                        clone.needs[0] = HelperGYK.NewItem("hamp_seed:1", 24);
                        clone.output[0] = HelperGYK.NewItem("hamp_crop:1", 30, "(26+4*WGOpar(\"lvl\"))", "(38+4*WGOpar(\"lvl\"))");
                        clone.output[1] = HelperGYK.NewItem("hamp_seed:1", 30, "24", "32");

                        clone.output_res_wgo._res_type[0] = "hamp";
                        clone.output_set_res_wgo._res_type[0] = "hamp";
                        clone.set_when_cancelled._res_type[0] = "hamp";

                        GameBalance.me.craft_data.Insert(++i, clone);
                        //Debug.Log($"sanity 1={clone.Equals(craft)} 2={clone.output.Equals(craft)} 3={clone.output[0].id} 4={craft.output[0].id} 5={clone.output[1].id} 6={craft.output[1].id}");

                        // TODO: mushrooms, red mushrooms, red flower, yellow flower, white flower

                    }

                    else if (craft.id == "water_pumping")
                    {
                        craft.output_to_wgo.Add(HelperGYK.NewItem("clay", 1, chance: "0.1"));
                        craft.output_to_wgo.Add(HelperGYK.NewItem("sand_river", 1, chance: "0.5"));
                    }

                    else if (craft.craft_in.Contains("table_book_constr"))
                    {
                        craft.craft_in.Add("alchemy_workbench_zombie");
                    }

                    else if (craft.craft_in.Contains("mf_preparation_2"))
                    {
                        craft.craft_time_is_zero = true;
                        craft.is_auto = true;
                        craft.craft_time.FromString("0");
                    }

                    else if (craft.craft_in.Contains("desk_2"))
                    {
                        craft.craft_in.Add("zombie_pulpit");
                    }

                    else if (craft.id is "candelabrum_3_to_3_3" or "wall_candelabrum_3_to_3_3")
                    {
                        craft.craft_after_finish = "";
                    }

                    else if (craft.id == "pail_wet_paper")
                    {
                        craft.needs[0] = HelperGYK.NewItem("water", 10);
                    }

                    else if (craft.id is "firewood" or "spike_1")
                    {
                        craft.craft_in.Add("mf_saw_1");
                    }

                    else if (craft.id == "mf_furnace_2_fuel_coal")
                    {
                        craft.craft_in.Add("oven");
                        craft.craft_in.Add("tavern_oven");
                    }

                    else if (craft.id is "alchemy_1_intestine" or "alchemy_2_brain" or "alchemy_3_heart")
                    {
                        craft.needs[0].multiquality_items.RemoveAll(a => a.EndsWith("0_2") || a.EndsWith("_3")); // disallow best zombie parts from recipes
                    }

                    else if (craft.id == "crematorium_corp_burning")
                    {
                        craft.output[0].value = 5;
                        craft.output[0].min_value.FromString("5");
                    }

                    else if (craft.id == "mine_zombie_bench_coal_production")
                    {
                        craft.output.Add(HelperGYK.NewItem("nugget_silver", 1, chance: "Ppar(\"p_t_silver_ore\")*(0.05+Ppar(\"p_miner\")*0.05)"));
                        craft.output.Add(HelperGYK.NewItem("nugget_gold", 1, chance: "Ppar(\"p_t_silver_ore\")*(0.05+Ppar(\"p_miner\")*0.05)"));
                    }

                    else if (craft.id == "mine_zombie_bench_iron_production")
                    {
                        craft.output.Add(HelperGYK.NewItem("pyrite", 1, 
                            min_value: "(1+Ppar(\"p_miner\"))", 
                            max_value: "(2+2*Ppar(\"p_miner\"))", 
                            chance: "Ppar(\"p_t_pyrite\")*(0.10+Ppar(\"p_miner\")*0.10)"));
                        craft.output.Add(HelperGYK.NewItem("sulfur", 1,
                            min_value: "(1+Ppar(\"p_miner\"))",
                            max_value: "(2+2*Ppar(\"p_miner\"))",
                            chance: "Ppar(\"p_t_sulfur\")*(0.10+Ppar(\"p_miner\")*0.10)"));
                        craft.output.Add(HelperGYK.NewItem("lifestone", 1,
                            min_value: "(1+Ppar(\"p_miner\"))",
                            max_value: "(2+2*Ppar(\"p_miner\"))",
                            chance: "Ppar(\"p_t_lifestone\")*(0.10+Ppar(\"p_miner\")*0.10)"));
                    }

                    craft.craft_time?.FromString(craft.craft_time.GetRawExpressionString().Replace("WGOpar(\"buff_plant\")", "Ppar(\"buff_plant\")"));
                }

                // add plant crafts
                //keeper_room_plant_yellow_1 - mf_anvil_1; honey_production
                //interaction_type = 1

                // add organe enhance crafts
                addCraft("blood", 1, -1, 1, 3); // blood:   +1 -1
                addCraft("fat", 1, -1, 1, 3);   // fat:     +1 -1
                addCraft("skin", -1, 1, -1, 3); // skin:    -1 +1
                addCraft("flesh", 0, 1, 0, 3);  // flesh:   +0 +1
                foreach (string organ in new string[] { "brain", "heart", "intestine" })
                {
                    for (int i = 0; i < 3; i++)
                    {
                        addCraft(organ, 0, i, 0, 3);
                        addCraft(organ, 1, i, 0, 3);
                        addCraft(organ, 2, i, 0, 3);
                        addCraft(organ, 3, i, 0, 3);
                    }
                }

                static void addCraft(string id, int in_red, int in_white, int out_red, int out_white)
                {
                    bool is_white = in_white < out_white;
                    int cost = Settings.Instance.CheapOrganes ? 1 : getCost(in_white, out_white) + getCost(in_red, out_red);

                    var craft = new CraftDefinition()
                    {
                        id = $"organ_enhance_{id}_{in_red}_{in_white}_{out_red}_{out_white}",
                        icon = $"i_{id}_{(is_white ? "w" : "r")}_upgrd",
                        craft_in = new() { "soul_workbench" },
                        sanity = new(),
                        condition = new(),
                        needs = new() { HelperGYK.NewItem(fixID($"{id}:{id}_{in_red}_{in_white}"), 1), HelperGYK.NewItem("sin_shard", cost) },
                        output = new() { HelperGYK.NewItem(fixID($"{id}:{id}_{out_red}_{out_white}"), 1) },
                        energy = HelperGYK.NewExpression("3"),
                        craft_time = HelperGYK.NewExpression("4"),
                        enqueue_type = CraftDefinition.EnqueueType.CanEnqueue,
                        force_multi_craft = true,
                    };

                    if (GameBalance.me.craft_data.Any(a => a.id == craft.id))
                        Debug.LogError($"[exception] duplicate craft '{craft.id}'");
                    else
                        GameBalance.me.craft_data.Add(craft);
                }
                static int getCost(int in_skull, int out_skull)
                {
                    int cost = 0;
                    while (in_skull < out_skull)
                    {
                        in_skull++;
                        cost += in_skull switch
                        {
                            2 => 3,
                            >= 3 => 4,
                            _ => 2,
                        };
                    }
                    return cost;
                }
                static string fixID(string id)
                {
                    return id switch
                    {
                        "blood:blood_1_-1" => "blood",
                        "fat:fat_1_-1" => "fat",
                        "skin:skin_-1_1" => "skin",
                        "flesh:flesh_0_1" => "flesh",
                        _ => id,
                    };
                }

                // change items
                foreach (var item in GameBalance.me.items_data)
                {
                    if (item.id.StartsWith("heart:") || item.id.StartsWith("brain:") || item.id.StartsWith("intestine:")
                        || item.id.StartsWith("skin:") || item.id.StartsWith("flesh:") || item.id.StartsWith("blood:") || item.id.StartsWith("fat:")
                        || item.id == "heart_dark" || item.id == "brain_dark" || item.id == "intestine_dark"
                        )
                    {
                        item.stack_count = 999;
                    }

                    else if (item.id == "aristocrat_paper")
                    {
                        item.player_cant_throw_out = false;
                    }
                }

                // change buildings
                foreach (var obj in GameBalance.me.objs_data)
                {
                    if (obj.id == "well_pump")
                    {
                        obj.can_insert_items.Add("clay"); // TODO: check if this can be removed
                        obj.can_insert_items.Add("sand_river");
                    }

                    else if (obj.id is "cooking_table" or "cooking_table_2" or "tavern_kitchen" or "mf_vine_press" or "soul_workbench")
                    {
                        obj.can_insert_zombie = true;
                        obj.pre_interaction_2.script = ":LinkOverheadZombieWorker()";
                        obj.pre_interaction_2.condition.FromString("CanLinkOverheadZombieWorker()");
                        obj.pre_interaction_2.hint = "put_hint";
                    }

                    // fix bonus crop not working
                    else if (obj.id.EndsWith("_ready") && obj.drop_items != null)
                    {
                        foreach (var drop in obj.drop_items)
                        {
                            drop.min_value?.FromString(drop.min_value.GetRawExpressionString().Replace("WGOpar(\"p_farmer\")", "Ppar(\"p_farmer\")"));
                            drop.max_value?.FromString(drop.max_value.GetRawExpressionString().Replace("WGOpar(\"p_farmer\")", "Ppar(\"p_farmer\")"));
                        }
                    }
                }


                // recalc caches
                GameBalance.me.CreateIDsCache();
                GameBalance.me.CreateItemsBaseNameCache();
                GameBalance.me.CreateToolsCache();
                GameBalance.me.CreateCraftsCache();
                ObjectGroupDefinition.LinkObjectsToGroups();
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        // fix stacking organs
        [HarmonyPatch(typeof(CraftComponent), nameof(CraftComponent.CraftReally))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixStackingOrgansBug(IEnumerable<CodeInstruction> instr)
        {
            var code = instr.ToList();
            var fi = AccessTools.Field(typeof(ItemDefinition), nameof(ItemDefinition.stack_count));

            // make autopsy extraction ignore stack_count field by pretenting it is count=1 even if it's not
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && object.Equals(code[i].operand, fi))
                {
                    code.Insert(++i, new CodeInstruction(OpCodes.Ldarg_1));
                    code.Insert(++i, new CodeInstruction(OpCodes.Call, ((Delegate)Patch0).Method));
                    break;
                }
            }
            return code;
        }
        public static int Patch0(int stack0, CraftDefinition stack1)
        {
            if (stack1.IsBodyPartExtractionCraft())
                return 1;
            return stack0;
        }

        // fix well sand chance
        [HarmonyPatch(typeof(CraftComponent), nameof(CraftComponent.ProcessFinishedCraft))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixToWgoOutputChance(IEnumerable<CodeInstruction> instr, ILGenerator generator, MethodBase original)
        {
            var code = instr.ToList();

            var fi_out = AccessTools.Field(typeof(CraftDefinition), nameof(CraftDefinition.output_to_wgo));
            var mi_add = AccessTools.Method(typeof(WorldGameObject), nameof(WorldGameObject.AddToInventory), [typeof(Item)]);

            bool flag = false;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && object.Equals(code[i].operand, fi_out))
                    flag = true;
                else if (flag && code[i].Calls(mi_add))
                {
                    code.Insert(i++, new CodeInstruction(OpCodes.Ldarg_0));
                    code[i].opcode = OpCodes.Call;
                    code[i].operand = ((Delegate)Patch1).Method;
                    break;
                }
            }

            return code;
            //var tool = new TranspilerTool(instr, generator, original);
        }
        public static bool Patch1(WorldGameObject wgo, Item item, CraftComponent __instance)
        {
            // fixes crafting which adds chance items to wgo (e.g. clay/sand to the well pump)
            if (item.self_chance == null || !item.self_chance.has_expression || item.self_chance.EvaluateChance(wgo, __instance.other_obj))
                return wgo.AddToInventory(item);
            return false;
        }

        // fix kitchen zombie
        [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.CanPutToAllPossibleInventories))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixTavernKitchen1(IEnumerable<CodeInstruction> instr)
        {
            foreach (var line in instr)
            {
                if (line.opcode == OpCodes.Ldstr && object.Equals(line.operand, "tavern_kitchen"))
                    line.operand = "###";
                yield return line;
            }
        }
        [HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.PutToAllPossibleInventories))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixTavernKitchen2(IEnumerable<CodeInstruction> instr)
        {
            foreach (var line in instr)
            {
                if (line.opcode == OpCodes.Ldstr && object.Equals(line.operand, "tavern_kitchen"))
                    line.operand = "###";
                yield return line;
            }
        }

        // patch scripts
        [HarmonyPatch(typeof(SmartExpression), nameof(SmartExpression.CheckExpressionInit))]
        [HarmonyPrefix]
        public static void EditScriptPre(SmartExpression __instance, ref bool __state)
        {
            __state = __instance._exp == null && !__instance._simplified;
        }
        [HarmonyPatch(typeof(SmartExpression), nameof(SmartExpression.CheckExpressionInit))]
        [HarmonyPostfix]
        public static void EditScriptPost(SmartExpression __instance, bool __state)
        {
            if (!__state)
                return;

            __instance._exp._parser._registeredFunctions["TakeWaterFromPump"] = delegate
            {
                int num;
                drop("water");
                drop("clay");
                drop("sand_river");
                WorldMap.GetWorldGameObjectByObjId("water_well").TriggerSmartAnimation("work");
                return true;

                void drop(string id)
                {
                    num = __instance._wgo.data.GetItemsCount(id);
                    if (num > 10) num = 10;
                    __instance._wgo.DropItem(new Item(id, num), Direction.ToPlayer);
                    __instance._wgo.data.RemoveItem(id, num);
                }
            };
        }
    }
}
