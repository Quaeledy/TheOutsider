using Fisobs.Properties;
using MoreSlugcats;

namespace TheOutsider.CustomLore.CustomCreature.Alcedo
{
    sealed class AlcedoProperties : ItemProperties
    {
        private readonly Alcedo alcedo;
        private readonly int FoodPoints;

        public AlcedoProperties(Alcedo alcedo)
        {
            this.alcedo = alcedo;
            FoodPoints = 8;
        }

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            if (alcedo.State.alive)
            {
                grabability = Player.ObjectGrabability.CantGrab;
            }
            else
            {
                grabability = Player.ObjectGrabability.Drag;
            }
        }

        public override void Nourishment(Player player, ref int quarterPips)
        {
            if (player.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                quarterPips = -1;
            }
            else
            {
                quarterPips = 4 * FoodPoints;
            }
        }
    }
}
