using TheOutsider.CustomLore.CustomCreature.Alcedo;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheOutsider.CustomLore.CustomCosmetics
{
    internal class AlcedoLongHeadScales : AlcedoLongBodyScales
    {
        public AlcedoLongHeadScales(AlcedoGraphics aGraphics, int startSprite)
            : base(aGraphics, startSprite)
        {
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
                //sLeaser.sprites[num].color = base.CurrentScaleColor();//aGraphics.HeadColor(timeStacker);
                if (colored > 0f)
                {
                    sLeaser.sprites[num + scalesPositions.Length].color = Color.red;//base.CurrentScaleColor(num - startSprite, this.ScalesPos(num - startSprite)); //aGraphics.effectColor;
                }
            }
        }
    }
}
