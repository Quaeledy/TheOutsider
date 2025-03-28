using Mono.Cecil.Cil;
//using Debug = UnityEngine.Debug;
using MonoMod.Cil;
using System;

namespace TheOutsider.Menu_Hooks
{
    public class IntroRollHooks
    {
        public static void InitIL()
        {
            IL.Menu.IntroRoll.ctor += IntroRoll_ctorIL;
            //On.Menu.IntroRoll.ctor += IntroRoll_ctor;
        }

        public static ILCursor text;
        public static bool logged;

        public static void IntroRoll_ctorIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //在开局标题中加入outsider
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Dup),
                    (i) => i.Match(OpCodes.Ldc_I4_4),
                    (i) => i.MatchLdstr("saint"),
                    (i) => i.Match(OpCodes.Stelem_Ref),
                    (i) => i.Match(OpCodes.Stloc_3)))
                {
                    c.Emit(OpCodes.Ldloc_3);
                    c.EmitDelegate<Func<string[], string[]>>((slugcat) =>
                    {
                        int length = slugcat.Length;
                        Array.Resize<string>(ref slugcat, length + 1);
                        slugcat[slugcat.Length - 1] = "outsider";
                        return slugcat;
                    });
                    c.Emit(OpCodes.Stloc_3);
                }
                /*
                //测试用，固定让outsider出现
                if (c.TryGotoNext(MoveType.After,
                    (i) => i.Match(OpCodes.Ldc_I4_0),
                    (i) => i.Match(OpCodes.Ldloc_3),
                    (i) => i.Match(OpCodes.Ldlen),
                    (i) => i.Match(OpCodes.Conv_I4),
                    (i) => i.Match(OpCodes.Call)))
                {
                    c.EmitDelegate<Func<int, int>>((slugcatInt) =>
                    {
                        return 5;
                    });
                }*/
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        /*
        public static void IntroRoll_ctor(On.Menu.IntroRoll.orig_ctor orig, IntroRoll self, ProcessManager manager)
        {
            orig(self, manager);
            try
            {
                if (!logged)
                {
                    foreach(var instruct in text.Instrs)
                    {
                        Debug.Log("[Outsider]" + instruct.ToString());
                    }
                    logged = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        */
    }
}
