using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using Expedition;
using Random = UnityEngine.Random;

namespace TheOutsider.Player_Hooks
{
    public class SporeCloudHooks
    {
        public static void Update(On.SporeCloud.orig_Update orig, SporeCloud self, bool eu)
        {
            orig(self, eu);

            if (!PlayerHooks.PlayerData.TryGetValue(PlayerEx.playerSelf, out var player) || !player.IsMoth)
            {
                return;
            }

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
                            if (self.room.abstractRoom.creatures[i].realizedCreature == PlayerEx.playerSelf as Creature)
                            {
                                if (Custom.DistLess(self.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, self.rad + self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.rad + 20f))
                                {
                                    if (Random.value < self.life)
                                    {
                                        self.room.abstractRoom.creatures[i].realizedCreature.Die();
                                    }
                                    else
                                    {
                                        self.room.abstractRoom.creatures[i].realizedCreature.Stun(Random.Range(10, 120));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
