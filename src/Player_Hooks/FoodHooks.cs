using MoreSlugcats;
using SlugBase.Features;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;

namespace TheOutsider.Player_Hooks
{
    public class FoodHooks
    {
        public static ConditionalWeakTable<PhysicalObject, PhysicalObjectEx> PhysicalObjectData = new();
        public static readonly SlugNPCAI.Food MothFood = new SlugNPCAI.Food("MothFood", true);
        public static void Init()
        {
            //check if rotund world is enabled (our mass increases if we are full)
            try
            {
                On.PhysicalObject.ctor += PhysicalObject_ctor;
                CustomEdible.Register(new CustomEdibleData(Plugin.SlugName,
                    new CustomEdibleData.FoodData[]{
                        //可以食用
                        new CustomEdibleData.FoodData(AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant, 1, 2),
                        new CustomEdibleData.FoodData(AbstractPhysicalObject.AbstractObjectType.FlareBomb, 1, 2),
                        new CustomEdibleData.FoodData(AbstractPhysicalObject.AbstractObjectType.FlyLure, 1, 0),
                        new CustomEdibleData.FoodData(AbstractPhysicalObject.AbstractObjectType.BubbleGrass, 1, 0),
                        new CustomEdibleData.FoodData(AbstractPhysicalObject.AbstractObjectType.WaterNut, 1, 0),
                        new CustomEdibleData.FoodData(AbstractPhysicalObject.AbstractObjectType.Mushroom, 0, 1),
                        new CustomEdibleData.FoodData(AbstractPhysicalObject.AbstractObjectType.PuffBall, 1, 0),
                        //禁止食用
                        new CustomEdibleData.FoodData(AbstractPhysicalObject.AbstractObjectType.EggBugEgg),
                        new CustomEdibleData.FoodData(AbstractPhysicalObject.AbstractObjectType.JellyFish),
                        new CustomEdibleData.FoodData(AbstractPhysicalObject.AbstractObjectType.Creature),
                        new CustomEdibleData.FoodData(MoreSlugcatsEnums.AbstractObjectType.FireEgg),
                    }));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            //食用腐化蓝果眩晕概率和眩晕时间都减少，获取食物点增加
            On.Player.ObjectEaten += Player_ObjectEaten;
            //食用腐化爆米花眩晕概率和眩晕时间都减少，获取食物点增加
            On.Player.Update += Player_Update;
            //无需把石榴摔到地上，直接就能啃食
            On.Pomegranate.Update += Pomegranate_Update; ;
        }

        private static void PhysicalObject_ctor(On.PhysicalObject.orig_ctor orig, PhysicalObject self, AbstractPhysicalObject abstractPhysicalObject)
        {
            orig(self, abstractPhysicalObject);
            if (CustomEdible.edibleDatas.ContainsKey(Plugin.SlugName) &&
                CustomEdible.edibleDatas[Plugin.SlugName].edibleDatas.Any(d => d.edibleType == self.abstractPhysicalObject.type) &&
                !PhysicalObjectData.TryGetValue(self, out _))
                PhysicalObjectData.Add(self, new PhysicalObjectEx(self));
        }

        //食用腐化蓝果眩晕概率和眩晕时间都减少，获取食物点增加
        private static void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
        {
            int stun = -1;
            if (PlayerHooks.PlayerData.TryGetValue(self, out var player))
            {
                stun = self.stun;
            }

            orig(self, edible);

            if (PlayerHooks.PlayerData.TryGetValue(self, out player) && edible is DangleFruit && (edible as DangleFruit).AbstrConsumable.rotted)
            {
                if (stun != -1)
                    self.stun = stun;
                float value = Random.value;
                if (value < 0.15f)//0.3f
                {
                    self.Stun(80);//120
                    self.warpExhausionTime = Mathf.Max(280, self.warpExhausionTime);
                    return;
                }
                if (value < 0.35f)//0.65f
                {
                    self.Stun(40);//60
                }
                self.AddQuarterFood();
            }
        }

        //食用腐化爆米花眩晕概率和眩晕时间都减少，获取食物点增加
        private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (PlayerHooks.PlayerData.TryGetValue(self, out var player))
            {
                if (self.eatExternalFoodSourceCounter > 0)
                {
                    //self.eatExternalFoodSourceCounter--;
                    if (self.eatExternalFoodSourceCounter < 2)//1
                    {
                        if (self.externalFoodSourceRotten)
                        {
                            self.AddQuarterFood();
                            self.AddQuarterFood();
                            float value = Random.value;
                            if (value < 0.1f)//0.2f
                            {
                                self.Stun(80);//120
                                self.warpExhausionTime = Mathf.Max(280, self.warpExhausionTime);
                            }
                            else if (value < 0.25f)//0.5f
                            {
                                self.Stun(40);//60
                            }
                        }
                        else
                        {
                            self.AddFood(1);
                        }
                        self.dontEatExternalFoodSourceCounter = 45;
                        self.handOnExternalFoodSource = null;
                        self.room.PlaySound(SoundID.Slugcat_Bite_Fly, self.mainBodyChunk);
                        self.externalFoodSourceRotten = false;
                    }
                }

            }
            orig(self, eu);
        }

        //无需把石榴摔到地上，直接就能啃食
        private static void Pomegranate_Update(On.Pomegranate.orig_Update orig, Pomegranate self, bool eu)
        {
            orig(self, eu);
            if (self.grabbedBy != null && self.grabbedBy.Count > 0)//self.currentlyEdible
            {
                int l = 0;
                while (l < (ModManager.MSC ? self.room.abstractRoom.creatures.Count : self.room.game.Players.Count))
                {
                    Player player;
                    if (ModManager.MSC)
                    {
                        Creature realizedCreature = self.room.abstractRoom.creatures[l].realizedCreature;
                        if (realizedCreature != null && realizedCreature is Player && realizedCreature.Consious)
                        {
                            player = (realizedCreature as Player);
                            goto IL_7C6;
                        }
                    }
                    else if (self.room.game.Players[l].realizedCreature != null && self.room.game.Players[l].realizedCreature.Consious)
                    {
                        player = (self.room.game.Players[l].realizedCreature as Player);
                        goto IL_7C6;
                    }
                    IL_AF9:
                    l++;
                    continue;
                    IL_7C6:
                    if (PlayerHooks.PlayerData.TryGetValue(player, out var playerEX))
                    {
                        if (player.room != self.room || player.handOnExternalFoodSource != null || 
                            player.eatExternalFoodSourceCounter >= 1 || player.dontEatExternalFoodSourceCounter >= 1 || 
                            player.FoodInStomach >= player.MaxFoodInStomach || 
                            player.wantToPickUp <= 0// && !player.input[0].pckp && player.touchedNoInputCounter <= 5 &&
                            )// || player.FreeHand() <= -1
                        {
                            goto IL_AF9;
                        }
                        Vector2 pos = player.mainBodyChunk.pos;
                        if (!Custom.DistLess(pos, self.firstChunk.pos, 35f))
                        {
                            goto IL_AF9;
                        }
                        player.handOnExternalFoodSource = new Vector2?(self.firstChunk.pos + Custom.DirVec(pos, self.firstChunk.pos) * 5f);
                        player.eatExternalFoodSourceCounter = 15;
                        if (Random.value < 0.75f)
                        {
                            self.room.AddObject(new Pomegranate.PomegranateSeed(self.firstChunk.pos + Custom.RNV() * Random.Range(0f, 10f), Custom.RNV() * 4f + new Vector2(0f, 3f), self.baseColor));
                        }
                        for (int m = 0; m < Random.Range(4, 8); m++)//for (int m = 0; m < Random.Range(1, 3); m++)
                        {
                            self.room.AddObject(new WaterDrip(self.firstChunk.pos + Custom.RNV() * Random.Range(0f, 10f), Custom.RNV() * Random.Range(0f, 2f), false));
                        }
                        for (int n = 2; n < self.smashedBits.GetLength(0); n++)
                        {
                            self.smashedBits[n, 2] += new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f));
                        }
                        if (Random.value < 0.5f)
                        {
                            self.room.PlaySound(SoundID.Slime_Mold_Terrain_Impact, self.firstChunk.pos, 0.5f, 1f);
                        }
                        if (self.room.game.IsStorySession && player.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && self.room.game.GetStorySession.playerSessionRecords != null)
                        {
                            self.room.game.GetStorySession.playerSessionRecords[(player.abstractCreature.state as PlayerState).playerNumber].AddEat(self);
                        }
                        if (player.graphicsModule != null)
                        {
                            (player.graphicsModule as PlayerGraphics).LookAtPoint(self.firstChunk.pos, 100f);
                            goto IL_AF9;
                        }
                        goto IL_AF9;
                    }
                }
            }
        }
    }

    public class PhysicalObjectEx
    {
        public int bitesLeft;

        public PhysicalObjectEx(PhysicalObject self)
        {
            bitesLeft = 3;
        }
    }
}
