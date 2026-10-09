namespace BuildAnywhere
{
    [HarmonyPatch(typeof(AutopsyGUI), nameof(AutopsyGUI.OnBodyItemPress))]
    public class Patch_AutopsyNoConfirm
    {
        public static bool Prepare()
        {
            return Settings.Instance.AutopsyNoConfirm;
        }
        public static bool Prefix(BaseItemCellGUI item_gui, AutopsyGUI __instance)
        {
            if (item_gui.item.id == "insertion_button_pseudoitem")
                return true;

            var craft_definition = __instance.GetExtractCraftDefinition(item_gui.item);
            if (craft_definition == null)
                return true;

            AutopsyGUI.RemoveBodyPartFromBody(__instance._body, item_gui.item);
            __instance._autopti_obj.components.craft.CraftAsPlayer(craft_definition, item_gui.item);
            __instance.Hide(true);

            return false;
        }
    }
}
