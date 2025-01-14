using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature
{
    internal class AlcedoScaleTemplate : BodyPart
    {
        public AlcedoScaleTemplate(AlcedoGraphics kGraphics, int startSprite) : base(kGraphics)
        {
            this.kGraphics = kGraphics;
            this.startSprite = startSprite;
        }

        public override void Update()
        {
            base.Update();
        }

        public virtual void Reset()
        {
        }

        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            this.palette = palette;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            for (int i = this.startSprite; i < this.startSprite + this.numberOfSprites; i++)
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }

        public AlcedoGraphics kGraphics;
        public int numberOfSprites;
        public int startSprite;
        public RoomPalette palette;
        public AlcedoScaleTemplate.SpritesOverlap spritesOverlap;

        public class SpritesOverlap : ExtEnum<AlcedoScaleTemplate.SpritesOverlap>
        {
            public SpritesOverlap(string value, bool register = false) : base(value, register)
            {
            }
            public static readonly AlcedoScaleTemplate.SpritesOverlap Behind = new AlcedoScaleTemplate.SpritesOverlap("Behind", true);
            public static readonly AlcedoScaleTemplate.SpritesOverlap BehindHead = new AlcedoScaleTemplate.SpritesOverlap("BehindHead", true);
            public static readonly AlcedoScaleTemplate.SpritesOverlap InFront = new AlcedoScaleTemplate.SpritesOverlap("InFront", true);
        }
    }
}
