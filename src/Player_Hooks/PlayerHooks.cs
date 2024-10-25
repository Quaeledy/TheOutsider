using System.Runtime.CompilerServices;
using TheOutsider.CustomLore.CustomCreature;

namespace TheOutsider.Player_Hooks
{
    public class PlayerHooks
    {
        public static ConditionalWeakTable<Player, PlayerEx> PlayerData = new();
        public static void Init()
        {
            On.Player.ctor += Player_ctor;
            On.Player.MovementUpdate += Flight.Player_MovementUpdate;
            On.Player.UpdateMSC += Flight.Player_Fly;
            On.Player.UpdateMSC += Flare.Player_Flare;
            On.Player.Jump += JumpHooks.Player_Jump;
            On.SporeCloud.Update += SporeCloudHooks.SporeCloud_Update;
            On.Player.UpdateMSC += SporeCloudHooks.Player_Update;
            On.Player.NewRoom += Player_NewRoom;
        }

        #region Player
        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if ((self.SlugCatClass == Plugin.SlugName || self.abstractCreature.creatureTemplate.type == MothPupCritob.MothPup) &&
                !PlayerData.TryGetValue(self, out _))
            {
                PlayerEx player = new PlayerEx(self);
                PlayerData.Add(self, player);
                if (player.isMothNPC)
                {
                    self.npcStats = new Player.NPCStats(self);
                }
            }
        }

        private static void Player_NewRoom(On.Player.orig_NewRoom orig, Player self, Room newRoom)
        {
            orig(self, newRoom);

            if (PlayerData.TryGetValue(self, out var player))
            {
                if (self.AI != null)
                    self.AI.NewRoom(newRoom);
            }
        }
        #endregion
    }
}
