using System;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class Flare : OutsiderGraphics
    {
        private int flareSprite;
        private int flareSpriteLength;

        public Flare(PlayerGraphics self, TheOutsider outsider) : base(self, outsider)
        {
            flareSpriteLength = 1;
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            flareSprite = sLeaser.sprites.Length;

            Array.Resize(ref sLeaser.sprites, flareSprite + flareSpriteLength);

            //闪光图层
            sLeaser.sprites[flareSprite] = new FSprite("Futile_White", true);
        }

        //闪光
        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            Vector2 vector = Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker);

            sLeaser.sprites[flareSprite].scale = 2.5f;
            sLeaser.sprites[flareSprite].x = vector.x - camPos.x;
            sLeaser.sprites[flareSprite].y = vector.y - camPos.y;


            if (outsider.burning == 0f || self.player.room != rCam.room || Plugin.optionsMenuInstance.hideFlare.Value)
            {
                sLeaser.sprites[flareSprite].isVisible = false;
                outsider.LightColor = outsider.NullColor;
            }
            else
            {
                sLeaser.sprites[flareSprite].isVisible = true;
                sLeaser.sprites[flareSprite].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
                sLeaser.sprites[flareSprite].x = vector.x - camPos.x + Mathf.Lerp(outsider.lastFlickerDir.x, outsider.flickerDir.x, timeStacker);
                sLeaser.sprites[flareSprite].y = vector.y - camPos.y + Mathf.Lerp(outsider.lastFlickerDir.y, outsider.flickerDir.y, timeStacker);
                sLeaser.sprites[flareSprite].scale = Mathf.Lerp(outsider.lastFlashRad, outsider.flashRad, timeStacker) / 16f;
                sLeaser.sprites[flareSprite].color = outsider.GetFlareColor();
                sLeaser.sprites[flareSprite].alpha = Mathf.Lerp(outsider.lastFlashAlpha, outsider.flashAplha, timeStacker);

                outsider.LightColor = sLeaser.sprites[flareSprite].color;
            }
        }
    }
}
