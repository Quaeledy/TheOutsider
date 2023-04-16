using SlugBase.Features;
using SlugBase;
using System.Linq;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using TheOutsider;
using SlugBase.DataTypes;
using BepInEx.Logging;
using System.Collections.Generic;

namespace TheOutsider
{
    public class PlayerEx
    {
        public readonly float WingSpeed;
        public readonly float UpFlytime;
        public readonly bool MothOutsider;
        public readonly bool IsMoth;

        public EntityID ID;

        public readonly SlugcatStats.Name Name;
        public SlugBaseCharacter Character;
        
        public bool CanFly => WingSpeed > 0;

        public int preventGrabs;
        public bool isFlying;
        public int currentFlightDuration;
        public int preventFlight;
        public int lastTail;
        public int quarterFoodPoints;

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
        public int initialWingSprite;
        public int antennaeSprite;
        public int FlareSprite;
        public int swallowtailSprite;
        public int stripeSprite;
        public Vector2 lastAntennaePos;

        public static WeakReference<Player> playerRef;
        public WeakReference<PlayerGraphics> iGraphicsRef;
        public static Player playerSelf;
        public float[] wingDeployment;
        public float[] wingDeploymentSpeed;
        public float wingDeploymentGetTo;
        public float wingOffset;
        public float wingTimeAdd;
        public Vector2 zRotation;
        public Vector2 lastZRotation;
        public float wingYAdjust;

        public TailSegment[] SwallowTail;
        public readonly int tailN = 7;
        public readonly float SwallowTailSpacing = 6f;
        public readonly float MaxLength = 9f;
        public readonly float SwallowTailWidth = 0.4f;
        public float tailTimeAdd;

        public DynamicSoundLoop flyingBuzzSound;

        public PlayerEx(Player player)
        {
            IsMoth =
                Plugin.WingSpeed.TryGet(player, out WingSpeed) &&
                Plugin.UpFlytime.TryGet(player, out UpFlytime) &&
                Plugin.MothOutsider.TryGet(player, out MothOutsider);

            
            if (!IsMoth)
            {
                return;
            }

            playerRef = new WeakReference<Player>(player);
            playerSelf = player;

            quarterFoodPoints = 4;

            ID = player.abstractCreature.ID;

            if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), "Outsider", true, out var extEnum))
            {
                Name = extEnum as SlugcatStats.Name;
            }

            flyingBuzzSound = new ChunkDynamicSoundLoop(player.bodyChunks[0]);
            flyingBuzzSound.sound = MothEnums.MothBuzz;
            flyingBuzzSound.Pitch = 1f;
            flyingBuzzSound.Volume = 1f;
        }

         #region 飞行相关
         public void StopFlight()
         {
             currentFlightDuration = 0;
             isFlying = false;
         }

         public void InitiateFlight()
         {
             if (!playerRef.TryGetTarget(out var player))
             {
                 return;
             }
             if(playerSelf.input[0].y < 0)
             {
                 return;
             }
             player.bodyMode = Player.BodyModeIndex.Default;
             player.animation = Player.AnimationIndex.None;
             player.wantToJump = 0;
             currentFlightDuration = 0;
             isFlying = true;
         }

         public bool CanSustainFlight(Player player)
         {
             return preventFlight == 0 && player.canJump <= 0 && player.bodyMode != Player.BodyModeIndex.Crawl && player.bodyMode != Player.BodyModeIndex.CorridorClimb && player.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && player.animation != Player.AnimationIndex.HangFromBeam && player.animation != Player.AnimationIndex.ClimbOnBeam && player.bodyMode != Player.BodyModeIndex.WallClimb && player.bodyMode != Player.BodyModeIndex.Swimming && player.Consious && !player.Stunned && player.animation != Player.AnimationIndex.AntlerClimb && player.animation != Player.AnimationIndex.VineGrab && player.animation != Player.AnimationIndex.ZeroGPoleGrab;
         }
        
         public int WingSprite(int side, int wing)
         {
             return initialWingSprite + side + wing + wing;
         }
         #endregion
         
        #region 尾巴相关
        public void MothSwallowTail(PlayerGraphics self)
        {
            bool isPup = self.player.playerState.isPup;

            SwallowTail = new TailSegment[2 * tailN];
            for (int i = 0; i < 2; i++)
            {
                SwallowTail[i * 7 + 0] = new TailSegment(self, 5f, 4f * (isPup ? 0.8f : 1f), null, 0.85f, 1f, 3f, true);
                SwallowTail[i * 7 + 1] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), SwallowTail[i * 7 + 0], 0.55f, 1f, 0.5f, true);
                SwallowTail[i * 7 + 2] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), SwallowTail[i * 7 + 1], 0.55f, 1f, 0.5f, true);
                SwallowTail[i * 7 + 3] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), SwallowTail[i * 7 + 2], 0.55f, 1f, 0.5f, true);
                SwallowTail[i * 7 + 4] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), SwallowTail[i * 7 + 3], 0.55f, 1f, 0.5f, true);
                SwallowTail[i * 7 + 5] = new TailSegment(self, 7f, 7f * (isPup ? 0.8f : 1f), SwallowTail[i * 7 + 4], 0.55f, 1f, 0.5f, true);
                SwallowTail[i * 7 + 6] = new TailSegment(self, 6f, 7f * (isPup ? 0.8f : 1f), SwallowTail[i * 7 + 5], 0.55f, 1f, 0.5f, true);
            }
        }
        #endregion
    }
}