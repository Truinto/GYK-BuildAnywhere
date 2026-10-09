using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BuildAnywhere
{
    [HarmonyPatch]
    public static class Patch_Debugging
    {
        //[HarmonyPatch(typeof(CraftComponent), nameof(CraftComponent.CanStartCraftFromQueue))]
        //[HarmonyPrefix]
        public static void X1(ref bool use_player_inventory, bool start_by_player, CraftComponent __instance)
        {
            if (__instance.IsCraftQueueEmpty())
            {
                return;
            }

            //use_player_inventory = true;

            var usePlayerInventory = use_player_inventory ? MultiInventory.PlayerMultiInventory.IncludePlayer : MultiInventory.PlayerMultiInventory.ExcludePlayer;
            if (GlobalCraftControlGUI.is_global_control_active && !WorldZone.GetZoneOfObject(__instance.wgo).IsPlayerInZone())
                usePlayerInventory = MultiInventory.PlayerMultiInventory.ExcludePlayer;

            Debug.Log($"CanStartCraftFromQueue p-inventory={use_player_inventory} g-control={GlobalCraftControlGUI.is_global_control_active} p-zone={WorldZone.GetZoneOfObject(__instance.wgo).IsPlayerInZone()}");

            //MultiInventory multiInventory = __instance.wgo.GetMultiInventory(null, "", usePlayerInventory);
            MultiInventory multiInventory = Patch_SharedInventory.GlobalInventory;

            foreach (CraftComponent.CraftQueueItem item in __instance.craft_queue)
            {
                Debug.Log($" {item.id} -> inv:IsEnoughItems={multiInventory.IsEnoughItems(item.craft.needs)} wgo:IsEnoughItems={__instance.wgo.data.IsEnoughItems(item.craft.needs_from_wgo)} condition={item.craft.condition.EvaluateBoolean(__instance.wgo, MainGame.me.player)} by_player={start_by_player} not-grad={!item.is_gratitude_points_craft}");

                if (multiInventory.IsEnoughItems(item.craft.needs) && __instance.wgo.data.IsEnoughItems(item.craft.needs_from_wgo) && item.craft.condition.EvaluateBoolean(__instance.wgo, MainGame.me.player) && (start_by_player || !item.is_gratitude_points_craft || __instance.CanSpendPlayerGratitudePoints(item.craft.gratitude_points_craft_cost?.EvaluateFloat() ?? 0f)))
                {
                    //return; // item;
                }
            }
            return;
        }

        //[HarmonyPatch(typeof(WorldGameObject), nameof(WorldGameObject.GetMultiInventory))]
        //[HarmonyPrefix]
        public static void X2(List<WorldGameObject> exceptions, string force_world_zone, MultiInventory.PlayerMultiInventory player_mi, bool include_toolbelt, bool sortWGOS, bool include_bags, WorldGameObject __instance)
        {
            Debug.Log($" GetMultiInventory 1 {new System.Diagnostics.StackTrace().GetFrames().Join(a => a.GetMethod().Name)}");

            if (!new System.Diagnostics.StackTrace().GetFrames().Any(a => a.GetMethod().Name.StartsWith("CanStartCraftFromQueue")))
                return;

            MultiInventory result;

            MultiInventory multiInventory = (__instance.obj_def.open_in_multiinventory ? (__instance.IsWorker() ? new MultiInventory() : new MultiInventory(new Inventory(__instance))) : new MultiInventory());
            bool flag = false;
            bool flag2 = false;
            if (GlobalCraftControlGUI.is_global_control_active && !__instance.is_player)
            {
                WorldZone zoneOfObject = WorldZone.GetZoneOfObject(__instance);
                if (zoneOfObject != null && !zoneOfObject.IsPlayerInZone())
                {
                    player_mi = MultiInventory.PlayerMultiInventory.ExcludePlayer;
                    include_bags = false;
                    Debug.Log(" GetMultiInventory 2a");
                }
                else
                {
                    player_mi = MultiInventory.PlayerMultiInventory.IncludePlayer;
                    flag2 = true;
                    Debug.Log(" GetMultiInventory 2b");
                }
            }
            bool flag3 = false;
            if (player_mi == MultiInventory.PlayerMultiInventory.IncludePlayer || flag2 || __instance.is_player)
            {
                Debug.Log(" GetMultiInventory 3");
                Inventory inventory = new Inventory(MainGame.me.player);
                multiInventory.AddInventory(inventory);
                flag = true;
                if (include_toolbelt)
                {
                    Item item = new Item
                    {
                        inventory = MainGame.me.player.data.secondary_inventory,
                        inventory_size = 7
                    };
                    multiInventory.AddInventory(new Inventory(item));
                }
                int specific_position_num = 1;
                foreach (Item item2 in inventory.data.inventory)
                {
                    if (item2 != null && !item2.IsEmpty() && item2.is_bag)
                    {
                        multiInventory.AddInventory(new Inventory(item2, item2.id), specific_position_num);
                    }
                }
                flag3 = true;
            }
            if (include_bags && !flag3)
            {
                Debug.Log(" GetMultiInventory 4");
                foreach (Item item3 in __instance.data.inventory)
                {
                    if (item3 != null && !item3.IsEmpty() && item3.is_bag)
                    {
                        multiInventory.AddInventory(new Inventory(item3, item3.id));
                    }
                }
            }
            WorldZone worldZone = null;
            if (!__instance.is_player && __instance.IsWorker())
            {
                if (__instance.linked_workbench == null)
                {
                    Debug.Log(" GetMultiInventory 5");
                    result = multiInventory; goto end;
                }
                worldZone = (string.IsNullOrEmpty(force_world_zone) ? __instance.linked_workbench.GetMyWorldZone() : WorldZone.GetZoneByID(force_world_zone));
            }
            if (worldZone == null)
            {
                worldZone = (string.IsNullOrEmpty(force_world_zone) ? __instance.GetMyWorldZone() : WorldZone.GetZoneByID(force_world_zone));
            }
            if (worldZone != null)
            {
                List<Inventory> multiInventory2 = worldZone.GetMultiInventory(exceptions, flag ? MultiInventory.PlayerMultiInventory.ExcludePlayer : player_mi, include_toolbelt, sortWGOS);
                if (multiInventory2 != null)
                {
                    Debug.Log($" GetMultiInventory 6 flag={flag} player_mi={player_mi}");
                    foreach (Inventory item4 in multiInventory2)
                    {
                        multiInventory.AddInventory(item4);
                    }
                }
            }
            if (__instance.obj_def.additional_worldzone_inventories != null && __instance.obj_def.additional_worldzone_inventories.Count > 0)
            {
                Debug.Log(" GetMultiInventory 7");
                MultiInventory.PlayerMultiInventory playerMultiInventory = player_mi;
                if (playerMultiInventory == MultiInventory.PlayerMultiInventory.IncludePlayer)
                {
                    playerMultiInventory = MultiInventory.PlayerMultiInventory.DontChange;
                }
                {
                    foreach (string additional_worldzone_inventory in __instance.obj_def.additional_worldzone_inventories)
                    {
                        WorldZone zoneByID = WorldZone.GetZoneByID(additional_worldzone_inventory);
                        if (zoneByID == null || zoneByID == worldZone)
                        {
                            continue;
                        }
                        foreach (Inventory item5 in zoneByID.GetMultiInventory(null, playerMultiInventory, include_toolbelt: false, sortWGOS))
                        {
                            multiInventory.AddInventory(item5);
                        }
                    }
                    result = multiInventory; goto end;
                }
            }
            if (!__instance.is_player && __instance.IsWorker())
            {
                Debug.Log(" GetMultiInventory 8");
                WorldGameObject worldGameObject = __instance.linked_workbench;
                if (worldGameObject?.obj_def?.additional_worldzone_inventories != null && worldGameObject.obj_def?.additional_worldzone_inventories.Count > 0)
                {
                    Debug.Log(" GetMultiInventory 9");
                    MultiInventory.PlayerMultiInventory playerMultiInventory2 = player_mi;
                    if (playerMultiInventory2 == MultiInventory.PlayerMultiInventory.IncludePlayer)
                    {
                        Debug.Log(" GetMultiInventory 10");
                        playerMultiInventory2 = MultiInventory.PlayerMultiInventory.DontChange;
                    }
                    {
                        foreach (string additional_worldzone_inventory2 in worldGameObject.obj_def.additional_worldzone_inventories)
                        {
                            WorldZone zoneByID2 = WorldZone.GetZoneByID(additional_worldzone_inventory2);
                            if (zoneByID2 == null || zoneByID2 == worldZone)
                            {
                                continue;
                            }
                            foreach (Inventory item6 in zoneByID2.GetMultiInventory(null, playerMultiInventory2, include_toolbelt: false, sortWGOS))
                            {
                                multiInventory.AddInventory(item6);
                            }
                        }
                        result = multiInventory; goto end;
                    }
                }
            }
            Debug.Log(" GetMultiInventory 11");
            result = multiInventory; goto end;

        end:
            Debug.Log($" GetMultiInventory 12 count={result._inventories?.Count} {result._inventories.Join(a => a.name)}");
            return;
        }
    }
}
