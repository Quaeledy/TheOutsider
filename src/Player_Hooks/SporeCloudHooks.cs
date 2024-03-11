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
        static bool init = false;

        public static void Update(On.SporeCloud.orig_Update orig, SporeCloud self, bool eu)
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
                                if (self.room.abstractRoom.creatures[i].realizedCreature is Player && (self.room.abstractRoom.creatures[i].realizedCreature as Player).slugcatStats.name == Plugin.SlugName)
                                {
                                    if (Custom.DistLess(self.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, self.rad + self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.rad + 20f))
                                    {
                                        if (!init)
                                        {
                                            self.room.abstractRoom.creatures[i].realizedCreature.Die();
                                            init = true;
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
}
