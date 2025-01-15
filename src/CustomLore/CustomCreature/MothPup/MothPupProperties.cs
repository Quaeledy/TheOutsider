using Fisobs.Properties;
using MoreSlugcats;

namespace TheOutsider.CustomLore.CustomCreature.MothPup
{
    sealed class MothPupProperties : ItemProperties
    {
        private readonly Player mothPup;
        private readonly int FoodPoints;

        public MothPupProperties(Player mothPup)
        {
            this.mothPup = mothPup;
            FoodPoints = 2;
        }
        /*
        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            if (mothPup.State.alive)
            {
                grabability = Player.ObjectGrabability.OneHand;
            }
            else
            {
                grabability = Player.ObjectGrabability.OneHand;
            }
        }*/

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
