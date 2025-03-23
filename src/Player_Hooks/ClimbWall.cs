using MonoMod.Cil;
using System;
using UnityEngine;
using Mono.Cecil.Cil;

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
            if (self.bodyChunks[0].ContactPoint.x != 0 && self.bodyMode == Player.BodyModeIndex.WallClimb &&
                !self.input[0].jmp && self.input[0].x == 0)
            {
                self.input[0].x = self.bodyChunks[0].ContactPoint.x;
            }
        }

        /*
        //手部动画（导致抓墙结束）
        public static void Player_MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            if (PlayerHooks.PlayerData.TryGetValue(self, out var player))
            {
                if (self.bodyMode == Player.BodyModeIndex.WallClimb && self.input[0].y > 0)
                {
                    self.animation = Player.AnimationIndex.LedgeCrawl;
                }
            }
        }
        
        //自动抓墙
        public static void Player_MovementUpdateIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                if (c.TryGotoNext(MoveType.After,
                                     (i) => i.Match(OpCodes.Ldarg_0),
                                     (i) => i.MatchLdsfld<Player.BodyModeIndex>("WallClimb"),
                                     (i) => i.MatchStfld<Player>("bodyMode"), 
                                     (i) => i.Match(OpCodes.Ldarg_0)))
                {
                    c.EmitDelegate<Action<Player>>((self) =>
                    {
                        if ((self.bodyChunks[0].ContactPoint.x != 0 || self.bodyChunks[0].lastContactPoint.x != 0) && 
                            !self.input[0].jmp &&
                            (self.bodyChunks[0].ContactPoint.x == self.input[0].x || self.input[0].x == 0))
                        {
                            if (self.bodyChunks[0].lastContactPoint.x != self.bodyChunks[0].ContactPoint.x)
                            {
                                self.room.PlaySound(SoundID.Slugcat_Enter_Wall_Slide, self.mainBodyChunk, false, 1f, 1f);
                            }
                            self.bodyMode = Player.BodyModeIndex.WallClimb;
                        }
                    });
                    c.Emit(OpCodes.Ldarg_0);
                }
                else
                    Plugin.Log("Player_MovementUpdateIL HOOK FAILED");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        */
    }
}
