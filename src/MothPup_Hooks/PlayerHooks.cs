using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using UnityEngine;
using Custom = RWCustom.Custom;
using Random = UnityEngine.Random;

namespace TheOutsider.MothPup_Hooks
{
    public class PlayerHooks
    {
        public static void Init()
        {
            IL.Player.ThrowObject += Player_ThrowObjectIL;

            On.Player.AllowGrabbingBatflys += Player_AllowGrabbingBatflys;
            On.Player.CanEatMeat += Player_CanEatMeat;
            On.Player.ThrownSpear += Player_ThrownSpear;

            //IL.Player.ObjectEaten += IL_Player_ObjectEaten;
            //IL.Player.ctor += IL_Player_ctor;
            //IL.Player.Update += IL_Player_Update;
            //IL.Player.SwallowObject += IL_Player_SwallowObject;
            //IL.Player.GrabUpdate += IL_Player_GrabUpdate;
            //IL.Player.Jump += IL_Player_Jump;
            //IL.Player.UpdateAnimation += IL_Player_UpdateAnimation;
            //IL.Player.LungUpdate += IL_Player_LungUpdate;
            //IL.Player.StomachGlowLightColor += IL_Player_StomachGlowLightColor;
            //IL.Player.FoodInRoom_Room_bool += IL_Player_FoodInRoom;
            //IL.Player.SetMalnourished += IL_Player_SetMalnourished;
            //IL.Player.NPCStats.ctor += IL_NPCStats_ctor;

            //On.Player.ctor += Player_ctor;
            //On.Player.UpdateMSC += Player_UpdateMSC;
            //On.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint;
            //On.Player.SlugSlamConditions += Player_SlugSlamConditions;
            //On.Player.GrabUpdate += Player_GrabUpdate;
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
        private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            Player pupGrabbed = null;
            int grabbedIndex = -1;
            foreach (var grasped in self.grasps)
            {
                if (grasped?.grabbed is Player pup && pup.isNPC)
                {
                    pupGrabbed = pup;
                    break;
                }
            }
            if (pupGrabbed != null)
            {
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed != null && grasped.grabbed != pupGrabbed)
                    {
                        grabbedIndex = grasped.graspUsed;
                        break;
                    }
                }
                if (self.input[0].pckp && self.input[0].y == 1)
                {
                    if (pupGrabbed.grasps[0] != null && SlugpupSafari)
                    {
                        orig(self, eu);
                        return;
                    }
                    if (self.slugOnBack != null) self.slugOnBack.increment = false;
                    if (self.spearOnBack != null) self.spearOnBack.increment = false;

                    if (grabbedIndex > -1 && self.grasps[grabbedIndex].grabbed != null)
                    {
                        pupGrabbed.PupSwallowObject(grabbedIndex);
                    }
                    else if (pupGrabbed.objectInStomach != null || pupGrabbed.isRotundpup() && slugpupRemix.ManualItemGen.Value)
                    {
                        pupGrabbed.PupRegurgitate();
                    }
                    else
                    {
                        orig(self, eu);
                    }
                }
                else
                {
                    if (pupGrabbed.swallowAndRegurgitateCounter > 0)
                    {
                        pupGrabbed.swallowAndRegurgitateCounter--;
                    }
                    orig(self, eu);
                }
            }
            else
            {
                orig(self, eu);
            }
        }
        private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);
            self.VariantMechanicsMothPup();
            if (slugpupRemix.RotundBackExaustion.Value) self.VariantMechanicsRotundpup();
        }
        private static void Player_ClassMechanicsSaint(On.Player.orig_ClassMechanicsSaint orig, Player self)
        {
            orig(self);
        }
        private static bool Player_SlugSlamConditions(On.Player.orig_SlugSlamConditions orig, Player self, PhysicalObject otherObject)
        {
            if (self.isNPC && otherObject is Player)
            {
                return false;
            }
            return orig(self, otherObject);
        }*/
        private static void Player_ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
        {
            orig(self, spear);
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
            {
                spear.spearDamageBonus = 0.1f + 0.1f * Mathf.Pow(Random.value, 6f);
            }
        }
        private static bool Player_CanEatMeat(On.Player.orig_CanEatMeat orig, Player self, Creature crit)
        {
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
            {
                return false;
            }
            return orig(self, crit);
        }
        private static bool Player_AllowGrabbingBatflys(On.Player.orig_AllowGrabbingBatflys orig, Player self)
        {
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
            {
                return false;
            }
            return orig(self);
        }
        private static void Player_ThrowObjectIL(ILContext il)
        {
            ILCursor c = new(il);

            if (true)
            {
                while (c.TryGotoNext(MoveType.After,
                    i => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Saint)),
                    i => i.Match(OpCodes.Call)))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool, Player, bool>>((isSaint, self) =>
                    {
                        bool isMothPup = false;
                        if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) &&
                            player.isMothNPC && self.isSlugpup)
                            isMothPup = true;
                        return isSaint || isMothPup;
                    });
                }
            }
        }
        #region 未使用
        private static void IL_Player_ObjectEatenIL(ILContext il)
        {
            ILCursor nameCurs = new(il);

            while (nameCurs.TryGotoNext(MoveType.After, x => x.MatchLdfld<Player>(nameof(Player.SlugCatClass))))
            {
                /* WHILE TRYGOTO AFTER ldfld class SlugcatStats/Name Player::SlugCatClass
                 * 	IL_****: ldarg.0
                 *  IL_****: ldfld class SlugcatStats/Name Player::SlugCatClass
                 */
                // ldfld class SlugcatStats/Name Player::SlugCatClass => SlugCatClass
                nameCurs.Emit(OpCodes.Ldarg_0); // self
                nameCurs.EmitDelegate((SlugcatStats.Name SlugCatClass, Player self) =>   // If self.isSlugpup, return slugcatStats.name, else return SlugCatClass
                {
                    if (self.isSlugpup)
                    {
                        return self.slugcatStats.name;
                    }
                    return SlugCatClass;
                });
            }
        }
        private static void IL_Player_FoodInRoom(ILContext il)
        {
            ILCursor nameCurs = new(il);

            while (nameCurs.TryGotoNext(MoveType.After, x => x.MatchLdfld<Player>(nameof(Player.SlugCatClass))))
            {
                /* WHILE TRYGOTO AFTER ldfld class SlugcatStats/Name Player::SlugCatClass
                 * 	IL_****: ldarg.0
                 *  IL_****: ldfld class SlugcatStats/Name Player::SlugCatClass
                 */
                // ldfld class SlugcatStats/Name Player::SlugCatClass => SlugCatClass
                nameCurs.Emit(OpCodes.Ldarg_0); // self
                nameCurs.EmitDelegate((SlugcatStats.Name SlugCatClass, Player self) =>   // If self.isSlugpup, return slugcatStats.name, else return SlugCatClass
                {
                    if (self.isSlugpup)
                    {
                        return self.slugcatStats.name;
                    }
                    return SlugCatClass;
                });
            }
        }
        private static void IL_NPCStats_ctor(ILContext il)
        {
            ILCursor statsCurs = new(il);

            statsCurs.GotoNext(x => x.MatchStloc(0));
            statsCurs.GotoNext(MoveType.After, x => x.MatchStloc(0));
            /* GOTO AFTER IL_0036
             * 	IL_002c: isinst MoreSlugcats.PlayerNPCState
	         *  IL_0031: ldfld bool MoreSlugcats.PlayerNPCState::Malnourished
	         *  IL_0036: stloc.0
             */
            statsCurs.Emit(OpCodes.Ldarg_1); // player
            statsCurs.EmitDelegate((Player player) =>
            {
                if (SlugNPCAIHooks.TryGetPupState(player.playerState, out var pupNPCState))
                {
                    if (player.isSlugpup && player.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                    {
                        pupNPCState.Variant ??= player.SlugCatClass;
                    }
                }
            });

            statsCurs.GotoNext(MoveType.After, x => x.MatchStfld<Player.NPCStats>(nameof(Player.NPCStats.EyeColor)));
            /* GOTO AFTER IL_01d5
             * 	IL_01cf: sub
	         *  IL_01d0: call float32 [UnityEngine.CoreModule]UnityEngine.Mathf::Pow(float32, float32)
	         *  IL_01d5: stfld float32 Player/NPCStats::EyeColor
             */
            statsCurs.Emit(OpCodes.Ldarg_0); // self
            statsCurs.Emit(OpCodes.Ldarg_1); // player
            statsCurs.EmitDelegate((Player.NPCStats self, Player player) =>
            {

                if (SlugNPCAIHooks.TryGetPupState(player.playerState, out var pupNPCState))
                {
                    if (player.abstractCreature.superSizeMe) player.playerState.forceFullGrown = true;

                    Random.State state = Random.state;
                    Random.InitState(player.abstractCreature.ID.RandomSeed);
                    if (pupNPCState.Variant != null)
                    {
                        AbstractCreature.Personality personality = player.abstractCreature.personality;
                        if (pupNPCState.Variant == Plugin.MothPup)
                        {
                            self.Bal = Mathf.Lerp(Random.Range(0f, 0.2f), 1f, self.Bal);
                            self.Met = Mathf.Lerp(Random.Range(0.1f, 0.3f), 1f, self.Met);

                            // Higher Energy
                            //      increased by higher metabolism, and lower size
                            //      decreased by lower balance
                            personality.energy = Mathf.Clamp01(Mathf.Pow(personality.energy + Random.Range(0f, 0.25f), 0.5f + 0.1f * (1f - self.Met) + 0.15f * self.Size + 0.1f * (1f - self.Bal)));

                            // Base Personality Calculations
                            personality.nervous = Mathf.Lerp(Random.value, Mathf.Lerp(personality.energy, 1f - personality.bravery, 0.5f), Mathf.Pow(Random.value, 0.25f));
                            personality.aggression = Mathf.Lerp(Random.value, (personality.energy + personality.bravery) / 2f * (1f - personality.sympathy), Mathf.Pow(Random.value, 0.25f));
                            personality.dominance = Mathf.Lerp(Random.value, (personality.energy + personality.bravery + personality.aggression) / 3f, Mathf.Pow(Random.value, 0.25f));
                            personality.nervous = Custom.PushFromHalf(personality.nervous, 2.5f);
                            personality.aggression = Custom.PushFromHalf(personality.aggression, 2.5f);
                        }
                        player.abstractCreature.personality = personality;
                    }
                    Random.state = state;
                }
            });

            statsCurs.GotoNext(MoveType.After, x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Slugpup)));
            /* GOTO AFTER IL_0246
             * 	IL_0246: ldsfld class SlugcatStats/Name MoreSlugcats.MoreSlugcatsEnums/SlugcatStatsName::Slugpup
	         *  IL_024b: ldloc.0
	         *  IL_024c: newobj instance void SlugcatStats::.ctor(class SlugcatStats/Name, bool)
             */
            // IL_0246 => slugpup
            statsCurs.Emit(OpCodes.Ldarg_1); // player
            statsCurs.EmitDelegate((SlugcatStats.Name slugpup, Player player) =>
            {
                if (SlugNPCAIHooks.TryGetPupState(player.playerState, out var pupNPCState))
                {
                    if (pupNPCState.Variant != null)
                    {
                        return pupNPCState.Variant;
                    }
                }
                return slugpup;
            });
        }
        private static void IL_Player_Jump(ILContext il)
        {
            ILCursor aquaCurs = new(il);

            aquaCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isSlugpup"));
            /* GOTO AFTER IL_01ad
             * 	IL_01ac: ldarg.0
	         *  IL_01ad: call instance bool Player::get_isSlugpup()
	         *  IL_01b2: brfalse.s IL_01c0
             */
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((Player self) =>   // If self is aquaticpup, return false
            {
                return !Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC;
            });
            aquaCurs.Emit(OpCodes.And);

            aquaCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(2.25f));
            /* GOTO AFTER IL_02a0
             * 	IL_029e: br.s IL_02a5
		     *  IL_02a0: ldc.r4 2.25
		     *  IL_02a5: add
             */
            // ldc.r4 2.25 => f
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((float f, Player self) =>   // If self is aquaticpup, return 3.25f, else return f
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return 3.25f;
                }
                return f;
            });

            aquaCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(1f));
            /* GOTO AFTER IL_02ca
             * 	IL_02c8: br.s IL_02cf
		     *  IL_02ca: ldc.r4 1
		     *  IL_02cf: add
             */
            // ldc.r4 1 => f
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((float f, Player self) =>   // If self is aquaticpup, return 1.5f, else return f
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return 1.5f;
                }
                return f;
            });

            aquaCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isSlugpup"));
            /* GOTO AFTER IL_048e
             *	IL_048d: ldarg.0
	         *  IL_048e: call instance bool Player::get_isSlugpup()
	         *  IL_0493: brfalse.s IL_050b
             */
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((Player self) =>   // If self is aquaticpup, return false
            {
                return !Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC;
            });
            aquaCurs.Emit(OpCodes.And);

            aquaCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(0.65f));
            /* GOTO AFTER IL_06fd
             *	IL_06f6: ldc.r4 1
	         *  IL_06fb: br.s IL_0702
	         *  IL_06fd: ldc.r4 0.65
             */
            // ldc.r4 0.65 => f
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((float f, Player self) =>   // If self is aquaticpup, return 0.8f, else return f
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return 0.8f;
                }
                return f;
            });

            aquaCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(0.65f));
            /* GOTO AFTER IL_069f
	         *  IL_0698: ldc.r4 1
	         *  IL_069d: br.s IL_06a4
	         *  IL_069f: ldc.r4 0.65
             */
            // ldc.r4 0.65 => f
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((float f, Player self) =>   // If self is aquaticpup, return 0.8f, else return f
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return 0.8f;
                }
                return f;
            });

            aquaCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(1f));
            /* GOTO AFTER IL_0728
	         *	IL_0726: brtrue.s IL_072f
	         *  IL_0728: ldc.r4 1
	         *  IL_072d: br.s IL_0734
             */
            // ldc.r4 1 => f
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((float f, Player self) =>   // If self is aquaticpup, return 1.2f, else return f
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return 1.2f;
                }
                return f;
            });

            aquaCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(1f));
            /* GOTO AFTER IL_0752
	         *	IL_0750: brtrue.s IL_0759
	         *  IL_0752: ldc.r4 1
	         *  IL_0757: br.s IL_075e
             */
            // ldc.r4 1 => f
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((float f, Player self) =>   // If self is aquaticpup, return 1.2f, else return f
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return 1.2f;
                }
                return f;
            });

            aquaCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(6f));
            /* GOTO AFTER IL_0816
	         *	IL_0814: brfalse.s IL_081d
	         *  IL_0816: ldc.r4 6
	         *  IL_081b: stloc.s 4
             */
            // ldc.r4 6 => f
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((float f, Player self) =>   // If self is aquaticpup, return 9f, else return f
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return 9f;
                }
                return f;
            });

            aquaCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(6f));
            /* GOTO AFTER IL_0bf5
	         *	IL_0bf3: brfalse.s IL_0bfc
	         *  IL_0bf5: ldc.r4 6
	         *  IL_0bfa: stloc.s 9
             */
            // ldc.r4 6 => f
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((float f, Player self) =>   // If self is aquaticpup, return 9f, else return f
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return 9f;
                }
                return f;
            });

            aquaCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isSlugpup"));
            /* GOTO AFTER IL_0f2f
             *	IL_0f2e: ldarg.0
	         *  IL_0f2f: call instance bool Player::get_isSlugpup()
	         *  IL_0f34: brtrue.s IL_0f39
             */
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((Player self) =>   // If self is aquaticpup, return false
            {
                return !Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC;
            });
            aquaCurs.Emit(OpCodes.And);

            aquaCurs.GotoNext(x => x.MatchCall<Player>("get_isSlugpup"));
            aquaCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(3f));
            /* GOTO AFTER IL_140e
	         *	IL_140d: ldarg.0
	         *  IL_140e: ldc.r4 3
	         *  IL_1413: stfld float32 Player::jumpBoost
             */
            // ldc.r4 3 => f
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((float f, Player self) =>   // If self is aquaticpup, return 6.5f, else return f
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return 6.5f;
                }
                return f;
            });

            aquaCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isSlugpup"));
            /* GOTO AFTER IL_14df
             *	IL_14de: ldarg.0
	         *  IL_14df: call instance bool Player::get_isSlugpup()
	         *  IL_14e4: brtrue.s IL_14e9
             */
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((Player self) =>   // If self is aquaticpup, return false
            {
                return !Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC;
            });
            aquaCurs.Emit(OpCodes.And);

            aquaCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(5.5f));
            /* GOTO AFTER IL_15bf
	         *	IL_15bd: brfalse.s IL_15c6
	         *  IL_15bf: ldc.r4 5.5
	         *  IL_15c4: stloc.s 13
             */
            // ldc.r4 5.5 => f
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((float f, Player self) =>   // If self is aquaticpup, return 7.5f, else return f
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return 7.5f;
                }
                return f;
            });

        }
        private static void IL_Player_UpdateAnimation(ILContext il)
        {
            ILCursor swimCurs = new(il);

            swimCurs.GotoNext(x => x.MatchLdsfld<Player.AnimationIndex>(nameof(Player.AnimationIndex.DeepSwim)));
            swimCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isRivulet"));
            /* GOTO AFTER IL_24f4
             * 	IL_24f3: ldarg.0
	         *  IL_24f4: call instance bool Player::get_isRivulet()
	         *  IL_24f9: brfalse.s IL_2503
             */
            swimCurs.Emit(OpCodes.Ldarg_0); // self
            swimCurs.EmitDelegate((Player self) =>   // If self is aquaticpup or holding aquaticpup, return true
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return true;
                }
                return false;
            });
            swimCurs.Emit(OpCodes.Or);

            swimCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isRivulet"));
            /* GOTO AFTER IL_258e
             * 	IL_258d: ldarg.0
	         *  IL_258e: call instance bool Player::get_isRivulet()
	         *  IL_2593: brfalse.s IL_25e8
             */
            swimCurs.Emit(OpCodes.Ldarg_0); // self
            swimCurs.EmitDelegate((Player self) =>   // If self is aquaticpup, return true
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return true;
                }
                return false;
            });
            swimCurs.Emit(OpCodes.Or);

            ILLabel burstLabel = il.DefineLabel();
            swimCurs.GotoNext(x => x.Match(OpCodes.Br));
            swimCurs.GotoNext(MoveType.After, x => x.MatchBr(out burstLabel), x => x.MatchLdarg(0)); // Get out IL_2683 as burstLabel
            /* GOTO AFTER IL_263c
             * 	IL_2635: stfld valuetype [UnityEngine.CoreModule]UnityEngine.Vector2 BodyChunk::vel
	         *  IL_263a: br.s IL_2683
	         *  IL_263c: ldarg.0
             */
            // ldarg.0 => self
            swimCurs.Emit(OpCodes.Ldloc, 10); // vector
            swimCurs.Emit(OpCodes.Ldloc, 11); // num3
            swimCurs.EmitDelegate((Player self, Vector2 vector, float num3) =>   // If self is not Rivulet and holding aquaticpup, add burst velocity and branch to burstLabel
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (!self.isRivulet && pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC))
                {
                    self.bodyChunks[0].vel += vector * ((vector.y > 0.5f) ? 300f : 50f);
                    self.airInLungs -= 0.08f * num3;
                    return true;
                }
                return false;
            });
            swimCurs.Emit(OpCodes.Brtrue_S, burstLabel);
            swimCurs.Emit(OpCodes.Ldarg_0);

            swimCurs.GotoNext(MoveType.After, x => x.MatchStfld<Player>(nameof(Player.waterJumpDelay)));
            /* GOTO AFTER IL_26d4
             * 	IL_26d0: br.s IL_26d4
	         *  IL_26d2: ldc.i4.s 10
	         *  IL_26d4: stfld int32 Player::waterJumpDelay
             */
            swimCurs.Emit(OpCodes.Ldarg_0); // self
            swimCurs.EmitDelegate((Player self) =>
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    self.waterJumpDelay = 12;
                }
            });

            swimCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isRivulet"));
            /* GOTO AFTER IL_294b
	         *  IL_294a: ldarg.0
	         *  IL_294b: call instance bool Player::get_isRivulet()
	         *  IL_2950: brfalse.s IL_296d
             */
            swimCurs.Emit(OpCodes.Ldarg_0); // self
            swimCurs.EmitDelegate((Player self) =>   // If self is aquaticpup or holding aquaticpup, return true
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return true;
                }
                return false;
            });
            swimCurs.Emit(OpCodes.Or);

            swimCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isRivulet"));
            /* GOTO AFTER IL_2d46
	         *  IL_2d45: ldarg.0
	         *  IL_2d46: call instance bool Player::get_isRivulet()
	         *  IL_2d4b: brfalse.s IL_2d63
             */
            swimCurs.Emit(OpCodes.Ldarg_0); // self
            swimCurs.EmitDelegate((Player self) =>   // If self is aquaticpup or holding aquaticpup, return true
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return true;
                }
                return false;
            });
            swimCurs.Emit(OpCodes.Or);

            swimCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isRivulet"));
            /* GOTO AFTER IL_2e27
	         *  IL_2e26: ldarg.0
	         *  IL_2e27: call instance bool Player::get_isRivulet()
	         *  IL_2e2c: brfalse.s IL_2e44
             */
            swimCurs.Emit(OpCodes.Ldarg_0); // self
            swimCurs.EmitDelegate((Player self) =>   // If self is aquaticpup or holding aquaticpup, return true
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return true;
                }
                return false;
            });
            swimCurs.Emit(OpCodes.Or);

            swimCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isRivulet"));
            /* GOTO AFTER IL_2e27
	         *  IL_3007: ldarg.0
	         *  IL_3008: call instance bool Player::get_isRivulet()
	         *  IL_300d: brtrue.s IL_3016
             */
            swimCurs.Emit(OpCodes.Ldarg_0); // self
            swimCurs.EmitDelegate((Player self) =>   // If self is aquaticpup or holding aquaticpup, return true
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return true;
                }
                return false;
            });
            swimCurs.Emit(OpCodes.Or);

            swimCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isRivulet"));
            /* GOTO AFTER IL_303d
	         *  IL_303c: ldarg.0
	         *  IL_303d: call instance bool Player::get_isRivulet()
	         *  IL_3042: brtrue.s IL_304b
             */
            swimCurs.Emit(OpCodes.Ldarg_0); // self
            swimCurs.EmitDelegate((Player self) =>   // If self is aquaticpup or holding aquaticpup, return true
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return true;
                }
                return false;
            });
            swimCurs.Emit(OpCodes.Or);

            swimCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(18f));
            /* GOTO AFTER IL_3134
             * 	IL_3132: br.s IL_3139
	         *  IL_3134: ldc.r4 18
	         *  IL_3139: add
             */
            //ldc.r4 => f
            swimCurs.Emit(OpCodes.Ldarg_0);
            swimCurs.EmitDelegate((float f, Player self) =>
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return 16f;
                }
                return f;
            });

            swimCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(18f));
            /* GOTO AFTER IL_315e
	         *  IL_315c: br.s IL_3163
	         *  IL_315e: ldc.r4 18
	         *  IL_3163: add
             */
            //ldc.r4 => f
            swimCurs.Emit(OpCodes.Ldarg_0);
            swimCurs.EmitDelegate((float f, Player self) =>
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return 16f;
                }
                return f;
            });

            swimCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isRivulet"));
            /* GOTO AFTER IL_303d
	         *  IL_316a: ldarg.0
	         *  IL_316b: call instance bool Player::get_isRivulet()
	         *  IL_3170: brtrue.s IL_3179
             */
            swimCurs.Emit(OpCodes.Ldarg_0); // self
            swimCurs.EmitDelegate((Player self) =>   // If self is aquaticpup or holding aquaticpup, return true
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return true;
                }
                return false;
            });
            swimCurs.Emit(OpCodes.Or);

            swimCurs.GotoNext(MoveType.After, x => x.MatchLdcI4(6));
            /* GOTO AFTER IL_3213
	         *  IL_3211: br.s IL_3214
	         *  IL_3213: ldc.i4.6
	         *  IL_3214: stfld int32 Player::waterJumpDelay
             */
            //ldc.i4 => i
            swimCurs.Emit(OpCodes.Ldarg_0);
            swimCurs.EmitDelegate((int i, Player self) =>
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return 9;
                }
                return i;
            });

            swimCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isRivulet"));
            /* GOTO AFTER IL_3226
	         *  IL_3225: ldarg.0
	         *  IL_3226: call instance bool Player::get_isRivulet()
	         *  IL_322b: brtrue.s IL_3235
             */
            swimCurs.Emit(OpCodes.Ldarg_0); // self
            swimCurs.EmitDelegate((Player self) =>   // If self is aquaticpup or holding aquaticpup, return true
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return true;
                }
                return false;
            });
            swimCurs.Emit(OpCodes.Or);

            swimCurs.GotoNext(MoveType.After, x => x.MatchLdcR4(12f));
            /* GOTO AFTER IL_315e
	         *  IL_3255: br.s IL_325c
	         *  IL_3257: ldc.r4 12
	         *  IL_325c: newobj instance void [UnityEngine.CoreModule]UnityEngine.Vector2::.ctor(float32, float32)
             */
            //ldc.r4 => f
            swimCurs.Emit(OpCodes.Ldarg_0);
            swimCurs.EmitDelegate((float f, Player self) =>
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && (pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC)))
                {
                    return 10f;
                }
                return f;
            });
        }
        private static void IL_Player_LungUpdate(ILContext il)
        {
            ILCursor aquaCurs = new(il);

            aquaCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>("get_isRivulet"));
            /* GOTO AFTER IL_0465
	         *  IL_0464: ldarg.0
	         *  IL_0465: call instance bool Player::get_isRivulet()
	         *  IL_046a: brfalse.s IL_046e
             */
            aquaCurs.Emit(OpCodes.Ldarg_0); // self
            aquaCurs.EmitDelegate((Player self) =>   // If self is aquaticpup or holding aquaticpup, return true
            {
                Player pupGrabbed = null;
                foreach (var grasped in self.grasps)
                {
                    if (grasped?.grabbed is Player pup && pup.isNPC)
                    {
                        pupGrabbed = pup;
                        break;
                    }
                }
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC || !self.isRivulet && pupGrabbed != null && (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(pupGrabbed, out var mothPup) && mothPup.isMothNPC))
                {
                    return true;
                }
                return false;
            });
            aquaCurs.Emit(OpCodes.Or);
        }
        private static void IL_Player_ctor(ILContext il)
        {
            ILCursor pupCurs = new(il);

            pupCurs.GotoNext(MoveType.After, x => x.MatchCall<Player>(nameof(Player.SetMalnourished)));
            /* GOTO AFTER IL_0a7f
	         *  IL_0a7c: br.s IL_0a7f
	         *  IL_0a7e: ldc.i4.1
	         *  IL_0a7f: call instance void Player::SetMalnourished(bool)
             */
            pupCurs.Emit(OpCodes.Ldarg_0);
            pupCurs.Emit(OpCodes.Ldarg_1);
            pupCurs.EmitDelegate((Player self, AbstractCreature abstractCreature) =>   // If game session is StoryGameSession and PupsPlusStomachObject is not null, set objectInStomach to PupsPlusStomachObject
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC &&
                    SlugNPCAIHooks.pupStateCWT.TryGetValue(self.State as PlayerNPCState, out var pupNPCState))
                {
                    if (abstractCreature.Room?.world?.game?.session != null && abstractCreature.Room.world.game.session is StoryGameSession)
                    {
                        if (pupNPCState?.PupsPlusStomachObject != null)
                        {
                            self.objectInStomach = pupNPCState.PupsPlusStomachObject;
                            self.objectInStomach.pos = abstractCreature.pos;
                        }
                        else
                        {
                            self.objectInStomach = null;
                        }
                    }
                }
            });
        }
        private static void IL_Player_Update(ILContext il)
        {
            ILCursor exhaustCurs = new(il);

            exhaustCurs.GotoNext(MoveType.After, x => x.MatchLdfld<Player>(nameof(Player.gourmandExhausted)));
            /* GOTO AFTER ldfld bool Player::gourmandExhausted
             * 	ldarg.0
	         *  ldfld bool Player::gourmandExhausted
	         *  brtrue.s IL_0c80
             */
            exhaustCurs.Emit(OpCodes.Ldarg_0);
            exhaustCurs.EmitDelegate((Player self) =>   // If pup is on back and pup is Rotundpup, return true
            {
                if (SlugNPCAIHooks.TryGetParentVariables(self, out var parentVariables))
                {
                    return parentVariables.rotundPupExhaustion;
                }
                return false;
            });
            exhaustCurs.Emit(OpCodes.Or);

            exhaustCurs.GotoNext(MoveType.After, x => x.MatchLdfld<Player>(nameof(Player.gourmandExhausted)));
            /* GOTO AFTER ldfld bool Player::gourmandExhausted
             * 	ldarg.0
	         *  ldfld bool Player::gourmandExhausted
	         *  brtrue.s IL_0d4e
             */
            exhaustCurs.Emit(OpCodes.Ldarg_0);
            exhaustCurs.EmitDelegate((Player self) =>   // If pup is on back and pup is Rotundpup, return true
            {
                if (SlugNPCAIHooks.TryGetParentVariables(self, out var parentVariables))
                {
                    return parentVariables.rotundPupExhaustion;
                }
                return false;
            });
            exhaustCurs.Emit(OpCodes.Or);
        }
        private static void IL_Player_SwallowObject(ILContext il)
        {
            ILCursor parentCurs = new(il);
            ILCursor graspedCurs = new(il);

            parentCurs.GotoNext(MoveType.After, x => x.MatchStloc(0));
            /* GOTO AFTER IL_0021
	            IL_0016: ldelem.ref
	            IL_0017: ldfld class PhysicalObject Creature/Grasp::grabbed
	            IL_001c: ldfld class AbstractPhysicalObject PhysicalObject::abstractPhysicalObject
	            IL_0021: stloc.0
            */
            ILLabel graspedLabel = parentCurs.MarkLabel();

            parentCurs.Emit(OpCodes.Ldloc_0); // abstractPhysicalObject
            parentCurs.Emit(OpCodes.Ldarg_0); // self
            parentCurs.Emit(OpCodes.Ldarg_1); // grasp
            parentCurs.EmitDelegate((AbstractPhysicalObject abstractPhysicalObject, Player self, int grasp) =>  // If grabbed by a player, set abstractPhysicalObject to object in the player's grasps[grasp]
            {
                if (self.isNPC && self.grabbedBy.Count > 0 && self.grabbedBy[0].grabber is Player parent)
                {
                    return parent.grasps[grasp].grabbed.abstractPhysicalObject;
                }
                return abstractPhysicalObject;
            });
            parentCurs.Emit(OpCodes.Stloc_0);

            // graspedCurs is at IL_0000
            graspedCurs.Emit(OpCodes.Ldarg_0); // self
            graspedCurs.EmitDelegate((Player self) =>   // If grabbed by a player, branch to graspedLabel
            {
                if (self.isNPC && self.grabbedBy.Count > 0 && self.grabbedBy[0].grabber is Player parent)
                {
                    return true;
                }
                return false;
            });
            graspedCurs.Emit(OpCodes.Brtrue_S, graspedLabel);

            graspedCurs.GotoNext(MoveType.After, x => x.MatchCallvirt<Creature>(nameof(Creature.ReleaseGrasp)));
            /* GOTO AFTER IL_007d
                IL_007b: ldarg.0
                IL_007c: ldarg.1
                IL_007d: callvirt instance void Creature::ReleaseGrasp(int32)
            */
            ILLabel releaseLabel = graspedCurs.MarkLabel();

            parentCurs.GotoNext(MoveType.After, x => x.MatchCallvirt<StoryGameSession>(nameof(StoryGameSession.RemovePersistentTracker)), x => x.MatchLdarg(0));
            /* GOTO AFTER IL_007b
	            IL_0071: ldfld class AbstractPhysicalObject Player::objectInStomach
	            IL_0076: callvirt instance void StoryGameSession::RemovePersistentTracker(class AbstractPhysicalObject)
	            IL_007b: ldarg.0
            */
            parentCurs.Emit(OpCodes.Ldarg_1);
            parentCurs.EmitDelegate((Player self, int grasp) =>   // If grabbed by a player, make the player drop the object in their grasps[grasp]
            {
                if (self.isNPC && self.grabbedBy.Count > 0 && self.grabbedBy[0].grabber is Player parent)
                {
                    parent.ReleaseGrasp(grasp);
                    return true;
                }
                return false;
            });
            parentCurs.Emit(OpCodes.Brtrue_S, releaseLabel);
            parentCurs.Emit(OpCodes.Ldarg_0);
        }
        private static void IL_Player_GrabUpdate(ILContext il)
        {
            ILCursor counterCurs = new(il);

            ILLabel branchLabel = il.DefineLabel();

            counterCurs.GotoNext(x => x.MatchStfld<PlayerGraphics>(nameof(PlayerGraphics.swallowing)));
            counterCurs.GotoNext(x => x.MatchLdcI4(0), x => x.MatchStfld<Player>(nameof(Player.swallowAndRegurgitateCounter)));
            counterCurs.GotoPrev(x => x.MatchBr(out branchLabel)); // Get IL_1aeb as branchlabel
            /* GOTO IL_1ae2
             * 	IL_1ae2: br.s IL_1aeb
	         *  IL_1ae4: ldarg.0
	         *  IL_1ae5: ldc.i4.0
             */
            counterCurs.GotoNext(MoveType.After, x => x.MatchLdarg(0));
            /* GOTO AFTER IL_1ae4
             * 	IL_1ae4: ldarg.0
	         *  IL_1ae5: ldc.i4.0
	         *  IL_1ae6: stfld int32 Player::swallowAndRegurgitateCounter
             */
            counterCurs.EmitDelegate((Player self) =>   // If pupVariables.regurgitating or pupVariables.swallowing, branch to branchLabel
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                {
                    return player.regurgitating || player.swallowing;
                }
                return false;
            });
            counterCurs.Emit(OpCodes.Brtrue_S, branchLabel);
            counterCurs.Emit(OpCodes.Ldarg_0);
        }
        private static void IL_Player_StomachGlowLightColor(ILContext il)
        {
            ILCursor glowCurs = new(il);

            glowCurs.GotoNext(MoveType.After, x => x.Match(OpCodes.Call));
            /* GOTO AFTER IL_0001
             * 	IL_0000: ldarg.0
	         *  IL_0001: call instance class MoreSlugcats.SlugNPCAI Player::get_AI()
	         *  IL_0006: brtrue.s IL_0011 
             */
            glowCurs.Emit(OpCodes.Pop);   // Pop IL_0001 off the stack
            glowCurs.Emit(OpCodes.Ldc_I4_0);   // Emit false into IL_0011 branch
        }
        private static void IL_Player_SetMalnourished(ILContext il)
        {
            ILCursor variCurs = new(il);

            variCurs.GotoNext(MoveType.After, x => x.MatchLdfld<Player>(nameof(Player.SlugCatClass)));
            /* GOTO AFTER ldfld class SlugcatStats/Name Player::SlugCatClass
             * 	ldarg.0
	         *  ldfld class SlugcatStats/Name Player::SlugCatClass
	         *  ldarg.1
             */
            //ldfld class SlugcatStats/Name Player::SlugCatClass => slugpup
            variCurs.Emit(OpCodes.Ldarg_0); // self
            variCurs.EmitDelegate((SlugcatStats.Name slugpup, Player self) =>
            {
                if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC &&
                    SlugNPCAIHooks.pupStateCWT.TryGetValue(self.State as PlayerNPCState, out var pupNPCState))
                {
                    if (pupNPCState.Variant != null)
                    {
                        return pupNPCState.Variant;
                    }
                }
                return slugpup;
            });
        }
        public static void PupSwallowObject(Player self, int grabbedIndex)
        {
            Player parent = null;
            if (self.grabbedBy.Count > 0 && self.grabbedBy[0].grabber is Player player)
            {
                parent = player;
            }
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var mothPup) && mothPup.isMothNPC)
            {
                if (self.objectInStomach == null && self.CanBeSwallowed(parent != null ? parent.grasps[grabbedIndex].grabbed : self.grasps[grabbedIndex].grabbed) && self.Consious)
                {
                    mothPup.swallowing = true;
                    self.swallowAndRegurgitateCounter++;
                    self.AI.heldWiggle = 0;
                    if (self.swallowAndRegurgitateCounter > 90)
                    {
                        self.SwallowObject(grabbedIndex);
                        self.swallowAndRegurgitateCounter = 0;
                        (self.graphicsModule as PlayerGraphics).swallowing = 20;

                        mothPup.swallowing = false;
                        mothPup.wantsToSwallowObject = false;
                    }
                }
            }
        }
        public static void PupRegurgitate(Player self)
        {
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var mothPup) && mothPup.isMothNPC && self.Consious)
            {
                mothPup.regurgitating = true;
                self.swallowAndRegurgitateCounter++;

                bool spitUpObject = false;/*
                if (self.isRotundpup() && self.objectInStomach == null)
                {
                    spitUpObject = true;
                }*/
                self.AI.heldWiggle = 0;
                if (self.swallowAndRegurgitateCounter > 110)
                {
                    if (!spitUpObject || (spitUpObject && self.FoodInStomach > 0 && !self.Malnourished))
                    {
                        if (spitUpObject)
                        {
                            self.SubtractFood(1);
                        }
                        self.Regurgitate();
                    }
                    else
                    {
                        self.firstChunk.vel += new Vector2(Random.Range(-1f, 1f), 0f);
                        self.Stun(30);
                    }

                    self.swallowAndRegurgitateCounter = 0;
                    mothPup.regurgitating = false;
                    mothPup.wantsToRegurgitate = false;
                }
            }
        }
        #endregion
    }
}
