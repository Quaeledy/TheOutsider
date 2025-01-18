using RWCustom;
using TheOutsider.CustomLore.CustomCreature.Alcedo;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheOutsider.CustomLore.CustomCosmetics
{
    internal class AlcedoLongBodyScales : AlcedoBodyScales
    {
        public AlcedoScale[] scaleObjects;

        public float[] backwardsFactors;

        public float graphicHeight;

        public float rigor;

        public float ScalesPos(int i)
        {
            return Mathf.InverseLerp(0f,
                        Vector2.Distance(scalesPositions[0], scalesPositions[scalesPositions.Length - 1]),
                        Vector2.Distance(scalesPositions[0], scalesPositions[i]));
        }

        public AlcedoLongBodyScales(AlcedoGraphics aGraphics, int startSprite)
            : base(aGraphics, startSprite)
        {
        }

        public override void Update()
        {
            base.Update();
            for (int i = 0; i < scaleObjects.Length; i++)
            {
                AlcedoGraphics.AlcedoSpineData backPos = GetBackPos(i, 1f, changeDepthRotation: true);
                Vector2 a = Vector2.Lerp(backPos.dir, Custom.DirVec(backPos.pos, backPos.outerPos), Mathf.Abs(backPos.depthRotation));
                if (scalesPositions[i].y < 0.2f)
                {
                    a -= Custom.DegToVec(aGraphics.HeadRotation(1f)) * Mathf.Pow(Mathf.InverseLerp(0.2f, 0f, scalesPositions[i].y), 2f) * 2f;
                }
                a = Vector2.Lerp(a, backPos.dir, Mathf.Pow(backwardsFactors[i], Mathf.Lerp(1f, 15f, aGraphics.showDominance))).normalized;
                Vector2 vector = backPos.outerPos + a * scaleObjects[i].length;
                if (!Custom.DistLess(scaleObjects[i].pos, vector, scaleObjects[i].length / 2f))
                {
                    Vector2 vector2 = Custom.DirVec(scaleObjects[i].pos, vector);
                    float num = Vector2.Distance(scaleObjects[i].pos, vector);
                    float num2 = scaleObjects[i].length / 2f;
                    scaleObjects[i].pos += vector2 * (num - num2);
                    scaleObjects[i].vel += vector2 * (num - num2);
                }
                scaleObjects[i].vel += Vector2.ClampMagnitude(vector - scaleObjects[i].pos, Mathf.Lerp(10f, 20f, aGraphics.showDominance)) / Mathf.Lerp(5f, 1.5f, rigor);
                scaleObjects[i].vel *= Mathf.Lerp(1f, 0.8f, rigor);
                if (aGraphics.showDominance > 0f)
                {
                    scaleObjects[i].vel += Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0f, 6f, aGraphics.showDominance);
                }
                scaleObjects[i].ConnectToPoint(backPos.outerPos, scaleObjects[i].length, push: true, 0f, new Vector2(0f, 0f), 0f, 0f);
                scaleObjects[i].Update();
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
            {
                sLeaser.sprites[num] = new FSprite("AlcedoScaleA" + graphic);
                sLeaser.sprites[num].scaleY = 1f;//scaleObjects[num - startSprite].length / graphicHeight;
                sLeaser.sprites[num].anchorY = 0.1f;
                if (colored > 0f)
                {
                    sLeaser.sprites[num + scalesPositions.Length] = new FSprite("AlcedoScaleB" + graphic);
                    sLeaser.sprites[num + scalesPositions.Length].scaleY = 1f;//scaleObjects[num - startSprite].length / graphicHeight;
                    sLeaser.sprites[num + scalesPositions.Length].anchorY = 0.1f;
                }
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
            {
                AlcedoGraphics.AlcedoSpineData backPos = GetBackPos(num - startSprite, timeStacker, changeDepthRotation: true);
                float rotation = Custom.AimFromOneVectorToAnother(backPos.outerPos, Vector2.Lerp(scaleObjects[num - startSprite].lastPos, scaleObjects[num - startSprite].pos, timeStacker));
                sLeaser.sprites[num].x = backPos.outerPos.x - camPos.x;
                sLeaser.sprites[num].y = backPos.outerPos.y - camPos.y;
                sLeaser.sprites[num].rotation = rotation;
                sLeaser.sprites[num].scaleX = scaleObjects[num - startSprite].width * Mathf.Sign(backPos.depthRotation);
                if (colored > 0f)
                {
                    sLeaser.sprites[num + scalesPositions.Length].x = backPos.outerPos.x - camPos.x;
                    sLeaser.sprites[num + scalesPositions.Length].y = backPos.outerPos.y - camPos.y;
                    sLeaser.sprites[num + scalesPositions.Length].rotation = rotation;
                    sLeaser.sprites[num + scalesPositions.Length].scaleX = scaleObjects[num - startSprite].width * Mathf.Sign(backPos.depthRotation); 
                    sLeaser.sprites[num + scalesPositions.Length].color = base.CurrentScaleColor(num - startSprite, this.ScalesPos(num - startSprite));
                }
            }/*
            if (aGraphics.lizard.Template.type == CreatureTemplate.Type.WhiteAlcedo)
            {
                ApplyPalette(sLeaser, rCam, palette);
            }*/
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
            {
                sLeaser.sprites[num].color = base.CurrentScaleColor(num - startSprite, this.ScalesPos(num - startSprite));//aGraphics.BodyColor(scalesPositions[num - startSprite].y);
                if (colored > 0f)
                {/*
                    if (aGraphics.lizard.Template.type == CreatureTemplate.Type.WhiteAlcedo)
                    {
                        sLeaser.sprites[num + scalesPositions.Length].color = aGraphics.HeadColor(1f);
                    }
                    else
                    {*/
                    sLeaser.sprites[num + scalesPositions.Length].color = base.CurrentScaleColor(num - startSprite, this.ScalesPos(num - startSprite));//aGraphics.effectColor;
                    //}
                }
            }
            base.ApplyPalette(sLeaser, rCam, palette);
        }
    }
}
