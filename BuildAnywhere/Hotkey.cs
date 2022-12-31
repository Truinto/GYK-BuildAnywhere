using System.Collections.Generic;
using System.Threading;

namespace BuildAnywhere
{
    public class Hotkey
    {
        public static List<KeyState> Listener = new List<KeyState>();

        public static void Update()
        {
            // while (true)
            // {
            //     if (!MainGame.paused && MainGame.game_started && Input.GetKey(KeyCode.H))
            //     {
            //         if (!flag)
            //         {
            //             Debug.Log("Manual StoneTeleport");
            //             try
            //             {
            //                 GS.RunFlowScript("StoneTeleport", null);
            //             }
            //             catch (Exception ex)
            //             {
            //                 Debug.Log("Wups " + ex.ToString());
            //             }

            //             flag = true;
            //         }
            //     }
            //     else
            //     {
            //         flag = false;
            //     }

            //     Thread.Sleep(50);
            // }

            while (true)
            {
                if (!MainGame.paused && MainGame.game_started)
                {
                    foreach (var l in Listener)
                    {
                        Debug.Log("[BuildAnywhere] Checking for button: " + l.Key);

                        if (Input.GetKey(l.Key))
                            Debug.Log("[BuildAnywhere] Button pressed");

                        if (Input.GetKey(l.Key) != l.State)
                        {
                            Debug.Log("[BuildAnywhere] State triggered");
                            l.State = !l.State;
                            if (l.State == false)
                                Keypress?.Invoke(null, l.Key);
                        }
                    }
                }
                else
                {
                    foreach (var l in Listener)
                        l.State = false;
                }

                Thread.Sleep(50);
            }

        }

        public static event EventHandler<KeyCode> Keypress;

        private class Press : EventArgs
        {
            public KeyCode Key;
            public Press(KeyCode Key)
            {
                this.Key = Key;
            }
        }

        public class KeyState
        {
            public KeyCode Key;
            public bool State;
            public KeyState(KeyCode Key)
            {
                this.Key = Key;
                this.State = false;
            }

            public override bool Equals(object obj)
            {
                return this.Key == (obj as KeyState)?.Key;
            }

            public override int GetHashCode()
            {
                return (int)Key;
            }
        }


        ////[HarmonyPatch(typeof(GS), nameof(GS.RunFlowScript))]
        public static class Patch_test1
        {
            public static void Prefix(string uscript_name)
            {
                Debug.Log("HELLO WORLD: " + uscript_name);
            }
        }
    }
}
