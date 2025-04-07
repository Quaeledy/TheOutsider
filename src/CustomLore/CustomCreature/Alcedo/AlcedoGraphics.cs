using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using TheOutsider.CustomLore.CustomCosmetics;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature.Alcedo
{
    sealed class AlcedoGraphics : GraphicsModule
    {
        public class WingColorWave
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
        public bool albino;
        public float darkness;
        private List<WingColorWave> colorWaves;
        private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;
        public float headFlip;
        public float lastHeadFlip;
        public float eyeSize;
        public Color eyeCol;
        public float beakFatness;
        public Color laserColor;
        public Color lastLaserColor;
        public ChunkDynamicSoundLoop soundLoop;
        public float depthRotation;
        public float lastDepthRotation;
        public float headDepthRotation;
        public float lastHeadDepthRotation;
        public int ExtraSpritesLength;
        public List<AlcedoScaleTemplate> cosmetics;
        public AlcedoLimb[] limbs;
        public int legsGrabbing;
        public int frontLegsGrabbing;
        public int hindLegsGrabbing;
        public int noGripCounter;
        private float lastBreath;
        private float breath;
        private bool rotateWhileStunned;

        public Alcedo alcedo => owner as Alcedo;

        public bool IsKing => alcedo.IsKing;
        public bool IsMiros => alcedo.IsMiros;
        public override bool ShouldBeCulled
        {
            get
            {
                if (base.ShouldBeCulled)
                {
                    if (owner.room != null)
                    {
                        return owner.firstChunk.pos.y < owner.room.PixelHeight;
                    }
                    return true;
                }
                return false;
            }
        }
        public BodyChunk head => alcedo.bodyChunks[4];

        #region 绘制图形的长度
        public float SpineLength => NeckLength + ForeBodyLength + WaistLength + HindBodyLength + TailLength;
        public float BodyLength => WaistLength;// ForeBodyLength + WaistLength + HindBodyLength;
        public float NeckLength => alcedo.neck.idealLength;
        public float ForeBodyLength
        {
            get
            {
                float bodyLength = 2f * (alcedo.bodyChunks[0].rad + alcedo.bodyChunks[1].rad) * 0.5f; ;
                return bodyLength;
            }
        }
        public float HindBodyLength
        {
            get
            {
                float bodyLength = 2f * (alcedo.bodyChunks[5].rad + alcedo.bodyChunks[6].rad) * 0.5f; ;
                return bodyLength;
            }
        }
        public float WaistLength => alcedo.waist.idealLength;
        public float TailLength
        {
            get
            {
                float tailLength = 0f;
                for (int i = 0; i < tail.Length; i++)
                {
                    tailLength += tail[i].connectionRad;
                }
                return tailLength;
            }
        }
        #endregion

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
        private int FeatherSpriteLength => featherLayersPerWing * feathersPerLayer * 2 * alcedo.tentacles.Length;

        private int ForePawSpriteStart => FeatherSpriteStart + FeatherSpriteLength;
        private int ForePawSpriteLength => alcedo.tentacles.Length * 2;

        private int BodySpriteStart => ForePawSpriteStart + ForePawSpriteLength;
        private int BodySpriteLength => 3;

        public int NeckSpriteStart => BodySpriteStart + BodySpriteLength;
        public int NeckSpriteLength => 1;

        public int HeadSpriteStart => NeckSpriteStart + NeckSpriteLength;
        public int HeadSpriteLength => 1;

        private int EyesSpriteStart => HeadSpriteStart + HeadSpriteLength;
        public int EyesSpriteLength => 1;

        public int MaskSpriteStart => EyesSpriteStart + EyesSpriteLength;
        public int MaskSpriteLength => 1;

        private int ExtraSpritesStart => MaskSpriteStart + 1;

        private int TotalSprites => ExtraSpritesStart + ExtraSpritesLength;
        #endregion

        #region 贴图序号
        private int LegSprite(int i)
        {
            int sprite = LegSpriteStart + i;
            if (sprite >= LegSpriteStart + LegSpriteLength)
            {
                Plugin.Log("LegSprite index is out of range!");
                return -1;
            }
            return sprite;
        }

        private int HindPawSprite(int i)
        {
            int sprite = HindPawSpriteStart + i * 2;
            if (sprite >= HindPawSpriteStart + HindPawSpriteLength)
            {
                Plugin.Log("HindPawSprite index is out of range!");
                return -1;
            }
            return sprite;
        }
        private int HindPawColorSprite(int i)
        {
            int sprite = HindPawSpriteStart + i * 2 + 1;
            if (sprite >= HindPawSpriteStart + HindPawSpriteLength)
            {
                Plugin.Log("HindPawColorSprite index is out of range!");
                return -1;
            }
            return sprite;
        }

        private int TentacleSprite(int i)
        {
            int sprite = TentacleSpriteStart + i;
            if (sprite >= TentacleSpriteStart + TentacleSpriteLength)
            {
                Plugin.Log("TentacleSprite index is out of range!");
                return -1;
            }
            return sprite;
        }

        private int FeatherSprite(int wing, int layer, int index)
        {
            int sprite = FeatherSpriteStart + featherLayersPerWing * feathersPerLayer * wing * 2 + feathersPerLayer * layer * 2 + index * 2;
            if (sprite >= FeatherSpriteStart + FeatherSpriteLength)
            {
                Plugin.Log("FeatherSprite index is out of range!");
                return -1;
            }
            return sprite;
        }
        private int FeatherColorSprite(int wing, int layer, int index)
        {
            int sprite = FeatherSpriteStart + featherLayersPerWing * feathersPerLayer * wing * 2 + feathersPerLayer * layer * 2 + index * 2 + 1;
            if (sprite >= FeatherSpriteStart + FeatherSpriteLength)
            {
                Plugin.Log("FeatherColorSprite index is out of range!");
                return -1;
            }
            return sprite;
        }

        private int ForePawSprite(int i)
        {
            int sprite = ForePawSpriteStart + i * 2;
            if (sprite >= ForePawSpriteStart + ForePawSpriteLength)
            {
                Plugin.Log("ForePawSprite index is out of range!");
                return -1;
            }
            return sprite;
        }
        private int ForePawColorSprite(int i)
        {
            int sprite = ForePawSpriteStart + i * 2 + 1;
            if (sprite >= ForePawSpriteStart + ForePawSpriteLength)
            {
                Plugin.Log("ForePawColorSprite index is out of range!");
                return -1;
            }
            return sprite;
        }

        private int BodySprite(int i)
        {
            int sprite = BodySpriteStart + i;
            if (sprite >= BodySpriteStart + BodySpriteLength)
            {
                Plugin.Log("BodySprite index is out of range!");
                return -1;
            }
            return sprite;
        }
        #endregion

        public void MakeColorWave(int delay)
        {
            colorWaves.Add(new WingColorWave(delay, 1f / Mathf.Lerp(10f, 30f, UnityEngine.Random.value), 1f - UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value, 1f - UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value, 1f - UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value));
        }

        public AlcedoGraphics(Alcedo ow)
            : base(ow, internalContainers: false)
        {
            cosmetics = new List<AlcedoScaleTemplate>();
            this.limbs = new AlcedoLimb[4];
            for (int i = 0; i < this.limbs.Length; i++)
            {
                int connectChunk = (i < 2) ? 0 : 5;
                this.limbs[i] = new AlcedoLimb(this, base.owner.bodyChunks[connectChunk], i, 2.5f, 0.7f, 0.99f, this.alcedo.limbSpeed, this.alcedo.limbQuickness, (i % 2 == 1) ? this.limbs[i - 1] : null);
                //list.Add(this.limbs[i]);
            }
            cullRange = 1400f;
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(alcedo.abstractCreature.ID.RandomSeed);
            ColorA = new HSLColor(Mathf.Lerp(120f / 360f, 170f / 360f, Mathf.Lerp(UnityEngine.Random.value, 1f, this.alcedo.bodySizeFac - 0.5f)),
                Mathf.Lerp(0.5f, 0.7f, UnityEngine.Random.value),
                Mathf.Lerp(0.7f, 0.8f, UnityEngine.Random.value));
            ColorB = new HSLColor(ColorA.hue + Mathf.Lerp(Mathf.Lerp(-0.05f, 0.25f, UnityEngine.Random.value), 0.25f, this.alcedo.bodySizeFac - 0.5f),
                Mathf.Lerp(0.8f, 1f, 1f - UnityEngine.Random.value * UnityEngine.Random.value),
                Mathf.Lerp(0.45f, 1f, UnityEngine.Random.value * UnityEngine.Random.value));
            featherLayersPerWing = 3;
            feathersPerLayer = UnityEngine.Random.Range(14, 18);
            colorWaves = new List<WingColorWave>();
            wings = new AlcedoFeather[alcedo.tentacles.Length, featherLayersPerWing, feathersPerLayer];//翅膀数，每只翅膀羽毛层数，每层羽毛的羽毛数
            for (int j = 0; j < alcedo.tentacles.Length; j++)
            {
                for (int k = 0; k < feathersPerLayer; k++)
                {
                    float num5 = (k + 0.5f) / feathersPerLayer;
                    float value = Mathf.Lerp(1f - Mathf.Pow(IsMiros ? 0.95f : 0.89f, k), Mathf.Sqrt(num5), 0.5f);
                    value = num5;
                    value = Mathf.InverseLerp(0.1f, 1.1f, value);
                    if (IsMiros && k == feathersPerLayer - 1)
                    {
                        value = 0.8f;
                    }
                    //飞羽
                    wings[j, 0, k] = new AlcedoFeather(this, alcedo.tentacles[j], value,
                        alcedo.bodySizeFac * 1.5f * AlcedoFeather.FeatherContour(num5, 0f) * Mathf.Lerp(75f, 80f, UnityEngine.Random.value),
                        alcedo.bodySizeFac * 1.5f * AlcedoFeather.FeatherContour(num5, 1f) * Mathf.Lerp(80f, 85f, UnityEngine.Random.value) * (IsKing ? 1.3f : 1f),
                        Mathf.Lerp(5f, 8f, AlcedoFeather.FeatherWidth(num5)), AlcedoFeather.Type.FlightFeather);
                    //覆羽
                    wings[j, 1, k] = new AlcedoFeather(this, alcedo.tentacles[j], value + 0.05f * UnityEngine.Random.value,
                        alcedo.bodySizeFac * 1.5f * AlcedoFeather.FeatherContour(num5, 0f) * Mathf.Pow(Mathf.Lerp(1f, 0.25f, (float)(k + 0.5f) / (float)feathersPerLayer), 0.2f) * Mathf.Lerp(35f, 40f, UnityEngine.Random.value),
                        alcedo.bodySizeFac * 1.5f * AlcedoFeather.FeatherContour(num5, 1f) * Mathf.Pow(Mathf.Lerp(1f, 0.25f, (float)(k + 0.5f) / (float)feathersPerLayer), 0.2f) * Mathf.Lerp(40f, 45f, UnityEngine.Random.value) * (IsKing ? 1.3f : 1f),
                        Mathf.Lerp(5f, 8f, AlcedoFeather.FeatherWidth(num5)), AlcedoFeather.Type.Covert);
                    wings[j, 2, k] = new AlcedoFeather(this, alcedo.tentacles[j], value + 0.05f * UnityEngine.Random.value,
                        alcedo.bodySizeFac * 1.5f * AlcedoFeather.FeatherContour(num5, 0f) * Mathf.Pow(Mathf.Lerp(1f, 0.25f, (float)(k + 0.5f) / (float)feathersPerLayer), 0.2f) * Mathf.Lerp(22f, 25f, UnityEngine.Random.value),
                        alcedo.bodySizeFac * 1.5f * AlcedoFeather.FeatherContour(num5, 1f) * Mathf.Pow(Mathf.Lerp(1f, 0.25f, (float)(k + 0.5f) / (float)feathersPerLayer), 0.2f) * Mathf.Lerp(25f, 28f, UnityEngine.Random.value) * (IsKing ? 1.3f : 1f),
                        Mathf.Lerp(5f, 8f, AlcedoFeather.FeatherWidth(num5)), AlcedoFeather.Type.Covert);
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
            //尾巴
            tailSegments = 16;//16
            tailLength = 8f;//160f
            tailStiffness = 200f;
            tailStiffnessDecline = 0.2f;
            tailDirection = Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value;
            tail = new TailSegment[tailSegments];
            for (int j = 0; j < tailSegments; j++)
            {
                float t = (tailSegments - j) / (float)tailSegments;
                tail[j] = new TailSegment(this,
                    8f * t,
                    ((j > 0 ? 1f : 2f) + 2f * t) / 2f * tailLength,//(((j > 0) ? 8f : 16f) + 8f * t) / 2f * this.tailLength,
                    j > 0 ? tail[j - 1] : null,
                    0.85f, 1f, 0.4f, false);
                //bodyParts.Add(this.tail[j]);
            }
            //装饰
            int scaleStartIndex = ExtraSpritesStart;
            scaleStartIndex = AddCosmetic(scaleStartIndex, new AlcedoLongHeadScales(this, scaleStartIndex));
            scaleStartIndex = AddCosmetic(scaleStartIndex, new AlcedoSpineTeardrop(this, scaleStartIndex));
            //
        }

        public override void Reset()
        {
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
            for (int j = 0; j < tail.Length; j++)
            {
                tail[j].Reset(owner.bodyChunks[1].pos);
            }
            for (int l = 0; l < cosmetics.Count; l++)
            {
                cosmetics[l].Reset();
            }
            base.Reset();
        }

        public override void Update()
        {
            base.Update();
            if (!culled)
            {
                for (int j = 0; j < cosmetics.Count; j++)
                {
                    cosmetics[j].Update();
                }
            }
            #region 尾巴
            /*
            if (this.alcedo.animation == Lizard.Animation.Lounge)
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
                    if (this.alcedo.bodyChunks[i].ContactPoint.x != 0 || this.alcedo.room.GetTile(this.alcedo.bodyChunks[i].pos - new Vector2(this.BodyChunkDisplayRad(i) + 6f, 0f)).Solid || this.alcedo.room.GetTile(this.alcedo.bodyChunks[i].pos + new Vector2(this.BodyChunkDisplayRad(i) + 6f, 0f)).Solid)
                    {
                        num += ((this.alcedo.bodyChunks[i].ContactPoint.x != 0) ? 1f : 0.5f) / 3f;
                        num2 += Mathf.Abs(this.alcedo.bodyChunks[i].vel.y) / 3f;
                    }
                    if (this.alcedo.bodyChunks[i].ContactPoint.y != 0 || this.alcedo.room.GetTile(this.alcedo.bodyChunks[i].pos - new Vector2(0f, this.BodyChunkDisplayRad(i) + 6f)).Solid || this.alcedo.room.GetTile(this.alcedo.bodyChunks[i].pos + new Vector2(0f, this.BodyChunkDisplayRad(i) + 6f)).Solid)
                    {
                        num += ((this.alcedo.bodyChunks[i].ContactPoint.y != 0) ? 1f : 0.5f) / 3f;
                        num3 += Mathf.Abs(this.alcedo.bodyChunks[i].vel.x) / 3f;
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
            if (alcedo.Consious && UnityEngine.Random.value < 0.05f)
            {
                tailDirection = Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value;
            }
            showDominance = Mathf.Clamp(showDominance - 1f / Mathf.Lerp(60f, 120f, UnityEngine.Random.value), 0f, 1f);
            tail[0].connectedPoint = new Vector2?(owner.bodyChunks[5].pos);
            float stiffness = Mathf.Lerp(tailStiffness, 900f, showDominance);
            if (alcedo.Consious && alcedo.swim > 0.5f)// && !this.alcedo.salamanderLurk
            {
                for (int num12 = 0; num12 < tail.Length; num12++)
                {
                    UpdateTailSegment(num12, stiffness);
                }/*
                if ((this.alcedo.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && this.alcedo.Template.type == DLCSharedEnums.CreatureTemplateType.EelLizard)) && !this.alcedo.salamanderLurk)
                {
                    Vector2 a2 = Custom.PerpendicularVector(Custom.DirVec(this.alcedo.bodyChunks[2].pos, this.alcedo.bodyChunks[0].pos));
                    this.alcedo.bodyChunks[0].pos += a2 * Mathf.Sin(this.breath * 3.1415927f * 2f * 3f) * 3.5f * this.alcedo.swim;
                    base.owner.bodyChunks[1].pos += a2 * Mathf.Sin((this.breath + 0.4f) * 3.1415927f * 2f * 3f) * 3f * this.alcedo.swim;
                    base.owner.bodyChunks[5].pos += a2 * Mathf.Sin((this.breath + 0.8f) * 3.1415927f * 2f * 3f) * 2f * this.alcedo.swim;
                }*/
            }
            else
            {
                for (int num13 = tail.Length - 1; num13 >= 0; num13--)
                {
                    UpdateTailSegment(num13, stiffness);
                }
            }
            #endregion
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
                    headGraphic += num6 >= headGraphic ? 1 : -1;
                }
            }
            else
            {
                changeHeadGraphicCounter = 0;
            }
            Vector2 vector = Custom.DirVec(alcedo.bodyChunks[0].pos, alcedo.bodyChunks[1].pos);
            Vector2 vector2 = Custom.PerpendicularVector(vector);
            for (int num8 = 0; num8 < 4; num8++)
            {
                float num9 = num8 <= 1 ? 1f : -1f;
                int num10 = num8 > 1 ? 1 : 0;
                int num11 = num8 % 2 != 0 ? 1 : 0;
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
            b /= alcedo.bodyChunks.Length;
            if (!shadowMode && alcedo.mainBodyChunk.pos.y > alcedo.room.PixelHeight + 400f)
            {
                shadowMode = true;
            }
            else if (shadowMode && alcedo.mainBodyChunk.pos.y < alcedo.room.PixelHeight + 200f)
            {
                shadowMode = false;
            }
            #region 装饰
            float nowDepthRotation = 0f;
            this.legsGrabbing = 0;
            this.frontLegsGrabbing = 0;
            this.hindLegsGrabbing = 0;
            bool hasLimbsGrip = true;
            for (int legIndex = 0; legIndex < this.limbs.Length; legIndex++)
            {
                if (this.alcedo.Consious && this.alcedo.swim > 0.5f)
                {
                    if (this.alcedo.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && this.alcedo.Template.type == DLCSharedEnums.CreatureTemplateType.EelLizard))
                    {/*
                        if (!this.alcedo.salamanderLurk)
                        {
                            Vector2 vector3 = Custom.DirVec(this.drawPositions[2, 0], this.alcedo.bodyChunks[0].pos);
                            Vector2 a = Custom.PerpendicularVector(Custom.DirVec(this.drawPositions[2, 0], this.alcedo.bodyChunks[0].pos));
                            float num9 = (float)legIndex / (float)this.limbs.Length + this.breath * 1.5f;
                            this.limbs[legIndex].vel += vector3 * Mathf.Sin(num9 * 3.1415927f * 2f) * 3f;
                            if (Mathf.Sin(num9 * 3.1415927f * 2f) > 0f)
                            {
                                this.limbs[legIndex].vel *= 0.7f;
                                this.limbs[legIndex].vel += (Custom.ClosestPointOnLine(this.alcedo.bodyChunks[5].pos - vector3, this.alcedo.bodyChunks[5].pos + vector3, this.limbs[legIndex].pos) + a * ((legIndex % 2 == 0) ? -5f : 5f) - this.limbs[legIndex].pos) / 5f;
                            }
                            else
                            {
                                this.limbs[legIndex].vel += a * (0.5f + 0.5f * Mathf.Cos(num9 * 3.1415927f * 2f)) * 1.5f * ((legIndex % 2 == 0) ? -1f : 1f);
                            }
                        }*/
                    }
                    else
                    {
                        AlcedoLimb alcedoLimb = this.limbs[legIndex];
                        alcedoLimb.vel.x = alcedoLimb.vel.x + Mathf.Sin(((float)legIndex / (float)this.limbs.Length * (this.alcedo.smoothenLegMovement ? 1f : 0.1f) + this.breath * 2f) * 3.1415927f * 2f) * this.alcedo.swim * 0.5f;
                    }
                }
                this.limbs[legIndex].Update();
                if (this.limbs[legIndex].gripCounter >= this.alcedo.limbGripDelay)
                {
                    this.legsGrabbing++;
                    if (legIndex < 2)
                    {
                        this.frontLegsGrabbing++;
                    }
                    else if (legIndex >= this.limbs.Length - 2)
                    {
                        this.hindLegsGrabbing++;
                    }
                }
                if (this.limbs[legIndex].gripCounter > 0)
                {
                    hasLimbsGrip = false;
                }
                float legRotation = Custom.DistanceToLine(this.limbs[legIndex].pos, this.limbs[legIndex].connection.pos, this.limbs[legIndex].connection.rotationChunk.pos);
                legRotation *= ((legIndex > 1) ? 1f : -1f);
                if (Mathf.Abs(legRotation) > 10f)
                {
                    nowDepthRotation += Mathf.Sign(legRotation);
                }
                this.limbs[legIndex].flip = Mathf.Lerp(this.limbs[legIndex].flip, (legRotation < 0f) ? 1f : -1f, 0.3f);
            }
            nowDepthRotation = Mathf.Clamp(nowDepthRotation, -1f, 1f);
            if (this.alcedo.Consious)
            {
                nowDepthRotation = (this.alcedo.bodyChunks[0].pos.x > this.alcedo.bodyChunks[5].pos.x) ? -1f : 1f;
                nowDepthRotation *= Mathf.Abs(Mathf.Sin(Custom.VecToDeg(this.alcedo.bodyChunks[0].pos - this.alcedo.bodyChunks[5].pos) / 180f * Mathf.PI));
                //nowDepthRotation = Mathf.Lerp(nowDepthRotation, (this.alcedo.bodyChunks[0].pos.x > this.alcedo.bodyChunks[5].pos.x) ? -1f : 1f, this.alcedo.swim);
            }
            this.lastDepthRotation = this.depthRotation;
            if (!this.alcedo.Stunned || this.rotateWhileStunned)
            {
                this.depthRotation = Mathf.Lerp(this.depthRotation, nowDepthRotation, 0.1f);
            }
            this.lastHeadDepthRotation = this.headDepthRotation;
            float headDepthRotationFac = Vector2.Dot(Custom.DegToVec(HeadRotation(0f)), (this.head.pos - this.alcedo.bodyChunks[0].pos).normalized);
            headDepthRotationFac = Mathf.InverseLerp(0f, 0.6f, Mathf.Abs(headDepthRotationFac));
            this.headDepthRotation = HeadRotation(0f);//Mathf.Lerp(this.headDepthRotation, this.depthRotation * headDepthRotationFac, 0.5f);
            #endregion
        }

        #region 绘图
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[TotalSprites];/*
            if (alcedo.kingTusks != null)
            {
                alcedo.kingTusks.InitiateSprites(this, sLeaser, rCam);
            }*/
            #region 尾巴
            TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[(tail.Length - 1) * 4 + 1];
            TriangleMesh triangleMesh = new TriangleMesh("Futile_White", array, false, false);
            for (int num8 = 0; num8 < tail.Length - 1; num8++)
            {
                int num9 = num8 * 4;
                for (int num10 = 0; num10 < 4; num10++)
                {
                    array[num9 + num10] = new TriangleMesh.Triangle(num9 + num10, num9 + num10 + 1, num9 + num10 + 2);
                }
            }
            array[(tail.Length - 1) * 4] = new TriangleMesh.Triangle((tail.Length - 1) * 4, (tail.Length - 1) * 4 + 1, (tail.Length - 1) * 4 + 2);
            triangleMesh = new TriangleMesh("Futile_White", array, false, false);
            sLeaser.sprites[TailSpriteStart] = triangleMesh;
            #endregion
            #region 后腿
            for (int i = 0; i < alcedo.legs.Length; i++)
            {
                sLeaser.sprites[LegSprite(i)] = TriangleMesh.MakeLongMesh(alcedo.legs[i].tChunks.Length, pointyTip: false, customColor: false);
                //前爪
                //后爪
                sLeaser.sprites[HindPawSprite(i)] = new FSprite("JetFishFlipper1");
                sLeaser.sprites[HindPawColorSprite(i)] = new FSprite("AlcedoClawColorA");
                //前爪
                //后爪
                if (i == 1)
                {
                    sLeaser.sprites[HindPawSprite(i)].scaleX = IsKing ? -1.15f : -1f;
                    sLeaser.sprites[HindPawColorSprite(i)].scaleX = IsKing ? -1.15f : -1f;
                }
                else
                {
                    sLeaser.sprites[HindPawSprite(i)].scaleX = IsKing ? 1.15f : 1f;
                    sLeaser.sprites[HindPawColorSprite(i)].scaleX = IsKing ? 1.15f : 1f;
                }
                sLeaser.sprites[HindPawSprite(i)].anchorX = 1f;
                sLeaser.sprites[HindPawColorSprite(i)].anchorX = 1f;
            }
            #endregion
            #region 翅膀
            for (int j = 0; j < alcedo.tentacles.Length; j++)
            {
                sLeaser.sprites[TentacleSprite(j)] = TriangleMesh.MakeLongMesh(alcedo.tentacles[j].tChunks.Length, pointyTip: false, customColor: false);
                sLeaser.sprites[ForePawSprite(j)] = new FSprite("JetFishFlipper1");
                sLeaser.sprites[ForePawColorSprite(j)] = new FSprite("AlcedoClawColorA");
                if (j % 2 == 1)
                {
                    sLeaser.sprites[ForePawSprite(j)].scaleX = IsKing ? -1.15f : -1f;
                    sLeaser.sprites[ForePawColorSprite(j)].scaleX = IsKing ? -1.15f : -1f;
                }
                else
                {
                    sLeaser.sprites[ForePawSprite(j)].scaleX = IsKing ? 1.15f : 1f;
                    sLeaser.sprites[ForePawColorSprite(j)].scaleX = IsKing ? 1.15f : 1f;
                }
                sLeaser.sprites[ForePawSprite(j)].anchorX = 1f;
                sLeaser.sprites[ForePawColorSprite(j)].anchorX = 1f;
            }
            for (int k = 0; k < alcedo.tentacles.Length; k++)
            {
                for (int x = 0; x < featherLayersPerWing; x++)
                    for (int l = 0; l < feathersPerLayer; l++)
                    {
                        FSprite sp1 = new FSprite("AlcedoFeatherA");
                        FSprite sp2 = new FSprite("AlcedoFeatherColorA");
                        if (wings[k, x, l].type == AlcedoFeather.Type.Covert)
                        {
                            sp1 = new FSprite("AlcedoFeatherB");
                            sp2 = new FSprite("AlcedoFeatherColorB");
                        }
                        else if (wings[k, x, l].type == AlcedoFeather.Type.Covert)
                        {
                            sp1 = new FSprite("AlcedoFeatherA");
                            sp2 = new FSprite("AlcedoFeatherColorA");
                        }
                        sLeaser.sprites[FeatherSprite(k, x, l)] = sp1;
                        sLeaser.sprites[FeatherSprite(k, x, l)].anchorY = IsMiros ? 0.94f : 0.97f;
                        sLeaser.sprites[FeatherColorSprite(k, x, l)] = sp2;
                        sLeaser.sprites[FeatherColorSprite(k, x, l)].anchorY = IsMiros ? 0.94f : 0.97f;
                    }
            }
            #endregion
            #region 身体
            //上半身
            sLeaser.sprites[BodySprite(0)] = new FSprite("KrakenBody");
            sLeaser.sprites[BodySprite(0)].scale = 1.2f * 0.8f * alcedo.bodySizeFac;
            sLeaser.sprites[BodySprite(0)].scaleX = sLeaser.sprites[BodySprite(0)].scaleX * 0.6f;
            //下半身
            sLeaser.sprites[BodySprite(1)] = new FSprite("KrakenBody");
            sLeaser.sprites[BodySprite(1)].scale = 1.2f * 0.75f * alcedo.bodySizeFac;
            sLeaser.sprites[BodySprite(1)].scaleX = sLeaser.sprites[BodySprite(1)].scaleX * 0.5f;
            sLeaser.sprites[BodySprite(1)].anchorY = 0.4f;
            //腰部
            sLeaser.sprites[BodySprite(2)] = TriangleMesh.MakeLongMesh(alcedo.waist.tChunks.Length, pointyTip: false, customColor: true);
            #endregion
            #region 头颈
            //脖子
            sLeaser.sprites[NeckSpriteStart] = TriangleMesh.MakeLongMesh(alcedo.neck.tChunks.Length, pointyTip: false, customColor: true);
            //头
            sLeaser.sprites[HeadSpriteStart] = new FSprite("AlcedoHeadA0");
            sLeaser.sprites[HeadSpriteStart].anchorX = 0.5f;
            sLeaser.sprites[HeadSpriteStart].anchorY = 0.5f;
            sLeaser.sprites[EyesSpriteStart] = new FSprite(IsMiros ? "Circle20" : "AlcedoEyesA0");
            sLeaser.sprites[EyesSpriteStart].anchorX = 0.5f;
            sLeaser.sprites[EyesSpriteStart].anchorY = 0.5f;
            sLeaser.sprites[MaskSpriteStart] = new FSprite("AlcedoMaskA0");
            sLeaser.sprites[MaskSpriteStart].anchorX = 0.5f;
            sLeaser.sprites[MaskSpriteStart].anchorY = 0.5f;
            #endregion
            for (int l = 0; l < cosmetics.Count; l++)
            {
                cosmetics[l].InitiateSprites(sLeaser, rCam);
            }
            AddToContainer(sLeaser, rCam, null);
            base.InitiateSprites(sLeaser, rCam);
            Plugin.Log("TailSpriteStart: " + TailSpriteStart);
            Plugin.Log("LegSpriteStart: " + LegSpriteStart);
            Plugin.Log("HindPawSpriteStart: " + HindPawSpriteStart);
            Plugin.Log("TentacleSpriteStart: " + TentacleSpriteStart);
            Plugin.Log("FeatherSpriteStart: " + FeatherSpriteStart);
            Plugin.Log("ForePawSpriteStart: " + ForePawSpriteStart);
            Plugin.Log("BodySpriteStart: " + BodySpriteStart);
            Plugin.Log("NeckSpriteStart: " + NeckSpriteStart);
            Plugin.Log("HeadSpriteStart: " + HeadSpriteStart);
            Plugin.Log("EyesSpriteStart: " + EyesSpriteStart);
            Plugin.Log("MaskSpriteStart: " + MaskSpriteStart);
            Plugin.Log("ExtraSpritesStart: " + ExtraSpritesStart);
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
            sLeaser.RemoveAllSpritesFromContainer();
            for (int j = 0; j < cosmetics.Count; j++)
            {
                if (cosmetics[j].spritesOverlap == AlcedoScaleTemplate.SpritesOverlap.Behind ||
                    cosmetics[j].spritesOverlap == AlcedoScaleTemplate.SpritesOverlap.BehindHead ||
                    cosmetics[j].spritesOverlap == AlcedoScaleTemplate.SpritesOverlap.Discretion)
                {
                    cosmetics[j].AddToContainer(sLeaser, rCam, newContatiner);
                }
            }
            for (int k = 0; k < ExtraSpritesStart; k++)
            {
                newContatiner.AddChild(sLeaser.sprites[k]);
            }
            for (int num4 = 0; num4 < cosmetics.Count; num4++)
            {
                if (cosmetics[num4].spritesOverlap == AlcedoScaleTemplate.SpritesOverlap.InFront)
                {
                    cosmetics[num4].AddToContainer(sLeaser, rCam, newContatiner);
                }
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            this.palette = palette;
            ExitShadowMode(sLeaser, rCam, changeContainer: false);
            for (int i = 0; i < cosmetics.Count; i++)
            {
                cosmetics[i].ApplyPalette(sLeaser, rCam, palette);
            }
            base.ApplyPalette(sLeaser, rCam, palette);
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
            for (int j = 0; j < cosmetics.Count; j++)
            {
                cosmetics[j].DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }
            Vector2 backPos = Vector2.Lerp(owner.bodyChunks[1].lastPos, owner.bodyChunks[1].pos, timeStacker);
            Vector2 chestPos = Vector2.Lerp(owner.bodyChunks[0].lastPos, owner.bodyChunks[0].pos, timeStacker);
            Vector2 bellyPos = Vector2.Lerp(owner.bodyChunks[5].lastPos, owner.bodyChunks[5].pos, timeStacker);
            Vector2 hipPos = Vector2.Lerp(owner.bodyChunks[6].lastPos, owner.bodyChunks[6].pos, timeStacker);
            #region 尾巴
            Vector2 lastTailPos = Vector2.Lerp(bellyPos, hipPos, 0.5f);
            float width = owner.bodyChunks[5].rad * 0.7f;
            for (int num11 = 0; num11 < tail.Length; num11++)
            {
                Vector2 tailPos = Vector2.Lerp(tail[num11].lastPos, tail[num11].pos, timeStacker);
                Vector2 tailDir = (tailPos - lastTailPos).normalized;
                Vector2 tailPerp = Custom.PerpendicularVector(tailDir);
                float d2 = Vector2.Distance(tailPos, lastTailPos) / 5f;
                (sLeaser.sprites[TailSpriteStart] as TriangleMesh).MoveVertice(num11 * 4 + 0, lastTailPos - tailPerp * (width + tail[num11].StretchedRad) * 0.5f + tailDir * d2 - camPos);
                (sLeaser.sprites[TailSpriteStart] as TriangleMesh).MoveVertice(num11 * 4 + 1, lastTailPos + tailPerp * (width + tail[num11].StretchedRad) * 0.5f + tailDir * d2 - camPos);
                if (num11 < tail.Length - 1)
                {
                    (sLeaser.sprites[TailSpriteStart] as TriangleMesh).MoveVertice(num11 * 4 + 2, tailPos - tailPerp * tail[num11].StretchedRad - tailDir * d2 - camPos);
                    (sLeaser.sprites[TailSpriteStart] as TriangleMesh).MoveVertice(num11 * 4 + 3, tailPos + tailPerp * tail[num11].StretchedRad - tailDir * d2 - camPos);
                }
                else
                {
                    (sLeaser.sprites[TailSpriteStart] as TriangleMesh).MoveVertice(num11 * 4 + 2, tailPos - camPos);
                }
                width = tail[num11].StretchedRad;
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
            #region 头部
            Vector2 headPos = Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, alcedo.Snapping ? Mathf.Lerp(-1.5f, 1.5f, UnityEngine.Random.value) : timeStacker);
            if (alcedo.ChargingSnap)
            {
                headPos += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 4f;
            }
            float neckToHeadAngle = Custom.AimFromOneVectorToAnother(Vector2.Lerp(alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].lastPos,
                                                                                  alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].pos, timeStacker),
                Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker));
            sLeaser.sprites[HeadSpriteStart].x = headPos.x - camPos.x;
            sLeaser.sprites[HeadSpriteStart].y = headPos.y - camPos.y;
            sLeaser.sprites[EyesSpriteStart].x = headPos.x - camPos.x;
            sLeaser.sprites[EyesSpriteStart].y = headPos.y - camPos.y;
            if (!IsMiros)
            {
                sLeaser.sprites[MaskSpriteStart].x = headPos.x - camPos.x;
                sLeaser.sprites[MaskSpriteStart].y = headPos.y - camPos.y;
            }
            sLeaser.sprites[HeadSpriteStart].rotation = HeadRotation(timeStacker);
            sLeaser.sprites[HeadSpriteStart].element = Futile.atlasManager.GetElementWithName("AlcedoHeadA" + headGraphic);
            sLeaser.sprites[HeadSpriteStart].anchorX = 0.5f;
            sLeaser.sprites[HeadSpriteStart].anchorY = 0.5f;
            sLeaser.sprites[HeadSpriteStart].scaleX = (neckToHeadAngle > 0f ? -1f : 1f) * (IsKing ? 1.15f : 1f);
            sLeaser.sprites[HeadSpriteStart].scaleY = IsKing ? 1.15f : 1f;
            sLeaser.sprites[MaskSpriteStart].rotation = HeadRotation(timeStacker);
            sLeaser.sprites[MaskSpriteStart].element = Futile.atlasManager.GetElementWithName("AlcedoMaskA" + headGraphic);
            sLeaser.sprites[MaskSpriteStart].anchorX = 0.5f;
            sLeaser.sprites[MaskSpriteStart].anchorY = 0.5f;
            sLeaser.sprites[MaskSpriteStart].scaleX = (neckToHeadAngle > 0f ? -1f : 1f) * (IsKing ? 1.15f : 1f);
            sLeaser.sprites[MaskSpriteStart].scaleY = IsKing ? 1.15f : 1f;
            sLeaser.sprites[MaskSpriteStart].isVisible = (alcedo.State as Alcedo.AlcedoState).mask;

            sLeaser.sprites[EyesSpriteStart].rotation = HeadRotation(timeStacker);
            sLeaser.sprites[EyesSpriteStart].element = Futile.atlasManager.GetElementWithName("AlcedoEyesA" + headGraphic);
            sLeaser.sprites[EyesSpriteStart].scaleX = (neckToHeadAngle > 0f ? -1f : 1f) * (IsKing ? 1.15f : 1f);
            sLeaser.sprites[EyesSpriteStart].anchorX = 0.5f;
            sLeaser.sprites[EyesSpriteStart].anchorY = 0.5f;
            sLeaser.sprites[EyesSpriteStart].scaleY = IsKing ? 1.15f : 1f;/*
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
            #endregion
            #region 颈部
            Vector2 lastNeckPos = Vector2.Lerp(alcedo.neck.connectedChunk.lastPos, alcedo.neck.connectedChunk.pos, timeStacker);
            float lastNeckStretchedRad = 9f;// * alcedo.bodySizeFac;//(IsKing ? 11f : 8f);
            for (int j = 0; j < alcedo.neck.tChunks.Length; j++)
            {
                Vector2 neckPos = Vector2.Lerp(alcedo.neck.tChunks[j].lastPos, alcedo.neck.tChunks[j].pos, timeStacker);/*
                if (ModManager.MMF && !IsMiros)
                {
                    neckPos = Vector2.Lerp(neckPos, Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker), Mathf.Pow(j / (alcedo.neck.tChunks.Length - 1), 7f));
                    //neckPos = Vector2.Lerp(neckPos, Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker), Mathf.Pow(j / (alcedo.neck.tChunks.Length - 1), 7f));
                }*/

                if (j == alcedo.neck.tChunks.Length - 1)
                {
                    neckPos = Vector2.Lerp(neckPos, headPos, 0.5f) + Custom.DegToVec(neckToHeadAngle) * 2f;
                }
                Vector2 normalized = (neckPos - lastNeckPos).normalized;
                Vector2 neckPerp = Custom.PerpendicularVector(normalized);
                float segLength = Vector2.Distance(neckPos, lastNeckPos) / 5f;
                (sLeaser.sprites[NeckSpriteStart] as TriangleMesh).MoveVertice(j * 4 + 0, lastNeckPos - neckPerp * (alcedo.neck.tChunks[j].stretchedRad + lastNeckStretchedRad) * 0.5f + normalized * segLength - camPos);
                (sLeaser.sprites[NeckSpriteStart] as TriangleMesh).MoveVertice(j * 4 + 1, lastNeckPos + neckPerp * (alcedo.neck.tChunks[j].stretchedRad + lastNeckStretchedRad) * 0.5f + normalized * segLength - camPos);
                if (j == alcedo.neck.tChunks.Length - 1 && !IsMiros)
                {
                    segLength = 0f;
                }
                (sLeaser.sprites[NeckSpriteStart] as TriangleMesh).MoveVertice(j * 4 + 2, neckPos - neckPerp * alcedo.neck.tChunks[j].stretchedRad - normalized * segLength - camPos);
                (sLeaser.sprites[NeckSpriteStart] as TriangleMesh).MoveVertice(j * 4 + 3, neckPos + neckPerp * alcedo.neck.tChunks[j].stretchedRad - normalized * segLength - camPos);
                lastNeckStretchedRad = alcedo.neck.tChunks[j].stretchedRad;
                lastNeckPos = neckPos;
            }
            #endregion
            #region 腰部
            Vector2 lastWaistPos = Vector2.Lerp(alcedo.waist.connectedChunk.lastPos, alcedo.waist.connectedChunk.pos, timeStacker);
            float lastWaistStretchedRad = 1.2f * 8f * alcedo.bodySizeFac;
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
                    float tentacleRad = alcedo.tentacles[k].TentacleContour((m + 0.5f) / alcedo.tentacles[k].tChunks.Length);
                    tentacleRad *= Mathf.Clamp(Mathf.Pow(alcedo.tentacles[k].tChunks[m].stretchedFac, 0.35f), 0.5f, 1.5f);
                    (sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 0, lastTentaclePos - tentaclePerp * (tentacleRad + lastTentacleRad) * 0.5f + dir * segLength - camPos);
                    (sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 1, lastTentaclePos + tentaclePerp * (tentacleRad + lastTentacleRad) * 0.5f + dir * segLength - camPos);
                    tentacleRad = alcedo.tentacles[k].TentacleContour((m + 1f) / alcedo.tentacles[k].tChunks.Length);
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
                sLeaser.sprites[ForePawSprite(k)].rotation = Custom.AimFromOneVectorToAnother(halfWingPos, nextHalfWingPos) * Mathf.Lerp(0.9f, 1f, alcedo.tentacles[k].flyingMode)
                    - Mathf.Lerp(-10f, 60f, alcedo.tentacles[k].flyingMode) * (k % 2 == 0 ? 1f : -1f);
                sLeaser.sprites[ForePawColorSprite(k)].x = sLeaser.sprites[ForePawSprite(k)].x;
                sLeaser.sprites[ForePawColorSprite(k)].y = sLeaser.sprites[ForePawSprite(k)].y;
                sLeaser.sprites[ForePawColorSprite(k)].rotation = sLeaser.sprites[ForePawSprite(k)].rotation;
                sLeaser.sprites[ForePawSprite(k)].color = sLeaser.sprites[BodySprite(0)].color;
                sLeaser.sprites[ForePawColorSprite(k)].color = sLeaser.sprites[MaskSpriteStart].color;//Color.Lerp(wings[k, 0, 0].CurrentClawColor(), palette.blackColor, ModManager.MMF && !IsMiros ? darkness : 0f);
                for (int l = 0; l < featherLayersPerWing; l++)
                    for (int n = 0; n < feathersPerLayer; n++)
                    {
                        sLeaser.sprites[FeatherSprite(k, l, n)].x = Mathf.Lerp(wings[k, l, n].ConnectedLastPos.x, wings[k, l, n].ConnectedPos.x, timeStacker) - camPos.x;
                        sLeaser.sprites[FeatherSprite(k, l, n)].y = Mathf.Lerp(wings[k, l, n].ConnectedLastPos.y, wings[k, l, n].ConnectedPos.y, timeStacker) - camPos.y;
                        sLeaser.sprites[FeatherSprite(k, l, n)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(wings[k, l, n].lastPos, wings[k, l, n].pos, timeStacker), Vector2.Lerp(wings[k, l, n].ConnectedLastPos, wings[k, l, n].ConnectedPos, timeStacker));
                        if (!IsMiros || n != feathersPerLayer - 1)
                        {
                            sLeaser.sprites[FeatherSprite(k, l, n)].scaleX = Mathf.Lerp(3f, wings[k, l, n].width, (wings[k, l, n].extendedFac + alcedo.tentacles[k].flyingMode) * 0.5f) / 9f * (k % 2 == 0 ? 1f : -1f) * (IsKing ? 1.3f : 1f);
                            sLeaser.sprites[FeatherSprite(k, l, n)].scaleY = Vector2.Distance(Vector2.Lerp(wings[k, l, n].ConnectedLastPos, wings[k, l, n].ConnectedPos, timeStacker), Vector2.Lerp(wings[k, l, n].lastPos, wings[k, l, n].pos, timeStacker)) / 107f;
                        }
                        else if (IsMiros)
                        {
                            sLeaser.sprites[FeatherSprite(k, l, n)].scaleX = (k % 2 == 0 ? 1 : -1) * Mathf.Pow(wings[k, l, n].extendedFac, 3f);
                            sLeaser.sprites[FeatherSprite(k, l, n)].scaleY = Mathf.Pow(wings[k, l, n].extendedFac, 3f);
                            sLeaser.sprites[FeatherSprite(k, l, n)].rotation += 200 * (k % 2 == 0 ? 1 : -1);
                        }
                        if (!IsMiros || n != feathersPerLayer - 1)
                        {
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].x = Mathf.Lerp(wings[k, l, n].ConnectedLastPos.x, wings[k, l, n].ConnectedPos.x, timeStacker) - camPos.x;
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].y = Mathf.Lerp(wings[k, l, n].ConnectedLastPos.y, wings[k, l, n].ConnectedPos.y, timeStacker) - camPos.y;
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].scaleY = Vector2.Distance(Vector2.Lerp(wings[k, l, n].ConnectedLastPos, wings[k, l, n].ConnectedPos, timeStacker), Vector2.Lerp(wings[k, l, n].lastPos, wings[k, l, n].pos, timeStacker)) / 107f;
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(wings[k, l, n].lastPos, wings[k, l, n].pos, timeStacker), Vector2.Lerp(wings[k, l, n].ConnectedLastPos, wings[k, l, n].ConnectedPos, timeStacker));
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].scaleX = Mathf.Lerp(3f, wings[k, l, n].width, (wings[k, l, n].extendedFac + alcedo.tentacles[k].flyingMode) * 0.5f) / 9f * (k % 2 == 0 ? 1f : -1f) * (IsKing ? 1.3f : 1f);
                        }
                        if (!shadowMode)
                        {
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].color = Color.Lerp(wings[k, l, n].CurrentColor(), palette.blackColor, ModManager.MMF && !IsMiros ? darkness : 0f);
                        }
                        //暂时隐藏了羽毛
                        /*
                        sLeaser.sprites[FeatherSprite(k, l, n)].isVisible = false;
                        sLeaser.sprites[FeatherColorSprite(k, l, n)].isVisible = false;*/
                        //暂时换了爪子颜色
                        //sLeaser.sprites[ForePawColorSprite(0)].color = Color.red;
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
                    float tentacleRad = alcedo.legs[k].TentacleContour((m + 0.5f) / alcedo.legs[k].tChunks.Length);
                    tentacleRad *= Mathf.Clamp(Mathf.Pow(alcedo.legs[k].tChunks[m].stretchedFac, 0.35f), 0.5f, 1.5f);
                    (sLeaser.sprites[LegSprite(k)] as TriangleMesh).MoveVertice(m * 4, lastLegPos - tentaclePerp * tentacleRad + dir * segLength - camPos);
                    (sLeaser.sprites[LegSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 1, lastLegPos + tentaclePerp * tentacleRad + dir * segLength - camPos);
                    tentacleRad = alcedo.legs[k].TentacleContour((m + 1f) / alcedo.legs[k].tChunks.Length);
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
                sLeaser.sprites[HindPawSprite(k)].rotation = Custom.AimFromOneVectorToAnother(halfWingPos, nextHalfWingPos) * Mathf.Lerp(0.9f, 1f, alcedo.legs[k].flyingMode)
                    - Mathf.Lerp(-10f, 60f, alcedo.legs[k].flyingMode) * (k % 2 == 0 ? 1f : -1f);
                sLeaser.sprites[HindPawColorSprite(k)].x = sLeaser.sprites[HindPawSprite(k)].x;
                sLeaser.sprites[HindPawColorSprite(k)].y = sLeaser.sprites[HindPawSprite(k)].y;
                sLeaser.sprites[HindPawColorSprite(k)].rotation = sLeaser.sprites[HindPawSprite(k)].rotation;
                sLeaser.sprites[HindPawSprite(k)].color = sLeaser.sprites[BodySprite(0)].color;
                sLeaser.sprites[HindPawColorSprite(k)].color = sLeaser.sprites[MaskSpriteStart].color;//Color.Lerp(wings[k, 0, 0].CurrentClawColor(), palette.blackColor, ModManager.MMF && !IsMiros ? darkness : 0f);
            }
            #endregion
            UpdateSpritesLevel(sLeaser);
        }
        #endregion

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
                sLeaser.sprites[MaskSpriteStart].color = Color.Lerp(color3, palette.blackColor, t);
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
                sLeaser.sprites[EyesSpriteStart].color = eyeCol;
            }
            else
            {
                sLeaser.sprites[EyesSpriteStart].color = Color.Lerp(Color.Lerp(ColorB.rgb, new Color(0f, 0f, 0f), IsKing ? 0.4f : 0.6f), palette.blackColor, t);
            }
            if (albino)
            {
                sLeaser.sprites[EyesSpriteStart].color = Color.Lerp(Color.Lerp(Color.red, new Color(0f, 0f, 0f), IsKing ? 0.2f : 0.3f), palette.blackColor, t);
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

        public override void SuckedIntoShortCut(Vector2 shortCutPosition)
        {
            for (int i = 0; i < tail.Length; i++)
            {
                tail[i].lastPos = tail[i].pos;
                tail[i].vel *= 0.5f;
                tail[i].pos = (tail[i].pos * 5f + shortCutPosition) / 6f;
            }
            for (int i = 0; i < alcedo.legs.Length; i++)
            {
                for (int j = 0; j < alcedo.legs[i].tChunks.Length; j++)
                {
                    alcedo.legs[i].tChunks[j].lastPos = alcedo.legs[i].tChunks[j].pos;
                    alcedo.legs[i].tChunks[j].vel *= 0.5f;
                    alcedo.legs[i].tChunks[j].pos = (alcedo.legs[i].tChunks[j].pos * 5f + shortCutPosition) / 6f;
                }
            }
            for (int i = 0; i < alcedo.tentacles.Length; i++)
            {
                for (int j = 0; j < alcedo.tentacles[i].tChunks.Length; j++)
                {
                    alcedo.tentacles[i].tChunks[j].lastPos = alcedo.tentacles[i].tChunks[j].pos;
                    alcedo.tentacles[i].tChunks[j].vel *= 0.5f;
                    alcedo.tentacles[i].tChunks[j].pos = (alcedo.tentacles[i].tChunks[j].pos * 5f + shortCutPosition) / 6f;
                }
            }
        }

        private void UpdateTailSegment(int i, float stiffness)
        {
            tail[i].Update();
            Vector2 hipPos = Vector2.Lerp(owner.bodyChunks[5].pos, owner.bodyChunks[6].pos, 0.5f);
            Vector2 bodyPos = Vector2.Lerp(owner.bodyChunks[0].pos, owner.bodyChunks[1].pos, 0.5f);
            tail[0].connectedPoint = hipPos;
            tail[0].vel += Custom.DirVec(bodyPos, hipPos) * 0.5f;
            tail[1].vel += Custom.DirVec(bodyPos, hipPos) * 0.2f;
            tail[i].vel += Custom.DirVec(bodyPos, hipPos) * 0.2f * (float)(tail.Length - i) / (float)tail.Length;
            if (alcedo.room.PointSubmerged(tail[i].pos))
            {
                tail[i].vel *= 0.8f;
            }
            else
            {
                TailSegment tailSegment = tail[i];
                tailSegment.vel.y = tailSegment.vel.y - 0.9f * Mathf.Pow(i / (float)(tail.Length - 1), 3f);
            }
            if (!Custom.DistLess(tail[i].pos, hipPos, 15f * (i + 1)))
            {
                tail[i].pos = hipPos + Custom.DirVec(hipPos, tail[i].pos) * 15f * (i + 1) * (ModManager.MMF ? owner.room.gravity : 1f);
            }
            Vector2 vector = hipPos;
            if (i == 1)
            {
                vector = hipPos;
            }
            else if (i > 1)
            {
                vector = tail[i - 2].pos;
            }
            vector = Vector2.Lerp(vector, hipPos, 0.2f);
            tail[i].vel += Custom.DirVec(vector, tail[i].pos) * stiffness * Mathf.Pow(tailStiffnessDecline, i) / Vector2.Distance(vector, tail[i].pos);
            if (i == 0)
            {
                tail[i].vel += tailDirection;
            }/*
            if (this.alcedo.Consious && this.alcedo.swim > 0.5f)
            {
                if (!(this.alcedo.Template.type == CreatureTemplate.Type.Salamander) && (!ModManager.MSC || !(this.alcedo.Template.type == DLCSharedEnums.CreatureTemplateType.EelLizard)))
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
            else if (showDominance > 0f)
            {
                tail[i].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * showDominance * UnityEngine.Random.value * 2f;
            }
        }

        private void UpdateSpritesLevel(RoomCamera.SpriteLeaser sLeaser)
        {
            if (headGraphic >= 5)
            {
                sLeaser.sprites[NeckSpriteStart].MoveInFrontOfOtherNode(sLeaser.sprites[HeadSpriteStart]);
            }
            else
            {
                sLeaser.sprites[NeckSpriteStart].MoveBehindOtherNode(sLeaser.sprites[HeadSpriteStart]);
            }
            int frontWing = Custom.DirVec(alcedo.bodyChunks[5].pos, alcedo.bodyChunks[0].pos).x >= 0 ? 0 : 1;
            int behindWing = 1 - frontWing;
            FNode frontPos = sLeaser.sprites[BodySprite(2)];// alcedo.AirBorne ? sLeaser.sprites[TentacleSprite(behindWing)] : sLeaser.sprites[HindPawColorSprite(frontWing)];
            FNode behindPos = sLeaser.sprites[TailSpriteStart];
            for (int i = 0; i < alcedo.tentacles.Length; i++)
            {
                if (i == frontWing &&
                    Custom.DirVec(alcedo.bodyChunks[0].pos, alcedo.tentacles[i].tChunks[0].pos).x * Custom.DirVec(alcedo.bodyChunks[5].pos, alcedo.bodyChunks[0].pos).x < 0)
                {
                    if (alcedo.AirBorne)
                    {
                        for (int j = 0; j < featherLayersPerWing; j++)
                            for (int k = 0; k < feathersPerLayer; k++)
                            {
                                sLeaser.sprites[FeatherSprite(i, j, k)].MoveBehindOtherNode(frontPos);
                                sLeaser.sprites[FeatherColorSprite(i, j, k)].MoveBehindOtherNode(frontPos);
                            }
                        sLeaser.sprites[TentacleSprite(i)].MoveBehindOtherNode(frontPos);
                        sLeaser.sprites[ForePawSprite(i)].MoveBehindOtherNode(frontPos);
                        sLeaser.sprites[ForePawColorSprite(i)].MoveBehindOtherNode(frontPos);
                    }
                    else
                    {
                        sLeaser.sprites[ForePawColorSprite(i)].MoveInFrontOfOtherNode(frontPos);
                        sLeaser.sprites[ForePawSprite(i)].MoveInFrontOfOtherNode(frontPos);
                        sLeaser.sprites[TentacleSprite(i)].MoveInFrontOfOtherNode(frontPos);
                        for (int j = featherLayersPerWing - 1; j >= 0; j--)
                            for (int k = feathersPerLayer - 1; k >= 0; k--)
                            {
                                sLeaser.sprites[FeatherColorSprite(i, j, k)].MoveInFrontOfOtherNode(frontPos);
                                sLeaser.sprites[FeatherSprite(i, j, k)].MoveInFrontOfOtherNode(frontPos);
                            }
                    }
                }
                else
                {
                    sLeaser.sprites[ForePawColorSprite(i)].MoveInFrontOfOtherNode(behindPos);
                    sLeaser.sprites[ForePawSprite(i)].MoveInFrontOfOtherNode(behindPos);
                    sLeaser.sprites[TentacleSprite(i)].MoveInFrontOfOtherNode(behindPos);
                    for (int j = featherLayersPerWing - 1; j >= 0; j--)
                        for (int k = feathersPerLayer - 1; k >= 0; k--)
                        {
                            sLeaser.sprites[FeatherColorSprite(i, j, k)].MoveInFrontOfOtherNode(behindPos);
                            sLeaser.sprites[FeatherSprite(i, j, k)].MoveInFrontOfOtherNode(behindPos);
                        }
                }
            }
            for (int i = 0; i < alcedo.legs.Length; i++)
            {
                if (i == frontWing)
                {
                    sLeaser.sprites[HindPawColorSprite(i)].MoveInFrontOfOtherNode(sLeaser.sprites[BodySprite(2)]);
                    sLeaser.sprites[HindPawSprite(i)].MoveInFrontOfOtherNode(sLeaser.sprites[BodySprite(2)]);
                    sLeaser.sprites[LegSprite(i)].MoveInFrontOfOtherNode(sLeaser.sprites[BodySprite(2)]);
                }
                else
                {
                    sLeaser.sprites[HindPawColorSprite(i)].MoveInFrontOfOtherNode(sLeaser.sprites[TailSpriteStart]);
                    sLeaser.sprites[HindPawSprite(i)].MoveInFrontOfOtherNode(sLeaser.sprites[TailSpriteStart]);
                    sLeaser.sprites[LegSprite(i)].MoveInFrontOfOtherNode(sLeaser.sprites[TailSpriteStart]);
                }
            }
            sLeaser.sprites[TailSpriteStart].MoveInFrontOfOtherNode(sLeaser.sprites[HindPawColorSprite(behindWing)]);
            for (int j = 0; j < cosmetics.Count; j++)
            {
                if (cosmetics[j].spritesOverlap == AlcedoScaleTemplate.SpritesOverlap.BehindHead)
                {
                    for (int i = cosmetics[j].startSprite; i < cosmetics[j].startSprite + cosmetics[j].numberOfSprites; i++)
                    {
                        if (headGraphic < 5)
                        {
                            sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[MaskSpriteStart]);
                        }
                        else
                        {
                            sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[HeadSpriteStart]);
                        }
                    }
                }
                if (cosmetics[j].spritesOverlap == AlcedoScaleTemplate.SpritesOverlap.Behind)
                {
                    if (alcedo.AirBorne)
                    {
                        for (int i = cosmetics[j].startSprite; i < cosmetics[j].startSprite + cosmetics[j].numberOfSprites; i++)
                        {
                            sLeaser.sprites[i].MoveBehindOtherNode(sLeaser.sprites[TailSpriteStart]);
                        }
                    }
                    else
                    {
                        for (int i = cosmetics[j].startSprite + cosmetics[j].numberOfSprites - 1; i >= cosmetics[j].startSprite; i--)
                        {
                            sLeaser.sprites[i].MoveInFrontOfOtherNode(sLeaser.sprites[TailSpriteStart]);
                        }
                    }
                }
                if (cosmetics[j].spritesOverlap == AlcedoScaleTemplate.SpritesOverlap.Discretion)
                    cosmetics[j].UpdateSpritesLevel(sLeaser);
            }
        }

        #region 装饰
        public AlcedoSpineData SpinePosition(float s, float timeStacker)
        {
            //float headLength = 10f;//此为估测值
            Vector2 lastChunkPos;
            Vector2 nextNextChunkPos;
            Vector2 nextChunkPos;
            Vector2 lastChunkVel;
            Vector2 nextChunkVel;
            float lastRad;
            float nextRad;
            float t;

            if (s < 0)
            {
                float neckToHeadAngle = Custom.AimFromOneVectorToAnother(Vector2.Lerp(alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].lastPos,
                                                                                      alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].pos, timeStacker),
                                                                         Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker));
                lastChunkPos = Vector2.Lerp(head.lastPos, head.pos, timeStacker) + Custom.DegToVec(neckToHeadAngle);
                lastChunkVel = head.vel;
                lastRad = head.rad;
                nextChunkPos = Vector2.Lerp(head.lastPos, head.pos, timeStacker);
                nextChunkVel = Vector2.Lerp(alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].vel, head.vel, 0.85f);
                nextNextChunkPos = Vector2.Lerp(alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].lastPos, alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].pos, timeStacker) - Custom.DegToVec(neckToHeadAngle);
                nextRad = head.rad;
                t = 0.5f;
            }
            else if (s < NeckLength / SpineLength)
            {
                float inBodyRatio = Mathf.InverseLerp(0f, NeckLength / SpineLength, s);
                int lastIndex = alcedo.neck.tChunks.Length - 1 - Mathf.FloorToInt(inBodyRatio * alcedo.neck.tChunks.Length);
                int nextIndex = alcedo.neck.tChunks.Length - 2 - Mathf.FloorToInt(inBodyRatio * alcedo.neck.tChunks.Length);

                if (lastIndex > alcedo.neck.tChunks.Length - 1)
                {
                    lastChunkPos = Vector2.Lerp(head.lastPos, head.pos, timeStacker);
                    lastRad = alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].rad;
                    lastChunkVel = head.vel;
                }
                else
                {
                    lastChunkPos = Vector2.Lerp(alcedo.neck.tChunks[lastIndex].lastPos, alcedo.neck.tChunks[lastIndex].pos, timeStacker);
                    lastRad = alcedo.neck.tChunks[lastIndex].rad;//this.BodyChunkDisplayRad((lastIndex < 2) ? 1 : 2) * this.iVars.fatness;
                    lastChunkVel = alcedo.neck.tChunks[lastIndex].vel;
                }

                if (nextIndex < 0)
                {
                    nextChunkPos = Vector2.Lerp(alcedo.neck.connectedChunk.lastPos, alcedo.neck.connectedChunk.pos, timeStacker);
                    nextChunkVel = alcedo.neck.connectedChunk.vel;
                    nextRad = alcedo.neck.connectedChunk.rad;

                    float neckPosRatio = Vector2.Distance(Vector2.Lerp(alcedo.neck.tChunks[0].lastPos, alcedo.neck.tChunks[0].pos, timeStacker), Vector2.Lerp(alcedo.neck.connectedChunk.lastPos, alcedo.neck.connectedChunk.pos, timeStacker));
                    float ratio = Mathf.InverseLerp(0f, BodyLength, neckPosRatio);
                    nextNextChunkPos = Vector2.Lerp(Vector2.Lerp(alcedo.neck.connectedChunk.lastPos, alcedo.neck.connectedChunk.pos, timeStacker),
                                                Vector2.Lerp(this.tail[0].lastPos, this.tail[0].pos, timeStacker),
                                                ratio);
                }
                else
                {
                    nextChunkPos = Vector2.Lerp(alcedo.neck.tChunks[nextIndex].lastPos, alcedo.neck.tChunks[nextIndex].pos, timeStacker);
                    nextChunkVel = alcedo.neck.tChunks[nextIndex].vel;
                    nextRad = alcedo.neck.tChunks[nextIndex].rad;

                    if (nextIndex < 1)
                    {
                        nextNextChunkPos = Vector2.Lerp(alcedo.neck.connectedChunk.lastPos, alcedo.neck.connectedChunk.pos, timeStacker);
                    }
                    else
                    {
                        nextNextChunkPos = Vector2.Lerp(alcedo.neck.tChunks[nextIndex - 1].lastPos, alcedo.neck.tChunks[nextIndex - 1].pos, timeStacker);
                    }
                }
                t = Mathf.InverseLerp(lastIndex + 1, nextIndex + 1, (1f - inBodyRatio) * alcedo.neck.tChunks.Length);
            }
            else if (s < (NeckLength + BodyLength) / SpineLength)
            {
                float inBodyRatio = Mathf.InverseLerp(NeckLength / SpineLength, (NeckLength + BodyLength) / SpineLength, s);

                lastChunkPos = Vector2.Lerp(alcedo.neck.connectedChunk.lastPos, alcedo.neck.connectedChunk.pos, timeStacker);
                lastRad = alcedo.neck.connectedChunk.rad;
                lastChunkVel = alcedo.neck.connectedChunk.vel;
                nextChunkPos = Vector2.Lerp(this.tail[0].lastPos, this.tail[0].pos, timeStacker);
                nextRad = this.tail[0].rad;
                nextChunkVel = this.tail[0].vel;
                nextNextChunkPos = Vector2.Lerp(this.tail[1].lastPos, this.tail[1].pos, timeStacker);
                t = inBodyRatio;
            }
            else
            {
                float inBodyRatio = Mathf.InverseLerp((NeckLength + BodyLength) / SpineLength, 1f, s);
                int lastIndex = Mathf.FloorToInt(inBodyRatio * tail.Length);
                int nextIndex = Mathf.FloorToInt(inBodyRatio * tail.Length) + 1;
                if (nextIndex > tail.Length - 1)
                {
                    nextIndex = tail.Length - 1;
                }
                lastChunkPos = Vector2.Lerp(tail[lastIndex].lastPos, tail[lastIndex].pos, timeStacker);
                lastRad = tail[lastIndex].StretchedRad;
                lastChunkVel = tail[lastIndex].vel;
                nextChunkPos = Vector2.Lerp(tail[nextIndex].lastPos, tail[nextIndex].pos, timeStacker);
                nextRad = tail[nextIndex].StretchedRad;
                nextChunkVel = tail[nextIndex].vel;
                nextNextChunkPos = Vector2.Lerp(tail[Math.Min(nextIndex + 1, tail.Length - 1)].lastPos, tail[Math.Min(nextIndex + 1, tail.Length - 1)].pos, timeStacker);
                t = Mathf.InverseLerp(lastIndex, nextIndex, inBodyRatio * tail.Length);
            }
            //从前向后的方向,指向身体后侧
            Vector2 dir = Vector2.Lerp(nextChunkPos - lastChunkPos, nextNextChunkPos - nextChunkPos, t).normalized;
            if (dir.x == 0f && dir.y == 0f)
            {
                dir = (tail[tail.Length - 1].pos - tail[tail.Length - 2].pos).normalized;
            }
            //逆时针旋转90°
            Vector2 perp = Custom.PerpendicularVector(dir);
            float rad = Mathf.Lerp(lastRad, nextRad, t);
            //中间0，朝右1，朝左-1，Custom.VecToDeg(dir)：dir与向量(0, 1)的夹角
            float depthRotation = Mathf.Sin(Custom.VecToDeg(dir) / 180f * (float)Math.PI);
            //如果是脖子上的装饰，那么看头和身体的位置来改变透视方向（而不完全是脖子的位置）
            if (s < NeckLength / SpineLength)
            {
                float newDepthRotation = Mathf.Abs(depthRotation);
                newDepthRotation *= 2f * Mathf.Sin(Custom.VecToDeg(BodyPosition(1, timeStacker) - BodyPosition(4, timeStacker)) / 180f * (float)Math.PI);
                depthRotation = Mathf.Lerp(newDepthRotation, depthRotation, s / (NeckLength / SpineLength));
            }
            depthRotation = Mathf.Pow(Mathf.Abs(depthRotation), Mathf.Lerp(1.2f, 0.3f, Mathf.Pow(Mathf.Max(0f, s), 0.5f))) * Mathf.Sign(depthRotation);
            Vector2 pos = Vector2.Lerp(lastChunkPos, nextChunkPos, t);
            Vector2 vel = Vector2.Lerp(lastChunkVel, nextChunkVel, t);
            Vector2 outerPos = pos + perp * depthRotation * rad * 0.85f;
            return new AlcedoSpineData(s, pos, vel, outerPos, dir, perp, depthRotation, rad);
        }

        private Vector2 BodyPosition(int p, float timeStacker)
        {
            if (p > alcedo.bodyChunks.Length - 1)
            {
                return new Vector2(100f, 100f);
            }
            return Vector2.Lerp(alcedo.bodyChunks[p].lastPos, alcedo.bodyChunks[p].pos, timeStacker);/*
            int num = (p < 2) ? 1 : 2;
            if (p % 2 == 1)
            {
                return Vector2.Lerp(this.drawPositions[num, 1], this.drawPositions[num, 0], timeStacker);
            }
            Vector2 vector = Vector2.Lerp(this.drawPositions[num, 1], this.drawPositions[num, 0], timeStacker);
            Vector2 a = Vector2.Lerp(this.tail[0].lastPos, this.tail[0].pos, timeStacker);
            if (num == 1)
            {
                a = Vector2.Lerp(this.drawPositions[2, 1], this.drawPositions[2, 0], timeStacker);
            }
            Vector2 b = Vector2.Lerp(this.drawPositions[num - 1, 1], this.drawPositions[num - 1, 0], timeStacker);
            Vector3 vector2 = Vector3.Slerp(vector - b, a - vector, 0.35f).normalized;
            vector2 *= -0.5f * Vector2.Distance(vector, b);
            return new Vector2(vector.x + vector2.x, vector.y + vector2.y);*/
        }

        public float HeadRotation(float timeStacker)
        {
            float neckToHeadAngle = Custom.AimFromOneVectorToAnother(Vector2.Lerp(alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].lastPos,
                                                                                  alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].pos, timeStacker),
                Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker));
            float num3 = (8 - headGraphic) * Mathf.Sign(neckToHeadAngle) * 22.5f;
            return neckToHeadAngle - num3;
            /*
            float num = Custom.AimFromOneVectorToAnother(Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker), Vector2.Lerp(head.lastPos, head.pos, timeStacker));
            float num2 = Mathf.Lerp(lastHeadDepthRotation, headDepthRotation, timeStacker);
            float num3 = Mathf.Clamp(Mathf.Lerp(alcedo.lastJawOpen, alcedo.JawOpen, timeStacker), 0f, 1f);
            return num + alcedo.alcedoParams.jawOpenAngle * (1f - alcedo.alcedoParams.jawOpenLowerJawFac) * num3 * num2;*/
        }

        private int AddCosmetic(int spriteIndex, AlcedoScaleTemplate cosmetic)
        {
            cosmetics.Add(cosmetic);
            spriteIndex += cosmetic.numberOfSprites;
            ExtraSpritesLength += cosmetic.numberOfSprites;
            return spriteIndex;
        }

        public struct AlcedoSpineData
        {
            public float f;
            public Vector2 pos;
            public Vector2 vel;
            public Vector2 outerPos;
            public Vector2 dir;
            public Vector2 perp;
            public float depthRotation;
            public float rad;

            public AlcedoSpineData(float f, Vector2 pos, Vector2 vel, Vector2 outerPos, Vector2 dir, Vector2 perp, float depthRotation, float rad)
            {
                this.f = f;
                this.pos = pos;
                this.vel = vel;
                this.outerPos = outerPos;
                this.dir = dir;
                this.perp = perp;
                this.depthRotation = depthRotation;
                this.rad = rad;
            }
        }
        #endregion
    }
}