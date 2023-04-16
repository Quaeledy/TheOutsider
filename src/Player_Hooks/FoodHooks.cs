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
using static MonoMod.InlineRT.MonoModRule;

namespace TheOutsider.Player_Hooks
{
    public class FoodHooks
    {
        public static void Init()
        {
            On.Player.SwallowObject += Player_SwallowObject;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.PlayerGraphics.Update += PlayerGraphics_Update_Swallow;
            On.SlugcatHand.Update += SlugcatHand_Update;
        }

        public static void Player_SwallowObject(On.Player.orig_SwallowObject orig, Player self, int grasp)
        {
            if (!PlayerHooks.PlayerData.TryGetValue(PlayerEx.playerSelf, out var player) || !player.IsMoth)
            {
                orig(self, grasp);
            }
            //如果是蛾猫
            else
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
                if ((abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.BubbleGrass || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Mushroom || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.WaterNut || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall) && (self.FoodInStomach < self.MaxFoodInStomach || self.IsJollyPlayer))
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
                        PlayerEx.playerSelf.AddFood(1);
                        PlayerEx.playerSelf.AddQuarterFood();
                        PlayerEx.playerSelf.AddQuarterFood();
                    }
                    //吃蝙蝠草、气泡草、未泡开的泡水果
                    if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.BubbleGrass || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.WaterNut)
                    {
                        PlayerEx.playerSelf.AddFood(1);
                    }
                    //吃子弹菇
                    if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Mushroom)
                    {
                        PlayerEx.playerSelf.AddQuarterFood();
                    }
                    //吃烟雾果
                    if (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall)
                    {
                        self.Die();
                    }
                }
                else
                {
                    self.objectInStomach = abstractPhysicalObject;
                    if (ModManager.MMF && self.room.game.session is StoryGameSession)
                    {
                        (self.room.game.session as StoryGameSession).RemovePersistentTracker(self.objectInStomach);
                    }
                    self.ReleaseGrasp(grasp);
                    self.objectInStomach.realizedObject.RemoveFromRoom();
                    self.objectInStomach.Abstractize((self as Creature).abstractCreature.pos);
                    self.objectInStomach.Room.RemoveEntity(self.objectInStomach);
                }
                BodyChunk mainBodyChunk = (self as Creature).mainBodyChunk;
                mainBodyChunk.vel.y = mainBodyChunk.vel.y + 2f;
                self.room.PlaySound(SoundID.Slugcat_Swallow_Item, (self as Creature).mainBodyChunk);
            }
        }

        public static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {

            if (!PlayerHooks.PlayerData.TryGetValue(PlayerEx.playerSelf, out var player) || !player.IsMoth)
            {
                orig(self, eu);
            }
            //如果是蛾猫
            else
            {
                if (self.spearOnBack != null)
                {
                    self.spearOnBack.Update(eu);
                }
                if ((ModManager.MSC || ModManager.CoopAvailable) && self.slugOnBack != null)
                {
                    self.slugOnBack.Update(eu);
                }
                bool flag = ((self.input[0].x == 0 && self.input[0].y == 0 && !self.input[0].jmp && !self.input[0].thrw) || (ModManager.MMF && self.input[0].x == 0 && self.input[0].y == 1 && !self.input[0].jmp && !self.input[0].thrw && (self.bodyMode != Player.BodyModeIndex.ClimbingOnBeam || self.animation == Player.AnimationIndex.BeamTip || self.animation == Player.AnimationIndex.StandOnBeam))) && ((self as Creature).mainBodyChunk.submersion < 0.5f || self.isRivulet);
                bool flag2 = false;
                bool flag3 = false;
                self.craftingObject = false;
                int num = -1;
                int num2 = -1;
                bool flag4 = false;
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

                if (ModManager.MSC && !self.input[0].pckp && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
                {
                    PlayerGraphics.TailSpeckles tailSpecks = ((self as Creature).graphicsModule as PlayerGraphics).tailSpecks;
                    if (tailSpecks.spearProg > 0f)
                    {
                        tailSpecks.setSpearProgress(Mathf.Lerp(tailSpecks.spearProg, 0f, 0.05f));
                        if (tailSpecks.spearProg < 0.025f)
                        {
                            tailSpecks.setSpearProgress(0f);
                        }
                    }
                    else
                    {
                        self.smSpearSoundReady = false;
                    }
                }
                if (self.input[0].pckp && !self.input[1].pckp && self.switchHandsProcess == 0f && !self.isSlugpup)
                {
                    bool flag5 = (self as Creature).grasps[0] != null || (self as Creature).grasps[1] != null;
                    if ((self as Creature).grasps[0] != null && (self.Grabability((self as Creature).grasps[0].grabbed) == Player.ObjectGrabability.TwoHands || self.Grabability((self as Creature).grasps[0].grabbed) == Player.ObjectGrabability.Drag))
                    {
                        flag5 = false;
                    }
                    if (flag5)
                    {
                        if (self.switchHandsCounter == 0)
                        {
                            self.switchHandsCounter = 15;
                        }
                        else
                        {
                            self.room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, (self as Creature).mainBodyChunk);
                            self.switchHandsProcess = 0.01f;
                            self.wantToPickUp = 0;
                            self.noPickUpOnRelease = 20;
                        }
                    }
                    else
                    {
                        self.switchHandsProcess = 0f;
                    }
                }
                if (self.switchHandsProcess > 0f)
                {
                    float num3 = self.switchHandsProcess;
                    self.switchHandsProcess += 0.083333336f;
                    if (num3 < 0.5f && self.switchHandsProcess >= 0.5f)
                    {
                        self.room.PlaySound(SoundID.Slugcat_Switch_Hands_Complete, (self as Creature).mainBodyChunk);
                        (self as Creature).SwitchGrasps(0, 1);
                    }
                    if (self.switchHandsProcess >= 1f)
                    {
                        self.switchHandsProcess = 0f;
                    }
                }
                int num4 = -1;
                int num5 = -1;
                int num6 = -1;
                if (flag)
                {
                    int num7 = -1;
                    if (ModManager.MSC)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if ((self as Creature).grasps[i] != null)
                            {
                                if ((self as Creature).grasps[i].grabbed is JokeRifle)
                                {
                                    num2 = i;
                                }
                                else if (JokeRifle.IsValidAmmo((self as Creature).grasps[i].grabbed))
                                {
                                    num = i;
                                }
                            }
                        }
                    }
                    int num8 = 0;
                    while (num5 < 0 && num8 < 2 && (!ModManager.MSC || self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear))
                    {
                        if ((self as Creature).grasps[num8] != null && (self as Creature).grasps[num8].grabbed is IPlayerEdible && ((self as Creature).grasps[num8].grabbed as IPlayerEdible).Edible)
                        {
                            num5 = num8;
                        }
                        num8++;
                    }
                    if ((num5 == -1 || (self.FoodInStomach >= self.MaxFoodInStomach && !((self as Creature).grasps[num5].grabbed is KarmaFlower) && !((self as Creature).grasps[num5].grabbed is Mushroom))) && (self.objectInStomach == null || self.CanPutSpearToBack || self.CanPutSlugToBack))
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
                    if (num5 > -1 && self.noPickUpOnRelease < 1)
                    {
                        if (!self.input[0].pckp)
                        {
                            int num10 = 1;
                            while (num10 < 10 && self.input[num10].pckp)
                            {
                                num10++;
                            }
                            if (num10 > 1 && num10 < 10)
                            {
                                self.PickupPressed();
                            }
                        }
                    }
                    else if (self.input[0].pckp && !self.input[1].pckp)
                    {
                        self.PickupPressed();
                    }
                    if (self.input[0].pckp)
                    {
                        if (ModManager.MSC && (self.FreeHand() == -1 || self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer) && self.GraspsCanBeCrafted())
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
                        else if ((num7 > -1 || self.objectInStomach != null || self.isGourmand) && (!ModManager.MSC || self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Spear))
                        {
                            flag3 = true;
                        }
                        if (num > -1 && num2 > -1)
                        {
                            flag4 = true;
                        }
                        if (ModManager.MSC && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear && ((self as Creature).grasps[0] == null || (self as Creature).grasps[1] == null) && num5 == -1 && self.input[0].y == 0)
                        {
                            PlayerGraphics.TailSpeckles tailSpecks2 = ((self as Creature).graphicsModule as PlayerGraphics).tailSpecks;
                            if (tailSpecks2.spearProg == 0f)
                            {
                                tailSpecks2.newSpearSlot();
                            }
                            if (tailSpecks2.spearProg < 0.1f)
                            {
                                tailSpecks2.setSpearProgress(Mathf.Lerp(tailSpecks2.spearProg, 0.11f, 0.1f));
                            }
                            else
                            {
                                if (!self.smSpearSoundReady)
                                {
                                    self.smSpearSoundReady = true;
                                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.SM_Spear_Pull, 0f, 1f, 1f + Random.value * 0.5f);
                                }
                                tailSpecks2.setSpearProgress(Mathf.Lerp(tailSpecks2.spearProg, 1f, 0.05f));
                            }
                            if (tailSpecks2.spearProg > 0.6f)
                            {
                                ((self as Creature).graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * ((tailSpecks2.spearProg - 0.6f) / 0.4f) * 2f;
                            }
                            if (tailSpecks2.spearProg > 0.95f)
                            {
                                tailSpecks2.setSpearProgress(1f);
                            }
                            if (tailSpecks2.spearProg == 1f)
                            {
                                self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.SM_Spear_Grab, 0f, 1f, 0.5f + Random.value * 1.5f);
                                self.smSpearSoundReady = false;
                                Vector2 pos = ((self as Creature).graphicsModule as PlayerGraphics).tail[(int)((float)((self as Creature).graphicsModule as PlayerGraphics).tail.Length / 2f)].pos;
                                for (int j = 0; j < 4; j++)
                                {
                                    Vector2 a = Custom.DirVec(pos, (self as Creature).bodyChunks[1].pos);
                                    self.room.AddObject(new WaterDrip(pos + Custom.RNV() * Random.value * 1.5f, Custom.RNV() * 3f * Random.value + a * Mathf.Lerp(2f, 6f, Random.value), false));
                                }
                                for (int k = 0; k < 5; k++)
                                {
                                    Vector2 a2 = Custom.RNV();
                                    self.room.AddObject(new Spark(pos + a2 * Random.value * 40f, a2 * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
                                }
                                int spearType = tailSpecks2.spearType;
                                tailSpecks2.setSpearProgress(0f);
                                AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, self.room.GetWorldCoordinate((self as Creature).mainBodyChunk.pos), self.room.game.GetNewID(), false);
                                self.room.abstractRoom.AddEntity(abstractSpear);
                                abstractSpear.pos = (self as Creature).abstractCreature.pos;
                                abstractSpear.RealizeInRoom();
                                Vector2 vector = (self as Creature).bodyChunks[0].pos;
                                Vector2 a3 = Custom.DirVec((self as Creature).bodyChunks[1].pos, (self as Creature).bodyChunks[0].pos);
                                if (Mathf.Abs((self as Creature).bodyChunks[0].pos.y - (self as Creature).bodyChunks[1].pos.y) > Mathf.Abs((self as Creature).bodyChunks[0].pos.x - (self as Creature).bodyChunks[1].pos.x) && (self as Creature).bodyChunks[0].pos.y > (self as Creature).bodyChunks[1].pos.y)
                                {
                                    vector += Custom.DirVec((self as Creature).bodyChunks[1].pos, (self as Creature).bodyChunks[0].pos) * 5f;
                                    a3 *= -1f;
                                    a3.x += 0.4f * (float)self.flipDirection;
                                    a3.Normalize();
                                }
                                abstractSpear.realizedObject.firstChunk.HardSetPosition(vector);
                                abstractSpear.realizedObject.firstChunk.vel = Vector2.ClampMagnitude((a3 * 2f + Custom.RNV() * Random.value) / abstractSpear.realizedObject.firstChunk.mass, 6f);
                                if (self.FreeHand() > -1)
                                {
                                    self.SlugcatGrab(abstractSpear.realizedObject, self.FreeHand());
                                }
                                if (abstractSpear.type == AbstractPhysicalObject.AbstractObjectType.Spear)
                                {
                                    (abstractSpear.realizedObject as Spear).Spear_makeNeedle(spearType, true);
                                    if (((self as Creature).graphicsModule as PlayerGraphics).useJollyColor)
                                    {
                                        (abstractSpear.realizedObject as Spear).jollyCustomColor = new Color?(PlayerGraphics.JollyColor(self.playerState.playerNumber, 2));
                                    }
                                }
                                self.wantToThrow = 0;
                            }
                        }
                    }
                    if (num5 > -1 && self.wantToPickUp < 1 && (self.input[0].pckp || self.eatCounter <= 15) && (self as Creature).Consious && Custom.DistLess((self as Creature).mainBodyChunk.pos, (self as Creature).mainBodyChunk.lastPos, 3.6f))
                    {
                        if ((self as Creature).graphicsModule != null)
                        {
                            ((self as Creature).graphicsModule as PlayerGraphics).LookAtObject((self as Creature).grasps[num5].grabbed);
                        }
                        if (ModManager.MSC && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint && (self.KarmaCap == 9 || (self.room.game.IsArenaSession && self.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType != MoreSlugcatsEnums.GameTypeID.Challenge) || (self.room.game.session is ArenaGameSession && self.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge && self.room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeMeta.ascended)) && (self as Creature).grasps[num5].grabbed is Fly && self.eatCounter < 1)
                        {
                            self.room.PlaySound(SoundID.Snail_Pop, (self as Creature).mainBodyChunk, false, 1f, 1.5f + Random.value);
                            self.eatCounter = 30;
                            self.room.AddObject(new ShockWave((self as Creature).grasps[num5].grabbed.firstChunk.pos, 25f, 0.8f, 4, false));
                            for (int l = 0; l < 5; l++)
                            {
                                self.room.AddObject(new Spark((self as Creature).grasps[num5].grabbed.firstChunk.pos, Custom.RNV() * 3f, Color.yellow, null, 25, 90));
                            }
                            (self as Creature).grasps[num5].grabbed.Destroy();
                            (self as Creature).grasps[num5].grabbed.abstractPhysicalObject.Destroy();
                            if (self.room.game.IsArenaSession)
                            {
                                self.AddFood(1);
                            }
                        }
                        flag2 = true;
                        if (self.FoodInStomach < self.MaxFoodInStomach || (self as Creature).grasps[num5].grabbed is KarmaFlower || (self as Creature).grasps[num5].grabbed is Mushroom)
                        {
                            flag3 = false;
                            if (self.spearOnBack != null)
                            {
                                self.spearOnBack.increment = false;
                            }
                            if ((ModManager.MSC || ModManager.CoopAvailable) && self.slugOnBack != null)
                            {
                                self.slugOnBack.increment = false;
                            }
                            if (self.eatCounter < 1)
                            {
                                self.eatCounter = 15;
                                self.BiteEdibleObject(eu);
                            }
                        }
                        else if (self.eatCounter < 20 && self.room.game.cameras[0].hud != null)
                        {
                            self.room.game.cameras[0].hud.foodMeter.RefuseFood();
                        }
                    }
                }
                else if (self.input[0].pckp && !self.input[1].pckp)
                {
                    self.PickupPressed();
                }
                else
                {
                    if (self.CanPutSpearToBack)
                    {
                        for (int m = 0; m < 2; m++)
                        {
                            if ((self as Creature).grasps[m] != null && (self as Creature).grasps[m].grabbed is Spear)
                            {
                                num4 = m;
                                break;
                            }
                        }
                    }
                    if (self.CanPutSlugToBack)
                    {
                        for (int n = 0; n < 2; n++)
                        {
                            if ((self as Creature).grasps[n] != null && (self as Creature).grasps[n].grabbed is Player && !((self as Creature).grasps[n].grabbed as Player).dead)
                            {
                                num6 = n;
                                break;
                            }
                        }
                    }
                    if (self.input[0].pckp && (num6 > -1 || self.CanRetrieveSlugFromBack))
                    {
                        self.slugOnBack.increment = true;
                    }
                    if (self.input[0].pckp && (num4 > -1 || self.CanRetrieveSpearFromBack))
                    {
                        self.spearOnBack.increment = true;
                    }
                }
                int num11 = 0;
                if (ModManager.MMF && ((self as Creature).grasps[0] == null || !((self as Creature).grasps[0].grabbed is Creature)) && (self as Creature).grasps[1] != null && (self as Creature).grasps[1].grabbed is Creature)
                {
                    num11 = 1;
                }
                if (ModManager.MSC && SlugcatStats.SlugcatCanMaul(self.SlugCatClass))
                {
                    if (self.input[0].pckp && (self as Creature).grasps[num11] != null && (self as Creature).grasps[num11].grabbed is Creature && (self.CanMaulCreature((self as Creature).grasps[num11].grabbed as Creature) || self.maulTimer > 0))
                    {
                        self.maulTimer++;
                        ((self as Creature).grasps[num11].grabbed as Creature).Stun(60);
                        self.MaulingUpdate(num11);
                        if (self.spearOnBack != null)
                        {
                            self.spearOnBack.increment = false;
                            self.spearOnBack.interactionLocked = true;
                        }
                        if (self.slugOnBack != null)
                        {
                            self.slugOnBack.increment = false;
                            self.slugOnBack.interactionLocked = true;
                        }
                        if ((self as Creature).grasps[num11] != null && self.maulTimer % 40 == 0)
                        {
                            self.room.PlaySound(SoundID.Slugcat_Eat_Meat_B, (self as Creature).mainBodyChunk);
                            self.room.PlaySound(SoundID.Drop_Bug_Grab_Creature, (self as Creature).mainBodyChunk, false, 1f, 0.76f);
                            if (RainWorld.ShowLogs)
                            {
                                Debug.Log("Mauled target");
                            }
                            if (!((self as Creature).grasps[num11].grabbed as Creature).dead)
                            {
                                for (int num12 = Random.Range(8, 14); num12 >= 0; num12--)
                                {
                                    self.room.AddObject(new WaterDrip(Vector2.Lerp((self as Creature).grasps[num11].grabbedChunk.pos, (self as Creature).mainBodyChunk.pos, Random.value) + (self as Creature).grasps[num11].grabbedChunk.rad * Custom.RNV() * Random.value, Custom.RNV() * 6f * Random.value + Custom.DirVec((self as Creature).grasps[num11].grabbed.firstChunk.pos, ((self as Creature).mainBodyChunk.pos + ((self as Creature).graphicsModule as PlayerGraphics).head.pos) / 2f) * 7f * Random.value + Custom.DegToVec(Mathf.Lerp(-90f, 90f, Random.value)) * Random.value * self.EffectiveRoomGravity * 7f, false));
                                }
                                Creature creature = (self as Creature).grasps[num11].grabbed as Creature;
                                creature.SetKillTag((self as Creature).abstractCreature);
                                creature.Violence((self as Creature).bodyChunks[0], new Vector2?(new Vector2(0f, 0f)), (self as Creature).grasps[num11].grabbedChunk, null, Creature.DamageType.Bite, 1f, 15f);
                                creature.stun = 5;
                                if (creature.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Inspector)
                                {
                                    creature.Die();
                                }
                            }
                            self.maulTimer = 0;
                            self.wantToPickUp = 0;
                            if ((self as Creature).grasps[num11] != null)
                            {
                                self.TossObject(num11, eu);
                                self.ReleaseGrasp(num11);
                            }
                            self.standing = true;
                        }
                        return;
                    }
                    if ((self as Creature).grasps[num11] != null && (self as Creature).grasps[num11].grabbed is Creature && ((self as Creature).grasps[num11].grabbed as Creature).Consious && !self.IsCreatureLegalToHoldWithoutStun((self as Creature).grasps[num11].grabbed as Creature))
                    {
                        if (RainWorld.ShowLogs)
                        {
                            Debug.Log("Lost hold of live mauling target");
                        }
                        self.maulTimer = 0;
                        self.wantToPickUp = 0;
                        self.ReleaseGrasp(num11);
                        return;
                    }
                }
                if (self.input[0].pckp && (self as Creature).grasps[num11] != null && (self as Creature).grasps[num11].grabbed is Creature && self.CanEatMeat((self as Creature).grasps[num11].grabbed as Creature) && ((self as Creature).grasps[num11].grabbed as Creature).Template.meatPoints > 0)
                {
                    self.eatMeat++;
                    self.EatMeatUpdate(num11);
                    if (!ModManager.MMF)
                    {
                    }
                    if (self.spearOnBack != null)
                    {
                        self.spearOnBack.increment = false;
                        self.spearOnBack.interactionLocked = true;
                    }
                    if ((ModManager.MSC || ModManager.CoopAvailable) && self.slugOnBack != null)
                    {
                        self.slugOnBack.increment = false;
                        self.slugOnBack.interactionLocked = true;
                    }
                    if ((self as Creature).grasps[num11] != null && self.eatMeat % 80 == 0 && (((self as Creature).grasps[num11].grabbed as Creature).State.meatLeft <= 0 || self.FoodInStomach >= self.MaxFoodInStomach))
                    {
                        self.eatMeat = 0;
                        self.wantToPickUp = 0;
                        self.TossObject(num11, eu);
                        self.ReleaseGrasp(num11);
                        self.standing = true;
                    }
                    return;
                }
                if (!self.input[0].pckp && (self as Creature).grasps[num11] != null && self.eatMeat > 60)
                {
                    self.eatMeat = 0;
                    self.wantToPickUp = 0;
                    self.TossObject(num11, eu);
                    self.ReleaseGrasp(num11);
                    self.standing = true;
                    return;
                }
                self.eatMeat = Custom.IntClamp(self.eatMeat - 1, 0, 50);
                self.maulTimer = Custom.IntClamp(self.maulTimer - 1, 0, 20);
                if (!ModManager.MMF || self.input[0].y == 0)
                {
                    if (flag2 && self.eatCounter > 0)
                    {
                        if (ModManager.MSC)
                        {
                            if (num5 <= -1 || (self as Creature).grasps[num5] == null || !((self as Creature).grasps[num5].grabbed is GooieDuck) || ((self as Creature).grasps[num5].grabbed as GooieDuck).bites != 6 || self.timeSinceSpawned % 2 == 0)
                            {
                                self.eatCounter--;
                            }
                            if (num5 > -1 && (self as Creature).grasps[num5] != null && (self as Creature).grasps[num5].grabbed is GooieDuck && ((self as Creature).grasps[num5].grabbed as GooieDuck).bites == 6 && self.FoodInStomach < self.MaxFoodInStomach)
                            {
                                ((self as Creature).graphicsModule as PlayerGraphics).BiteStruggle(num5);
                            }
                        }
                        else
                        {
                            self.eatCounter--;
                        }
                    }
                    else if (!flag2 && self.eatCounter < 40)
                    {
                        self.eatCounter++;
                    }
                }
                if (flag4 && self.input[0].y == 0)
                {
                    self.reloadCounter++;
                    if (self.reloadCounter > 40)
                    {
                        ((self as Creature).grasps[num2].grabbed as JokeRifle).ReloadRifle((self as Creature).grasps[num].grabbed);
                        BodyChunk mainBodyChunk = (self as Creature).mainBodyChunk;
                        mainBodyChunk.vel.y = mainBodyChunk.vel.y + 4f;
                        self.room.PlaySound(SoundID.Gate_Clamp_Lock, (self as Creature).mainBodyChunk, false, 0.5f, 3f + Random.value);
                        AbstractPhysicalObject abstractPhysicalObject = (self as Creature).grasps[num].grabbed.abstractPhysicalObject;
                        self.ReleaseGrasp(num);
                        abstractPhysicalObject.realizedObject.RemoveFromRoom();
                        abstractPhysicalObject.Room.RemoveEntity(abstractPhysicalObject);
                        self.reloadCounter = 0;
                    }
                }
                else
                {
                    self.reloadCounter = 0;
                }
                if (ModManager.MMF && (self as Creature).mainBodyChunk.submersion >= 0.5f)
                {
                    flag3 = false;
                }
                if (flag3)
                {
                    if (self.craftingObject)
                    {
                        self.swallowAndRegurgitateCounter++;
                        if (self.swallowAndRegurgitateCounter > 105)
                        {
                            self.SpitUpCraftedObject();
                            self.swallowAndRegurgitateCounter = 0;
                        }
                    }
                    else if (!ModManager.MMF || self.input[0].y == 0)
                    {
                        self.swallowAndRegurgitateCounter++;
                        
                        if ((isFood[0] || isFood[1]) && self.FoodInStomach < self.MaxFoodInStomach && self.swallowAndRegurgitateCounter > 90)
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
                        else if (((self.objectInStomach != null) || self.isGourmand || (ModManager.MSC && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)) && self.swallowAndRegurgitateCounter > 110)
                        {
                            bool flag6 = false;
                            if (self.isGourmand && self.objectInStomach == null)
                            {
                                flag6 = true;
                            }
                            if (!flag6 || (flag6 && self.FoodInStomach >= 1))
                            {
                                if (flag6)
                                {
                                    self.SubtractFood(1);
                                }
                                self.Regurgitate();
                            }
                            else
                            {
                                (self as Creature).firstChunk.vel += new Vector2(Random.Range(-1f, 1f), 0f);
                                self.Stun(30);
                            }
                            if (self.spearOnBack != null)
                            {
                                self.spearOnBack.interactionLocked = true;
                            }
                            if ((ModManager.MSC || ModManager.CoopAvailable) && self.slugOnBack != null)
                            {
                                self.slugOnBack.interactionLocked = true;
                            }
                            self.swallowAndRegurgitateCounter = 0;
                        }
                        else if (self.objectInStomach == null && self.swallowAndRegurgitateCounter > 90)
                        {
                            for (int num13 = 0; num13 < 2; num13++)
                            {
                                if ((self as Creature).grasps[num13] != null && self.CanBeSwallowed((self as Creature).grasps[num13].grabbed) && !isFood[num13])
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
                    else
                    {
                        if (self.swallowAndRegurgitateCounter > 0)
                        {
                            self.swallowAndRegurgitateCounter--;
                        }
                        if (self.eatCounter > 0)
                        {
                            self.eatCounter--;
                        }
                    }
                }
                else
                {
                    self.swallowAndRegurgitateCounter = 0;
                }
                for (int num14 = 0; num14 < (self as Creature).grasps.Length; num14++)
                {
                    if ((self as Creature).grasps[num14] != null && (self as Creature).grasps[num14].grabbed.slatedForDeletetion)
                    {
                        self.ReleaseGrasp(num14);
                    }
                }
                if ((self as Creature).grasps[0] != null && self.Grabability((self as Creature).grasps[0].grabbed) == Player.ObjectGrabability.TwoHands)
                {
                    self.pickUpCandidate = null;
                }
                else
                {
                    PhysicalObject physicalObject = (self.dontGrabStuff < 1) ? self.PickupCandidate(20f) : null;
                    if (self.pickUpCandidate != physicalObject && physicalObject != null && physicalObject is PlayerCarryableItem)
                    {
                        (physicalObject as PlayerCarryableItem).Blink();
                    }
                    self.pickUpCandidate = physicalObject;
                }
                if (self.switchHandsCounter > 0)
                {
                    self.switchHandsCounter--;
                }
                if (self.wantToPickUp > 0)
                {
                    self.wantToPickUp--;
                }
                if (self.wantToThrow > 0)
                {
                    self.wantToThrow--;
                }
                if (self.noPickUpOnRelease > 0)
                {
                    self.noPickUpOnRelease--;
                }
                if (self.input[0].thrw && !self.input[1].thrw && (!ModManager.MSC || !self.monkAscension))
                {
                    self.wantToThrow = 5;
                }
                if (self.wantToThrow > 0)
                {
                    if (ModManager.MSC && MMF.cfgOldTongue.Value && (self as Creature).grasps[0] == null && (self as Creature).grasps[1] == null && self.SaintTongueCheck())
                    {
                        Vector2 vector2 = new Vector2((float)self.flipDirection, 0.7f);
                        Vector2 normalized = vector2.normalized;
                        if (self.input[0].y > 0)
                        {
                            normalized = new Vector2(0f, 1f);
                        }
                        normalized = (normalized + (self as Creature).mainBodyChunk.vel.normalized * 0.2f).normalized;
                        self.tongue.Shoot(normalized);
                        self.wantToThrow = 0;
                    }
                    else
                    {
                        for (int num15 = 0; num15 < 2; num15++)
                        {
                            if ((self as Creature).grasps[num15] != null && self.IsObjectThrowable((self as Creature).grasps[num15].grabbed))
                            {
                                self.ThrowObject(num15, eu);
                                self.wantToThrow = 0;
                                break;
                            }
                        }
                    }
                    if ((ModManager.MSC || ModManager.CoopAvailable) && self.wantToThrow > 0 && self.slugOnBack != null && self.slugOnBack.HasASlug)
                    {
                        Player slugcat = self.slugOnBack.slugcat;
                        self.slugOnBack.SlugToHand(eu);
                        self.ThrowObject(0, eu);
                        float num16 = (self.ThrowDirection >= 0) ? Mathf.Max((self as Creature).bodyChunks[0].pos.x, (self as Creature).bodyChunks[1].pos.x) : Mathf.Min((self as Creature).bodyChunks[0].pos.x, (self as Creature).bodyChunks[1].pos.x);
                        for (int num17 = 0; num17 < slugcat.bodyChunks.Length; num17++)
                        {
                            slugcat.bodyChunks[num17].pos.y = (self as Creature).firstChunk.pos.y + 20f;
                            if (self.ThrowDirection < 0)
                            {
                                if (slugcat.bodyChunks[num17].pos.x > num16 - 8f)
                                {
                                    slugcat.bodyChunks[num17].pos.x = num16 - 8f;
                                }
                                if (slugcat.bodyChunks[num17].vel.x > 0f)
                                {
                                    slugcat.bodyChunks[num17].vel.x = 0f;
                                }
                            }
                            else if (self.ThrowDirection > 0)
                            {
                                if (slugcat.bodyChunks[num17].pos.x < num16 + 8f)
                                {
                                    slugcat.bodyChunks[num17].pos.x = num16 + 8f;
                                }
                                if (slugcat.bodyChunks[num17].vel.x < 0f)
                                {
                                    slugcat.bodyChunks[num17].vel.x = 0f;
                                }
                            }
                        }
                    }
                }
                if (self.wantToPickUp > 0)
                {
                    bool flag7 = true;
                    if (self.animation == Player.AnimationIndex.DeepSwim)
                    {
                        if ((self as Creature).grasps[0] == null && (self as Creature).grasps[1] == null)
                        {
                            flag7 = false;
                        }
                        else
                        {
                            for (int num18 = 0; num18 < 10; num18++)
                            {
                                if (self.input[num18].y > -1 || self.input[num18].x != 0)
                                {
                                    flag7 = false;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int num19 = 0; num19 < 5; num19++)
                        {
                            if (self.input[num19].y > -1)
                            {
                                flag7 = false;
                                break;
                            }
                        }
                    }
                    if (ModManager.MSC)
                    {
                        if ((self as Creature).grasps[0] != null && (self as Creature).grasps[0].grabbed is EnergyCell && (self as Creature).mainBodyChunk.submersion > 0f)
                        {
                            flag7 = false;
                        }
                        else if ((self as Creature).grasps[0] != null && (self as Creature).grasps[0].grabbed is EnergyCell && self.canJump <= 0 && self.bodyMode != Player.BodyModeIndex.Crawl && self.bodyMode != Player.BodyModeIndex.CorridorClimb && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && self.animation != Player.AnimationIndex.HangFromBeam && self.animation != Player.AnimationIndex.ClimbOnBeam && self.animation != Player.AnimationIndex.AntlerClimb && self.animation != Player.AnimationIndex.VineGrab && self.animation != Player.AnimationIndex.ZeroGPoleGrab)
                        {
                            ((self as Creature).grasps[0].grabbed as EnergyCell).Use(false);
                        }
                    }
                    if (!ModManager.MMF && (self as Creature).grasps[0] != null && self.HeavyCarry((self as Creature).grasps[0].grabbed))
                    {
                        flag7 = true;
                    }
                    if (flag7)
                    {
                        int num20 = -1;
                        for (int num21 = 0; num21 < 2; num21++)
                        {
                            if ((self as Creature).grasps[num21] != null)
                            {
                                num20 = num21;
                                break;
                            }
                        }
                        if (num20 > -1)
                        {
                            self.wantToPickUp = 0;
                            if (!ModManager.MSC || self.SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Artificer || !((self as Creature).grasps[num20].grabbed is Scavenger))
                            {
                                self.pyroJumpDropLock = 0;
                            }
                            if (self.pyroJumpDropLock == 0 && (!ModManager.MSC || self.wantToJump == 0))
                            {
                                self.ReleaseObject(num20, eu);
                                return;
                            }
                        }
                        else
                        {
                            if (self.spearOnBack != null && self.spearOnBack.spear != null && (self as Creature).mainBodyChunk.ContactPoint.y < 0)
                            {
                                self.room.socialEventRecognizer.CreaturePutItemOnGround(self.spearOnBack.spear, self);
                                self.spearOnBack.DropSpear();
                                return;
                            }
                            if ((ModManager.MSC || ModManager.CoopAvailable) && self.slugOnBack != null && self.slugOnBack.slugcat != null && (self as Creature).mainBodyChunk.ContactPoint.y < 0)
                            {
                                self.room.socialEventRecognizer.CreaturePutItemOnGround(self.slugOnBack.slugcat, self);
                                self.slugOnBack.DropSlug();
                                self.wantToPickUp = 0;
                                return;
                            }
                            if (ModManager.MSC && self.room != null && self.room.game.IsStorySession && self.room.game.GetStorySession.saveState.wearingCloak && self.AI == null)
                            {
                                self.room.game.GetStorySession.saveState.wearingCloak = false;
                                AbstractConsumable abstractConsumable = new AbstractConsumable(self.room.game.world, MoreSlugcatsEnums.AbstractObjectType.MoonCloak, null, self.room.GetWorldCoordinate((self as Creature).mainBodyChunk.pos), self.room.game.GetNewID(), -1, -1, null);
                                self.room.abstractRoom.AddEntity(abstractConsumable);
                                abstractConsumable.pos = (self as Creature).abstractCreature.pos;
                                abstractConsumable.RealizeInRoom();
                                (abstractConsumable.realizedObject as MoonCloak).free = true;
                                for (int num22 = 0; num22 < abstractConsumable.realizedObject.bodyChunks.Length; num22++)
                                {
                                    abstractConsumable.realizedObject.bodyChunks[num22].HardSetPosition((self as Creature).mainBodyChunk.pos);
                                }
                                self.dontGrabStuff = 15;
                                self.wantToPickUp = 0;
                                self.noPickUpOnRelease = 20;
                                return;
                            }
                        }
                    }
                    else if (self.pickUpCandidate != null)
                    {
                        if (self.pickUpCandidate is Spear && self.CanPutSpearToBack && (((self as Creature).grasps[0] != null && self.Grabability((self as Creature).grasps[0].grabbed) >= Player.ObjectGrabability.BigOneHand) || ((self as Creature).grasps[1] != null && self.Grabability((self as Creature).grasps[1].grabbed) >= Player.ObjectGrabability.BigOneHand) || ((self as Creature).grasps[0] != null && (self as Creature).grasps[1] != null)))
                        {
                            Debug.Log("spear straight to back");
                            self.room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, (self as Creature).mainBodyChunk);
                            self.spearOnBack.SpearToBack(self.pickUpCandidate as Spear);
                        }
                        else if (self.CanPutSlugToBack && self.pickUpCandidate is Player && (!(self.pickUpCandidate as Player).dead || self.CanIPutDeadSlugOnBack(self.pickUpCandidate as Player)) && (((self as Creature).grasps[0] != null && (self.Grabability((self as Creature).grasps[0].grabbed) > Player.ObjectGrabability.BigOneHand || (self as Creature).grasps[0].grabbed is Player)) || ((self as Creature).grasps[1] != null && (self.Grabability((self as Creature).grasps[1].grabbed) > Player.ObjectGrabability.BigOneHand || (self as Creature).grasps[1].grabbed is Player)) || ((self as Creature).grasps[0] != null && (self as Creature).grasps[1] != null) || self.bodyMode == Player.BodyModeIndex.Crawl))
                        {
                            Debug.Log("slugpup/player straight to back");
                            self.room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, (self as Creature).mainBodyChunk);
                            self.slugOnBack.SlugToBack(self.pickUpCandidate as Player);
                        }
                        else
                        {
                            int num23 = 0;
                            for (int num24 = 0; num24 < 2; num24++)
                            {
                                if ((self as Creature).grasps[num24] == null)
                                {
                                    num23++;
                                }
                            }
                            if (self.Grabability(self.pickUpCandidate) == Player.ObjectGrabability.TwoHands && num23 < 4)
                            {
                                for (int num25 = 0; num25 < 2; num25++)
                                {
                                    if ((self as Creature).grasps[num25] != null)
                                    {
                                        self.ReleaseGrasp(num25);
                                    }
                                }
                            }
                            else if (num23 == 0)
                            {
                                for (int num26 = 0; num26 < 2; num26++)
                                {
                                    if ((self as Creature).grasps[num26] != null && (self as Creature).grasps[num26].grabbed is Fly)
                                    {
                                        self.ReleaseGrasp(num26);
                                        break;
                                    }
                                }
                            }
                            int num27 = 0;
                            while (num27 < 2)
                            {
                                if ((self as Creature).grasps[num27] == null)
                                {
                                    if (self.pickUpCandidate is Creature)
                                    {
                                        self.room.PlaySound(SoundID.Slugcat_Pick_Up_Creature, self.pickUpCandidate.firstChunk, false, 1f, 1f);
                                    }
                                    else if (self.pickUpCandidate is PlayerCarryableItem)
                                    {
                                        for (int num28 = 0; num28 < self.pickUpCandidate.grabbedBy.Count; num28++)
                                        {
                                            if (self.pickUpCandidate.grabbedBy[num28].grabber.room == self.pickUpCandidate.grabbedBy[num28].grabbed.room)
                                            {
                                                self.pickUpCandidate.grabbedBy[num28].grabber.GrabbedObjectSnatched(self.pickUpCandidate.grabbedBy[num28].grabbed, self);
                                            }
                                            else
                                            {
                                                string str = "Item theft room mismatch? ";
                                                AbstractPhysicalObject abstractPhysicalObject2 = self.pickUpCandidate.grabbedBy[num28].grabbed.abstractPhysicalObject;
                                                Debug.Log(str + ((abstractPhysicalObject2 != null) ? abstractPhysicalObject2.ToString() : null));
                                            }
                                            self.pickUpCandidate.grabbedBy[num28].grabber.ReleaseGrasp(self.pickUpCandidate.grabbedBy[num28].graspUsed);
                                        }
                                        (self.pickUpCandidate as PlayerCarryableItem).PickedUp(self);
                                    }
                                    else
                                    {
                                        self.room.PlaySound(SoundID.Slugcat_Pick_Up_Misc_Inanimate, self.pickUpCandidate.firstChunk, false, 1f, 1f);
                                    }
                                    self.SlugcatGrab(self.pickUpCandidate, num27);
                                    if (self.pickUpCandidate.graphicsModule != null && self.Grabability(self.pickUpCandidate) < (Player.ObjectGrabability)5)
                                    {
                                        self.pickUpCandidate.graphicsModule.BringSpritesToFront();
                                        break;
                                    }
                                    break;
                                }
                                else
                                {
                                    num27++;
                                }
                            }
                        }
                        self.wantToPickUp = 0;
                    }
                }
            }
        }

        public static void PlayerGraphics_Update_Swallow(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsMoth)
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

                //用很傻的办法绕过原版的吐东西检测
                AbstractPhysicalObject objectInStomach = self.player.objectInStomach;
                
                if ((isFood[0] || isFood[1]) && self.player.FoodInStomach < self.player.MaxFoodInStomach)
                {
                    self.player.objectInStomach = null;
                }

                orig(self);

                if ((isFood[0] || isFood[1]) && self.player.FoodInStomach < self.player.MaxFoodInStomach)
                {
                    self.player.objectInStomach = objectInStomach;
                }
            }
        }

        public static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig, SlugcatHand self)
        {
            orig(self);

            if (!PlayerHooks.PlayerData.TryGetValue(self.owner.owner as Player, out var player) || !player.IsMoth)
            {
                return;
            }

            //给下面的条件判断定义一下是不是额外素食
            bool[] isFood = new bool[2];
            for (int i = 0; i < 2; i++)
            {
                if (((self.owner.owner as Player) as Creature).grasps[i] != null)
                {
                    AbstractPhysicalObject abstractPhysicalObject = ((self.owner.owner as Player) as Creature).grasps[i].grabbed.abstractPhysicalObject;
                    isFood[i] = (abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlareBomb || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FlyLure || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.BubbleGrass || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.Mushroom || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.WaterNut || abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall);
                }
            }

            if (((self.owner.owner as Player).swallowAndRegurgitateCounter > 10 && ((isFood[0] || isFood[1]) && (self.owner.owner as Player).FoodInStomach < (self.owner.owner as Player).MaxFoodInStomach)))
            {
                bool flag;
                if (self.reachingForObject)
                {
                    (self as Limb).mode = Limb.Mode.HuntAbsolutePosition;
                    flag = false;
                    self.reachingForObject = false;
                }
                else
                {
                    flag = self.EngageInMovement();
                }
                if (ModManager.MMF)
                {
                    if ((self.owner.owner as Player).grasps[self.limbNumber] != null && (self.owner.owner as Player).HeavyCarry((self.owner.owner as Player).grasps[self.limbNumber].grabbed))
                    {
                        flag = true;
                    }
                }
                else if (self.limbNumber == 0 && (self.owner.owner as Player).grasps[0] != null && (self.owner.owner as Player).HeavyCarry((self.owner.owner as Player).grasps[0].grabbed))
                {
                    flag = true;
                }
                if (flag)
                {
                    if (((self.owner.owner as Player).grasps[0] != null && (self.owner.owner as Player).HeavyCarry((self.owner.owner as Player).grasps[0].grabbed)) || (ModManager.MMF && (self.owner.owner as Player).grasps[1] != null && (self.owner.owner as Player).HeavyCarry((self.owner.owner as Player).grasps[1].grabbed)))
                    {
                        (self as Limb).mode = Limb.Mode.HuntAbsolutePosition;
                        BodyChunk bodyChunk;
                        if (ModManager.MMF)
                        {
                            bodyChunk = (((self.owner.owner as Player).grasps[0] != null && (self.owner.owner as Player).HeavyCarry((self.owner.owner as Player).grasps[0].grabbed)) ? (self.owner.owner as Player).grasps[0].grabbedChunk : (self.owner.owner as Player).grasps[1].grabbedChunk);
                        }
                        else
                        {
                            bodyChunk = (self.owner.owner as Player).grasps[0].grabbedChunk;
                        }
                        self.absoluteHuntPos = bodyChunk.pos + Custom.PerpendicularVector((self.connection.pos - bodyChunk.pos).normalized) * bodyChunk.rad * 0.8f * ((self.limbNumber == 0) ? -1f : 1f);
                        self.huntSpeed = 20f;
                        self.quickness = 1f;
                        flag = false;
                    }
                    else if ((self.owner.owner as Player).grasps[self.limbNumber] != null)
                    {
                        (self as Limb).mode = Limb.Mode.HuntRelativePosition;
                        if (ModManager.MSC && (self.owner.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Slugpup)
                        {
                            self.relativeHuntPos.x = (float)((self.owner.owner as Player).ThrowDirection * 3);
                        }
                        else
                        {
                            self.relativeHuntPos.x = -20f + 40f * (float)self.limbNumber;
                        }
                        self.relativeHuntPos.y = -12f;

                        if (((self.owner.owner as Player).swallowAndRegurgitateCounter > 10 && ((isFood[0] || isFood[1]) && (self.owner.owner as Player).FoodInStomach < (self.owner.owner as Player).MaxFoodInStomach)))
                        {
                            int numF1 = -1;
                            int numF2 = 0;
                            while (numF1 < 0 && numF2 < 2)
                            {
                                if (isFood[numF2] && (self.owner.owner as Player).grasps[numF2] != null)
                                {
                                    numF1 = numF2;
                                }
                                numF2++;
                            }
                            if (numF1 == self.limbNumber)
                            {
                                float numF3 = Mathf.InverseLerp(10f, 90f, (float)(self.owner.owner as Player).swallowAndRegurgitateCounter);
                                if (numF3 < 0.5f)
                                {
                                    self.relativeHuntPos *= Mathf.Lerp(0.9f, 0.7f, numF3 * 2f);
                                    self.relativeHuntPos.y = self.relativeHuntPos.y + Mathf.Lerp(2f, 4f, numF3 * 2f);
                                    self.relativeHuntPos.x = self.relativeHuntPos.x * Mathf.Lerp(1f, 1.2f, numF3 * 2f);
                                }
                                else
                                {
                                    (self.owner as PlayerGraphics).blink = 5;
                                    self.relativeHuntPos = new Vector2(0f, -4f) + Custom.RNV() * 2f * Random.value * Mathf.InverseLerp(0.5f, 1f, numF3);
                                    (self.owner as PlayerGraphics).head.vel += Custom.RNV() * 2f * Random.value * Mathf.InverseLerp(0.5f, 1f, numF3);
                                    self.owner.owner.bodyChunks[0].vel += Custom.RNV() * 0.2f * Random.value * Mathf.InverseLerp(0.5f, 1f, numF3);
                                }
                            }
                        }
                        self.relativeHuntPos.x = self.relativeHuntPos.x * (1f - Mathf.Sin((self.owner.owner as Player).switchHandsProcess * 3.1415927f));
                        if ((self.owner as PlayerGraphics).spearDir != 0f && (self.owner.owner as Player).bodyMode == Player.BodyModeIndex.Stand)
                        {
                            Vector2 b = Custom.DegToVec(180f + ((self.limbNumber == 0) ? -1f : 1f) * 8f + (float)(self.owner.owner as Player).input[0].x * 4f) * 12f;
                            b.y += Mathf.Sin((float)(self.owner.owner as Player).animationFrame / 6f * 2f * 3.1415927f) * 2f;
                            b.x -= Mathf.Cos((float)((self.owner.owner as Player).animationFrame + ((self.owner.owner as Player).leftFoot ? 0 : 6)) / 12f * 2f * 3.1415927f) * 4f * (float)(self.owner.owner as Player).input[0].x;
                            b.x += (float)(self.owner.owner as Player).input[0].x * 2f;
                            self.relativeHuntPos = Vector2.Lerp(self.relativeHuntPos, b, Mathf.Abs((self.owner as PlayerGraphics).spearDir));
                            if ((self.owner.owner as Player).grasps[self.limbNumber].grabbed is Weapon)
                            {
                                ((self.owner.owner as Player).grasps[self.limbNumber].grabbed as Weapon).ChangeOverlap(((self.owner as PlayerGraphics).spearDir > -0.4f && self.limbNumber == 0) || ((self.owner as PlayerGraphics).spearDir < 0.4f && self.limbNumber == 1));
                            }
                        }
                        flag = false;
                        if ((self.owner.owner as Creature).grasps[self.limbNumber].grabbed is Fly && !((self.owner.owner as Creature).grasps[self.limbNumber].grabbed as Fly).dead)
                        {
                            self.huntSpeed = Random.value * 5f;
                            self.quickness = Random.value * 0.3f;
                            self.vel += Custom.DegToVec(Random.value * 360f) * Random.value * Random.value * (Custom.DistLess(self.absoluteHuntPos, self.pos, 7f) ? 4f : 1.5f);
                            self.pos += Custom.DegToVec(Random.value * 360f) * Random.value * 4f;
                            (self.owner as PlayerGraphics).NudgeDrawPosition(0, Custom.DirVec((self.owner.owner as Creature).mainBodyChunk.pos, self.pos) * 3f * Random.value);
                            (self.owner as PlayerGraphics).head.vel += Custom.DirVec((self.owner.owner as Creature).mainBodyChunk.pos, self.pos) * 2f * Random.value;
                        }
                        else if ((self.owner.owner as Creature).grasps[self.limbNumber].grabbed is VultureMask)
                        {
                            self.relativeHuntPos *= 1f - ((self.owner.owner as Creature).grasps[self.limbNumber].grabbed as VultureMask).donned;
                        }
                    }
                }
                if (flag && (self as Limb).mode != Limb.Mode.Retracted)
                {
                    self.retractCounter++;
                    if ((float)self.retractCounter > 5f)
                    {
                        (self as Limb).mode = Limb.Mode.HuntAbsolutePosition;
                        self.pos = Vector2.Lerp(self.pos, self.owner.owner.bodyChunks[0].pos, Mathf.Clamp(((float)self.retractCounter - 5f) * 0.05f, 0f, 1f));
                        if (Custom.DistLess(self.pos, self.owner.owner.bodyChunks[0].pos, 2f) && self.reachedSnapPosition)
                        {
                            (self as Limb).mode = Limb.Mode.Retracted;
                        }
                        self.absoluteHuntPos = self.owner.owner.bodyChunks[0].pos;
                        self.huntSpeed = 1f + (float)self.retractCounter * 0.2f;
                        self.quickness = 1f;
                        return;
                    }
                }
                else
                {
                    self.retractCounter -= 10;
                    if (self.retractCounter < 0)
                    {
                        self.retractCounter = 0;
                    }
                }
            }

        }
    }
}
