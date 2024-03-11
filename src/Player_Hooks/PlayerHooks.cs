using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using SlugBase;
using SlugBase.DataTypes;
using SlugBase.Features;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TheOutsider.Player_Hooks;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace TheOutsider.Player_Hooks
{
    public class PlayerHooks
    {
        public static ConditionalWeakTable<Player, PlayerEx> PlayerData = new();
        public static void Init()
        {
            On.Player.ctor += Player_ctor;
            On.Player.MovementUpdate += Flight.Player_MovementUpdate;
            On.Player.UpdateMSC += Flight.Player_Fly;
            On.Player.UpdateMSC += Flare.Player_Flare;
            On.Player.Jump += JumpHooks.Player_Jump;
            On.SporeCloud.Update += SporeCloudHooks.Update;
        }

        #region Player
        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            PlayerData.Add(self, new PlayerEx(self));
        }
        #endregion
    }
}
