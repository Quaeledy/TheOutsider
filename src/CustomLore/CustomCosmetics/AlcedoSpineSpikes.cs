using RWCustom;
using System;
using TheOutsider.CustomLore.CustomCreature.Alcedo;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCosmetics
{
    internal class AlcedoSpineSpikes : AlcedoScaleTemplate
    {
        public int bumps;

        public float spineLength;

        public float sizeSkewExponent;

        public float sizeRangeMin;

        public float sizeRangeMax;

        public int graphic;

        public float scaleX;

        public AlcedoSpineSpikes(AlcedoGraphics aGraphics, int startSprite)
            : base(aGraphics, startSprite)
        {
            spritesOverlap = SpritesOverlap.Behind;
            //spritesOverlap = SpritesOverlap.BehindHead;
            float num = Mathf.Lerp(5f, 8f, Mathf.Pow(UnityEngine.Random.value, 0.7f));
            spineLength = Mathf.Lerp(0.75f, 0.95f, UnityEngine.Random.value) * aGraphics.SpineLength;
            //spineLength = Mathf.Lerp(0.2f, 0.95f, UnityEngine.Random.value) * aGraphics.SpineLength;
            sizeRangeMin = Mathf.Lerp(0.25f, 0.4f, Mathf.Pow(UnityEngine.Random.value, 2f));
            sizeRangeMax = Mathf.Lerp(sizeRangeMin + 0.2f, 0.9f, UnityEngine.Random.value);/*
            sizeRangeMin = Mathf.Lerp(0.1f, 0.5f, Mathf.Pow(UnityEngine.Random.value, 2f));
            sizeRangeMax = Mathf.Lerp(sizeRangeMin, 1.1f, UnityEngine.Random.value);*//*
            if (UnityEngine.Random.value < 0.5f)
            {
                sizeRangeMax = 1f;
            }
            if (aGraphics.alcedo.Template.type == CreatureTemplate.Type.BlueAlcedo)
            {
                sizeRangeMin = Mathf.Min(sizeRangeMin, 0.3f);
                sizeRangeMax = Mathf.Min(sizeRangeMax, 0.6f);
            }
            else if (aGraphics.alcedo.Template.type != CreatureTemplate.Type.GreenAlcedo && UnityEngine.Random.value < 0.7f)
            {
                sizeRangeMin *= 0.7f;
                sizeRangeMax *= 0.7f;
            }
            else if (aGraphics.alcedo.Template.type == CreatureTemplate.Type.GreenAlcedo && UnityEngine.Random.value < 0.7f)
            {
                sizeRangeMin = Mathf.Lerp(sizeRangeMin, 1.1f, 0.1f);
                sizeRangeMax = Mathf.Lerp(sizeRangeMax, 1.1f, 0.4f);
            }*/
            sizeSkewExponent = Mathf.Lerp(0.5f, 0.9f, UnityEngine.Random.value);//Mathf.Lerp(0.1f, 0.9f, UnityEngine.Random.value);
            bumps = (int)(spineLength / num);
            scaleX = 1f;/*
            graphic = UnityEngine.Random.Range(0, 5);
            if (graphic == 1)
            {
                graphic = 0;
            }
            if (graphic == 4)
            {
                graphic = 3;
            }
            else if (graphic == 3 && UnityEngine.Random.value < 0.5f)
            {
                scaleX = -1f;
            }
            else if (UnityEngine.Random.value < 1f / 15f)
            {
                scaleX = -1f;
            }/*
            if (aGraphics.alcedo.Template.type == CreatureTemplate.Type.PinkAlcedo && UnityEngine.Random.value < 0.7f)
            {
                graphic = 0;
            }
            else if (aGraphics.alcedo.Template.type == CreatureTemplate.Type.GreenAlcedo && UnityEngine.Random.value < 0.5f)
            {
                graphic = 3;
            }
            colored = UnityEngine.Random.Range(0, 3);
            if (aGraphics.alcedo.Template.type == CreatureTemplate.Type.PinkAlcedo && UnityEngine.Random.value < 0.5f)
            {
                colored = 0;
            }
            else if (aGraphics.alcedo.Template.type == CreatureTemplate.Type.GreenAlcedo && UnityEngine.Random.value < 0.5f)
            {
                colored = 2;
            }
            else if (aGraphics.alcedo.Template.type == CreatureTemplate.Type.GreenAlcedo && UnityEngine.Random.value < 0.5f)
            {
                colored = 1;
            }
            if (ModManager.MSC && aGraphics.alcedo.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TrainAlcedo)
            {
                colored = 1;
                sizeRangeMin = 1f;
                sizeRangeMax = 3f;
            }*/
            //！！！此处限定了装饰种类！！！
            graphic = 1;
            colored = 0.9f;
            numberOfSprites = ((colored > 0f) ? (bumps * 2) : bumps); 
            scalesCount = bumps;
        }

        public override void Update()
        {
            for (int i = 0; i < bumps; i++)
                base.ColorUpdate(i, (float)i / (float)bumps);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            for (int num = startSprite + bumps - 1; num >= startSprite; num--)
            {
                sLeaser.sprites[num] = new FSprite("AlcedoScaleA" + graphic);
                sLeaser.sprites[num].anchorY = 0.05f;
                if (colored > 0f)
                {
                    sLeaser.sprites[num + bumps] = new FSprite("AlcedoScaleB" + graphic);
                    sLeaser.sprites[num + bumps].anchorY = 0.05f;
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int num = startSprite + bumps - 1; num >= startSprite; num--)
            {
                float num2 = Mathf.InverseLerp(startSprite, startSprite + bumps - 1, num);
                AlcedoGraphics.AlcedoSpineData alcedoSpineData = aGraphics.SpinePosition(Mathf.Lerp(0.05f, spineLength / aGraphics.SpineLength, num2), timeStacker);
                Vector2 natureDir = Vector2.Lerp(alcedoSpineData.perp * alcedoSpineData.depthRotation, alcedoSpineData.dir, 0.3f + 0.5f * (float)(num - startSprite) / (float)bumps).normalized;
                float rotation = Custom.VecToDeg(natureDir);
                //float rotation = Custom.VecToDeg(Vector2.Lerp(natureDir, -alcedoSpineData.vel.normalized, Mathf.Clamp(alcedoSpineData.vel.magnitude / 7f + 0.1f, 0.1f, 0.9f))) - 90f;
                sLeaser.sprites[num].x = alcedoSpineData.outerPos.x - camPos.x;
                sLeaser.sprites[num].y = alcedoSpineData.outerPos.y - camPos.y;
                sLeaser.sprites[num].rotation = rotation;//Custom.AimFromOneVectorToAnother(-alcedoSpineData.perp * alcedoSpineData.depthRotation, alcedoSpineData.perp * alcedoSpineData.depthRotation);
                float sizeFac = Mathf.Lerp(sizeRangeMin, sizeRangeMax, Mathf.Sin(Mathf.Pow(num2, sizeSkewExponent) * (float)Math.PI));
                sLeaser.sprites[num].scaleX = Mathf.Lerp(sLeaser.sprites[num].scaleX, 0.7f * Mathf.Sign(alcedoSpineData.depthRotation) * scaleX * sizeFac, 0.2f);
                sLeaser.sprites[num].scaleY = Mathf.Lerp(sLeaser.sprites[num].scaleY, sizeFac * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(alcedoSpineData.depthRotation))), 0.2f);
                //sLeaser.sprites[num].scaleX = Mathf.Sign(aGraphics.depthRotation) * scaleX * sizeFac;
                //sLeaser.sprites[num].scaleY = sizeFac * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(aGraphics.depthRotation)));
                if (colored > 0f)
                {
                    sLeaser.sprites[num + bumps].x = alcedoSpineData.outerPos.x - camPos.x;
                    sLeaser.sprites[num + bumps].y = alcedoSpineData.outerPos.y - camPos.y;
                    sLeaser.sprites[num + bumps].rotation = rotation;//Custom.AimFromOneVectorToAnother(-alcedoSpineData.perp * alcedoSpineData.depthRotation, alcedoSpineData.perp * alcedoSpineData.depthRotation);
                    sLeaser.sprites[num + bumps].scaleX = Mathf.Lerp(sLeaser.sprites[num + bumps].scaleX, sizeFac * 0.7f * Mathf.Sign(alcedoSpineData.depthRotation) * scaleX, 0.2f);
                    sLeaser.sprites[num + bumps].scaleY = Mathf.Lerp(sLeaser.sprites[num + bumps].scaleY, sizeFac * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(alcedoSpineData.depthRotation))), 0.2f);
                    //sLeaser.sprites[num + bumps].scaleX = Mathf.Sign(aGraphics.depthRotation) * scaleX * sizeFac;
                    //sLeaser.sprites[num + bumps].scaleY = sizeFac * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(aGraphics.depthRotation)));
                    sLeaser.sprites[num + bumps].color = base.CurrentScaleColor(num - startSprite, (float)(num - startSprite) / (float)bumps);
                }
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            for (int i = startSprite; i < startSprite + bumps; i++)
            {
                float f = Mathf.Lerp(0.05f, spineLength / aGraphics.SpineLength, Mathf.InverseLerp(startSprite, startSprite + bumps - 1, i));
                //sLeaser.sprites[i].color = aGraphics.BodyColor(f);
                /*
                if (colored == 1)
                {
                    sLeaser.sprites[i + bumps].color = aGraphics.effectColor;
                }
                else if (colored == 2)
                {
                    float f2 = Mathf.InverseLerp(startSprite, startSprite + bumps - 1, i);
                    sLeaser.sprites[i + bumps].color = Color.Lerp(aGraphics.effectColor, aGraphics.BodyColor(f), Mathf.Pow(f2, 0.5f));
                }*/
                sLeaser.sprites[i + bumps].color = base.CurrentScaleColor(i - startSprite, (float)(i - startSprite) / (float)bumps);
            }
        }
    }

}
