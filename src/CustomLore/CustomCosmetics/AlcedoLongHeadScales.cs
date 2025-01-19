using RWCustom;
using TheOutsider.CustomLore.CustomCreature.Alcedo;
using UnityEngine;
using static TheOutsider.CustomLore.CustomCreature.Alcedo.AlcedoGraphics;
using Random = UnityEngine.Random;

namespace TheOutsider.CustomLore.CustomCosmetics
{
    internal class AlcedoLongHeadScales : AlcedoLongBodyScales
    {
        public AlcedoLongHeadScales(AlcedoGraphics aGraphics, int startSprite)
            : base(aGraphics, startSprite)
        {
            spritesOverlap = SpritesOverlap.BehindHead;
            rigor = Random.value;
            GenerateTwoHorns();
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

        protected void GenerateTwoHorns()
        {
            scalesPositions = new Vector2[2];
            float y = -1f;
            //float y = Mathf.Lerp(0f, 0.07f, Random.value);
            float num = Mathf.Lerp(0.5f, 1.5f, Random.value);
            for (int i = 0; i < scalesPositions.Length; i++)
            {
                scalesPositions[i] = new Vector2((i == 0) ? (0f - num) : num, y);
            }
            scalesCount = scalesPositions.Length;
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
            {
                float sizeFac = 0.5f;//Mathf.Lerp(sizeRangeMin, sizeRangeMax, Mathf.Sin(Mathf.Pow(num2, sizeSkewExponent) * (float)Math.PI));
                float headRotationFac = aGraphics.headGraphic <= 4 ? 
                    Custom.LerpMap(aGraphics.headGraphic, 0, 4, 0f, 1f) :
                    Custom.LerpMap(aGraphics.headGraphic, 5, 8, 0.9f, 0f);
                float lengthFac = aGraphics.headGraphic <= 4 ?
                    Custom.LerpMap(aGraphics.headGraphic, 0, 4, 0.8f, 1f) :
                    Custom.LerpMap(aGraphics.headGraphic, 5, 8, 0.9f, 0f);
                //AlcedoGraphics.AlcedoSpineData alcedoSpineData = aGraphics.SpinePosition(scalesPositions[num - startSprite].y, timeStacker);
                float neckToHeadAngle = aGraphics.HeadRotation(timeStacker);/*Custom.AimFromOneVectorToAnother(Vector2.Lerp(aGraphics.alcedo.neck.tChunks[aGraphics.alcedo.neck.tChunks.Length - 1].lastPos,
                                                                                      aGraphics.alcedo.neck.tChunks[aGraphics.alcedo.neck.tChunks.Length - 1].pos, timeStacker),
                                                                         Vector2.Lerp(aGraphics.alcedo.bodyChunks[4].lastPos, aGraphics.alcedo.bodyChunks[4].pos, timeStacker));*/
                Vector2 perp = -Custom.DegToVec(neckToHeadAngle) * (neckToHeadAngle > 0f ? -1f : 1f);
                Vector2 offset = Mathf.Lerp(headRotationFac, 1f, 0.8f) * (7f * perp + 5f * Custom.DegToVec(neckToHeadAngle));
                offset += (((num - startSprite) % 2 == 0) ? -1f : 1f) * (1f - headRotationFac) * 7f * perp;
                sLeaser.sprites[num].x = Mathf.Lerp(sLeaser.sprites[num].x, sLeaser.sprites[aGraphics.HeadSprite].x + sizeFac * offset.x, 0.25f);
                sLeaser.sprites[num].y = Mathf.Lerp(sLeaser.sprites[num].y, sLeaser.sprites[aGraphics.HeadSprite].y + sizeFac * offset.y, 0.25f);
                sLeaser.sprites[num].rotation += 10f * (((num - startSprite) % 2 == 0) ? -1f : 1f) * headRotationFac;
                sLeaser.sprites[num].scaleX = Mathf.Lerp(sLeaser.sprites[num].scaleX, sizeFac * 0.05f * Mathf.Sign(headRotationFac) * scaleX, 0.2f);
                sLeaser.sprites[num].scaleY = Mathf.Lerp(sLeaser.sprites[num].scaleY, sizeFac * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(lengthFac))), 0.2f);
                //sLeaser.sprites[num].color = base.CurrentScaleColor();//aGraphics.HeadColor(timeStacker);
                if (colored > 0f)
                {
                    sLeaser.sprites[num + scalesPositions.Length].x = sLeaser.sprites[num].x;
                    sLeaser.sprites[num + scalesPositions.Length].y = sLeaser.sprites[num].y;
                    sLeaser.sprites[num + scalesPositions.Length].rotation = sLeaser.sprites[num].rotation;
                    sLeaser.sprites[num + scalesPositions.Length].scaleX = sLeaser.sprites[num].scaleX;
                    sLeaser.sprites[num + scalesPositions.Length].scaleY = sLeaser.sprites[num].scaleY;
                    sLeaser.sprites[num + scalesPositions.Length].color = base.CurrentScaleColor(Mathf.Abs(num - startSprite - Mathf.FloorToInt((float)scalesPositions.Length / 2f) + 0.5f), this.ScalesPos(Mathf.RoundToInt(Mathf.Abs(num - startSprite - Mathf.FloorToInt((float)scalesPositions.Length / 2f) + 0.5f)))); //aGraphics.effectColor;
                }
            }
        }
    }
}
