using RWCustom;
using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class Speckles : OutsiderGraphics
    {
        private int speckleSprite;
        private int speckleSpriteLength;
        //两亮斑间距
        private float distance;

        public Speckles(PlayerGraphics self, TheOutsider outsider) : base(self, outsider)
        {
            speckleSpriteLength = 6;
            distance = 3f;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (speckleSprite >= 1 && sLeaser.sprites.Length >= speckleSprite + speckleSpriteLength)
            {
                var foregroundContainer = rCam.ReturnFContainer("Foreground");
                var midgroundContainer = newContatiner != null ? newContatiner : rCam.ReturnFContainer("Midground");

                //让尾巴移到臀部后
                sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[4]);

                //加入尾部凸起
                for (int k = 0; k < 2; k++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        foregroundContainer.RemoveChild(sLeaser.sprites[SpeckleSprite(k, i)]);
                        midgroundContainer.AddChild(sLeaser.sprites[SpeckleSprite(k, i)]);
                        sLeaser.sprites[SpeckleSprite(k, i)].MoveBehindOtherNode(sLeaser.sprites[4]);
                    }
                }
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            speckleSprite = sLeaser.sprites.Length;

            Array.Resize(ref sLeaser.sprites, speckleSprite + speckleSpriteLength);

            //尾巴亮斑
            for (int k = 0; k < 2; k++)
            {
                for (int i = 0; i < 3; i++)
                {
                    sLeaser.sprites[SpeckleSprite(k, i)] = new FSprite("Pebble5", true);
                    if (k == 1)
                        sLeaser.sprites[SpeckleSprite(k, i)].scaleX = -1;
                }
            }
        }

        //尾巴亮斑
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            //身体位置
            Vector2 drawPos1 = Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker);
            //臀部位置
            Vector2 drawPos2 = Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker);
            //身体至臀部方向的向量
            Vector2 dif = (drawPos1 - drawPos2).normalized;

            Vector2 tailPos1 = Vector2.Lerp(self.tail[0].lastPos, self.tail[0].pos, timeStacker);
            Vector2 tailPos2 = Vector2.Lerp(self.tail[1].lastPos, self.tail[1].pos, timeStacker);

            //通过尾巴前两截的夹角判断尾巴的朝向
            var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, (tailPos1 - tailPos2).normalized), -22.5f, 22.5f);

            //根据尾巴朝向设置两侧的亮斑是否可见
            for (int i = 0; i < 3; i++)
            {
                if (Plugin.optionsMenuInstance.hideSpeckle.Value)
                {
                    sLeaser.sprites[SpeckleSprite(0, i)].isVisible = false;
                    sLeaser.sprites[SpeckleSprite(1, i)].isVisible = false;
                }
                else if (moveDeg > 10f)
                {
                    sLeaser.sprites[SpeckleSprite(0, i)].isVisible = true;
                    sLeaser.sprites[SpeckleSprite(1, i)].isVisible = false;
                }
                else if (moveDeg < -10f)
                {
                    sLeaser.sprites[SpeckleSprite(0, i)].isVisible = false;
                    sLeaser.sprites[SpeckleSprite(1, i)].isVisible = true;
                }
                else if (moveDeg < 5f && moveDeg > -5f)
                {
                    sLeaser.sprites[SpeckleSprite(0, i)].isVisible = true;
                    sLeaser.sprites[SpeckleSprite(1, i)].isVisible = true;
                }
            }

            Vector2 normalized = (tailPos1 - tailPos2).normalized;
            Vector2 a = Custom.PerpendicularVector(-normalized);
            //对tail的第一节和第二节进行插值
            //第一对亮斑位于比第一节更靠前的位置
            Vector2 taillerp1 = 1.2f * tailPos1 - 0.2f * tailPos2;
            //最后一对亮斑位于第一节、第二节中间
            Vector2 taillerp2 = 0.2f * tailPos1 + 0.8f * tailPos2;

            for (int k = 0; k < 2; k++)
            {
                for (int i = 0; i < 3; i++)
                {
                    //确定同一对亮斑中，两个亮斑的距离
                    float d = (2f - i * 0.2f) * distance * (0.2f + 0.8f * Mathf.Abs(dif.y));
                    //确定偏移量
                    Vector2 drift = a * d * (k == 0 ? -1f : 1f);
                    //确定亮斑位置
                    sLeaser.sprites[speckleSprite + k * 3 + i].x = Mathf.Lerp(taillerp1.x, taillerp2.x, i / 2f) - camPos.x + drift.x;
                    sLeaser.sprites[speckleSprite + k * 3 + i].y = Mathf.Lerp(taillerp1.y, taillerp2.y, i / 2f) - camPos.y + drift.y;
                    //确定亮斑大小（从第一对亮斑到第三对亮斑逐渐减小）
                    sLeaser.sprites[speckleSprite + k * 3 + i].scale = 0.4f - 0.05f * i;
                    //确定亮斑颜色（可自定义颜色）
                    sLeaser.sprites[speckleSprite + k * 3 + i].color = outsider.GetSpeckleColor();
                }
            }
        }

        public int SpeckleSprite(int side, int speckle)
        {
            return speckleSprite + side * 3 + speckle;
        }
    }
}
