using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOutsider.World_Hooks;
using MoreSlugcats;
using System.Reflection;
using SlugBase.Features;
using UnityEngine;
using RWCustom;
using CoralBrain;
using Expedition;
using HUD;
using JollyCoop;
using JollyCoop.JollyMenu;
using Noise;

namespace TheOutsider.CustomLore.CustomCreature
{
    internal class MothPupHooks
    {
        private static BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
        private static BindingFlags methodFlags = BindingFlags.Static | BindingFlags.Public;
        public delegate bool orig_isNPC(Player self);
        public delegate bool orig_isSlugpup(Player self);

        public static void Init()
        {
            On.Player.ctor += Player_ctor;
            //On.MoreSlugcats.SlugNPCAI.TheoreticallyEatMeat += SlugNPCAI_TheoreticallyEatMeat;
            On.Player.GetInitialSlugcatClass += Player_GetInitialSlugcatClass;
            //On.AbstractCreature.MSCStateAI += AbstractCreature_MSCStateAI;
            /*
            Hook hook1 = new Hook(typeof(Player).GetProperty("isNPC", MothPupHooks.propFlags).GetGetMethod(), 
                                 typeof(MothPupHooks).GetMethod("Player_isNPC", MothPupHooks.methodFlags));*/
            Hook hook2 = new Hook(typeof(Player).GetProperty("isSlugpup", MothPupHooks.propFlags).GetGetMethod(),
                                 typeof(MothPupHooks).GetMethod("Player_isSlugpup", MothPupHooks.methodFlags));
        }
        
        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            self.dynamicRunSpeed = new float[2];
            self.abstractPhysicalObject = abstractCreature;
            if (abstractCreature != null)
            {
                abstractCreature.realizedObject = self;
            }
            //abstractCreature.state = new PlayerState(abstractCreature, i, this.GetStorySession.saveState.saveStateNumber, false);
            self.grabbedBy = new List<Creature.Grasp>();
            self.collisionRange = 50f;
            self.dead = self.State.dead;
            self.stun = 0;
            self.enteringShortCut = null;
            self.inShortcut = false;
            self.shortcutDelay = 0;
            self.mainBodyChunkIndex = 0;
            if (self.Template.grasps > 0)
            {
                self.grasps = new Creature.Grasp[self.Template.grasps];
            }
            self.stunDamageType = Creature.DamageType.None;
            if (ModManager.MSC)
            {
                self.inputWithoutDiagonals = null;
                self.lastInputWithoutDiagonals = null;
                self.inputWithDiagonals = null;
                self.lastInputWithDiagonals = null;
            }
            if (ModManager.MMF && MMF.cfgKeyItemTracking.Value && world.game.IsStorySession && !world.singleRoomWorld)
            {
                self.lastGoodTrackerSpawnRoom = abstractCreature.Room.name;
                self.lastGoodTrackerSpawnRegion = world.region.name;
                self.lastGoodTrackerSpawnCoord = abstractCreature.pos;
            }
            self.customPlayerGravity = 0.9f;
            self.godDeactiveTimer = 400f;
            self.lastPingRegion = "";
            self.GetInitialSlugcatClass();
            if (self.isSlugpup)
            {
                self.npcStats = new Player.NPCStats(self);
            }
            if (ModManager.MSC && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer && self.abstractCreature.world.game.IsStorySession && self.abstractCreature.world.game.GetStorySession.saveState.deathPersistentSaveData.altEnding)
            {
                (self.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = 0;
                (self.abstractCreature.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = 0;
            }
            self.feetStuckPos = null;
            self.standing = false;
            self.animationFrame = 0;
            self.superLaunchJump = 0;
            self.directionBoosts = new float[4];
            float num = 0.7f * self.slugcatStats.bodyWeightFac;
            self.bodyChunks = new BodyChunk[2];
            self.bodyChunks[0] = new BodyChunk(self, 0, new Vector2(0f, 0f), 9f, num / 2f);
            self.bodyChunks[1] = new BodyChunk(self, 1, new Vector2(0f, 0f), 8f, num / 2f);
            self.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[1];
            self.bodyChunkConnections[0] = new PhysicalObject.BodyChunkConnection(self.bodyChunks[0], self.bodyChunks[1], 17f, PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
            self.input = new Player.InputPackage[10];
            for (int i = 0; i < 10; i++)
            {
                int num2 = self.playerState.playerNumber;
                if (ModManager.MSC && abstractCreature.world.game.IsArenaSession && abstractCreature.world.game.GetArenaGameSession.chMeta != null)
                {
                    num2 = 0;
                }
                self.input[i] = new Player.InputPackage(self.AI == null && world.game.rainWorld.options.controls[num2].gamePad, world.game.rainWorld.options.controls[num2].GetActivePreset(), 0, 0, false, false, false, false, false);
            }
            self.animation = Player.AnimationIndex.None;
            self.bodyMode = Player.BodyModeIndex.Default;
            self.airFriction = 0.999f;
            self.gravity = 0.9f;
            self.bounce = 0.1f;
            self.surfaceFriction = 0.5f;
            self.collisionLayer = 1;
            self.waterFriction = 0.96f;
            self.buoyancy = 0.95f;
            self.airInLungs = 1f;
            self.flipDirection = 1;
            if ((double)UnityEngine.Random.value < 0.5)
            {
                self.flipDirection = -1;
            }
            self.room = world.GetAbstractRoom(abstractCreature.pos.room).realizedRoom;
            if (ModManager.MSC && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                self.maxGodTime = (float)((int)(200f + 40f * (float)self.Karma));
                if (self.room != null && self.room.world.name == "HR")
                {
                    self.maxGodTime = 560f;
                }
                self.godTimer = self.maxGodTime;
                self.tongue = new Player.Tongue(self, 0);
            }
            else
            {
                self.tongue = null;
            }
            self.swimBits = new CoralCircuit.CircuitBit[2];
            if (self.room != null)
            {
                if (self.AI == null)
                {
                    self.glowing = ((self.room.game.session is StoryGameSession && (self.room.game.session as StoryGameSession).saveState.theGlow) || self.room.game.setupValues.playerGlowing);
                    if (self.room.game.session is StoryGameSession && (self.room.game.session as StoryGameSession).saveState.swallowedItems != null && self.playerState.playerNumber < (self.room.game.session as StoryGameSession).saveState.swallowedItems.Length && (self.room.game.session as StoryGameSession).saveState.swallowedItems[self.playerState.playerNumber] != "" && (self.room.game.session as StoryGameSession).saveState.swallowedItems[self.playerState.playerNumber] != "0")
                    {
                        string text = (self.room.game.session as StoryGameSession).saveState.swallowedItems[self.playerState.playerNumber];
                        if (text.Contains("<oA>"))
                        {
                            self.objectInStomach = SaveState.AbstractPhysicalObjectFromString(world, text);
                        }
                        else if (text.Contains("<cA>"))
                        {
                            self.objectInStomach = SaveState.AbstractCreatureFromString(world, text, false);
                        }
                        if (self.objectInStomach != null)
                        {
                            self.objectInStomach.pos = abstractCreature.pos;
                        }
                    }
                }
                else
                {
                    self.glowing = (self.playerState as PlayerNPCState).Glowing;
                    if (self.room.game.session is StoryGameSession)
                    {
                        self.objectInStomach = (self.playerState as PlayerNPCState).StomachObject;
                    }
                }
            }
            if (abstractCreature.Room.world.game.IsArenaSession)
            {
                self.glowing = abstractCreature.Room.world.game.GetArenaGameSession.playersGlowing;
            }
            if (world.GetAbstractRoom(abstractCreature.pos.room).shelter)
            {
                self.sleepCounter = 100;
                for (int j = 0; j < world.GetAbstractRoom(abstractCreature.pos.room).creatures.Count; j++)
                {
                    if (world.GetAbstractRoom(abstractCreature.pos.room).creatures[j].creatureTemplate.type != CreatureTemplate.Type.Slugcat && (!ModManager.MSC || world.GetAbstractRoom(abstractCreature.pos.room).creatures[j].creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC))
                    {
                        self.sleepCounter = 0;
                    }
                }
            }
            if (ModManager.MSC && self.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && abstractCreature.Room.world.game.IsStorySession)
            {
                AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(abstractCreature.Room.world, MoreSlugcatsEnums.AbstractObjectType.SingularityBomb, null, self.room.GetWorldCoordinate(self.mainBodyChunk.pos), abstractCreature.Room.world.game.GetNewID());
                abstractCreature.Room.AddEntity(abstractPhysicalObject);
                abstractPhysicalObject.RealizeInRoom();
            }
            if (self.SlugCatClass == SlugcatStats.Name.Red && !self.playerState.isGhost)
            {
                if (self.room.game.devToolsActive && self.room.game.rainWorld.buildType == RainWorld.BuildType.Development && self.room.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.Dev && self.objectInStomach == null)
                {
                    self.objectInStomach = new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.NSHSwarmer, null, abstractCreature.pos, world.game.GetNewID(), -1, -1, null);
                }
                self.spearOnBack = new Player.SpearOnBack(self);
                if (abstractCreature.world.game.IsStorySession && abstractCreature.world.game.GetStorySession.saveState.cycleNumber >= RedsIllness.RedsCycles(abstractCreature.world.game.GetStorySession.saveState.redExtraCycles) && (!ModManager.CoopAvailable || !(abstractCreature.world.game.StoryCharacter != SlugcatStats.Name.Red)))
                {
                    self.redsIllness = new RedsIllness(self, Math.Abs(RedsIllness.RedsCycles(abstractCreature.world.game.GetStorySession.saveState.redExtraCycles) - abstractCreature.world.game.GetStorySession.saveState.cycleNumber));
                }
                if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode)
                {
                    self.redsIllness = null;
                }
            }
            if (ModManager.Expedition && Custom.rainWorld.ExpeditionMode && ExpeditionGame.activeUnlocks.Contains("unl-backspear"))
            {
                self.spearOnBack = new Player.SpearOnBack(self);
            }
            self.standStillOnMapButton = world.game.IsStorySession;
            if (ModManager.MSC)
            {
                if (!self.isSlugpup)
                {
                    self.slugOnBack = new Player.SlugOnBack(self);
                }
                if (self.AI == null)
                {
                    ChatlogData.setHostPlayer(self);
                }
                if (world.game.rainWorld.setup.forcePup || (world.game.autoPupStoryCompanionPlayers && self.playerState.playerNumber > 0))
                {
                    self.setPupStatus(true);
                }
                else if (self.playerState.forceFullGrown)
                {
                    self.setPupStatus(false);
                }
                else if (self.isSlugpup)
                {
                    self.setPupStatus(true);
                }
                else if (self.playerState.isPup)
                {
                    self.setPupStatus(true);
                }
                if (self.isSlugpup && self.playerState is PlayerNPCState)
                {
                    self.glowing = (self.playerState as PlayerNPCState).Glowing;
                    self.SetMalnourished((self.playerState as PlayerNPCState).Malnourished || self.dead);
                }
            }
            if (ModManager.CoopAvailable)
            {
                self.bool1 = (Custom.rainWorld.options.jollyPlayerOptionsArray[self.playerState.playerNumber].customPlayerName == JollyCustom.Test1());
                if (!self.isSlugpup)
                {
                    self.slugOnBack = new Player.SlugOnBack(self);
                }
                if (self.playerState.isPup || self.bool1)
                {
                    self.setPupStatus(true);
                }
            }

            Player_Hooks.PlayerHooks.PlayerData.Add(self, new PlayerEx(self));
        }
        /*
        public static void AbstractCreature_MSCStateAI(On.AbstractCreature.orig_MSCStateAI orig, AbstractCreature self)
        {
            orig(self);

            if (self.creatureTemplate.type == MothPupCritob.MothPup)
            {
                self.state = new MothPupState(self, 0);
                self.abstractAI = new SlugNPCAbstractAI(self.world, self);
            }
        }*/

        public static void Player_GetInitialSlugcatClass(On.Player.orig_GetInitialSlugcatClass orig, Player self)
        {
            Plugin.Log("self.abstractCreature == null?" + (self.abstractCreature == null));
            Plugin.Log("self.abstractCreature.creatureTemplate == null?" + (self.abstractCreature.creatureTemplate == null));
            if (self.abstractCreature.creatureTemplate.type == MothPupCritob.MothPup) 
            {
                self.SlugCatClass = Plugin.MothPup;
                return;
            }
            orig(self);
        }

        /*
        public static bool Player_isNPC(MothPupHooks.orig_isNPC orig, Player self)
        {
            bool result = orig(self);
            if (self.abstractCreature.creatureTemplate.type == MothPupCritob.MothPup)
                result = true;
            return result;
        }*/
        public static bool Player_isSlugpup(MothPupHooks.orig_isSlugpup orig, Player self)
        {
            bool result = orig(self);
            if (self.SlugCatClass == Plugin.MothPup)
                result = true;
            return result;
        }

        private static bool SlugNPCAI_TheoreticallyEatMeat(On.MoreSlugcats.SlugNPCAI.orig_TheoreticallyEatMeat orig, SlugNPCAI self, Creature crit, bool excludeCentipedes)
        {
            bool result = orig(self, crit, excludeCentipedes);
            if (self is MothPupAI)
                result = false;
            return result;
        }

        private static SlugNPCAI.Food SlugNPCAI_GetFoodType(On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI self, PhysicalObject food)
        {
            if (food is DangleFruit)
            {
                return SlugNPCAI.Food.DangleFruit;
            }
            if (food is SwollenWaterNut)
            {
                return SlugNPCAI.Food.WaterNut;
            }
            if (food is JellyFish)
            {
                return SlugNPCAI.Food.JellyFish;
            }
            if (food is SlimeMold)
            {
                return SlugNPCAI.Food.SlimeMold;
            }
            if (food is EggBugEgg)
            {
                return SlugNPCAI.Food.EggBugEgg;
            }
            if (food is FireEgg)
            {
                return SlugNPCAI.Food.FireEgg;
            }
            if (food is SeedCob || (food is SlimeMold && food.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.Seed))
            {
                return SlugNPCAI.Food.Popcorn;
            }
            if (food is GooieDuck)
            {
                return SlugNPCAI.Food.GooieDuck;
            }
            if (food is LillyPuck)
            {
                return SlugNPCAI.Food.LillyPuck;
            }
            if (food is GlowWeed)
            {
                return SlugNPCAI.Food.GlowWeed;
            }
            if (food is DandelionPeach)
            {
                return SlugNPCAI.Food.DandelionPeach;
            }
            if (food is OracleSwarmer)
            {
                return SlugNPCAI.Food.Neuron;
            }
            if (food is Centipede)
            {
                if (!(food as Centipede).Small)
                {
                    return SlugNPCAI.Food.Centipede;
                }
                return SlugNPCAI.Food.SmallCentipede;
            }
            else
            {
                if (food is VultureGrub)
                {
                    return SlugNPCAI.Food.VultureGrub;
                }
                if (food is SmallNeedleWorm)
                {
                    return SlugNPCAI.Food.SmallNeedleWorm;
                }
                if (food is Hazer)
                {
                    return SlugNPCAI.Food.Hazer;
                }
                return SlugNPCAI.Food.NotCounted;
            }
        }
    }
}
