using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using DevInterface;
using UnityEngine;
using Random = UnityEngine.Random;
using MoreSlugcats;
using Expedition;
using System.Globalization;
using TheOutsider.Player_Hooks;
using System.Runtime.CompilerServices;
using MonoMod;

namespace TheOutsider.World_Hooks
{
    public class RoomHooks
    {
        public static void Init()
        {
            On.RoomCamera.Update += RoomCamera_Update;
            On.Room.Update += Room_Update;
            On.DynamicSoundLoop.Update += DynamicSoundLoop_Update;
            On.WaterFall.DrawSprites += WaterFall_DrawSprites;
            On.DaddyLongLegs.Update += DaddyLongLegs_Update;
            On.DaddyCorruption.Update += DaddyCorruption_Update;
            On.AbstractCreature.Update += AbstractCreature_Update;


        }

        public static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
        {
            orig(self);

            if (self.game.session.characterStats.name.value == "Outsider")
            {
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
                    if(self.waterLight != null && !self.room.IsGateRoom())
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

            if (self.game.session.characterStats.name.value == "Outsider")
            {
                if (self.game.world.region.name == "CC")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                    self.roomSettings.Grime = float.Parse("0.7", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "CL")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                    self.roomSettings.Grime = float.Parse("0.9", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "DS")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                    self.roomSettings.Grime = float.Parse("0.5", NumberStyles.Any, CultureInfo.InvariantCulture);
                    //没水了
                    if (self.waterObject != null && !self.IsGateRoom())
                    {
                        self.waterObject.Destroy();
                        self.waterObject.fWaterLevel = float.MinValue;
                        for (int i = 0; i < self.waterObject.surface.GetLength(0); i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                self.waterObject.surface[i, j] = new Water.SurfacePoint(new Vector2(0, 0));
                            }
                        }
                    }
                }
                if (self.game.world.region.name == "GW")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                    self.roomSettings.Grime = float.Parse("0.9", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "HI")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                    self.roomSettings.Grime = float.Parse("0.9", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "LF")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                    self.roomSettings.Grime = float.Parse("0.7", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "MS")
                {
                    //沉没巨构有雪
                    self.roomSettings.Grime = float.Parse("0.1", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "SB")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                    self.roomSettings.Grime = float.Parse("0.3", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "SI")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                    self.roomSettings.Grime = float.Parse("0.3", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "SL")
                {
                    //海岸线有雪
                    self.roomSettings.Grime = float.Parse("0.1", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "SU")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                    self.roomSettings.Grime = float.Parse("0.5", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                if (self.game.world.region.name == "VS")
                {
                    self.snow = false;
                    self.roomSettings.DangerType = new RoomRain.DangerType("None", false);
                    self.roomSettings.Grime = float.Parse("0.7", NumberStyles.Any, CultureInfo.InvariantCulture);
                }
            }
        }

        public static void DynamicSoundLoop_Update(On.DynamicSoundLoop.orig_Update orig, DynamicSoundLoop self)
        {
            orig(self);

            if (self.owner.room.game.session.characterStats.name.value == "Outsider")
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

            if(self.room.game.session.characterStats.name.value == "Outsider")
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

            if (self.water.room.game.session.characterStats.name.value == "Outsider")
            {
                if (self.water.room.game.world.region.name == "DS")
                    self.amount = 0;
            }
        }
        */
        public static void WaterFall_DrawSprites(On.WaterFall.orig_DrawSprites orig, WaterFall self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (self.room.game.session.characterStats.name.value == "Outsider")
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

            if (!ModManager.MSC || self.room.world.game.session.characterStats.name.value != "Outsider")
            {
                return;
            }

            if (ModManager.MSC && (self as Creature).abstractCreature.superSizeMe)
            {
                self.effectColor = new Color(0.3f, 0f, 1f);
                self.eyeColor = self.effectColor;
            }
            else
            {
                self.effectColor = new Color(0f, 0f, 1f);
                self.eyeColor = self.effectColor;
            }
        }

        public static void DaddyCorruption_Update(On.DaddyCorruption.orig_Update orig, DaddyCorruption self, bool eu)
        {
            orig(self, eu);

            if (!ModManager.MSC || self.room.world.game.session.characterStats.name.value != "Outsider")
            {
                return;
            }
            self.effectColor = new Color(0f, 0f, 1f);
            self.eyeColor = self.effectColor;
        }

        public static void AbstractCreature_Update(On.AbstractCreature.orig_Update orig, AbstractCreature self, int time)
        {
            orig(self, time);
            
            if (ModManager.MSC && self.realizedCreature == null && self.Hypothermia < 1f && self.world.game.IsStorySession && self.world.game.session.characterStats.name.value == "Outsider")
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
    }
}
