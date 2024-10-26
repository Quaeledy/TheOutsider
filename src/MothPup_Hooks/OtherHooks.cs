using HUD;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using System.Collections.Generic;
using TheOutsider.CustomLore.CustomCreature;

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

            On.MoreSlugcats.PlayerNPCState.ctor += PlayerNPCState_ctor;
            On.AbstractCreature.ctor += AbstractCreature_ctor;
            On.AImap.TileAccessibleToCreature_IntVector2_CreatureTemplate += AImap_TileAccessibleToCreature;
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
                    if (self.player.creatureTemplate.type == MothPupCritob.Mothpup)
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
                    if ((self.hud.owner as Player).abstractCreature.Room.creatures[i].creatureTemplate.type == MothPupCritob.Mothpup)
                    {
                        isMothPup = true;
                    }
                    return isSlugNPC || isMothPup;
                });
            }
        }
        #endregion
        private static void PlayerNPCState_ctor(On.MoreSlugcats.PlayerNPCState.orig_ctor orig, PlayerNPCState self, AbstractCreature abstractCreature, int playerNumber)
        {
            orig(self, abstractCreature, playerNumber);
            if (abstractCreature.creatureTemplate.type == MothPupCritob.Mothpup)
            {
                self.Glowing = true;
            }
        }
        private static void AbstractCreature_ctor(On.AbstractCreature.orig_ctor orig, AbstractCreature self, World world, CreatureTemplate creatureTemplate, Creature realizedCreature, WorldCoordinate pos, EntityID ID)
        {
            orig(self, world, creatureTemplate, realizedCreature, pos, ID);
            if (self.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC && PlayerEx.PlayerNPCShouldBeMoth(world))
            {
                self.creatureTemplate = StaticWorld.GetCreatureTemplate(MothPupCritob.Mothpup);
            }
        }
        private static void SlugcatStats_ctor(On.SlugcatStats.orig_ctor orig, SlugcatStats self, SlugcatStats.Name slugcat, bool malnourished)
        {
            orig(self, slugcat, malnourished);

            Plugin.Log("SlugcatStats_ctor: " + slugcat.ToString());
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
            if (slugcat == Plugin.Mothpup)// || (player != null && PlayerEx.PlayerNPCShouldBeMoth(player)))
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
            if (crit.type == MothPupCritob.Mothpup)
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
    }
}
