using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System.Text.RegularExpressions;
using TheOutsider.CustomLore.CustomCreature;
using Custom = RWCustom.Custom;
using UnityEngine;
using System.Collections.Generic;
using HUD;

namespace TheOutsider.MothPup_Hooks
{
    public class OtherHooks
    {
        public static void Init()
        {
            IL.MoreSlugcats.PlayerNPCState.CycleTick += PlayerNPCState_CycleTickIL;
            IL.HUD.FoodMeter.TrySpawnPupBars += FoodMeter_TrySpawnPupBarsIL;

            On.SlugcatStats.ctor += SlugcatStats_ctor;
            On.SlugcatStats.HiddenOrUnplayableSlugcat += SlugcatStats_HiddenOrUnplayableSlugcat;
            On.SlugcatStats.SlugcatFoodMeter += SlugcatStats_SlugcatFoodMeter;

            On.AbstractCreature.ctor += AbstractCreature_ctor;
            On.AImap.TileAccessibleToCreature_IntVector2_CreatureTemplate += AImap_TileAccessibleToCreature;

            //IL.RegionState.AdaptRegionStateToWorld += IL_RegionState_AdaptRegionStateToWorld;
            //IL.SlugcatStats.NourishmentOfObjectEaten += IL_SlugcatStats_NourishmentOfObjectEaten;

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
                    Plugin.Log("PlayerNPCState_CycleTickIL 0 :" + result.ToString());
                    if (self.player.creatureTemplate.type == MothPupCritob.MothPup)
                    {
                        result = Plugin.MothPup;
                        Plugin.Log("PlayerNPCState_CycleTickIL 1 :" + result.ToString());
                    }
                    Plugin.Log("PlayerNPCState_CycleTickIL 2 :" + result.ToString());
                    return result;
                });
            }
        }

        private static void FoodMeter_TrySpawnPupBarsIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
                i => i.Match(OpCodes.Call)))
            {
                Plugin.Log("FoodMeter_TrySpawnPupBarsIL MatchFind!");
                c.Emit(OpCodes.Ldarg_0); // self
                c.Emit(OpCodes.Ldloc_1); // i
                c.EmitDelegate((bool isSlugNPC, FoodMeter self, int i) =>
                {
                    bool isMothPup = false;
                    if ((self.hud.owner as Player).abstractCreature.Room.creatures[i].creatureTemplate.type == MothPupCritob.MothPup)
                    {
                        isMothPup = true;
                    }
                    return isSlugNPC || isMothPup;
                });
            }
        }
        #endregion
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

            Plugin.Log("SlugcatStats_ctor :" + slugcat.ToString());
            if (slugcat == Plugin.MothPup)
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
        {/*/
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
            }*/

            IntVector2 result = orig(slugcat);
            Plugin.Log("SlugcatStats_SlugcatFoodMeter 0 :" + result.ToString());
            Plugin.Log("SlugcatStats_SlugcatFoodMeter slugcat :" + slugcat.ToString());
            if (slugcat == Plugin.MothPup)// || (player != null && PlayerEx.PlayerNPCShouldBeMoth(player)))
            {
                result = new IntVector2(4, 3);
                Plugin.Log("SlugcatStats_SlugcatFoodMeter 1 :" + result.ToString());
            }
            return result;
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
        private static bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i)
        {
            if (i == Plugin.MothPup)
            {
                return true;
            }
            return orig(i);
        }
        private static string PlayerNPCState_ToString(On.MoreSlugcats.PlayerNPCState.orig_ToString orig, PlayerNPCState self)
        {
            string text = orig(self);
            if (self.player.realizedCreature is Player pup && Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pup, out var player) && player.isMothNPC)
            {
                text += "Variant<cC>" + ((pup.slugcatStats.name != MoreSlugcatsEnums.SlugcatStatsName.Slugpup) ? pup.slugcatStats.name.value : "NULL") + "<cB>";
                text += "IsColorVariation<cC>" + (player.isColorVariation) + "<cB>";
            }
            return text;

        }
        /*
        private static void PlayerNPCState_LoadFromString(On.MoreSlugcats.PlayerNPCState.orig_LoadFromString orig, PlayerNPCState self, string[] s)
        {
            orig(self, s);
            if (self.player.realizedCreature != null && Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self.player.realizedCreature as Player, out var player) && player.isMothNPC)
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
                            break;
                    }
                }
            }
            self.unrecognizedSaveStrings.Remove("Variant");
            self.unrecognizedSaveStrings.Remove("PupsPlusStomach");
        }
        */
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
        /*
        private static void IL_Snail_Click(ILContext il)
        {
            ILCursor staggerCurs = new(il);

            staggerCurs.TryGotoNext(MoveType.After, x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Saint)), x => x.Match(OpCodes.Call));
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
