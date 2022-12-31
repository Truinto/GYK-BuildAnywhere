//#define DATAMINING

global using HarmonyLib;
global using Newtonsoft.Json;
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using UnityEngine;
using Fishing;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace BuildAnywhere
{
    public class Main
    {
        public static Harmony harmony;
        public static Thread hotkeys = new Thread(Hotkey.Update);
        public static void Load()
        {
            try
            {
                Main.Log("Loading BuildAnywhere...");

                if (!Settings.i.Load())
                    Settings.i.Save();

                Settings.i.ParseEnums();

                if (Settings.i.Hotkeys)
                {
                    Hotkey.Listener.Add(new Hotkey.KeyState(Settings.i._TeleportStone));
                    Hotkey.Keypress += TeleportStone.HandleKeypress;
                    Hotkey.Listener.Add(new Hotkey.KeyState(Settings.i._TeleportMouse));
                    Hotkey.Keypress += TeleportMouse.HandleKeypress;
                }

                if (Settings.i.VariousTests)
                {
                    Hotkey.Listener.Add(new Hotkey.KeyState(KeyCode.P));
                    Hotkey.Keypress += Datamining.HandleKeypress;
                }

                harmony = new("com.Fumihiko.BuildAnywhere");
                harmony.PatchAll(typeof(Main).Assembly);

                if (Settings.i.Hotkeys || Settings.i.VariousTests)
                {
                    hotkeys.IsBackground = true;
                    hotkeys.Start();
                }

            }
            catch (Exception e)
            {
                Main.Log(e.ToString());
            }
        }

        public static void Log(string str)
        {
            Debug.Log("[BuildAnywhere] " + str);
        }
    }

    #region datamining

    public class Datamining
    {
        public static void HandleKeypress(object sender, KeyCode key)
        {
            var pools = SmartResourceHelper.me.GetField<Dictionary<Type, SmartResourceHelperPool>>("_pools");
            foreach (var entry in pools)
            {
                foreach (var sub in entry.Value.loaded)
                {
                    PrintAll(sub.Value, sub.Key, "resource_pools", entry.Value.type.ToString());
                }
            }

            foreach (var recipe in GameBalance.me.craft_data)
            {
                if (recipe.craft_in != null && recipe.craft_in.Count == 1)
                    PrintAll(recipe, recipe.id, "craft_data", recipe.craft_in[0].Replace(':', '.'));
                else
                    PrintAll(recipe, recipe.id, "craft_data");
            }

            foreach (var obj in GameBalance.me.craft_obj_data)
            {
                PrintAll(obj, obj.id, "craft_obj_data");
            }

            foreach (var item in GameBalance.me.items_data)
            {
                PrintAll(item, item.id, "items_data");
            }

            foreach (var building in GameBalance.me.objs_data)
            {
                PrintAll(building, building.id, "objs_data");
            }

            foreach (var perk in GameBalance.me.perks_data)
            {
                PrintAll(perk, perk.id, "perks_data");
            }

            foreach (var buff in GameBalance.me.buffs_data)
            {
                PrintAll(buff, buff.id, "buffs_data");
            }

            foreach (var tech in GameBalance.me.techs_data)
            {
                PrintAll(tech, tech.id, "techs_data");
            }

            foreach (var zone in GameBalance.me.world_zones_data)
            {
                PrintAll(zone, zone.id, "world_zones_data");
            }

            foreach (var vendor in GameBalance.me.vendors_data)
            {
                PrintAll(vendor, vendor.id, "vendors_data");
            }

            foreach (var spawner in GameBalance.me.spawners_data)
            {
                PrintAll(spawner, spawner.id, "spawners_data");
            }

            foreach (var body in GameBalance.me.bodies_data)
            {
                PrintAll(body, body.id, "bodies_data");
            }

            var collection = AccessTools.Field(typeof(GameBalanceBase), "_datas").GetValue(GameBalance.me) as List<IList>;
            var c_types = AccessTools.Field(typeof(GameBalanceBase), "_types").GetValue(GameBalance.me) as List<Type>;

            //GameBalance.me._datas

            if (collection != null && c_types != null)
            {
                for (int i = 0; i < c_types.Count; i++)
                {
                    FieldInfo fi_id = c_types[i].GetField("id");
                    for (int j = 0; j < collection.Count; j++)
                    {
                        string id = null;
                        if (fi_id != null)
                            id = fi_id.GetValue(collection[j]) as string;

                        PrintAll(collection[j], id ?? $"{i}.{j}", "_datas", c_types[i].ToString());
                    }
                }
            }
        }

        public static void PrintAll(object value, string name, params string[] folders)
        {
            try
            {
                string folder = Path.Combine(folders);
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, name.Replace(':', '.') + ".json");

                JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    TypeNameHandling = TypeNameHandling.None,
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ContractResolver = new MyContractResolver()
                });

                using (StreamWriter streamReader = new StreamWriter(path)) // TODO make sure directory exists
                {
                    using (JsonTextWriter jsonReader = new JsonTextWriter(streamReader))
                    {
                        serializer.Serialize(jsonReader, value);

                        jsonReader.Close();
                    }

                    streamReader.Close();
                }
            }
            catch (Exception e)
            {
                Main.Log(e.ToString());
            }
        }

        public class MyContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {

                var props1 = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(s => s.IsPublic || Attribute.IsDefined(s, typeof(SerializeField)))
                            .Select(f => base.CreateProperty(f, memberSerialization))
                            .ToList();
                props1.ForEach(p => { p.Writable = true; p.Readable = true; });
                return props1;

                // var props2 = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                //             .Select(p => base.CreateProperty(p, memberSerialization))
                //         .Union(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(s => s.IsPublic || Attribute.IsDefined(s, typeof(SerializeField)))
                //             .Select(f => base.CreateProperty(f, memberSerialization)))
                //         .ToList();
                // props2.ForEach(p => { p.Writable = true; p.Readable = true; });
                // return props2;
            }
        }
    }

    #endregion

    #region Teleport

    public class TeleportStone
    {
        public static void HandleKeypress(object sender, KeyCode key)
        {
            if (key == Settings.i._TeleportStone)
            {
                Debug.Log("Manual StoneTeleport");
                try
                {
                    GS.RunFlowScript("StoneTeleport", null);
                }
                catch (Exception ex)
                {
                    Debug.Log("Wups " + ex.ToString());
                }
            }
        }
    }

    public class TeleportMouse
    {
        public static void HandleKeypress(object sender, KeyCode key)
        {
            if (key == Settings.i._TeleportMouse)
            {
                Debug.Log("Teleport to mouse position");
                Vector2 screenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                Vector2 worldPosition = MainGame.me.world_cam.ScreenToWorldPoint(screenPosition);
                MainGame.me.player.transform.position = worldPosition;
            }
        }
    }

    #endregion

    #region BuildMode

    //[HarmonyPatch(typeof(FlowGridCell), nameof(FlowGridCell.IsInsideWorldZone))]
    public class Patch_IgnoreBuildingZone
    {
        public static bool Prepare()
        {
            return Settings.i._ButtonIgnoreBuildArea != KeyCode.None;
        }

        public static void Postfix(ref bool __result)
        {
            if (Input.GetKey(Settings.i._ButtonIgnoreBuildArea))
                __result = true;
        }
    }

    //[HarmonyPatch(typeof(FloatingWorldGameObject), nameof(FloatingWorldGameObject.RecalculateAvailability))]
    public class Patch_IgnoreOverlap
    {
        public static bool Prepare()
        {
            return Settings.i._ButtonIgnoreBuildOverlap != KeyCode.None;
        }

        public static void Postfix()
        {
            if (Input.GetKey(Settings.i._ButtonIgnoreBuildOverlap))
                FloatingWorldGameObject.can_be_built = true;
        }
    }

    ////[HarmonyPatch(typeof(WorldZone), nameof(WorldZone.GetBounds))]
    public class Patch_BuildFreeCam
    {
        public static bool Prepare()
        {
            return false;//Settings.i.BuildModeExpandView > 0;
        }

        public static void Postfix(ref Bounds __result)
        {
            //__result.Expand(Settings.i.BuildModeExpandView);
        }
    }

    #endregion

    #region Fishing

    //[HarmonyPatch(typeof(FishLogic), nameof(FishLogic.CalculateFishPos))]
    public class Patch_FishingAlwaysSuccess
    {
        public static bool Prepare()
        {
            return Settings.i.FishingAlwaysSuccess;
        }
        public static void Prefix(ref float pos, ref float rod_zone_size)
        {
            pos = 0f;
            rod_zone_size = 100f;
        }
    }

    //[HarmonyPatch(typeof(FishingGUI), "UpdateWaitingForBite")]
    public class Patch_FishAlwaysBiting
    {
        public static bool Prepare()
        {
            return Settings.i.FishAlwaysBiting;
        }
        public static void Prefix(FishingGUI __instance)
        {
            AccessTools.Field(typeof(FishingGUI), "_waiting_for_bite_delay").SetValue(__instance, 0f);
        }
    }

    //[HarmonyPatch(typeof(FishingGUI), "UpdateWaitingForPulling")]
    public class Patch_PullAutomatically
    {
        public static bool Prepare()
        {
            return Settings.i.FishPullAutomatically;
        }
        public static void Prefix(FishingGUI __instance)
        {
            AccessTools.Method(typeof(FishingGUI), "ChangeState").Invoke(__instance, new object[] { FishingGUI.FishingState.Pulling });
        }
    }

    //[HarmonyPatch(typeof(FishingGUI), "RemoveBait")]
    public class Patch_NoDurabiliyLoss
    {
        public static bool Prepare()
        {
            return Settings.i.FishNoDurabiliyLoss;
        }
        public static void Postfix(Item bait)
        {
            if (bait.definition.has_durability)
                bait.durability = 1;
        }
    }

    #endregion

    #region Items

    //[HarmonyPatch(typeof(Item), nameof(Item.durability), MethodType.Getter)]
    public class Patch_NoItemDurability
    {
        public static bool Prepare()
        {
            return Settings.i.AllObjectsNoDurability;
        }
        public static bool Prefix(Item __instance)
        {
            try
            {
                (AccessTools.Field(typeof(Item), "_params").GetValue(__instance) as GameRes).durability = 1f;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }

    #endregion

    #region Crafting

    //[HarmonyPatch(typeof(GameBalance), nameof(GameBalance.LoadGameBalance))]
    public class Patch_GameBalance
    {
        public static bool Prepare()
        {
            return Settings.i.PGB_global;
        }
        public static void Postfix()
        {
            // patching crafting recipies
            if (Settings.i.PGB_MoreZombieRecipies)
            {
                CraftDefinition zombiegarden = null;
                List<CraftDefinition> tavern = new List<CraftDefinition>();
                foreach (var recipe in GameBalance.me.craft_data)
                {
                    if (recipe.craft_in.Contains("tavern_kitchen") || recipe.craft_in.Contains("tavern_oven"))
                    {
                        tavern.AddRange(SplitMultiQualityRecipeX(recipe));
                    }

                    if (recipe.craft_in.Contains("table_book_constr"))
                    {
                        recipe.craft_in.Add("alchemy_workbench_zombie");
                    }

                    else if (recipe.craft_in.Contains("mf_preparation_2"))
                    {
                        recipe.craft_time_is_zero = true;
                        recipe.is_auto = true;
                        recipe.craft_time.FromString("0");
                    }

                    else if (recipe.craft_in.Contains("desk_2"))
                    {
                        recipe.craft_in.Add("zombie_pulpit");
                    }

                    else if (recipe.tab_id != "fuel" && recipe.craft_in.Find(s => s.StartsWith("mf_furnace_")) != null)
                    {
                        recipe.output.Add(new Item("ash").MinValue(1));
                    }

                    else if (recipe.id == "marble_plate_3")
                    {
                        recipe.difficulty = 0.8f;
                    }

                    else if (recipe.id == "iron_to_small" || recipe.id == "iron_to_small_2")
                    {
                        recipe.SetChanceOutputSelf("nugget_gold", "Ppar(\"p_t_gold_ore\")*0.3", 2);
                    }

                    else if (recipe.id == "peat_from_waste")
                    {
                        recipe.craft_type = CraftDefinition.CraftType.None;
                        recipe.enqueue_type = CraftDefinition.EnqueueType.CanEnqueue;
                        recipe.dont_close_window_on_craft = true;
                    }

                    else if (recipe.id == "candelabrum_3_to_3_3" || recipe.id == "wall_candelabrum_3_to_3_3")
                    {
                        recipe.craft_after_finish = "";
                    }

                    else if (recipe.id == "firewood" || recipe.id == "spike_1")
                    {
                        recipe.craft_in.Add("mf_saw_1");
                    }

                    else if (recipe.id == "garden_wheat_grow_desk_planting")
                    {
                        zombiegarden = recipe;
                    }

                    else if (recipe.id == "water_pumping")
                    {
                        recipe.output_to_wgo.FirstOrDefault()?.MinValue(5);
                    }

                    recipe.craft_time.FromString(recipe.craft_time.GetRawExpressionString().Replace("WGOpar(\"buff_plant\")", "Ppar(\"buff_plant\")"));
                }

                GameBalance.me.craft_data.Add(Helper.CreateCraftDefinition(
                    "_stone_to_sand",
                    "mf_hammer_1",
                    2f,
                    2f,
                    false,
                    0,
                    new Item[] { new Item("stone_plate_1").MinValue(1) },
                    new Item[] { new Item("sand_river").MinValue(6), new Item("clay").MinValue(2) },
                    tab: "stone"
                ));

                GameBalance.me.craft_data.Add(Helper.CreateCraftDefinition(
                    "_marble_plate_3_1",
                    "mf_hammer_1",
                    20f,
                    3f,
                    false,
                    0,
                    new Item[] { new Item("chisel:chisel_2:1").MinValue(1), new Item("marble_plate_2").MinValue(1), new Item("faith").MinValue(5) },
                    new Item[] { new Item("marble_plate_3:1").MinValue(1), new Item("r", 3), new Item("g"), new Item("b", 2) },
                    tab: "stone",
                    takedurability: 0.1f
                ));
                GameBalance.me.craft_data.Add(Helper.CreateCraftDefinition(
                    "_marble_plate_3_2",
                    "mf_hammer_1",
                    20f,
                    3f,
                    false,
                    0,
                    new Item[] { new Item("chisel:chisel_2:2").MinValue(1), new Item("marble_plate_2").MinValue(1), new Item("faith").MinValue(5) },
                    new Item[] { new Item("marble_plate_3:2").MinValue(1), new Item("r", 3), new Item("g"), new Item("b", 2) },
                    tab: "stone",
                    takedurability: 0.1f
                ));
                GameBalance.me.craft_data.Add(Helper.CreateCraftDefinition(
                    "_marble_plate_3_3",
                    "mf_hammer_1",
                    20f,
                    3f,
                    false,
                    0,
                    new Item[] { new Item("chisel:chisel_2:3").MinValue(1), new Item("marble_plate_2").MinValue(1), new Item("faith").MinValue(5) },
                    new Item[] { new Item("marble_plate_3:3").MinValue(1), new Item("r", 3), new Item("g"), new Item("b", 2) },
                    tab: "stone",
                    takedurability: 0.1f
                ));

                GameBalance.me.craft_data.Add(Helper.CreateCraftDefinition(
                    "_wood_to_ash",
                    "mf_furnace_2",
                    3f,
                    0f,
                    true,
                    0,
                    new Item[] { new Item("wood1", 5).MinValue(5) },
                    new Item[] { new Item("ash", 5).MinValue(5) },
                    tab: ""
                ));

                GameBalance.me.craft_data.Add(Helper.CreateCraftDefinition(
                    "_cup_mead_zombie_1",
                    "mf_zombie_brewing",
                    0f,
                    120f,
                    false,
                    0,
                    new Item[] { new Item("water").MinValue(50), new Item("hop_crop:1").MinValue(3), new Item("honey").MinValue(1) },
                    new Item[] { new Item("cup_mead:1").MinValue(10) }
                ));

                GameBalance.me.craft_data.Add(Helper.CreateCraftDefinition(
                    "_cup_mead_zombie_2",
                    "mf_zombie_brewing",
                    0f,
                    120f,
                    false,
                    0,
                    new Item[] { new Item("water").MinValue(50), new Item("hop_crop:2").MinValue(3), new Item("honey").MinValue(1) },
                    new Item[] { new Item("cup_mead:2").MinValue(10) }
                ));

                GameBalance.me.craft_data.Add(Helper.CreateCraftDefinition(
                    "_cup_mead_zombie_3",
                    "mf_zombie_brewing",
                    0f,
                    120f,
                    false,
                    0,
                    new Item[] { new Item("water").MinValue(50), new Item("hop_crop:3").MinValue(3), new Item("honey").MinValue(1) },
                    new Item[] { new Item("cup_mead:3").MinValue(10) }
                ));

                var fuel_big = Helper.CreateCraftDefinition(
                    "_fuel_firewood_large",
                    "oven",
                    0f,
                    0f,
                    true,
                    100,
                    new Item[] { new Item("fire").MinValue(3000) },
                    tab: "fuel",
                    icon: "i_fire"
                );
                fuel_big.craft_in.Add("cooking_bonfire");
                fuel_big.craft_in.Add("tavern_oven");
                fuel_big.craft_in.Add("mf_furnace_0");
                fuel_big.craft_in.Add("mf_furnace_1");
                fuel_big.craft_in.Add("mf_furnace_2");
                fuel_big.craft_in.Add("brewing_stand");
                fuel_big.craft_in.Add("mf_distcube_3");
                GameBalance.me.AddData(fuel_big);

                if (zombiegarden != null)
                {
                    var hamp = Helper.Clone(zombiegarden);
                    hamp.id = "_garden_cannabis_grow_desk_planting";
                    //hamp.output_res_wgo = new GameRes("hamp", -1f);//wheat
                    //hamp.output_set_res_wgo = new GameRes("hamp", 1f);
                    //hamp.output_set_res_wgo.Add("growing", 0f);
                    //hamp.set_when_cancelled = new GameRes("hamp", 1f);
                    //hamp.set_when_cancelled.Add("growing", 0f);
                    hamp.icon = "i_hamp_crop";
                    hamp.needs = new List<Item>();
                    hamp.needs.Add(new Item("hamp_seed:1").MinValue(24));
                    hamp.output = new List<Item>();
                    hamp.output.Add(new Item("hamp_crop:1").MinValue(24));
                    hamp.output.Add(new Item("hamp_seed:1").MinValue(36));
                    hamp.output.Add(new Item("crop_waste").MinValue(12));
                    //hamp.craft_time = SmartExpression.ParseExpression("1");
                    GameBalance.me.craft_data.Add(hamp);
                }

                GameBalance.me.craft_data.AddRange(tavern);

            }

            if (true)
            {
                foreach (var obj in GameBalance.me.objs_data)
                {
                    try
                    {
                        if (obj.id.EndsWith("_ready") && obj.drop_items != null)    // TODO: check difference
                        {
                            foreach (var drop in obj.drop_items)
                            {
                                if (drop.min_value != null)
                                    drop.min_value.FromString(drop.min_value.GetRawExpressionString().Replace("WGOpar(\"p_farmer\")", "Ppar(\"p_farmer\")"));
                                if (drop.max_value != null)
                                    drop.max_value.FromString(drop.max_value.GetRawExpressionString().Replace("WGOpar(\"p_farmer\")", "Ppar(\"p_farmer\")"));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Main.Log("Expection: Wups, expression editing failed.");
                    }
                }
            }

            if (Settings.i.PGB_LinkStorageWellPump)
            {
                foreach (var building in GameBalance.me.objs_data)
                {
                    if (building.id == "tavern_kitchen"
                        || building.id == "tavern_oven"
                        //|| building.id == "alchemy_table_zombie"
                        )
                    {
                        building.additional_worldzone_inventories.Add("mf_wood");
                        building.additional_worldzone_inventories.Add("garden");
                        building.additional_worldzone_inventories.Add("vineyard");
                        //building.additional_worldzone_inventories.Add("home");
                    }
                    else if (building.id == "cooking_table"
                        || building.id == "oven"
                        )
                    {
                        building.additional_worldzone_inventories.Add("mf_wood");
                        building.additional_worldzone_inventories.Add("garden");
                        building.additional_worldzone_inventories.Add("vineyard");
                        //building.additional_worldzone_inventories.Add("tavern");
                    }
                    else if (building.id == "mf_zombie_brewing")
                    {
                        building.additional_worldzone_inventories.Add("mf_wood");
                        building.additional_worldzone_inventories.Add("garden");
                    }
                }
            }

            if (true)
            {
                foreach (var item in GameBalance.me.items_data)
                {
                    if (item.id == "hearthstone")
                    {
                        item.cooldown.FromString("1");
                    }

                    else if (item.id == "ingot_gold")
                    {
                        item.product_types.Add("luxery");
                        item.product_tier = 3;
                        item.base_count = 1;
                        item.base_price = 20;
                    }

                    else if (item.id == "egg_chicken")
                    {
                        item.base_count = 50;
                    }

                    else if (item.id == "faith")
                    {
                        item.product_types.Add("church");
                        item.product_tier = 3;
                        item.base_count = 10;
                        item.base_price = 3;
                        item.custom_sell_price_koeff = 0.1f;
                    }
                }

                //foreach (var vendor in GameBalance.me.vendors_data) {}
            }

            if (Settings.i.PGB__AllBuildingsAtAllBlueprintDesks)
            {
                HashSet<string> blueprint_desks = new HashSet<string>();
                foreach (var blueprint in GameBalance.me.craft_obj_data)
                {
                    if (blueprint.builder_ids.Count > 0)
                        blueprint_desks.Add(blueprint.builder_ids[0]);
                }

                foreach (var blueprint in GameBalance.me.craft_obj_data)
                {
                    if (blueprint.builder_ids.Count > 0)
                    {
                        blueprint.builder_ids.Clear();
                        blueprint.builder_ids.AddRange(blueprint_desks);
                    }
                }
            }

            // recalc caches
            GameBalance.me.CreateIDsCache();
            AccessTools.Method(typeof(GameBalance), "CreateItemsBaseNameCache").Invoke(GameBalance.me, new object[0]);
            AccessTools.Method(typeof(GameBalance), "CreateToolsCache").Invoke(GameBalance.me, new object[0]);
            AccessTools.Method(typeof(GameBalance), "CreateCraftsCache").Invoke(GameBalance.me, new object[0]);
            ObjectGroupDefinition.LinkObjectsToGroups();
        }

        public static List<CraftDefinition> SplitMultiQualityRecipe(CraftDefinition recipe)
        {
            int count = 0;
            foreach (var item in recipe.needs)
                count = Math.Max(item.multiquality_items.Count / 3, count);
            foreach (var item in recipe.output)
                count = Math.Max(item.multiquality_items.Count / 3, count);

            Main.Log($"SplitRecipe id={recipe.id}, count={count}");

            List<CraftDefinition> result = new List<CraftDefinition>();

            for (int i = 0; i < count; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    CraftDefinition craft = Helper.Clone(recipe);
                    craft.needs_unlock = false;
                    craft.id = $"_{recipe.id}_{k}_{i}";
                    craft.enqueue_type = CraftDefinition.EnqueueType.CanEnqueue;
                    craft.needs = new List<Item>();
                    craft.output = new List<Item>();

                    foreach (var item in recipe.needs)
                    {
                        if (item.multiquality_items.Count == 3)
                            craft.needs.Add(new Item(item.multiquality_items[k]).MinValue(item.value));
                        else if (item.multiquality_items.Count >= count * 3)
                            craft.needs.Add(new Item(item.multiquality_items[i * 3 + k]).MinValue(item.value));
                        else
                            craft.needs.Add(item);
                    }
                    foreach (var item in recipe.output)
                    {
                        if (item.multiquality_items.Count == 3)
                            craft.output.Add(new Item(item.multiquality_items[k]).MinValue(item.value));
                        else if (item.multiquality_items.Count >= count * 3)
                            craft.output.Add(new Item(item.multiquality_items[i * 3 + k]).MinValue(item.value));
                        else
                            craft.output.Add(item);
                    }
                    result.Add(craft);
                    Main.Log($"SplitRecipe Added {craft.id}");
                }
            }
            return result;
        }

        public static List<CraftDefinition> SplitMultiQualityRecipeX(CraftDefinition recipe)
        {
            List<CraftDefinition> result = new List<CraftDefinition>();

            try
            {
                int index_input = -1;
                int input_max = -1;
                for (int i = 0; i < recipe.needs.Count; i++)
                {
                    if (recipe.needs[i].is_multiquality)
                    {
                        if (input_max < recipe.needs[i].multiquality_items.Count)
                        {
                            input_max = recipe.needs[i].multiquality_items.Count;
                            index_input = i;
                        }
                    }
                }

                if (input_max < 0 && recipe.id.EndsWith("fillet") && recipe.needs.Count == 1)   //; note: not working as expected
                {
                    index_input = 0;
                    input_max = 1;
                }

                if (input_max < 0)
                {
                    Main.Log($"SplitRecipeX failed to find quality output {recipe.id}");
                    return result;
                }

                Main.Log($"SplitRecipeX id={recipe.id} index_input={index_input}, input_max={input_max}");

                for (int i = 0; i < input_max; i++)
                {
                    CraftDefinition craft = Helper.Clone(recipe);
                    craft.needs_unlock = false;
                    craft.id = $"_{recipe.id}_{i}";
                    craft.enqueue_type = CraftDefinition.EnqueueType.CanEnqueue;
                    craft.needs = new List<Item>();
                    craft.output = new List<Item>();

                    if (input_max > 1)
                        craft.needs.Add(new Item(recipe.needs[index_input].multiquality_items[i]).MinValue(recipe.needs[index_input].value));   //deciding input first
                    else
                        craft.needs.Add(recipe.needs[index_input]);  //special case fillet

                    int quality = (int)craft.needs[0].definition.quality - 1;
                    if (quality < 0 || quality >= 3)
                    {
                        Main.Log($"SplitRecipeX recipe {recipe.id} has no quality after all"); //fillet no quality
                        continue;
                    }

                    for (int j = 0; j < recipe.needs.Count; j++)
                    {
                        if (j == index_input)  //we already got this
                        {
                        }
                        else if (recipe.needs[j].multiquality_items.Count >= 3)   //inputs that have multiple qualities will have to match the leading one
                        {
                            craft.needs.Add(new Item(recipe.needs[j].multiquality_items[quality]).MinValue(recipe.needs[j].value));
                        }
                        else   //inputs without quality get copied
                        {
                            craft.needs.Add(recipe.needs[j]);
                            if (recipe.needs[j].is_multiquality)
                                Main.Log($"SplitRecipeX Wups @{recipe.id}, i={i}, q={quality}, j={j}");
                        }
                    }

                    for (int j = 0; j < recipe.output.Count; j++)
                    {
                        if (recipe.output[j].multiquality_items.Count >= 3)
                        {
                            craft.output.Add(new Item(recipe.output[j].multiquality_items[quality]).MinValue(recipe.output[j].value));
                        }
                        else
                        {
                            craft.output.Add(recipe.output[j]);
                        }
                    }
                    result.Add(craft);
                    Main.Log($"SplitRecipeX added {craft.id}");
                }
            }
            catch (Exception ex)
            {
                Main.Log("SplitRecipeX " + ex.Message);
            }

            return result;
        }
    }

    //[HarmonyPatch(typeof(AutopsyGUI), "OnBodyItemPress")]
    public class Patch_AutopsyNoConfirm
    {
        public static bool Prepare()
        {
            return Settings.i.AutopsyNoConfirm;
        }
        public static bool Prefix(BaseItemCellGUI item_gui, AutopsyGUI __instance)
        {
            if (item_gui.item.id == "insertion_button_pseudoitem")
                return true;

            CraftDefinition craft_definition = AccessTools.Method(typeof(AutopsyGUI), "GetExtractCraftDefinition").Invoke(__instance, new object[] { item_gui.item }) as CraftDefinition;
            if (craft_definition == null)
                return true;

            AccessTools.Method(typeof(AutopsyGUI), "RemoveBodyPartFromBody").Invoke(null, new object[] { __instance.GetField<Item>("_body"), item_gui.item });
            __instance.GetField<WorldGameObject>("_autopti_obj").components.craft.CraftAsPlayer(craft_definition, item_gui.item, null, null, false, 1);
            __instance.Hide(true);

            return false;
        }
    }

    //[HarmonyPatch(typeof(CraftComponent), "ProcessFinishedCraft")]
    public class Patch_AutoDropIntoStorage
    {
        public static bool IsPlayer;

        public static bool Prepare()
        {
            return Settings.i.AutoDropIntoStorage;
        }

        public static void __Prefix(CraftComponent __instance, out int? __state)
        {
            __state = null;
            Main.Log($"Patch_AutoDropIntoStorage start");
            if (__instance.current_craft == null || __instance.current_craft.flag != 0)
                return;
            Main.Log($"Patch_AutoDropIntoStorage: id={__instance.current_craft.id}");

            //if (__instance.current_craft != null) Patch_GameBalance.PrintAll("crafts\\" + __instance.current_craft.id.Replace(':','.') + ".json", __instance.current_craft);

            var other_obj = __instance.GetField<WorldGameObject>("other_obj");

            if (__instance.current_craft != null
                && __instance.current_craft.flag != 1
                && (other_obj == null || !other_obj.is_player)
                && (__instance.current_craft.is_auto || Settings.i.ZombieDropTechOrbs)
                && __instance.current_craft.craft_type != CraftDefinition.CraftType.Survey
                && __instance.wgo.obj_id != "tavern_kitchen" && __instance.wgo.obj_id != "tavern_oven"
                && !__instance.current_craft.IsBodyPartExtractionCraft()
                && !__instance.current_craft.IsBodyPartInsertionCraft()
                && (__instance.current_craft.output_to_wgo == null || __instance.current_craft.output_to_wgo.Count == 0)
                && !(__instance.current_craft.IsMultiqualityOutput() && __instance.GetField("_multiquality_craft_result") != null))
            {
                __instance.wgo.OnBeganObjectModifications();
                List<Item> list = new List<Item>();
                list = ResModificator.ProcessItemsListBeforeDrop(__instance.current_craft.output, __instance.wgo,
                    __instance.HasLinkedWorker() ? MainGame.me.player : other_obj);

                Main.Log("Patch_AutoDropIntoStorage: output=" + string.Join(", ", list));

                List<Item> tech_points = list.Where(s => s.is_tech_point).ToList();
                list.RemoveAll(s => s.is_tech_point);


                List<Item> list2;

                if (__instance.HasLinkedWorker() && !__instance.wgo.CanPutToAllPossibleInventories(list, out list2))
                {
                    Main.Log("Pausing worker");
                    __instance.wgo.progress = 0.999999f;
                    typeof(CraftComponent).Invoke("SetWorkerPausedMode", __instance, true);
                    __instance.SetField("_time_worker_tried_to_continue_craft", 1f);
                    __instance.wgo.linked_worker.components.character.SetNoWorkerTool();
                }
                else
                {
                    if (tech_points.Count > 0)
                        __instance.wgo.DropItems(tech_points);

                    __instance.wgo.PutToAllPossibleInventories(list, out list2);
                    if (list2 != null && list2.Count > 0)
                    {
                        Main.Log("PutToAllPossibleInventories had leftovers: " + string.Join(", ", list2));
                        __instance.wgo.DropItems(list2);
                    }

                    if (__instance.current_craft.takes_item_durability)
                    {
                        //var _current_item = __instance.GetField<Item>("_current_item");
                        if (__instance.GetField<Item>("_dur_item") != null)
                            typeof(CraftComponent).Invoke("DropDurItem", __instance);
                    }

                    __state = __instance.current_craft.flag;
                    __instance.current_craft.flag = 1;
                }

            }

            return;
        }

        public static void __Postfix(CraftComponent __instance, int? __state)
        {
            if (__instance.current_craft != null && __state.HasValue)
                __instance.current_craft.flag = __state.Value;
        }

        public static void Prefix(CraftComponent __instance)
        {
            var other_obj = __instance.GetField<WorldGameObject>("other_obj");
            IsPlayer = (other_obj != null && other_obj.is_player);

            Main.Log($"Patch_AutoDropIntoStorage: id={__instance.current_craft?.id}");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            List<CodeInstruction> code = instr.ToList();

            int i = 0;
            int j = 0;
            foreach (var line in code)
            {
                if (line.opcode == OpCodes.Callvirt && (line.operand as MethodInfo) == typeof(WorldGameObject).GetMethod("DropItems"))
                {
                    line.operand = typeof(Patch_AutoDropIntoStorage).GetMethod("InjectionDropItem");
                    ++i;
                }
                else if (line.opcode == OpCodes.Call && (line.operand as MethodInfo) == typeof(ResModificator).GetMethod("ProcessItemsListBeforeDrop"))
                {
                    line.operand = typeof(Patch_AutoDropIntoStorage).GetMethod("InjectionProcessItems");
                    if (++j >= 2)
                        break;
                }
            }

            if (i != 1 && j != 2)
                Main.Log("Exception: Critical failure while patching CraftComponent: " + i.ToString() + j.ToString());

            return code;
        }

        public static List<Item> InjectionProcessItems(List<Item> items, WorldGameObject wgo, WorldGameObject character)
        {
            List<Item> result = ResModificator.ProcessItemsListBeforeDrop(items, wgo, character);

            if (Settings.i.ZombieDropTechOrbs)
            {
                List<Item> techorbs = new List<Item>();
                foreach (Item item in result)
                {
                    if (item.is_tech_point)
                        techorbs.Add(item);
                }
                result.RemoveAll(s => s.is_tech_point);

                wgo.DropItems(techorbs);
            }

            return result;
        }

        public static void InjectionDropItem(WorldGameObject wgo, List<Item> items, Direction direction)
        {
            if (Settings.i.AutoDropIntoStorage
                && (!IsPlayer || items.Count == 0 || items[0].definition == null || items[0].definition.stack_count > 1 || Settings.i.AutoDropIntoStoragePlayerStacks)
                && (!IsPlayer || Settings.i.AutoDropIntoStoragePlayerAll))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i].is_tech_point)
                    {
                        items.RemoveAt(i);
                        i--;
                    }
                }

                List<Item> items2;
                wgo.PutToAllPossibleInventories(items, out items2);
                if (items2 != null && items2.Count > 0)
                {
                    wgo.DropItems(items2, Direction.None);
                }
            }
            else
            {
                wgo.DropItems(items, direction);
            }
        }

    }

    ////[HarmonyPatch(typeof(CraftComponent), "ReallyUpdateComponent")]   //works, but have no use ATM
    public class Patch_CraftSpeed
    {
        public static void Prefix(ref float delta_time, CraftComponent __instance)
        {
            if (__instance.wgo.obj_id == "mf_preparation_2")
                delta_time *= 10;
        }
    }

    #endregion

    #region Buffs

    //[HarmonyPatch(typeof(GameSave), nameof(GameSave.GenerateBody))]
    public class Patch_SkullBuff
    {
        public static System.Random rng = new System.Random();

        public static bool Prepare()
        {
            return Settings.i.CorpseBuff;
        }

        public static void Prefix(ref int tier_min, ref int tier_max)
        {
            if (tier_min <= 3 && BuffsLogics.FindBuffByID("buff_skull") != null)
            {
                tier_min = 3;
                tier_max = 3;
                Main.Log("Tier3");
            }
        }

        public static void Postfix(Item __result, int tier_min)
        {
            if (tier_min <= 3 && BuffsLogics.FindBuffByID("buff_skull") != null)
            {
                switch (rng.Next(1, 7))
                {
                    case 1:
                        __result.inventory.RemoveAll(s => s.id.StartsWith("brain"));
                        __result.inventory.Add(new Item("brain:brain_-2_3", 1));
                        break;
                    case 2:
                        __result.inventory.RemoveAll(s => s.id.StartsWith("heart"));
                        __result.inventory.Add(new Item("heart:heart_-2_3", 1));
                        break;
                    case 3:
                        __result.inventory.RemoveAll(s => s.id.StartsWith("intestine"));
                        __result.inventory.Add(new Item("intestine:intestine_-2_3", 1));
                        break;
                    case 4:
                        __result.inventory.RemoveAll(s => s.id.StartsWith("brain"));
                        __result.inventory.Add(new Item("brain:brain_3_3", 1));
                        break;
                    case 5:
                        __result.inventory.RemoveAll(s => s.id.StartsWith("heart"));
                        __result.inventory.Add(new Item("heart:heart_3_3", 1));
                        break;
                    case 6:
                        __result.inventory.RemoveAll(s => s.id.StartsWith("intestine"));
                        __result.inventory.Add(new Item("intestine:intestine_3_3", 1));
                        break;
                }
            }
        }

    }

    //[HarmonyPatch(typeof(BuffsLogics), nameof(BuffsLogics.RecalculateBuffs))]
    public class Patch_InfiniteBuffDuration
    {
        public static bool Prepare()
        {
            return Settings.i.InfiniteBuffs;
        }

        public static bool Prefix()
        {
            return false;
        }
    }

    ////[HarmonyPatch(typeof(PlayerBuff), nameof(PlayerBuff.GetTimerText))]
    public class Patch_BuffSymbol
    {
        public static bool Prepare()
        {
            return Settings.i.InfiniteBuffs;
        }

        public static bool Prefix(ref string __result)
        {
            __result = "\u221E";
            return false;
        }
    }

    #endregion
}
