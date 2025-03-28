using MonoMod.Cil;
using System;
using UnityEngine;
using Mono.Cecil.Cil;
using RWCustom;

namespace TheOutsider.Player_Hooks
{
    public class ClimbWall
    {
        public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (PlayerHooks.PlayerData.TryGetValue(self, out var player))
            {
                if (self.bodyMode == Player.BodyModeIndex.WallClimb)
                {
                    //爬墙上行
                    if (self.input[0].y > 0)
                    {
                        for (int i = 0; i < self.bodyChunks.Length; i++)
                            self.bodyChunks[i].pos.y = self.bodyChunks[i].lastPos.y + 1.2f;
                    }
                    //抓墙时不再下滑
                    else if (self.input[0].y == 0)
                    {
                        for (int i = 0; i < self.bodyChunks.Length; i++)
                            self.bodyChunks[i].pos.y = self.bodyChunks[i].lastPos.y;
                    }
                    //爬墙下行
                    else
                    {
                        for (int i = 0; i < self.bodyChunks.Length; i++)
                            self.bodyChunks[i].pos.y = self.bodyChunks[i].lastPos.y - 1.2f;
                    }
                    /*
                    if (self.input[0].x == 0 && !self.input[0].jmp)
                        for (int i = 0; i < self.bodyChunks.Length; i++)
                            self.bodyChunks[i].vel.x += self.bodyChunks[i].lastContactPoint.x;*/
                }
            }
        }

        //自动抓墙
        public static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
        {
            orig(self); 
            if (PlayerHooks.PlayerData.TryGetValue(self, out var player))
            {
                if (self.bodyChunks[0].ContactPoint.x != 0 && self.bodyMode == Player.BodyModeIndex.WallClimb &&
                !self.input[0].jmp && self.input[0].x == 0)
                {
                    self.input[0].x = self.bodyChunks[0].ContactPoint.x;
                }
            }
        }

        //手部动画
        /*
        public static void SlugcatHand_EngageInMovementIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //使轮到非圣徒线的海岸线、沉没巨构回响不会直接返回
                for (int k = 0; k < 2; k++)
                {
                    if (c.TryGotoNext(MoveType.After,
                   (i) => i.MatchLdfld<StoryGameSession>(nameof(StoryGameSession.saveStateNumber)),
                   (i) => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Saint)),
                   (i) => i.Match(OpCodes.Call)))
                    {
                        Plugin.Log("World_SpawnGhostIL MatchFind 1!");
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
                   (i) => i.MatchLdsfld<SlugcatStats.Name>(nameof(SlugcatStats.Name.Red)),
                   (i) => i.Match(OpCodes.Call),
                   (i) => i.Match(OpCodes.Call)))
                {
                    Plugin.Log("World_SpawnGhostIL MatchFind 2!");
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
        */
        public static bool SlugcatHand_EngageInMovement(On.SlugcatHand.orig_EngageInMovement orig, SlugcatHand self)
        {
            bool isWallClimb = self.owner != null && (self.owner.owner as Player).bodyMode == Player.BodyModeIndex.WallClimb;
            if (PlayerHooks.PlayerData.TryGetValue(self.owner.owner as Player, out var player) && isWallClimb)
                (self.owner.owner as Player).bodyMode = Player.BodyModeIndex.Default;

            bool result = orig(self);

            if (PlayerHooks.PlayerData.TryGetValue(self.owner.owner as Player, out player) && isWallClimb)
            {
                (self.owner.owner as Player).bodyMode = Player.BodyModeIndex.WallClimb;

                result = false;
                self.mode = Limb.Mode.HuntAbsolutePosition;
                self.huntSpeed = 6f;//12f;
                self.quickness = 0.5f;// 0.7f;
                if ((self.limbNumber == 0 ||
                     (Mathf.Abs((self.owner as PlayerGraphics).hands[0].pos.y - self.owner.owner.bodyChunks[0].pos.y) < 10f && (self.owner as PlayerGraphics).hands[0].reachedSnapPosition)) &&
                    !Custom.DistLess(self.owner.owner.bodyChunks[0].pos, self.absoluteHuntPos, 29f))
                {
                    Vector2 absoluteHuntPos = self.absoluteHuntPos;
                    self.FindGrip(self.owner.owner.room,
                        self.connection.pos + new Vector2(0f, (float)(self.owner.owner as Player).input[0].y * 20f),
                        self.connection.pos + new Vector2(0f, (float)(self.owner.owner as Player).input[0].y * 20f),
                        100f,
                        new Vector2(self.owner.owner.room.MiddleOfTile(self.owner.owner.bodyChunks[0].pos).x + (self.owner.owner.bodyChunks[0].ContactPoint.x) * 10f,
                                    self.owner.owner.bodyChunks[0].pos.y + (float)(self.owner.owner as Player).input[0].y * 28f),
                        -self.owner.owner.bodyChunks[0].ContactPoint.x, 2, false);
                    if (self.absoluteHuntPos != absoluteHuntPos)
                    {
                    }
                }
            }
            return result;
        }
    }
}
