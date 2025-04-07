using System;
using System.Globalization;
using UnityEngine;

namespace TheOutsider.World_Hooks
{
    public class RoomHooks
    {

        public static void Init()
        {
            On.RoomCamera.Update += RoomCamera_Update;
            //On.Room.ctor += Room_ctor;
            On.Room.Update += Room_Update;
            On.DynamicSoundLoop.Update += DynamicSoundLoop_Update;
            On.WaterFall.DrawSprites += WaterFall_DrawSprites;
            On.DaddyLongLegs.Update += DaddyLongLegs_Update;
            On.DaddyCorruption.Update += DaddyCorruption_Update;
            On.AbstractCreature.Update += AbstractCreature_Update;
            On.RainCycle.ctor += RainCycle_ctor;
            /*
            Hook hook = new Hook(
                typeof(RainWorldGame.SetupValues).GetProperty("disableRain", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(),
                typeof(RoomHooks).GetMethod("DisableRain", BindingFlags.Static | BindingFlags.Public));*/
        }

        public static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
        {
            orig(self);

            if (self.game.StoryCharacter != null && self.game.StoryCharacter == Plugin.SlugName && !self.room.IsGateRoom())
            {
                //Plugin.Log("RoomCamera_Update");
                if (self.game.world.region.name == "CC")
                {
                    self.currentPalette.darkness = 0.6f;
                    self.effect_darkness = 0.6f;
                    self.effect_desaturation = 0.65f;
                }
                if (self.game.world.region.name == "CL")
                {
                    self.currentPalette.darkness = 0.7f;
                    self.effect_darkness = 0.7f;
                    self.effect_desaturation = 0.65f;
                }
                if (self.game.world.region.name == "DS")
                {
                    self.currentPalette.darkness = 0.7f;
                    self.effect_darkness = 0.7f;
                    self.effect_desaturation = 0.45f;
                    //没水了
                    if (self.waterLight != null && !self.room.IsGateRoom())
                    {
                        self.waterLight.sprite.isVisible = false;
                    }
                }
                if (self.game.world.region.name == "GW")
                {
                    self.currentPalette.darkness = 0.6f;
                    self.effect_darkness = 0.6f;
                    self.effect_desaturation = 0.55f;
                }
                if (self.game.world.region.name == "HI")
                {
                    self.currentPalette.darkness = 0.7f;
                    self.effect_darkness = 0.7f;
                    self.effect_desaturation = 0.65f;
                }
                if (self.game.world.region.name == "LF")
                {
                    self.currentPalette.darkness = 0.5f;
                    self.effect_darkness = 0.5f;
                    self.effect_desaturation = 0.45f;
                }
                if (self.game.world.region.name == "MS")
                {
                    self.currentPalette.darkness = 0.2f;
                    self.effect_darkness = 0.2f;
                    self.effect_desaturation = 0.15f;
                }
                if (self.game.world.region.name == "SB")
                {
                    //Plugin.Log("RoomCamera_Update 1");
                    self.currentPalette.darkness = 0.5f;
                    self.effect_darkness = 0.5f;
                    self.effect_desaturation = 0.45f;
                }
                if (self.game.world.region.name == "SI")
                {
                    self.currentPalette.darkness = 0.5f;
                    self.effect_darkness = 0.5f;
                    self.effect_desaturation = 0.45f;
                }
                if (self.game.world.region.name == "SL")
                {
                    self.currentPalette.darkness = 0.5f;
                    self.effect_darkness = 0.5f;
                    self.effect_desaturation = 0.55f;
                }
                if (self.game.world.region.name == "SU")
                {
                    self.currentPalette.darkness = 0.5f;
                    self.effect_darkness = 0.5f;
                    self.effect_desaturation = 0.55f;
                }
                if (self.game.world.region.name == "VS")
                {
                    self.currentPalette.darkness = 0.5f;
                    self.effect_darkness = 0.5f;
                    self.effect_desaturation = 0.55f;
                }
            }
        }

        public static void Room_Update(On.Room.orig_Update orig, Room self)
        {
            orig(self);

            if (self.game.StoryCharacter != null && self.game.StoryCharacter == Plugin.SlugName)
            {
                //除海岸线、沉没巨构无雪
                if (self.game.world.region.name != "MS" && self.game.world.region.name != "SL")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                }
                else//停雨的尝试
                {
                    self.world.rainCycle.dayNightCounter = 0;
                }
                //尘土设置
                if (self.game.world.region.name == "CC")
                {
                    self.roomSettings.Grime = float.Parse("0.7", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "CL")
                {
                    self.roomSettings.Grime = float.Parse("0.9", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "DS")
                {
                    self.roomSettings.Grime = float.Parse("0.5", NumberStyles.Any, CultureInfo.InvariantCulture);
                    //没水了
                    if (self.waterObject != null && !self.IsGateRoom())
                    {
                        self.waterObject.Destroy();
                        self.waterObject.fWaterLevel = float.MinValue;
                        for (int i = 0; i < self.waterObject.surfaces.GetLength(0); i++)
                        {
                            for (int j = 0; j < self.waterObject.surfaces[i].points.GetLength(0); j++)
                                for (int k = 0; k < self.waterObject.surfaces[i].points.GetLength(1); k++)
                                    self.waterObject.surfaces[i].points[j, k] = new Water.SurfacePoint(new Vector2(0, 0));
                        }
                    }
                }
                if (self.game.world.region.name == "GW")
                {
                    self.roomSettings.Grime = float.Parse("0.9", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "HI")
                {
                    self.roomSettings.Grime = float.Parse("0.9", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "LF")
                {
                    self.roomSettings.Grime = float.Parse("0.7", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "MS")
                {
                    self.roomSettings.Grime = float.Parse("0.1", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "SB")
                {
                    self.roomSettings.Grime = float.Parse("0.3", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "SI")
                {
                    self.roomSettings.Grime = float.Parse("0.3", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "SL")
                {
                    self.roomSettings.Grime = float.Parse("0.1", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "SU")
                {
                    self.roomSettings.Grime = float.Parse("0.5", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "VS")
                {
                    self.roomSettings.Grime = float.Parse("0.7", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
            }
        }

        public static void DynamicSoundLoop_Update(On.DynamicSoundLoop.orig_Update orig, DynamicSoundLoop self)
        {
            orig(self);

            if (self.owner.room != null && self.owner.room.game != null &&
                self.owner.room.game.IsStorySession &&
                self.owner.room.game.StoryCharacter != null &&
                self.owner.room.game.StoryCharacter == Plugin.SlugName)
            {
                if (self.owner.room.world.region.name == "DS" && self.owner.room.waterObject != null && !self.owner.room.IsGateRoom())
                {
                    self.Stop();
                }
            }
        }
        /*
        public static void Water_Update(On.Water.orig_Update orig, Water self)
        {
            orig(self);

            if(self.room.game.session.characterStats.name == Plugin.SlugName)
            {
                if(self.room.world.region.name == "DS" && self.room.waterObject != null)
                {
                    self.room.waterObject.Destroy();
                    self.room.waterObject.fWaterLevel = float.MinValue;
                    self.waterSounds.destroyClipWhenDone = true;
                }
            }
        }

        public static void Water_BubbleEmitter_Update(On.Water.BubbleEmitter.orig_Update orig, Water.BubbleEmitter self)
        {
            orig(self);

            if (self.water.room.game.session.characterStats.name == Plugin.SlugName)
            {
                if (self.water.room.game.world.region.name == "DS")
                    self.amount = 0;
            }
        }
        */
        public static void WaterFall_DrawSprites(On.WaterFall.orig_DrawSprites orig, WaterFall self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (self.room.game.session.characterStats.name == Plugin.SlugName)
            {
                if (self.room.world.region.name == "DS" && !self.room.IsGateRoom())
                {
                    for (int i = 0; i < sLeaser.sprites.Length; i++)
                    {
                        sLeaser.sprites[i].isVisible = false;
                    }
                }
            }
        }

        public static void DaddyLongLegs_Update(On.DaddyLongLegs.orig_Update orig, DaddyLongLegs self, bool eu)
        {
            orig(self, eu);

            if (!ModManager.MSC || self.abstractCreature.world.game.session.characterStats.name.value != Plugin.SlugName.value)
            {
                return;
            }

            if (ModManager.MSC && (self as Creature).abstractCreature.superSizeMe)
            {
                self.effectColor = new Color(0.3f, 0f, 1f);
                self.eyeColor = self.effectColor;
            }
        }

        public static void DaddyCorruption_Update(On.DaddyCorruption.orig_Update orig, DaddyCorruption self, bool eu)
        {
            orig(self, eu);

            if (!ModManager.MSC || self.room.world.game.session.characterStats.name.value != Plugin.SlugName.value)
            {
                return;
            }
            self.effectColor = new Color(0f, 0f, 1f);
            self.eyeColor = self.effectColor;
        }

        public static void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
        {
            orig(self, time);

            if (ModManager.MSC && self.realizedCreature == null && self.Hypothermia < 1f && self.world.game.IsStorySession && self.world.game.session.characterStats.name == Plugin.SlugName)
            {
                if (self.world.region.name != "SL" && self.world.region.name != "MS")
                {
                    return;
                }
                if (self.InDen || self.HypothermiaImmune)
                {
                    self.Hypothermia = Mathf.Lerp(self.Hypothermia, 0f, 0.04f);
                    return;
                }
                self.Hypothermia = Mathf.Lerp(self.Hypothermia, 3f, Mathf.InverseLerp(0f, -600f, self.world.rainCycle.AmountLeft));
            }
        }
        /*
        public static void Room_ctor(On.Room.orig_ctor orig, Room self, RainWorldGame game, World world, AbstractRoom abstractRoom)
        {
            orig(self, game, world, abstractRoom);

            if (self.game.session.characterStats.name == Plugin.SlugName)
            {

            }

            if (self.fluorescentButterfliesfliesRoomAi != null)
            {
                self.fluorescentButterfliesfliesRoomAi.Update(self.game.evenUpdate);
            }
        }
        */

        //禁用避难所故障
        public static void RainCycle_ctor(On.RainCycle.orig_ctor orig, RainCycle self, World world, float minutes)
        {
            bool DisablePrecycles = MoreSlugcats.MoreSlugcats.cfgDisablePrecycles.Value;

            if (world.game.StoryCharacter != null && world.game.StoryCharacter == Plugin.SlugName)
            {
                MoreSlugcats.MoreSlugcats.cfgDisablePrecycles.Value = false;
            }

            orig(self, world, minutes);

            if (world.game.StoryCharacter != null && world.game.StoryCharacter == Plugin.SlugName)
            {
                MoreSlugcats.MoreSlugcats.cfgDisablePrecycles.Value = DisablePrecycles;
            }
        }

        public static bool DisableRain(Func<RainWorldGame, bool> orig, RainWorldGame self)
        {
            if (self.world.game.StoryCharacter == Plugin.SlugName)
            {
                return true;
            }

            return orig(self);
        }

        //public FluorescentButterfliesRoomAI fluorescentButterfliesfliesRoomAi;
    }
}
