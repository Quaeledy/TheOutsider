using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using Expedition;
using Random = UnityEngine.Random;
using MoreSlugcats;
using System.Security.Permissions;
using SlugBase.Features;
//using TheOutsider.EmgTx;

namespace TheOutsider.Player_Hooks
{
    public class FoodHooks
    {
        public static void Init()
        {
            //check if rotund world is enabled (our mass increases if we are full)
            try
            {

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

            /*
            On.Player.SwallowObject += Player_SwallowObject;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.GrabUpdate += Player_GrabUpdate_Rotund;
            On.PlayerGraphics.Update += PlayerGraphics_Update_Swallow;
            
            //On.SlugcatHand.Update += SlugcatHand_Update;
            */
        }
        /*
        public static void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            //如果是蛾猫
            if (PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.IsOutsider)
            {
                if (grasp < 0 || (self as Creature).grasps[grasp] == null)
                {
                    return;
                }
                AbstractPhysicalObject abstractPhysicalObject = (self as Creature).grasps[grasp].grabbed.abstractPhysicalObject;
                if (abstractPhysicalObject is AbstractSpear)
                {
                    (abstractPhysicalObject as AbstractSpear).stuckInWallCycles = 0;
                }
                //给下面的条件判断定义一下是不是素食
                bool isFood = (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.BubbleGrass || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Mushroom || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.WaterNut || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall);
                //check if rotund world is enabled (our mass increases if we are full)
                bool rotund = self.TotalMass > (0.7f * self.slugcatStats.bodyWeightFac);

                if (isFood && (self.FoodInStomach < self.MaxFoodInStomach || self.IsJollyPlayer || rotund))
                {
                    if (ModManager.MMF && self.room.game.session is StoryGameSession)
                    {
                        (self.room.game.session as StoryGameSession).RemovePersistentTracker(abstractPhysicalObject);
                    }
                    self.ReleaseGrasp(grasp);
                    abstractPhysicalObject.realizedObject.RemoveFromRoom();
                    abstractPhysicalObject.Abstractize((self as Creature).abstractCreature.pos);
                    abstractPhysicalObject.Room.RemoveEntity(abstractPhysicalObject);
                    //吃闪光果、鞭炮草
                    if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant)
                    {
                        self.AddFood(1);
                        self.AddQuarterFood();
                        self.AddQuarterFood();
                        if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant)
                        {
                            self.Hypothermia = Mathf.Min(0, self.Hypothermia - 0.3f);
                        }
                    }
                    //吃蝙蝠草、气泡草、未泡开的泡水果
                    if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.BubbleGrass || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.WaterNut)
                    {
                        self.AddFood(1);
                    }
                    //吃子弹菇
                    if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Mushroom)
                    {
                        self.AddQuarterFood();
                    }
                    //吃烟雾果
                    if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall)
                    {
                        self.Die();
                    }
                }
            }

            orig(self, grasp);
        }

        public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            if (!PlayerHooks.PlayerData.TryGetValue(self, out var player) || !player.IsOutsider)
            {
                return;
            }
            //如果是蛾猫
            //给下面的条件判断定义一下是不是素食
            bool[] isFood = new bool[2];
            for (int i = 0; i < 2; i++)
            {
                if ((self as Creature).grasps[i] != null)
                {
                    AbstractPhysicalObject abstractPhysicalObject = (self as Creature).grasps[i].grabbed.abstractPhysicalObject;
                    isFood[i] = (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.BubbleGrass || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Mushroom || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.WaterNut || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall);
                }
            }
            //check if rotund world is enabled (our mass increases if we are full)
            bool rotund = self.TotalMass > (0.7f * self.slugcatStats.bodyWeightFac);

            bool flag = ((self.input[0].x == 0 && self.input[0].y == 0 && !self.input[0].jmp && !self.input[0].thrw) || (ModManager.MMF && self.input[0].x == 0 && self.input[0].y == 1 && !self.input[0].jmp && !self.input[0].thrw && (self.bodyMode != Player.BodyModeIndex.ClimbingOnBeam || self.animation == Player.AnimationIndex.BeamTip || self.animation == Player.AnimationIndex.StandOnBeam))) && ((self as Creature).mainBodyChunk.submersion < 0.5f || self.isRivulet);
            bool flag3 = false;

            int num4 = -1;
            int num5 = -1;
            int num6 = -1;

            self.craftingObject = false;

            if (flag)
            {
                int num7 = -1;
                int num8 = 0;
                while (num5 < 0 && num8 < 2)
                {
                    if ((self as Creature).grasps[num8] != null && (self as Creature).grasps[num8].grabbed is IPlayerEdible && ((self as Creature).grasps[num8].grabbed as IPlayerEdible).Edible)
                    {
                        num5 = num8;
                    }
                    num8++;
                }
                if ((num5 == -1 || (self.FoodInStomach >= self.MaxFoodInStomach && !((self as Creature).grasps[num5].grabbed is KarmaFlower) && !((self as Creature).grasps[num5].grabbed is Mushroom))) && (self.CanPutSpearToBack || self.CanPutSlugToBack))
                {
                    int num9 = 0;
                    while (num7 < 0 && num4 < 0 && num6 < 0 && num9 < 2)
                    {
                        if ((self as Creature).grasps[num9] != null)
                        {
                            if ((self.CanPutSlugToBack && (self as Creature).grasps[num9].grabbed is Player && !((self as Creature).grasps[num9].grabbed as Player).dead) || self.CanIPutDeadSlugOnBack((self as Creature).grasps[num9].grabbed as Player))
                            {
                                num6 = num9;
                            }
                            else if (self.CanPutSpearToBack && (self as Creature).grasps[num9].grabbed is Spear)
                            {
                                num4 = num9;
                            }
                            else if (self.CanBeSwallowed((self as Creature).grasps[num9].grabbed))
                            {
                                num7 = num9;
                            }
                        }
                        num9++;
                    }
                }
                if (self.input[0].pckp)
                {
                    if (ModManager.MSC && self.FreeHand() == -1 && self.GraspsCanBeCrafted())
                    {
                        self.craftingObject = true;
                        flag3 = true;
                        num5 = -1;
                    }
                    if (num6 > -1 || self.CanRetrieveSlugFromBack)
                    {
                        self.slugOnBack.increment = true;
                    }
                    else if (num4 > -1 || self.CanRetrieveSpearFromBack)
                    {
                        self.spearOnBack.increment = true;
                    }
                    else if (num7 > -1)
                    {
                        flag3 = true;
                    }
                }
                if (num5 > -1 && self.wantToPickUp < 1 && (self.input[0].pckp || self.eatCounter <= 15) && (self as Creature).Consious && Custom.DistLess((self as Creature).mainBodyChunk.pos, (self as Creature).mainBodyChunk.lastPos, 3.6f))
                {
                    if (self.FoodInStomach < self.MaxFoodInStomach || (self as Creature).grasps[num5].grabbed is KarmaFlower || (self as Creature).grasps[num5].grabbed is Mushroom)
                    {
                        flag3 = false;
                    }
                }
            }
            if (ModManager.MMF && (self as Creature).mainBodyChunk.submersion >= 0.5f)
            {
                flag3 = false;
            }
            if (flag3)
            {
                if (!ModManager.MMF || self.input[0].y == 0)
                {
                    self.swallowAndRegurgitateCounter++;

                    if ((isFood[0] || isFood[1]) && (self.FoodInStomach < self.MaxFoodInStomach || rotund) && self.swallowAndRegurgitateCounter > 90)
                    {
                        for (int num13 = 0; num13 < 2; num13++)
                        {
                            if (isFood[num13] && (self as Creature).grasps[num13] != null && self.CanBeSwallowed((self as Creature).grasps[num13].grabbed))
                            {
                                (self as Creature).bodyChunks[0].pos += Custom.DirVec((self as Creature).grasps[num13].grabbed.firstChunk.pos, (self as Creature).bodyChunks[0].pos) * 2f;
                                self.SwallowObject(num13);
                                if (self.spearOnBack != null)
                                {
                                    self.spearOnBack.interactionLocked = true;
                                }
                                if ((ModManager.MSC || ModManager.CoopAvailable) && self.slugOnBack != null)
                                {
                                    self.slugOnBack.interactionLocked = true;
                                }
                                self.swallowAndRegurgitateCounter = 0;
                                ((self as Creature).graphicsModule as PlayerGraphics).swallowing = 20;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static void Player_GrabUpdate_Rotund(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            if (PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.IsOutsider)
            {
                //给下面的条件判断定义一下是不是素食
                bool[] isFood = new bool[2];
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i] != null)
                    {
                        AbstractPhysicalObject abstractPhysicalObject = self.grasps[i].grabbed.abstractPhysicalObject;
                        isFood[i] = (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.BubbleGrass || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Mushroom || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.WaterNut || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall);
                    }
                }

                //check if rotund world is enabled (our mass increases if we are full)
                bool rotund = self.TotalMass > (0.7f * self.slugcatStats.bodyWeightFac);

                //If our food is full and we are trying to swallow something
                if (self.FoodInStomach >= self.MaxFoodInStomach && self.swallowAndRegurgitateCounter > 88 && !rotund)
                {
                    //check each hand to see if it is food
                    for (int i = 0; i < 2; i++)
                    {
                        if (isFood[i])
                        {
                            //reverse the swallow progress so it never fully completes
                            self.swallowAndRegurgitateCounter--;
                        }
                    }
                }
            }

            orig(self, eu);
        }

        public static void PlayerGraphics_Update_Swallow(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsOutsider)
            {
                orig(self);
            }
            else
            {
                //给下面的条件判断定义一下是不是额外素食
                bool[] isFood = new bool[2];
                for (int i = 0; i < 2; i++)
                {
                    if ((self.player as Creature).grasps[i] != null)
                    {
                        AbstractPhysicalObject abstractPhysicalObject = (self.player as Creature).grasps[i].grabbed.abstractPhysicalObject;
                        isFood[i] = (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.BubbleGrass || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Mushroom || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.WaterNut || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall);
                    }
                }
                //check if rotund world is enabled (our mass increases if we are full)
                bool rotund = self.player.TotalMass > (0.7f * self.player.slugcatStats.bodyWeightFac);

                //用很傻的办法绕过原版的吐东西检测
                AbstractPhysicalObject objectInStomach = self.player.objectInStomach;

                if ((isFood[0] || isFood[1]) && (self.player.FoodInStomach < self.player.MaxFoodInStomach || rotund))
                {
                    self.player.objectInStomach = null;
                }

                orig(self);

                if ((isFood[0] || isFood[1]) && (self.player.FoodInStomach < self.player.MaxFoodInStomach || rotund))
                {
                    self.player.objectInStomach = objectInStomach;
                }
            }
        }*/
    }
}
