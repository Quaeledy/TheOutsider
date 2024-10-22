using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System.Text.RegularExpressions;
using TheOutsider.CustomLore.CustomCreature;
using Custom = RWCustom.Custom;

namespace TheOutsider.MothPup_Hooks
{
    public class OtherHooks
    {
        public static void Init()
        {
            //On.SlugcatStats.ctor += SlugcatStats_ctor;
            //On.SlugcatStats.SlugcatFoodMeter += SlugcatStats_SlugcatFoodMeter;

            On.AbstractCreature.ctor += AbstractCreature_ctor;
            On.AImap.TileAccessibleToCreature_IntVector2_CreatureTemplate += AImap_TileAccessibleToCreature;

            IL.RegionState.AdaptRegionStateToWorld += IL_RegionState_AdaptRegionStateToWorld;
            IL.MoreSlugcats.PlayerNPCState.CycleTick += IL_PlayerNPCState_CycleTick;
            //IL.SlugcatStats.NourishmentOfObjectEaten += IL_SlugcatStats_NourishmentOfObjectEaten;

            //On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;
            //On.MoreSlugcats.PlayerNPCState.ctor += ;
            //On.MoreSlugcats.PlayerNPCState.ToString += PlayerNPCState_ToString;
            //On.MoreSlugcats.PlayerNPCState.LoadFromString += PlayerNPCState_LoadFromString;
            //On.AbstractCreature.setCustomFlags += AbstractCreature_setCustomFlags;
            //On.AbstractCreature.Move += AbstractCreature_Move;
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
        */
        private static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
            if (self.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && PlayerEx.PlayerNPCShouldBeMoth(world))
            {
                self.creatureTemplate = StaticWorld.GetCreatureTemplate(MothPupCritob.MothPup);
            }
        }
        private static void SlugcatStats_ctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
        {
            orig(self, slugcat, malnourished);
            if (slugcat == Plugin.MothPup)
            {
                self.bodyWeightFac = 0.65f;
                self.generalVisibilityBonus = -0.2f;
                self.visualStealthInSneakMode = 0.6f;
                self.loudnessFac = 0.5f;
                self.lungsFac = 0.8f;
                self.throwingSkill = 0;
                self.poleClimbSpeedFac = 1.4f;
                self.corridorClimbSpeedFac = 1.35f;
                self.runspeedFac = 1.35f;
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
            Player player = null;
            if (Custom.rainWorld != null && Custom.rainWorld.processManager != null &&
                Custom.rainWorld.processManager.currentMainLoop != null && Custom.rainWorld.processManager.currentMainLoop is RainWorldGame)
            {
                var session = (Custom.rainWorld.processManager.currentMainLoop as RainWorldGame).session;
                for (int i = session.Players.Count - 1; i >= 0; i--)
                {
                    if (session.Players[i].realizedCreature == null)
                        continue;
                    player = session.Players[i].realizedCreature as Player;
                    if (player == null)
                        continue;
                }
            }

            if (slugcat == Plugin.MothPup || (player != null && PlayerEx.PlayerNPCShouldBeMoth(player)))
            {
                return new IntVector2(4, 3);
            }
            return orig(slugcat);
        }
        private static bool AImap_TileAccessibleToCreature(On.AImap.orig_TileAccessibleToCreature_IntVector2_CreatureTemplate orig, AImap self, IntVector2 pos, CreatureTemplate crit)
        {
            bool result = orig(self, pos, crit);
            if (crit.type == MothPupCritob.MothPup)
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
                             StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite).AccessibilityResistance(aitile.acc).Allowed;
                }
            }
            return result;
        }
        private static string PlayerNPCState_ToString(On.MoreSlugcats.PlayerNPCState.orig_ToString orig, PlayerNPCState self)
        {
            string text = orig(self);
            if (self.player.realizedCreature is Player pup && SlugNPCAIHooks.TryGetPupState(pup.playerState, out var pupNPCState))
            {
                text += "Variant<cC>" + ((pup.slugcatStats.name != MoreSlugcatsEnums.SlugcatStatsName.Slugpup) ? pup.slugcatStats.name.value : "NULL") + "<cB>";
                text += "PupsPlusStomach<cC>" + ((pupNPCState.PupsPlusStomachObject != null) ? pupNPCState.PupsPlusStomachObject.ToString() : "NULL") + "<cB>";
            }
            return text;

        }
        private static void PlayerNPCState_LoadFromString(On.MoreSlugcats.PlayerNPCState.orig_LoadFromString orig, PlayerNPCState self, string[] s)
        {
            orig(self, s);
            if (SlugNPCAIHooks.TryGetPupState(self, out var pupNPCState))
            {
                for (int i = 0; i < s.Length - 1; i++)
                {
                    string[] array = Regex.Split(s[i], "<cC>");
                    switch (array[0])
                    {
                        case "Variant":
                            pupNPCState.Variant = array[1] switch
                            {
                                "MothPup" => Plugin.MothPup,
                                _ => null
                            };
                            break;
                        case "PupsPlusStomach":
                            if (array[1] != "NULL")
                            {
                                if (array[1].Contains("<oA>"))
                                {
                                    pupNPCState.PupsPlusStomachObject = SaveState.AbstractPhysicalObjectFromString(self.player.Room.world, array[1]);
                                }
                                else if (array[1].Contains("<cA>"))
                                {
                                    pupNPCState.PupsPlusStomachObject = SaveState.AbstractCreatureFromString(self.player.Room.world, array[1], onlyInCurrentRegion: false);
                                }
                            }
                            break;/*
                        // BeastMasterPupExtras Compat
                        case "SlugcatCharacter":
                            if (BeastMasterPupExtras && !array[1].Equals("Slugpup") && self.player.TryGetPupAbstract(out var pupAbstract))
                            {
                                pupAbstract.regular = true;
                            }
                            break;
                            */
                    }
                }
            }
            self.unrecognizedSaveStrings.Remove("Variant");
            self.unrecognizedSaveStrings.Remove("PupsPlusStomach");
        }
        private static void AbstractCreature_setCustomFlags(On.AbstractCreature.orig_setCustomFlags orig, AbstractCreature self)
        {
            if (self.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            {
                if (SlugNPCAIHooks.pupAbstractCWT.TryGetValue(self, out var pupAbstract))
                {
                    if (self.spawnData == null || self.spawnData[0] != '{')
                    {
                        orig(self);
                        return;
                    }
                    string[] array = self.spawnData.Substring(1, self.spawnData.Length - 2).Split(',');
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i].Length > 0)
                        {
                            switch (array[i].Split(':')[0].ToString())
                            {
                                case "Moth":
                                    pupAbstract.moth = true;
                                    break;
                            }
                        }
                    }
                }
            }
            orig(self);
        }
        private static void AbstractCreature_Move(On.AbstractCreature.orig_Move orig, AbstractCreature self, WorldCoordinate newCoord)
        {
            if (self.creatureTemplate.type == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            {
                RainWorldGame game = self.world.game;
                Plugin.Log("newCoord.room: " + newCoord.room);
                AbstractRoom room = self.world.GetAbstractRoom(newCoord.room);
                //更换房间
                if (room != null && room.realizedRoom == null)
                {
                    room.RealizeRoom(game.world, game);
                    if (game.world.loadingRooms.Count > 0)
                    {
                        for (int n = 0; n < 1; n++)
                        {
                            for (int num = game.world.loadingRooms.Count - 1; num >= 0; num--)
                            {
                                if (game.world.loadingRooms[num].done)
                                {
                                    game.world.loadingRooms.RemoveAt(num);
                                }
                                else
                                {
                                    while (!game.world.loadingRooms[num].done)
                                    {
                                        game.world.loadingRooms[num].Update();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            orig(self, newCoord);
        }
        private static bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i)
        {
            if (i == Plugin.MothPup)
            {
                return true;
            }
            return orig(i);
        }
        private static void IL_PlayerNPCState_CycleTick(ILContext il)
        {
            ILCursor foodCurs = new(il);

            while (foodCurs.TryGotoNext(MoveType.After, x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Slugpup))))
            {
                /* WHILE TRYGOTO AFTER ldsfld class SlugcatStats/Name MoreSlugcats.MoreSlugcatsEnums/SlugcatStatsName::Slugpup
                 * 	IL_****: ldarg.0
	             *  IL_****: ldsfld class SlugcatStats/Name MoreSlugcats.MoreSlugcatsEnums/SlugcatStatsName::Slugpup
                 */
                foodCurs.Emit(OpCodes.Ldarg_0); // self
                foodCurs.EmitDelegate((SlugcatStats.Name slugpup, PlayerNPCState self) =>   // If pupNPCState.variant != null, return variant, else return slugpup
                {
                    if (SlugNPCAIHooks.TryGetPupState(self, out var pupNPCState))
                    {
                        SlugcatStats.Name variant = pupNPCState.Variant;
                        if (variant != null)
                        {
                            return variant;
                        }
                    }
                    return slugpup;
                });
            }
        }
        private static void IL_RegionState_AdaptRegionStateToWorld(ILContext il)
        {
            ILCursor stomachObjCurs = new(il);

            stomachObjCurs.GotoNext(x => x.MatchLdstr("Add pup to pendingFriendSpawns {0}"));
            stomachObjCurs.GotoNext(MoveType.Before, x => x.MatchLdarg(0));
            /* GOTO BEFORE IL_02ab
             * 	IL_02b0: ldarg.0
			 *  IL_02b1: ldfld class SaveState RegionState::saveState
			 *  IL_02b6: ldfld class [mscorlib]System.Collections.Generic.List`1<string> SaveState::pendingFriendCreatures
             */
            stomachObjCurs.Emit(OpCodes.Ldloc, 6); // abstractCreature
            stomachObjCurs.EmitDelegate((AbstractCreature abstractCreature) =>   // If abstractCreature is player and player playerState is PlayerNPCState, set PupsPlusStomachObject to objectInStomach
            {
                if (abstractCreature.realizedCreature is Player player && player.isNPC)
                {
                    if (SlugNPCAIHooks.TryGetPupState(player.playerState, out var pupNPCState))
                    {
                        if (player.objectInStomach != null)
                        {
                            pupNPCState.PupsPlusStomachObject = player.objectInStomach;
                        }
                        else pupNPCState.PupsPlusStomachObject = null;
                    }
                }
            });
        }
        /*
        private static void IL_Snail_Click(ILContext il)
        {
            ILCursor staggerCurs = new(il);

            staggerCurs.GotoNext(MoveType.After, x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Saint)), x => x.Match(OpCodes.Call));
            staggerCurs.Emit(OpCodes.Ldloc, 10); // item
            staggerCurs.EmitDelegate((PhysicalObject item) =>   // If item is Player and Player is Tundrapup, return true
            {
                if (item is Player player && player.isMothPup())
                {
                    return true;
                }
                return false;
            });
            staggerCurs.Emit(OpCodes.Or);
        }*/
    }
}
