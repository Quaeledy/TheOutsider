using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using Expedition;

namespace TheOutsider.Player_Hooks
{
    public class JumpHooks
    {
        //static float upJumpTime = 0;

        public static void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            orig(self);

            if (!PlayerHooks.PlayerData.TryGetValue(self, out var player) || !player.IsOutsider)
            {
                return;
            }
            
            if (!player.isFlying && self.canJump <= 0 && 
                self.bodyMode != Player.BodyModeIndex.Crawl && 
                self.bodyMode != Player.BodyModeIndex.CorridorClimb && 
                self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && 
                self.animation != Player.AnimationIndex.HangFromBeam && 
                self.animation != Player.AnimationIndex.ClimbOnBeam && 
                self.bodyMode != Player.BodyModeIndex.WallClimb && 
                self.bodyMode != Player.BodyModeIndex.Swimming && 
                self.Consious && !self.Stunned && 
                self.animation != Player.AnimationIndex.AntlerClimb && 
                self.animation != Player.AnimationIndex.VineGrab && 
                self.animation != Player.AnimationIndex.ZeroGPoleGrab)
            {
                self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.wingSpeed * 0.5f;
                //self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f * (1f - 1f * Mathf.Cos(upJumpTime * 2 * 3.14159f / 7f));
                //upJumpTime++;
            }
            /*
            else if(self.canJump > 0)
            {
                upJumpTime = 0;
            }*/
        }
    }
}
