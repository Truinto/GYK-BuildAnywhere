using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BuildAnywhere
{
    [HarmonyPatch]
    public static class Patch_TimeScale
    {
        [HarmonyTargetMethod]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (var mi in typeof(WaitingGUI).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                if (mi.Name.StartsWith("<Open>"))
                {
                    Debug.Log($"Patch_TimeScale match {mi.FullDescription()}");
                    yield return mi;
                }
            }
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
        {
            foreach (var line in instr)
            {
                if (line.opcode == OpCodes.Ldc_R4)
                {
                    float value = Convert.ToSingle(line.operand);

                    if (value == 10f)                    
                        line.operand = 10f * 5f;                    
                    else if (value == 0.083333336f)                    
                        line.operand = 0.083333336f * 5f;
                    else                    
                        Debug.Log($"Wups: unexpected TimeScale value {value}");                    
                    Debug.Log($"TimeScale inc {line.operand}");
                }

                yield return line;
            }
        }
    }

    [HarmonyPatch(typeof(WaitingGUI), "<Open>b__11_0")]
    public static class Patch_TimeScale2
    {
        public static void Postfix()
        {
            Time.timeScale = 50f;
            Time.fixedDeltaTime = 0.41666666f;
        }
    }
}
