using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using SlugBase;
using SlugBase.DataTypes;
using SlugBase.Features;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TheOutsider.Player_Hooks;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace TheOutsider.Player_Hooks
{
    public class PlayerHooks
    {
        public static ConditionalWeakTable<Player, PlayerEx> PlayerData = new();
        public static void Init()
        {
            On.Player.ctor += Player_ctor;
            On.Player.UpdateMSC += Player_Fly;
            On.Player.UpdateMSC += Player_Flare;
            On.Player.Jump += JumpHooks.Player_Jump;
            On.SporeCloud.Update += SporeCloudHooks.Update;
        }

        #region Player
        
        //进行飞行
        private static void Player_Fly(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);

            if (!PlayerData.TryGetValue(self, out var player) || !player.IsMoth)
            {
                return;
            }

            //飞行相关属性
            const float normalGravity = 0.9f;
            const float normalAirFriction = 0.999f;
            const float flightAirFriction = 0.7f;
            const float flightKickinDuration = 6f;

            float flightGravity = 0.3f;

            //如果可以飞行
            if (player.CanFly)
            {
                if (self.animation == Player.AnimationIndex.HangFromBeam || self.animation == Player.AnimationIndex.SurfaceSwim)
                {
                    player.preventFlight = 15;
                }
                else if (player.preventFlight > 0)
                {
                    player.preventFlight--;
                }

                //如果正在飞行
                if (player.isFlying)
                {
                    player.flyingBuzzSound.Volume = Mathf.Lerp(0.8f, 0.5f, player.currentFlightDuration / (4 * flightKickinDuration));

                    player.currentFlightDuration++;

                    if (flightGravity < 0.9f && player.currentFlightDuration >= 0.5f * player.UpFlytime)
                    {
                        flightGravity += 0.05f * (player.currentFlightDuration - 0.5f * player.UpFlytime);
                    }

                    self.AerobicIncrease(0.08f);

                    self.gravity = Mathf.Lerp(normalGravity, flightGravity, player.currentFlightDuration / flightKickinDuration);
                    self.airFriction = Mathf.Lerp(normalAirFriction, flightAirFriction, player.currentFlightDuration / flightKickinDuration);
                    
                    //消耗饱食度飞行
                    if (self.input[0].jmp && player.currentFlightDuration >= player.UpFlytime && self.FoodInStomach >= 0)
                    {
                        if (self.abstractCreature.world.game.Players.Count == 1)
                        {
                            if (self.FoodInStomach >= 1)
                            {
                                self.SubtractFood(1);
                                self.AddQuarterFood(); self.AddQuarterFood(); self.AddQuarterFood();
                                if (player.quarterFoodPoints == 0)
                                {
                                    player.quarterFoodPoints = 3;
                                }
                                player.currentFlightDuration = 0;
                            }
                            else if (player.quarterFoodPoints != 0)
                            {
                                if (player.quarterFoodPoints == 3)
                                {
                                    self.AddQuarterFood();
                                    self.SubtractFood(1);
                                    self.AddQuarterFood(); self.AddQuarterFood();
                                }
                                else if (player.quarterFoodPoints == 2)
                                {
                                    self.AddQuarterFood();
                                    self.AddQuarterFood();
                                    self.SubtractFood(1);
                                    self.AddQuarterFood();
                                }
                                else if (player.quarterFoodPoints == 1)
                                {
                                    self.AddQuarterFood();
                                    self.AddQuarterFood();
                                    self.AddQuarterFood();
                                    self.SubtractFood(1);
                                }
                                player.quarterFoodPoints--;
                                player.currentFlightDuration = 0;
                            }
                        }
                        else
                        {
                            if (self.FoodInStomach == 0)
                            {
                                return;
                            }
                            player.quarterFoodPoints--;
                            player.currentFlightDuration = 0;
                            if (player.quarterFoodPoints == 0)
                            {
                                self.SubtractFood(1);
                                player.quarterFoodPoints = 4;
                            }
                        }
                    }
                    
                    //飞行速度
                    if (self.input[0].x > 0 && player.currentFlightDuration >= 1.5f * player.UpFlytime)
                    {
                        self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + player.WingSpeed;
                        self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - 1.5f * (1f - 1f * Mathf.Cos(player.currentFlightDuration * 2 * 3.14159f / 7f));
                    }
                    else if (self.input[0].x > 0)
                    {
                        self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + player.WingSpeed;
                        self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - 1f;
                    }
                    else if (self.input[0].x < 0 && player.currentFlightDuration >= 1.5f * player.UpFlytime)
                    {
                        self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - player.WingSpeed;
                        self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + 1.5f * (1f - 1f * Mathf.Cos(player.currentFlightDuration * 2 * 3.14159f / 7f));
                    }
                    else if (self.input[0].x < 0)
                    {
                        self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - player.WingSpeed;
                        self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + 1f;
                    }
                    //低重力环境的飞行
                    if (self.room.gravity <= 0.5)
                    {
                        if (self.input[0].y > 0)
                        {
                            self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.WingSpeed;
                            self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f;
                        }
                        else if (self.input[0].y < 0)
                        {
                            self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - player.WingSpeed;
                            self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 1f;
                        }
                    }
                    //飞起来一段时间内持续上升
                    else if (player.currentFlightDuration <= player.UpFlytime)
                    {
                        self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.WingSpeed * 0.75f;
                        self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f * (1f - 1f * Mathf.Cos(player.currentFlightDuration * 2 * 3.14159f / 7f));
                    }
                    //无论怎样，你还可以俯冲
                    else if (self.input[0].y < 0)
                    {
                        self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - player.WingSpeed;
                        self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 1f;
                    }
                    else
                    {
                        self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 0.3f;
                    }

                    if (!player.CanSustainFlight(self))
                    {
                        player.StopFlight();
                    }
                }
                else
                {
                    player.flyingBuzzSound.Volume = 0f;

                    if (self.wantToJump > 0 && player.CanSustainFlight(self))
                    {
                        player.InitiateFlight();
                    }

                    self.airFriction = normalAirFriction;
                    self.gravity = normalGravity;
                }
            }

            if (player.preventGrabs > 0)
            {
                player.preventGrabs--;
            }

            player.flyingBuzzSound.Update();

            
            //player.MothSwallowTail(self.graphicsModule as PlayerGraphics);
        }

        //爆发闪光
        private static void Player_Flare(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);

            //检测是不是蛾猫
            if (!PlayerData.TryGetValue(self, out var player) || !player.IsMoth)
            {
                return;
            }

            //消耗饱食度爆发闪光
            if (self.wantToJump > 0 && self.input[0].pckp && player.burning == 0f && self.FoodInStomach > 0 && !player.isFlying && self.bodyMode == Player.BodyModeIndex.Stand)
            {
                self.jumpBoost = 0f;
                self.wantToJump = 0;
                self.SubtractFood(1);
                player.charged = true;
            }
            //开始闪光
            if (player.charged && player.burning == 0f && (self.bodyChunks[1].ContactPoint.x != 0 || self.bodyChunks[1].ContactPoint.y != 0))
            {
                player.burning = 0.01f;
                self.room.PlaySound(SoundID.Flare_Bomb_Burn, self.bodyChunks[1]);
            }
            //正在闪光
            if (player.burning > 0f)
            {
                player.burning += 0.016666668f;
                //结束闪光
                if (player.burning > 1f)
                {
                    player.charged = false;
                    player.burning = 0f;
                    return;
                }
                player.lastFlickerDir = player.flickerDir;
                player.flickerDir = Custom.DegToVec(Random.value * 360f) * 50f * player.LightIntensity;
                player.lastFlashAlpha = player.flashAplha;
                player.flashAplha = Mathf.Pow(Random.value, 0.3f) * player.LightIntensity;
                player.lastFlashRad = player.flashRad;
                player.flashRad = Mathf.Pow(Random.value, 0.3f) * player.LightIntensity * 200f * 16f;
                for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
                {
                    if (self.room.abstractRoom.creatures[i].realizedCreature != null && (Custom.DistLess(self.bodyChunks[1].pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, player.LightIntensity * 600f) || Custom.DistLess(self.bodyChunks[1].pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, player.LightIntensity * 1600f) && self.room.VisualContact(self.bodyChunks[1].pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos)))
                    {
                        //杀死小蜘蛛
                        if (self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Spider && !self.room.abstractRoom.creatures[i].realizedCreature.dead)
                        {
                            self.room.abstractRoom.creatures[i].realizedCreature.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                            self.room.abstractRoom.creatures[i].realizedCreature.Die();
                        }
                        //杀死狼蛛
                        else if (self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.BigSpider)
                        {
                            (self.room.abstractRoom.creatures[i].realizedCreature as BigSpider).poison = 1f;
                            (self.room.abstractRoom.creatures[i].realizedCreature as BigSpider).State.health -= Random.value * 0.2f;
                            self.room.abstractRoom.creatures[i].realizedCreature.Stun(Random.Range(10, 20));
                            //击杀生物计算
                            self.room.abstractRoom.creatures[i].realizedCreature.SetKillTag((self as Creature).abstractCreature);
                        }
                        //杀死烈焰狼蛛
                        else if (self.room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.SpitterSpider)
                        {
                            (self.room.abstractRoom.creatures[i].realizedCreature as BigSpider).poison = 1f;
                            (self.room.abstractRoom.creatures[i].realizedCreature as BigSpider).State.health -= Random.value * 0.2f;
                            self.room.abstractRoom.creatures[i].realizedCreature.Stun(Random.Range(10, 20));
                            //击杀生物计算
                            self.room.abstractRoom.creatures[i].realizedCreature.SetKillTag((self as Creature).abstractCreature);
                        }
                        //杀死蛛母
                        else if (self.room.abstractRoom.creatures[i].creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider)
                        {
                            (self.room.abstractRoom.creatures[i].realizedCreature as BigSpider).poison = 1f;
                            (self.room.abstractRoom.creatures[i].realizedCreature as BigSpider).State.health -= Random.value * 0.2f;
                            self.room.abstractRoom.creatures[i].realizedCreature.Stun(Random.Range(10, 20));
                            //击杀生物计算
                            self.room.abstractRoom.creatures[i].realizedCreature.SetKillTag((self as Creature).abstractCreature);
                        }
                        self.room.abstractRoom.creatures[i].realizedCreature.Blind((int)Custom.LerpMap(Vector2.Distance(self.bodyChunks[1].pos, self.room.abstractRoom.creatures[i].realizedCreature.VisionPoint), 60f, 600f, 400f, 20f));
                    }
                }
            }

            if (player.light != null)
            {
                if (self.room.Darkness(self.bodyChunks[1].pos) == 0f)
                {
                    player.light.Destroy();
                }
                else
                {
                    player.light.setPos = new Vector2?(self.bodyChunks[1].pos + self.bodyChunks[1].vel);
                    player.light.setAlpha = new float?(Mathf.Lerp(0.5f, 1f, Random.value) * (1f - 0.6f * player.LightIntensity));
                    player.light.setRad = new float?(Mathf.Max(player.flashRad, Mathf.Lerp(60f, 290f, Random.value) * 1f + player.LightIntensity * 10f));
                    player.light.color = player.LightColor;
                }
                if (player.light.slatedForDeletetion || player.light.room != self.room)
                {
                    player.light = null;
                    return;
                }
            }
            else if (self.room.Darkness(self.bodyChunks[1].pos) > 0f)
            {
                player.light = new LightSource(self.bodyChunks[1].pos, false, player.LightColor, self);
                self.room.AddObject(player.light);
            }
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            PlayerData.Add(self, new PlayerEx(self));
        }
        #endregion
    }
}
