using RWCustom;
using System;
using TheOutsider.CustomLore.CustomCreature.Alcedo;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCosmetics
{
    internal class AlcedoSpineTeardrop : AlcedoScaleTemplate
    {
        public int bumps;

        public float spineSpikesLength;
        public float spineStartLength;

        public float sizeSkewExponent;
        public float sizeStartFac;
        public float sizeRangeMin;
        public float sizeRangeMax;

        public int graphic;

        public float scaleX;

        public AlcedoSpineTeardrop(AlcedoGraphics aGraphics, int startSprite)
            : base(aGraphics, startSprite)
        {
            spritesOverlap = SpritesOverlap.Behind;
            //spritesOverlap = SpritesOverlap.BehindHead;
            float scaleDist = Mathf.Lerp(5f, 10f, Mathf.Pow(UnityEngine.Random.value, 0.7f));
            spineStartLength = 0f; //aGraphics.NeckLength;
            spineSpikesLength = Mathf.Lerp(0.75f, 0.95f, UnityEngine.Random.value) * aGraphics.SpineLength;// * (aGraphics.ForeBodyLength + aGraphics.WaistLength + aGraphics.HindBodyLength + aGraphics.TailLength);
            //spineSpikesLength = Mathf.Lerp(0.2f, 0.95f, UnityEngine.Random.value) * aGraphics.SpineLength;
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
            sizeSkewExponent = Mathf.Lerp(0.1f, 0.9f, UnityEngine.Random.value);//Mathf.Lerp(0.1f, 0.9f, UnityEngine.Random.value);
            sizeStartFac = Mathf.Lerp(0.3f, 0.5f, UnityEngine.Random.value);
            bumps = (int)(spineSpikesLength / scaleDist);
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
                sLeaser.sprites[num].anchorY = 0.1f;
                if (colored > 0f)
                {
                    sLeaser.sprites[num + bumps] = new FSprite("AlcedoScaleB" + graphic);
                    sLeaser.sprites[num + bumps].anchorY = sLeaser.sprites[num].anchorY;
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int num = startSprite + bumps - 1; num >= startSprite; num--)
            {
                float t = Mathf.InverseLerp(startSprite, startSprite + bumps - 1, num);
                float neckEndT = Mathf.InverseLerp(0, spineSpikesLength / aGraphics.SpineLength, (aGraphics.NeckLength - spineStartLength) / aGraphics.SpineLength);
                float s = spineStartLength / aGraphics.SpineLength + Mathf.Lerp(0f, spineSpikesLength / aGraphics.SpineLength, t);
                AlcedoGraphics.AlcedoSpineData alcedoSpineData = aGraphics.SpinePosition(s, timeStacker);
                float dirFac = (s < aGraphics.NeckLength / aGraphics.SpineLength) ?
                    Custom.LerpMap(s, 0f, aGraphics.NeckLength / aGraphics.SpineLength, 0.8f, 0.3f) :
                    Custom.LerpMap(s, aGraphics.NeckLength / aGraphics.SpineLength, 1f, 0.3f, 0.8f);
                Vector2 natureDir = Vector2.Lerp(alcedoSpineData.perp * alcedoSpineData.depthRotation, alcedoSpineData.dir, dirFac).normalized;
                float rotation = Custom.VecToDeg(natureDir);
                //float rotation = Custom.VecToDeg(Vector2.Lerp(natureDir, -alcedoSpineData.vel.normalized, Mathf.Clamp(alcedoSpineData.vel.magnitude / 7f + 0.1f, 0.1f, 0.9f))) - 90f;
                sLeaser.sprites[num].x = alcedoSpineData.outerPos.x - camPos.x;
                sLeaser.sprites[num].y = alcedoSpineData.outerPos.y - camPos.y;
                sLeaser.sprites[num].rotation = rotation;//Custom.AimFromOneVectorToAnother(-alcedoSpineData.perp * alcedoSpineData.depthRotation, alcedoSpineData.perp * alcedoSpineData.depthRotation);
                /*sLeaser.sprites[num].rotation = Mathf.Lerp(sLeaser.sprites[num].rotation,
                    Mathf.Lerp(sLeaser.sprites[Mathf.Clamp(num - 1, startSprite, startSprite + bumps - 1)].rotation, sLeaser.sprites[Mathf.Clamp(num + 1, startSprite, startSprite + bumps - 1)].rotation, 0.5f),
                    0.5f);*//*
                float sizeFac = (s < aGraphics.NeckLength / aGraphics.SpineLength) ?
                    Custom.LerpMap(s, 0f, aGraphics.NeckLength / aGraphics.SpineLength, sizeRangeMin, sizeRangeMax, sizeSkewExponent) :
                    Custom.LerpMap(s, 1f, aGraphics.NeckLength / aGraphics.SpineLength, sizeRangeMin, sizeRangeMax, sizeSkewExponent);*/
                float sizeFacOffset = (t < neckEndT) ?
                    Mathf.Pow(Custom.LerpMap(t, 0f, neckEndT, 1f, 0f), sizeSkewExponent) :
                    Mathf.Pow(Custom.LerpMap(t, neckEndT, 1f, 0f, 1f), sizeSkewExponent);
                float sizeFac = Mathf.Lerp(sizeRangeMin, sizeRangeMax, Mathf.Sin(sizeFacOffset * (float)Math.PI * sizeStartFac + (float)Math.PI * (1f - sizeStartFac)));
                sLeaser.sprites[num].scaleX = Mathf.Lerp(sLeaser.sprites[num].scaleX, 0.7f * Mathf.Sign(alcedoSpineData.depthRotation) * scaleX * sizeFac, 0.2f);
                sLeaser.sprites[num].scaleY = Mathf.Lerp(sLeaser.sprites[num].scaleY, sizeFac * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(alcedoSpineData.depthRotation))), 0.2f);
                //sLeaser.sprites[num].scaleX = Mathf.Sign(aGraphics.depthRotation) * scaleX * sizeFac;
                //sLeaser.sprites[num].scaleY = sizeFac * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(aGraphics.depthRotation)));
                if (colored > 0f)
                {
                    sLeaser.sprites[num + bumps].x = alcedoSpineData.outerPos.x - camPos.x;
                    sLeaser.sprites[num + bumps].y = alcedoSpineData.outerPos.y - camPos.y;
                    sLeaser.sprites[num + bumps].rotation = sLeaser.sprites[num].rotation;
                    sLeaser.sprites[num + bumps].scaleX = sLeaser.sprites[num].scaleX;
                    sLeaser.sprites[num + bumps].scaleY = sLeaser.sprites[num].scaleY;
                    sLeaser.sprites[num + bumps].color = base.CurrentScaleColor(num - startSprite, (float)(num - startSprite) / (float)bumps);
                }
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            for (int i = startSprite; i < startSprite + bumps; i++)
            {
                float f = Mathf.Lerp(0.05f, spineSpikesLength / aGraphics.SpineLength, Mathf.InverseLerp(startSprite, startSprite + bumps - 1, i));
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
