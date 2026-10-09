namespace BuildAnywhere
{
    [HarmonyPatch]
    public static class Patch_QuickIncrease
    {
        [HarmonyPatch(typeof(CraftQueueItemGUI), nameof(CraftQueueItemGUI.OnIncreasePressed))]
        [HarmonyPrefix]
        public static bool StackIncrease(CraftQueueItemGUI __instance)
        {
            __instance._ci.infinite = false;
            if (__instance._is_current && __instance._ci.n == 0)
                CraftQueueGUI.current_instance.AddANewItemForCurrent(ref __instance._ci);
            else
                __instance._ci.n++;

            if (Input.GetKey(KeyCode.LeftShift))
                __instance._ci.n += 9;
            else if (Input.GetKey(KeyCode.LeftControl))
                __instance._ci.n += 99;

            __instance.Redraw();
            return false;
        }

        [HarmonyPatch(typeof(CraftQueueItemGUI), nameof(CraftQueueItemGUI.OnDecreasePressed))]
        [HarmonyPrefix]
        public static void StackDecrease(CraftQueueItemGUI __instance)
        {
            if (__instance._ci.n > 1)
            {
                if (Input.GetKey(KeyCode.LeftShift))
                    __instance._ci.n -= 9;
                else if (Input.GetKey(KeyCode.LeftControl))
                    __instance._ci.n -= 99;
                __instance._ci.n = Math.Max(1, __instance._ci.n);
            }
        }
    }
}
