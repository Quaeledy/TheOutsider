using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using System.Reflection;
using UnityEngine;
using Custom = RWCustom.Custom;
using Random = UnityEngine.Random;

namespace TheOutsider.MothPup_Hooks
{
    public class PlayerHooks
    {
        private static BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public;
        private static BindingFlags methodFlags = BindingFlags.Static | BindingFlags.NonPublic;
        public delegate bool orig_Player_isSlugpup(Player self);

        public static void Init()
        {
            IL.Player.NPCStats.ctor += Player_NPCStats_ctorIL;
            IL.Player.ObjectEaten += Player_ObjectEatenIL;
            IL.Player.ThrowObject += Player_ThrowObjectIL;
            IL.Player.FoodInRoom_Room_bool += Player_FoodInRoomIL;
            IL.Player.SetMalnourished += Player_SetMalnourishedIL;

            On.Player.NPCStats.ctor += Player_NPCStats_ctor;
            On.Player.ctor += Player_ctor;
            On.Player.AllowGrabbingBatflys += Player_AllowGrabbingBatflys;
            On.Player.CanEatMeat += Player_CanEatMeat;
            On.Player.ThrownSpear += Player_ThrownSpear;
            On.Player.ShortCutColor += Player_ShortCutColor;

            Hook hook = new Hook(typeof(Player).GetProperty(nameof(Player.isSlugpup), PlayerHooks.propFlags).GetGetMethod(), typeof(PlayerHooks).GetMethod(nameof(Player_get_isSlugpup), PlayerHooks.methodFlags));
        }

        private static bool Player_get_isSlugpup(PlayerHooks.orig_Player_isSlugpup orig, Player self)
        {
            bool result = orig(self);
            if (self.abstractCreature.creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                result = true;
            return result;
        }

        #region IL Hooks
        private static void Player_ThrowObjectIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (true)
            {
                while (c.TryGotoNext(MoveType.After,
                    i => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Saint)),
                    i => i.Match(OpCodes.Call)))
                {
                    Plugin.Log("Player_ThrowObjectIL MatchFind!");
                    c.Emit(OpCodes.Ldarg_0); // self
                    c.EmitDelegate((bool isSaint, Player self) =>
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

        private static void Player_NPCStats_ctorIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Slugpup))))
            {
                Plugin.Log("Player_NPCStats_ctorIL MatchFind!");
                c.Emit(OpCodes.Ldarg_1); // player
                c.EmitDelegate((SlugcatStats.Name slugpup, Player player) =>
                {
                    SlugcatStats.Name result = slugpup;
                    if (player.abstractCreature.creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        result = Plugin.Mothpup;
                    }
                    return result;
                });
            }
        }

        private static void Player_ObjectEatenIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(MoveType.After,
                i => i.MatchLdfld<Player>(nameof(Player.SlugCatClass))))
            {
                Plugin.Log("Player_ObjectEatenIL MatchFind!");
                c.Emit(OpCodes.Ldarg_0); // self
                c.EmitDelegate((SlugcatStats.Name SlugCatClass, Player self) =>
                {
                    SlugcatStats.Name result = SlugCatClass;
                    if (self.abstractCreature.creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        result = self.slugcatStats.name;
                    }
                    return result;
                });
            }
        }

        private static void Player_FoodInRoomIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(MoveType.After,
                i => i.MatchLdfld<Player>(nameof(Player.SlugCatClass))))
            {
                Plugin.Log("Player_FoodInRoomIL MatchFind!");
                c.Emit(OpCodes.Ldarg_0); // self
                c.EmitDelegate((SlugcatStats.Name SlugCatClass, Player self) =>   // If self.isSlugpup, return slugcatStats.name, else return SlugCatClass
                {
                    SlugcatStats.Name result = SlugCatClass;
                    if (self.abstractCreature.creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        result = self.slugcatStats.name;
                    }
                    return result;
                });
            }
        }

        private static void Player_SetMalnourishedIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdfld<Player>(nameof(Player.SlugCatClass))))
            {
                Plugin.Log("Player_SetMalnourishedIL MatchFind!");
                c.Emit(OpCodes.Ldarg_0); // self
                c.EmitDelegate((SlugcatStats.Name slugpup, Player self) =>
                {
                    SlugcatStats.Name result = slugpup;
                    if (self.abstractCreature.creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
                    {
                        result = Plugin.Mothpup;
                    }
                    return result;
                });
            }
        }
        #endregion
        private static void Player_NPCStats_ctor(On.Player.NPCStats.orig_ctor orig, Player.NPCStats self, Player player)
        {
            orig(self, player);
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(player, out var outsider) && outsider.isMothNPC)
            {
            }
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.abstractCreature.creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup &&
                !Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out _))
            {
                TheOutsider outsider = new TheOutsider(self);
                Player_Hooks.PlayerHooks.PlayerData.Add(self, outsider);

                if (outsider.isMothNPC)
                {
                    if (self.abstractCreature.superSizeMe)
                        self.playerState.forceFullGrown = true;

                    Random.InitState(self.abstractCreature.ID.number);
                    if (!outsider.isColorVariation)
                    {
                        self.npcStats.Dark = true;
                        Vector3 defaultColor = Custom.RGB2HSL(TheOutsider.BlueGreen);
                        self.npcStats.H = ((self.npcStats.H + 0.5f) % 1f) * Random.value * (Random.value - 1f) * 0.2f + defaultColor.x;
                        self.npcStats.S = ((self.npcStats.S + 0.5f) % 1f) * Random.value * (Random.value - 1f) * 0.2f + defaultColor.y;
                        self.npcStats.L = ((self.npcStats.L + 0.5f) % 1f) * Random.value * (Random.value - 1f) * 0.2f + defaultColor.z;
                    }
                    AbstractCreature.Personality personality = self.abstractCreature.personality;
                    personality.energy = Mathf.Pow(Mathf.Clamp01(personality.energy + 0.1f), 0.5f);
                    personality.aggression = Mathf.Pow(Mathf.Clamp01(personality.aggression - 1f), 2f);
                    personality.sympathy = Mathf.Pow(Mathf.Clamp01(personality.sympathy + 0.1f), 0.5f);
                    self.abstractCreature.personality = personality;
                }
            }
        }

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

        private static Color Player_ShortCutColor(On.Player.orig_ShortCutColor orig, Player self)
        {
            Color result = orig(self);
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
            {
                Random.InitState(self.abstractCreature.ID.number);
                if (!player.isColorVariation)
                {
                    result = Custom.HSL2RGB(self.npcStats.H, self.npcStats.S, Mathf.Clamp(self.npcStats.Dark ? self.npcStats.L : (1f - self.npcStats.L), 0.01f, 1f), 1f);
                }
            }
            return result;
        }
    }
}
