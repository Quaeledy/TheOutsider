using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        public class EyeTrail
        {
            public bool visible;

            public int sprite;

            public AlcedoGraphics owner;

            public List<Vector2> positionsList;

            public List<Color> colorsList;

            public int savPoss;

            public float updateTicker;

            public EyeTrail(AlcedoGraphics owner, int sprite)
            {
                this.sprite = sprite;
                this.owner = owner;
                savPoss = 15;
                Reset();
            }

            public void Reset()
            {
                positionsList = new List<Vector2> { owner.alcedo.VisionPoint };
                colorsList = new List<Color> { owner.EyeColor() };
            }

            private Vector2 GetSmoothPos(int i, float timeStacker)
            {
                return Vector2.Lerp(GetPos(i + 1), GetPos(i), timeStacker);
            }

            private Vector2 GetPos(int i)
            {
                return positionsList[Custom.IntClamp(i, 0, positionsList.Count - 1)];
            }

            private Color GetCol(int i)
            {
                return colorsList[Custom.IntClamp(i, 0, colorsList.Count - 1)];
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites[sprite] = TriangleMesh.MakeLongMesh(savPoss - 1, pointyTip: false, customColor: true);
            }

            public void DrawSprite(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 vector = owner.EyePos(timeStacker);
                float num = 8f * owner.eyeSize;
                for (int i = 0; i < savPoss - 1; i++)
                {
                    _ = (float)i / (float)(savPoss - 1);
                    Vector2 smoothPos = GetSmoothPos(i, timeStacker);
                    Vector2 smoothPos2 = GetSmoothPos(i + 1, timeStacker);
                    Vector2 normalized = (vector - smoothPos).normalized;
                    Vector2 vector2 = Custom.PerpendicularVector(normalized);
                    normalized *= Vector2.Distance(vector, smoothPos2) / 5f;
                    (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4, vector - vector2 * num - normalized - camPos);
                    (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector2 * num - normalized - camPos);
                    (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4 + 2, smoothPos - vector2 * num + normalized - camPos);
                    (sLeaser.sprites[sprite] as TriangleMesh).MoveVertice(i * 4 + 3, smoothPos + vector2 * num + normalized - camPos);
                    sLeaser.sprites[sprite].isVisible = visible;
                    vector = smoothPos;
                }
                for (int j = 0; j < (sLeaser.sprites[sprite] as TriangleMesh).verticeColors.Length; j++)
                {
                    float num2 = (float)j / (float)((sLeaser.sprites[sprite] as TriangleMesh).verticeColors.Length - 1);
                    (sLeaser.sprites[sprite] as TriangleMesh).verticeColors[j] = new Color(GetCol(j).r, GetCol(j).g, GetCol(j).b, Mathf.Min(1f, Mathf.Pow(1f - num2, 1.5f) * 1f));
                }
            }

            public void UpdatePosition(Vector2 pos)
            {
                updateTicker += 1f;
                if (updateTicker % 2f == 0f)
                {
                    positionsList.Insert(0, pos);
                    if (positionsList.Count > savPoss)
                    {
                        positionsList.RemoveAt(savPoss);
                    }
                    colorsList.Insert(0, owner.EyeColor());
                    if (colorsList.Count > savPoss)
                    {
                        colorsList.RemoveAt(savPoss);
                    }
                }
            }

            public void Update()
            {
                positionsList.Insert(0, owner.EyePos(1f));
                if (positionsList.Count > savPoss)
                {
                    positionsList.RemoveAt(savPoss);
                }
                colorsList.Insert(0, owner.EyeColor());
                if (colorsList.Count > savPoss)
                {
                    colorsList.RemoveAt(savPoss);
                }
            }
        }

        public class BeakGraphic
        {
            private AlcedoGraphics owner;

            public int totalSprites;

            public int firstSprite;

            public int index;

            public float[,] teeth;

            public BeakGraphic(AlcedoGraphics owner, int index, int firstSprite)
            {
                this.owner = owner;
                this.firstSprite = firstSprite;
                this.index = index;
                teeth = new float[UnityEngine.Random.Range(10, 20), 3];
                for (int i = 0; i < teeth.GetLength(0); i++)
                {
                    teeth[i, 0] = Mathf.Lerp(0.1f, 0.9f, Mathf.Pow(UnityEngine.Random.value, 0.8f));
                    teeth[i, 1] = Mathf.Lerp(0.5f, 1f, UnityEngine.Random.value);
                    teeth[i, 2] = Mathf.Lerp(-1f, 1f, Mathf.Lerp(UnityEngine.Random.value, teeth[i, 0], 1f));
                }
                totalSprites = 1 + teeth.GetLength(0);
            }

            private float OuterShape(float f)
            {
                return Mathf.Max(1f - f, Mathf.Sin(f * (float)Math.PI));
            }

            public void Update()
            {
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[11];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new TriangleMesh.Triangle(i, i + 1, i + 2);
                }
                sLeaser.sprites[firstSprite] = new TriangleMesh("Futile_White", array, customColor: false);
                for (int j = 0; j < teeth.GetLength(0); j++)
                {
                    sLeaser.sprites[firstSprite + 1 + j] = new FSprite("LizardScaleA6");
                    sLeaser.sprites[firstSprite + 1 + j].anchorY = 0.8f;
                }
            }

            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 headPos, Vector2 headDir, Vector2 headPerp, float headAng, float useFlip)
            {
                float num = Mathf.Lerp(owner.alcedo.lastJawOpen, owner.alcedo.jawOpen, timeStacker);
                Vector2 vector = Custom.DegToVec(headAng + 60f * num * (-1f + 2f * (float)index) * useFlip * Mathf.Pow(Mathf.Abs(useFlip), 0.5f));
                Vector2 vector2 = Custom.PerpendicularVector(vector) * (-1f + 2f * (float)index) * Mathf.Sign(useFlip);
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(0, headPos + headDir * owner.alcedo.Head().rad - camPos);
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(1, headPos + headPerp * (1f - 2f * (float)index) * owner.alcedo.Head().rad * useFlip - camPos);
                float num2 = 65f * Mathf.Lerp(Mathf.Lerp(1f, 0.6f, num), 1f, Mathf.Abs(useFlip));
                float num3 = Mathf.Lerp(5f, 7f, num) * Mathf.Lerp(0.5f, 1.5f, owner.beakFatness);
                for (int i = 1; i < 6; i++)
                {
                    float num4 = (float)i / 6f;
                    Vector2 vector3 = headPos + headDir * owner.alcedo.Head().rad * (1f - num4) * num + vector * num2 * num4;
                    (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 2, vector3 + vector2 * num3 * Mathf.Lerp(0.6f, 1f, num) * OuterShape(num4) * Mathf.Pow(1f - Mathf.Abs(useFlip), 0.4f) - camPos);
                    (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(i * 2 + 1, vector3 - vector2 * num3 * OuterShape(num4) * Mathf.Abs(useFlip) - camPos);
                }
                (sLeaser.sprites[firstSprite] as TriangleMesh).MoveVertice(12, headPos + vector * num2 + vector2 * num3 * 0.75f * num - camPos);
                for (int j = 0; j < teeth.GetLength(0); j++)
                {
                    Vector2 vector4 = headPos + headDir * owner.alcedo.Head().rad * (1f - teeth[j, 0]) * num + vector * num2 * teeth[j, 0];
                    sLeaser.sprites[firstSprite + 1 + j].x = vector4.x - camPos.x;
                    sLeaser.sprites[firstSprite + 1 + j].y = vector4.y - camPos.y;
                    sLeaser.sprites[firstSprite + 1 + j].rotation = Custom.VecToDeg((-vector2 - vector * teeth[j, 2]).normalized);
                    sLeaser.sprites[firstSprite + 1 + j].scaleY = 0.5f * OuterShape(teeth[j, 0]) * Mathf.Abs(useFlip) * Mathf.InverseLerp(0f, 0.1f, num) * teeth[j, 1];
                    sLeaser.sprites[firstSprite + 1 + j].scaleX = 0.4f * Mathf.InverseLerp(0f, 0.1f, num) * teeth[j, 1];
                }
            }
        }

        public AlcedoFeather[,,] wings;

        public int featherLayersPerWing;
        public int feathersPerLayer;

        public AlcedoAppendage[][] appendages;

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

        public EyeTrail eyeTrail;

        public float beakFatness;

        public BeakGraphic[] beak;

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

        private int FeatherSprites => featherLayersPerWing * feathersPerLayer * 2 * alcedo.tentacles.Length;

        private int ClawSpriteStart => 2 + alcedo.tentacles.Length + FeatherSprites;
        private int ClawSpriteLength => alcedo.tentacles.Length * 2;

        private int kngtskSprCount
        {
            get
            {/*
                if (alcedo.kingTusks == null)
                {*/
                return 0;/*
                }
                return KingTusks.Tusk.TotalSprites;*/
            }
        }

        private int BackShieldSpriteLength => 2;

        private int BodySprite => ClawSpriteStart + ClawSpriteLength + BackShieldSpriteLength;

        public int FirstKingTuskSpriteBehind => BodySprite + 5;

        private int NeckSprite => BodySprite + (IsKing ? (5 + kngtskSprCount) : 3);

        private int HeadSprite => NeckSprite + ((!IsKing) ? 1 : 5);

        private int EyesSprite
        {
            get
            {
                if (!IsMiros)
                {
                    return HeadSprite + 3;
                }
                return LastBeakSprite() + 1;
            }
        }

        private int MaskSprite => HeadSprite + 4;

        private int MaskArrowSprite => HeadSprite + 5;

        public int FirstKingTuskSpriteFront => HeadSprite + 6;

        private int TotalSprites
        {
            get
            {
                if (!IsMiros)
                {
                    return HeadSprite + (IsKing ? (6 + kngtskSprCount) : 5);
                }
                return 4 + LastBeakSprite();
            }
        }

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

        public Vector2 AppendageConnectionPos(int app, float timeStacker)
        {
            return Vector2.Lerp(Vector2.Lerp(alcedo.bodyChunks[0].lastPos, alcedo.bodyChunks[2 + app].lastPos, 0.45f), Vector2.Lerp(alcedo.bodyChunks[0].pos, alcedo.bodyChunks[2 + app].pos, 0.45f), timeStacker);
        }

        private int AppendageSprite(int i)
        {
            return i;
        }

        private int TentacleSprite(int i)
        {
            return 2 + i;
        }

        private int FeatherSprite(int w, int l, int i)
        {
            return 2 + alcedo.tentacles.Length + featherLayersPerWing * feathersPerLayer * w * 2 + feathersPerLayer * l * 2 + i * 2;
        }

        private int FeatherColorSprite(int w, int l, int i)
        {
            return 2 + alcedo.tentacles.Length + featherLayersPerWing * feathersPerLayer * w * 2 + feathersPerLayer * l * 2 + i * 2 + 1;
        }

        private int BackShieldSprite(int i)
        {
            return ClawSpriteStart + ClawSpriteLength + i;
        }

        private int FrontShieldSprite(int i)
        {
            return BodySprite + 1 + i;
        }

        public int TuskWireSprite(int side)
        {
            return BodySprite + 3 + side;
        }

        private int TubeSprite(int s)
        {
            return NeckSprite + 1 + s;
        }

        public int NeckLumpSprite(int s)
        {
            return NeckSprite + 3 + s;
        }

        private int TuskSprite(int i)
        {
            return HeadSprite + 1 + i;
        }

        private int ClawSprite(int i)
        {
            return ClawSpriteStart + i * 2;
        }
        private int ClawColorSprite(int i)
        {
            return ClawSpriteStart + i * 2 + 1;
        }

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
            if (IsMiros)
            {
                feathersPerLayer = UnityEngine.Random.Range(6, 8);
            }
            else
            {
                feathersPerLayer = UnityEngine.Random.Range(IsKing ? 25 : 21, IsKing ? 35 : 25);//UnityEngine.Random.Range(IsKing ? 15 : 13, IsKing ? 25 : 20);
            }
            colorWaves = new List<WingColorWave>();
            float num = ((UnityEngine.Random.value < 0.5f) ? 40f : Mathf.Lerp(8f, 15f, UnityEngine.Random.value));
            float num2 = ((UnityEngine.Random.value < 0.5f) ? 40f : Mathf.Lerp(8f, 15f, UnityEngine.Random.value));
            float num3 = ((UnityEngine.Random.value < 0.5f) ? 20f : Mathf.Lerp(3f, 6f, UnityEngine.Random.value));
            if (IsMiros)
            {
                beakFatness = UnityEngine.Random.value;
                beak = new BeakGraphic[2];
                int num4 = FirstBeakSprite();
                for (int i = 0; i < 2; i++)
                {
                    beak[i] = new BeakGraphic(this, i, num4);
                    num4 += beak[i].totalSprites;
                }
                laserColor = new Color(1f, 0.9f, 0f);
                lastLaserColor = laserColor;
            }
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
            appendages = new AlcedoAppendage[2][];
            for (int l = 0; l < 2; l++)
            {
                int num6 = UnityEngine.Random.Range(4, 5);
                //int num6 = (IsKing ? (14 - UnityEngine.Random.Range(0, UnityEngine.Random.Range(2, 12))) : UnityEngine.Random.Range(2, 12));
                appendages[l] = new AlcedoAppendage[num6];
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
                    appendages[l][m] = new AlcedoAppendage(this, l, m, num7 * num8, (m == 0) ? 15f : 4f);
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
            if (IsMiros)
            {
                eyeSize = Mathf.Lerp(0.6f, 0.6f, UnityEngine.Random.value);
                eyeCol = Custom.HSL2RGB(Mathf.Lerp(0.08f, 0.17f, UnityEngine.Random.value), 1f, 0.5f);
                eyeTrail = new EyeTrail(this, EyeTrailSprite());
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
            shellModes = new float[2, 2];
            bodyParts = new BodyPart[appendages[0].Length + appendages[1].Length];
            for (int num10 = 0; num10 < appendages[0].Length; num10++)
            {
                bodyParts[num10] = appendages[0][num10];
            }
            for (int num11 = 0; num11 < appendages[1].Length; num11++)
            {
                bodyParts[appendages[0].Length + num11] = appendages[1][num11];
            }
            if (IsKing)
            {
                neckTubes = new Vector2[2, 15, 3];
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
            for (int m = 0; m < appendages.Length; m++)
            {
                for (int n = 0; n < appendages[m].Length; n++)
                {
                    appendages[m][n].pos = alcedo.mainBodyChunk.pos;
                    appendages[m][n].lastPos = alcedo.mainBodyChunk.pos;
                    appendages[m][n].vel *= 0f;
                }
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
            if (IsMiros)
            {
                eyeTrail.Reset();
            }
            base.Reset();
        }

        public override void Update()
        {
            base.Update();
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
            for (int k = 0; k < appendages.Length; k++)
            {
                for (int l = 0; l < appendages[k].Length; l++)
                {
                    appendages[k][l].Update();
                }
            }
            if (IsMiros)
            {
                for (int m = 0; m < 2; m++)
                {
                    beak[m].Update();
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
            if (IsMiros)
            {
                eyeTrail.Update();
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
            if (IsMiros)
            {
                sLeaser.sprites[LaserSprite()] = new CustomFSprite("Futile_White");
                sLeaser.sprites[LaserSprite()].shader = rCam.game.rainWorld.Shaders["HologramBehindTerrain"];
            }
            for (int i = 0; i < 2; i++)
            {
                //前爪
                sLeaser.sprites[ClawSprite(i)] = new FSprite("JetFishFlipper1");
                sLeaser.sprites[ClawColorSprite(i)] = new FSprite("AlcedoClawColorA");
                //肩膀（已隐藏）
                sLeaser.sprites[BackShieldSprite(i)] = new FSprite("KrakenShield0");
                sLeaser.sprites[FrontShieldSprite(i)] = new FSprite("KrakenShield0");
                sLeaser.sprites[BackShieldSprite(i)].isVisible = false;
                sLeaser.sprites[FrontShieldSprite(i)].isVisible = false;
                //后爪
                sLeaser.sprites[AppendageSprite(i)] = TriangleMesh.MakeLongMesh(appendages[i].Length, pointyTip: false, customColor: true);
                if (!IsMiros)
                {
                    sLeaser.sprites[TuskSprite(i)] = new FSprite("pixel");
                    sLeaser.sprites[TuskSprite(i)].anchorY = 1f;
                    sLeaser.sprites[TuskSprite(i)].isVisible = false;
                }
                if (i == 1)
                {
                    sLeaser.sprites[ClawSprite(i)].scaleX = (IsKing ? (-1.15f) : (-1f));
                    sLeaser.sprites[ClawColorSprite(i)].scaleX = (IsKing ? (-1.15f) : (-1f));
                    sLeaser.sprites[BackShieldSprite(i)].scaleX = (IsKing ? (-1.15f) : (-1f));
                    sLeaser.sprites[FrontShieldSprite(i)].scaleX = (IsKing ? (-1.15f) : (-1f));
                }
                else
                {
                    sLeaser.sprites[ClawSprite(i)].scaleX = (IsKing ? 1.15f : 1f);
                    sLeaser.sprites[ClawColorSprite(i)].scaleX = (IsKing ? 1.15f : 1f);
                    sLeaser.sprites[BackShieldSprite(i)].scaleX = (IsKing ? 1.15f : 1f);
                    sLeaser.sprites[FrontShieldSprite(i)].scaleX = (IsKing ? 1.15f : 1f);
                }
                sLeaser.sprites[ClawSprite(i)].anchorX = 1f;
                sLeaser.sprites[ClawColorSprite(i)].anchorX = 1f;
                sLeaser.sprites[BackShieldSprite(i)].anchorY = 1f;
                sLeaser.sprites[FrontShieldSprite(i)].anchorY = 1f;
                if (IsKing)
                {
                    sLeaser.sprites[TubeSprite(i)] = TriangleMesh.MakeLongMesh(neckTubes.GetLength(1), pointyTip: false, customColor: false);
                }
            }
            for (int j = 0; j < alcedo.tentacles.Length; j++)
            {
                sLeaser.sprites[TentacleSprite(j)] = TriangleMesh.MakeLongMesh(alcedo.tentacles[j].tChunks.Length, pointyTip: false, customColor: true);
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
                            sLeaser.sprites[FeatherSprite(k, x, l)] = new FSprite("MirosClaw");
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
            if (IsMiros)
            {
                for (int m = 0; m < 2; m++)
                {
                    beak[m].InitiateSprites(sLeaser, rCam);
                }
            }
            sLeaser.sprites[BodySprite] = new FSprite("KrakenBody");
            sLeaser.sprites[BodySprite].scale = (IsKing ? 1.2f : 1f) * 0.7f;
            sLeaser.sprites[NeckSprite] = TriangleMesh.MakeLongMesh(alcedo.neck.tChunks.Length, pointyTip: false, customColor: false);
            sLeaser.sprites[HeadSprite] = new FSprite("AlcedoHeadA0");
            sLeaser.sprites[HeadSprite].anchorX = 0.5f;
            sLeaser.sprites[HeadSprite].anchorY = 0.5f;
            sLeaser.sprites[EyesSprite] = new FSprite(IsMiros ? "Circle20" : "AlcedoEyesA0");
            sLeaser.sprites[EyesSprite].anchorX = 0.5f;
            sLeaser.sprites[EyesSprite].anchorY = 0.5f;
            if (IsMiros)
            {
                sLeaser.sprites[EyesSprite].scale = 0.3f * eyeSize;
                eyeTrail.InitiateSprites(sLeaser, rCam);
            }
            if (!IsMiros)
            {
                sLeaser.sprites[MaskSprite] = new FSprite("AlcedoMaskA0");
                sLeaser.sprites[MaskSprite].anchorX = 0.5f;
                sLeaser.sprites[MaskSprite].anchorY = 0.5f;
                if (IsKing)
                {
                    sLeaser.sprites[MaskArrowSprite] = new FSprite("KrakenArrow0");
                }
            }
            AddToContainer(sLeaser, rCam, null);
            base.InitiateSprites(sLeaser, rCam);
        }

        public bool IsKingTuskSprite(int i)
        {
            if (!IsKing)
            {
                return false;
            }
            if ((i < FirstKingTuskSpriteBehind || i >= FirstKingTuskSpriteBehind + KingTusks.Tusk.TotalSprites) && (i < FirstKingTuskSpriteFront || i >= FirstKingTuskSpriteFront + KingTusks.Tusk.TotalSprites) && i != TuskWireSprite(0))
            {
                return i == TuskWireSprite(1);
            }
            return true;
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
            if (IsMiros)
            {
                for (int i = 0; i < sLeaser.sprites.Length; i++)
                {
                    if (i == EyeTrailSprite())
                    {
                        rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[i]);
                    }
                    else if (i >= FirstBeakSprite() && i <= LastBeakSprite())
                    {
                        rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[i]);
                    }
                    else
                    {
                        newContatiner.AddChild(sLeaser.sprites[i]);
                    }
                }
                if (sLeaser.containers != null)
                {
                    FContainer[] containers = sLeaser.containers;
                    foreach (FContainer node in containers)
                    {
                        newContatiner.AddChild(node);
                    }
                }
                return;
            }
            for (int k = 0; k < sLeaser.sprites.Length; k++)
            {
                sLeaser.sprites[k].RemoveFromContainer();
                if (IsKingTuskSprite(k))
                {
                    //alcedo.kingTusks.AddToContainer(this, k, sLeaser, rCam, newContatiner);
                }
                else
                {
                    newContatiner.AddChild(sLeaser.sprites[k]);
                }
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            this.palette = palette;
            ExitShadowMode(sLeaser, rCam, changeContainer: false);
            base.ApplyPalette(sLeaser, rCam, palette);
        }

        private void EnterShadowMode(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, bool changeContainer)
        {
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
            }
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
            }
            Color color3 = Color.Lerp(ColorA.rgb, new Color(1f, 1f, 1f), 0.35f);
            if (!IsMiros)
            {
                sLeaser.sprites[MaskSprite].color = Color.Lerp(color3, palette.blackColor, t);
            }
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
            }
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
            spritesInShadowMode = sLeaser.sprites[BodySprite].color == new Color(0.003921569f, 0f, 0f);
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
            }/*
            if (alcedo.kingTusks != null)
            {
                alcedo.kingTusks.DrawSprites(this, sLeaser, rCam, timeStacker, camPos);
            }*/
            Vector2 p = Vector2.Lerp(base.owner.bodyChunks[1].lastPos, base.owner.bodyChunks[1].pos, timeStacker);
            Vector2 p2 = Vector2.Lerp(base.owner.bodyChunks[0].lastPos, base.owner.bodyChunks[0].pos, timeStacker);
            if (IsMiros)
            {
                float useFlip = Mathf.Lerp(lastHeadFlip, headFlip, timeStacker);
                Vector2 vector = Vector2.Lerp(alcedo.Head().lastPos, alcedo.Head().pos, timeStacker);
                Vector2 vector2 = Custom.DirVec(Vector2.Lerp(alcedo.neck.Tip.lastPos, alcedo.neck.Tip.pos, timeStacker), vector);
                Vector2 headPerp = Custom.PerpendicularVector(vector2);
                float headAng = Custom.VecToDeg(vector2);
                for (int i = 0; i < 2; i++)
                {
                    beak[i].DrawSprites(sLeaser, rCam, timeStacker, camPos, vector, vector2, headPerp, headAng, useFlip);
                }
                float num = Mathf.Lerp(lastLaserActive, laserActive, timeStacker);
                Color col = Color.Lerp(lastLaserColor, laserColor, timeStacker);
                float t = Mathf.Lerp(lastFlash, flash, timeStacker);
                if (num <= 0f)
                {
                    sLeaser.sprites[LaserSprite()].isVisible = false;
                }
                else
                {
                    vector2 *= -1f;
                    sLeaser.sprites[LaserSprite()].isVisible = true;
                    sLeaser.sprites[LaserSprite()].alpha = num;
                    Vector2 corner = Custom.RectCollision(vector, vector - vector2 * 100000f, rCam.room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
                    IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(rCam.room, vector, corner);
                    if (intVector.HasValue)
                    {
                        corner = Custom.RectCollision(corner, vector, rCam.room.TileRect(intVector.Value)).GetCorner(FloatRect.CornerLabel.D);
                    }
                    (sLeaser.sprites[LaserSprite()] as CustomFSprite).verticeColors[0] = Custom.RGB2RGBA(col, num);
                    (sLeaser.sprites[LaserSprite()] as CustomFSprite).verticeColors[1] = Custom.RGB2RGBA(col, num);
                    (sLeaser.sprites[LaserSprite()] as CustomFSprite).verticeColors[2] = Custom.RGB2RGBA(col, Mathf.Pow(num, 2f) * Mathf.Lerp(0.5f, 1f, t));
                    (sLeaser.sprites[LaserSprite()] as CustomFSprite).verticeColors[3] = Custom.RGB2RGBA(col, Mathf.Pow(num, 2f) * Mathf.Lerp(0.5f, 1f, t));
                    (sLeaser.sprites[LaserSprite()] as CustomFSprite).MoveVertice(0, vector - vector2 * 4f + Custom.PerpendicularVector(vector2) * 0.5f - camPos);
                    (sLeaser.sprites[LaserSprite()] as CustomFSprite).MoveVertice(1, vector - vector2 * 4f - Custom.PerpendicularVector(vector2) * 0.5f - camPos);
                    (sLeaser.sprites[LaserSprite()] as CustomFSprite).MoveVertice(2, corner - Custom.PerpendicularVector(vector2) * 0.5f - camPos);
                    (sLeaser.sprites[LaserSprite()] as CustomFSprite).MoveVertice(3, corner + Custom.PerpendicularVector(vector2) * 0.5f - camPos);
                }
            }
            sLeaser.sprites[BodySprite].x = Mathf.Lerp(p.x, p2.x, 0.5f) - camPos.x;
            sLeaser.sprites[BodySprite].y = Mathf.Lerp(p.y, p2.y, 0.5f) - camPos.y;
            sLeaser.sprites[BodySprite].rotation = Custom.AimFromOneVectorToAnother(p2, p);
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
            if (!IsMiros)
            {
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
                sLeaser.sprites[EyesSprite].scaleY = (IsKing ? 1.15f : 1f);
            }
            else
            {
                sLeaser.sprites[EyesSprite].scaleX = eyeSize;
                sLeaser.sprites[EyesSprite].scaleY = eyeSize;
                sLeaser.sprites[EyesSprite].isVisible = true;
                eyeTrail.UpdatePosition(headPos);
            }
            if (IsKing)
            {
                sLeaser.sprites[MaskArrowSprite].x = headPos.x - camPos.x;
                sLeaser.sprites[MaskArrowSprite].y = headPos.y - camPos.y;
                sLeaser.sprites[MaskArrowSprite].rotation = neckToHeadAngle - num3;
                sLeaser.sprites[MaskArrowSprite].element = Futile.atlasManager.GetElementWithName("KrakenArrow" + headGraphic);
                sLeaser.sprites[MaskArrowSprite].scaleX = ((neckToHeadAngle > 0f) ? (-1f) : 1f) * (IsKing ? 1.15f : 1f);
                sLeaser.sprites[MaskArrowSprite].scaleY = (IsKing ? 1.15f : 1f);
                sLeaser.sprites[MaskArrowSprite].isVisible = (alcedo.State as Alcedo.AlcedoState).mask;
            }
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
                /*if (j == alcedo.neck.tChunks.Length - 1 && !IsMiros)
                {
                    segLength = 0f;
                }*/
                (sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(j * 4 + 2, neckPos - neckPerp * alcedo.neck.tChunks[j].stretchedRad - normalized * segLength - camPos);
                (sLeaser.sprites[NeckSprite] as TriangleMesh).MoveVertice(j * 4 + 3, neckPos + neckPerp * alcedo.neck.tChunks[j].stretchedRad - normalized * segLength - camPos);
                lastNeckStretchedRad = alcedo.neck.tChunks[j].stretchedRad;
                lastNeckPos = neckPos;
            }
            for (int k = 0; k < alcedo.tentacles.Length; k++)
            {
                if (IsKing)
                {
                    lastNeckPos = Vector2.Lerp(alcedo.bodyChunks[4].lastPos, alcedo.bodyChunks[4].pos, timeStacker);
                    lastNeckStretchedRad = 0f;
                    for (int l = 0; l < neckTubes.GetLength(1); l++)
                    {
                        Vector2 vector7 = Vector2.Lerp(neckTubes[k, l, 1], neckTubes[k, l, 0], timeStacker);
                        if (l == neckTubes.GetLength(1) - 1)
                        {
                            vector7 = Vector2.Lerp(alcedo.bodyChunks[0].lastPos, alcedo.bodyChunks[0].pos, timeStacker);
                        }
                        Vector2 normalized2 = (vector7 - lastNeckPos).normalized;
                        Vector2 vector8 = Custom.PerpendicularVector(normalized2);
                        float num6 = Vector2.Distance(vector7, lastNeckPos) / 5f;
                        float num7 = ((l % 3 == 0) ? 2.5f : 1.5f);
                        (sLeaser.sprites[TubeSprite(k)] as TriangleMesh).MoveVertice(l * 4, lastNeckPos - vector8 * (num7 + lastNeckStretchedRad) * 0.5f + normalized2 * num6 - camPos);
                        (sLeaser.sprites[TubeSprite(k)] as TriangleMesh).MoveVertice(l * 4 + 1, lastNeckPos + vector8 * (num7 + lastNeckStretchedRad) * 0.5f + normalized2 * num6 - camPos);
                        (sLeaser.sprites[TubeSprite(k)] as TriangleMesh).MoveVertice(l * 4 + 2, vector7 - vector8 * num7 - normalized2 * num6 - camPos);
                        (sLeaser.sprites[TubeSprite(k)] as TriangleMesh).MoveVertice(l * 4 + 3, vector7 + vector8 * num7 - normalized2 * num6 - camPos);
                        lastNeckStretchedRad = num7;
                        lastNeckPos = vector7;
                    }
                }
                Vector2 lastTentaclePos = Vector2.Lerp(alcedo.tentacles[k].connectedChunk.lastPos, alcedo.tentacles[k].connectedChunk.pos, timeStacker);
                float lastTentacleRad = 10f;
                for (int m = 0; m < alcedo.tentacles[k].tChunks.Length; m++)
                {
                    Vector2 tentaclePos = Vector2.Lerp(alcedo.tentacles[k].tChunks[m].lastPos, alcedo.tentacles[k].tChunks[m].pos, timeStacker);
                    Vector2 dir = (tentaclePos - lastTentaclePos).normalized;
                    Vector2 vector10 = Custom.PerpendicularVector(dir);
                    float segLength = Vector2.Distance(tentaclePos, lastTentaclePos) / 5f;
                    float tentacleRad = alcedo.tentacles[k].TentacleContour(((float)m + 0.5f) / (float)alcedo.tentacles[k].tChunks.Length);
                    tentacleRad *= Mathf.Clamp(Mathf.Pow(alcedo.tentacles[k].tChunks[m].stretchedFac, 0.35f), 0.5f, 1.5f);
                    (sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).MoveVertice(m * 4, lastTentaclePos - vector10 * tentacleRad + dir * segLength - camPos);
                    (sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 1, lastTentaclePos + vector10 * tentacleRad + dir * segLength - camPos);
                    tentacleRad = alcedo.tentacles[k].TentacleContour(((float)m + 1f) / (float)alcedo.tentacles[k].tChunks.Length);
                    tentacleRad *= Mathf.Clamp(Mathf.Pow(alcedo.tentacles[k].tChunks[m].stretchedFac, 0.35f), 0.5f, 1.5f);
                    (sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 2, tentaclePos - vector10 * tentacleRad - dir * segLength - camPos);
                    (sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).MoveVertice(m * 4 + 3, tentaclePos + vector10 * tentacleRad - dir * segLength - camPos);
                    //(sLeaser.sprites[TentacleSprite(k)] as TriangleMesh).color = Color.white;
                    lastTentacleRad = tentacleRad;
                    lastTentaclePos = tentaclePos;
                }
                int halfWingIndex = alcedo.tentacles[k].halfWingIndex;
                Vector2 halfWingPos = Vector2.Lerp(alcedo.tentacles[k].tChunks[halfWingIndex].lastPos, alcedo.tentacles[k].tChunks[halfWingIndex].pos, timeStacker);
                Vector2 nextHalfWingPos = Vector2.Lerp(alcedo.tentacles[k].tChunks[halfWingIndex - 1].lastPos, alcedo.tentacles[k].tChunks[halfWingIndex - 1].pos, timeStacker);
                sLeaser.sprites[ClawSprite(k)].x = halfWingPos.x - camPos.x;
                sLeaser.sprites[ClawSprite(k)].y = halfWingPos.y - camPos.y;
                sLeaser.sprites[ClawSprite(k)].rotation = Custom.AimFromOneVectorToAnother(halfWingPos, nextHalfWingPos) * Mathf.Lerp(0.9f, 1f, this.alcedo.tentacles[k].flyingMode)
                    - Mathf.Lerp(-10f, 60f, this.alcedo.tentacles[k].flyingMode) * (k % 2 == 0 ? 1f : -1f);
                sLeaser.sprites[ClawColorSprite(k)].x = sLeaser.sprites[ClawSprite(k)].x;
                sLeaser.sprites[ClawColorSprite(k)].y = sLeaser.sprites[ClawSprite(k)].y;
                sLeaser.sprites[ClawColorSprite(k)].rotation = sLeaser.sprites[ClawSprite(k)].rotation;
                sLeaser.sprites[ClawSprite(k)].color = sLeaser.sprites[BodySprite].color;
                sLeaser.sprites[ClawColorSprite(k)].color = Color.Lerp(wings[k, 0, 0].CurrentColor(), palette.blackColor, (ModManager.MMF && !IsMiros) ? darkness : 0f); ;
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
                        else if (IsMiros)
                        {
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].isVisible = false;
                        }
                        if (!shadowMode)
                        {
                            sLeaser.sprites[FeatherColorSprite(k, l, n)].color = Color.Lerp(wings[k, l, n].CurrentColor(), palette.blackColor, (ModManager.MMF && !IsMiros) ? darkness : 0f);
                        }
                    }
            }
            for (int num10 = 0; num10 < 2; num10++)
            {
                lastNeckPos = AppendageConnectionPos(num10, timeStacker);
                lastNeckStretchedRad = 10f;
                for (int num11 = 0; num11 < appendages[num10].Length; num11++)
                {
                    Vector2 vector11 = Vector2.Lerp(appendages[num10][num11].lastPos, appendages[num10][num11].pos, timeStacker);
                    Vector2 normalized4 = (vector11 - lastNeckPos).normalized;
                    Vector2 vector12 = Custom.PerpendicularVector(normalized4);
                    float num12 = Vector2.Distance(vector11, lastNeckPos) / 5f;
                    (sLeaser.sprites[AppendageSprite(num10)] as TriangleMesh).MoveVertice(num11 * 4, lastNeckPos - vector12 * (appendages[num10][num11].stretchedRad + lastNeckStretchedRad) * 0.5f + normalized4 * num12 - camPos);
                    (sLeaser.sprites[AppendageSprite(num10)] as TriangleMesh).MoveVertice(num11 * 4 + 1, lastNeckPos + vector12 * (appendages[num10][num11].stretchedRad + lastNeckStretchedRad) * 0.5f + normalized4 * num12 - camPos);
                    (sLeaser.sprites[AppendageSprite(num10)] as TriangleMesh).MoveVertice(num11 * 4 + 2, vector11 - vector12 * appendages[num10][num11].stretchedRad - normalized4 * num12 - camPos);
                    (sLeaser.sprites[AppendageSprite(num10)] as TriangleMesh).MoveVertice(num11 * 4 + 3, vector11 + vector12 * appendages[num10][num11].stretchedRad - normalized4 * num12 - camPos);
                    lastNeckStretchedRad = appendages[num10][num11].stretchedRad;
                    lastNeckPos = vector11;
                }
            }
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
            }
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
            }
            if (IsMiros)
            {
                if (shadowMode)
                {
                    eyeTrail.visible = false;
                }
                else
                {
                    eyeTrail.visible = true;
                }
                eyeTrail.DrawSprite(sLeaser, rCam, timeStacker, camPos);
            }
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

        public int EyeTrailSprite()
        {
            return 2 + LastBeakSprite();
        }

        private int FirstBeakSprite()
        {
            return 2 + alcedo.tentacles.Length + FeatherSprites + 7;
        }

        private int LastBeakSprite()
        {
            return FirstBeakSprite() + beak[0].totalSprites + beak[1].totalSprites - 1;
        }

        public int LaserSprite()
        {
            return 3 + LastBeakSprite();
        }
    }
}
