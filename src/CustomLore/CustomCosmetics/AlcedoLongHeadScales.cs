using RWCustom;
using System;
using TheOutsider.CustomLore.CustomCreature.Alcedo;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheOutsider.CustomLore.CustomCosmetics
{
    internal class AlcedoLongHeadScales : AlcedoLongBodyScales
    {
        private int count;
        public float sizeSkewExponent;
        public float sizeRangeMin;
        public float sizeRangeMax;
        public float angleRange;

        public AlcedoLongHeadScales(AlcedoGraphics aGraphics, int startSprite)
            : base(aGraphics, startSprite)
        {
            spritesOverlap = SpritesOverlap.Discretion;
            count = Mathf.RoundToInt(Mathf.Lerp(2f, 7f, Random.value));
            sizeRangeMin = Mathf.Lerp(Mathf.Lerp(0.35f, 0.5f, UnityEngine.Random.value),
                Custom.LerpMap(count, 2, 7, 0.5f, 0.35f),
                0.75f);
            Mathf.Lerp(0.35f, 0.45f, Mathf.Pow(UnityEngine.Random.value, 2f));
            sizeRangeMax = Mathf.Lerp(sizeRangeMin + 0.2f, 0.75f, UnityEngine.Random.value);
            sizeSkewExponent = Mathf.Lerp(0.1f, 0.9f, UnityEngine.Random.value);//Mathf.Lerp(0.1f, 0.9f, UnityEngine.Random.value);
            angleRange = Mathf.Lerp(Mathf.Lerp(10f, 80f, UnityEngine.Random.value),
                Custom.LerpMap(count, 2, 7, 10f, 80f),
                0.85f);
            rigor = Random.value;
            GenerateCrest();
            float num = Mathf.Pow(Random.value, 0.7f);// * aGraphics.alcedo.alcedoParams.headSize;
            colored = 0.9f;//Random.value < 0.5f && aGraphics.alcedo.Template.type != CreatureTemplate.Type.WhiteAlcedo;
            graphic = 1;/*Random.Range(4, 6);
            if (num < 0.5f && Random.value < 0.5f)
            {
                graphic = 6;
            }
            else if (num > 0.8f)
            {
                graphic = 5;
            }*/
            /*if (num < 0.2f)// && aGraphics.alcedo.Template.type != CreatureTemplate.Type.WhiteAlcedo
            {
                colored = 0.5f;//true;
            }*//*
            if (aGraphics.alcedo.Template.type == CreatureTemplate.Type.BlackAlcedo)
            {
                colored = false;
            }*/
            graphicHeight = Futile.atlasManager.GetElementWithName("AlcedoScaleA" + graphic).sourcePixelSize.y;
            scaleObjects = new AlcedoScale[scalesPositions.Length];
            backwardsFactors = new float[scalesPositions.Length];
            float value = Random.value;
            float num2 = Mathf.Pow(Random.value, 0.85f);
            for (int i = 0; i < scalesPositions.Length; i++)
            {
                scaleObjects[i] = new AlcedoScale(this);
                scaleObjects[i].length = Mathf.Lerp(5f, 35f, num);
                scaleObjects[i].width = Mathf.Lerp(0.65f, 1.2f, value * num);
                backwardsFactors[i] = num2;
            }
            numberOfSprites = (colored > 0f ? (scalesPositions.Length * 2) : scalesPositions.Length);
        }

        public override void Update()
        {
            base.Update();
            for (int i = 0; i < this.scalesPositions.Length; i++)
                base.ColorUpdate(i, 0f);
        }

        protected void GenerateCrest()
        {
            scalesPositions = new Vector2[count];
            float y = -1f;
            //float y = Mathf.Lerp(0f, 0.07f, Random.value);
            float num = Mathf.Lerp(0.5f, 1.5f, Random.value);
            for (int i = 0; i < scalesPositions.Length; i++)
            {
                scalesPositions[i] = new Vector2(Mathf.Lerp(-num, num, (float)i / (float)(scalesPositions.Length - 1)), y);
            }
            scalesCount = scalesPositions.Length;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
            for (int num = startSprite; num < startSprite + scalesPositions.Length; num++)
            {
                sLeaser.sprites[num].anchorX = 0.5f;
                sLeaser.sprites[num].anchorY = 0.05f;
                sLeaser.sprites[num + scalesPositions.Length].anchorX = 0.5f;
                sLeaser.sprites[num + scalesPositions.Length].anchorY = 0.05f;
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            float neckToHeadAngle = sLeaser.sprites[aGraphics.HeadSpriteStart].rotation;
            float headRotationFac = aGraphics.headGraphic <= 4 ?
                Custom.LerpMap(aGraphics.headGraphic, 0, 4, 0f, 1f) :
                Custom.LerpMap(aGraphics.headGraphic, 5, 8, 0.9f, 0f);
            float lengthFac = aGraphics.headGraphic <= 4 ?
                Custom.LerpMap(aGraphics.headGraphic, 0, 4, 0.8f, 1f) :
                Custom.LerpMap(aGraphics.headGraphic, 5, 8, 0.9f, 0f);
            float rotationOffset = aGraphics.headGraphic <= 4 ?
                Custom.LerpMap(aGraphics.headGraphic, 1, 4, 15f, 40f) :
                Custom.LerpMap(aGraphics.headGraphic, 5, 7, 90f, 150f);
            Vector2 offset = aGraphics.headGraphic <= 4 ?
                Vector2.Lerp(new Vector2(0, 5), new Vector2(-10, 11), Mathf.InverseLerp(0, 4, aGraphics.headGraphic)) :
                Vector2.Lerp(new Vector2(-4, 11), new Vector2(0, 8), Mathf.InverseLerp(5, 8, aGraphics.headGraphic));
            Vector2 extraOffset = aGraphics.headGraphic <= 4 ?
                Vector2.Lerp(new Vector2(-6, -8), new Vector2(1, -8), Mathf.InverseLerp(0, 4, aGraphics.headGraphic)) :
                Vector2.Lerp(new Vector2(-1, -9), new Vector2(-7, -2), Mathf.InverseLerp(5, 8, aGraphics.headGraphic));
            if (aGraphics.headGraphic == 0)
            {
                rotationOffset = 0f;
            }
            else if (aGraphics.headGraphic == 8)
            {
                rotationOffset = 180f;
            }
            for (int num = startSprite; num < startSprite + scalesPositions.Length; num++)
            {
                //两侧羽毛额外偏移
                float dist = num - startSprite - (float)(scalesPositions.Length - 1f) / 2f;
                extraOffset *= Mathf.Sign(dist);
                //offset += Vector2.Lerp(Vector2.zero, extraOffset, Mathf.Abs(dist) / ((float)(scalesPositions.Length - 1f) / 2f));
                //总偏移
                offset = offset.magnitude * Custom.DegToVec(Custom.VecToDeg(offset) + neckToHeadAngle);
                sLeaser.sprites[num].x = sLeaser.sprites[aGraphics.HeadSpriteStart].x + offset.x;
                sLeaser.sprites[num].y = sLeaser.sprites[aGraphics.HeadSpriteStart].y + offset.y;
                //两侧羽毛额外旋转
                float extraRotationOffset = 1f;/*
                if (Mathf.Sign(dist) * sLeaser.sprites[aGraphics.HeadSpriteStart].scaleX < 0)
                {
                    extraRotationOffset = aGraphics.headGraphic <= 4 ?
                    Custom.LerpMap(aGraphics.headGraphic, 0, 4, 1f, -0.9f) :
                    Custom.LerpMap(aGraphics.headGraphic, 5, 8, -1f, 0f);
                }*/
                //总旋转
                float t = Mathf.InverseLerp(0f, (float)scalesPositions.Length - 1f, (float)(num - startSprite));
                float idealRotation = sLeaser.sprites[aGraphics.HeadSpriteStart].rotation +
                                      rotationOffset * (sLeaser.sprites[aGraphics.HeadSpriteStart].scaleX > 0 ? 1f : -1f) +
                                      Mathf.Lerp(-angleRange, angleRange, t) * extraRotationOffset;// * (1f - headRotationFac) * (sLeaser.sprites[aGraphics.HeadSpriteStart].scaleX > 0 ? 1f : -1f)
                //idealRotation += Mathf.Lerp(0f, extraRotationOffset, Mathf.Abs(dist) / ((float)(scalesPositions.Length - 1f) / 2f));
                if (idealRotation - sLeaser.sprites[num].rotation > 180f)
                {
                    idealRotation = idealRotation - 360f;
                }
                else if (idealRotation - sLeaser.sprites[num].rotation < -180f)
                {
                    idealRotation = idealRotation + 360f;
                }
                sLeaser.sprites[num].rotation = idealRotation;// Mathf.Lerp(sLeaser.sprites[num].rotation, idealRotation, 0.5f);
                //羽毛大小
                float sizeFac = Mathf.Lerp(sizeRangeMin, sizeRangeMax, Mathf.Sin(t * (float)Math.PI));
                sLeaser.sprites[num].scaleX = sizeFac * 0.65f * Mathf.Sign(sLeaser.sprites[aGraphics.HeadSpriteStart].scaleX);
                sLeaser.sprites[num].scaleY = Mathf.Lerp(sLeaser.sprites[num].scaleY, sizeFac * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(lengthFac))), 0.2f);
                if (colored > 0f)
                {
                    sLeaser.sprites[num + scalesPositions.Length].x = sLeaser.sprites[num].x;
                    sLeaser.sprites[num + scalesPositions.Length].y = sLeaser.sprites[num].y;
                    sLeaser.sprites[num + scalesPositions.Length].rotation = sLeaser.sprites[num].rotation;
                    sLeaser.sprites[num + scalesPositions.Length].scaleX = sLeaser.sprites[num].scaleX;
                    sLeaser.sprites[num + scalesPositions.Length].scaleY = sLeaser.sprites[num].scaleY;
                    sLeaser.sprites[num + scalesPositions.Length].color = base.CurrentScaleColor(Mathf.Abs(dist), 1f);
                    //sLeaser.sprites[num + scalesPositions.Length].color = base.CurrentScaleColor(dist, this.ScalesPos(Mathf.RoundToInt(dist)));
                }
            }
        }

        public override void UpdateSpritesLevel(RoomCamera.SpriteLeaser sLeaser)
        {
            FNode mask = sLeaser.sprites[aGraphics.MaskSpriteStart];
            FNode neck = sLeaser.sprites[aGraphics.NeckSpriteStart];
            int halfIndex = Mathf.FloorToInt((float)scalesPositions.Length / 2f);
            for (int num = startSprite; num <= halfIndex; num++)
            {
                if (mask.scaleX >= 0 && aGraphics.headGraphic >= 1)
                {
                    sLeaser.sprites[num].MoveBehindOtherNode(neck);
                    sLeaser.sprites[num + scalesPositions.Length].MoveBehindOtherNode(neck);
                }
                else
                {
                    sLeaser.sprites[num].MoveBehindOtherNode(mask);
                    sLeaser.sprites[num + scalesPositions.Length].MoveBehindOtherNode(mask);
                }
            }
            for (int num = halfIndex; num <= scalesPositions.Length; num++)
            {
                if (mask.scaleX <= 0 || aGraphics.headGraphic >= 1)
                {
                    sLeaser.sprites[num].MoveBehindOtherNode(neck);
                    sLeaser.sprites[num + scalesPositions.Length].MoveBehindOtherNode(neck);
                }
                else
                {
                    sLeaser.sprites[num].MoveBehindOtherNode(mask);
                    sLeaser.sprites[num + scalesPositions.Length].MoveBehindOtherNode(mask);
                }
            }
        }
    }
}
