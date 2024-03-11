using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOutsider.Player_Hooks;
using UnityEngine;
using RWCustom;
using MoreSlugcats;
using HUD;
using System.Reflection;
using BepInEx.Logging;

namespace TheOutsider.World_Hooks
{
    class HUDHooks
    {
        public static void Init()
        {
            On.HUD.RainMeter.Update += RainMeter_Update;
            On.HUD.FoodMeter.QuarterPipShower.Update += QuarterPipShower_Update;
            On.MoreSlugcats.HypothermiaMeter.Update += HypothermiaMeter_Update;

            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
            On.HUD.HUD.Update += HUD_Update;
        }

        public static void RainMeter_Update(On.HUD.RainMeter.orig_Update orig, RainMeter self)
        {
            if (!(self.hud.owner is Player) || (self.hud.owner as Player).abstractCreature.world.game.session.characterStats.name != Plugin.SlugName)
            {
                orig(self);
            }
            else if ((self.hud.owner as Player).abstractCreature.world.region.name == "SL" || (self.hud.owner as Player).abstractCreature.world.region.name == "MS")
            {
                for (int i = 0; i < self.circles.Length; i++)
                {
                    self.circles[i].visible = false;
                }
            }
            else
            {
                bool flag = (self.hud.owner as Player).room != null;
                
                if (ModManager.MSC && (self.hud.owner as Player).inVoidSea)
                {
                    self.halfTimeShown = true;
                }
                self.lastPos = self.pos;
                self.pos = self.hud.karmaMeter.pos;
                if (!self.halfTimeShown && !flag && (self.hud.owner as Player).room != null && (self.hud.owner as Player).room.world.rainCycle.AmountLeft < 0.5f && (self.hud.owner as Player).room.roomSettings.DangerType != RoomRain.DangerType.None && !ModManager.MMF)
                {
                    self.halfTimeBlink = 220;
                    self.halfTimeShown = true;
                }
                self.lastFade = self.fade;
                if (self.remainVisibleCounter > 0)
                {
                    self.remainVisibleCounter--;
                }
                if (self.halfTimeBlink > 0)
                {
                    self.halfTimeBlink--;
                    self.hud.karmaMeter.forceVisibleCounter = Math.Max(self.hud.karmaMeter.forceVisibleCounter, 10);
                }
                if (ModManager.MMF && MMF.cfgTickTock.Value)
                {
                    self.tickPulse = Mathf.Lerp(self.tickPulse, 0f, 0.1f);
                }
                else
                {
                    self.tickPulse = 0f;
                }
                if ((self.hud.karmaMeter.fade > 0f && self.Show) || self.remainVisibleCounter > 0)
                {
                    self.fade = Mathf.Min(1f, self.fade + 0.033333335f);
                }
                else
                {
                    self.fade = Mathf.Max(0f, self.fade - 0.1f);
                }
                if (self.hud.HideGeneralHud)
                {
                    self.fade = 0f;
                }
                if (self.fade >= 0.7f)
                {
                    self.plop = Mathf.Min(1f, self.plop + 0.05f);
                }
                else
                {
                    self.plop = 0f;
                }
                if (flag)
                {
                    self.fRain = 1f;
                }
                else if ((self.hud.owner as Player).room != null)
                {
                    self.fRain = (self.hud.owner as Player).room.world.rainCycle.AmountLeft;
                }
                
                for (int i = 0; i < self.circles.Length; i++)
                {
                    self.circles[i].Update();

                    float num = (float)i / (float)(self.circles.Length - 1);
                    float value = Mathf.InverseLerp((float)i / (float)self.circles.Length, (float)(i + 1) / (float)self.circles.Length, self.fRain);
                    float num2 = Mathf.InverseLerp(0.5f, 0.475f, Mathf.Abs(0.5f - Mathf.InverseLerp(0.033333335f, 1f, value)));

                    self.circles[i].rad = (3f * Mathf.Pow(self.fade, 2f) + Mathf.InverseLerp(0.075f, 0f, Mathf.Abs(1f - num - Mathf.Lerp((1f - self.fRain) * self.fade - 0.075f, 1.075f, Mathf.Pow(self.plop, 0.85f)))) * 2f * self.fade) * Mathf.InverseLerp(0f, 0.033333335f, 1f);
                    self.circles[i].thickness = 1f;
                    self.circles[i].snapGraphic = HUDCircle.SnapToGraphic.smallEmptyCircle;
                    self.circles[i].snapRad = 3f;
                    self.circles[i].snapThickness = 1f;

                    self.circles[i].pos = self.pos + Custom.DegToVec((1f - (float)i / (float)self.circles.Length) * 360f * Custom.SCurve(Mathf.Pow(self.fade, 1.5f - num), 0.6f)) * (self.hud.karmaMeter.Radius + 8.5f + num2 + 4f * self.tickPulse);
                }
            }
        }
        
        // 减1/4饱食度动画修复
        public static void QuarterPipShower_Update(On.HUD.FoodMeter.QuarterPipShower.orig_Update orig, FoodMeter.QuarterPipShower self)
        {
            if ((self.owner.hud.owner is Player) && (self.owner.hud.owner as Player).abstractCreature.world.game.session.characterStats.name == Plugin.SlugName)
            {
                PlayerHooks.PlayerData.TryGetValue(self.owner.hud.owner as Player, out var player);
                if(player.shouldResetDisplayQuarterFood)
                {
                    self.displayQuarterFood = 4;
                    self.quarterPipDelay = 40;
                    player.shouldResetDisplayQuarterFood = false;
                    return;
                }
            }
            orig(self);
            if ((self.owner.hud.owner is Player) && (self.owner.hud.owner as Player).abstractCreature.world.game.session.characterStats.name == Plugin.SlugName)
            {
                if ((self.owner.IsPupFoodMeter ?
                (self.owner.pup.State as PlayerNPCState).quarterFoodPoints :
                (self.owner.hud.owner as Player).playerState.quarterFoodPoints) < self.displayQuarterFood)
                {
                    self.owner.visibleCounter = 80;
                    if (self.owner.fade < 0.5f)
                    {
                        self.quarterPipDelay = 45;
                        return;
                    }
                    if (self.quarterPipDelay > 0)
                    {
                        self.quarterPipDelay--;
                        return;
                    }
                    self.quarterPipDelay = 20;
                    self.displayQuarterFood--;
                    self.lightUp = 1f;
                    if (self.owner.showCount < self.owner.circles.Count)
                    {
                        self.owner.circles[self.owner.showCount].QuarterCirclePlop();
                    }
                }
            }
        }

        public static void HypothermiaMeter_Update(On.MoreSlugcats.HypothermiaMeter.orig_Update orig, HypothermiaMeter self)
        {
            orig(self);

            if (!(self.hud.owner is Player) || (self.hud.owner as Player).abstractCreature.world.game.session.characterStats.name != Plugin.SlugName)
            {
                return;
            }

            //在海岸线、沉没巨构之外隐藏寒冷条
            if((self.hud.owner as Player).abstractCreature.world.region.name != "SL" && (self.hud.owner as Player).abstractCreature.world.region.name != "MS")
            {
                for (int i = 0; i < self.circles.Length; i++)
                {
                    self.circles[i].visible = false;
                }
            }
            else
            {
                for (int i = 0; i < self.circles.Length; i++)
                {
                    self.circles[i].visible = true;
                }
            }
        }
        
        public static void HUD_Update(On.HUD.HUD.orig_Update orig, HUD.HUD self)
        {
            orig(self);
            if (outsiderHud != null)
                outsiderHud.Update();
        }

        private static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig(self, cam);
            
            //判断是否为蛾猫
            if (self.owner is Player && (self.owner as Player).abstractCreature.world.game.session.characterStats.name == Plugin.SlugName)
            {
                if (outsiderHud != null)
                    outsiderHud.Destroy();

                outsiderHud = new OutsiderMessionHud(self);
            }
        }

        public static OutsiderMessionHud outsiderHud;
    }

    class OutsiderMessionHud
    {
        public OutsiderMessionHud(HUD.HUD owner)
        {
            this.owner = owner;
        }

        public void Update()
        {
            if (owner == null)
                return;
            if (!(owner.owner is Player))
            {
                Destroy();
                return;
            }

            var room = (owner.owner as Player).room;
            if (room == null)
                return;

            //飞行教程
            if (!introText1 && room.roomSettings.name == "SB_TOPSIDE")
            {
                room.AddObject(new IntroText1(room));
                introText1 = true;
            }
            //闪光教程
            else if (!introText2 && (room.roomSettings.name == "SB_H03" || room.roomSettings.name == "LF_C03"))
            {
                room.AddObject(new IntroText2(room));
                introText2 = true;
            }
            //食素教程
            else if (!introText3 && (room.roomSettings.name == "SB_H03"))
            {
                room.AddObject(new IntroText3(room));
                introText3 = true;
            }
        }

        public void Destroy()
        {
            if (_hud != null)
                _hud.slatedForDeletion = true;
            owner = null;
            _hud = null;
        }

        static bool introText1 = false;
        static bool introText2 = false;
        static bool introText3 = false;
        MissionHud _hud;
        HUD.HUD owner;
    }

    class MissionHud : HudPart
    {
        public MissionHud(HUD.HUD hud, float aacc, ManualLogSource log) : base(hud)
        {
            acc[1] = aacc;
            _log = log;
        }
        public FSprite[] sprites;
        readonly float[] acc = new float[2] { 1f, 0.0f };
        readonly ManualLogSource _log;

    }
}
