using System;
using TheOutsider.Player_Hooks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class Flare
    {

        public static void Init()
        {
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        }
        #region PlayerGraphics

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {
                player.FlareSprite = sLeaser.sprites.Length;

                Array.Resize(ref sLeaser.sprites, player.FlareSprite + 1);

                //闪光图层
                sLeaser.sprites[player.FlareSprite] = new FSprite("Futile_White", true);

                self.AddToContainer(sLeaser, rCam, null);
            }
        }

        //闪光
        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {
                Vector2 vector = Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker);

                sLeaser.sprites[player.FlareSprite].scale = 2.5f;
                sLeaser.sprites[player.FlareSprite].x = vector.x - camPos.x;
                sLeaser.sprites[player.FlareSprite].y = vector.y - camPos.y;


                if (player.burning == 0f || self.player.room != rCam.room || Plugin.optionsMenuInstance.hideFlare.Value)
                {
                    sLeaser.sprites[player.FlareSprite].isVisible = false;
                    player.LightColor = player.NullColor;
                }
                else
                {
                    sLeaser.sprites[player.FlareSprite].isVisible = true;
                    sLeaser.sprites[player.FlareSprite].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
                    sLeaser.sprites[player.FlareSprite].x = vector.x - camPos.x + Mathf.Lerp(player.lastFlickerDir.x, player.flickerDir.x, timeStacker);
                    sLeaser.sprites[player.FlareSprite].y = vector.y - camPos.y + Mathf.Lerp(player.lastFlickerDir.y, player.flickerDir.y, timeStacker);
                    sLeaser.sprites[player.FlareSprite].scale = Mathf.Lerp(player.lastFlashRad, player.flashRad, timeStacker) / 16f;
                    sLeaser.sprites[player.FlareSprite].color = player.GetFlareColor();
                    sLeaser.sprites[player.FlareSprite].alpha = Mathf.Lerp(player.lastFlashAlpha, player.flashAplha, timeStacker);

                    player.LightColor = sLeaser.sprites[player.FlareSprite].color;
                }
            }
        }
        #endregion
    }
}
