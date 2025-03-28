﻿using UnityEngine;

namespace TheOutsider.Player_Hooks
{
    public class Flight
    {
        //飞行相关属性
        private static float normalGravity = 0.9f;
        private static float flightGravity = 0.3f;
        private static float normalAirFriction = 0.999f;
        private static float flightAirFriction = 0.7f;
        private static float flightKickinDuration = 6f;

        //进行飞行
        public static void Player_Fly(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);

            if (PlayerHooks.PlayerData.TryGetValue(self, out var player))
            {
                if (self.animation == Player.AnimationIndex.HangFromBeam || self.animation == Player.AnimationIndex.SurfaceSwim)
                {
                    player.preventFlight = 15;
                }
                else if (self.bodyMode == Player.BodyModeIndex.WallClimb)
                {
                    player.preventFlight = 8;
                }
                else if (player.preventFlight > 0)
                {
                    player.preventFlight--;
                }

                //设置加速度
                FlightAcceleration(self, player);

                //如果正在飞行
                if (player.isFlying)
                {
                    //player.flyingBuzzSound.Volume = Mathf.Lerp(0.8f, 0.5f, player.flightTime / (4 * flightKickinDuration));

                    player.flightTime++;

                    if (flightGravity < 0.9f && player.flightTime >= 0.5f * player.upFlightTime)
                    {
                        flightGravity += 0.05f * (player.flightTime - 0.5f * player.upFlightTime);
                    }

                    self.AerobicIncrease(0.08f);

                    self.gravity = Mathf.Lerp(normalGravity, flightGravity, player.flightTime / flightKickinDuration);
                    self.airFriction = Mathf.Lerp(normalAirFriction, flightAirFriction, player.flightTime / flightKickinDuration);

                    PlayerState playerState = self.abstractCreature.world.game.Players.Count == 1 ?
                                      self.playerState :
                                      self.abstractCreature.world.game.Players[0].state as PlayerState;

                    if (FlyKeyCode(self) && player.flightTime >= player.upFlightTime && (self.FoodInStomach > 0 || playerState.quarterFoodPoints > 0 || Plugin.optionsMenuInstance.infiniteFlight.Value))
                    {
                        //消耗饱食度飞行
                        if (!Plugin.optionsMenuInstance.infiniteFlight.Value &&
                            !(self.grabbedBy != null && self.grabbedBy.Count > 0 && self.grabbedBy[0].grabbed is Player) &&
                            !(self.onBack != null))
                        {
                            FoodConsumption(self, player);
                        }
                        else
                        {
                            player.flightTime = 0;
                            player.flyEnergy += 50f;
                        }
                    }

                    //飞行速度
                    FlightSpeed(self, player);
                }
                else
                {
                    //player.flyingBuzzSound.Volume = 0f;

                    if (FlyKeyCode(self) && player.CanSustainFlight(self, player))
                    {
                        player.InitiateFlight(self, player);
                    }

                    self.airFriction = normalAirFriction;
                    self.gravity = normalGravity;
                }

                if (player.preventGrabs > 0)
                {
                    player.preventGrabs--;
                }

                //player.flyingBuzzSound.Update();
            }
        }

        //调整姿势
        public static void Player_MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
        {
            if (PlayerHooks.PlayerData.TryGetValue(self, out var player))
            {
                if (player.isFlying)
                {
                    self.bodyMode = Player.BodyModeIndex.Default;
                    self.animation = Player.AnimationIndex.None;

                    orig(self, eu);

                    if (!player.CanSustainFlight(self, player))
                    {
                        player.StopFlight();
                    }
                    else
                    {
                        if (self.input[0].x != 0)
                        {
                            self.bodyMode = Player.BodyModeIndex.Default;
                            self.animation = Player.AnimationIndex.LedgeCrawl;
                        }
                        else
                        {
                            self.bodyMode = Player.BodyModeIndex.Default;
                            self.animation = Player.AnimationIndex.None;
                        }
                    }
                }/*
                //自动抓墙
                else if (self.bodyMode == Player.BodyModeIndex.WallClimb && self.input[0].x == 0 && !self.input[0].jmp)
                {
                    Player.InputPackage inputPackage = new Player.InputPackage(false, Options.ControlSetup.Preset.None,
                        self.input[0].x, self.input[0].y,
                        self.input[0].jmp, self.input[0].thrw, self.input[0].pckp,
                        self.input[0].mp, self.input[0].crouchToggle);
                    inputPackage.x = self.bodyChunks[0].ContactPoint.x;
                    self.input[0] = inputPackage;
                }*/
            }

            orig(self, eu);
        }

        //消耗饱食度飞行
        private static void FoodConsumption(Player self, TheOutsider player)
        {

            PlayerState playerState = self.abstractCreature.world.game.Players.Count == 1 || player.isMothNPC ?
                                      self.playerState :
                                      self.abstractCreature.world.game.Players[0].state as PlayerState;

            //消耗饱食度飞行
            if (playerState.quarterFoodPoints == 0)
            {
                self.SubtractFood(1);
                playerState.quarterFoodPoints = 4;
                player.shouldResetDisplayQuarterFood = true;
            }
            playerState.quarterFoodPoints--;
            player.flightTime = 0;
        }

        //飞行速度
        private static void FlightSpeed(Player self, TheOutsider player)
        {
            self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + player.wingSpeed * player.ax / player.upFlightTime;
            self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.wingSpeed * player.ay / player.upFlightTime;

            self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + 1f * player.ax / player.upFlightTime;
            self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 1f * player.ay / player.upFlightTime;

            /*
            if (player.flightTime >= 1.5f * player.upFlightTime)//self.input[0].x > 0 && 
            {
                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + player.wingSpeed * ax / player.upFlightTime;
                /self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - 1.5f * (1f - 1f * Mathf.Cos(player.flightTime * 2 * 3.14159f / 7f));
            }
            else// if (self.input[0].x > 0)
            {
                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + player.wingSpeed * ax / player.upFlightTime;
                //self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - 1f;
            }
            else if (self.input[0].x < 0 && player.flightTime >= 1.5f * player.upFlightTime)
            {
                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - player.wingSpeed;
                //self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + 1.5f * (1f - 1f * Mathf.Cos(player.flightTime * 2 * 3.14159f / 7f));
            }
            else if (self.input[0].x < 0)
            {
                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - player.wingSpeed;
                //self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + 1f;
            }
            //低重力环境的飞行
            if (self.room.gravity <= 0.5)
            {
                if (self.input[0].y > 0)
                {
                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.wingSpeed * ay / player.upFlightTime;
                    //self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f;
                }
                else if (self.input[0].y < 0)
                {
                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - player.wingSpeed;
                    //self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 1f;
                }

                if (player.flightTime > player.upFlightTime)
                {
                    player.flightTime = 0;
                }
            }
            //飞起来一段时间内持续上升
            else if (player.flightTime <= player.upFlightTime)
            {
                self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.wingSpeed * 0.75f;
                //self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f * (1f - 1f * Mathf.Cos(player.flightTime * 2 * 3.14159f / 7f));
            }
            //无论怎样，你还可以俯冲
            else if (self.input[0].y < 0)
            {
                self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - player.wingSpeed;
                //self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 1f;
            }
            else
            {
                self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 0.3f;
            }*/
        }
        public static float num = 0;
        //飞行加速度
        private static void FlightAcceleration(Player self, TheOutsider player)
        {
            //Debug.Log(player.ax + " + " + player.ay);
            if (player.isFlying)
            {
                if (player.ay > 0)
                    num += player.ay;
                //Debug.Log(num);
                float a = Mathf.Max((player.upFlightTime / 1.5f - player.flightTime) / player.upFlightTime, 0f);

                if (self.room.gravity <= 0.5f)
                {
                    if (self.input[0].x > 0)
                    {
                        player.ax = player.ax + 3f * a + 0.2f;
                    }
                    if (self.input[0].x < 0)
                    {
                        player.ax = player.ax - 3f * a - 0.2f;
                    }
                    if (self.input[0].y > 0)
                    {
                        player.ay = player.ay + 3f * a + 0.2f;
                    }
                    if (self.input[0].y < 0)
                    {
                        player.ay = player.ay - 3f * a - 0.2f;
                    }
                }
                else
                {
                    if (player.flightTime == 0)
                    {
                        player.ax /= 2f;
                        player.ay /= 2f;
                    }

                    //x方向加速度
                    if (self.input[0].x > 0)
                    {
                        player.ax = player.ax + 3f * a + 0.2f - 0.0012f * Mathf.Pow(self.bodyChunks[0].vel.x, 2) * (1 - a);
                    }
                    else if (self.input[0].x < 0)
                    {
                        player.ax = player.ax - 3f * a - 0.2f + 0.0012f * Mathf.Pow(self.bodyChunks[0].vel.x, 2) * (1 - a);
                    }
                    else
                    {
                        if (player.ax > 0.05f)
                        {
                            player.ax = Mathf.Max(player.ax - 0.2f, 0f);
                        }
                        else if (player.ax < -0.05f)
                        {
                            player.ax = Mathf.Min(player.ax + 0.2f, 0f);
                        }
                        else
                        {
                            player.ay -= 0.1f;
                        }
                    }

                    //y方向加速度
                    if (player.flightTime <= player.upFlightTime)
                    {
                        player.ay += (self.input[0].x == 0 ? 2f : 1.5f) * a;
                    }
                    else if (self.input[0].y >= 0)
                    {
                        player.ay = Mathf.Max(player.ay - 1f, -5f);
                    }


                    //修正(空翻)
                    if (self.input[0].y > 0 && self.input[0].x == 0)
                    {
                        if (self.input[1].x != 0 && player.flightTime <= 2 * player.upFlightTime)//拍翅动画
                        {
                            player.flutterTimeAdd = 0f;
                        }

                        if (player.ax > 0)
                        {
                            player.ax = Mathf.Max(player.ax - 2f, 0f);
                            player.ay += 2f;
                        }
                        else if (player.ax < 0)
                        {
                            player.ax = Mathf.Min(player.ax + 2f, 0f);
                            player.ay += 2f;
                        }
                        else if (player.flightTime > player.upFlightTime)
                        {
                            player.ay = Mathf.Max(player.ay - 1f, -5f);
                        }
                    }
                    else if (self.input[0].y < 0)
                    {
                        player.ay = player.ay - 3f * a - 1f;
                    }


                    //俯冲
                    if (self.bodyChunks[0].vel.y < 0 && player.flightTime >= player.upFlightTime / 2)
                    {
                        //注意，self.bodyChunks[0].vel.y是负数，因此下面都进行了变号
                        if (self.input[0].x > 0)
                        {
                            player.ax -= 0.2f * self.bodyChunks[0].vel.y;
                            player.ay += 0.2f * self.bodyChunks[0].vel.y;
                        }
                        else if (self.input[0].x < 0)
                        {
                            player.ax += 0.2f * self.bodyChunks[0].vel.y;
                            player.ay += 0.2f * self.bodyChunks[0].vel.y;
                        }
                        else
                        {
                            if (player.ax > 0)
                            {
                                player.ax = Mathf.Max(player.ax - 1f, 0f);
                            }
                            else if (player.ax < 0)
                            {
                                player.ax = Mathf.Min(player.ax + 1f, 0f);
                            }
                        }
                    }
                }
            }
            else
            {
                player.ax = 0f;
                player.ay = 0f;

                num = 0f;
            }
        }

        //自定义飞行按键
        private static bool FlyKeyCode(Player self)
        {
            if (Plugin.optionsMenuInstance.flyKeyCode.Value == KeyCode.None || self.isNPC)
            {
                return self.wantToJump > 0;
            }
            else
            {
                return Input.GetKey(Plugin.optionsMenuInstance.flyKeyCode.Value);
            }
        }
    }
}
