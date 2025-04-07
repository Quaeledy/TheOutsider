using RWCustom;

namespace TheOutsider.Player_Hooks
{
    public class SporeCloudHooks
    {
        public static void SporeCloud_Update(On.SporeCloud.orig_Update orig, SporeCloud self, bool eu)
        {
            orig(self, eu);

            if (!Plugin.optionsMenuInstance.immuneSporeCloud.Value)
            {
                if (!self.nonToxic && self.checkInsectsDelay > -1)
                {
                    self.checkInsectsDelay--;
                    if (self.checkInsectsDelay < 1)
                    {
                        self.checkInsectsDelay = 20;
                        for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
                        {
                            if (self.room.abstractRoom.creatures[i].realizedCreature != null)
                            {
                                if (self.room.abstractRoom.creatures[i].realizedCreature is Player && PlayerHooks.PlayerData.TryGetValue(self.room.abstractRoom.creatures[i].realizedCreature as Player, out var player))
                                {
                                    if (Custom.DistLess(self.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, self.rad + self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.rad + 20f))
                                    {
                                        if (!player.deadForSporeCloud)
                                        {
                                            self.room.abstractRoom.creatures[i].realizedCreature.Die();
                                            player.deadForSporeCloudCount = 40;
                                            player.deadForSporeCloud = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        public static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (PlayerHooks.PlayerData.TryGetValue(self, out var player))
            {
                if (player.deadForSporeCloudCount > 0)
                    player.deadForSporeCloudCount--;
                else
                    player.deadForSporeCloud = false;
            }
        }
    }
}
