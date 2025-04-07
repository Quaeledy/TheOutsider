using RWCustom;
using UnityEngine;

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
        public static void Player_Fly(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (PlayerHooks.PlayerData.TryGetValue(self, out var player))
            {
                if (self.room == null) return;
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

                    if (FlyKeyCode(self) && player.flightTime >= player.upFlightTime / 1.5f && 
                        (self.FoodInStomach > 0 || playerState.quarterFoodPoints > 0 || Plugin.optionsMenuInstance.infiniteFlight.Value))
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
                            //player.flyEnergy += 50f;
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
                }
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
            self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + player.flightSpeed * player.ax / player.upFlightTime;
            self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.flightSpeed * player.ay / player.upFlightTime;

            self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - 1f * player.ax / player.upFlightTime;
            self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f * player.ay / player.upFlightTime;
            
            for (int i = 0; i < self.bodyChunks.Length; i++)
            {
                self.bodyChunks[i].vel.x = Mathf.Clamp(self.bodyChunks[i].vel.x, -3f * player.flightSpeed, 3f * player.flightSpeed);
                self.bodyChunks[i].vel.y = Mathf.Clamp(self.bodyChunks[i].vel.y, -3f * player.flightSpeed, 3f * player.flightSpeed);
            }
            if (self.input[0].x != 0)
            {
                self.bodyChunks[0].vel.y -= 1f;
                self.bodyChunks[1].vel.y += 1f;
            }
            /*
            if (player.flightTime >= 1.5f * player.upFlightTime)//self.input[0].x > 0 && 
            {
                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + player.flightSpeed * ax / player.upFlightTime;
                /self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - 1.5f * (1f - 1f * Mathf.Cos(player.flightTime * 2 * 3.14159f / 7f));
            }
            else// if (self.input[0].x > 0)
            {
                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + player.flightSpeed * ax / player.upFlightTime;
                //self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - 1f;
            }
            else if (self.input[0].x < 0 && player.flightTime >= 1.5f * player.upFlightTime)
            {
                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - player.flightSpeed;
                //self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + 1.5f * (1f - 1f * Mathf.Cos(player.flightTime * 2 * 3.14159f / 7f));
            }
            else if (self.input[0].x < 0)
            {
                self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - player.flightSpeed;
                //self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + 1f;
            }
            //低重力环境的飞行
            if (self.room.gravity <= 0.5)
            {
                if (self.input[0].y > 0)
                {
                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.flightSpeed * ay / player.upFlightTime;
                    //self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f;
                }
                else if (self.input[0].y < 0)
                {
                    self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - player.flightSpeed;
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
                self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.flightSpeed * 0.75f;
                //self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f * (1f - 1f * Mathf.Cos(player.flightTime * 2 * 3.14159f / 7f));
            }
            //无论怎样，你还可以俯冲
            else if (self.input[0].y < 0)
            {
                self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - player.flightSpeed;
                //self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 1f;
            }
            else
            {
                self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 0.3f;
            }*/
        }

        //飞行加速度
        private static void FlightAcceleration(Player self, TheOutsider player)
        {
            //Plugin.Log($"player.flightTime: {player.flightTime}, player.ax: {player.ax}, player.ay:  {player.ay}");
            
            float sa = 3f / 60f;//3f;
            float sv = 0.2f / 60f;//0.2f;
            float sf = 0.0012f / 60f;//0.0012f
            float ss = 0.5f;//1f
            /*
            float sa = 3f;
            float sv = 0.2f;
            float sf = 0.0012f;
            float ss = 1f;
            */
            if (player.isFlying)
            {
                float t = Mathf.Max((float)(player.upFlightTime / 1.5f - player.flightTime) / ((float)player.upFlightTime / 1.5f), 0f);

                //低重力
                if (self.room.gravity <= 0.5f)
                {
                    if (self.input[0].x > 0)
                    {
                        self.bodyChunks[0].vel.x += 0.25f;
                        player.ax = Mathf.Max(0f, player.ax + sa * t + sv);
                    }
                    else if (self.input[0].x < 0)
                    {
                        self.bodyChunks[0].vel.x -= 0.25f;
                        player.ax = Mathf.Min(0f, player.ax - sa * t - sv);
                    }
                    else
                    {
                        self.bodyChunks[0].vel.x *= 0.9f;
                        player.ax *= 0.9f;
                    }
                    if (self.input[0].y > 0)
                    {
                        self.bodyChunks[0].vel.y += 0.25f;
                        player.ay = Mathf.Max(0f, player.ay + sa * t + sv);
                    }
                    else if (self.input[0].y < 0)
                    {
                        self.bodyChunks[0].vel.y -= 0.25f;
                        player.ay = Mathf.Min(0f, player.ay - sa * t - sv);
                    }
                    else
                    {
                        self.bodyChunks[0].vel.y *= 0.9f;
                        player.ay *= 0.9f;
                    }
                }
                //正常重力
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
                        player.ax = player.ax + sa * t + sv - sf * Mathf.Pow(self.bodyChunks[0].vel.x, 2f);
                    }
                    else if (self.input[0].x < 0)
                    {
                        player.ax = player.ax - sa * t - sv + sf * Mathf.Pow(self.bodyChunks[0].vel.x, 2f);
                    }
                    else
                    {
                        if (player.ax > 0.05f)
                        {
                            player.ax = Mathf.Max(player.ax - sv, 0f);
                        }
                        else if (player.ax < -0.05f)
                        {
                            player.ax = Mathf.Min(player.ax + sv, 0f);
                        }
                    }
                    if (self.input[0].x * self.bodyChunks[0].vel.x < 0 && self.input[0].x * self.input[1].x <= 0)//拍翅动画
                        player.flutterTimeAdd = 0f;

                    //y方向加速度
                    if (player.flightTime == 1 && self.input[0].y >= 0)
                    {
                        player.ay += 0.5f;
                    }
                    if (player.flightTime <= player.upFlightTime / 1.5f)
                    {
                        player.ay += (self.input[0].x == 0 ? 2f : 1.5f) * (self.input[0].y >= 0 ? 1f : -1f) * ss * t * 0.925f;
                    }
                    else
                    {/*
                        player.ay *= 0.85f;
                        player.ay = Mathf.Max(player.ay - (self.input[0].x == 0 ? 1f : 0.5f) * (self.input[0].y >= 0 ? 0.75f : 1f) * ss,
                                              self.input[0].y >= 0 ? 0f : -5f);*/

                        player.ay = self.input[0].y >= 0 ? 0f : -1f;
                        if (self.input[0].y >= 0)
                            self.bodyChunks[0].vel.y += (self.input[0].x == 0 ? 1.25f : 1.65f) * self.room.gravity;
                        else if (self.input[0].x != 0)
                        {
                            player.ax += 1.2f * sv * self.input[0].x;
                            self.bodyChunks[0].vel.x *= 1.1f;
                            self.bodyChunks[1].vel.x *= 0.9f;
                            self.bodyChunks[1].vel.y += 1.25f * self.room.gravity;
                        }

                        if (player.ay < 0f && self.input[1].y < 0 && self.input[0].y >= 0)//拍翅动画
                        {
                            player.flutterTimeAdd = 0f;
                        }
                    }

                    #region 额外修正
                    //修正(空翻)
                    if (self.input[0].y > 0 && self.input[0].x == 0)
                    {
                        if (self.input[1].x != 0 && player.flightTime <= 2f * player.upFlightTime)//拍翅动画
                        {
                            player.flutterTimeAdd = 0f;
                        }

                        if (player.ax > 0)
                        {
                            player.ax = Mathf.Max(player.ax - 2f * ss, 0f);
                        }
                        else if (player.ax < 0)
                        {
                            player.ax = Mathf.Min(player.ax + 2f * ss, 0f);
                        }
                        player.ay += 2f * ss * Mathf.Max(0f, Mathf.Abs(self.bodyChunks[0].vel.x) - 1f) * 0.05f;
                        self.bodyChunks[0].vel.x = Mathf.Sign(self.bodyChunks[0].vel.x) * Mathf.Max(Mathf.Abs(self.bodyChunks[0].vel.x) - 1f * ss, 0f);
                    }

                    //俯冲
                    if (self.bodyChunks[0].vel.y < 0 && 
                        player.flightTime >= player.upFlightTime / 2f)
                    {
                        //注意，self.bodyChunks[0].vel.y是负数，因此下面都进行了变号
                        if (self.input[0].x > 0)
                        {
                            player.ax -= sv * self.bodyChunks[0].vel.y * 0.5f;
                            player.ay += sv * ss;
                        }
                        else if (self.input[0].x < 0)
                        {
                            player.ax += sv * self.bodyChunks[0].vel.y * 0.5f;
                            player.ay += sv * ss;
                        }
                    }
                    #endregion
                    
                    if (player.flightTime == 0)
                    {
                        if (player.ax * self.bodyChunks[0].vel.x < 0)
                            self.bodyChunks[0].vel.x /= 2.5f;
                        if ((self.bodyChunks[0].vel.y >= 0 && self.input[0].y < 0) || 
                            (self.bodyChunks[0].vel.y < 0 && self.input[0].y >= 0))
                            self.bodyChunks[0].vel.y /= 5f;
                    }
                }
            }
            else
            {
                player.ax = 0f;
                player.ay = 0f;
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
