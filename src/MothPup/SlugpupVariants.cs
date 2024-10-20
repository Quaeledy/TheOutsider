using IL;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using On;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace TheOutsider.MothPup
{
    public partial class SlugpupStuff   // Slugpup Variants, Variant Stats, and Variant Abilities
    {
        public static float mothChance => 0.5f;

        public static class VariantName
        {
            public static SlugcatStats.Name MothPup;

            public static void RegisterValues()
            {
                MothPup = new("MothPup", true);
            }
            public static void UnregisterValues()
            {
                MothPup?.Unregister();
                MothPup = null;
            }
        }

        // Variant Methods
        public static List<int> ID_MothPupID()
        {
            List<int> idlist = new List<int>();
            return idlist;
        }

        public static List<int> ID_PupIDExclude()
        {
            List<int> idlist = new List<int>
            {
                1000,
                1001,
                2220,
                3118,
                4118,
                765
            };
            return idlist;
        }

        // Hooks
        private static void SlugcatStats_ctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
        {
            orig(self, slugcat, malnourished);
            if (slugcat == VariantName.MothPup)
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
            if (slugcat == VariantName.MothPup)
            {
                return new IntVector2(4, 3);
            }
            return orig(slugcat);
        }
        private static bool SlugcatStats_HiddenOrUnplayableSlugcat(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name i)
        {
            if (i == VariantName.MothPup)
            {
                return true;
            }
            return orig(i);
        }
        private static void IL_SlugcatStats_NourishmentOfObjectEaten(ILContext il)
        {
            ILCursor variCurs = new(il);

            variCurs.GotoNext(MoveType.After, x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Saint)), x => x.Match(OpCodes.Call));
            /* GOTO AFTER IL_000d
             * 	IL_0008: ldsfld class SlugcatStats/Name MoreSlugcats.MoreSlugcatsEnums/SlugcatStatsName::Saint
	         *  IL_000d: call bool class ExtEnum`1<class SlugcatStats/Name>::op_Equality(class ExtEnum`1<!0>, class ExtEnum`1<!0>)
	         *  IL_0012: brfalse.s IL_0046
             */
            variCurs.Emit(OpCodes.Ldarg_0); // slugcatIndex
            variCurs.EmitDelegate((SlugcatStats.Name slugcatIndex) =>   // If slugcatIndex is Tundrapup, return true
            {
                return slugcatIndex == VariantName.MothPup;
            });
            variCurs.Emit(OpCodes.Or);
            variCurs.Emit(OpCodes.Or);

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
        }
        private static void Player_ThrownSpear(On.Player.orig_ThrownSpear orig, Player self, Spear spear)
        {
            orig(self, spear);
            if (self.isMothPup())
            {
                spear.spearDamageBonus = 0.2f + 0.3f * Mathf.Pow(Random.value, 6f);
            }
        }
        private static bool Player_CanEatMeat(On.Player.orig_CanEatMeat orig, Player self, Creature crit)
        {
            if (self.isMothPup())
            {
                return false;
            }
            return orig(self, crit);
        }
        private static bool Player_AllowGrabbingBatflys(On.Player.orig_AllowGrabbingBatflys orig, Player self)
        {
            if (self.isNPC && self.isMothPup())
            {
                return false;
            }
            return orig(self);
        }
        private static float SlugNPCAI_LethalWeaponScore(On.MoreSlugcats.SlugNPCAI.orig_LethalWeaponScore orig, SlugNPCAI self, PhysicalObject obj, Creature target)
        {
            if (self.isMothPup())
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
            if (self.isMothPup())
            {/*
                if (self.TryGetPupVariables(out var pupVariables))
                {
                    if (crit.abstractCreature == pupVariables.giftedItem && crit.dead)
                    {
                        return true;
                    }
                }*/
                return false;
            }
            return orig(self, crit, excludeCentipedes);
        }
        private static bool SlugNPCAI_WantsToEatThis(On.MoreSlugcats.SlugNPCAI.orig_WantsToEatThis orig, SlugNPCAI self, PhysicalObject obj)
        {
            if (self.isMothPup())
            {
                if (obj is Creature or JellyFish or EggBug or FireEgg)
                {
                    return false;
                }
                if (obj is WaterNut)
                {
                    return true;
                }
            }
            return orig(self, obj);

        }
        private static bool SlugNPCAI_HasEdible(On.MoreSlugcats.SlugNPCAI.orig_HasEdible orig, SlugNPCAI self)
        {
            if (self.isMothPup() && self.TryGetPupVariables(out var pupVariables))
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
            if (self.isMothPup())
            {
                if (food is WaterNut)
                {
                    return SlugNPCAI.Food.WaterNut;
                }
            }
            return orig(self, food);
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
                return !self.isMothPup();
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
                if (self.isMothPup())
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
                if (self.isMothPup())
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
                return !self.isMothPup();
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
                if (self.isMothPup())
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
                if (self.isMothPup())
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
                if (self.isMothPup())
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
                if (self.isMothPup())
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
                if (self.isMothPup())
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
                if (self.isMothPup())
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
                return !self.isMothPup();
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
                if (self.isMothPup())
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
                return !self.isMothPup();
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
                if (self.isMothPup())
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
                if (self.isMothPup() || (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup())
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
                if (!self.isRivulet && pupGrabbed != null && pupGrabbed.isMothPup())
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && (pupGrabbed != null && pupGrabbed.isMothPup()))
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
                if (self.isMothPup() || !self.isRivulet && pupGrabbed != null && pupGrabbed.isMothPup())
                {
                    return true;
                }
                return false;
            });
            aquaCurs.Emit(OpCodes.Or);
        }
        private static void IL_Player_ThrowObject(ILContext il)
        {
            ILCursor rotundCurs = new(il);
            ILCursor tundraCurs = new(il);

            if (true)
            {
                while(tundraCurs.TryGotoNext(MoveType.After, x => x.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Saint)), x => x.Match(OpCodes.Call)))
                {
                    /* WHILE TRYGOTO AFTER call bool class ExtEnum`1<class SlugcatStats/Name>::op_Equality(class ExtEnum`1<!0>, class ExtEnum`1<!0>)
                     *  IL_****: ldsfld class SlugcatStats/Name MoreSlugcats.MoreSlugcatsEnums/SlugcatStatsName::Gourmand
                     *  IL_****: call bool class ExtEnum`1<class SlugcatStats/Name>::op_Equality(class ExtEnum`1<!0>, class ExtEnum`1<!0>)
                     *  IL_****: brfalse.s IL_****
                     */
                    tundraCurs.Emit(OpCodes.Ldarg_0);
                    tundraCurs.EmitDelegate((Player self) =>    // If self is Tundrapup, return true
                    {
                        return false;
                    });
                    tundraCurs.Emit(OpCodes.Or);
                }
            }
        }
        private static void IL_Player_ObjectEaten(ILContext il)
        {
            ILCursor nameCurs = new(il);

            while(nameCurs.TryGotoNext(MoveType.After, x => x.MatchLdfld<Player>(nameof(Player.SlugCatClass))))
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
                if (player.playerState.TryGetPupState(out var pupNPCState))
                {
                    if (player.isSlugpup && player.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
                    {
                        pupNPCState.Variant ??= player.GetSlugpupVariant();
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

                if (player.playerState.TryGetPupState(out var pupNPCState))
                {
                    if (player.abstractCreature.superSizeMe) player.playerState.forceFullGrown = true;

                    Random.State state = Random.state;
                    Random.InitState(player.abstractCreature.ID.RandomSeed);
                    if (pupNPCState.Variant != null)
                    {
                        AbstractCreature.Personality personality = player.abstractCreature.personality;
                        if (pupNPCState.Variant == VariantName.MothPup)
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
                if (player.playerState.TryGetPupState(out var pupNPCState))
                {
                    if (pupNPCState.Variant != null)
                    {
                        return pupNPCState.Variant;
                    }
                }
                return slugpup;
            });
        }

    }
}