using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using System;
using System.Linq;
using TheOutsider.Player_Hooks;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using Custom = RWCustom.Custom;
using Random = UnityEngine.Random;

namespace TheOutsider.MothPup_Hooks
{
    public class SlugNPCAIHooks
    {
        public static void Init()
        {
            On.MoreSlugcats.SlugNPCAI.ctor += SlugNPCAI_ctor;
            On.MoreSlugcats.SlugNPCAI.Update += SlugNPCAI_Update;
            On.MoreSlugcats.SlugNPCAI.TheoreticallyEatMeat += SlugNPCAI_TheoreticallyEatMeat;
            On.MoreSlugcats.SlugNPCAI.WantsToEatThis += SlugNPCAI_WantsToEatThis;
            On.MoreSlugcats.SlugNPCAI.HasEdible += SlugNPCAI_HasEdible;
            On.MoreSlugcats.SlugNPCAI.LethalWeaponScore += SlugNPCAI_LethalWeaponScore;
            On.MoreSlugcats.SlugNPCAI.GetFoodType += SlugNPCAI_GetFoodType;
            On.MoreSlugcats.SlugNPCAI.Move += SlugNPCAI_Move;
            On.MoreSlugcats.SlugNPCAI.DecideBehavior += SlugNPCAI_DecideBehavior;
            On.MoreSlugcats.SlugNPCAI.TravelPreference += SlugNPCAI_TravelPreference;
        }

        private static void SlugNPCAI_ctor(On.MoreSlugcats.SlugNPCAI.orig_ctor orig, SlugNPCAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.cat, out var player) && player.isMothNPC)
            {
                //pupVariables.pathingVisualizer = new(self, 5);
                //pupVariables.labelManager = new(self.cat);
                if (Plugin.optionsMenuInstance.infiniteFlight.Value)
                {
                    self.cat.abstractCreature.creatureTemplate.canFly = true;
                }
            }

        }
        private static void SlugNPCAI_Move(On.MoreSlugcats.SlugNPCAI.orig_Move orig, SlugNPCAI self)
        {
            orig(self);
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.cat, out var player) && player.isMothNPC && self.cat.room != null)
            {
                Player.InputPackage inputPackage = new Player.InputPackage(false, Options.ControlSetup.Preset.None, self.cat.input[0].x, self.cat.input[0].y, self.cat.input[0].jmp, self.cat.input[0].thrw, self.cat.input[0].pckp, self.cat.input[0].mp, self.cat.input[0].crouchToggle);
                MovementConnection movementConnection = (self.pathFinder as StandardPather).FollowPath(self.creature.pos, true);
                if (self.grabTarget == null && self.abstractAI.destination.room == self.abstractAI.parent.Room.index && (new Vector2((float)self.abstractAI.destination.x, (float)self.abstractAI.destination.y) - new Vector2((float)self.abstractAI.parent.pos.x, (float)self.abstractAI.parent.pos.y)).magnitude < 1.5f)
                {
                    movementConnection = default(MovementConnection);
                }
                if (!((self.HasEdible() & !self.IsFull) &&
                    (self.behaviorType == SlugNPCAI.BehaviorType.Following ||
                     self.behaviorType == SlugNPCAI.BehaviorType.Idle ||
                     (self.behaviorType == SlugNPCAI.BehaviorType.Attacking && self.AttackingPrey()))))
                {/*
                    if (self.jumping)
                    {
                        inputPackage.y = (self.catchPoles ? 1 : 0);
                        inputPackage.x = self.jumpDir;
                        inputPackage.jmp = true;
                        if ((self.cat.bodyMode != Player.BodyModeIndex.Default || self.OnHorizontalBeam()) && self.forceJump == 0)
                        {
                            self.jumping = false;
                        }
                    }
                    else */
                    if (movementConnection != default(MovementConnection))
                    {
                        WorldCoordinate startCoord = movementConnection.startCoord;
                        WorldCoordinate destinationCoord = movementConnection.destinationCoord;
                        /*
                        List<MovementConnection> upcoming = self.GetUpcoming();
                        if (upcoming != null)
                        {
                            for (int i = 0; i < upcoming.Count; i++)
                            {*/
                        //目的地在上方时起跳
                        if (self.abstractAI.parent.pos.y < destinationCoord.y)
                        {
                            //起跳
                            if (!self.jumping && !player.isFlying)
                            {
                                if (self.cat.animation != Player.AnimationIndex.HangFromBeam && self.cat.bodyMode != Player.BodyModeIndex.ClimbingOnBeam &&
                                    self.cat.animation != Player.AnimationIndex.GetUpOnBeam && self.cat.animation != Player.AnimationIndex.GetUpToBeamTip &&
                                    (self.cat.room.GetTile(self.abstractAI.parent.pos.Tile + new IntVector2(0, 1)).AnyBeam ||
                                     self.cat.room.GetTile(self.abstractAI.parent.pos.Tile + new IntVector2(0, 2)).AnyBeam ||
                                     self.cat.room.GetTile(self.abstractAI.parent.pos.Tile + new IntVector2(0, 3)).AnyBeam ||
                                     self.cat.room.GetTile(self.abstractAI.parent.pos.Tile + new IntVector2(0, 4)).AnyBeam ||
                                     Plugin.optionsMenuInstance.infiniteFlight.Value ||
                                     self.behaviorType == SlugNPCAI.BehaviorType.Fleeing))
                                {
                                    inputPackage.jmp = true;
                                    /*
                                    if (Plugin.optionsMenuInstance.infiniteFlight.Value || self.behaviorType == SlugNPCAI.BehaviorType.Fleeing)
                                        self.cat.wantToJump = 5;*/
                                }
                            }
                            //飞行
                            if (self.jumping && (!player.isFlying || self.behaviorType == SlugNPCAI.BehaviorType.Fleeing || Plugin.optionsMenuInstance.infiniteFlight.Value))
                            {
                                if (self.forceJump <= 3 && self.forceJump > 0)
                                {
                                    inputPackage.jmp = true;
                                    inputPackage.x = self.jumpDir;
                                    self.cat.wantToJump = 5;
                                }
                            }
                            //inputPackage.y = 1;
                        }
                        //未抓住杆子时抓杆子
                        if (self.abstractAI.parent.pos.y <= destinationCoord.y)
                        {
                            if ((self.jumping || player.isFlying) && !self.OnAnyBeam() &&
                                self.cat.room.GetTile(self.cat.abstractCreature.pos.Tile).AnyBeam)
                            {
                                inputPackage.y = 1;
                            }
                        }
                        if (player.isFlying)
                        {
                            //移动的左右方向
                            if (self.abstractAI.parent.pos.x != destinationCoord.x)
                                inputPackage.x = (self.abstractAI.parent.pos.x < destinationCoord.x) ? 1 : -1;
                            else
                                inputPackage.x = 0;
                            if ((self.abstractAI.parent.pos.y < destinationCoord.y ||
                                Mathf.Abs(self.abstractAI.parent.pos.x - destinationCoord.x) >= 4) &&
                                player.flutterTimeAdd > player.upFlightTime &&
                                (self.behaviorType == SlugNPCAI.BehaviorType.Fleeing || Plugin.optionsMenuInstance.infiniteFlight.Value))
                            {
                                inputPackage.jmp = true;
                                self.cat.wantToJump = 5;
                                if (Mathf.Abs(self.abstractAI.parent.pos.x - destinationCoord.x) >= 4)
                                    inputPackage.x = (self.abstractAI.parent.pos.x < destinationCoord.x) ? 1 : -1;
                                else
                                    inputPackage.y = 1;
                            }
                        }
                        /*
                        if (self.cat.gourmandExhausted)
                        {
                            if (!self.OnAnyBeam())
                            {
                                inputPackage.jmp = false;
                            }
                            if (inputPackage.x == 0 && inputPackage.y == 0 && !inputPackage.pckp && self.FunStuff)
                            {
                                if (Random.value < Mathf.Lerp(0f, 0.3f, Mathf.InverseLerp(0.1f, 0f, self.creature.personality.energy)))
                                {
                                    self.cat.standing = false;
                                }
                            }
                        }*/
                        if (!self.jumping && !player.isFlying && inputPackage.jmp)
                        {
                            //移动的左右方向
                            if (self.abstractAI.parent.pos.x != destinationCoord.x)
                                inputPackage.x = (self.abstractAI.parent.pos.x < destinationCoord.x) ? 1 : -1;
                            else
                                inputPackage.x = 0;
                            self.Jump(inputPackage.x, self.catchPoles, ref inputPackage);
                        }
                    }
                }
                self.cat.input[0] = inputPackage;
            }
        }
        private static void SlugNPCAI_Update(On.MoreSlugcats.SlugNPCAI.orig_Update orig, SlugNPCAI self)
        {
            orig(self);

            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.cat, out var player) && player.isMothNPC)
            {
                Player.InputPackage inputPackage = new Player.InputPackage(false, Options.ControlSetup.Preset.None, self.cat.input[0].x, self.cat.input[0].y, self.cat.input[0].jmp, self.cat.input[0].thrw, self.cat.input[0].pckp, self.cat.input[0].mp, self.cat.input[0].crouchToggle);
                //受蜘蛛威胁时释放闪光
                if (self.behaviorType == SlugNPCAI.BehaviorType.Fleeing || self.behaviorType == SlugNPCAI.BehaviorType.Attacking)
                {
                    for (int i = 0; i < self.tracker.CreaturesCount; i++)
                    {
                        if (self.threatTracker.GetThreatCreature(self.tracker.GetRep(i).representedCreature) != null && self.tracker.GetRep(i).representedCreature.realizedCreature != null)
                        {
                            Creature realizedCreature = self.tracker.GetRep(i).representedCreature.realizedCreature;
                            if (realizedCreature.bodyChunks.Length != 0 && !(realizedCreature.dead || realizedCreature.Blinded))
                            {
                                int num = UnityEngine.Random.Range(0, realizedCreature.bodyChunks.Length - 1);
                                bool isCreatureDislikeFlareAndBig = realizedCreature is BigSpider ||
                                                                    realizedCreature is MirosBird ||
                                                                    (realizedCreature is Vulture vulture && vulture.IsMiros);
                                bool isCreatureDislikeFlare = realizedCreature is Spider || isCreatureDislikeFlareAndBig;
                                bool inFlareRange = Custom.DistLess(self.cat.bodyChunks[1].pos, realizedCreature.mainBodyChunk.pos, player.burningRange) ||
                                                    (Custom.DistLess(self.cat.bodyChunks[1].pos, realizedCreature.mainBodyChunk.pos, player.burningRangeWithVisualContact) &&
                                                    self.cat.room.VisualContact(self.cat.bodyChunks[1].pos, realizedCreature.mainBodyChunk.pos));
                                bool canFlare = self.cat.FoodInStomach >= 1 && !Plugin.optionsMenuInstance.neverFlare.Value;
                                bool wantFlare = UnityEngine.Random.value < Mathf.Lerp(0.035f, 0.1f, Mathf.InverseLerp(0f, 1f, self.creature.personality.aggression)) ||
                                                 (isCreatureDislikeFlareAndBig && Custom.DistLess(self.cat.bodyChunks[1].pos, realizedCreature.mainBodyChunk.pos, 50f)) ||
                                                 (realizedCreature is Spider && (realizedCreature as Spider).centipede.totalMass > self.cat.TotalMass &&
                                                  Custom.DistLess(self.cat.bodyChunks[1].pos, (realizedCreature as Spider).centipede.FirstSpider.mainBodyChunk.pos, 200f));
                                if (isCreatureDislikeFlare && inFlareRange && canFlare && wantFlare)
                                {
                                    inputPackage.jmp = true;
                                    inputPackage.pckp = true;
                                    self.cat.wantToJump = 5;
                                    player.AIwantFlare = true;
                                }
                            }
                        }
                    }
                }
                self.cat.input[0] = inputPackage;
            }
        }
        private static void SlugNPCAI_DecideBehavior(On.MoreSlugcats.SlugNPCAI.orig_DecideBehavior orig, SlugNPCAI self)
        {
            orig(self);
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.cat, out var player) && player.isMothNPC)
            {
                //self.behaviorType = SlugNPCAI.BehaviorType.Fleeing;
            }
        }/*
        private static void SlugNPCAI_SocialEvent(On.MoreSlugcats.SlugNPCAI.orig_SocialEvent orig, SlugNPCAI self, SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
        {
            if (ID == SocialEventRecognizer.EventID.ItemTransaction && objectCrit == self.cat)
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.cat, out var player) && player.isMothNPC)
                {
                    pupVariables.giftedItem = involvedItem.abstractPhysicalObject;
                }
            }
                orig(self, ID, subjectCrit, objectCrit, involvedItem);
        }
        */
        private static float SlugNPCAI_LethalWeaponScore(On.MoreSlugcats.SlugNPCAI.orig_LethalWeaponScore orig, SlugNPCAI self, PhysicalObject obj, Creature target)
        {
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.cat, out var player) && player.isMothNPC)
            {
                if (obj is Spear)
                {
                    return 0.05f;
                }
            }
            return orig(self, obj, target);
        }
        private static bool SlugNPCAI_TheoreticallyEatMeat(On.MoreSlugcats.SlugNPCAI.orig_TheoreticallyEatMeat orig, SlugNPCAI self, Creature crit, bool excludeCentipedes)
        {
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.cat, out var player) && player.isMothNPC)
            {
                return false;
            }
            return orig(self, crit, excludeCentipedes);
        }
        private static bool SlugNPCAI_WantsToEatThis(On.MoreSlugcats.SlugNPCAI.orig_WantsToEatThis orig, SlugNPCAI self, PhysicalObject obj)
        {
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.cat, out var player) && player.isMothNPC)
            {
                if (CustomEdible.AdditionalConditions(self.cat) != null &&
                    CustomEdible.edibleDatas.ContainsKey(CustomEdible.AdditionalConditions(self.cat)) &&
                    CustomEdible.edibleDatas[CustomEdible.AdditionalConditions(self.cat)].edibleDatas.Any(d => d.forbidType == obj.abstractPhysicalObject.type))
                {
                    return false;
                }
                if (CustomEdible.AdditionalConditions(self.cat) != null &&
                    CustomEdible.edibleDatas.ContainsKey(CustomEdible.AdditionalConditions(self.cat)) &&
                    CustomEdible.edibleDatas[CustomEdible.AdditionalConditions(self.cat)].edibleDatas.Any(d => d.edibleType == obj.abstractPhysicalObject.type))
                {
                    return true;
                }
            }
            return orig(self, obj);

        }
        private static bool SlugNPCAI_HasEdible(On.MoreSlugcats.SlugNPCAI.orig_HasEdible orig, SlugNPCAI self)
        {
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.cat, out var player) && player.isMothNPC)
            {
                if (orig(self) && self.cat.grasps[0].grabbed is Creature or JellyFish or EggBug or FireEgg)
                {
                    return false;
                }
            }
            return orig(self);
        }
        private static SlugNPCAI.Food SlugNPCAI_GetFoodType(On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI self, PhysicalObject food)
        {
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.cat, out var player) && player.isMothNPC)
            {
                if (CustomEdible.AdditionalConditions(self.cat) != null &&
                    CustomEdible.edibleDatas.ContainsKey(CustomEdible.AdditionalConditions(self.cat)) &&
                    CustomEdible.edibleDatas[CustomEdible.AdditionalConditions(self.cat)].edibleDatas.Any(d => d.forbidType == food.abstractPhysicalObject.type))
                {
                    return SlugNPCAI.Food.NotCounted;
                }
                if (CustomEdible.AdditionalConditions(self.cat) != null &&
                    CustomEdible.edibleDatas.ContainsKey(CustomEdible.AdditionalConditions(self.cat)) &&
                    CustomEdible.edibleDatas[CustomEdible.AdditionalConditions(self.cat)].edibleDatas.Any(d => d.edibleType == food.abstractPhysicalObject.type))
                {
                    return SlugNPCAI.Food.WaterNut;
                }
            }
            return orig(self, food);
        }
        private static PathCost SlugNPCAI_TravelPreference(On.MoreSlugcats.SlugNPCAI.orig_TravelPreference orig, SlugNPCAI self, MovementConnection coord, PathCost cost)
        {
            PathCost origCost = orig(self, coord, cost);
            if (Plugin.optionsMenuInstance.infiniteFlight.Value)
            {
                origCost = new PathCost(Mathf.Min(cost.resistance, 1f) + self.threatTracker.ThreatOfTile(coord.destinationCoord, true) * 100f, cost.legality); ;
            }/*
            if (self.behaviorType != SlugNPCAI.BehaviorType.Fleeing)
            {
                origCost = cost;
                if (self.cat.gourmandExhausted)
                {
                    origCost += new PathCost(50f, PathCost.Legality.Unallowed);
                }
            }*/
            return origCost;
        }
    }
}
