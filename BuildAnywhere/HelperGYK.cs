using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace BuildAnywhere
{
    public static class HelperGYK
    {
        private static MethodInfo _MemberwiseClone = AccessTools.Method(typeof(object), "MemberwiseClone");
        public static T Clone<T>(this T obj, Action<T> action = null) where T : class
        {
            // TODO: check if AccessTools works!
            //var result = AccessTools.MakeDeepCopy<T>(obj);
            //action?.Invoke(result);
            //return result;

            var result = (T)_MemberwiseClone.Invoke(obj, null);

            // deep copy certain type of fields by reflection
            foreach (var fi in typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                //if (fi.FieldType.IsValueType)
                //    continue;
                //var old = fi.GetValue(result);
                //if (old == null)
                //    continue;
                //fi.SetValue(result, old.Clone());
                //continue;

                // deep copy ToolsActions
                if (fi.FieldType == typeof(ToolActions) && fi.GetValue(result) is ToolActions oldTools)
                {
                    var newTools = new ToolActions
                    {
                        action_tools = new(oldTools.action_tools),
                        action_k = new(oldTools.action_k),
                    };
                    fi.SetValue(result, newTools);
                    continue;
                }

                if (fi.FieldType == typeof(GameRes) && fi.GetValue(result) is GameRes oldRes)
                {
                    fi.SetValue(result, oldRes?.Clone());
                    continue;
                }

                if (fi.FieldType == typeof(SmartExpression) && fi.GetValue(result) is SmartExpression oldExp)
                {
                    var newExp = new SmartExpression
                    {
                        _expression = oldExp._expression,
                        _simplified = oldExp._simplified,
                        _simpified_float = oldExp._simpified_float
                    };
                    fi.SetValue(result, newExp);
                    continue;
                }

                // deep copy fields from type IList; does not deep copy entries in those lists
                if (!typeof(IList).IsAssignableFrom(fi.FieldType))
                    continue;

                object oldfield = fi.GetValue(result);
                object newfield = fi.FieldType.GetConstructor([fi.FieldType])?.Invoke([oldfield]);
                newfield ??= fi.FieldType.GetConstructor([])?.Invoke([]);
                if (newfield != null)
                    fi.SetValue(result, newfield);
            }

            action?.Invoke(result);
            return result;
        }

        public static object GetField(this object obj, string name)
        {
            return AccessTools.Field(obj.GetType(), name).GetValue(obj);
        }

        public static T GetField<T>(this object obj, string name)
        {
            return (T)AccessTools.Field(obj.GetType(), name).GetValue(obj);
        }

        public static void SetField(this object obj, string name, object value)
        {
            AccessTools.Field(obj.GetType(), name).SetValue(obj, value);
        }

        public static void Invoke(this Type type, string name, object instance, params object[] parameters)
        {
            AccessTools.Method(type, name).Invoke(instance, parameters);
        }

        public static CraftDefinition Clone(CraftDefinition input) //shallow copy
        {
            //var exp = SmartExpression.ParseExpression("5.0");
            var output = new CraftDefinition();

            //irrelevant
            output.ach_key = input.ach_key;                                         //string
            output.tool_actions = input.tool_actions;                               //ToolActions ?
            output.buff = input.buff;                                               //string
            output.change_wgo = input.change_wgo;                                   //string
            output.custom_name = input.custom_name;                                 //string
            output.disable_multi_craft = input.disable_multi_craft;                 //bool
            output.dont_close_window_on_craft = input.dont_close_window_on_craft;   //bool
            output.dont_show_in_hint = input.dont_show_in_hint;                     //bool
            output.dur_needs_item_index = input.dur_needs_item_index;               //int
            output.dur_parameter = input.dur_parameter;                             //float
            output.flag = input.flag;                                               //int
            output.force_multi_craft = input.force_multi_craft;                     //bool
            output.game_res_to_mirror_max = input.game_res_to_mirror_max;           //float
            output.hidden = input.hidden;                                           //bool
            output.item_needs = input.item_needs;                                   //List<Item>
            output.item_needs_leave = input.item_needs_leave;                       //bool
            output.k_faith = input.k_faith;                                         //float
            output.k_money = input.k_money;                                         //float
            output.linked_sub_id = input.linked_sub_id;                             //string
            output.needs_quality = input.needs_quality;                             //float
            output.needs_unlock = input.needs_unlock;                               //bool
            output.one_time_craft = input.one_time_craft;                           //bool
            output.puff_when_replaced = input.puff_when_replaced;                   //bool
            output.output_to_wgo_on_start = input.output_to_wgo_on_start;           //List<Item>        //candles
            output.sub_type = input.sub_type;                                       //enum CraftSubType
            output.transfer_needs_to_wgo = input.transfer_needs_to_wgo;             //bool
            output.item_output = input.item_output;                                 //List<Item>        //embalming

            //unsure
            output.condition = input.condition;                                     //SmartExpression
            output.end_script = input.end_script;                                   //string
            output.itempars_add = input.itempars_add;                               //GameRes
            output.itempars_set = input.itempars_set;                               //GameRes
            output.output_res_wgo = input.output_res_wgo;                           //GameRes
            output.output_set_res_wgo = input.output_set_res_wgo;                   //GameRes
            output.set_out_wgo_params_on_start = input.set_out_wgo_params_on_start; //bool
            output.set_when_cancelled = input.set_when_cancelled;                   //GameRes

            //some relevants
            output.craft_time_is_zero = input.craft_time_is_zero;                   //bool
            output.craft_after_finish = input.craft_after_finish;                   //string            //if finished, will start the next job
            output.difficulty = input.difficulty;                                   //float             //0-1, defines how difficult it is to get high quality
            output.craft_type = input.craft_type;                                   //enum CraftType    //used for alchemy and gardening
            output.game_res_to_mirror_name = input.game_res_to_mirror_name;         //string            //related to gardening
            output.store_last_craft_slot = input.store_last_craft_slot;             //int               //related to gardening
            output.is_item_crating_craft = input.is_item_crating_craft;             //bool              //related to food
            output.dur_needs_item = input.dur_needs_item;                           //float             //how much durability is lost
            output.enqueue_type = input.enqueue_type;                               //enum EnqueueType  //whenever the recipe can be queued or not
            output.hide_quality_icon = input.hide_quality_icon;                     //bool
            output.linked_buffs = input.linked_buffs;                               //List<string>      //relevant, if SmartExpression uses perks/buffs
            output.linked_perks = input.linked_perks;                               //List<string>      //relevant, if SmartExpression uses perks/buffs

            //important
            output.id = input.id;                                                   //string
            output.craft_in = input.craft_in;                                       //List<string>
            output.craft_time = input.craft_time;                                   //SmartExpression
            output.energy = input.energy;                                           //SmartExpression
            output.icon = input.icon;                                               //string
            output.is_auto = input.is_auto;                                         //bool
            output.needs = input.needs;                                             //List<Item>
            output.needs_from_wgo = input.needs_from_wgo;                           //List<Item>        //fuel
            output.output_to_wgo = input.output_to_wgo;                             //List<Item>        //fuel
            output.output = input.output;                                           //List<Item>
            output.sanity = input.sanity;                                           //SmartExpression   //must be set to empty
            output.tab_id = input.tab_id;                                           //string

            return output;
        }

        public static CraftDefinition CreateCraftDefinition(string recipe_id, string crafting_station, float energy, float time, bool isAuto, int fuel = 0, Item[] input = null, Item[] output = null, string tab = "", string icon = "", float takedurability = 0f)
        {
            var result = new CraftDefinition();

            result.energy = SmartExpression.ParseExpression(energy.ToString());
            result.craft_time = SmartExpression.ParseExpression(time.ToString());
            result.sanity = SmartExpression.ParseExpression("0.0");

            result.id = recipe_id;
            result.tab_id = tab;
            result.craft_in.Add(crafting_station);
            result.is_auto = isAuto;
            result.dur_needs_item = takedurability;

            if (icon == "" && output != null && output.Length > 1)
                result.icon = GetIcon(output) ?? GetIcon(input);
            else
                result.icon = icon;

            if (fuel > 0)
                result.output_to_wgo.Add(new Item("fire").MinValue(fuel));
            else if (fuel < 0)
                result.needs_from_wgo.Add(new Item("fire").MinValue(-fuel));

            if (input != null)
                foreach (Item item in input)
                    result.needs.Add(item);

            if (output != null)
                foreach (Item item in output)
                    result.output.Add(item);

            return result;

            //some relevants
            // result.craft_time_is_zero;       //bool
            // result.craft_after_finish;       //string            //if finished, will start the next job
            // result.difficulty;               //float             //0-1, defines how difficult it is to get high quality
            // result.craft_type;               //enum CraftType    //used for alchemy and gardening
            // result.game_res_to_mirror_name;  //string            //related to gardening
            // result.store_last_craft_slot;    //int               //related to gardening
            // result.is_item_crating_craft;    //bool              //related to food
            // result.dur_needs_item;           //float             //how much durability is lost (at index 0 of "needs", unless dur_needs_item_index is set)
            // result.enqueue_type;             //enum EnqueueType  //whenever the recipe can be queued or not
            // result.hide_quality_icon;        //bool
            // result.linked_buffs;             //List<string>      //relevant, if SmartExpression uses perks/buffs
            // result.linked_perks;             //List<string>      //relevant, if SmartExpression uses perks/buffs
        }

        public static CraftDefinition SetChanceOutputGroup(this CraftDefinition recipe, params float[] chances)
        {
            return SetChanceOutputGroup(recipe, 0, int.MaxValue, 1, chances);
        }

        public static CraftDefinition SetChanceOutputGroup(this CraftDefinition recipe, int start, int length, int group, params float[] chances)
        {
            // SmartExpression self_chance;
            // SmartExpression common_chance;

            float total = 0;

            for (int i = start, j = 0; i < recipe.output.Count && j < chances.Length && j < length; i++, j++)
            {
                total += chances[j];
                recipe.output[i].common_chance = SmartExpression.ParseExpression(chances[j].ToString());
                recipe.output[i].chance_group = group;
            }

            if (total != 1f)
                Main.Log("Warning: Probability does not add up to 100%");

            return recipe;
        }

        public static CraftDefinition SetChanceOutputSelf(this CraftDefinition recipe, string item_id, string chance, int? dropamount = null)
        {
            return SetChanceOutputSelf(recipe, recipe.output.FindIndex(s => s.id == item_id), chance, dropamount);
        }

        // something like "Ppar(\"p_t_gold_ore\")*(0.05+Ppar(\"p_miner\")*0.05)" or "0.1"
        public static CraftDefinition SetChanceOutputSelf(this CraftDefinition recipe, int index, string chance, int? dropamount)
        {
            if (0 <= index && index < recipe.output.Count)
            {
                recipe.output[index].self_chance = SmartExpression.ParseExpression(chance);
                if (dropamount != null)
                {
                    recipe.output[index].value = dropamount.Value;
                    recipe.output[index].min_value.FromString(dropamount.Value.ToString());
                }
            }
            else
                Main.Log("SetSelfChance: bad index for " + recipe.id);

            return recipe;
        }

        public static Item NewItem(string id, int amount = 1, string min_value = null, string max_value = null, string chance = null)
        {
            var item = new Item(id, amount)
            {
                min_value = new(),
                max_value = new(),
                chance_group = -1,
                self_chance = new(),
            };

            if (item.definition == null)
                Debug.LogError($"[exception] item id '{id}' does not exist");

            item.min_value.FromString(min_value ?? "");
            item.max_value.FromString(max_value ?? "");
            item.self_chance.FromString(chance ?? "1.0");
            item._params._durability = 1f;
            return item;
        }

        public static SmartExpression NewExpression(string value)
        {
            var result = new SmartExpression();
            result.FromString(value);
            return result;
        }

        /// <summary>Required if item is in the output of crafting recipies.</summary>
        public static Item MinValue(this Item item, int count)
        {
            item.value = count;
            item.min_value = SmartExpression.ParseExpression(count.ToString());
            item.max_value = new SmartExpression();//SmartExpression.ParseExpression(count.ToString());
            item.self_chance.default_value = 1f;
            return item;
        }


        public static string GetIcon(Item[] items)
        {
            string result = "";

            if (items == null)
                return result;

            foreach (Item item in items)
            {
                result = item.GetIcon();
                if (result != "")
                    return result;
            }

            return result;
        }
    }
}
