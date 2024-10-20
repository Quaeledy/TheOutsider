using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Generic;
using JetBrains.Annotations;
using SlugBase.Features;
using System.Drawing;
using TheOutsider.Player_Hooks;
using System.Security.Permissions;
using System.Security;
using System.ComponentModel;
using IL;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class PlayerGraphicsHooks
    {
        public static void Init()
        {
            Antennae.Init();
            Wing.Init();
            SwallowTail.Init();
            Speckle.Init();
            Flare.Init();
            On.PlayerGraphics.Reset += PlayerGraphics_Reset;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        }

        private static void PlayerGraphics_Reset(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
        {
            orig(self);
            
            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {
                //防止拉丝
                for (int i = 0; i < player.wing.Length; i++)
                {
                    player.wing[i].Reset((self as GraphicsModule).owner.bodyChunks[0].pos);
                }
                for (int i = 0; i < player.antennae.Length; i++)
                {
                    player.antennae[i].Reset((self as GraphicsModule).owner.bodyChunks[0].pos);
                }
                for (int i = 0; i < player.swallowtail.GetLength(0); i++)
                    for (int j = 0; j < player.swallowtail.GetLength(1); j++)
                        player.swallowtail[i, j].Reset((self as GraphicsModule).owner.bodyChunks[1].pos);
            }
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {
                for (int i = 0; i <= 8; i++)
                    sLeaser.sprites[0].color = player.GetBodyColor();
                sLeaser.sprites[9].color = player.GetEyesColor();
            }
        }

        //向量旋转
        public static Vector2 VectorRotation(Vector2 vector, float rotation)
        {
            Vector2 newVector = new Vector2(vector.x * rotation, vector.y * rotation);
            newVector.x = vector.x * Mathf.Cos(rotation) + vector.y * Mathf.Sin(rotation);
            newVector.y = -vector.x * Mathf.Sin(rotation) + vector.y * Mathf.Cos(rotation);
            //newVector.y = -newVector.x * Mathf.Sin(rotation) + vector.y * Mathf.Cos(rotation);
            return newVector;
        }
    }
}
