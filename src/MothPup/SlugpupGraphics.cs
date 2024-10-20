using BepInEx.Logging;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

namespace TheOutsider.MothPup
{
    public class SlugpupGraphics
    {
        public static void Init()
        {
            // Graphics On Hooks
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            On.PlayerGraphics.MSCUpdate += PlayerGraphics_MSCUpdate;
            On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
            On.PlayerGraphics.DefaultBodyPartColorHex += PlayerGraphics_DefaultBodyPartColorHex;
            On.PlayerGraphics.ColoredBodyPartList += PlayerGraphics_ColoredBodyPartList;
            On.SlugcatHand.Update += SlugcatHand_Update;

            // Graphics IL Hooks
            IL.PlayerGraphics.InitiateSprites += IL_PlayerGraphics_InitiateSprites;
        }


        // Hooks
        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (self.player.isMothPup())
            {
                self.gills = new PlayerGraphics.AxolotlGills(self, 13);
            }
        }
        private static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig, SlugcatHand self)
        {
            orig(self);
            Player pupGrabbed = null;
            int grabbedIndex = -1;
            foreach (var grasped in (self.owner.owner as Player).grasps)
            {
                if (grasped != null && grasped.grabbed is Player pup && pup.isNPC)
                {
                    pupGrabbed = pup;
                    break;
                }
            }
            if (pupGrabbed != null)
            {
                foreach (var grasped in (self.owner.owner as Player).grasps)
                {
                    if (grasped?.grabbed != null && grasped.grabbed != pupGrabbed)
                    {
                        grabbedIndex = grasped.graspUsed;
                        break;
                    }
                }
                if (grabbedIndex > -1)
                {
                    if (pupGrabbed.swallowAndRegurgitateCounter > 10 && pupGrabbed.objectInStomach == null && pupGrabbed.CanBeSwallowed((self.owner.owner as Player).grasps[grabbedIndex].grabbed) && pupGrabbed.Consious)
                    {
                        if (grabbedIndex == self.limbNumber)
                        {
                            self.mode = Limb.Mode.HuntRelativePosition;
                            float num5 = Mathf.InverseLerp(10f, 90f, pupGrabbed.swallowAndRegurgitateCounter);
                            if (num5 < 0.5f)
                            {
                                self.relativeHuntPos *= Mathf.Lerp(0.9f, 0.7f, num5 * 2f);
                                self.relativeHuntPos.y += Mathf.Lerp(2f, 4f, num5 * 2f);
                                self.relativeHuntPos.x *= Mathf.Lerp(1f, 1.2f, num5 * 2f);
                            }
                            else
                            {
                                self.mode = Limb.Mode.HuntAbsolutePosition;
                                self.absoluteHuntPos = (pupGrabbed.graphicsModule as PlayerGraphics).head.pos;
                                self.absoluteHuntPos += new Vector2(0f, -4f) + Custom.RNV() * 2f * Random.value * Mathf.InverseLerp(0.5f, 1f, num5);

                                (pupGrabbed.graphicsModule as PlayerGraphics).blink = 5;
                                (pupGrabbed.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * 2f * Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
                                pupGrabbed.bodyChunks[0].vel += Custom.RNV() * 0.2f * Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
                            }
                        }
                    }
                }
            }
        }
        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self.player.isMothPup())
            {
            }
        }
        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);
            if (self.player.isMothPup())
            {
                self.gills.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Midground"));
            }
            if (self.TryGetPupGraphics(out var pupGraphics))
            {/*
                if (self.player.isTundrapup())
                {
                    rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[pupGraphics.TongueSpriteIndex]);
                }*/
            }
            if (self.player.TryGetPupVariables(out var pupVariables))
            {
                pupVariables.labelManager?.AddLabelstoContainer(rCam);
            }
        }
        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.player.room != null && self.player.isMothPup())
            {
                self.gills.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }
            if (self.TryGetPupGraphics(out var pupGraphics))
            {
            }
        }
        private static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (self.gills != null && self.player.isMothPup())
            {
                Random.State state = Random.state;
                Random.InitState(self.player.abstractCreature.ID.RandomSeed);

                Color baseCol = self.player.ShortCutColor();
                Color.RGBToHSV(baseCol, out float H, out float S, out float V);

                H *= Random.Range(1.35f, 1.7f);
                S *= Random.Range(1.15f, 2.4f);
                if (self.player.npcStats.Dark)
                {
                    V *= Random.Range(7.5f, 9f);
                }
                Random.state = state;

                Color effectCol = Color.HSVToRGB(Mathf.Clamp01(H), S, V);

                if (self.player.room != null && self.player.room.world.game.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)
                {
                    effectCol = Color.red;
                }

                self.gills.effectColor = effectCol;
                self.gills.baseColor = baseCol;
                self.gills.ApplyPalette(sLeaser, rCam, palette);
            }
            if (self.TryGetPupGraphics(out var pupGraphics))
            {
            }
        }
        private static List<string> PlayerGraphics_DefaultBodyPartColorHex(On.PlayerGraphics.orig_DefaultBodyPartColorHex orig, SlugcatStats.Name slugcatID)
        {
            List<string> list = orig(slugcatID);
            if (slugcatID == SlugpupStuff.VariantName.MothPup)
            {
                list.Add("FFFFFF");
                return list;
            }
            return orig(slugcatID);
        }
        private static List<string> PlayerGraphics_ColoredBodyPartList(On.PlayerGraphics.orig_ColoredBodyPartList orig, SlugcatStats.Name slugcatID)
        {
            List<string> list = orig(slugcatID);
            if (slugcatID == SlugpupStuff.VariantName.MothPup)
            {
                list.Add("Gills");
                return list;
            }
            return orig(slugcatID);
        }
        private static void PlayerGraphics_MSCUpdate(On.PlayerGraphics.orig_MSCUpdate orig, PlayerGraphics self)
        {
            orig(self);
            if (self.player.room != null && self.player.isMothPup())
            {
                self.gills.Update();
            }
        }
        private static void IL_PlayerGraphics_InitiateSprites(ILContext il)
        {
            ILCursor initCurs = new(il);

            initCurs.GotoNext(MoveType.After, x => x.MatchCallvirt<PlayerGraphics.Gown>(nameof(PlayerGraphics.Gown.InitiateSprite)), x => x.MatchLdarg(0));
            /* GOTO AFTER IL_0662
             * 	IL_065b: ldarg.1
	         *  IL_065c: ldarg.2
	         *  IL_065d: callvirt instance void PlayerGraphics/Gown::InitiateSprite(int32, class RoomCamera/SpriteLeaser, class RoomCamera)
	         *  IL_0662: ldarg.0
             */
            // IL_0662: ldarg.0 => self
            initCurs.Emit(OpCodes.Ldarg_1); // sLeaser
            initCurs.Emit(OpCodes.Ldarg_2); // rCam
            initCurs.EmitDelegate((PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam) =>   // Resize sLeaser.sprites and initiate pup sprites
            {
                if (self.player.isMothPup())
                {
                    Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + self.gills.numberOfSprites);
                    self.gills.InitiateSprites(sLeaser, rCam);
                }
                if (self.TryGetPupGraphics(out var pupGraphics))
                {/*
                    if (self.player.isTundrapup())
                    {
                        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 1);
                        pupGraphics.TongueSpriteIndex = sLeaser.sprites.Length - 1;

                        sLeaser.sprites[pupGraphics.TongueSpriteIndex] = TriangleMesh.MakeLongMesh(self.ropeSegments.Length - 1, false, true);
                    }*/
                }
            });
            initCurs.Emit(OpCodes.Ldarg_0); // re-emit ldarg.0
        }
    }
}