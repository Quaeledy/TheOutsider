using System.Runtime.CompilerServices;

namespace TheOutsider.Player_Hooks
{
    public class PlayerHooks
    {
        public static ConditionalWeakTable<Player, TheOutsider> PlayerData = new();
        public static void Init()
        {
            //IL.Player.MovementUpdate += ClimbWall.Player_MovementUpdateIL;

            On.Player.ctor += Player_ctor;
            On.Player.NewRoom += Player_NewRoom;
            On.Player.MovementUpdate += Flight.Player_MovementUpdate;
            On.Player.UpdateMSC += Flight.Player_Fly;
            On.Player.UpdateMSC += Flare.Player_Flare;
            On.Player.Jump += JumpHooks.Player_Jump;
            On.Player.WallJump += JumpHooks.Player_WallJump;
            On.Player.UpdateMSC += SporeCloudHooks.Player_Update;
            On.Player.Update += ClimbWall.Player_Update;
            On.Player.checkInput += ClimbWall.Player_checkInput;
            //On.Player.MovementUpdate += ClimbWall.Player_MovementUpdate;
            On.Player.DeathByBiteMultiplier += Player_DeathByBiteMultiplier;
            On.SporeCloud.Update += SporeCloudHooks.SporeCloud_Update;
        }

        #region Player
        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if (self.SlugCatClass == Plugin.SlugName && !PlayerData.TryGetValue(self, out _))
            {
                TheOutsider player = new TheOutsider(self);
                PlayerData.Add(self, player);
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

        private static float Player_DeathByBiteMultiplier(On.Player.orig_DeathByBiteMultiplier orig, Player self)
        {
            float result = orig(self);
            if (PlayerData.TryGetValue(self, out _))
            {
                result = 100f;
            }
            return result;
        }
        #endregion
    }
}
