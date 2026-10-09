namespace BuildAnywhere
{
    [HarmonyPatch(typeof(Item), nameof(Item.durability), MethodType.Getter)]
    public class Patch_NoItemDurability
    {
        public static bool Prepare()
        {
            return Settings.Instance.AllObjectsNoDurability;
        }

        public static void Prefix(Item __instance)
        {
            if (__instance.id != "soul_healed")
                __instance._params.durability = 1f;
        }
    }
}
