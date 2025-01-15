using RWCustom;
using System;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature.Alcedo
{
    internal class AlcedoFeather : BodyPart
    {
        public float extendedFac
        {
            get
            {
                return ef;
            }
            set
            {
                ef = Mathf.Clamp(value, 0f, 1f);
            }
        }
        public float CurrentLength
        {
            get
            {
                return Mathf.Lerp(contractedLength, extendedLength, extendedFac) * ModifyFeatherContourFac();
            }
        }
        public Tentacle.TentacleChunk PreviousPreviousChunk
        {
            get
            {
                return wing.tChunks[Custom.IntClamp(Mathf.FloorToInt(wingPosition * wing.tChunks.Length) - 1, 0, wing.tChunks.Length - 1)];
            }
        }
        public Tentacle.TentacleChunk PreviousChunk
        {
            get
            {
                return wing.tChunks[Custom.IntClamp(Mathf.FloorToInt(wingPosition * wing.tChunks.Length), 0, wing.tChunks.Length - 1)];
            }
        }
        public Tentacle.TentacleChunk NextChunk
        {
            get
            {
                return wing.tChunks[Custom.IntClamp(Mathf.FloorToInt(wingPosition * wing.tChunks.Length) + 1, 0, wing.tChunks.Length - 1)];
            }
        }
        public float BetweenChunksLerp
        {
            get
            {
                return wingPosition * wing.tChunks.Length - Mathf.Floor(wingPosition * wing.tChunks.Length);
            }
        }
        public Vector2 ConnectedPos
        {
            get
            {
                return Vector2.Lerp(PreviousChunk.pos, NextChunk.pos, BetweenChunksLerp);
            }
        }
        public Vector2 ConnectedLastPos
        {
            get
            {
                return Vector2.Lerp(PreviousChunk.lastPos, NextChunk.lastPos, BetweenChunksLerp);
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
            lose = 0f;
            this.type = type;
        }

        //根据翅膀拍动来调整长度
        public float ModifyFeatherContourFac()
        {
            float num = 1f;
            if (wing.mode == AlcedoTentacle.Mode.Fly)
            {
                num = Mathf.Lerp(0.5f + 0.5f * Mathf.Sin((float)Math.PI * 2f * (wing.alcedo.wingFlap - wingPosition * 0.5f)), 1f, 0.5f);
                num = Mathf.Lerp(1f, num, wing.flyingMode);
            }
            return num;
        }

        public override void Update()
        {
            base.Update();
            if (kGraphics.owner.room.PointSubmerged(pos))
            {
                this.vel *= 0.1f;
            }
            lastPos = pos;
            pos += this.vel;
            this.vel *= 0.7f;
            Vector2 normalized = Vector2.Lerp(PreviousChunk.pos - PreviousPreviousChunk.pos,
                                              NextChunk.pos - PreviousChunk.pos,
                                              PreviousPreviousChunk == PreviousChunk ? 1f : BetweenChunksLerp).normalized;
            Vector2 a = Vector2.Lerp(Custom.PerpendicularVector(normalized) * (kGraphics.alcedo.IsMiros ? GetTentacleAngle(wing.tentacleNumber) : wing.tentacleNumber == 1 ? -1f : 1f),
                (kGraphics.alcedo.bodyChunks[5].pos - kGraphics.alcedo.bodyChunks[0].pos).normalized,
                1f - wingPosition);
            float d = Mathf.Lerp(Mathf.Lerp(1f,
                                            Mathf.Lerp(-0.2f, //-0.9f,//翅膀折叠，此时翅尖翅根距离为翅长的1/2
                                                        1.2f, //1.5f,//翅膀展开
                                                       Mathf.InverseLerp(wing.idealLength * 0.5f, wing.idealLength, Vector2.Distance(wing.FloatBase, wing.Tip.pos))), //翅膀折叠程度
                                            wingPosition), //翅根处几乎不变，翅尖变化剧烈
                                 Mathf.Lerp(-0.5f, 4f, wingPosition),  //翅根处几乎不变且反向，翅尖变化剧烈  Mathf.Lerp(-0.5f, 4f, this.wingPosition),
                                 extendedFac);//羽毛不伸展时不变，伸展程度越大变化越剧烈
            Vector2 a2 = ConnectedPos + (a * (1f - wingPosition) + normalized * d).normalized * CurrentLength;
            //Vector2 a2 = this.ConnectedPos + (a + normalized * d).normalized * this.CurrentLength;
            this.vel += (a2 - pos) * Mathf.Lerp(0.3f, 0.8f, wing.flyingMode) * (1f - lose);
            if (wing.flyingMode > extendedFac)
            {
                extendedFac += 1f / Mathf.Lerp(10f, 40f, UnityEngine.Random.value);
            }
            else if (wing.flyingMode < extendedFac)
            {
                if (extendedFac == 1f)
                {
                    contractMode = kGraphics.alcedo.IsMiros ? ContractMode.Jerky : ContractMode.Even;
                    contractSpeed = 1f / Mathf.Lerp(20f, 800f, UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value);
                    if (UnityEngine.Random.value < 0.3f)
                    {
                        if (UnityEngine.Random.value < 0.7f)
                        {
                            contractMode = ContractMode.Jerky;
                        }
                        else
                        {
                            contractMode = ContractMode.Jammed;
                        }
                    }
                }
                if (contractMode != ContractMode.Even && extendedFac > 0.5f)
                {
                    extendedFac -= 0.008333334f;
                }
                switch (contractMode)
                {
                    case ContractMode.Even:
                        extendedFac -= contractSpeed;
                        break;
                    case ContractMode.Jerky:
                        if (UnityEngine.Random.value < 0.0016666667f)
                        {
                            extendedFac -= 1f / Mathf.Lerp(4f, 30f, UnityEngine.Random.value);
                        }
                        break;
                    case ContractMode.Jammed:
                        if (UnityEngine.Random.value < 0.0007142857f)
                        {
                            contractMode = ContractMode.Jerky;
                        }
                        break;
                }
            }
            lightnessBonus = Mathf.Max(lightnessBonus - 0.1f, 0f);
            if (lightnessBonus == 0f)
            {
                saturationBonus = Mathf.Max(saturationBonus - 0.02f, 0f);
            }
            forcedAlpha = Mathf.Lerp(forcedAlpha, 0f, 0.05f);
            ConnectToPoint(ConnectedPos, CurrentLength, true, 0f, PreviousChunk.vel, 0.3f, 0f);
            if (terrainContact)
            {
                terrainContactTimer++;
            }
            else
            {
                terrainContactTimer = 0;
            }
            Vector2 vel = this.vel;
            PushOutOfTerrain(kGraphics.alcedo.room, ConnectedPos);
            if (terrainContact && terrainContactTimer > 4)
            {
                if (kGraphics.alcedo.IsMiros)
                {
                    kGraphics.alcedo.room.PlaySound(UnityEngine.Random.value < 0.5f ? SoundID.Spear_Fragment_Bounce : SoundID.Spear_Bounce_Off_Wall, pos, Mathf.InverseLerp(10f, 60f, vel.magnitude), Mathf.Lerp(3.5f, 0.5f, Mathf.InverseLerp(7f, 70f, CurrentLength)));
                }
                else
                {
                    kGraphics.alcedo.room.PlaySound(SoundID.Vulture_Feather_Hit_Terrain, pos, Mathf.InverseLerp(0.2f, 20f, vel.magnitude), Mathf.Lerp(3.5f, 0.5f, Mathf.InverseLerp(7f, 70f, CurrentLength)));
                }
                terrainContactTimer = 0;
            }
        }

        public Color CurrentColor()
        {
            if (kGraphics.alcedo.IsMiros)
            {
                Color rgb = HSLColor.Lerp(new HSLColor(kGraphics.ColorB.hue, Mathf.Lerp(kGraphics.ColorB.saturation, 1f, saturationBonus), Mathf.Lerp(kGraphics.ColorB.lightness, 1f, lightnessBonus)),
                                          kGraphics.ColorA,
                                          Mathf.Cos(Mathf.Pow(wingPosition, 0.75f) * 3.1415927f)).rgb;
                rgb.a = Mathf.Max(new float[]
                {
                    0.4f,
                    forcedAlpha,
                    Mathf.Lerp(0.4f, 0.8f, Mathf.Cos(Mathf.Pow(wingPosition, 1.7f) * 3.1415927f))
                }) * (extendedFac + wing.flyingMode) * 0.5f * (1f - brokenColor);
                if (kGraphics.alcedo.isLaserActive())
                {
                    rgb.a = UnityEngine.Random.value;
                }
                return rgb;
            }
            HSLColor colorB = kGraphics.ColorB;
            HSLColor colorA = kGraphics.ColorA;
            if (type == "Covert")
            {
                colorB.hue -= 0.1f;
                colorA.hue -= 0.1f;
                colorB.hue = (colorB.hue + 1f) % 1f;
                colorA.hue = (colorA.hue + 1f) % 1f;
            }
            if (kGraphics.albino)
            {
                colorB.saturation = Mathf.Lerp(colorB.saturation, 1f, 0.2f);
                colorB.hue = 0f;
                colorB.lightness = Mathf.Lerp(colorB.saturation, 0.2f, 0.8f);
                colorA.saturation = 0.8f;
                colorA.lightness = 0.6f;
            }
            Color rgb2 = HSLColor.Lerp(new HSLColor(colorB.hue, Mathf.Lerp(colorB.saturation, 1f, saturationBonus), Mathf.Lerp(colorB.lightness, 1f, lightnessBonus)),
                                       colorA,
                                       Mathf.Cos(Mathf.Pow(wingPosition, 0.75f) * 3.1415927f)).rgb;
            rgb2.a = Mathf.Max(forcedAlpha,
                Mathf.Lerp(0.7f, 0.9f, Mathf.Cos(Mathf.Pow(wingPosition, 1.7f) * 3.1415927f))) * Mathf.Lerp((extendedFac + wing.flyingMode) * 0.5f, 1f, 0.5f) * (1f - brokenColor);
            /*
            rgb2.a = Mathf.Max(this.forcedAlpha, 
                Mathf.Lerp(0.2f, 0.6f, Mathf.Cos(Mathf.Pow(this.wingPosition, 1.7f) * 3.1415927f))) * (this.extendedFac + this.wing.flyingMode) * 0.5f * (1f - this.brokenColor);
            */
            return rgb2;
        }

        public Color CurrentClawColor()
        {
            HSLColor colorB = kGraphics.ColorB;
            HSLColor colorA = kGraphics.ColorA;
            Color rgb2 = HSLColor.Lerp(new HSLColor(colorB.hue, Mathf.Lerp(colorB.saturation, 1f, saturationBonus), Mathf.Lerp(colorB.lightness, 1f, lightnessBonus)),
                                       colorA,
                                       Mathf.Cos(Mathf.Pow(wingPosition, 0.75f) * 3.1415927f)).rgb;
            rgb2.a = Mathf.Max(forcedAlpha, 0.9f);
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
        private ContractMode contractMode;
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
