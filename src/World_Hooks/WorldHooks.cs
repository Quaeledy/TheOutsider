using Mono.Cecil.Cil;
//using Debug = UnityEngine.Debug;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using System.Linq;
using TheOutsider.Player_Hooks;
using Watcher;

namespace TheOutsider.World_Hooks
{
    public class WorldHooks
    {
        public static void InitIL()
        {
            IL.World.SpawnGhost += World_SpawnGhostIL;
        }
        public static void Init()
        {
            On.World.CheckForRegionGhost += World_CheckForRegionGhost;
            //On.StaticWorld.InitStaticWorldRelationshipsWatcher += StaticWorld_InitStaticWorldRelationshipsWatcher;
            if (ModManager.ActiveMods.Any(mod => mod.id == "watcher"))//ModManager.Watcher
                On.Watcher.BigMothAI.IUseARelationshipTracker_UpdateDynamicRelationship += BigMothAI_IUseARelationshipTracker_UpdateDynamicRelationship;
        }

        private static void World_SpawnGhostIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                //使轮到非圣徒线的海岸线、沉没巨构回响不会直接返回
                for (int k = 0; k < 2; k++)
                {
                    if (c.TryGotoNext(MoveType.After,
                   (i) => i.MatchLdfld<StoryGameSession>(nameof(StoryGameSession.saveStateNumber)),
                   (i) => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Saint)),
                   (i) => i.Match(OpCodes.Call)))
                    {
                        Plugin.Log("World_SpawnGhostIL MatchFind 1!");
                        c.Emit(OpCodes.Ldarg_0);
                        c.EmitDelegate<Func<bool, World, bool>>((shouldReturn, self) =>
                        {
                            if (self.game.StoryCharacter != null && self.game.StoryCharacter == Plugin.SlugName)
                            {
                                shouldReturn = false;
                            }
                            return shouldReturn;
                        });
                    }
                }
                //使第一个雨循环不会生成回响
                if (c.TryGotoNext(MoveType.After,
                   (i) => i.MatchLdsfld<SlugcatStats.Name>(nameof(SlugcatStats.Name.Red)),
                   (i) => i.Match(OpCodes.Call),
                   (i) => i.Match(OpCodes.Call)))
                {
                    Plugin.Log("World_SpawnGhostIL MatchFind 2!");
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, World, bool>>((shouldSpawnGhost, self) =>
                    {
                        if (self.game.StoryCharacter != null &&
                            self.game.StoryCharacter == Plugin.SlugName &&
                            Custom.rainWorld.progression.currentSaveState.cycleNumber == 0)
                        {
                            shouldSpawnGhost = false;
                        }
                        return shouldSpawnGhost;
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static bool World_CheckForRegionGhost(On.World.orig_CheckForRegionGhost orig, SlugcatStats.Name slugcatIndex, string regionString)
        {
            bool result = orig(slugcatIndex, regionString);
            GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(regionString);
            if (ModManager.MSC &&
                (ghostID == MoreSlugcatsEnums.GhostID.SL || ghostID == MoreSlugcatsEnums.GhostID.MS) &&
                slugcatIndex == Plugin.SlugName)
            {
                result = true;
            }
            return result;
        }

        #region 生物关系
        public static CreatureTemplate.Relationship BigMothAI_IUseARelationshipTracker_UpdateDynamicRelationship(On.Watcher.BigMothAI.orig_IUseARelationshipTracker_UpdateDynamicRelationship orig, BigMothAI self, RelationshipTracker.DynamicRelationship dRelation)
        {
            CreatureTemplate.Relationship result = orig(self, dRelation);
            if (dRelation.trackerRep != null && dRelation.trackerRep.representedCreature != null && dRelation.trackerRep.representedCreature.realizedCreature != null &&
                dRelation.trackerRep.representedCreature.realizedCreature is Player player && PlayerHooks.PlayerData.TryGetValue(player, out var playerEX))
            {
                BigMothAI.BigMothTrackState bigMothTrackState = dRelation.state as BigMothAI.BigMothTrackState;
                CreatureTemplate.Relationship relationship = self.StaticRelationship(dRelation.trackerRep.representedCreature);
                if (relationship.type == CreatureTemplate.Relationship.Type.Afraid || relationship.type == CreatureTemplate.Relationship.Type.Uncomfortable)
                {
                    if (!dRelation.state.alive)
                    {
                        relationship.intensity = 0f;
                    }
                    else
                    {
                        float tempLike = self.creature.state.socialMemory.GetLike(dRelation.trackerRep.representedCreature.ID);// GetTempLike -> GetLike
                        if (!self.bug.Small)
                        {
                            if (tempLike > -0.15f)// 0.15f
                            {
                                relationship.type = CreatureTemplate.Relationship.Type.Ignores;
                            }/*
                            else if (tempLike < -0.9f)
                            {
                                relationship.type = CreatureTemplate.Relationship.Type.Eats;
                            }*/
                            else if (tempLike < -0.5f)// -0.15f
                            {
                                relationship.type = CreatureTemplate.Relationship.Type.Attacks;
                            }
                        }
                    }
                }
                else if (relationship.type == CreatureTemplate.Relationship.Type.Antagonizes)
                {// GetTempLike -> GetLike
                    if ((double)self.creature.state.socialMemory.GetLike(dRelation.trackerRep.representedCreature.ID) > -0.15f)// -0.2f
                    {
                        relationship.type = CreatureTemplate.Relationship.Type.Ignores;
                    }
                    else if ((double)self.creature.state.socialMemory.GetLike(dRelation.trackerRep.representedCreature.ID) < -0.65f)// -0.2f
                    {
                        relationship.type = CreatureTemplate.Relationship.Type.Afraid;
                    }
                    else if (self.bug.room.abstractRoom.AttractionForCreature(self.bug.Template.type) == AbstractRoom.CreatureRoomAttraction.Forbidden)
                    {
                        return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
                    }
                }
                else if (relationship.type == CreatureTemplate.Relationship.Type.Uncomfortable)
                {
                    if (bigMothTrackState.vultureMasked && self.creature.state.socialMemory.GetLike(dRelation.trackerRep.representedCreature.ID) < -0.15f)
                    {
                        relationship.type = CreatureTemplate.Relationship.Type.Eats;
                    }
                    else if (bigMothTrackState.thrownTimer > 0)
                    {
                        relationship.type = CreatureTemplate.Relationship.Type.Ignores;
                        relationship.intensity = 0f;
                    }
                }
            }
            return result;
        }
        #endregion
    }
}
