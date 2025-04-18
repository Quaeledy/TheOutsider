﻿using HUD;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using RWCustom;
using System.Reflection;
using System;

namespace TheOutsider.MothPup_Hooks
{
    public class OtherHooks
    {
        private static BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
        private static BindingFlags methodFlags = BindingFlags.Static | BindingFlags.NonPublic;
        public delegate int orig_StoryGameSession_get_slugPupMaxCount(StoryGameSession self);

        public static void InitIL()
        {
            IL.MoreSlugcats.PlayerNPCState.CycleTick += PlayerNPCState_CycleTickIL;
            IL.HUD.FoodMeter.TrySpawnPupBars += FoodMeter_TrySpawnPupBarsIL;
            IL.ShelterDoor.Update += ShelterDoor_UpdateIL;
            IL.GhostCreatureSedater.Update += GhostCreatureSedater_UpdateIL;
            IL.SaveState.SessionEnded += SaveState_SessionEndedIL;
            IL.World.SpawnPupNPCs += World_SpawnPupNPCsIL;
        }

        public static void Init()
        {
            On.SlugcatStats.ctor += SlugcatStats_ctor;
            On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;
            On.SlugcatStats.SlugcatFoodMeter += SlugcatStats_SlugcatFoodMeter;

            On.MoreSlugcats.PlayerNPCState.ctor += PlayerNPCState_ctor;
            On.AbstractCreature.ctor += AbstractCreature_ctor;
            On.AImap.IsConnectionAllowedForCreature += AImap_IsConnectionAllowedForCreature;
            //On.AImap.TileAccessibleToCreature_IntVector2_CreatureTemplate += AImap_TileAccessibleToCreature;
            //On.SaveState.AbstractCreatureFromString += SaveState_AbstractCreatureFromString;

            Hook hook = new Hook(typeof(StoryGameSession).GetProperty(nameof(StoryGameSession.slugPupMaxCount), OtherHooks.propFlags).GetGetMethod(), typeof(OtherHooks).GetMethod(nameof(StoryGameSession_get_slugPupMaxCount), OtherHooks.methodFlags));
        }

        private static bool AImap_IsConnectionAllowedForCreature(On.AImap.orig_IsConnectionAllowedForCreature orig, AImap self, MovementConnection connection, CreatureTemplate crit)
        {
            bool result;
            if (self.IsConnectionForceAllowedForCreature(connection, crit, out result))
            {
                return result;
            }
            if (!self.WorldCoordinateAccessibleToCreature(connection.startCoord, crit) || !self.WorldCoordinateAccessibleToCreature(connection.destinationCoord, crit) || !crit.ConnectionResistance(connection.type).Allowed)
            {
                return false;
            }
            if (connection.type == MovementConnection.MovementType.DropToClimb || connection.type == MovementConnection.MovementType.DropToFloor || connection.type == MovementConnection.MovementType.ReachDown || connection.type == MovementConnection.MovementType.ReachOverGap || connection.type == MovementConnection.MovementType.ReachUp)
            {
                if (connection.type == MovementConnection.MovementType.ReachUp && self.getAItile(connection.StartTile).acc == AItile.Accessibility.Floor && self.getAItile(connection.DestTile).acc == AItile.Accessibility.Floor)
                {
                    return !self.TileAccessibleToCreature(connection.StartTile + new IntVector2(0, 1), crit) && (self.TileAccessibleToCreature(connection.StartTile + new IntVector2(0, -1), crit) || self.getAItile(connection.StartTile + new IntVector2(0, -1)).acc == AItile.Accessibility.Solid);
                }
                IntVector2 b = IntVector2.ClampAtOne(new IntVector2(connection.destinationCoord.x - connection.startCoord.x, connection.destinationCoord.y - connection.startCoord.y));
                return !self.TileAccessibleToCreature(connection.StartTile + b, crit);
            }
            else
            {
                if (connection.type == MovementConnection.MovementType.SemiDiagonalReach)
                {
                    IntVector2 b2 = IntVector2.ClampAtOne(new IntVector2(connection.destinationCoord.x - connection.startCoord.x, connection.destinationCoord.y - connection.startCoord.y));
                    if (Math.Abs(connection.destinationCoord.x - connection.startCoord.x) > Math.Abs(connection.destinationCoord.y - connection.startCoord.y))
                    {
                        b2.y = 0;
                    }
                    else
                    {
                        b2.x = 0;
                    }
                    return !self.TileAccessibleToCreature(connection.StartTile + b2, crit);
                }
                if (connection.type == MovementConnection.MovementType.DoubleReachUp)
                {
                    return self.getAItile(connection.StartTile).acc <= 
                        (AItile.Accessibility)crit.doubleReachUpConnectionParams[0] && 
                        self.getAItile(connection.StartTile + new IntVector2(0, 1)).acc <= (AItile.Accessibility)crit.doubleReachUpConnectionParams[1] && 
                        self.getAItile(connection.StartTile + new IntVector2(0, 2)).acc <= (AItile.Accessibility)crit.doubleReachUpConnectionParams[1] && 
                        self.getAItile(connection.StartTile + new IntVector2(0, 3)).acc <= (AItile.Accessibility)crit.doubleReachUpConnectionParams[2] && 
                        !self.TileAccessibleToCreature(connection.StartTile + new IntVector2(0, 1), crit) && !self.TileAccessibleToCreature(connection.StartTile + new IntVector2(0, 2), crit);
                }
                return connection.type != MovementConnection.MovementType.OpenDiagonal || (!self.TileAccessibleToCreature(new IntVector2(connection.startCoord.x, connection.destinationCoord.y), crit) && !self.TileAccessibleToCreature(new IntVector2(connection.destinationCoord.x, connection.startCoord.y), crit));
            }
        }

        private static int StoryGameSession_get_slugPupMaxCount(OtherHooks.orig_StoryGameSession_get_slugPupMaxCount orig, StoryGameSession self)
        {
            int result = orig(self);
            if ((self.saveState.progression.miscProgressionData.beaten_Gourmand_Full || MoreSlugcats.MoreSlugcats.chtUnlockSlugpups.Value) &&
                (self.saveState.saveStateNumber == Plugin.SlugName))
            {
                result = 2;
            }
            return result;
        }

        #region IL Hooks
        private static void PlayerNPCState_CycleTickIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Slugpup))))
            {
                Plugin.Log("PlayerNPCState_CycleTickIL MatchFind!");
                c.Emit(OpCodes.Ldarg_0); // self
                c.EmitDelegate((SlugcatStats.Name slugpup, PlayerNPCState self) =>
                {
                    SlugcatStats.Name result = slugpup;
                    if (self.player.creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        result = Plugin.Mothpup;
                    }
                    return result;
                });
            }
        }

        private static void FoodMeter_TrySpawnPupBarsIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
                i => i.Match(OpCodes.Call)))
            {
                Plugin.Log("FoodMeter_TrySpawnPupBarsIL MatchFind!");
                c.Emit(OpCodes.Ldarg_0); // self
                c.Emit(OpCodes.Ldloc_1); // i
                c.EmitDelegate((bool isSlugNPC, FoodMeter self, int i) =>
                {
                    bool isMothPup = false;
                    if ((self.hud.owner as Player).abstractCreature.Room.creatures[i].creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        isMothPup = true;
                    }
                    return isSlugNPC || isMothPup;
                });
            }
        }

        private static void ShelterDoor_UpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
                i => i.Match(OpCodes.Call)))
            {
                Plugin.Log("ShelterDoor_UpdateIL MatchFind!");
                c.Emit(OpCodes.Ldarg_0); // self
                c.Emit(OpCodes.Ldloc_S, (byte)11); // i
                c.EmitDelegate((bool notSlugNPC, ShelterDoor self, int i) =>
                {
                    bool notMothPup = true;
                    if (self.room.abstractRoom.creatures[i].creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        notMothPup = false;
                    }
                    return notSlugNPC && notMothPup;
                });
            }
        }

        private static void GhostCreatureSedater_UpdateIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
                i => i.Match(OpCodes.Call)))
            {
                Plugin.Log("GhostCreatureSedater_UpdateIL MatchFind!");
                c.Emit(OpCodes.Ldarg_0); // self
                c.Emit(OpCodes.Ldloc_2); // i
                c.EmitDelegate((bool notSlugNPC, GhostCreatureSedater self, int i) =>
                {
                    bool notMothPup = true;
                    if (self.room.abstractRoom.creatures[i].creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        notMothPup = false;
                    }
                    return notSlugNPC && notMothPup;
                });
            }
        }

        private static void SaveState_SessionEndedIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
                i => i.Match(OpCodes.Call)))
            {
                Plugin.Log("SaveState_SessionEndedIL MatchFind 1!");
                c.Emit(OpCodes.Ldarg_1); // game
                c.Emit(OpCodes.Ldloc_3); // k
                c.Emit(OpCodes.Ldloc_S, (byte)5); // l
                c.EmitDelegate((bool isSlugNPC, RainWorldGame game, int k, int l) =>
                {
                    bool isMothPup = false;
                    if (game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        isMothPup = true;
                    }
                    return isSlugNPC || isMothPup;
                });
            }
            while (c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Slugpup))))
            {
                Plugin.Log("SaveState_SessionEndedIL MatchFind 2!");
                c.Emit(OpCodes.Ldarg_1); // game
                c.Emit(OpCodes.Ldloc_3); // k
                c.Emit(OpCodes.Ldloc_S, (byte)5); // l
                c.EmitDelegate((SlugcatStats.Name slugpup, RainWorldGame game, int k, int l) =>
                {
                    SlugcatStats.Name result = slugpup;
                    if (game.world.GetAbstractRoom(game.Players[k].pos).creatures[l].creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        result = Plugin.Mothpup;
                    }
                    return result;
                });
            }
        }

        private static void World_SpawnPupNPCsIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
                i => i.Match(OpCodes.Call)))
            {
                Plugin.Log("World_SpawnPupNPCsIL MatchFind 1!");
                c.Emit(OpCodes.Ldloc_S, (byte)5); // abstractCreature
                c.EmitDelegate((bool isSlugNPC, AbstractCreature abstractCreature) =>
                {
                    bool isMothPup = false;
                    if (abstractCreature.creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        isMothPup = true;
                    }
                    return isSlugNPC || isMothPup;
                });
            }

            if (c.TryGotoNext(MoveType.After,
                i => i.MatchCallvirt<RainWorldGame>("GetNewID"),
                i => i.Match(OpCodes.Newobj),
                i => i.MatchStloc(13)))
            {
                Plugin.Log("World_SpawnPupNPCsIL MatchFind 2!");
                c.Emit(OpCodes.Ldarg_0); // this, world
                c.Emit(OpCodes.Ldloc_S, (byte)13); // abstractCreature
                c.EmitDelegate((World world, AbstractCreature abstractCreature) =>
                {
                    if (TheOutsider.PlayerNPCShouldBeMoth(world, abstractCreature.ID))
                    {
                        abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(TheOutsiderEnums.CreatureTemplateType.Mothpup), null, 
                            abstractCreature.pos, abstractCreature.ID);
                    }
                    return abstractCreature;
                });
                c.Emit(OpCodes.Stloc_S, (byte)13); // abstractCreature
            }
            /*
            while (c.TryGotoNext(MoveType.After,
                i => i.Match(OpCodes.Ldarg_0),
                i => i.MatchLdsfld<DLCSharedEnums.CreatureTemplateType>(nameof(DLCSharedEnums.CreatureTemplateType.SlugNPC))))
            {
                Plugin.Log("World_SpawnPupNPCsIL MatchFind 2!");
                c.Emit(OpCodes.Ldarg_0); // world
                c.EmitDelegate((CreatureTemplate.Type slugpup, World self) =>
                {
                    CreatureTemplate.Type result = slugpup;
                    if (TheOutsider.PlayerNPCShouldBeMoth(self))
                    {
                        result = TheOutsiderEnums.CreatureTemplateType.Mothpup;
                    }
                    return result;
                });
            }*/
        }
        #endregion

        private static void PlayerNPCState_ctor(On.MoreSlugcats.PlayerNPCState.orig_ctor orig, PlayerNPCState self, AbstractCreature abstractCreature, int playerNumber)
        {
            orig(self, abstractCreature, playerNumber);
            if (abstractCreature.creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
            {
                self.Glowing = true;
            }
        }
        private static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            if (//(creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.SlugNPC && TheOutsider.PlayerNPCShouldBeMoth(world, ID)) ||
                 creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
            {
                creatureTemplate = StaticWorld.GetCreatureTemplate(TheOutsiderEnums.CreatureTemplateType.Mothpup);
                self.creatureTemplate = creatureTemplate;
                self.personality = new AbstractCreature.Personality(ID);
                self.remainInDenCounter = -1;
                if (world == null)
                {
                    return;
                }
            }
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
        }
        private static void SlugcatStats_ctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
        {
            orig(self, slugcat, malnourished);

            if (slugcat == Plugin.Mothpup)
            {
                self.bodyWeightFac = 0.65f;
                self.generalVisibilityBonus = -0.2f;
                self.visualStealthInSneakMode = 0.6f;
                self.loudnessFac = 1f;
                self.lungsFac = 1f;
                self.throwingSkill = 0;
                self.poleClimbSpeedFac = 1.6f;
                self.corridorClimbSpeedFac = 1.15f;
                self.runspeedFac = 1.25f;
                if (malnourished)
                {
                    self.runspeedFac = 1f;
                    self.poleClimbSpeedFac = 0.9f;
                    self.corridorClimbSpeedFac = 0.9f;
                }
            }
        }
        private static IntVector2 SlugcatStats_SlugcatFoodMeter(On.SlugcatStats.orig_SlugcatFoodMeter orig, SlugcatStats.Name slugcat)
        {
            IntVector2 result = orig(slugcat);
            if (slugcat == Plugin.Mothpup)// || (player != null && TheOutsider.PlayerNPCShouldBeMoth(player)))
            {
                result = new IntVector2(4, 3);
            }
            return result;
        }
        private static bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i)
        {
            bool result = orig(i);
            if (i == Plugin.Mothpup)
            {
                result = true;
            }
            return result;
        }
        private static bool AImap_TileAccessibleToCreature(On.AImap.orig_TileAccessibleToCreature_IntVector2_CreatureTemplate orig, AImap self, IntVector2 pos, CreatureTemplate crit)
        {
            bool result = orig(self, pos, crit);
            if (crit.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
            {
                AItile aitile = self.getAItile(pos);
                if (Plugin.optionsMenuInstance.infiniteFlight.Value)
                {
                    result = result ||
                             StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.CicadaA).AccessibilityResistance(aitile.acc).Allowed;
                }
                else
                {
                    result = result ||
                             StaticWorld.GetCreatureTemplate(DLCSharedEnums.CreatureTemplateType.ScavengerElite).AccessibilityResistance(aitile.acc).Allowed;
                }
            }
            return result;
        }
    }
}
