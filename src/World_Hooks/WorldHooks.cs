using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using MoreSlugcats;
using BepInEx.Logging;
using UnityEngine;
//using Debug = UnityEngine.Debug;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace TheOutsider.World_Hooks
{
    public class WorldHooks
    {
        public static void Init()
        {
            IL.World.SpawnGhost += World_SpawnGhostIL;
            On.World.CheckForRegionGhost += World_CheckForRegionGhost;
        }

        private static void World_SpawnGhostIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //使轮到非圣徒线的海岸线、沉没巨构回响不会直接返回
                for(int k = 0; k < 2; k++)
                {
                    if (c.TryGotoNext(MoveType.After,
                   (i) => i.MatchLdfld<StoryGameSession>("saveStateNumber"),
                   (i) => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>("Saint"),
                   (i) => i.Match(OpCodes.Call)))
                    {
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<bool, World, bool>>((shouldReturn, self) =>
                        {
                            if (self.game.StoryCharacter != null && self.game.StoryCharacter == Plugin.SlugName)
                            {
                                shouldReturn = false;
                            }
                            return shouldReturn;
                        });
                    }
                }
                //使第一个雨循环不会生成回响
                if (c.TryGotoNext(MoveType.After,
                   (i) => i.MatchLdsfld<SlugcatStats.Name>("Red"),
                   (i) => i.Match(OpCodes.Call),
                   (i) => i.Match(OpCodes.Call)))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, World, bool>>((shouldSpawnGhost, self) =>
                    {
                        if (self.game.StoryCharacter != null && 
                            self.game.StoryCharacter == Plugin.SlugName && 
                            Custom.rainWorld.progression.currentSaveState.cycleNumber == 0)
                        {
                            shouldSpawnGhost = false;
                        }
                        return shouldSpawnGhost;
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static bool World_CheckForRegionGhost(On.World.orig_CheckForRegionGhost orig, SlugcatStats.Name slugcatIndex, string regionString)
        {
            bool result = orig(slugcatIndex, regionString);
            GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(regionString);
            if (ModManager.MSC &&
                (ghostID == MoreSlugcatsEnums.GhostID.SL || ghostID == MoreSlugcatsEnums.GhostID.MS) &&
                slugcatIndex == Plugin.SlugName)
            {
                result = true;
            }
            return result;
        }
    }
}
