using RWCustom;
using SlugBase.DataTypes;
using System;
using TheOutsider.CustomLore.CustomCreature;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
                    return player.SlugCatClass == Plugin.SlugName || player.slugcatStats.name == Plugin.MothPup || isMothNPC;
                else
                    return false;
            }
        }

        public static readonly Color BlueGreen = new Color(40f / 255f, 102f / 255f, 141f / 255f);
        public static readonly Color LightGreen = new Color(106f / 255f, 229f / 255f, 191f / 255f);
        public static readonly Color LightBlue = new Color(106f / 255f, 229f / 255f, 191f / 255f);

        #region 猫崽相关
        public bool isMothNPC;
        public bool wantsToRegurgitate;
        public bool regurgitating;
        public bool wantsToSwallowObject;
        public bool swallowing;
        public bool isColorVariation;
        #endregion
        #region 飞行相关
        public float flutterTimeAdd;
        public readonly float wingSpeed;
        public readonly float upFlightTime;
        public int preventGrabs;
        public bool isFlying;
        public int flightTime;
        public int preventFlight;
        //加速度
        public float ax;
        public float ay;
        //飞行能量
        public float flyEnergy;
        //姿势
        public bool spreadWings;
        public bool foldUpWings;
        #endregion
        #region 进食和毒性相关
        public bool shouldResetDisplayQuarterFood;
        //烟雾果毒性
        public bool deadForSporeCloud;
        public int deadForSporeCloudCount;
        #endregion
        #region 闪光相关
        //闪光相关
        public bool AIwantFlare;
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
        public float burningRange;
        public float burningRangeWithVisualContact;
        public float LightIntensity
        {
            get
            {
                return Mathf.Pow(Mathf.Sin(burning * 3.1415927f), 0.4f);
            }
        }
        #endregion
        #region 外观相关
        //图像
        public int wingSprite;
        public int antennaeSprite;
        public int FlareSprite;
        public int swallowtailSprite;
        public int speckleSprite;
        //手臂替代贴图
        public int handWingSprite;

        //翅膀长度及宽度
        public float wingLength;
        public float wingWidth;
        //触须长度
        public float antennaeLength;

        //身体部件
        public GenericBodyPart[] wing;
        public GenericBodyPart[] antennae;
        public TailSegment[,] swallowtail;

        public Vector2 lastAntennaePos;

        public int tailN;
        public readonly float swallowTailSpacing = 6f;
        public readonly float MaxLength = 15f;
        public readonly float swallowTailWidth = 0.4f;
        public float tailTimeAdd;
        #endregion
        //public DynamicSoundLoop flyingBuzzSound;

        //梦境相关
        //public DreamStateOverride stateOverride;

        public PlayerEx(Player player)
        {
            playerRef = new WeakReference<Player>(player);
            if (player.isNPC)
            {
                isMothNPC = true;
                player.glowing = true;
                Random.InitState(player.abstractCreature.ID.number);
                if (Random.value <= 0.15f) 
                    isColorVariation = true;
                else
                    isColorVariation = false;
            }

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

            burningRange = isMothNPC ? 300f: 600f;
            burningRangeWithVisualContact = isMothNPC ? 800f : 1600f;

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
            if (!playerRef.TryGetTarget(out Player player))
                return BlueGreen;
            if (player.isNPC)
                return player.ShortCutColor();
            if (player.graphicsModule == null)
                return BlueGreen;
            if (PlayerColor.Body.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)PlayerColor.Body.GetColor(player.graphicsModule as PlayerGraphics);
            return BlueGreen;
        }

        public Color GetEyesColor()
        {
            if (!playerRef.TryGetTarget(out Player player))
                return new Color(1f / 255f, 1f / 255f, 1f / 255f);
            if (player.graphicsModule == null)
            {
                if (player.isNPC && player.npcStats != null && player.npcStats.Dark && isColorVariation)
                    return new Color(255f / 255f, 255f / 255f, 255f / 255f);
                else
                    return new Color(1f / 255f, 1f / 255f, 1f / 255f);
            }
            if (PlayerColor.Eyes.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)PlayerColor.Eyes.GetColor(player.graphicsModule as PlayerGraphics);

            if (player.isNPC && player.npcStats != null && player.npcStats.Dark && isColorVariation)
                return new Color(255f / 255f, 255f / 255f, 255f / 255f);
            else
                return new Color(1f / 255f, 1f / 255f, 1f / 255f);
        }

        public Color GetAntennaeColor()
        {
            if (!playerRef.TryGetTarget(out Player player) || player.graphicsModule == null)
                return LightGreen;
            if (Plugin.AntennaeColor.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)Plugin.AntennaeColor.GetColor(player.graphicsModule as PlayerGraphics);
            return AntennaeColorLerp(GetBodyColor());
        }

        public Color GetLepidoticWingColor()
        {
            if (!playerRef.TryGetTarget(out Player player) || player.graphicsModule == null)
                return LightGreen;
            if (Plugin.LepidoticWingColor.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)Plugin.LepidoticWingColor.GetColor(player.graphicsModule as PlayerGraphics);
            return AntennaeColorLerp(GetBodyColor());
        }

        public Color GetSpeckleColor()
        {
            if (!playerRef.TryGetTarget(out Player player) || player.graphicsModule == null)
                return LightBlue;
            if (Plugin.SpeckleColor.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)Plugin.SpeckleColor.GetColor(player.graphicsModule as PlayerGraphics);
            return SpeckleColorLerp(GetBodyColor());
        }

        public Color GetFlareColor()
        {
            if (!playerRef.TryGetTarget(out Player player) || player.graphicsModule == null)
                return LightBlue;
            if (Plugin.FlareColor.GetColor(player.graphicsModule as PlayerGraphics) != null)
                return (Color)Plugin.FlareColor.GetColor(player.graphicsModule as PlayerGraphics);
            return SpeckleColorLerp(GetBodyColor());
        }

        public Color AntennaeColorLerp(Color bodyColor)
        {
            Vector3 defaultColor = Custom.RGB2HSL(LightGreen);
            Vector3 presetColor1 = Custom.RGB2HSL(new Color(135f / 255f, 231f / 255f, 234f / 255f));
            Vector3 presetColor2 = Custom.RGB2HSL(new Color(255f / 255f, 115f / 255f, 115f / 255f));
            Vector3 presetColor3 = Custom.RGB2HSL(new Color(46f / 255f, 20f / 255f, 79f / 255f));
            Vector3 presetColor4 = Custom.RGB2HSL(new Color(255f / 255f, 255f / 255f, 115f / 255f));

            Vector3 color = ColorLerp(bodyColor, defaultColor, presetColor1, presetColor2, presetColor3, presetColor4);
            if (playerRef.TryGetTarget(out Player player) && player.isNPC)
            {
                Random.InitState(player.abstractCreature.ID.number + 1);//不能用Random.InitState(player.abstractCreature.ID.number)，否则下一个随机数必<=0.15
                color.x = (color.x + (Random.value - 0.5f) * (this.isColorVariation ? 2f : 0.2f) + 1f) % 1f;
            }
            return Custom.HSL2RGB(color.x, color.y, color.z);
        }

        public Color SpeckleColorLerp(Color bodyColor)
        {
            Vector3 defaultColor = Custom.RGB2HSL(LightBlue);
            Vector3 presetColor1 = Custom.RGB2HSL(new Color(135f / 255f, 231f / 255f, 234f / 255f));
            Vector3 presetColor2 = Custom.RGB2HSL(new Color(241f / 255f, 255f / 255f, 146f / 255f));
            Vector3 presetColor3 = Custom.RGB2HSL(new Color(255f / 255f, 174f / 255f, 175f / 255f));
            Vector3 presetColor4 = Custom.RGB2HSL(new Color(191f / 255f, 160f / 255f, 255f / 255f));

            Vector3 color = ColorLerp(bodyColor, defaultColor, presetColor1, presetColor2, presetColor3, presetColor4);
            if (playerRef.TryGetTarget(out Player player) && player.isNPC)
            {
                Random.InitState(player.abstractCreature.ID.number + 1);
                color.x = (color.x + (Random.value - 0.5f) * (this.isColorVariation ? 2f : 0.2f) + 1f) % 1f;
            }
            return Custom.HSL2RGB(color.x, color.y, color.z);
        }

        public Vector3 ColorLerp(Color bodyColor, Vector3 defaultColor, Vector3 presetColor1, Vector3 presetColor2, Vector3 presetColor3, Vector3 presetColor4)
        {
            Vector3 bodyColorHSL = Custom.RGB2HSL(bodyColor);
            Vector3 defaultBodyColor = Custom.RGB2HSL(BlueGreen);
            Vector3 presetBodyColor1 = Custom.RGB2HSL(new Color(240f / 255f, 240f / 255f, 255f / 255f));
            Vector3 presetBodyColor2 = Custom.RGB2HSL(new Color(255f / 255f, 255f / 255f, 115f / 255f));
            Vector3 presetBodyColor3 = Custom.RGB2HSL(new Color(255f / 255f, 115f / 255f, 115f / 255f));
            Vector3 presetBodyColor4 = Custom.RGB2HSL(new Color(46f / 255f, 20f / 255f, 79f / 255f));
            if (bodyColorHSL == defaultBodyColor)
                return defaultColor;
            if (bodyColorHSL == presetBodyColor1)
                return presetColor1;
            if (bodyColorHSL == presetBodyColor2)
                return presetColor2;
            if (bodyColorHSL == presetBodyColor3)
                return presetColor3;
            if (bodyColorHSL == presetBodyColor4)
                return presetColor4;
            float dist0 = Vector3.Distance(bodyColorHSL, defaultBodyColor);
            float dist1 = Mathf.Abs(bodyColorHSL.z - presetBodyColor1.z);
            float dist2 = Vector3.Distance(bodyColorHSL, presetBodyColor2);
            float dist3 = Vector3.Distance(bodyColorHSL, presetBodyColor3);
            float dist4 = Mathf.Abs(bodyColorHSL.z - presetBodyColor4.z);
            float distMuti = dist0 * dist1 * dist2 * dist3 * dist4;
            float distReciprocal0 = distMuti / Mathf.Pow(dist0, 2f);
            float distReciprocal1 = distMuti / Mathf.Pow(dist1, 2f);
            float distReciprocal2 = distMuti / Mathf.Pow(dist2, 2f);
            float distReciprocal3 = distMuti / Mathf.Pow(dist3, 2f);
            float distReciprocal4 = distMuti / Mathf.Pow(dist4, 2f);
            float distReciprocalAdd = distReciprocal0 + distReciprocal1 + distReciprocal2 + distReciprocal3 + distReciprocal4;

            Vector3 color = (distReciprocal0 * defaultColor +
                             distReciprocal1 * presetColor1 +
                             distReciprocal2 * presetColor2 +
                             distReciprocal3 * presetColor3 +
                             distReciprocal4 * presetColor4) / distReciprocalAdd;
            return color;
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
            if (player.abstractCreature.creatureTemplate.type == MothPupCritob.MothPup)
                return true;
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
        public static bool PlayerNPCShouldBeMoth(World world)
        {
            if (world != null && world.game != null && world.game.IsStorySession &&
               (world.game.session.characterStats.name == Plugin.SlugName || world.region.name == "OSAM"))
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