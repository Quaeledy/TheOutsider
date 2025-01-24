using System.Collections.Generic;
using TheOutsider.CustomLore.CustomCreature.Alcedo;
using UnityEngine;
using Color = UnityEngine.Color;

namespace TheOutsider.CustomLore.CustomCosmetics
{
    internal class AlcedoScaleTemplate : BodyPart
    {
        public AlcedoGraphics aGraphics;
        public int numberOfSprites;
        public int startSprite;
        public RoomPalette palette;
        public SpritesOverlap spritesOverlap;
        public float colored;
        public float forcedAlpha;
        public float[] saturationBonus;
        public float[] lightnessBonus;
        private List<AlcedoGraphics.WingColorWave> colorWaves;
        public int scalesCount;

        public AlcedoScaleTemplate(AlcedoGraphics aGraphics, int startSprite) : base(aGraphics)
        {
            this.aGraphics = aGraphics;
            this.startSprite = startSprite;
            this.colorWaves = new List<AlcedoGraphics.WingColorWave>();
            //this.forcedAlpha = 1f;
        }

        public override void Update()
        {
            if (this.saturationBonus == null)
                this.saturationBonus = new float[scalesCount];
            if (this.lightnessBonus == null)
                this.lightnessBonus = new float[scalesCount];
            base.Update();
        }

        public virtual void ColorUpdate(int i, float scalePosition)
        {
            this.lightnessBonus[i] = Mathf.Max(this.lightnessBonus[i] - 0.1f, 0f);
            if (this.lightnessBonus[i] == 0f)
            {
                this.saturationBonus[i] = Mathf.Max(this.saturationBonus[i] - 0.02f, 0f);
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

                    if (colorWaves[num].lastPosition < scalePosition && colorWaves[num].position >= scalePosition)
                    {
                        saturationBonus[i] = Mathf.Max(saturationBonus[i], colorWaves[num].saturation);
                        lightnessBonus[i] = Mathf.Max(lightnessBonus[i], colorWaves[num].lightness);
                        forcedAlpha = Mathf.Max(forcedAlpha, colorWaves[num].forceAlpha);
                        break;
                    }
                }
            }
        }

        public virtual void Reset()
        {
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            this.palette = palette;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            for (int i = startSprite; i < startSprite + numberOfSprites; i++)
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }

        public Color CurrentScaleColor(float i, float scalePosition)
        {
            if (this.saturationBonus == null)
                this.saturationBonus = new float[scalesCount];
            if (this.lightnessBonus == null)
                this.lightnessBonus = new float[scalesCount];
            HSLColor colorB = this.aGraphics.ColorB;
            HSLColor colorA = this.aGraphics.ColorA;
            if (this.aGraphics.albino)
            {
                colorB.saturation = Mathf.Lerp(colorB.saturation, 1f, 0.2f);
                colorB.hue = 0f;
                colorB.lightness = Mathf.Lerp(colorB.saturation, 0.2f, 0.8f);
                colorA.saturation = 0.8f;
                colorA.lightness = 0.6f;
            }
            float s = Mathf.Lerp(this.saturationBonus[Mathf.FloorToInt(i)], this.saturationBonus[Mathf.CeilToInt(i)], i - Mathf.FloorToInt(i));
            float l = Mathf.Lerp(this.lightnessBonus[Mathf.FloorToInt(i)], this.lightnessBonus[Mathf.CeilToInt(i)], i - Mathf.FloorToInt(i));
            Color rgb2 = HSLColor.Lerp(new HSLColor(colorB.hue, Mathf.Lerp(colorB.saturation, 1f, s), Mathf.Lerp(colorB.lightness, 1f, l)),
                                       colorA,
                                       Mathf.Cos(Mathf.Pow(scalePosition, 0.75f) * 3.1415927f)).rgb;
            rgb2.a = Mathf.Max(this.forcedAlpha,
                Mathf.Lerp(0.7f, 0.9f, Mathf.Cos(Mathf.Pow(scalePosition, 1.7f) * 3.1415927f))) * colored;
            return rgb2;
        }

        public void MakeColorWave(int delay)
        {
            colorWaves.Add(new AlcedoGraphics.WingColorWave(delay, 1f / Mathf.Lerp(10f, 30f, UnityEngine.Random.value), 1f - UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value, 1f - UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value, 1f - UnityEngine.Random.value * UnityEngine.Random.value * UnityEngine.Random.value));
        }

        public class SpritesOverlap : ExtEnum<SpritesOverlap>
        {
            public SpritesOverlap(string value, bool register = false) : base(value, register)
            {
            }
            public static readonly SpritesOverlap Behind = new SpritesOverlap("Behind", true);
            public static readonly SpritesOverlap BehindHead = new SpritesOverlap("BehindHead", true);
            public static readonly SpritesOverlap InFront = new SpritesOverlap("InFront", true);
            public static readonly SpritesOverlap Discretion = new SpritesOverlap("Discretion", true);
        }
    }
}
