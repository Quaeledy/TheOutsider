﻿using Mono.Cecil.Cil;
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
    public class Flare
    {
        //爆发闪光
        public static void Player_Flare(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);

            //检测是不是蛾猫
            if (!PlayerHooks.PlayerData.TryGetValue(self, out var player) || !player.IsOutsider)
            {
                return;
            }

            if (Plugin.optionsMenuInstance.neverFlare.Value)
            {
                return;
            }

            //消耗饱食度爆发闪光
            if (FlareKeyCode(self) && player.burning == 0f && self.FoodInStomach > 0 && !player.isFlying && self.bodyMode == Player.BodyModeIndex.Stand)
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
                        if (self.room.abstractRoom.creatures[i].realizedCreature is Spider && !self.room.abstractRoom.creatures[i].realizedCreature.dead)
                        {
                            self.room.abstractRoom.creatures[i].realizedCreature.firstChunk.vel += Custom.DegToVec(Random.value * 360f) * Random.value * 7f;
                            self.room.abstractRoom.creatures[i].realizedCreature.Die();
                        }
                        //杀死所有狼蛛
                        else if (self.room.abstractRoom.creatures[i].realizedCreature is BigSpider && !self.room.abstractRoom.creatures[i].realizedCreature.dead)
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

        private static bool FlareKeyCode(Player self)
        {
            if (Plugin.optionsMenuInstance.flareKeyCode.Value == KeyCode.None)
            {
                return self.wantToJump > 0 && self.input[0].pckp;
            }
            else
            {
                return Input.GetKey(Plugin.optionsMenuInstance.flareKeyCode.Value);
            }
        }
    }
}