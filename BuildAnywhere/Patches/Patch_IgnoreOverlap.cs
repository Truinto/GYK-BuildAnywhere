
namespace BuildAnywhere
{
    [HarmonyPatch(typeof(FloatingWorldGameObject), nameof(FloatingWorldGameObject.RecalculateAvailability))]
    public class Patch_IgnoreOverlap
    {
        public static bool Prepare()
        {
            return Settings.Instance.ButtonIgnoreBuildOverlap != KeyCode.None;
        }

        public static void Postfix()
        {
            if (Input.GetKey(Settings.Instance.ButtonIgnoreBuildOverlap))
                FloatingWorldGameObject.can_be_built = true;
        }
    }
}
