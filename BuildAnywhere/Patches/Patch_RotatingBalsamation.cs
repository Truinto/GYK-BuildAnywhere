using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildAnywhere
{
    [HarmonyPatch(typeof(CraftComponent), nameof(CraftComponent.ProcessFinishedCraft))]
    public class Patch_RotatingBalsamation
    {
        public static void Postfix(CraftComponent __instance)
        {
            if (__instance.wgo.obj_id is "mf_balsamation_2" or "mf_balsamation_1")
            {
                switch (__instance.wgo.components?.craft?.last_craft_id)
                {
                    case "u_embalm2_1_1":
                        __instance.wgo.components.craft._last_craft_id = __instance.wgo.components.craft._last_craft_id_2 = "u_embalm2_0_1";
                        break;
                    case "u_embalm2_0_1":
                        __instance.wgo.components.craft._last_craft_id = __instance.wgo.components.craft._last_craft_id_2 = "u_embalm2_-1_1";
                        break;
                    case "u_embalm2_-1_1":
                        __instance.wgo.components.craft._last_craft_id = __instance.wgo.components.craft._last_craft_id_2 = "u_embalm2_-2_2";
                        break;
                    case "u_embalm2_-2_2":
                        __instance.wgo.components.craft._last_craft_id = __instance.wgo.components.craft._last_craft_id_2 = "u_embalm2_1_1";
                        break;

                    case "u_embalm_1_1":
                        __instance.wgo.components.craft._last_craft_id = __instance.wgo.components.craft._last_craft_id_2 = "u_embalm_0_1";
                        break;
                    case "u_embalm_0_1":
                        __instance.wgo.components.craft._last_craft_id = __instance.wgo.components.craft._last_craft_id_2 = "u_embalm_-1_1";
                        break;
                    case "u_embalm_-1_1":
                        __instance.wgo.components.craft._last_craft_id = __instance.wgo.components.craft._last_craft_id_2 = "u_embalm_-2_2";
                        break;
                    case "u_embalm_-2_2":
                        __instance.wgo.components.craft._last_craft_id = __instance.wgo.components.craft._last_craft_id_2 = "u_embalm_1_1";
                        break;

                    default:
                        Debug.Log("no matching u_embalm");
                        break;
                }
            }
        }
    }
}
