using RWCustom;
using System;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature
{
    internal class AlcedoFeather : BodyPart
    {
        public float extendedFac
        {
            get
            {
                return this.ef;
            }
            set
            {
                this.ef = Mathf.Clamp(value, 0f, 1f);
            }
        }
        public float CurrentLength
        {
            get
            {
                return Mathf.Lerp(this.contractedLength, this.extendedLength, this.extendedFac) * ModifyFeatherContourFac();
            }
        }
        public Tentacle.TentacleChunk PreviousPreviousChunk
        {
            get
            {
                return this.wing.tChunks[Custom.IntClamp(Mathf.FloorToInt(this.wingPosition * (float)this.wing.tChunks.Length) - 1, 0, this.wing.tChunks.Length - 1)];
            }
        }
        public Tentacle.TentacleChunk PreviousChunk
        {
            get
            {
                return this.wing.tChunks[Custom.IntClamp(Mathf.FloorToInt(this.wingPosition * (float)this.wing.tChunks.Length), 0, this.wing.tChunks.Length - 1)];
            }
        }
        public Tentacle.TentacleChunk NextChunk
        {
            get
            {
                return this.wing.tChunks[Custom.IntClamp(Mathf.FloorToInt(this.wingPosition * (float)this.wing.tChunks.Length) + 1, 0, this.wing.tChunks.Length - 1)];
            }
        }
        public float BetweenChunksLerp
        {
            get
            {
                return this.wingPosition * (float)this.wing.tChunks.Length - Mathf.Floor(this.wingPosition * (float)this.wing.tChunks.Length);
            }
        }
        public Vector2 ConnectedPos
        {
            get
            {
                return Vector2.Lerp(this.PreviousChunk.pos, this.NextChunk.pos, this.BetweenChunksLerp);
            }
        }
        public Vector2 ConnectedLastPos
        {
            get
            {
                return Vector2.Lerp(this.PreviousChunk.lastPos, this.NextChunk.lastPos, this.BetweenChunksLerp);
            }
        }

        public AlcedoFeather(AlcedoGraphics kGraphics, AlcedoTentacle wing, float wingPosition, float contractedLength, float extendedLength, float width, string type) : base(kGraphics)
        {
            this.kGraphics = kGraphics;
            this.wing = wing;
            this.wingPosition = wingPosition;
            this.contractedLength = contractedLength;
            this.extendedLength = extendedLength;
            this.width = width;
            this.lose = 0f;
            this.type = type;
        }

        //根据翅膀拍动来调整长度
        public float ModifyFeatherContourFac()
        {
            float num = 1f;
            if (this.wing.mode == AlcedoTentacle.Mode.Fly)
            {
                num = Mathf.Lerp(0.5f + 0.5f * Mathf.Sin((float)Math.PI * 2f * (this.wing.alcedo.wingFlap - wingPosition * 0.5f)), 1f, 0.5f);
                num = Mathf.Lerp(1f, num, this.wing.flyingMode);
            }
            return num;
        }

        public override void Update()
        {
            base.Update();
            if (this.kGraphics.owner.room.PointSubmerged(this.pos))
            {
                this.vel *= 0.1f;
            }
            this.lastPos = this.pos;
            this.pos += this.vel;
            this.vel *= 0.7f;
            Vector2 normalized = Vector2.Lerp(this.PreviousChunk.pos - this.PreviousPreviousChunk.pos,
                                              this.NextChunk.pos - this.PreviousChunk.pos,
                                              (this.PreviousPreviousChunk == this.PreviousChunk) ? 1f : this.BetweenChunksLerp).normalized;
            Vector2 a = Vector2.Lerp(Custom.PerpendicularVector(normalized) * (this.kGraphics.alcedo.IsMiros ? this.GetTentacleAngle(this.wing.tentacleNumber) : ((this.wing.tentacleNumber == 1) ? -1f : 1f)),
                (this.kGraphics.alcedo.bodyChunks[5].pos - this.kGraphics.alcedo.bodyChunks[0].pos).normalized,
                1f - this.wingPosition);
            float d = Mathf.Lerp(Mathf.Lerp(1f,
                                            Mathf.Lerp(-0.2f, //-0.9f,//翅膀折叠，此时翅尖翅根距离为翅长的1/2
                                                        1.2f, //1.5f,//翅膀展开
                                                       Mathf.InverseLerp(this.wing.idealLength * 0.5f, this.wing.idealLength, Vector2.Distance(this.wing.FloatBase, this.wing.Tip.pos))), //翅膀折叠程度
                                            this.wingPosition), //翅根处几乎不变，翅尖变化剧烈
                                 Mathf.Lerp(-0.5f, 4f, this.wingPosition),  //翅根处几乎不变且反向，翅尖变化剧烈  Mathf.Lerp(-0.5f, 4f, this.wingPosition),
                                 this.extendedFac);//羽毛不伸展时不变，伸展程度越大变化越剧烈
            Vector2 a2 = this.ConnectedPos + (a * (1f - this.wingPosition) + normalized * d).normalized * this.CurrentLength;
            //Vector2 a2 = this.ConnectedPos + (a + normalized * d).normalized * this.CurrentLength;
            this.vel += (a2 - this.pos) * Mathf.Lerp(0.3f, 0.8f, this.wing.flyingMode) * (1f - this.lose);
            if (this.wing.flyingMode > this.extendedFac)
            {
                this.extendedFac += 1f / Mathf.Lerp(10f, 40f, UnityEngine.Random.value);
            }
            else if (this.wing.flyingMode < this.extendedFac)
            {
                if (this.extendedFac == 1f)
                {
                    this.contractMode = (this.kGraphics.alcedo.IsMiros ? AlcedoFeather.ContractMode.Jerky : AlcedoFeather.ContractMode.Even);
                    this.contractSpeed = 1f / Mathf.Lerp(20f, 800f, UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
                    if (UnityEngine.Random.value < 0.3f)
                    {
                        if (UnityEngine.Random.value < 0.7f)
                        {
                            this.contractMode = AlcedoFeather.ContractMode.Jerky;
                        }
                        else
                        {
                            this.contractMode = AlcedoFeather.ContractMode.Jammed;
                        }
                    }
                }
                if (this.contractMode != AlcedoFeather.ContractMode.Even && this.extendedFac > 0.5f)
                {
                    this.extendedFac -= 0.008333334f;
                }
                switch (this.contractMode)
                {
                    case AlcedoFeather.ContractMode.Even:
                        this.extendedFac -= this.contractSpeed;
                        break;
                    case AlcedoFeather.ContractMode.Jerky:
                        if (UnityEngine.Random.value < 0.0016666667f)
                        {
                            this.extendedFac -= 1f / Mathf.Lerp(4f, 30f, UnityEngine.Random.value);
                        }
                        break;
                    case AlcedoFeather.ContractMode.Jammed:
                        if (UnityEngine.Random.value < 0.0007142857f)
                        {
                            this.contractMode = AlcedoFeather.ContractMode.Jerky;
                        }
                        break;
                }
            }
            this.lightnessBonus = Mathf.Max(this.lightnessBonus - 0.1f, 0f);
            if (this.lightnessBonus == 0f)
            {
                this.saturationBonus = Mathf.Max(this.saturationBonus - 0.02f, 0f);
            }
            this.forcedAlpha = Mathf.Lerp(this.forcedAlpha, 0f, 0.05f);
            base.ConnectToPoint(this.ConnectedPos, this.CurrentLength, true, 0f, this.PreviousChunk.vel, 0.3f, 0f);
            if (this.terrainContact)
            {
                this.terrainContactTimer++;
            }
            else
            {
                this.terrainContactTimer = 0;
            }
            Vector2 vel = this.vel;
            base.PushOutOfTerrain(this.kGraphics.alcedo.room, this.ConnectedPos);
            if (this.terrainContact && this.terrainContactTimer > 4)
            {
                if (this.kGraphics.alcedo.IsMiros)
                {
                    this.kGraphics.alcedo.room.PlaySound((UnityEngine.Random.value < 0.5f) ? SoundID.Spear_Fragment_Bounce : SoundID.Spear_Bounce_Off_Wall, this.pos, Mathf.InverseLerp(10f, 60f, vel.magnitude), Mathf.Lerp(3.5f, 0.5f, Mathf.InverseLerp(7f, 70f, this.CurrentLength)));
                }
                else
                {
                    this.kGraphics.alcedo.room.PlaySound(SoundID.Vulture_Feather_Hit_Terrain, this.pos, Mathf.InverseLerp(0.2f, 20f, vel.magnitude), Mathf.Lerp(3.5f, 0.5f, Mathf.InverseLerp(7f, 70f, this.CurrentLength)));
                }
                this.terrainContactTimer = 0;
            }
        }

        public Color CurrentColor()
        {
            if (this.kGraphics.alcedo.IsMiros)
            {
                Color rgb = HSLColor.Lerp(new HSLColor(this.kGraphics.ColorB.hue, Mathf.Lerp(this.kGraphics.ColorB.saturation, 1f, this.saturationBonus), Mathf.Lerp(this.kGraphics.ColorB.lightness, 1f, this.lightnessBonus)),
                                          this.kGraphics.ColorA,
                                          Mathf.Cos(Mathf.Pow(this.wingPosition, 0.75f) * 3.1415927f)).rgb;
                rgb.a = Mathf.Max(new float[]
                {
                    0.4f,
                    this.forcedAlpha,
                    Mathf.Lerp(0.4f, 0.8f, Mathf.Cos(Mathf.Pow(this.wingPosition, 1.7f) * 3.1415927f))
                }) * (this.extendedFac + this.wing.flyingMode) * 0.5f * (1f - this.brokenColor);
                if (this.kGraphics.alcedo.isLaserActive())
                {
                    rgb.a = UnityEngine.Random.value;
                }
                return rgb;
            }
            HSLColor colorB = this.kGraphics.ColorB;
            HSLColor colorA = this.kGraphics.ColorA;
            if (this.type == "Covert")
            {
                colorB.hue -= 0.1f;
                colorA.hue -= 0.1f;
                colorB.hue = (colorB.hue + 1f) % 1f;
                colorA.hue = (colorA.hue + 1f) % 1f;
            }
            if (this.kGraphics.albino)
            {
                colorB.saturation = Mathf.Lerp(colorB.saturation, 1f, 0.2f);
                colorB.hue = 0f;
                colorB.lightness = Mathf.Lerp(colorB.saturation, 0.2f, 0.8f);
                colorA.saturation = 0.8f;
                colorA.lightness = 0.6f;
            }
            Color rgb2 = HSLColor.Lerp(new HSLColor(colorB.hue, Mathf.Lerp(colorB.saturation, 1f, this.saturationBonus), Mathf.Lerp(colorB.lightness, 1f, this.lightnessBonus)),
                                       colorA,
                                       Mathf.Cos(Mathf.Pow(this.wingPosition, 0.75f) * 3.1415927f)).rgb;
            rgb2.a = Mathf.Max(this.forcedAlpha,
                Mathf.Lerp(0.7f, 0.9f, Mathf.Cos(Mathf.Pow(this.wingPosition, 1.7f) * 3.1415927f))) * Mathf.Lerp((this.extendedFac + this.wing.flyingMode) * 0.5f, 1f, 0.5f) * (1f - this.brokenColor);
            /*
            rgb2.a = Mathf.Max(this.forcedAlpha, 
                Mathf.Lerp(0.2f, 0.6f, Mathf.Cos(Mathf.Pow(this.wingPosition, 1.7f) * 3.1415927f))) * (this.extendedFac + this.wing.flyingMode) * 0.5f * (1f - this.brokenColor);
            */
            return rgb2;
        }

        public Color CurrentClawColor()
        {
            HSLColor colorB = this.kGraphics.ColorB;
            HSLColor colorA = this.kGraphics.ColorA;
            Color rgb2 = HSLColor.Lerp(new HSLColor(colorB.hue, Mathf.Lerp(colorB.saturation, 1f, this.saturationBonus), Mathf.Lerp(colorB.lightness, 1f, this.lightnessBonus)),
                                       colorA,
                                       Mathf.Cos(Mathf.Pow(this.wingPosition, 0.75f) * 3.1415927f)).rgb;
            rgb2.a = Mathf.Max(this.forcedAlpha, 0.9f);
            return rgb2;
        }

        public float GetTentacleAngle(int id)
        {
            if (id == 0)
            {
                return 1f;
            }
            if (id == 1)
            {
                return -1f;
            }
            if (id == 2)
            {
                return 4f;
            }
            return -4f;
        }

        public AlcedoGraphics kGraphics;
        public AlcedoTentacle wing;
        public string type;
        public float wingPosition;
        private float ef;
        public float width;
        public float contractedLength;
        public float extendedLength;
        private AlcedoFeather.ContractMode contractMode;
        public float contractSpeed;
        public float lose;
        public float brokenColor;
        public float forcedAlpha;
        public float lightnessBonus;
        public float saturationBonus;
        private int terrainContactTimer;

        private enum ContractMode
        {
            Even,
            Jerky,
            Jammed
        }
    }
}
