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
        //我选择直接复制粘贴重写jump
        public static void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            if (!PlayerHooks.PlayerData.TryGetValue(self, out var player) || !player.IsMoth)
            {
                orig(self);
            }
            else
            {
                self.feetStuckPos = null;
                self.pyroJumpDropLock = 40;
                self.forceSleepCounter = 0;
                if (self.PainJumps && ((self as Creature).grasps[0] == null || !((self as Creature).grasps[0].grabbed is Yeek)))
                {
                    self.gourmandExhausted = true;
                    self.aerobicLevel = 1f;
                }
                float num = Mathf.Lerp(1f, 1.15f, self.Adrenaline);
                if ((self as Creature).grasps[0] != null && self.HeavyCarry((self as Creature).grasps[0].grabbed) && !((self as Creature).grasps[0].grabbed is Cicada))
                {
                    num += Mathf.Min(Mathf.Max(0f, (self as Creature).grasps[0].grabbed.TotalMass - 0.2f) * 1.5f, 1.3f);
                }

                self.AerobicIncrease(self.isGourmand ? 0.75f : 1f);
                if (self.bodyMode == Player.BodyModeIndex.WallClimb)
                {
                    int direction;
                    if (self.canWallJump != 0)
                    {
                        direction = Math.Sign(self.canWallJump);
                    }
                    else if ((self as Creature).bodyChunks[0].ContactPoint.x != 0)
                    {
                        direction = -(self as Creature).bodyChunks[0].ContactPoint.x;
                    }
                    else
                    {
                        direction = -self.flipDirection;
                    }
                    self.WallJump(direction);
                    return;
                }
                if (!(self.bodyMode == Player.BodyModeIndex.CorridorClimb))
                {
                    if (self.animation == Player.AnimationIndex.LedgeGrab)
                    {
                        if (self.input[0].x != 0)
                        {
                            self.WallJump(-self.input[0].x);
                            return;
                        }
                    }
                    else if (self.animation == Player.AnimationIndex.ClimbOnBeam)
                    {
                        self.jumpBoost = 0f;
                        if (self.input[0].x != 0)
                        {
                            self.animation = Player.AnimationIndex.None;
                            if (self.PainJumps)
                            {
                                (self as Creature).bodyChunks[0].vel.y = 3f * num;
                                (self as Creature).bodyChunks[1].vel.y = 2f * num;
                                (self as Creature).bodyChunks[0].vel.x = 3f * self.flipDirection * num;
                                (self as Creature).bodyChunks[1].vel.x = 2f * self.flipDirection * num;
                            }
                            else if (self.isRivulet || player.IsMoth)
                            {
                                (self as Creature).bodyChunks[0].vel.y = 9f * num;
                                (self as Creature).bodyChunks[1].vel.y = 8f * num;
                                (self as Creature).bodyChunks[0].vel.x = 9f * self.flipDirection * num;
                                (self as Creature).bodyChunks[1].vel.x = 7f * self.flipDirection * num;
                            }
                            else if (self.isSlugpup)
                            {
                                (self as Creature).bodyChunks[0].vel.y = 7f * num;
                                (self as Creature).bodyChunks[1].vel.y = 6f * num;
                                (self as Creature).bodyChunks[0].vel.x = 5f * self.flipDirection * num;
                                (self as Creature).bodyChunks[1].vel.x = 4.5f * self.flipDirection * num;
                            }
                            else
                            {
                                (self as Creature).bodyChunks[0].vel.y = 8f * num;
                                (self as Creature).bodyChunks[1].vel.y = 7f * num;
                                (self as Creature).bodyChunks[0].vel.x = 6f * self.flipDirection * num;
                                (self as Creature).bodyChunks[1].vel.x = 5f * self.flipDirection * num;
                            }
                            self.room.PlaySound(SoundID.Slugcat_From_Vertical_Pole_Jump, (self as Creature).mainBodyChunk, false, 1f, 1f);
                            return;
                        }
                        if (self.input[0].y <= 0)
                        {
                            self.animation = Player.AnimationIndex.None;
                            (self as Creature).bodyChunks[0].vel.y = 2f * num;
                            if (self.input[0].y > -1)
                            {
                                (self as Creature).bodyChunks[0].vel.x = 2f * self.flipDirection * num;
                            }
                            self.room.PlaySound(SoundID.Slugcat_From_Vertical_Pole_Jump, (self as Creature).mainBodyChunk, false, 0.3f, 1f);
                            return;
                        }
                        if (self.slowMovementStun < 1 && self.slideUpPole < 1)
                        {
                            self.Blink(7);
                            for (int i = 0; i < 2; i++)
                            {
                                BodyChunk bodyChunk = (self as Creature).bodyChunks[i];
                                bodyChunk.pos.y = bodyChunk.pos.y + (self.isSlugpup ? 2.25f : 4.5f);
                                BodyChunk bodyChunk2 = (self as Creature).bodyChunks[i];
                                bodyChunk2.vel.y = bodyChunk2.vel.y + (self.isSlugpup ? 1f : 2f);
                            }
                            self.slideUpPole = 17;
                            self.room.PlaySound(SoundID.Slugcat_From_Vertical_Pole_Jump, (self as Creature).mainBodyChunk, false, 0.8f, 1f);
                            return;
                        }
                    }
                    else
                    {
                        if (self.animation == Player.AnimationIndex.Roll)
                        {
                            (self as Creature).bodyChunks[1].vel *= 0f;
                            (self as Creature).bodyChunks[1].pos += new Vector2(5f * self.rollDirection, 5f);
                            (self as Creature).bodyChunks[0].pos = (self as Creature).bodyChunks[1].pos + new Vector2(5f * self.rollDirection, 5f);
                            float t = Mathf.InverseLerp(0f, 25f, self.rollCounter);
                            (self as Creature).bodyChunks[0].vel = Custom.DegToVec(self.rollDirection * Mathf.Lerp(60f, 35f, t)) * Mathf.Lerp(9.5f, 13.1f, t) * num * (self.isSlugpup ? 0.65f : 1f);
                            (self as Creature).bodyChunks[1].vel = Custom.DegToVec(self.rollDirection * Mathf.Lerp(60f, 35f, t)) * Mathf.Lerp(9.5f, 13.1f, t) * num * (self.isSlugpup ? 0.65f : 1f);
                            BodyChunk bodyChunk3 = (self as Creature).bodyChunks[0];
                            bodyChunk3.vel.x = bodyChunk3.vel.x * (self.isRivulet || player.IsMoth ? 1.5f : 1f);
                            BodyChunk bodyChunk4 = (self as Creature).bodyChunks[1];
                            bodyChunk4.vel.x = bodyChunk4.vel.x * (self.isRivulet || player.IsMoth ? 1.5f : 1f);
                            self.animation = Player.AnimationIndex.RocketJump;
                            self.room.PlaySound(SoundID.Slugcat_Rocket_Jump, (self as Creature).mainBodyChunk, false, 1f, 1f);
                            self.rollDirection = 0;
                            return;
                        }
                        if (self.animation == Player.AnimationIndex.BellySlide)
                        {
                            float num2 = 9f;
                            if (self.isRivulet || player.IsMoth)
                            {
                                num2 = 18f;
                                if (self.isGourmand && ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-agility"))
                                {
                                    num2 = Mathf.Lerp(14f, 9f, self.aerobicLevel);
                                    self.AerobicIncrease(1f);
                                }
                            }
                            if (self.isSlugpup)
                            {
                                num2 = 6f;
                            }
                            if (!self.whiplashJump && self.input[0].x != -self.rollDirection)
                            {
                                float y = 8.5f;
                                if (self.isRivulet || player.IsMoth)
                                {
                                    y = 10f;
                                }
                                if (self.isSlugpup)
                                {
                                    y = 6f;
                                }
                                (self as Creature).bodyChunks[1].pos += new Vector2(5f * self.rollDirection, 5f);
                                (self as Creature).bodyChunks[0].pos = (self as Creature).bodyChunks[1].pos + new Vector2(5f * self.rollDirection, 5f);
                                (self as Creature).bodyChunks[1].vel = new Vector2(self.rollDirection * num2, y) * num * (self.longBellySlide ? 1.2f : 1f);
                                (self as Creature).bodyChunks[0].vel = new Vector2(self.rollDirection * num2, y) * num * (self.longBellySlide ? 1.2f : 1f);
                                self.animation = Player.AnimationIndex.RocketJump;
                                self.rocketJumpFromBellySlide = true;
                                self.room.PlaySound(SoundID.Slugcat_Rocket_Jump, (self as Creature).mainBodyChunk, false, 1f, 1f);
                                self.rollDirection = 0;
                                return;
                            }
                            self.animation = Player.AnimationIndex.Flip;
                            self.standing = true;
                            self.room.AddObject(new ExplosionSpikes(self.room, (self as Creature).bodyChunks[1].pos + new Vector2(0f, -(self as Creature).bodyChunks[1].rad), 8, 7f, 5f, 5.5f, 40f, new Color(1f, 1f, 1f, 0.5f)));
                            int num3 = 1;
                            int num4 = 1;
                            while (num4 < 4 && !self.room.GetTile((self as Creature).bodyChunks[0].pos + new Vector2((float)(num4 * -(float)self.rollDirection) * 15f, 0f)).Solid && !self.room.GetTile((self as Creature).bodyChunks[0].pos + new Vector2((float)(num4 * -(float)self.rollDirection) * 15f, 20f)).Solid)
                            {
                                num3 = num4;
                                num4++;
                            }
                            (self as Creature).bodyChunks[0].pos += new Vector2(self.rollDirection * -(num3 * 15f + 8f), 14f);
                            (self as Creature).bodyChunks[1].pos += new Vector2(self.rollDirection * -(num3 * 15f + 2f), 0f);
                            (self as Creature).bodyChunks[0].vel = new Vector2(self.rollDirection * (self.isRivulet || player.IsMoth ? -11f : -7f), self.isRivulet || player.IsMoth ? 12f : 10f);
                            (self as Creature).bodyChunks[1].vel = new Vector2(self.rollDirection * (self.isRivulet || player.IsMoth ? -11f : -7f), self.isRivulet || player.IsMoth ? 13f : 11f);
                            self.rollDirection = -self.rollDirection;
                            self.flipFromSlide = true;
                            self.whiplashJump = false;
                            self.jumpBoost = 0f;
                            self.room.PlaySound(SoundID.Slugcat_Sectret_Super_Wall_Jump, (self as Creature).mainBodyChunk, false, 1f, 1f);
                            if (self.pickUpCandidate != null && self.CanIPickThisUp(self.pickUpCandidate) && ((self as Creature).grasps[0] == null || (self as Creature).grasps[1] == null) && (self.Grabability(self.pickUpCandidate) == Player.ObjectGrabability.OneHand || self.Grabability(self.pickUpCandidate) == Player.ObjectGrabability.BigOneHand))
                            {
                                int graspUsed = (self as Creature).grasps[0] == null ? 0 : 1;
                                for (int j = 0; j < self.pickUpCandidate.grabbedBy.Count; j++)
                                {
                                    self.pickUpCandidate.grabbedBy[j].grabber.GrabbedObjectSnatched(self.pickUpCandidate.grabbedBy[j].grabbed, self);
                                    self.pickUpCandidate.grabbedBy[j].grabber.ReleaseGrasp(self.pickUpCandidate.grabbedBy[j].graspUsed);
                                }
                                self.SlugcatGrab(self.pickUpCandidate, graspUsed);
                                if (self.pickUpCandidate is PlayerCarryableItem)
                                {
                                    (self.pickUpCandidate as PlayerCarryableItem).PickedUp(self);
                                }
                                if (self.pickUpCandidate.graphicsModule != null)
                                {
                                    self.pickUpCandidate.graphicsModule.BringSpritesToFront();
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (self.animation == Player.AnimationIndex.AntlerClimb)
                            {
                                self.animation = Player.AnimationIndex.None;
                                self.jumpBoost = 0f;
                                (self as Creature).bodyChunks[0].vel = self.playerInAntlers.antlerChunk.vel;
                                if (!self.playerInAntlers.dangle)
                                {
                                    (self as Creature).bodyChunks[1].vel = self.playerInAntlers.antlerChunk.vel;
                                }
                                if (self.playerInAntlers.dangle)
                                {
                                    if (self.input[0].x == 0)
                                    {
                                        BodyChunk bodyChunk5 = (self as Creature).bodyChunks[0];
                                        bodyChunk5.vel.y = bodyChunk5.vel.y + 3f;
                                        BodyChunk bodyChunk6 = (self as Creature).bodyChunks[1];
                                        bodyChunk6.vel.y = bodyChunk6.vel.y - 3f;
                                        self.standing = true;
                                        self.room.PlaySound(SoundID.Slugcat_Climb_Along_Horizontal_Beam, (self as Creature).mainBodyChunk, false, 1f, 1f);
                                    }
                                    else
                                    {
                                        BodyChunk bodyChunk7 = (self as Creature).bodyChunks[1];
                                        bodyChunk7.vel.y = bodyChunk7.vel.y + 4f;
                                        BodyChunk bodyChunk8 = (self as Creature).bodyChunks[1];
                                        bodyChunk8.vel.x = bodyChunk8.vel.x + 2f * self.input[0].x;
                                        BodyChunk bodyChunk9 = (self as Creature).bodyChunks[0];
                                        bodyChunk9.vel.y = bodyChunk9.vel.y + 6f;
                                        BodyChunk bodyChunk10 = (self as Creature).bodyChunks[0];
                                        bodyChunk10.vel.x = bodyChunk10.vel.x + 3f * self.input[0].x;
                                        self.room.PlaySound(SoundID.Slugcat_From_Vertical_Pole_Jump, (self as Creature).mainBodyChunk, false, 0.15f, 1f);
                                    }
                                }
                                else if (self.input[0].x == 0)
                                {
                                    if (self.input[0].y > 0)
                                    {
                                        BodyChunk bodyChunk11 = (self as Creature).bodyChunks[0];
                                        bodyChunk11.vel.y = bodyChunk11.vel.y + 4f * num;
                                        BodyChunk bodyChunk12 = (self as Creature).bodyChunks[1];
                                        bodyChunk12.vel.y = bodyChunk12.vel.y + 3f * num;
                                        self.jumpBoost = self.isSlugpup ? 7 : 8;
                                        self.room.PlaySound(SoundID.Slugcat_From_Horizontal_Pole_Jump, (self as Creature).mainBodyChunk, false, 1f, 1f);
                                        self.standing = true;
                                    }
                                    else
                                    {
                                        (self as Creature).bodyChunks[0].vel.y = 3f;
                                        (self as Creature).bodyChunks[1].vel.y = -3f;
                                        self.standing = true;
                                        self.room.PlaySound(SoundID.Slugcat_Climb_Along_Horizontal_Beam, (self as Creature).mainBodyChunk, false, 1f, 1f);
                                    }
                                }
                                else
                                {
                                    BodyChunk bodyChunk13 = (self as Creature).bodyChunks[0];
                                    bodyChunk13.vel.y = bodyChunk13.vel.y + 8f * num;
                                    BodyChunk bodyChunk14 = (self as Creature).bodyChunks[1];
                                    bodyChunk14.vel.y = bodyChunk14.vel.y + 7f * num;
                                    BodyChunk bodyChunk15 = (self as Creature).bodyChunks[0];
                                    bodyChunk15.vel.x = bodyChunk15.vel.x + 6f * self.input[0].x * num;
                                    BodyChunk bodyChunk16 = (self as Creature).bodyChunks[1];
                                    bodyChunk16.vel.x = bodyChunk16.vel.x + 5f * self.input[0].x * num;
                                    self.room.PlaySound(SoundID.Slugcat_From_Vertical_Pole_Jump, (self as Creature).mainBodyChunk, false, 1f, 1f);
                                }
                                Vector2 vector = (self as Creature).bodyChunks[0].vel - self.playerInAntlers.antlerChunk.vel + ((self as Creature).bodyChunks[1].vel - self.playerInAntlers.antlerChunk.vel) * (self.playerInAntlers.dangle ? 0f : 1f);
                                vector -= Custom.DirVec((self as Creature).mainBodyChunk.pos, self.playerInAntlers.deer.mainBodyChunk.pos) * vector.magnitude;
                                vector.x *= 0.1f;
                                vector = Vector2.ClampMagnitude(vector, 10f);
                                self.playerInAntlers.antlerChunk.vel -= vector * 1.2f;
                                self.playerInAntlers.deer.mainBodyChunk.vel -= vector * 0.25f;
                                self.playerInAntlers.playerDisconnected = true;
                                self.playerInAntlers = null;
                                return;
                            }
                            if (!(self.animation == Player.AnimationIndex.ZeroGSwim) && !(self.animation == Player.AnimationIndex.ZeroGPoleGrab))
                            {
                                int num5 = self.input[0].x;
                                bool flag = false;
                                if (self.animation == Player.AnimationIndex.DownOnFours && (self as Creature).bodyChunks[1].ContactPoint.y < 0 && self.input[0].downDiagonal == self.flipDirection)
                                {
                                    self.animation = Player.AnimationIndex.BellySlide;
                                    self.rollDirection = self.flipDirection;
                                    self.rollCounter = 0;
                                    self.standing = false;
                                    self.room.PlaySound(SoundID.Slugcat_Belly_Slide_Init, (self as Creature).mainBodyChunk, false, 1f, 1f);
                                    flag = true;
                                }
                                if (!flag)
                                {
                                    self.animation = Player.AnimationIndex.None;
                                    if (self.standing)
                                    {
                                        if (self.slideCounter > 0 && self.slideCounter < 10)
                                        {
                                            if (self.PainJumps)
                                            {
                                                (self as Creature).bodyChunks[0].vel.y = 4f * num;
                                                (self as Creature).bodyChunks[1].vel.y = 3f * num;
                                            }
                                            else
                                            {
                                                (self as Creature).bodyChunks[0].vel.y = (self.isRivulet || player.IsMoth ? 12f : 9f) * num;
                                                (self as Creature).bodyChunks[1].vel.y = (self.isRivulet || player.IsMoth ? 10f : 7f) * num;
                                            }
                                            BodyChunk bodyChunk17 = (self as Creature).bodyChunks[0];
                                            bodyChunk17.vel.x = bodyChunk17.vel.x * 0.5f;
                                            BodyChunk bodyChunk18 = (self as Creature).bodyChunks[1];
                                            bodyChunk18.vel.x = bodyChunk18.vel.x * 0.5f;
                                            BodyChunk bodyChunk19 = (self as Creature).bodyChunks[0];
                                            bodyChunk19.vel.x = bodyChunk19.vel.x - self.slideDirection * 4f * num;
                                            self.jumpBoost = 5f;
                                            if (self.isRivulet || player.IsMoth)
                                            {
                                                self.jumpBoost = 9f;
                                                if (self.isGourmand && ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-agility"))
                                                {
                                                    self.jumpBoost = Mathf.Lerp(8f, 2f, self.aerobicLevel);
                                                    self.AerobicIncrease(2f);
                                                }
                                            }
                                            if (self.isSlugpup)
                                            {
                                                self.jumpBoost = 3f;
                                            }
                                            self.animation = Player.AnimationIndex.Flip;
                                            self.room.PlaySound(SoundID.Slugcat_Flip_Jump, (self as Creature).mainBodyChunk, false, 1f, 1f);
                                            self.slideCounter = 0;
                                        }
                                        else
                                        {
                                            if (self.PainJumps)
                                            {
                                                (self as Creature).bodyChunks[0].vel.y = 2f * num;
                                                (self as Creature).bodyChunks[1].vel.y = 1f * num;
                                            }
                                            else
                                            {
                                                (self as Creature).bodyChunks[0].vel.y = (self.isRivulet || player.IsMoth ? 6f : 4f) * num;
                                                (self as Creature).bodyChunks[1].vel.y = (self.isRivulet || player.IsMoth ? 5f : 3f) * num;
                                            }
                                            self.jumpBoost = self.isSlugpup ? 7 : 8;
                                            self.room.PlaySound(self.bodyMode == Player.BodyModeIndex.ClimbingOnBeam ? SoundID.Slugcat_From_Horizontal_Pole_Jump : SoundID.Slugcat_Normal_Jump, (self as Creature).mainBodyChunk, false, 1f, 1f);
                                        }
                                    }
                                    else
                                    {
                                        float num6 = 1.5f;
                                        if (self.superLaunchJump >= 20)
                                        {
                                            self.superLaunchJump = 0;
                                            num6 = 9f;
                                            if (self.PainJumps)
                                            {
                                                num6 = 2.5f;
                                            }
                                            else if (self.isRivulet || player.IsMoth)
                                            {
                                                num6 = 12f;
                                                if (self.isGourmand && ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-agility"))
                                                {
                                                    num6 = Mathf.Lerp(8f, 3f, self.aerobicLevel);
                                                }
                                            }
                                            else if (self.isSlugpup)
                                            {
                                                num6 = 5.5f;
                                            }
                                            num5 = (self as Creature).bodyChunks[0].pos.x > (self as Creature).bodyChunks[1].pos.x ? 1 : -1;
                                            self.simulateHoldJumpButton = 6;
                                        }
                                        BodyChunk bodyChunk20 = (self as Creature).bodyChunks[0];
                                        bodyChunk20.pos.y = bodyChunk20.pos.y + 6f;
                                        if ((self as Creature).bodyChunks[0].ContactPoint.y == -1)
                                        {
                                            BodyChunk bodyChunk21 = (self as Creature).bodyChunks[0];
                                            bodyChunk21.vel.y = bodyChunk21.vel.y + 3f * num;
                                            if (num5 == 0)
                                            {
                                                BodyChunk bodyChunk22 = (self as Creature).bodyChunks[0];
                                                bodyChunk22.vel.y = bodyChunk22.vel.y + 3f * num;
                                            }
                                        }
                                        BodyChunk bodyChunk23 = (self as Creature).bodyChunks[1];
                                        bodyChunk23.vel.y = bodyChunk23.vel.y + 4f * num;
                                        self.jumpBoost = 6f;
                                        if (num5 != 0 && (self as Creature).bodyChunks[0].pos.x > (self as Creature).bodyChunks[1].pos.x == num5 > 0)
                                        {
                                            BodyChunk bodyChunk24 = (self as Creature).bodyChunks[0];
                                            bodyChunk24.vel.x = bodyChunk24.vel.x + num5 * num6 * num;
                                            BodyChunk bodyChunk25 = (self as Creature).bodyChunks[1];
                                            bodyChunk25.vel.x = bodyChunk25.vel.x + num5 * num6 * num;
                                            self.room.PlaySound(num6 >= 9f ? SoundID.Slugcat_Super_Jump : SoundID.Slugcat_Crouch_Jump, (self as Creature).mainBodyChunk, false, 1f, 1f);
                                        }
                                    }
                                    if ((self as Creature).bodyChunks[1].onSlope != 0)
                                    {
                                        if (num5 == -(self as Creature).bodyChunks[1].onSlope)
                                        {
                                            BodyChunk bodyChunk26 = (self as Creature).bodyChunks[1];
                                            bodyChunk26.vel.x = bodyChunk26.vel.x + (self as Creature).bodyChunks[1].onSlope * 8f * num;
                                            return;
                                        }
                                        BodyChunk bodyChunk27 = (self as Creature).bodyChunks[0];
                                        bodyChunk27.vel.x = bodyChunk27.vel.x + (self as Creature).bodyChunks[1].onSlope * 1.8f * num;
                                        BodyChunk bodyChunk28 = (self as Creature).bodyChunks[1];
                                        bodyChunk28.vel.x = bodyChunk28.vel.x + (self as Creature).bodyChunks[1].onSlope * 1.2f * num;
                                    }
                                }
                            }
                        }
                    }
                    return;
                }
            (self as Creature).bodyChunks[0].vel.y = 6f * num;
                (self as Creature).bodyChunks[1].vel.y = 5f * num;
                self.standing = true;
                if (self.isRivulet || player.IsMoth)
                {
                    self.jumpBoost = 14f;
                    return;
                }
                if (self.isSlugpup)
                {
                    self.jumpBoost = 4f;
                    return;
                }
                self.jumpBoost = 8f;
            }
        }
    }
}
