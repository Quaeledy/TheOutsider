using RWCustom;
using System;
using TheOutsider.Player_Hooks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class Speckle
    {
        public static void Init()
        {
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
        }
        #region PlayerGraphics

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {
                if (player.speckleSprite >= 1 && sLeaser.sprites.Length >= player.speckleSprite + 6)
                {
                    var foregroundContainer = rCam.ReturnFContainer("Foreground");
                    var midgroundContainer = newContatiner != null ? newContatiner : rCam.ReturnFContainer("Midground");

                    //让尾巴移到臀部后
                    sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[4]);

                    //加入尾部凸起
                    for (int i = 0; i < 6; i++)
                    {
                        foregroundContainer.RemoveChild(sLeaser.sprites[player.speckleSprite + i]);
                        midgroundContainer.AddChild(sLeaser.sprites[player.speckleSprite + i]);
                        sLeaser.sprites[player.speckleSprite + i].MoveBehindOtherNode(sLeaser.sprites[4]);
                    }
                }
            }
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {
                player.speckleSprite = sLeaser.sprites.Length;

                Array.Resize(ref sLeaser.sprites, player.speckleSprite + 6);

                //尾巴亮斑
                for (int i = 0; i < 6; i++)
                {
                    sLeaser.sprites[player.speckleSprite + i] = new FSprite("Pebble5", true);
                }

                self.AddToContainer(sLeaser, rCam, null);
            }
        }


        //尾巴亮斑
        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {//身体位置
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
                        sLeaser.sprites[player.speckleSprite + i].isVisible = false;
                        sLeaser.sprites[player.speckleSprite + i + 3].isVisible = false;
                    }
                    else if (moveDeg > 10f)
                    {
                        sLeaser.sprites[player.speckleSprite + i].isVisible = true;
                        sLeaser.sprites[player.speckleSprite + i + 3].isVisible = false;
                    }
                    else if (moveDeg < -10f)
                    {
                        sLeaser.sprites[player.speckleSprite + i].isVisible = false;
                        sLeaser.sprites[player.speckleSprite + i + 3].isVisible = true;
                    }
                    else if (moveDeg < 5f && moveDeg > -5f)
                    {
                        sLeaser.sprites[player.speckleSprite + i].isVisible = true;
                        sLeaser.sprites[player.speckleSprite + i + 3].isVisible = true;
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
                        sLeaser.sprites[player.speckleSprite + k * 3 + i].x = Mathf.Lerp(taillerp1.x, taillerp2.x, i / 2f) - camPos.x + drift.x;
                        sLeaser.sprites[player.speckleSprite + k * 3 + i].y = Mathf.Lerp(taillerp1.y, taillerp2.y, i / 2f) - camPos.y + drift.y;
                        //确定亮斑大小（从第一对亮斑到第三对亮斑逐渐减小）
                        sLeaser.sprites[player.speckleSprite + k * 3 + i].scale = 0.4f - 0.05f * i;
                        //确定亮斑颜色（可自定义颜色）
                        sLeaser.sprites[player.speckleSprite + k * 3 + i].color = player.GetSpeckleColor();
                    }
                }
            }
        }

        //两亮斑间距
        static float distance = 3f;
        #endregion
    }
}
