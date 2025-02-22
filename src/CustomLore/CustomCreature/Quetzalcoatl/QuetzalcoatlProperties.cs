using Fisobs.Properties;
using MoreSlugcats;

namespace TheOutsider.CustomLore.CustomCreature.Quetzalcoatl
{
    sealed class QuetzalcoatlProperties : ItemProperties
    {
        private readonly Quetzalcoatl quetzalcoatl;

        public QuetzalcoatlProperties(Quetzalcoatl quetzalcoatl)
        {
            this.quetzalcoatl = quetzalcoatl;
        }

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            if (quetzalcoatl.State.alive)
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
                quarterPips = 4 * quetzalcoatl.FoodPoints;
            }
        }
    }
}
