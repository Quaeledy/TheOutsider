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
                if (self.input[0].y >= 0)
                {
                    if (self.bodyChunks[i].vel.y < -3f * player.flightSpeed * 0.45f)
                        player.flutterTimeAdd = 0;
                    self.bodyChunks[i].vel.y = Mathf.Clamp(self.bodyChunks[i].vel.y, -3f * player.flightSpeed * 0.35f, 3f * player.flightSpeed);
                }
                else
                    self.bodyChunks[i].vel.y = Mathf.Clamp(self.bodyChunks[i].vel.y, -3f * player.flightSpeed, 3f * player.flightSpeed);
            }
            if (self.input[0].x != 0)
            {
                self.bodyChunks[0].vel.y -= 1f;
                self.bodyChunks[1].vel.y += 1f;
            }
            if (player.flightTime > player.upFlightTime)
            {
                float scale = Mathf.Abs(Mathf.Cos(Custom.AimFromOneVectorToAnother(self.bodyChunks[0].pos, self.bodyChunks[1].pos) / 180f * Mathf.PI));
                float fac = Custom.LerpMap(scale, 1f, 0f, 
                    Custom.LerpMap((float)player.flightTime / (float)player.upFlightTime, 1f / 2f, 1f, 0.975f, 0.925f),
                    Custom.LerpMap((float)player.flightTime / (float)player.upFlightTime, 1f / 1.5f, 1f, 0.99f, 0.975f));
                for (int i = 0; i < self.bodyChunks.Length; i++)
                    self.bodyChunks[i].vel.x *= fac;
            }
        }

        //飞行加速度
        private static void FlightAcceleration(Player self, TheOutsider player)
        {
            Plugin.Log($"player.flightTime: {player.flightTime}, player.ax: {player.ax}, player.ay:  {player.ay}");
            float sa = 3f / 80f;//3f;
            float sv = 0.2f / 80f;//0.2f;
            float sf = 0.0012f / 40f;//0.0012f
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
                float lowGravityFac = 1f - self.room.gravity;

                #region 低重力
                float axLowG = player.ax;
                float ayLowG = player.ay;
                float vxLowG = self.bodyChunks[0].vel.x;
                float vyLowG = self.bodyChunks[0].vel.y;
                // x 方向
                if (self.input[0].x > 0)
                {
                    vxLowG += 0.65f;
                    axLowG = Mathf.Max(0f, axLowG + (sa * t + sv));
                }
                else if (self.input[0].x < 0)
                {
                    vxLowG -= 0.65f;
                    axLowG = Mathf.Min(0f, axLowG - (sa * t + sv));
                }
                else
                {
                    vxLowG *= 0.95f;
                    axLowG *= 0.95f;
                }

                if (self.input[0].x * self.bodyChunks[0].vel.x < 0)
                {
                    vxLowG *= 0.9f;
                    axLowG *= 0.9f;
                }
                // y 方向
                if (self.input[0].y > 0)
                {
                    vyLowG += 0.65f;
                    ayLowG = Mathf.Max(0f, ayLowG + (sa * t + sv));
                }
                else if (self.input[0].y < 0)
                {
                    vyLowG -= 0.65f;
                    ayLowG = Mathf.Min(0f, ayLowG - (sa * t + sv));
                }
                else
                {
                    vyLowG *= 0.95f;
                    ayLowG *= 0.95f;
                }

                if (self.input[0].y * self.bodyChunks[0].vel.y < 0)
                {
                    vyLowG *= 0.9f;
                    ayLowG *= 0.9f;
                }
                #endregion
                //正常重力
                float axHighG = player.ax;
                float ayHighG = player.ay;
                float vxHighG = self.bodyChunks[0].vel.x;
                float vyHighG = self.bodyChunks[0].vel.y;
                if (player.flightTime == 0)
                {
                    axHighG /= 2f;
                    ayHighG /= 2f;
                }

                //x方向加速度
                if (self.input[0].x > 0)
                {
                    axHighG = axHighG + (sa * t + sv - sf * Mathf.Pow(vxHighG, 2f));
                }
                else if (self.input[0].x < 0)
                {
                    axHighG = axHighG - (sa * t + sv - sf * Mathf.Pow(vxHighG, 2f));
                }
                else
                {
                    if (axHighG > 0.05f)
                    {
                        axHighG = Mathf.Max(axHighG - sv, 0f);
                    }
                    else if (axHighG < -0.05f)
                    {
                        axHighG = Mathf.Min(axHighG + sv, 0f);
                    }
                }
                if (self.input[0].x * vxHighG < 0 && self.input[0].x * self.input[1].x <= 0)//拍翅动画
                    player.flutterTimeAdd = 0f;

                //y方向加速度
                if (player.flightTime == 1 && self.input[0].y >= 0)
                {
                    ayHighG += 0.5f;
                }
                if (player.flightTime <= player.upFlightTime / 1.5f)
                {
                    ayHighG += (self.input[0].x == 0 ? 2f : 1.5f) * (self.input[0].y >= 0 ? 1f : -1f) * ss * t * 0.925f;
                    if (self.input[0].x != 0 && player.flightTime <= player.upFlightTime / 4f)
                        vyHighG += 0.5f;
                }
                else
                {/*
                        ayHighG *= 0.85f;
                        ayHighG = Mathf.Max(ayHighG - (self.input[0].x == 0 ? 1f : 0.5f) * (self.input[0].y >= 0 ? 0.75f : 1f) * ss,
                                              self.input[0].y >= 0 ? 0f : -5f);*/

                    ayHighG = self.input[0].y >= 0 ? 0f : -1f;
                    if (self.input[0].y >= 0)
                        vyHighG += (self.input[0].x == 0 ? 1.45f : 1.65f) * self.room.gravity;
                    else if (self.input[0].x != 0)
                    {
                        axHighG += 1.2f * sv * self.input[0].x;
                        vxHighG *= 1.1f;
                        vyHighG -= 1f * self.room.gravity;
                        self.bodyChunks[1].vel.x *= Mathf.Lerp(0.9f, 1f, lowGravityFac);
                        self.bodyChunks[1].vel.y += Mathf.Lerp(0.9f, 1f, lowGravityFac);
                    }

                    if (ayHighG < 0f && self.input[1].y < 0 && self.input[0].y >= 0)//拍翅动画
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

                    if (axHighG > 0)
                    {
                        axHighG = Mathf.Max(axHighG - 2f * ss, 0f);
                    }
                    else if (axHighG < 0)
                    {
                        axHighG = Mathf.Min(axHighG + 2f * ss, 0f);
                    }
                    ayHighG += 2f * ss * Mathf.Max(0f, 20f * Mathf.Abs(axHighG), Mathf.Abs(vxHighG) - 1f) * 0.05f;
                    vxHighG = Mathf.Sign(vxHighG) * Mathf.Max(Mathf.Abs(vxHighG) - 1f * ss, 0f);
                }

                //俯冲
                if (vyHighG < 0 &&
                    player.flightTime >= player.upFlightTime / 2f)
                {
                    //注意，vyHighG是负数，因此下面都进行了变号
                    if (self.input[0].x > 0)
                    {
                        axHighG -= sv * vyHighG * 0.25f;
                        ayHighG += sv * ss;
                    }
                    else if (self.input[0].x < 0)
                    {
                        axHighG += sv * vyHighG * 0.25f;
                        ayHighG += sv * ss;
                    }
                }
                #endregion

                if (player.flightTime == 0)
                {
                    if (axHighG * vxHighG < 0)
                        vxHighG /= 2.5f;
                    if ((vyHighG >= 0 && self.input[0].y < 0) ||
                        (vyHighG < 0 && self.input[0].y >= 0))
                        vyHighG /= 5f;
                }
                
                player.ax = Mathf.Lerp(axHighG, axLowG, lowGravityFac);
                player.ay = Mathf.Lerp(ayHighG, ayLowG, lowGravityFac);
                self.bodyChunks[0].vel.x = Mathf.Lerp(vxHighG, vxLowG, lowGravityFac);
                self.bodyChunks[0].vel.y = Mathf.Lerp(vyHighG, vyLowG, lowGravityFac);
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
            if (ModManager.CoopAvailable)
            {
                int i = -1;
                RainWorldGame game = Custom.rainWorld.processManager.currentMainLoop as RainWorldGame;
                if (game != null && game.Players != null)
                {
                    for (int j = 0; j < game.Players.Count; j++) 
                    {
                        if(self.abstractCreature == game.Players[j])
                        {
                            i = j;
                            break;
                        }
                    }
                }
                if (i == -1)
                    i = self.playerState.playerNumber;
                if (i > Plugin.optionsMenuInstance.flyKeyCode.Length || i < 0)
                    return self.wantToJump > 0;
                if (Plugin.optionsMenuInstance.flyKeyCode[i].Value == KeyCode.None || self.isNPC)
                {
                    return self.wantToJump > 0;
                }
                else
                {
                    return Input.GetKey(Plugin.optionsMenuInstance.flyKeyCode[i].Value);
                }
            }
            else
            {
                if (Plugin.optionsMenuInstance.flyKeyCode[0].Value == KeyCode.None || self.isNPC)
                {
                    return self.wantToJump > 0;
                }
                else
                {
                    return Input.GetKey(Plugin.optionsMenuInstance.flyKeyCode[0].Value);
                }
            }
        }
    }
}
