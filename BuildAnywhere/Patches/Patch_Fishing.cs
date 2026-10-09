using Fishing;

namespace BuildAnywhere
{
    [HarmonyPatch]
    public class Patch_Fishing
    {
        [HarmonyPatch(typeof(FishLogic), nameof(FishLogic.CalculateFishPos))]
        [HarmonyPrepare]
        public static bool FishingAlwaysSuccess_()
        {
            return Settings.Instance.FishingAlwaysSuccess;
        }
        [HarmonyPatch(typeof(FishLogic), nameof(FishLogic.CalculateFishPos))]
        [HarmonyPrefix]
        public static void FishingAlwaysSuccess(ref float pos, ref float rod_zone_size)
        {
            pos = 0f;
            rod_zone_size = 100f;
        }


        [HarmonyPatch(typeof(FishingGUI), nameof(FishingGUI.UpdateWaitingForBite))]
        [HarmonyPrepare]
        public static bool NoWait_()
        {
            return Settings.Instance.FishAlwaysBiting;
        }
        [HarmonyPatch(typeof(FishingGUI), nameof(FishingGUI.UpdateWaitingForBite))]
        [HarmonyPrefix]
        public static void NoWait(FishingGUI __instance)
        {
            __instance._waiting_for_bite_delay = 0f;
            //AccessTools.Field(typeof(FishingGUI), "_waiting_for_bite_delay").SetValue(__instance, 0f);
        }


        [HarmonyPatch(typeof(FishingGUI), nameof(FishingGUI.UpdateWaitingForPulling))]
        [HarmonyPrepare]
        public static bool PullAutomatically_()
        {
            return Settings.Instance.FishPullAutomatically;
        }
        [HarmonyPatch(typeof(FishingGUI), nameof(FishingGUI.UpdateWaitingForPulling))]
        [HarmonyPostfix]
        public static void PullAutomatically(FishingGUI __instance)
        {
            __instance.ChangeState(FishingGUI.FishingState.Pulling);
            //AccessTools.Method(typeof(FishingGUI), "ChangeState").Invoke(__instance, new object[] { FishingGUI.FishingState.Pulling });
        }


        [HarmonyPatch(typeof(FishingGUI), nameof(FishingGUI.RemoveBait))]
        [HarmonyPrepare]
        public static bool KeepBait_()
        {
            return Settings.Instance.FishNoDurabiliyLoss;
        }
        [HarmonyPatch(typeof(FishingGUI), nameof(FishingGUI.RemoveBait))]
        [HarmonyPostfix]
        public static void KeepBait(Item bait)
        {
            if (bait.definition.has_durability)
                bait.durability = 1;
        }
    }
}
