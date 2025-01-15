using TheOutsider.CustomLore.CustomCosmetics;

namespace TheOutsider.CustomLore.CustomCreature.Alcedo
{
    internal class AlcedoScale : BodyPart
    {
        public AlcedoScaleTemplate aCosmetics;

        public float length;

        public float width;

        public AlcedoScale(AlcedoScaleTemplate aCosmetics)
            : base(aCosmetics.aGraphics)
        {
            this.aCosmetics = aCosmetics;
        }

        public override void Update()
        {
            base.Update();
            if (owner.owner.room.PointSubmerged(pos))
            {
                vel *= 0.5f;
            }
            else
            {
                vel *= 0.9f;
            }
            lastPos = pos;
            pos += vel;
        }
    }

}
