using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using UnityEngine;
using Custom = RWCustom.Custom;
using Random = UnityEngine.Random;

namespace TheOutsider.MothPup_Hooks
{
    public class PlayerHooks
    {
        public static void Init()
        {
            IL.Player.NPCStats.ctor += NPCStats_ctorIL;
            IL.Player.ObjectEaten += Player_ObjectEatenIL;
            IL.Player.ThrowObject += Player_ThrowObjectIL;
            IL.Player.FoodInRoom_Room_bool += Player_FoodInRoomIL;
            IL.Player.SetMalnourished += Player_SetMalnourishedIL;

            On.Player.NPCStats.ctor += Player_NPCStats_ctor;
            On.Player.AllowGrabbingBatflys += Player_AllowGrabbingBatflys;
            On.Player.CanEatMeat += Player_CanEatMeat;
            On.Player.ThrownSpear += Player_ThrownSpear;
            On.Player.ShortCutColor += Player_ShortCutColor;
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

        private static void NPCStats_ctorIL(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                i => i.MatchLdsfld<MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcatsEnums.SlugcatStatsName.Slugpup))))
            {
                Plugin.Log("NPCStats_ctorIL MatchFind!");
                c.Emit(OpCodes.Ldarg_1); // player
                c.EmitDelegate((SlugcatStats.Name slugpup, Player player) =>
                {
                    if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(player, out var playerEX) && playerEX.isMothNPC)
                    {
                        return Plugin.MothPup;
                    }
                    return slugpup;
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
                    if (self.isSlugpup)
                    {
                        return self.slugcatStats.name;
                    }
                    return SlugCatClass;
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
                    if (self.isSlugpup)
                    {
                        return self.slugcatStats.name;
                    }
                    return SlugCatClass;
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
                    if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(self, out var player) && player.isMothNPC)
                    {
                        return self.slugcatStats.name;
                    }
                    return slugpup;
                });
            }
        }
        #endregion
        private static void Player_NPCStats_ctor(On.Player.NPCStats.orig_ctor orig, Player.NPCStats self, Player player)
        {
            orig(self, player);
            if (Player_Hooks.PlayerHooks.PlayerData.TryGetValue(player, out var playerEX) && playerEX.isMothNPC)
            {
                if (player.abstractCreature.superSizeMe)
                    player.playerState.forceFullGrown = true;

                Random.InitState(player.abstractCreature.ID.number);
                if (!playerEX.isColorVariation)
                {
                    self.Dark = true;
                    Vector3 defaultColor = Custom.RGB2HSL(PlayerEx.BlueGreen);
                    self.H = ((self.H + 0.5f) % 1f) * Random.value * (Random.value - 1f) * 0.2f + defaultColor.x;
                    self.S = ((self.S + 0.5f) % 1f) * Random.value * (Random.value - 1f) * 0.2f + defaultColor.y;
                    self.L = ((self.L + 0.5f) % 1f) * Random.value * (Random.value - 1f) * 0.2f + defaultColor.z;
                }
                AbstractCreature.Personality personality = player.abstractCreature.personality;
                personality.energy = Mathf.Pow(Mathf.Clamp01(personality.energy + 0.1f), 0.5f);
                personality.aggression = Mathf.Pow(Mathf.Clamp01(personality.aggression - 1f), 2f);
                personality.sympathy = Mathf.Pow(Mathf.Clamp01(personality.sympathy + 0.1f), 0.5f);
                player.abstractCreature.personality = personality;
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
