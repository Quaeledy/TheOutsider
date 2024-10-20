using SlugBase.Features;
using SlugBase;
using System.Linq;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using SlugBase.DataTypes;
using BepInEx.Logging;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;

namespace TheOutsider
{
    public class PlayerEx
    {
        public WeakReference<Player> playerRef;
        //public WeakReference<PlayerGraphics> iGraphicsRef; 
        
        public bool IsOutsider
        {
            get
            {
                if (playerRef.TryGetTarget(out Player player))
                    return player.SlugCatClass == Plugin.SlugName || player.SlugCatClass == Plugin.MothPup || isMothNPC;
                else
                    return false;
            }
        }

        public bool isMothNPC;

        
        public float flutterTimeAdd;
        public readonly float wingSpeed;
        public readonly float upFlightTime;
        public int preventGrabs;
        public bool isFlying;
        public int flightTime;
        public int preventFlight;
        public int lastTail;
        public bool shouldResetDisplayQuarterFood;

        //烟雾果毒性
        public bool deadForSporeCloud;
        public int deadForSporeCloudCount;

        //加速度
        public float ax;
        public float ay;
        //飞行能量
        public float flyEnergy;
        //姿势
        public bool spreadWings;
        public bool foldUpWings;
        //翅膀长度及宽度
        public float wingLength;
        public float wingWidth;
        //触须长度
        public float antennaeLength;

        //闪光相关
        public bool charged;
        public LightSource light;
        public Vector2 flickerDir;
        public Vector2 lastFlickerDir;
        public Color LightColor;
        public Color NullColor;
        public float flashRad;
        public float lastFlashRad;
        public float flashAplha;
        public float lastFlashAlpha;
        public float burning;
        public float LightIntensity
        {
            get
            {
                return Mathf.Pow(Mathf.Sin(burning * 3.1415927f), 0.4f);
            }
        }

        //图像
        public int wingSprite;
        public int antennaeSprite;
        public int FlareSprite;
        public int swallowtailSprite;
        public int speckleSprite;
        //手臂替代贴图
        public int handWingSprite;

        //身体部件
        public GenericBodyPart[] wing;
        public GenericBodyPart[] antennae;
        public TailSegment[,] swallowtail;

        public Vector2 lastAntennaePos;

        public int tailN;
        public readonly float swallowTailSpacing = 6f;
        public readonly float MaxLength = 10f;
        public readonly float swallowTailWidth = 0.4f;
        public float tailTimeAdd;

        //public DynamicSoundLoop flyingBuzzSound;

        //梦境相关
        //public DreamStateOverride stateOverride;

        public PlayerEx(Player player)
        {
            playerRef = new WeakReference<Player>(player);
            if (player.isNPC)
                isMothNPC = true;

            if (!IsOutsider)
                return;
            
            /*
            flyingBuzzSound = new ChunkDynamicSoundLoop(player.bodyChunks[0]);
            flyingBuzzSound.sound = MothEnums.MothBuzz;
            flyingBuzzSound.Pitch = 1f;
            flyingBuzzSound.Volume = 1f;
            */
            wingSpeed = 10;
            upFlightTime = 30;

            if (player.playerState.isPup || player.isSlugpup)
            {
                wingLength = 10f;
                wingWidth = 14f;
                antennaeLength = 0.1f;
            }
            else
            {
                wingLength = 15f;
                wingWidth = 20f;
                antennaeLength = 0.3f;
            }
        }

        public Color GetBodyColor()
        {
            if (!playerRef.TryGetTarget(out Player player) || player.graphicsModule == null)
                return new Color(40f / 255f, 102f / 255f, 141f / 255f);
            if (PlayerColor.Body.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)PlayerColor.Body.GetColor(player.graphicsModule as PlayerGraphics);
            return new Color(40f / 255f, 102f / 255f, 141f / 255f);
        }

        public Color GetEyesColor()
        {
            if (!playerRef.TryGetTarget(out Player player) || player.graphicsModule == null)
            {
                if (player.isNPC && Custom.RGB2HSL(player.ShortCutColor()).z < 0.5f)
                    return new Color(255f / 255f, 255f / 255f, 255f / 255f);
                else
                    return new Color(1f / 255f, 1f / 255f, 1f / 255f);
            }
            if (PlayerColor.Eyes.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)PlayerColor.Eyes.GetColor(player.graphicsModule as PlayerGraphics);

            if (player.isNPC && Custom.RGB2HSL(player.ShortCutColor()).z < 0.5f)
                return new Color(255f / 255f, 255f / 255f, 255f / 255f);
            else
                return new Color(1f / 255f, 1f / 255f, 1f / 255f);
        }

        public Color GetAntennaeColor()
        {
            if (!playerRef.TryGetTarget(out Player player) || player.graphicsModule == null)
                return new Color(106f / 255f, 229f / 255f, 191f / 255f);
            if (Plugin.AntennaeColor.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)Plugin.AntennaeColor.GetColor(player.graphicsModule as PlayerGraphics);
            return new Color(106f / 255f, 229f / 255f, 191f / 255f);
        }

        public Color GetLepidoticWingColor()
        {
            if (!playerRef.TryGetTarget(out Player player) || player.graphicsModule == null)
                return new Color(106f / 255f, 229f / 255f, 191f / 255f);
            if (Plugin.LepidoticWingColor.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)Plugin.LepidoticWingColor.GetColor(player.graphicsModule as PlayerGraphics);
            return new Color(106f / 255f, 229f / 255f, 191f / 255f);
        }

        public Color GetSpeckleColor()
        {
            if (!playerRef.TryGetTarget(out Player player) || player.graphicsModule == null)
                return new Color(106f / 255f, 191f / 255f, 229f / 255f);
            if (Plugin.SpeckleColor.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)Plugin.SpeckleColor.GetColor(player.graphicsModule as PlayerGraphics);
            return new Color(106f / 255f, 191f / 255f, 229f / 255f);
        }

        public Color GetFlareColor()
        {
            if (!playerRef.TryGetTarget(out Player player) || player.graphicsModule == null)
                return new Color(106f / 255f, 191f / 255f, 229f / 255f);
            if (Plugin.FlareColor.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)Plugin.FlareColor.GetColor(player.graphicsModule as PlayerGraphics);
            return new Color(106f / 255f, 191f / 255f, 229f / 255f);
        }

        #region 飞行相关
        public void StopFlight()
        {
            flightTime = -1;
            isFlying = false;
        }

        public void InitiateFlight(Player self, PlayerEx player)
        {
            if (self.input[0].y < 0)
            {
                return;
            }
            self.bodyMode = Player.BodyModeIndex.Default;
            self.animation = Player.AnimationIndex.None;
            self.wantToJump = 0;
            flightTime = 0;
            isFlying = true;
        }

        public bool CanSustainFlight(Player self, PlayerEx player)
        {
            return preventFlight == 0
                && self.canJump <= 0
                && self.Consious && !self.Stunned
                && self.bodyMode != Player.BodyModeIndex.Crawl
                && self.bodyMode != Player.BodyModeIndex.CorridorClimb
                && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut
                && self.bodyMode != Player.BodyModeIndex.WallClimb
                && self.bodyMode != Player.BodyModeIndex.Swimming
                && self.animation != Player.AnimationIndex.HangFromBeam
                && self.animation != Player.AnimationIndex.ClimbOnBeam
                && self.animation != Player.AnimationIndex.AntlerClimb
                && self.animation != Player.AnimationIndex.VineGrab
                && self.animation != Player.AnimationIndex.ZeroGPoleGrab
                && self.animation != Player.AnimationIndex.HangUnderVerticalBeam;
        }

        public int WingSprite(int side, int wing)
        {
            return wingSprite + side + wing + wing;
        }
        #endregion
        
        #region 翅膀相关
        public void MothWing(PlayerGraphics self)
        {
            wing = new GenericBodyPart[36];
            for (int i = 0; i < wing.Length; i++)
            {
                wing[i] = new GenericBodyPart(self, 1f, 0.5f, 0.9f, self.player.bodyChunks[0]);
            }

        }
        #endregion
        
        #region 尾巴相关
        public void MothSwallowTail(PlayerGraphics self)
        {
            bool isPup = self.player.playerState.isPup || self.player.isSlugpup;

            if (isPup)
            {
                tailN = 4;
            }
            else
            {
                tailN = 7;
            }

            swallowtail = new TailSegment[2, tailN];
            for (int i = 0; i < 2; i++)
            {
                if (isPup)
                {
                    swallowtail[i, 0] = new TailSegment(self, 5f, 4f * (isPup ? 0.8f : 1f), null, 0.85f, 1f, 3f, true);
                    swallowtail[i, 1] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 0], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 2] = new TailSegment(self, 7f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 1], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 3] = new TailSegment(self, 6f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 2], 0.55f, 1f, 0.5f, true);
                }
                else
                {
                    swallowtail[i, 0] = new TailSegment(self, 5f, 4f * (isPup ? 0.8f : 1f), null, 0.85f, 1f, 3f, true);
                    swallowtail[i, 1] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 0], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 2] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 1], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 3] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 2], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 4] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 3], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 5] = new TailSegment(self, 7f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 4], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 6] = new TailSegment(self, 6f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 5], 0.55f, 1f, 0.5f, true);
                }
            }
        }
        #endregion

        #region 触须相关
        public void MothAntennae(PlayerGraphics self)
        {
            antennae = new GenericBodyPart[2];
            for (int i = 0; i < antennae.Length; i++)
            {
                antennae[i] = new GenericBodyPart(self, 1f, 0.5f, 0.9f, self.player.bodyChunks[0]);
            }
        }
        #endregion

        #region 猫崽相关
        public static bool PlayerNPCShouldBeMoth(Player player)
        {
            if (player.abstractCreature != null && 
                player.abstractCreature.world != null && 
                player.abstractCreature.world.game != null && 
                player.abstractCreature.world.game.IsStorySession &&
                (player.abstractCreature.world.game.session.characterStats.name == Plugin.SlugName || player.abstractCreature.world.region.name == "OSAM"))
            {
                return true;
            }
            return false;
        }
        #endregion
        /*
        #region 梦境相关
        public class DreamStateOverride
        {
            public int availableDroneCount;
            public int overrideHealth;
            public bool initDronePortGraphics = false;
            public Vector2 dronePortGraphicsPos = Vector2.zero;
            public Vector2 currentPortPos = Vector2.zero;
            public float dronePortGraphicsRotation = 0f;

            public float connectToDMProggress = 1f;

            public DreamStateOverride(int availableDroneCount, bool initDronePortGraphics, Vector2 dronePortGraphicsPos, float dronePortGraphicsRotation, int overrideHealth)
            {
                this.availableDroneCount = availableDroneCount;
                this.initDronePortGraphics = initDronePortGraphics;
                this.dronePortGraphicsPos = dronePortGraphicsPos;
                this.dronePortGraphicsRotation = dronePortGraphicsRotation;
                this.overrideHealth = overrideHealth;

                Plugin.Log("Set up override {0},{1},{2},{3}", availableDroneCount, initDronePortGraphics, dronePortGraphicsPos, dronePortGraphicsRotation);
            }
        }
        #endregion
        */
    }
}