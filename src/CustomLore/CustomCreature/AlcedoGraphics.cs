using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using MoreSlugcats;

namespace TheOutsider.CustomLore.CustomCreature
{
    sealed class AlcedoGraphics : GraphicsModule
    {
        private class WingColorWave
        {
            public int delay;

            public float speed;

            public float lightness;

            public float saturation;

            public float forceAlpha;

            public float position;

            public float lastPosition;

            public WingColorWave(int delay, float speed, float lightness, float saturation, float forceAlpha)
            {
                this.delay = delay;
                this.speed = speed;
                this.lightness = lightness;
                this.saturation = saturation;
                this.forceAlpha = forceAlpha;
                position = 0f;
                lastPosition = 0f;
            }
        }

        public AlcedoFeather[,,] wings;
        public TailSegment[] tail;
        public int tailSegments;
        public float tailLength;
        public float tailStiffness;
        public float tailStiffnessDecline; 
        public float showDominance;
        private Vector2 tailDirection;
        public int featherLayersPerWing;
        public int feathersPerLayer;
        public GenericBodyPart[] tusks;
        public float[] tuskRotations;
        public int headGraphic;
        public int changeHeadGraphicCounter;
        public Vector2[,] shells;
        public int[] shellModeChangeCounter;
        public float[,] shellModes;
        public HSLColor ColorA;
        public HSLColor ColorB;
        public bool shadowMode;
        public bool spritesInShadowMode;
        public RoomPalette palette;
        public Vector2[,,] neckTubes;
        public bool albino;
        public float darkness;
        private List<WingColorWave> colorWaves;
        private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;
        private Vector2[] _cachedTusk1;
        private Vector2[] _cachedTusk2;
        private Vector2[] _cachedTuskConPos;
        public float headFlip;
        public float lastHeadFlip;
        public float eyeSize;
        public Color eyeCol;
        public float beakFatness;
        public Color laserColor;
        public Color lastLaserColor;
        public ChunkDynamicSoundLoop soundLoop;
        private float laserActive;
        private float lastLaserActive;
        public float flash;
        public float lastFlash;

        public Alcedo alcedo => base.owner as Alcedo;

        public bool IsKing => alcedo.IsKing;
        public bool IsMiros => alcedo.IsMiros;
        public override bool ShouldBeCulled
        {
            get
            {
                if (base.ShouldBeCulled)
                {
                    if (base.owner.room != null)
                    {
                        return base.owner.firstChunk.pos.y < base.owner.room.PixelHeight;
                    }
                    return true;
                }
                return false;
            }
        }

        #region 贴图序号起始值及长度
        private int TailSpriteStart => 0;
        private int TailSpriteLength => 1;

        private int LegSpriteStart => TailSpriteStart + TailSpriteLength;
        private int LegSpriteLength => alcedo.legs.Length;

        private int HindPawSpriteStart => LegSpriteStart + LegSpriteLength;
        private int HindPawSpriteLength => alcedo.legs.Length * 2;

        private int TentacleSpriteStart => HindPawSpriteStart + HindPawSpriteLength; 
        private int TentacleSpriteLength => alcedo.tentacles.Length;

        private int FeatherSpriteStart => TentacleSpriteStart + TentacleSpriteLength;
        private int FeatherSpritesLength => featherLayersPerWing * feathersPerLayer * 2 * alcedo.tentacles.Length;

        private int ForePawSpriteStart => FeatherSpriteStart + FeatherSpritesLength;
        private int ForePawSpriteLength => alcedo.tentacles.Length * 2;

        private int BodySpriteStart => ForePawSpriteStart + ForePawSpriteLength;
        private int BodySpriteLength => 3;

        private int NeckSprite => BodySpriteStart + BodySpriteLength;

        private int HeadSprite => NeckSprite + 1;

        private int EyesSprite => HeadSprite + 3;

        private int MaskSprite => EyesSprite + 1;

        private int TotalSprites => MaskSprite + 1;
        #endregion

        #region 贴图序号
        private int LegSprite(int i)
        {
            return LegSpriteStart + i;
        }

        private int HindPawSprite(int i)
        {
            return HindPawSpriteStart + i * 2;
        }
        private int HindPawColorSprite(int i)
        {
            return HindPawSpriteStart + i * 2 + 1;
        }

        private int TentacleSprite(int i)
        {
            return TentacleSpriteStart + i;
        }

        private int FeatherSprite(int w, int l, int i)
        {
            return FeatherSpriteStart + featherLayersPerWing * feathersPerLayer * w * 2 + feathersPerLayer * l * 2 + i * 2;
        }
        private int FeatherColorSprite(int w, int l, int i)
        {
            return FeatherSpriteStart + featherLayersPerWing * feathersPerLayer * w * 2 + feathersPerLayer * l * 2 + i * 2 + 1;
        }

        private int ForePawSprite(int i)
        {
            return ForePawSpriteStart + i * 2;
        }
        private int ForePawColorSprite(int i)
        {
            return ForePawSpriteStart + i * 2 + 1;
        }

        private  int BodySprite(int i)
        {
            return BodySpriteStart + i;
        }

        private int TuskSprite(int i)
        {
            return HeadSprite + 1 + i;
        }
        #endregion

        public void MakeColorWave(int delay)
        {
            colorWaves.Add(new WingColorWave(delay, 1f / Mathf.Lerp(10f, 30f, UnityEngine.Random.value), 1f - UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value, 1f - UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value, 1f - UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value));
        }

        public AlcedoGraphics(Alcedo ow)
            : base(ow, internalContainers: false)
        {
            _cachedTusk1 = new Vector2[4];
            _cachedTusk2 = new Vector2[4];
            _cachedTuskConPos = new Vector2[4];
            cullRange = 1400f;
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(alcedo.abstractCreature.ID.RandomSeed);
            ColorA = new HSLColor(Mathf.Lerp(120f / 360f, 170f / 360f, UnityEngine.Random.value), Mathf.Lerp(0.5f, 0.7f, UnityEngine.Random.value), Mathf.Lerp(0.7f, 0.8f, UnityEngine.Random.value));
            ColorB = new HSLColor(ColorA.hue + Mathf.Lerp(-0.25f, 0.25f, UnityEngine.Random.value), Mathf.Lerp(0.8f, 1f, 1f - UnityEngine.Random.value * UnityEngine.Random.value), Mathf.Lerp(0.45f, 1f, UnityEngine.Random.value * UnityEngine.Random.value));
            /*
            if (IsMiros)
            {
                ColorA = new HSLColor(Custom.WrappedRandomVariation(0.0025f, 0.02f, 0.6f), 1f, Custom.ClampedRandomVariation(0.5f, 0.15f, 0.1f));
                ColorB = new HSLColor(ColorA.hue + Mathf.Lerp(-0.25f, 0.25f, UnityEngine.Random.value), Mathf.Lerp(0.8f, 1f, 1f - UnityEngine.Random.value * UnityEngine.Random.value), Mathf.Lerp(0.45f, 1f, UnityEngine.Random.value * UnityEngine.Random.value));
            }
            else if (IsKing)
            {
                ColorB = new HSLColor(Mathf.Lerp(0.93f, 1.07f, UnityEngine.Random.value), Mathf.Lerp(0.8f, 1f, 1f - UnityEngine.Random.value * UnityEngine.Random.value), Mathf.Lerp(0.45f, 1f, UnityEngine.Random.value * UnityEngine.Random.value));
                ColorA = new HSLColor(ColorB.hue + Mathf.Lerp(-0.25f, 0.25f, UnityEngine.Random.value), Mathf.Lerp(0.5f, 0.7f, UnityEngine.Random.value), Mathf.Lerp(0.7f, 0.8f, UnityEngine.Random.value));
            }
            else
            {
                ColorA = new HSLColor(Mathf.Lerp(0.9f, 1.6f, UnityEngine.Random.value), Mathf.Lerp(0.5f, 0.7f, UnityEngine.Random.value), Mathf.Lerp(0.7f, 0.8f, UnityEngine.Random.value));
                ColorB = new HSLColor(ColorA.hue + Mathf.Lerp(-0.25f, 0.25f, UnityEngine.Random.value), Mathf.Lerp(0.8f, 1f, 1f - UnityEngine.Random.value * UnityEngine.Random.value), Mathf.Lerp(0.45f, 1f, UnityEngine.Random.value * UnityEngine.Random.value));
            }*/
            featherLayersPerWing = 3;
            feathersPerLayer = UnityEngine.Random.Range(13, 20);//UnityEngine.Random.Range(IsKing ? 15 : 13, IsKing ? 25 : 20);
            colorWaves = new List<WingColorWave>();
            float num = ((UnityEngine.Random.value < 0.5f) ? 40f : Mathf.Lerp(8f, 15f, UnityEngine.Random.value));
            float num2 = ((UnityEngine.Random.value < 0.5f) ? 40f : Mathf.Lerp(8f, 15f, UnityEngine.Random.value));
            float num3 = ((UnityEngine.Random.value < 0.5f) ? 20f : Mathf.Lerp(3f, 6f, UnityEngine.Random.value));
            wings = new AlcedoFeather[alcedo.tentacles.Length, featherLayersPerWing, feathersPerLayer];//翅膀数，每只翅膀羽毛层数，每层羽毛的羽毛数
            for (int j = 0; j < alcedo.tentacles.Length; j++)
            {
                for (int k = 0; k < feathersPerLayer; k++)
                {
                    float num5 = ((float)k + 0.5f) / (float)feathersPerLayer;
                    float value = Mathf.Lerp(1f - Mathf.Pow(IsMiros ? 0.95f : 0.89f, k), Mathf.Sqrt(num5), 0.5f);
                    value = num5;
                    value = Mathf.InverseLerp(0.1f, 1.1f, value);
                    if (IsMiros && k == feathersPerLayer - 1)
                    {
                        value = 0.8f;
                    }
                    //飞羽
                    wings[j, 0, k] = new AlcedoFeather(this, alcedo.tentacles[j], value, 
                        AlcedoTentacle.FeatherContour(num5, 0f) * 1.5f * Mathf.Lerp(45f, 50f, UnityEngine.Random.value), 
                        AlcedoTentacle.FeatherContour(num5, 1f) * 1.5f * Mathf.Lerp(50f, 55f, UnityEngine.Random.value) * (IsKing ? 1.3f : 1f), 
                        Mathf.Lerp(5f, 8f, AlcedoTentacle.FeatherWidth(num5)), "FlightFeather");
                    //覆羽

                    wings[j, 1, k] = new AlcedoFeather(this, alcedo.tentacles[j], value + 0.05f * UnityEngine.Random.value,
                        AlcedoTentacle.FeatherContour(num5, 0f) * 1.5f * Mathf.Lerp(20f, 25f, UnityEngine.Random.value),
                        AlcedoTentacle.FeatherContour(num5, 1f) * 1.5f * Mathf.Lerp(25f, 30f, UnityEngine.Random.value) * (IsKing ? 1.3f : 1f),
                        Mathf.Lerp(5f, 8f, AlcedoTentacle.FeatherWidth(num5)), "Covert");
                    wings[j, 2, k] = new AlcedoFeather(this, alcedo.tentacles[j], value + 0.05f * UnityEngine.Random.value,
                        AlcedoTentacle.FeatherContour(num5, 0f) * 1.5f * Mathf.Lerp(10f, 12f, UnityEngine.Random.value),
                        AlcedoTentacle.FeatherContour(num5, 1f) * 1.5f * Mathf.Lerp(12f, 15f, UnityEngine.Random.value) * (IsKing ? 1.3f : 1f),
                        Mathf.Lerp(5f, 8f, AlcedoTentacle.FeatherWidth(num5)), "Covert");
                    /*
                    bool flag = UnityEngine.Random.value < 0.025f;
                    if (UnityEngine.Random.value < 1f / num || (flag && UnityEngine.Random.value < 0.5f))
                    {
                        wings[j, k].lose = 1f - UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value;
                        if (UnityEngine.Random.value < 0.4f)
                        {
                            wings[j, k].brokenColor = 1f - UnityEngine.Random.value * UnityEngine.Random.value;
                        }
                    }
                    if (UnityEngine.Random.value < 1f / num2)
                    {
                        wings[j, k].extendedLength /= 5f;
                        wings[j, k].contractedLength = wings[j, k].extendedLength;
                        wings[j, k].brokenColor = 1f;
                        wings[j, k].width /= 1.7f;
                    }
                    if (UnityEngine.Random.value < 0.025f || (flag && UnityEngine.Random.value < 0.5f))
                    {
                        wings[j, k].contractedLength = wings[j, k].extendedLength * 0.7f;
                    }
                    if (UnityEngine.Random.value < 1f / num3 || (flag && UnityEngine.Random.value < 0.5f))
                    {
                        wings[j, k].brokenColor = ((UnityEngine.Random.value < 0.5f) ? 1f : UnityEngine.Random.value);
                    }*/
                }
            }
            for (int l = 0; l < 2; l++)
            {
                int num6 = UnityEngine.Random.Range(4, 5);
                //int num6 = (IsKing ? (14 - UnityEngine.Random.Range(0, UnityEngine.Random.Range(2, 12))) : UnityEngine.Random.Range(2, 12));
                float num7 = 3f;
                bool flag2 = false;
                bool flag3 = false;
                for (int m = 0; m < num6; m++)
                {
                    float num8 = Mathf.Sqrt(1f - Mathf.Pow(((float)m + 0.5f) / (float)num6, 3f));
                    if (m == num6 - 1)
                    {
                        num7 = 2f;
                    }
                    //appendages[l][m] = new AlcedoAppendage(this, l, m, num7 * num8, (m == 0) ? 15f : Mathf.Lerp(3f, 5f, UnityEngine.Random.value));
                    if (!flag2 && UnityEngine.Random.value < 0.5f)
                    {
                        flag2 = true;
                        num7 = (flag3 ? 4f : 6f);
                        flag3 = !flag3;
                    }
                    else
                    {
                        flag2 = false;
                    }
                    num7 += Mathf.Lerp(-1f, 1f, Mathf.Lerp(UnityEngine.Random.value, 0.5f, 0.5f)) * 2f;
                    num7 = Mathf.Clamp(num7, 3f, 7f);
                }
            }/*
            if (alcedo.kingTusks != null)
            {
                alcedo.kingTusks.patternDisplace = UnityEngine.Random.value;
            }*/
            if (ModManager.MSC)
            {
                albino = UnityEngine.Random.value < 0.001f;
            }
            if (alcedo.abstractCreature.superSizeMe)
            {
                albino = true;
            }
            if (IsMiros)
            {
                albino = false;
            }
            UnityEngine.Random.state = state;
            if (!IsMiros)
            {
                tusks = new GenericBodyPart[2];
                tuskRotations = new float[2];
                for (int n = 0; n < 2; n++)
                {
                    tusks[n] = new GenericBodyPart(this, 2f, 0.5f, 0.95f, alcedo.bodyChunks[4]);
                    tuskRotations[n] = 0f;
                }
            }
            shells = new Vector2[4, 3];
            for (int num9 = 0; num9 < 4; num9++)
            {
                shells[num9, 0] = alcedo.mainBodyChunk.pos;
                shells[num9, 1] = new Vector2(0f, 0f);
                shells[num9, 2] = alcedo.mainBodyChunk.pos;
            }
            shellModeChangeCounter = new int[2];
            shellModes = new float[2, 2];/*
            bodyParts = new BodyPart[appendages[0].Length + appendages[1].Length];
            for (int num10 = 0; num10 < appendages[0].Length; num10++)
            {
                bodyParts[num10] = appendages[0][num10];
            }
            for (int num11 = 0; num11 < appendages[1].Length; num11++)
            {
                bodyParts[appendages[0].Length + num11] = appendages[1][num11];
            }*/
            if (IsKing)
            {
                neckTubes = new Vector2[2, 15, 3];
            }
            //尾巴 TODO
            this.tailSegments = 16;//16
            this.tailLength = 8f;//160f
            this.tailStiffness = 200f;
            this.tailStiffnessDecline = 0.2f; 
            this.tailDirection = Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value;
            this.tail = new TailSegment[this.tailSegments];
            for (int j = 0; j < this.tailSegments; j++)
            {
                float t = (float)(this.tailSegments - j) / (float)this.tailSegments;
                this.tail[j] = new TailSegment(this, 
                    8f * t,
                    (((j > 0) ? 1f : 2f) + 2f * t) / 2f * this.tailLength,//(((j > 0) ? 8f : 16f) + 8f * t) / 2f * this.tailLength,
                    (j > 0) ? this.tail[j - 1] : null, 
                    0.85f, 1f, 0.4f, false);
                //bodyParts.Add(this.tail[j]);
            }
        }

        public override void Reset()
        {
            if (!IsMiros)
            {
                for (int i = 0; i < 2; i++)
                {
                    tusks[i].Reset(alcedo.bodyChunks[4].pos);
                }
            }
            for (int j = 0; j < 4; j++)
            {
                shells[j, 0] = alcedo.mainBodyChunk.pos;
                shells[j, 1] = new Vector2(0f, 0f);
                shells[j, 2] = alcedo.mainBodyChunk.pos;
            }
            for (int k = 0; k < alcedo.tentacles.Length; k++)
                for (int l1 = 0; l1 < featherLayersPerWing; l1++)
                    for (int l2 = 0; l2 < feathersPerLayer; l2++)
                    {
                        wings[k, l1, l2].pos = alcedo.tentacles[k].connectedChunk.pos;
                        wings[k, l1, l2].lastPos = alcedo.tentacles[k].connectedChunk.pos;
                        wings[k, l1, l2].vel = alcedo.tentacles[k].connectedChunk.vel;
                    }
            for (int j = 0; j < this.tail.Length; j++)
            {
                this.tail[j].Reset(base.owner.bodyChunks[1].pos);
            }
            if (neckTubes != null)
            {
                for (int num = 0; num < neckTubes.GetLength(0); num++)
                {
                    for (int num2 = 0; num2 < neckTubes.GetLength(1); num2++)
                    {
                        neckTubes[num, num2, 0] = alcedo.mainBodyChunk.pos + Custom.RNV() * UnityEngine.Random.value;
                        neckTubes[num, num2, 1] = neckTubes[num, num2, 0];
                        neckTubes[num, num2, 2] *= 0f;
                    }
                }
            }
            base.Reset();
        }

        public override void Update()
        {
            base.Update();
            #region 尾巴
            /*
            if (this.lizard.animation == Lizard.Animation.Lounge)
            {
                this.soundLoop.sound = SoundID.Lizard_Lounge_Attack_LOOP;
                this.soundLoop.Pitch = 1f;
                this.soundLoop.Volume = 1f;
            }
            else
            {
                this.soundLoop.sound = SoundID.Lizard_Belly_Drag_LOOP;
                this.soundLoop.Pitch = 1f;
                this.soundLoop.Volume = 1f;
                float num = 0f;
                float num2 = 0f;
                float num3 = 0f;
                for (int i = 0; i < 3; i++)
                {
                    if (this.lizard.bodyChunks[i].ContactPoint.x != 0 || this.lizard.room.GetTile(this.lizard.bodyChunks[i].pos - new Vector2(this.BodyChunkDisplayRad(i) + 6f, 0f)).Solid || this.lizard.room.GetTile(this.lizard.bodyChunks[i].pos + new Vector2(this.BodyChunkDisplayRad(i) + 6f, 0f)).Solid)
                    {
                        num += ((this.lizard.bodyChunks[i].ContactPoint.x != 0) ? 1f : 0.5f) / 3f;
                        num2 += Mathf.Abs(this.lizard.bodyChunks[i].vel.y) / 3f;
                    }
                    if (this.lizard.bodyChunks[i].ContactPoint.y != 0 || this.lizard.room.GetTile(this.lizard.bodyChunks[i].pos - new Vector2(0f, this.BodyChunkDisplayRad(i) + 6f)).Solid || this.lizard.room.GetTile(this.lizard.bodyChunks[i].pos + new Vector2(0f, this.BodyChunkDisplayRad(i) + 6f)).Solid)
                    {
                        num += ((this.lizard.bodyChunks[i].ContactPoint.y != 0) ? 1f : 0.5f) / 3f;
                        num3 += Mathf.Abs(this.lizard.bodyChunks[i].vel.x) / 3f;
                    }
                }
                int num4 = 0;
                while (num4 < this.tail.Length && num <= 1f)
                {
                    if (this.tail[num4].terrainContact)
                    {
                        num += 0.5f / (float)this.tail.Length;
                    }
                    num4++;
                }
                this.bellyDragVolume = Mathf.Lerp(this.bellyDragVolume, Mathf.Clamp(num, 0f, 1f), 0.2f);
                this.soundLoop.Pitch = Mathf.Lerp(0.3f, 2.2f, Mathf.Lerp(Mathf.InverseLerp(0.2f, 8f, Mathf.Max(num2, num3)), 0.5f, 0.3f));
                this.soundLoop.Volume = this.bellyDragVolume * Mathf.InverseLerp(0.2f, 2f, Mathf.Max(num2, num3));
            }*/
            if (this.alcedo.Consious && UnityEngine.Random.value < 0.05f)
            {
                this.tailDirection = Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value;
            }
            this.showDominance = Mathf.Clamp(this.showDominance - 1f / Mathf.Lerp(60f, 120f, UnityEngine.Random.value), 0f, 1f);
            this.tail[0].connectedPoint = new Vector2?(base.owner.bodyChunks[5].pos);
            float stiffness = Mathf.Lerp(this.tailStiffness, 900f, this.showDominance);
            if (this.alcedo.Consious && this.alcedo.swim > 0.5f)// && !this.alcedo.salamanderLurk
            {
                for (int num12 = 0; num12 < this.tail.Length; num12++)
                {
                    this.UpdateTailSegment(num12, stiffness);
                }/*
                if ((this.alcedo.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && this.alcedo.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)) && !this.alcedo.salamanderLurk)
                {
                    Vector2 a2 = Custom.PerpendicularVector(Custom.DirVec(this.alcedo.bodyChunks[2].pos, this.alcedo.bodyChunks[0].pos));
                    this.drawPositions[0, 0] += a2 * Mathf.Sin(this.breath * 3.1415927f * 2f * 3f) * 3.5f * this.alcedo.swim;
                    base.owner.bodyChunks[1].pos += a2 * Mathf.Sin((this.breath + 0.4f) * 3.1415927f * 2f * 3f) * 3f * this.alcedo.swim;
                    base.owner.bodyChunks[5].pos += a2 * Mathf.Sin((this.breath + 0.8f) * 3.1415927f * 2f * 3f) * 2f * this.alcedo.swim;
                }*/
            }
            else
            {
                for (int num13 = this.tail.Length - 1; num13 >= 0; num13--)
                {
                    this.UpdateTailSegment(num13, stiffness);
                }
            }
            #endregion
            if (IsMiros)
            {
                lastHeadFlip = headFlip;
                if (Custom.DistanceToLine(alcedo.Head().pos, alcedo.bodyChunks[1].pos, alcedo.bodyChunks[0].pos) < 0f)
                {
                    headFlip = Mathf.Min(1f, headFlip + 1f / 6f);
                }
                else
                {
                    headFlip = Mathf.Max(-1f, headFlip - 1f / 6f);
                }
                if (soundLoop == null && laserActive > 0f)
                {
                    soundLoop = new ChunkDynamicSoundLoop(alcedo.bodyChunks[4]);
                    soundLoop.sound = SoundID.Vulture_Grub_Laser_LOOP;
                }
                else if (soundLoop != null)
                {
                    soundLoop.Volume = Mathf.InverseLerp(0.3f, 1f, laserActive);
                    soundLoop.Pitch = 0.2f + 0.8f * Mathf.Pow(laserActive, 0.6f);
                    soundLoop.Update();
                    if (laserActive == 0f)
                    {
                        if (soundLoop.emitter != null)
                        {
                            soundLoop.emitter.slatedForDeletetion = true;
                        }
                        soundLoop = null;
                    }
                }
                lastLaserActive = laserActive;
                laserActive = Custom.LerpAndTick(laserActive, (!alcedo.isLaserActive()) ? 0f : 1f, 0.05f, 0.05f);
                lastLaserColor = laserColor;
                lastFlash = flash;
                flash = Custom.LerpAndTick(flash, 0f, 0.02f, 0.025f);
            }
            if (DEBUGLABELS != null)
            {
                DEBUGLABELS[0].label.text = alcedo.abstractCreature.pos.x + " " + alcedo.abstractCreature.pos.y + "   " + alcedo.AI.behavior.ToString();
                DEBUGLABELS[0].label.x = 10f;
                DEBUGLABELS[0].label.y = 10f;
            }
            for (int i = 0; i < alcedo.tentacles.Length; i++)
            {
                for (int l = 0; l < featherLayersPerWing; l++)
                    for (int j = 0; j < feathersPerLayer; j++)
                    {
                        wings[i, l, j].Update();
                    }
            }
            if (UnityEngine.Random.value < 0.005f)
            {
                MakeColorWave(0);
            }
            for (int num = colorWaves.Count - 1; num >= 0; num--)
            {
                if (colorWaves[num].lastPosition > 1f)
                {
                    colorWaves.RemoveAt(num);
                }
                else if (colorWaves[num].delay > 0)
                {
                    colorWaves[num].delay--;
                }
                else
                {
                    colorWaves[num].lastPosition = colorWaves[num].position;
                    colorWaves[num].position += colorWaves[num].speed / (1f + colorWaves[num].position);
                    for (int n = 0; n < alcedo.tentacles.Length; n++)
                    {
                        for (int l = 0; l < featherLayersPerWing; l++)
                            for (int num2 = 0; num2 < feathersPerLayer; num2++)
                            {
                                if (colorWaves[num].lastPosition < wings[n, l, num2].wingPosition && colorWaves[num].position >= wings[n, l, num2].wingPosition)
                                {
                                    wings[n, l, num2].saturationBonus = Mathf.Max(wings[n, l, num2].saturationBonus, colorWaves[num].saturation);
                                    wings[n, l, num2].lightnessBonus = Mathf.Max(wings[n, l, num2].lightnessBonus, colorWaves[num].lightness);
                                    wings[n, l, num2].forcedAlpha = Mathf.Max(wings[n, l, num2].forcedAlpha, colorWaves[num].forceAlpha);
                                    break;
                                }
                            }
                    }
                }
            }
            float num3 = Custom.AimFromOneVectorToAnother(alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].pos, alcedo.bodyChunks[4].pos);
            float num4 = Custom.AimFromOneVectorToAnother(alcedo.bodyChunks[0].pos, alcedo.bodyChunks[1].pos);
            float num5 = num3 - num4;
            if (num5 > 180f)
            {
                num5 -= 360f;
            }
            else if (num5 < -180f)
            {
                num5 += 360f;
            }
            int num6 = Custom.IntClamp(8 - (int)(Mathf.Abs(num5 / 180f) * 9f), 0, 8);
            if (num6 != headGraphic)
            {
                changeHeadGraphicCounter++;
                if (changeHeadGraphicCounter > 4)
                {
                    changeHeadGraphicCounter = 0;
                    headGraphic += ((num6 >= headGraphic) ? 1 : (-1));
                }
            }
            else
            {
                changeHeadGraphicCounter = 0;
            }
            if (!IsMiros)
            {
                if (alcedo.TusksStuck > 0.5f)
                {
                    tuskRotations[0] = Mathf.Lerp(tuskRotations[0], 0f, 0.05f);
                    tuskRotations[1] = Mathf.Lerp(tuskRotations[1], 0f, 0.05f);
                }
                else if (Mathf.Abs(num3) < 30f || Mathf.Abs(num3) > 150f)
                {
                    tuskRotations[0] = Mathf.Lerp(tuskRotations[0], 1f, 0.1f);
                    tuskRotations[1] = Mathf.Lerp(tuskRotations[1], -1f, 0.1f);
                }
                else if (Mathf.Abs(num3) > 60f && Mathf.Abs(num3) < 120f)
                {
                    tuskRotations[0] = Mathf.Lerp(tuskRotations[0], 2f * Mathf.Sign(num3), 0.1f);
                    tuskRotations[1] = Mathf.Lerp(tuskRotations[1], 2f * Mathf.Sign(num3), 0.1f);
                }
                else
                {
                    tuskRotations[(!(num3 < 0f)) ? 1u : 0u] = Mathf.Lerp(tuskRotations[(!(num3 < 0f)) ? 1u : 0u], 1f * Mathf.Sign(num3), 0.1f);
                    tuskRotations[(num3 < 0f) ? 1u : 0u] = Mathf.Lerp(tuskRotations[(num3 < 0f) ? 1u : 0u], 2f * Mathf.Sign(num3), 0.1f);
                }
                for (int num7 = 0; num7 < 2; num7++)
                {
                    tusks[num7].Update();
                    tusks[num7].PushOutOfTerrain(alcedo.room, alcedo.bodyChunks[4].pos);
                    tusks[num7].vel.y -= 0.6f;
                    TuskConnectionPositions(num7, 1f, ref _cachedTusk1);
                    tusks[num7].vel += _cachedTusk1[1] * 0.7f * (-1f + 2f * Mathf.Pow(Mathf.InverseLerp(0f, 180f, Mathf.Abs(num5)), 0.5f)) + _cachedTusk1[2] * 0.3f;
                    tusks[num7].ConnectToPoint(_cachedTusk1[0], 20f, push: true, 0f, alcedo.bodyChunks[4].vel, 0f, 0f);
                    tusks[num7].vel = Vector2.Lerp(tusks[num7].vel, _cachedTusk1[3] - tusks[num7].pos, alcedo.TusksStuck);
                }
            }
            Vector2 vector = Custom.DirVec(alcedo.bodyChunks[0].pos, alcedo.bodyChunks[1].pos);
            Vector2 vector2 = Custom.PerpendicularVector(vector);
            for (int num8 = 0; num8 < 4; num8++)
            {
                float num9 = ((num8 <= 1) ? 1f : (-1f));
                int num10 = ((num8 > 1) ? 1 : 0);
                int num11 = ((num8 % 2 != 0) ? 1 : 0);
                shells[num8, 2] = shells[num8, 0];
                shells[num8, 0] += shells[num8, 1];
                shells[num8, 1] *= 0.9f;
                shells[num8, 1].y -= 0.5f;
                Vector2 vector3 = alcedo.bodyChunks[1].pos + vector2 * (IsKing ? 18f : 13f) * num9;
                if (num11 == 1)
                {
                    vector3 += vector * Mathf.Lerp(14f, 22f, shellModes[num10, 0]);
                    vector3 += 14f * vector2 * num9 * shellModes[num10, 0];
                    vector3 += Custom.DirVec(vector3, alcedo.tentacles[num10].tChunks[3].pos) * 2.5f * (1f - shellModes[num10, 0]);
                }
                else
                {
                    vector3 += vector * Mathf.Lerp(24f, 28f, shellModes[num10, 0]);
                    vector3 += 16f * vector2 * num9 * shellModes[num10, 0];
                    vector3 += Custom.DirVec(vector3, alcedo.tentacles[num10].tChunks[3].pos) * 3.5f * shellModes[num10, 0];
                }
                shells[num8, 1] += vector2 * num9 * 0.5f;
                if (!Custom.DistLess(shells[num8, 0], vector3, Mathf.Lerp(2f, 4f, shellModes[num10, 0])))
                {
                    Vector2 vector4 = Custom.DirVec(shells[num8, 0], vector3);
                    shells[num8, 1] += vector4 * (Vector2.Distance(shells[num8, 0], vector3) - Mathf.Lerp(2f, 4f, shellModes[num10, 0]));
                    shells[num8, 0] += vector4 * (Vector2.Distance(shells[num8, 0], vector3) - Mathf.Lerp(2f, 4f, shellModes[num10, 0]));
                }
                else
                {
                    shells[num8, 1] += (vector3 - shells[num8, 0]) / 5f;
                }
            }
            for (int num12 = 0; num12 < 2; num12++)
            {
                shellModes[num12, 1] = shellModes[num12, 0];
                if (shellModeChangeCounter[num12] >= 30)
                {
                    shellModes[num12, 0] = Mathf.Lerp(shellModes[num12, 0], alcedo.tentacles[num12].flyingMode, 0.2f);
                    if (Mathf.Abs(shellModes[num12, 0] - alcedo.tentacles[num12].flyingMode) < 0.1f)
                    {
                        shellModes[num12, 0] = alcedo.tentacles[num12].flyingMode;
                        shellModeChangeCounter[num12] = 0;
                    }
                }
                else if (alcedo.tentacles[num12].flyingMode > 0f && shellModes[num12, 0] == 0f)
                {
                    shellModeChangeCounter[num12]++;
                }
                else if (alcedo.tentacles[num12].flyingMode == 0f && shellModes[num12, 0] != 0f)
                {
                    shellModeChangeCounter[num12]++;
                }
                else
                {
                    shellModeChangeCounter[num12] = 0;
                }
            }
            Vector2 b = new Vector2(0f, 0f);
            for (int num13 = 0; num13 < alcedo.bodyChunks.Length; num13++)
            {
                b += alcedo.bodyChunks[num13].pos - alcedo.bodyChunks[num13].lastPos;
            }
            b /= (float)alcedo.bodyChunks.Length;
            if (neckTubes != null)
            {
                for (int num14 = 0; num14 < neckTubes.GetLength(0); num14++)
                {
                    TuskConnectionPositions(num14, 1f, ref _cachedTusk2);
                    ConnectNeckTubes(num14, _cachedTusk2);
                    Vector2 vector5 = Custom.DirVec(alcedo.bodyChunks[1].pos, Vector2.Lerp(alcedo.bodyChunks[2 + num14].pos, alcedo.bodyChunks[0].pos, 0.5f));
                    for (int num15 = 0; num15 < neckTubes.GetLength(1); num15++)
                    {
                        float value = Mathf.InverseLerp(0f, neckTubes.GetLength(1) - 1, num15);
                        neckTubes[num14, num15, 2] += vector5 * Mathf.InverseLerp(0.6f, 1f, value) * 2f;
                        neckTubes[num14, num15, 2] -= _cachedTusk2[1] * Mathf.InverseLerp(0.25f, 0f, value) * 2f;
                        neckTubes[num14, num15, 1] = neckTubes[num14, num15, 0];
                        neckTubes[num14, num15, 0] += neckTubes[num14, num15, 2];
                        neckTubes[num14, num15, 2] = Vector2.Lerp(neckTubes[num14, num15, 2], b, 0.05f);
                        neckTubes[num14, num15, 2].y -= 0.9f;
                        if (num15 > 1)
                        {
                            Vector2 vector6 = Custom.DirVec(neckTubes[num14, num15, 0], neckTubes[num14, num15 - 2, 0]);
                            neckTubes[num14, num15, 2] -= vector6;
                            neckTubes[num14, num15 - 2, 2] += vector6;
                        }
                        if (num15 > 2 && num15 < neckTubes.GetLength(1) - 3)
                        {
                            SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(neckTubes[num14, num15, 0], neckTubes[num14, num15, 1], neckTubes[num14, num15, 2], 1f, new IntVector2(0, 0), goThroughFloors: true);
                            cd = SharedPhysics.VerticalCollision(alcedo.room, cd);
                            cd = SharedPhysics.HorizontalCollision(alcedo.room, cd);
                            neckTubes[num14, num15, 0] = cd.pos;
                            neckTubes[num14, num15, 2] = cd.vel;
                        }
                    }
                    ConnectNeckTubes(num14, _cachedTusk2);
                }
            }
            if (!shadowMode && alcedo.mainBodyChunk.pos.y > alcedo.room.PixelHeight + 400f)
            {
                shadowMode = true;
            }
            else if (shadowMode && alcedo.mainBodyChunk.pos.y < alcedo.room.PixelHeight + 200f)
            {
                shadowMode = false;
            }
        }

        private void ConnectNeckTubes(int s, Vector2[] tuskCon)
        {
            Vector2 vector = Vector2.Lerp(alcedo.bodyChunks[2 + s].pos, alcedo.bodyChunks[0].pos, 0.5f);
            vector += Custom.DirVec(alcedo.bodyChunks[1].pos, vector) * 11f;
            neckTubes[s, 0, 0] = tuskCon[0] + tuskCon[1] * 5f - tuskCon[2] * 10f;
            neckTubes[s, 0, 2] *= 0f;
            neckTubes[s, neckTubes.GetLength(1) - 1, 0] = vector;
            neckTubes[s, neckTubes.GetLength(1) - 1, 2] *= 0f;
            for (int i = 0; i < neckTubes.GetLength(1) - 1; i++)
            {
                Vector2 vector2 = Custom.DirVec(neckTubes[s, i, 0], neckTubes[s, i + 1, 0]) * (Vector2.Distance(neckTubes[s, i, 0], neckTubes[s, i + 1, 0]) - 5f);
                neckTubes[s, i, 0] += vector2 / 2f;
                neckTubes[s, i, 2] += vector2 / 2f;
                neckTubes[s, i + 1, 0] -= vector2 / 2f;
                neckTubes[s, i + 1, 2] -= vector2 / 2f;
            }
            neckTubes[s, 0, 0] = tuskCon[0] + tuskCon[1] * 5f - tuskCon[2] * 10f;
            neckTubes[s, 0, 2] *= 0f;
            neckTubes[s, neckTubes.GetLength(1) - 1, 0] = vector;
            neckTubes[s, neckTubes.GetLength(1) - 1, 2] *= 0f;
        }

        public void TuskConnectionPositions(int tusk, float timeStacker, ref Vector2[] resVec)
        {
            Vector2 vector = Custom.DirVec(Vector2.Lerp(alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].lastPos, alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].pos, timeStacker), Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker));
            Vector2 vector2 = Custom.PerpendicularVector(vector);
            float num = Custom.AimFromOneVectorToAnother(Vector2.Lerp(alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].lastPos, alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].pos, timeStacker), Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker)) - Custom.AimFromOneVectorToAnother(Vector2.Lerp(alcedo.bodyChunks[0].lastPos, alcedo.bodyChunks[0].pos, timeStacker), Vector2.Lerp(alcedo.bodyChunks[1].lastPos, alcedo.bodyChunks[1].pos, timeStacker));
            if (num > 180f)
            {
                num -= 360f;
            }
            else if (num < -180f)
            {
                num += 360f;
            }
            Vector2 vector3 = vector2 * ((tusk == 0) ? (-1f) : 1f) * Mathf.Pow(Mathf.Abs(Mathf.Cos((float)Math.PI * num / 180f)), 0.5f) * ((Mathf.Abs(num) > 90f) ? (-1f) : 1f);
            Vector2 vector4 = Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker) + vector * Mathf.Lerp(7f, -13f, (alcedo.Snapping || alcedo.grasps[0] != null) ? 0f : alcedo.TusksStuck) + vector3 * Mathf.Lerp(4f, 9f, alcedo.TusksStuck);
            resVec[0] = vector4;
            resVec[1] = vector;
            resVec[2] = vector3;
            resVec[3] = vector4 + vector * 20f - vector3 * ((alcedo.grasps[0] != null) ? 5f : 0f);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[TotalSprites];/*
            if (alcedo.kingTusks != null)
            {
                alcedo.kingTusks.InitiateSprites(this, sLeaser, rCam);
            }*/
            #region 尾巴
            TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[(this.tail.Length - 1) * 4 + 1];
            TriangleMesh triangleMesh = new TriangleMesh("Futile_White", array, false, false);
            for (int num8 = 0; num8 < this.tail.Length - 1; num8++)
            {
                int num9 = num8 * 4;
                for (int num10 = 0; num10 < 4; num10++)
                {
                    array[num9 + num10] = new TriangleMesh.Triangle(num9 + num10, num9 + num10 + 1, num9 + num10 + 2);
                }
            }
            array[(this.tail.Length - 1) * 4] = new TriangleMesh.Triangle((this.tail.Length - 1) * 4, (this.tail.Length - 1) * 4 + 1, (this.tail.Length - 1) * 4 + 2);
            triangleMesh = new TriangleMesh("Futile_White", array, false, false);
            sLeaser.sprites[this.TailSpriteStart] = triangleMesh;
            #endregion
            for (int i = 0; i < 2; i++)
            {
                //前爪
                sLeaser.sprites[ForePawSprite(i)] = new FSprite("JetFishFlipper1");
                sLeaser.sprites[ForePawColorSprite(i)] = new FSprite("AlcedoClawColorA");/*
                //肩膀（已隐藏）
                sLeaser.sprites[BackShieldSprite(i)] = new FSprite("KrakenShield0");
                sLeaser.sprites[FrontShieldSprite(i)] = new FSprite("KrakenShield0");
                sLeaser.sprites[BackShieldSprite(i)].isVisible = false;
                sLeaser.sprites[FrontShieldSprite(i)].isVisible = false;
                //垂肢（已隐藏）
                sLeaser.sprites[AppendageSprite(i)] = TriangleMesh.MakeLongMesh(appendages[i].Length, pointyTip: false, customColor: true);
                sLeaser.sprites[AppendageSprite(i)].isVisible = false;*/
                //后爪
                sLeaser.sprites[HindPawSprite(i)] = new FSprite("JetFishFlipper1");
                sLeaser.sprites[HindPawColorSprite(i)] = new FSprite("AlcedoClawColorA");
                if (!IsMiros)
                {
                    sLeaser.sprites[TuskSprite(i)] = new FSprite("pixel");
                    sLeaser.sprites[TuskSprite(i)].anchorY = 1f;
                    sLeaser.sprites[TuskSprite(i)].isVisible = false;
                }
                if (i == 1)
                {
                    sLeaser.sprites[ForePawSprite(i)].scaleX = (IsKing ? (-1.15f) : (-1f));
                    sLeaser.sprites[ForePawColorSprite(i)].scaleX = (IsKing ? (-1.15f) : (-1f));/*
                    sLeaser.sprites[BackShieldSprite(i)].scaleX = (IsKing ? (-1.15f) : (-1f));
                    sLeaser.sprites[FrontShieldSprite(i)].scaleX = (IsKing ? (-1.15f) : (-1f));*/
                }
                else
                {
                    sLeaser.sprites[ForePawSprite(i)].scaleX = (IsKing ? 1.15f : 1f);
                    sLeaser.sprites[ForePawColorSprite(i)].scaleX = (IsKing ? 1.15f : 1f);/*
                    sLeaser.sprites[BackShieldSprite(i)].scaleX = (IsKing ? 1.15f : 1f);
                    sLeaser.sprites[FrontShieldSprite(i)].scaleX = (IsKing ? 1.15f : 1f);*/
                }
                sLeaser.sprites[ForePawSprite(i)].anchorX = 1f;
                sLeaser.sprites[ForePawColorSprite(i)].anchorX = 1f;/*
                sLeaser.sprites[BackShieldSprite(i)].anchorY = 1f;
                sLeaser.sprites[FrontShieldSprite(i)].anchorY = 1f;*/
                if (i == 1)
                {
                    sLeaser.sprites[HindPawSprite(i)].scaleX = (IsKing ? (-1.15f) : (-1f));
                    sLeaser.sprites[HindPawColorSprite(i)].scaleX = (IsKing ? (-1.15f) : (-1f));
                }
                else
                {
                    sLeaser.sprites[HindPawSprite(i)].scaleX = (IsKing ? 1.15f : 1f);
                    sLeaser.sprites[HindPawColorSprite(i)].scaleX = (IsKing ? 1.15f : 1f);
                }
                sLeaser.sprites[HindPawSprite(i)].anchorX = 1f;
                sLeaser.sprites[HindPawColorSprite(i)].anchorX = 1f;
            }
            for (int j = 0; j < alcedo.tentacles.Length; j++)
            {
                sLeaser.sprites[TentacleSprite(j)] = TriangleMesh.MakeLongMesh(alcedo.tentacles[j].tChunks.Length, pointyTip: false, customColor: false);
            }
            for (int j = 0; j < alcedo.legs.Length; j++)
            {
                sLeaser.sprites[LegSprite(j)] = TriangleMesh.MakeLongMesh(alcedo.legs[j].tChunks.Length, pointyTip: false, customColor: false);
            }
            for (int k = 0; k < alcedo.tentacles.Length; k++)
            {
                for (int x = 0; x < featherLayersPerWing; x++)
                    for (int l = 0; l < feathersPerLayer; l++)
                    {
                        FSprite sp1 = new FSprite("AlcedoFeatherA");
                        FSprite sp2 = new FSprite("AlcedoFeatherColorA");
                        if (wings[k, x, l].type == "Covert")
                        {
                            sp1 = new FSprite("AlcedoFeatherB");
                            sp2 = new FSprite("AlcedoFeatherColorB");
                        }
                        else if (wings[k, x, l].type == "FlightFeather")
                        {

                        }
                        sLeaser.sprites[IsMiros ? FeatherColorSprite(k, x, l) : FeatherSprite(k, x, l)] = sp1;
                        sLeaser.sprites[IsMiros ? FeatherColorSprite(k, x, l) : FeatherSprite(k, x, l)].anchorY = (IsMiros ? 0.94f : 0.97f);
                        if (IsMiros && l == feathersPerLayer - 1)
                        {
                            sLeaser.sprites[FeatherSprite(k, x, l)] = new FSprite("MirosForePaw");
                            sLeaser.sprites[FeatherSprite(k, x, l)].anchorY = 0.3f;
                            sLeaser.sprites[FeatherSprite(k, x, l)].anchorX = 0f;
                        }
                        else
                        {
                            sLeaser.sprites[IsMiros ? FeatherSprite(k, x, l) : FeatherColorSprite(k, x, l)] = sp2;
                            sLeaser.sprites[IsMiros ? FeatherSprite(k, x, l) : FeatherColorSprite(k, x, l)].anchorY = (IsMiros ? 0.94f : 0.97f);
                        }
                    }
            }
            //上半身
            sLeaser.sprites[BodySprite(0)] = new FSprite("KrakenBody");
            sLeaser.sprites[BodySprite(0)].scale = (IsKing ? 1.2f : 1f) * 0.8f;
            sLeaser.sprites[BodySprite(0)].scaleX = sLeaser.sprites[BodySprite(0)].scaleX * 0.6f;
            //下半身
            sLeaser.sprites[BodySprite(1)] = new FSprite("KrakenBody");
            sLeaser.sprites[BodySprite(1)].scale = (IsKing ? 1.2f : 1f) * 0.75f;
            sLeaser.sprites[BodySprite(1)].scaleX = sLeaser.sprites[BodySprite(1)].scaleX * 0.5f;
            sLeaser.sprites[BodySprite(1)].anchorY = 0.4f;
            //腰部
            sLeaser.sprites[BodySprite(2)] = TriangleMesh.MakeLongMesh(alcedo.waist.tChunks.Length, pointyTip: false, customColor: true);
            //脖子
            sLeaser.sprites[NeckSprite] = TriangleMesh.MakeLongMesh(alcedo.neck.tChunks.Length, pointyTip: false, customColor: true);
            //头
            sLeaser.sprites[HeadSprite] = new FSprite("AlcedoHeadA0");
            sLeaser.sprites[HeadSprite].anchorX = 0.5f;
            sLeaser.sprites[HeadSprite].anchorY = 0.5f;
            sLeaser.sprites[EyesSprite] = new FSprite(IsMiros ? "Circle20" : "AlcedoEyesA0");
            sLeaser.sprites[EyesSprite].anchorX = 0.5f;
            sLeaser.sprites[EyesSprite].anchorY = 0.5f;
            if (!IsMiros)
            {
                sLeaser.sprites[MaskSprite] = new FSprite("AlcedoMaskA0");
                sLeaser.sprites[MaskSprite].anchorX = 0.5f;
                sLeaser.sprites[MaskSprite].anchorY = 0.5f;/*
                if (IsKing)
                {
                    sLeaser.sprites[MaskArrowSprite] = new FSprite("KrakenArrow0");
                }*/
            }
            AddToContainer(sLeaser, rCam, null);
            base.InitiateSprites(sLeaser, rCam);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (shadowMode)
            {
                newContatiner = rCam.ReturnFContainer("Shadows");
                if (IsMiros)
                {
                    base.AddToContainer(sLeaser, rCam, newContatiner);
                    return;
                }
            }
            else
            {
                if (IsMiros)
                {
                    sLeaser.RemoveAllSpritesFromContainer();
                }
                if (newContatiner == null)
                {
                    newContatiner = rCam.ReturnFContainer("Midground");
                }
            }
            for (int k = 0; k < sLeaser.sprites.Length; k++)
            {
                sLeaser.sprites[k].RemoveFromContainer();/*
                if (IsKingTuskSprite(k))
                {
                    //alcedo.kingTusks.AddToContainer(this, k, sLeaser, rCam, newContatiner);
                }
                else
                {*/
                    newContatiner.AddChild(sLeaser.sprites[k]);
                //}
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            this.palette = palette;
            ExitShadowMode(sLeaser, rCam, changeContainer: false);
            base.ApplyPalette(sLeaser, rCam, palette);
        }

        private void EnterShadowMode(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, bool changeContainer)
        {/*
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                if (!IsKingTuskSprite(i))
                {
                    sLeaser.sprites[i].color = new Color(0.003921569f, 0f, 0f);
                }
            }
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < (sLeaser.sprites[AppendageSprite(j)] as TriangleMesh).verticeColors.Length; k++)
                {
                    (sLeaser.sprites[AppendageSprite(j)] as TriangleMesh).verticeColors[k] = new Color(0.003921569f, 0f, 0f);
                }
            }*/
            if (changeContainer)
            {
                AddToContainer(sLeaser, rCam, null);
            }
        }

        private void ExitShadowMode(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, bool changeContainer)
        {
            Color color = palette.blackColor;
            _ = ColorB.rgb;
            Color color2 = Color.white;
            if (albino)
            {
                color = Color.Lerp(color, Color.white, 0.86f - palette.darkness / 1.8f);
                HSLColor colorB = ColorB;
                colorB.saturation = Mathf.Lerp(colorB.saturation, 1f, 0.15f);
                colorB.hue = 0f;
                _ = colorB.rgb;
                color2 = Color.Lerp(color, palette.blackColor, 0.74f);
                color = Color.Lerp(color, palette.skyColor, 0.21f);
            }
            float t = 0f;
            if (ModManager.MMF)
            {
                t = darkness;
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].color = color;
            }
            /*
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < (sLeaser.sprites[AppendageSprite(j)] as TriangleMesh).verticeColors.Length; k++)
                {
                    float value = (float)(k / 2) * 2f / (float)Math.Max((sLeaser.sprites[AppendageSprite(j)] as TriangleMesh).verticeColors.Length - 1, 24);
                    float f = Mathf.Clamp(Mathf.InverseLerp(0.15f, 0.7f, value), 0f, 1f);
                    f = Mathf.Pow(f, 0.5f);
                    if (albino)
                    {
                        (sLeaser.sprites[AppendageSprite(j)] as TriangleMesh).verticeColors[k] = Color.Lerp(Color.Lerp(color, color2, f), palette.blackColor, t);
                    }
                    else
                    {
                        (sLeaser.sprites[AppendageSprite(j)] as TriangleMesh).verticeColors[k] = Color.Lerp(Color.Lerp(palette.blackColor, ColorA.rgb, f), palette.blackColor, t);
                    }
                }
            }*/
            Color color3 = Color.Lerp(ColorA.rgb, new Color(1f, 1f, 1f), 0.35f);
            if (!IsMiros)
            {
                sLeaser.sprites[MaskSprite].color = Color.Lerp(color3, palette.blackColor, t);
            }/*
            for (int l = 0; l < 2; l++)
            {
                if (albino)
                {
                    sLeaser.sprites[BackShieldSprite(l)].color = Color.Lerp(Color.Lerp(color2, color, 0.75f), palette.blackColor, t);
                    sLeaser.sprites[FrontShieldSprite(l)].color = Color.Lerp(Color.Lerp(color2, color, 0.35f), palette.blackColor, t);
                }
                else
                {
                    sLeaser.sprites[BackShieldSprite(l)].color = Color.Lerp(Color.Lerp(ColorA.rgb, palette.blackColor, 0.85f), palette.blackColor, t);
                    sLeaser.sprites[FrontShieldSprite(l)].color = Color.Lerp(Color.Lerp(ColorA.rgb, palette.blackColor, 0.55f), palette.blackColor, t);
                }
            }*/
            if (IsMiros)
            {
                sLeaser.sprites[EyesSprite].color = eyeCol;
            }
            else
            {
                sLeaser.sprites[EyesSprite].color = Color.Lerp(Color.Lerp(ColorB.rgb, new Color(0f, 0f, 0f), IsKing ? 0.4f : 0.6f), palette.blackColor, t);
            }
            if (albino)
            {
                sLeaser.sprites[EyesSprite].color = Color.Lerp(Color.Lerp(Color.red, new Color(0f, 0f, 0f), IsKing ? 0.2f : 0.3f), palette.blackColor, t);
            }/*
            if (alcedo.kingTusks != null)
            {
                alcedo.kingTusks.ApplyPalette(this, palette, color3, sLeaser, rCam);
                sLeaser.sprites[MaskArrowSprite].color = Color.Lerp(Color.Lerp(Color.Lerp(HSLColor.Lerp(ColorA, ColorB, 0.5f).rgb, palette.blackColor, 0.53f), color3, 0.1f), palette.blackColor, t);
            }*/
            if (changeContainer)
            {
                AddToContainer(sLeaser, rCam, null);
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            darkness = rCam.room.Darkness(Vector2.Lerp(alcedo.mainBodyChunk.lastPos, alcedo.mainBodyChunk.pos, timeStacker));
            darkness *= 1f - 0.5f * rCam.room.LightSourceExposure(Vector2.Lerp(alcedo.mainBodyChunk.lastPos, alcedo.mainBodyChunk.pos, timeStacker));
            spritesInShadowMode = sLeaser.sprites[BodySprite(0)].color == new Color(0.003921569f, 0f, 0f);
            if (!shadowMode && (ModManager.MMF || spritesInShadowMode))
            {
                ExitShadowMode(sLeaser, rCam, changeContainer: true);
                spritesInShadowMode = false;
            }
            else if (shadowMode && !spritesInShadowMode)
            {
                EnterShadowMode(sLeaser, rCam, changeContainer: true);
                spritesInShadowMode = true;
            }
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (shadowMode)
            {
                camPos.y += rCam.room.PixelHeight + 300f;
            }
            else if (culled)
            {
                return;
            }
            Vector2 backPos = Vector2.Lerp(base.owner.bodyChunks[1].lastPos, base.owner.bodyChunks[1].pos, timeStacker);
            Vector2 chestPos = Vector2.Lerp(base.owner.bodyChunks[0].lastPos, base.owner.bodyChunks[0].pos, timeStacker);
            Vector2 bellyPos = Vector2.Lerp(base.owner.bodyChunks[5].lastPos, base.owner.bodyChunks[5].pos, timeStacker);
            Vector2 hipPos = Vector2.Lerp(base.owner.bodyChunks[6].lastPos, base.owner.bodyChunks[6].pos, timeStacker);
            #region 尾巴
            Vector2 lastTailPos = Vector2.Lerp(bellyPos, hipPos, 0.5f); 
            float width = base.owner.bodyChunks[5].rad * 0.7f;
            for (int num11 = 0; num11 < this.tail.Length; num11++)
            {
                Vector2 tailPos = Vector2.Lerp(this.tail[num11].lastPos, this.tail[num11].pos, timeStacker);
                Vector2 tailDir = (tailPos - lastTailPos).normalized;
                Vector2 tailPerp = Custom.PerpendicularVector(tailDir);
                float d2 = Vector2.Distance(tailPos, lastTailPos) / 5f;
                (sLeaser.sprites[TailSpriteStart] as TriangleMesh).MoveVertice(num11 * 4 + 0, lastTailPos - tailPerp * (width + this.tail[num11].StretchedRad) * 0.5f + tailDir * d2 - camPos);
                (sLeaser.sprites[TailSpriteStart] as TriangleMesh).MoveVertice(num11 * 4 + 1, lastTailPos + tailPerp * (width + this.tail[num11].StretchedRad) * 0.5f + tailDir * d2 - camPos);
                if (num11 < this.tail.Length - 1)
                {
                    (sLeaser.sprites[TailSpriteStart] as TriangleMesh).MoveVertice(num11 * 4 + 2, tailPos - tailPerp * this.tail[num11].StretchedRad - tailDir * d2 - camPos);
                    (sLeaser.sprites[TailSpriteStart] as TriangleMesh).MoveVertice(num11 * 4 + 3, tailPos + tailPerp * this.tail[num11].StretchedRad - tailDir * d2 - camPos);
                }
                else
                {
                    (sLeaser.sprites[TailSpriteStart] as TriangleMesh).MoveVertice(num11 * 4 + 2, tailPos - camPos);
                }
                width = this.tail[num11].StretchedRad;
                lastTailPos = tailPos;
            }
            #endregion
            #region 身体
            sLeaser.sprites[BodySprite(0)].x = Mathf.Lerp(backPos.x, chestPos.x, 0.5f) - camPos.x;
            sLeaser.sprites[BodySprite(0)].y = Mathf.Lerp(backPos.y, chestPos.y, 0.5f) - camPos.y;
            sLeaser.sprites[BodySprite(0)].rotation = Custom.AimFromOneVectorToAnother(chestPos, backPos);
            sLeaser.sprites[BodySprite(1)].x = Mathf.Lerp(bellyPos.x, hipPos.x, 0.5f) - camPos.x;
            sLeaser.sprites[BodySprite(1)].y = Mathf.Lerp(bellyPos.y, hipPos.y, 0.5f) - camPos.y;
            sLeaser.sprites[BodySprite(1)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(bellyPos, hipPos, 0.5f), Vector2.Lerp(backPos, chestPos, 0.5f));
            #endregion
            Vector2 headPos = Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, alcedo.Snapping ? Mathf.Lerp(-1.5f, 1.5f, UnityEngine.Random.value) : timeStacker);
            if (alcedo.ChargingSnap)
            {
                headPos += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 4f;
            }
            float neckToHeadAngle = Custom.AimFromOneVectorToAnother(Vector2.Lerp(alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].lastPos, 
                                                                                  alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].pos, timeStacker), 
                Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker));
            sLeaser.sprites[HeadSprite].x = headPos.x - camPos.x;
            sLeaser.sprites[HeadSprite].y = headPos.y - camPos.y;
            sLeaser.sprites[EyesSprite].x = headPos.x - camPos.x;
            sLeaser.sprites[EyesSprite].y = headPos.y - camPos.y;
            if (!IsMiros)
            {
                sLeaser.sprites[MaskSprite].x = headPos.x - camPos.x;
                sLeaser.sprites[MaskSprite].y = headPos.y - camPos.y;
            }
            float num3 = (float)(8 - headGraphic) * Mathf.Sign(neckToHeadAngle) * 22.5f;
            sLeaser.sprites[HeadSprite].rotation = neckToHeadAngle - num3;
            sLeaser.sprites[HeadSprite].element = Futile.atlasManager.GetElementWithName("AlcedoHeadA" + headGraphic);
            sLeaser.sprites[HeadSprite].anchorX = 0.5f;
            sLeaser.sprites[HeadSprite].anchorY = 0.5f;
            sLeaser.sprites[HeadSprite].scaleX = ((neckToHeadAngle > 0f) ? (-1f) : 1f) * (IsKing ? 1.15f : 1f);
            sLeaser.sprites[HeadSprite].scaleY = (IsKing ? 1.15f : 1f);
            sLeaser.sprites[MaskSprite].rotation = neckToHeadAngle - num3;
            sLeaser.sprites[MaskSprite].element = Futile.atlasManager.GetElementWithName("AlcedoMaskA" + headGraphic);
            sLeaser.sprites[MaskSprite].anchorX = 0.5f;
            sLeaser.sprites[MaskSprite].anchorY = 0.5f;
            sLeaser.sprites[MaskSprite].scaleX = ((neckToHeadAngle > 0f) ? (-1f) : 1f) * (IsKing ? 1.15f : 1f);
            sLeaser.sprites[MaskSprite].scaleY = (IsKing ? 1.15f : 1f);
            sLeaser.sprites[MaskSprite].isVisible = (alcedo.State as Alcedo.AlcedoState).mask;

            sLeaser.sprites[EyesSprite].rotation = neckToHeadAngle - num3;
            sLeaser.sprites[EyesSprite].element = Futile.atlasManager.GetElementWithName("AlcedoEyesA" + headGraphic);
            sLeaser.sprites[EyesSprite].scaleX = ((neckToHeadAngle > 0f) ? (-1f) : 1f) * (IsKing ? 1.15f : 1f);
            sLeaser.sprites[EyesSprite].anchorX = 0.5f;
            sLeaser.sprites[EyesSprite].anchorY = 0.5f;
            sLeaser.sprites[EyesSprite].scaleY = (IsKing ? 1.15f : 1f);/*
            if (IsKing)
            {
                sLeaser.sprites[MaskArrowSprite].x = headPos.x - camPos.x;
                sLeaser.sprites[MaskArrowSprite].y = headPos.y - camPos.y;
                sLeaser.sprites[MaskArrowSprite].rotation = neckToHeadAngle - num3;
                sLeaser.sprites[MaskArrowSprite].element = Futile.atlasManager.GetElementWithName("KrakenArrow" + headGraphic);
                sLeaser.sprites[MaskArrowSprite].scaleX = ((neckToHeadAngle > 0f) ? (-1f) : 1f) * (IsKing ? 1.15f : 1f);
                sLeaser.sprites[MaskArrowSprite].scaleY = (IsKing ? 1.15f : 1f);
                sLeaser.sprites[MaskArrowSprite].isVisible = (alcedo.State as Alcedo.AlcedoState).mask;
            }*/
            Vector2 lastNeckPos = Vector2.Lerp(alcedo.neck.connectedChunk.lastPos, alcedo.neck.connectedChunk.pos, timeStacker);
            float lastNeckStretchedRad = (IsKing ? 11f : 8f);
            for (int j = 0; j < alcedo.neck.tChunks.Length; j++)
            {
                Vector2 neckPos = Vector2.Lerp(alcedo.neck.tChunks[j].lastPos, alcedo.neck.tChunks[j].pos, timeStacker);
                if (ModManager.MMF && !IsMiros)
                {
                    neckPos = Vector2.Lerp(neckPos, Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker), Mathf.Pow(j / (alcedo.neck.tChunks.Length - 1), 7f));
                    //neckPos = Vector2.Lerp(neckPos, Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker), Mathf.Pow(j / (alcedo.neck.tChunks.Length - 1), 7f));
                }
                if (j == alcedo.neck.tChunks.Length - 1)
                {
                    neckPos = headPos;//Vector2.Lerp(neckPos, headPos, 0.5f) - Custom.DegToVec(neckToHeadAngle) * (IsKing ? 1f : 5f);
                }
                Vector2 normalized = (neckPos - lastNeckPos).normalized;
                Vector2 neckPerp = Custom.PerpendicularVector(normalized);
                float segLength = Vector2.Distance(neckPos, lastNeckPos) / 5f;
                (sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(j * 4 + 0, lastNeckPos - neckPerp * (alcedo.neck.tChunks[j].stretchedRad + lastNeckStretchedRad) * 0.5f + normalized * segLength - camPos);
                (sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(j * 4 + 1, lastNeckPos + neckPerp * (alcedo.neck.tChunks[j].stretchedRad + lastNeckStretchedRad) * 0.5f + normalized * segLength - camPos);
                if (j == alcedo.neck.tChunks.Length - 1 && !IsMiros)
                {
                    segLength = 0f;
                }
                (sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(j * 4 + 2, neckPos - neckPerp * alcedo.neck.tChunks[j].stretchedRad - normalized * segLength - camPos);
                (sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(j * 4 + 3, neckPos + neckPerp * alcedo.neck.tChunks[j].stretchedRad - normalized * segLength - camPos);
                lastNeckStretchedRad = alcedo.neck.tChunks[j].stretchedRad;
                lastNeckPos = neckPos;
            }
            #region 腰部
            Vector2 lastWaistPos = Vector2.Lerp(alcedo.waist.connectedChunk.lastPos, alcedo.waist.connectedChunk.pos, timeStacker);
            float lastWaistStretchedRad = 8f;
            for (int j = 0; j < alcedo.waist.tChunks.Length; j++)
            {
                Vector2 waistPos = Vector2.Lerp(alcedo.waist.tChunks[j].lastPos, alcedo.waist.tChunks[j].pos, timeStacker);
                if (ModManager.MMF && !IsMiros)
                {
                    waistPos = Vector2.Lerp(waistPos, Vector2.Lerp(bellyPos, hipPos, 0.5f), Mathf.Pow(j / (alcedo.waist.tChunks.Length - 1), 7f));
                }
                if (j == alcedo.waist.tChunks.Length - 1)
                {
                    waistPos = Vector2.Lerp(bellyPos, hipPos, 0.5f);
                }
                Vector2 normalized = (waistPos - lastWaistPos).normalized;
                Vector2 waistPerp = Custom.PerpendicularVector(normalized);
                float segLength = Vector2.Distance(waistPos, lastWaistPos) / 5f;
                (sLeaser.sprites[BodySprite(2)] as TriangleMesh).MoveVertice(j * 4 + 0, lastWaistPos - waistPerp * (alcedo.waist.tChunks[j].stretchedRad + lastWaistStretchedRad) * 0.5f + normalized * segLength - camPos);
                (sLeaser.sprites[BodySprite(2)] as TriangleMesh).MoveVertice(j * 4 + 1, lastWaistPos + waistPerp * (alcedo.waist.tChunks[j].stretchedRad + lastWaistStretchedRad) * 0.5f + normalized * segLength - camPos);
                (sLeaser.sprites[BodySprite(2)] as TriangleMesh).MoveVertice(j * 4 + 2, waistPos - waistPerp * alcedo.waist.tChunks[j].stretchedRad - normalized * segLength - camPos);
                (sLeaser.sprites[BodySprite(2)] as TriangleMesh).MoveVertice(j * 4 + 3, waistPos + waistPerp * alcedo.waist.tChunks[j].stretchedRad - normalized * segLength - camPos);
                lastWaistStretchedRad = alcedo.waist.tChunks[j].stretchedRad;
                lastWaistPos = waistPos;
            }
            #endregion
            #region 翅膀
            for (int k = 0; k < alcedo.tentacles.Length; k++)
            {
                Vector2 lastTentaclePos = Vector2.Lerp(alcedo.tentacles[k].connectedChunk.lastPos, alcedo.tentacles[k].connectedChunk.pos, timeStacker);
                float lastTentacleRad = 10f;
                for (int m = 0; m < alcedo.tentacles[k].tChunks.Length; m++)
                {
                    Vector2 tentaclePos = Vector2.Lerp(alcedo.tentacles[k].tChunks[m].lastPos, alcedo.tentacles[k].tChunks[m].pos, timeStacker);
                    Vector2 dir = (tentaclePos - lastTentaclePos).normalized;
                    Vector2 tentaclePerp = Custom.PerpendicularVector(dir);
                    float segLength = Vector2.Distance(tentaclePos, lastTentaclePos) / 5f;
                    float tentacleRad = alcedo.tentacles[k].TentacleContour(((float)m + 0.5f) / (float)alcedo.tentacles[k].tChunks.Length);
                    tentacleRad *= Mathf.Clamp(Mathf.Pow(alcedo.tentacles[k].tChunks[m].stretchedFac, 0.35f), 0.5f, 1.5f);
                    (sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 0, lastTentaclePos - tentaclePerp * (tentacleRad + lastTentacleRad) * 0.5f + dir * segLength - camPos);
                    (sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 1, lastTentaclePos + tentaclePerp * (tentacleRad + lastTentacleRad) * 0.5f + dir * segLength - camPos);
                    tentacleRad = alcedo.tentacles[k].TentacleContour(((float)m + 1f) / (float)alcedo.tentacles[k].tChunks.Length);
                    tentacleRad *= Mathf.Clamp(Mathf.Pow(alcedo.tentacles[k].tChunks[m].stretchedFac, 0.35f), 0.5f, 1.5f);
                    (sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 2, tentaclePos - tentaclePerp * tentacleRad - dir * segLength - camPos);
                    (sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 3, tentaclePos + tentaclePerp * tentacleRad - dir * segLength - camPos);
                    //(sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).color = Color.white;
                    lastTentacleRad = tentacleRad;
                    lastTentaclePos = tentaclePos;
                }
                int halfWingIndex = alcedo.tentacles[k].halfWingIndex;
                Vector2 halfWingPos = Vector2.Lerp(alcedo.tentacles[k].tChunks[halfWingIndex].lastPos, alcedo.tentacles[k].tChunks[halfWingIndex].pos, timeStacker);
                Vector2 nextHalfWingPos = Vector2.Lerp(alcedo.tentacles[k].tChunks[halfWingIndex - 1].lastPos, alcedo.tentacles[k].tChunks[halfWingIndex - 1].pos, timeStacker);
                sLeaser.sprites[ForePawSprite(k)].x = halfWingPos.x - camPos.x;
                sLeaser.sprites[ForePawSprite(k)].y = halfWingPos.y - camPos.y;
                sLeaser.sprites[ForePawSprite(k)].rotation = Custom.AimFromOneVectorToAnother(halfWingPos, nextHalfWingPos) * Mathf.Lerp(0.9f, 1f, this.alcedo.tentacles[k].flyingMode)
                    - Mathf.Lerp(-10f, 60f, this.alcedo.tentacles[k].flyingMode) * (k % 2 == 0 ? 1f : -1f);
                sLeaser.sprites[ForePawColorSprite(k)].x = sLeaser.sprites[ForePawSprite(k)].x;
                sLeaser.sprites[ForePawColorSprite(k)].y = sLeaser.sprites[ForePawSprite(k)].y;
                sLeaser.sprites[ForePawColorSprite(k)].rotation = sLeaser.sprites[ForePawSprite(k)].rotation;
                sLeaser.sprites[ForePawSprite(k)].color = sLeaser.sprites[BodySprite(0)].color;
                sLeaser.sprites[ForePawColorSprite(k)].color = Color.Lerp(wings[k, 0, 0].CurrentClawColor(), palette.blackColor, (ModManager.MMF && !IsMiros) ? darkness : 0f);
                for (int l = 0; l < featherLayersPerWing; l++)
                    for (int n = 0; n < feathersPerLayer; n++)
                    {
                        sLeaser.sprites[FeatherSprite(k, l, n)].x = Mathf.Lerp(wings[k, l, n].ConnectedLastPos.x, wings[k, l, n].ConnectedPos.x, timeStacker) - camPos.x;
                        sLeaser.sprites[FeatherSprite(k, l, n)].y = Mathf.Lerp(wings[k, l, n].ConnectedLastPos.y, wings[k, l, n].ConnectedPos.y, timeStacker) - camPos.y;
                        sLeaser.sprites[FeatherSprite(k, l, n)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(wings[k, l, n].lastPos, wings[k, l, n].pos, timeStacker), Vector2.Lerp(wings[k, l, n].ConnectedLastPos, wings[k, l, n].ConnectedPos, timeStacker));
                        if (!IsMiros || n != feathersPerLayer - 1)
                        {
                            sLeaser.sprites[FeatherSprite(k, l, n)].scaleX = Mathf.Lerp(3f, wings[k, l, n].width, (wings[k, l, n].extendedFac + alcedo.tentacles[k].flyingMode) * 0.5f) / 9f * ((k % 2 == 0) ? 1f : (-1f)) * (IsKing ? 1.3f : 1f);
                            sLeaser.sprites[FeatherSprite(k, l, n)].scaleY = Vector2.Distance(Vector2.Lerp(wings[k, l, n].ConnectedLastPos, wings[k, l, n].ConnectedPos, timeStacker), Vector2.Lerp(wings[k, l, n].lastPos, wings[k, l, n].pos, timeStacker)) / 107f;
                        }
                        else if (IsMiros)
                        {
                            sLeaser.sprites[FeatherSprite(k, l, n)].scaleX = (float)((k % 2 == 0) ? 1 : (-1)) * Mathf.Pow(wings[k, l, n].extendedFac, 3f);
                            sLeaser.sprites[FeatherSprite(k, l, n)].scaleY = Mathf.Pow(wings[k, l, n].extendedFac, 3f);
                            sLeaser.sprites[FeatherSprite(k, l, n)].rotation += 200 * ((k % 2 == 0) ? 1 : (-1));
                        }
                        if (!IsMiros || n != feathersPerLayer - 1)
                        {
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].x = Mathf.Lerp(wings[k, l, n].ConnectedLastPos.x, wings[k, l, n].ConnectedPos.x, timeStacker) - camPos.x;
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].y = Mathf.Lerp(wings[k, l, n].ConnectedLastPos.y, wings[k, l, n].ConnectedPos.y, timeStacker) - camPos.y;
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].scaleY = Vector2.Distance(Vector2.Lerp(wings[k, l, n].ConnectedLastPos, wings[k, l, n].ConnectedPos, timeStacker), Vector2.Lerp(wings[k, l, n].lastPos, wings[k, l, n].pos, timeStacker)) / 107f;
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(wings[k, l, n].lastPos, wings[k, l, n].pos, timeStacker), Vector2.Lerp(wings[k, l, n].ConnectedLastPos, wings[k, l, n].ConnectedPos, timeStacker));
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].scaleX = Mathf.Lerp(3f, wings[k, l, n].width, (wings[k, l, n].extendedFac + alcedo.tentacles[k].flyingMode) * 0.5f) / 9f * ((k % 2 == 0) ? 1f : (-1f)) * (IsKing ? 1.3f : 1f);
                        }
                        if (!shadowMode)
                        {
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].color = Color.Lerp(wings[k, l, n].CurrentColor(), palette.blackColor, (ModManager.MMF && !IsMiros) ? darkness : 0f);
                        }
                        //暂时隐藏了羽毛
                        sLeaser.sprites[FeatherSprite(k, l, n)].isVisible = false;
                        sLeaser.sprites[FeatherColorSprite(k, l, n)].isVisible = false;
                    }
            }
            #endregion
            #region 后腿
            for (int k = 0; k < alcedo.legs.Length; k++)
            {
                Vector2 lastLegPos = Vector2.Lerp(alcedo.legs[k].connectedChunk.lastPos, alcedo.legs[k].connectedChunk.pos, timeStacker);
                float lastLegRad = 10f;
                for (int m = 0; m < alcedo.legs[k].tChunks.Length; m++)
                {
                    Vector2 tentaclePos = Vector2.Lerp(alcedo.legs[k].tChunks[m].lastPos, alcedo.legs[k].tChunks[m].pos, timeStacker);
                    Vector2 dir = (tentaclePos - lastLegPos).normalized;
                    Vector2 tentaclePerp = Custom.PerpendicularVector(dir);
                    float segLength = Vector2.Distance(tentaclePos, lastLegPos) / 5f;
                    float tentacleRad = alcedo.legs[k].TentacleContour(((float)m + 0.5f) / (float)alcedo.legs[k].tChunks.Length);
                    tentacleRad *= Mathf.Clamp(Mathf.Pow(alcedo.legs[k].tChunks[m].stretchedFac, 0.35f), 0.5f, 1.5f);
                    (sLeaser.sprites[LegSprite(k)] as TriangleMesh).MoveVertice(m * 4, lastLegPos - tentaclePerp * tentacleRad + dir * segLength - camPos);
                    (sLeaser.sprites[LegSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 1, lastLegPos + tentaclePerp * tentacleRad + dir * segLength - camPos);
                    tentacleRad = alcedo.legs[k].TentacleContour(((float)m + 1f) / (float)alcedo.legs[k].tChunks.Length);
                    tentacleRad *= Mathf.Clamp(Mathf.Pow(alcedo.legs[k].tChunks[m].stretchedFac, 0.35f), 0.5f, 1.5f);
                    (sLeaser.sprites[LegSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 2, tentaclePos - tentaclePerp * tentacleRad - dir * segLength - camPos);
                    (sLeaser.sprites[LegSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 3, tentaclePos + tentaclePerp * tentacleRad - dir * segLength - camPos);
                    //(sLeaser.sprites[LegSprite(k)] as TriangleMesh).color = Color.white;
                    lastLegRad = tentacleRad;
                    lastLegPos = tentaclePos;
                }
                int halfWingIndex = alcedo.legs[k].tChunks.Length - 1;
                Vector2 halfWingPos = Vector2.Lerp(alcedo.legs[k].tChunks[halfWingIndex].lastPos, alcedo.legs[k].tChunks[halfWingIndex].pos, timeStacker);
                Vector2 nextHalfWingPos = Vector2.Lerp(alcedo.legs[k].tChunks[halfWingIndex - 1].lastPos, alcedo.legs[k].tChunks[halfWingIndex - 1].pos, timeStacker);
                sLeaser.sprites[HindPawSprite(k)].x = halfWingPos.x - camPos.x;
                sLeaser.sprites[HindPawSprite(k)].y = halfWingPos.y - camPos.y;
                sLeaser.sprites[HindPawSprite(k)].rotation = Custom.AimFromOneVectorToAnother(halfWingPos, nextHalfWingPos) * Mathf.Lerp(0.9f, 1f, this.alcedo.legs[k].flyingMode)
                    - Mathf.Lerp(-10f, 60f, this.alcedo.legs[k].flyingMode) * (k % 2 == 0 ? 1f : -1f);
                sLeaser.sprites[HindPawColorSprite(k)].x = sLeaser.sprites[HindPawSprite(k)].x;
                sLeaser.sprites[HindPawColorSprite(k)].y = sLeaser.sprites[HindPawSprite(k)].y;
                sLeaser.sprites[HindPawColorSprite(k)].rotation = sLeaser.sprites[HindPawSprite(k)].rotation;
                sLeaser.sprites[HindPawSprite(k)].color = sLeaser.sprites[BodySprite(0)].color;
                sLeaser.sprites[HindPawColorSprite(k)].color = Color.Lerp(wings[k, 0, 0].CurrentClawColor(), palette.blackColor, (ModManager.MMF && !IsMiros) ? darkness : 0f);
            }
            #endregion
            if (!IsMiros)
            {
                for (int num13 = 0; num13 < 2; num13++)
                {
                    sLeaser.sprites[TuskSprite(num13)].scaleX = 0.8f * ((tuskRotations[num13] < 0f) ? 1f : (-1f));
                    sLeaser.sprites[TuskSprite(num13)].scaleY = 0.8f;
                    int num14 = Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(tuskRotations[num13])), 0, 2);
                    sLeaser.sprites[TuskSprite(num13)].element = Futile.atlasManager.GetElementWithName("KrakenTusk" + num14);
                    Vector2 p3 = Vector2.Lerp(tusks[num13].lastPos, tusks[num13].pos, timeStacker);
                    TuskConnectionPositions(num13, timeStacker, ref _cachedTuskConPos);
                    sLeaser.sprites[TuskSprite(num13)].rotation = Custom.AimFromOneVectorToAnother(p3, _cachedTuskConPos[0]);
                    sLeaser.sprites[TuskSprite(num13)].x = _cachedTuskConPos[0].x - camPos.x;
                    sLeaser.sprites[TuskSprite(num13)].y = _cachedTuskConPos[0].y - camPos.y;
                }
            }/*
            for (int num15 = 0; num15 < 2; num15++)
            {
                sLeaser.sprites[FrontShieldSprite(num15)].x = Mathf.Lerp(Mathf.Lerp(shells[num15 * 2, 2].x, shells[num15 * 2, 0].x, timeStacker), Mathf.Lerp(shells[num15 * 2 + 1, 2].x, shells[num15 * 2 + 1, 0].x, timeStacker), 0.1f) - camPos.x;
                sLeaser.sprites[FrontShieldSprite(num15)].y = Mathf.Lerp(Mathf.Lerp(shells[num15 * 2, 2].y, shells[num15 * 2, 0].y, timeStacker), Mathf.Lerp(shells[num15 * 2 + 1, 2].y, shells[num15 * 2 + 1, 0].y, timeStacker), 0.1f) - camPos.y;
                sLeaser.sprites[FrontShieldSprite(num15)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(shells[num15 * 2 + 1, 2], shells[num15 * 2 + 1, 0], timeStacker), Vector2.Lerp(shells[num15 * 2, 2], shells[num15 * 2, 0], timeStacker));
                sLeaser.sprites[FrontShieldSprite(num15)].scaleY = Mathf.Lerp(1f, 0.5f, Mathf.Lerp(Mathf.Lerp(shellModes[num15, 1], shellModes[num15, 0], timeStacker), alcedo.tentacles[num15].flyingMode, 0.35f)) * (IsKing ? 1.15f : 1f);
                sLeaser.sprites[BackShieldSprite(num15)].scaleY = Mathf.Lerp(1f, 1.2f, Mathf.Lerp(Mathf.Lerp(shellModes[num15, 1], shellModes[num15, 0], timeStacker), alcedo.tentacles[num15].flyingMode, 0.35f)) * (IsKing ? 1.15f : 1f);
                sLeaser.sprites[BackShieldSprite(num15)].x = Mathf.Lerp(Mathf.Lerp(shells[num15 * 2, 2].x, shells[num15 * 2, 0].x, timeStacker), Mathf.Lerp(shells[num15 * 2 + 1, 2].x, shells[num15 * 2 + 1, 0].x, timeStacker), 0.1f) - camPos.x;
                sLeaser.sprites[BackShieldSprite(num15)].y = Mathf.Lerp(Mathf.Lerp(shells[num15 * 2, 2].y, shells[num15 * 2, 0].y, timeStacker), Mathf.Lerp(shells[num15 * 2 + 1, 2].y, shells[num15 * 2 + 1, 0].y, timeStacker), 0.1f) - camPos.y;
                sLeaser.sprites[BackShieldSprite(num15)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(shells[num15 * 2 + 1, 2], shells[num15 * 2 + 1, 0], timeStacker), Vector2.Lerp(shells[num15 * 2, 2], shells[num15 * 2, 0], timeStacker));
            }*/
        }

        public Vector2 EyePos(float timeStacker)
        {
            Vector2 result = Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, (!alcedo.Snapping) ? timeStacker : Mathf.Lerp(-1.5f, 1.5f, UnityEngine.Random.value));
            if (alcedo.ChargingSnap)
            {
                result += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 4f;
            }
            return result;
        }

        public Color EyeColor()
        {
            if (alcedo.Blinded)
            {
                return Custom.HSL2RGB(UnityEngine.Random.value, 1f, 0.5f + 0.5f * UnityEngine.Random.value);
            }
            return eyeCol;
        }

        public override void SuckedIntoShortCut(Vector2 shortCutPosition)
        {
            for (int i = 0; i < this.tail.Length; i++)
            {
                this.tail[i].lastPos = this.tail[i].pos;
                this.tail[i].vel *= 0.5f;
                this.tail[i].pos = (this.tail[i].pos * 5f + shortCutPosition) / 6f;
            }
            for (int i = 0; i < this.alcedo.legs.Length; i++)
            {
                for (int j = 0; j < this.alcedo.legs[i].tChunks.Length; j++)
                {
                    this.alcedo.legs[i].tChunks[j].lastPos = this.alcedo.legs[i].tChunks[j].pos;
                    this.alcedo.legs[i].tChunks[j].vel *= 0.5f;
                    this.alcedo.legs[i].tChunks[j].pos = (this.alcedo.legs[i].tChunks[j].pos * 5f + shortCutPosition) / 6f;
                }
            }
            for (int i = 0; i < this.alcedo.tentacles.Length; i++)
            {
                for (int j = 0; j < this.alcedo.tentacles[i].tChunks.Length; j++)
                {
                    this.alcedo.tentacles[i].tChunks[j].lastPos = this.alcedo.tentacles[i].tChunks[j].pos;
                    this.alcedo.tentacles[i].tChunks[j].vel *= 0.5f;
                    this.alcedo.tentacles[i].tChunks[j].pos = (this.alcedo.tentacles[i].tChunks[j].pos * 5f + shortCutPosition) / 6f;
                }
            }
        }

        private void UpdateTailSegment(int i, float stiffness)
        {
            this.tail[i].Update();
            if (this.alcedo.room.PointSubmerged(this.tail[i].pos))
            {
                this.tail[i].vel *= 0.8f;
            }
            else
            {
                TailSegment tailSegment = this.tail[i];
                tailSegment.vel.y = tailSegment.vel.y - 0.9f * Mathf.Pow((float)i / (float)(this.tail.Length - 1), 3f);
            }
            if (!Custom.DistLess(this.tail[i].pos, base.owner.bodyChunks[5].pos, 15f * (float)(i + 1)))
            {
                this.tail[i].pos =base.owner.bodyChunks[5].pos + Custom.DirVec(base.owner.bodyChunks[5].pos, this.tail[i].pos) * 15f * (float)(i + 1) * (ModManager.MMF ? base.owner.room.gravity : 1f);
            }
            Vector2 vector = base.owner.bodyChunks[5].pos;
            if (i == 1)
            {
                vector = base.owner.bodyChunks[5].pos;
            }
            else if (i > 1)
            {
                vector = this.tail[i - 2].pos;
            }
            vector = Vector2.Lerp(vector, base.owner.bodyChunks[5].pos, 0.2f);
            this.tail[i].vel += Custom.DirVec(vector, this.tail[i].pos) * stiffness * Mathf.Pow(this.tailStiffnessDecline, (float)i) / Vector2.Distance(vector, this.tail[i].pos);
            if (i == 0)
            {
                this.tail[i].vel += this.tailDirection;
            }/*
            if (this.alcedo.Consious && this.alcedo.swim > 0.5f)
            {
                if (!(this.alcedo.Template.type == CreatureTemplate.Type.Salamander) && (!ModManager.MSC || !(this.alcedo.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)))
                {
                    this.tail[i].vel *= 1f - 0.5f * Mathf.InverseLerp(0.5f, 1f, this.alcedo.swim);
                    this.tail[i].vel += (Custom.DirVec(base.owner.bodyChunks[1].pos, base.owner.bodyChunks[5].pos) + Custom.PerpendicularVector((this.tail[i].pos - vector).normalized) * Mathf.Sin((this.breath * 5f - (float)i / (float)this.tail.Length) * 0.3f * 3.1415927f)) * Mathf.InverseLerp(0.5f, 1f, this.alcedo.swim) * Mathf.Pow(0.85f, (float)i) * 7f;
                    return;
                }
                if (true)//!this.alcedo.salamanderLurk
                {
                    this.tail[i].vel += (Custom.DirVec(base.owner.bodyChunks[1].pos, base.owner.bodyChunks[5].pos) * 0.2f + Custom.PerpendicularVector((this.tail[i].pos - vector).normalized) * Mathf.Sin((this.breath * 7f - (float)i / (float)this.tail.Length) * 1.5f * 3.1415927f)) * Mathf.InverseLerp(0.5f, 1f, this.alcedo.swim) * Mathf.Pow(0.85f, (float)i) * 4f;
                    return;
                }
            }*/
            else if (this.showDominance > 0f)
            {
                this.tail[i].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * this.showDominance * UnityEngine.Random.value * 2f;
            }
        }
    }
}
