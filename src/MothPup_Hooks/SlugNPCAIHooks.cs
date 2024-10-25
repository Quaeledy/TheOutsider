using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System.Linq;
using System.Runtime.CompilerServices;
using TheOutsider.Player_Hooks;
using UnityEngine;
using Custom = RWCustom.Custom;
using Random = UnityEngine.Random;

namespace TheOutsider.MothPup_Hooks
{
    public class SlugNPCAIHooks
    {
        public static readonly ConditionalWeakTable<Player, ParentVariables> parentVariablesCWT = new();
        public static readonly ConditionalWeakTable<PlayerNPCState, PupNPCState> pupStateCWT = new();
        public static readonly ConditionalWeakTable<AbstractCreature, PupAbstract> pupAbstractCWT = new();

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
            //IL.MoreSlugcats.SlugNPCAI.ctor += IL_SlugNPCAI_ctor;
            //On.MoreSlugcats.SlugNPCAI.SocialEvent += SlugNPCAI_SocialEvent;
        }
        /*
        public void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (PostIsInit) return;

                if (ModManager.ActiveMods.Any(mod => mod.id == "dressmyslugcat"))
                {
                    PupsPlusModCompat.SetupDMSSprites();
                }
                if (ModManager.ActiveMods.Any(mod => mod.id == "yeliah.slugpupFieldtrip"))
                {
                    SlugpupSafari = true;
                }
                if (ModManager.ActiveMods.Any(mod => mod.id == "rgbpups"))
                {
                    RainbowPups = true;
                }
                if (ModManager.ActiveMods.Any(mod => mod.id == "pearlcat"))
                {
                    Pearlcat = true;
                }
                if (ModManager.ActiveMods.Any(mod => mod.id == "NoirCatto.BeastMasterPupExtras"))
                {
                    BeastMasterPupExtras = true;
                }
                if (ModManager.ActiveMods.Any(mod => mod.id == "slime-cubed.devconsole"))
                {
                    PupsPlusModCompat.RegisterSpawnPupCommand();
                    Logger.LogInfo("spawn_pup command registered");
                    //PupsPlusModCompat.RegisterPupsPlusDebugCommands();
                }

                PostIsInit = true;
            }
            catch (Exception ex)
            {
                Logger.LogError("Pups+ PostModsInit failed to load!");
                Logger.LogError(ex);
                throw;
            }
        }
        private static void IL_SlugNPCAI_ctor(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.Before, 
                i => i.MatchNewobj<ItemTracker>()))
            {
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldc_I4_0);
            }
        }
        */
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
                //pupVariables.pathingVisualizer?.VisualizeConnections();
                /*MovementConnection movementConnection = (self.pathFinder as StandardPather).FollowPath(self.creature.pos, true);
                if (self.grabTarget == null && self.abstractAI.destination.room == self.abstractAI.parent.Room.index &&
                    (new Vector2((float)self.abstractAI.destination.x, (float)self.abstractAI.destination.y) - new Vector2((float)self.abstractAI.parent.pos.x, (float)self.abstractAI.parent.pos.y)).magnitude < 1.5f)
                {
                    movementConnection = default(MovementConnection);
                }*/
                Player.InputPackage inputPackage = new Player.InputPackage(false, Options.ControlSetup.Preset.None, self.cat.input[0].x, self.cat.input[0].y, self.cat.input[0].jmp, self.cat.input[0].thrw, self.cat.input[0].pckp, self.cat.input[0].mp, self.cat.input[0].crouchToggle);
                if (self.abstractAI.parent.pos.y < self.abstractAI.destination.y)
                {
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
                            inputPackage.jmp = true;/*
                            if (Plugin.optionsMenuInstance.infiniteFlight.Value || self.behaviorType == SlugNPCAI.BehaviorType.Fleeing)
                                self.cat.wantToJump = 5;*/
                        }
                    }
                    if (self.jumping && (!player.isFlying || self.behaviorType == SlugNPCAI.BehaviorType.Fleeing || Plugin.optionsMenuInstance.infiniteFlight.Value))
                    {
                        if (self.forceJump == 3)
                        {
                            inputPackage.jmp = true;
                            inputPackage.x = self.jumpDir;
                            self.cat.wantToJump = 5;
                        }
                    }
                    //inputPackage.y = 1;
                }
                if (self.abstractAI.parent.pos.y <= self.abstractAI.destination.y)
                {
                    if ((self.jumping || player.isFlying) && !self.OnAnyBeam() &&
                        self.cat.room.GetTile(self.cat.abstractCreature.pos.Tile).AnyBeam)
                    {
                        inputPackage.y = 1;
                    }
                }
                if (player.isFlying)
                {
                    if (self.abstractAI.parent.pos.x != self.abstractAI.destination.x)
                        inputPackage.x = (self.abstractAI.parent.pos.x < self.abstractAI.destination.x) ? 1 : -1;
                    else
                        inputPackage.x = 0;

                    if ((self.abstractAI.parent.pos.y < self.abstractAI.destination.y ||
                        Mathf.Abs(self.abstractAI.parent.pos.x - self.abstractAI.destination.x) >= 4) &&
                        player.flutterTimeAdd > player.upFlightTime && Plugin.optionsMenuInstance.infiniteFlight.Value)
                    {
                        inputPackage.jmp = true;
                        self.cat.wantToJump = 5;
                        if (Mathf.Abs(self.abstractAI.parent.pos.x - self.abstractAI.destination.x) >= 4)
                            inputPackage.x = (self.abstractAI.parent.pos.x < self.abstractAI.destination.x) ? 1 : -1;
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
                                    //self.cat.standing = true;
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
                /*
                if (self.nap)
                {
                    if (Mathf.Clamp01(0.06f / self.creature.personality.energy) > Random.Range(0.35f, 1f) || self.cat.emoteSleepCounter > 1.4f)
                    {
                        self.cat.emoteSleepCounter += Mathf.Clamp(0.0008f / self.creature.personality.energy, 0.0008f, 0.05f);
                        if (self.cat.emoteSleepCounter > 1.4f)
                        {
                            if (self.cat.graphicsModule != null)
                            {
                                (self.cat.graphicsModule as PlayerGraphics).blink = 5;
                            }
                            self.cat.sleepCurlUp = Mathf.SmoothStep(self.cat.sleepCurlUp, 1f, self.cat.emoteSleepCounter - 1.4f);
                        }
                        else
                        {
                            self.cat.sleepCurlUp = Mathf.Max(0f, self.cat.sleepCurlUp - 0.1f);
                        }
                    }
                }
                else
                {
                    self.cat.emoteSleepCounter = 0f;
                }
                if (pupVariables.wantsToRegurgitate)
                {
                    self.cat.PupRegurgitate();
                }
                if (pupVariables.wantsToSwallowObject && self.cat.grasps[0]?.grabbed != null)
                {
                    self.cat.PupSwallowObject(0);
                }
                if (pupVariables.giftedItem != null)
                {
                    bool giftedTracked = false;
                    foreach(var rep in self.itemTracker.items)
                    {
                        if (rep.representedItem != pupVariables.giftedItem) continue;
                        giftedTracked = true;
                    }
                    if (pupVariables.giftedItem.realizedObject == null || !giftedTracked)
                    {
                        pupVariables.giftedItem = null;
                    }
                }
                if (self.behaviorType == SlugNPCAI.BehaviorType.OnHead || self.behaviorType == SlugNPCAI.BehaviorType.BeingHeld)
                {
                    if (self.cat.grasps[0]?.grabbed != null)
                    {
                        if (self.cat.Grabability(self.cat.grasps[0].grabbed) > Player.ObjectGrabability.TwoHands)
                        {
                            self.cat.ReleaseGrasp(0);
                        }
                    }
                }
                */
                //pupVariables.pathingVisualizer?.Update();
                //if (pupVariables.labelManager != null)
                //{
                //    pupVariables.labelManager.UpdateLabel("grabTarget", $"grabTarget: {(self.grabTarget != null ? self.grabTarget is Creature ? (self.grabTarget as Creature).abstractCreature.creatureTemplate.type : self.grabTarget.abstractPhysicalObject.type : "NULL")}", self.grabTarget != null);
                //    pupVariables.labelManager.UpdateLabel("giftedItem", $"giftedItem: {(pupVariables.giftedItem != null ? pupVariables.giftedItem is AbstractCreature ? (pupVariables.giftedItem as AbstractCreature).creatureTemplate.type : pupVariables.giftedItem.type : "NULL")}", pupVariables.giftedItem != null);
                //    pupVariables.labelManager.Update(self.cat.mainBodyChunk.pos + new Vector2(35f, 30f));
                //}
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

        public static bool TryGetParentVariables(Player self, out ParentVariables parentVariables)
        {
            if (self != null)
            {
                parentVariables = GetParentVariables(self);
            }
            else parentVariables = null;

            return parentVariables != null;
        }
        public static ParentVariables GetParentVariables(Player self)
        {
            if (self != null)
            {
                return parentVariablesCWT.GetValue(self, _ => new ParentVariables());
            }
            return null;
        }
        public static PupNPCState GetPupState(PlayerNPCState self)
        {
            if (self != null)
            {
                return pupStateCWT.GetValue(self, _ => new PupNPCState());
            }
            return null;
        }
    }

    public class PupNPCState // DONT CHANGE THIS FFS, BEASTMASTERPUPEXTRAS RELIES ON IT!!!!!
    {
        public SlugcatStats.Name Variant;
        public AbstractPhysicalObject PupsPlusStomachObject;
    }

    public class PupAbstract
    {
        public bool moth;
    }

    public class ParentVariables
    {
        public bool rotundPupExhaustion;
    }
}
