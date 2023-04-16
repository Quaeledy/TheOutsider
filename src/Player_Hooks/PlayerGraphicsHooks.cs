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

namespace TheOutsider.Player_Hooks
{
    public class PlayerGraphicsHooks
    {
        public static void Init()
        {
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites_Fly;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites_Flare;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites_Stripe;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites_SwallowTail;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;

            IL.PlayerGraphics.InitiateSprites += new ILContext.Manipulator(PlayerGraphics_InitiateSprites1);
        }


        #region ILHooks

        private static void PlayerGraphics_InitiateSprites1(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdstr("Futile_White"),
                                                    i => i.MatchLdloc(0)))
            {
                return;
            }

            cursor.MoveAfterLabels();

            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4_1);

        }
        #endregion

        #region PlayerGraphics

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsMoth)
            {
                return;
            }

            if (player.initialWingSprite > 0 && sLeaser.sprites.Length > player.initialWingSprite + 5)
            {
                var foregroundContainer = rCam.ReturnFContainer("Foreground");
                var midgroundContainer = rCam.ReturnFContainer("Midground");

                //让翅膀移到身体后
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        var sprite1 = sLeaser.sprites[player.WingSprite(i, j)];
                        foregroundContainer.RemoveChild(sprite1);
                        midgroundContainer.AddChild(sprite1);
                        sprite1.MoveBehindOtherNode(sLeaser.sprites[0]);
                    }
                }

                //让尾巴移到臀部后
                sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);

                //尾部凸起
                for (int i = 0; i < 6; i++)
                {
                    foregroundContainer.RemoveChild(sLeaser.sprites[player.stripeSprite + i]);
                    midgroundContainer.AddChild(sLeaser.sprites[player.stripeSprite + i]);
                    //sLeaser.sprites[player.stripeSprite + i].MoveBehindOtherNode(sLeaser.sprites[0]);
                }

                //让凤尾移到臀部后
                for (int i = 0; i < 2; i++)
                {
                    var sprite2 = sLeaser.sprites[player.swallowtailSprite + i];
                    foregroundContainer.RemoveChild(sprite2);
                    midgroundContainer.AddChild(sprite2);
                    sprite2.MoveBehindOtherNode(sLeaser.sprites[0]);
                }

                //触须的添加
                foregroundContainer.RemoveChild(sLeaser.sprites[player.antennaeSprite]);
                midgroundContainer.AddChild(sLeaser.sprites[player.antennaeSprite]);
            }
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsMoth)
            {
                return;
            }

            player.MothSwallowTail(self);

            player.wingDeployment = new float[2];
            player.wingDeploymentSpeed = new float[2];
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsMoth)
            {
                return;
            }

            player.initialWingSprite = sLeaser.sprites.Length;
            player.antennaeSprite = player.initialWingSprite + 12;
            player.FlareSprite = player.antennaeSprite + 1;
            player.swallowtailSprite = player.FlareSprite + 1;
            player.stripeSprite = player.swallowtailSprite + 2;

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 22);

            //翅膀
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    sLeaser.sprites[player.WingSprite(i, j)] = new FSprite("MothWing" + "A" + j, true);
                    sLeaser.sprites[player.WingSprite(i, j)].anchorX = 0f;
                    sLeaser.sprites[player.WingSprite(i, j)].scaleY = 1f;
                }
            }

            //猫猫头
            sLeaser.sprites[player.antennaeSprite] = new FSprite("MothAntennaeHeadA0", true);

            //闪光图层
            sLeaser.sprites[player.FlareSprite] = new FSprite("Futile_White", true);

            //尾巴
            for (int i = 0; i < 6; i++)
            {
                sLeaser.sprites[player.stripeSprite + i] = new FSprite("Pebble5", true);
            }

            //凤尾
            int tailN = player.tailN;
            for (int i = 0; i < 2; i++)
            {
                TriangleMesh.Triangle[] tris2 = new TriangleMesh.Triangle[]
                {
                    new TriangleMesh.Triangle(0, 1, 2),
                    new TriangleMesh.Triangle(1, 2, 3),
                    new TriangleMesh.Triangle(4, 5, 6),
                    new TriangleMesh.Triangle(5, 6, 7),
                    new TriangleMesh.Triangle(8, 9, 10),
                    new TriangleMesh.Triangle(9, 10, 11),
                    new TriangleMesh.Triangle(12, 13, 14),
                    new TriangleMesh.Triangle(13, 14, 15),
                    new TriangleMesh.Triangle(16, 17, 18),
                    new TriangleMesh.Triangle(17, 18, 19),
                    new TriangleMesh.Triangle(20, 21, 22),
                    new TriangleMesh.Triangle(21, 22, 23),
                    new TriangleMesh.Triangle(24, 25, 26),

                    new TriangleMesh.Triangle(2, 3, 4),
                    new TriangleMesh.Triangle(3, 4, 5),
                    new TriangleMesh.Triangle(6, 7, 8),
                    new TriangleMesh.Triangle(7, 8, 9),
                    new TriangleMesh.Triangle(10, 11, 12),
                    new TriangleMesh.Triangle(11, 12, 13),
                    new TriangleMesh.Triangle(14, 15, 16),
                    new TriangleMesh.Triangle(15, 16, 17),
                    new TriangleMesh.Triangle(18, 19, 20),
                    new TriangleMesh.Triangle(19, 20, 21),
                    new TriangleMesh.Triangle(22, 23, 24),
                    new TriangleMesh.Triangle(23, 24, 25)
                };//一个带状mesh 结尾为三角

                TriangleMesh triangleMesh2 = new TriangleMesh("Futile_White", tris2, true, false);
                sLeaser.sprites[player.swallowtailSprite + i] = triangleMesh2;
            }

            self.AddToContainer(sLeaser, rCam, null);
        }

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsMoth)
            {
                return;
            }

            //飞行
            if (player.isFlying)
            {
                player.wingDeploymentGetTo = 1f;
            }
            else
            {
                player.wingDeploymentGetTo = 0.9f;
            }

            player.lastZRotation = player.zRotation;
            player.zRotation = Vector2.Lerp(player.zRotation, Custom.DirVec(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos), 0.15f);
            player.zRotation = player.zRotation.normalized;

            for (int k = 0; k < 2; k++)
            {
                if (self.player.Consious)
                {
                    if (Random.value < 0.033333335f)
                    {
                        player.wingDeploymentSpeed[k] = 0.6f;
                    }
                    else if (player.wingDeployment[k] < player.wingDeploymentGetTo)
                    {
                        player.wingDeployment[k] = Mathf.Min(player.wingDeployment[k] + player.wingDeploymentSpeed[k], player.wingDeploymentGetTo);
                    }
                    else if (player.wingDeployment[k] > player.wingDeploymentGetTo)
                    {
                        player.wingDeployment[k] = Mathf.Max(player.wingDeployment[k] - player.wingDeploymentSpeed[k], player.wingDeploymentGetTo);
                    }
                }
                else if (player.wingDeployment[k] == 1f)
                {
                    player.wingDeployment[k] = 0.9f;
                }
            }

            if (player.currentFlightDuration == 0)
            {
                player.wingTimeAdd = 0f;
            }
            else if (player.currentFlightDuration <= player.UpFlytime)
            {
                player.wingTimeAdd += 1f;
            }

            player.wingOffset += 1f / Random.Range(50, 60);


            //凤尾
            if (!player.isFlying)
            {
                player.tailTimeAdd = 0f;
            }
            else
            {
                player.tailTimeAdd += 1f;
            }

            Vector2 drawPos1 = self.drawPositions[1, 1];
            Vector2 hipsPos = Vector2.Lerp(self.owner.bodyChunks[1].pos, self.owner.bodyChunks[0].pos, 0.25f);
            int tailN = player.tailN;

            //通过身体角度判断移动
            var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, (hipsPos - drawPos1).normalized), -22.5f, 22.5f);
            //实际凤尾偏移
            var nowSwallowTailSpacing = player.SwallowTailSpacing * (Mathf.Abs(moveDeg) > 10 ? 0.3f : 1f);

            for (int i = 0; i < 2; i++)
            {
                var dir = Custom.DirVec(self.owner.bodyChunks[0].pos, hipsPos).normalized;
                var rootPos = hipsPos + (i == 0 ? -1 : 1) * Custom.PerpendicularVector(dir).normalized * nowSwallowTailSpacing + dir * -0.2f;

                var num3 = 1f - Mathf.Clamp((Mathf.Abs(Mathf.Lerp(self.owner.bodyChunks[1].vel.x, self.owner.bodyChunks[0].vel.x, 0.35f)) - 1f) * 0.5f, 0f, 1f);

                Vector2 vector2 = rootPos;
                Vector2 pos = rootPos;
                float num9 = 28f;

                player.SwallowTail[i * tailN].connectedPoint = new Vector2?(rootPos);
                for (int k = 0; k < tailN; k++)
                {
                    player.SwallowTail[k + i * tailN].Update();
                    player.SwallowTail[k + i * tailN].vel *= Mathf.Lerp(0.75f, 0.95f, num3 * (1f - self.owner.bodyChunks[1].submersion));//水中减少速度

                    TailSegment tailSegment = player.SwallowTail[k + i * tailN];
                    tailSegment.vel.y = tailSegment.vel.y - Mathf.Lerp(0.1f, 0.5f, num3) * (1f - self.owner.bodyChunks[1].submersion) * self.owner.EffectiveRoomGravity;
                    num3 = (num3 * 10f + 1f) / 11f;

                    //超出长度限位
                    if (!Custom.DistLess(player.SwallowTail[k + i * tailN].pos, rootPos, player.MaxLength * (k + 1)))
                    {
                        player.SwallowTail[k + i * tailN].pos = rootPos + Custom.DirVec(rootPos, player.SwallowTail[k + i * tailN].pos) * player.MaxLength * (k + 1);
                    }

                    player.SwallowTail[k + i * tailN].vel += Custom.DirVec(vector2, player.SwallowTail[k + i * tailN].pos) * num9 / Vector2.Distance(vector2, player.SwallowTail[k + i * tailN].pos);
                    num9 *= 0.5f;
                    vector2 = pos;
                    pos = player.SwallowTail[k + i * tailN].pos;
                }
            }
        }

        public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsMoth)
            {
                return;
            }

            //凤尾着色
            var fadeLength = 15;
            for (int i = 0; i < 2; i++)
            {
                var mesh = sLeaser.sprites[player.swallowtailSprite + i] as TriangleMesh;
                for (int j = 0; j < fadeLength; j++)
                    mesh.verticeColors[j] = Color.Lerp((Color)Plugin.AntennaeColor.GetColor(self), sLeaser.sprites[0].color, Mathf.Pow(j / (float)(fadeLength - 1), 0.5f));
                for (int j = fadeLength; j < mesh.verticeColors.Length; j++)
                {
                    if (j != 24)
                    {
                        mesh.verticeColors[j] = sLeaser.sprites[0].color;
                    }
                    else
                    {
                        mesh.verticeColors[j] = (Color)Plugin.StripeColor.GetColor(self);
                    }
                }
            }
        }


        static float wingLength = 1f;

        //-- Wing rotation
        static float m5 = -70f;

        //-- Vertical adjustment when crouching
        static float m6 = 4f;

        //-- Distance between wings
        static float distanceWingsTop = 3f;

        //-- wing rotation min/max when flying
        static float m1 = -20f;
        static float m2 = 90f;


        public static Vector2 GetAnimationOffset(PlayerGraphics self)
        {
            Vector2 result = Vector2.zero;

            if (self.player.bodyMode == Player.BodyModeIndex.Stand)
            {
                result.x += self.player.flipDirection * (self.RenderAsPup ? 2f : 6f) * Mathf.Clamp(Mathf.Abs(self.owner.bodyChunks[1].vel.x) - 0.2f, 0f, 1f) * 0.3f;
                result.y += Mathf.Cos((self.player.animationFrame + 0f) / 6f * 2f * 3.1415927f) * (self.RenderAsPup ? 1.5f : 2f) * 0.3f;
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.Crawl)
            {
                float num4 = Mathf.Sin(self.player.animationFrame / 21f * 2f * 3.1415927f);
                float num5 = Mathf.Cos(self.player.animationFrame / 14f * 2f * 3.1415927f);
                result.x += num5 * self.player.flipDirection * 2f;
                result.y -= num4 * -1.5f - 3f;
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam)
            {
                if (self.player.animation == Player.AnimationIndex.ClimbOnBeam)
                {
                    result.x += self.player.flipDirection * 2.5f + self.player.flipDirection * 0.5f * Mathf.Sin(self.player.animationFrame / 20f * 3.1415927f * 2f);
                }
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.WallClimb)
            {
                result.y += 2f;
                result.x -= self.player.flipDirection * (self.owner.bodyChunks[1].ContactPoint.y < 0 ? 3f : 5f);
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.Default)
            {
                if (self.player.animation == Player.AnimationIndex.LedgeGrab)
                {
                    result.x -= self.player.flipDirection * 5f;
                }
            }
            else if (self.player.animation == Player.AnimationIndex.CorridorTurn)
            {
                result += Custom.DegToVec(Random.value * 360f) * 3f * Random.value;
            }

            return result;
        }

        //飞行
        private static void PlayerGraphics_DrawSprites_Fly(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsMoth)
            {
                return;
            }

            var animationOffset = GetAnimationOffset(self);

            //触须
            var headSpriteName = sLeaser.sprites[3].element.name;
            if (!string.IsNullOrWhiteSpace(headSpriteName) && headSpriteName.StartsWith("HeadA"))
            {
                var headSpriteNumber = headSpriteName.Substring(5);

                var antennaeOffsetX = 0f;
                var antennaeOffsetY = 0f;
                switch (headSpriteNumber)
                {
                    case "0":
                    case "1":
                    case "2":
                    case "3":
                        antennaeOffsetY = 2f;
                        break;
                    case "5":
                    case "6":
                        antennaeOffsetX = -1.5f * Math.Sign(sLeaser.sprites[3].scaleX);
                        break;
                    case "7":
                        antennaeOffsetY = -3.5f;
                        break;
                }

                var antennaePos = new Vector2(sLeaser.sprites[3].x + antennaeOffsetX, sLeaser.sprites[3].y + antennaeOffsetY);

                sLeaser.sprites[player.antennaeSprite].scaleX = sLeaser.sprites[3].scaleX * 1.3f;
                sLeaser.sprites[player.antennaeSprite].scaleY = 1.3f;
                sLeaser.sprites[player.antennaeSprite].rotation = sLeaser.sprites[3].rotation;
                sLeaser.sprites[player.antennaeSprite].x = antennaePos.x;
                sLeaser.sprites[player.antennaeSprite].y = antennaePos.y;
                sLeaser.sprites[player.antennaeSprite].element = Futile.atlasManager.GetElementWithName("MothAntennae" + headSpriteName);
                sLeaser.sprites[player.antennaeSprite].color = (Color)Plugin.AntennaeColor.GetColor(self);
                sLeaser.sprites[player.antennaeSprite].alpha = 0.8f;

                player.lastAntennaePos = new Vector2(sLeaser.sprites[player.antennaeSprite].x, sLeaser.sprites[player.antennaeSprite].y);
            }

            for (var i = 5; i <= 8; i++)
            {
                if (!sLeaser.sprites[i].element.name.StartsWith("Mothcat"))
                {
                    sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("Mothcat" + sLeaser.sprites[i].element.name);
                }
            }

            //翅膀
            Vector2 drawPos1 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
            Vector2 hipsPos = Vector2.Lerp(Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker), Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker), 0.25f);

            //身体角度与透视缩放
            var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, (hipsPos - drawPos1).normalized), -90f, 90f);
            var moveScale = Mathf.Cos(moveDeg / 180f * 3.1415927f);
            bool isScale;

            Vector2 vector = Vector3.Slerp(player.lastZRotation, player.zRotation, timeStacker);
            Vector2 vector2 = Vector2.Lerp(Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker), Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker), 0.5f);
            Vector2 normalized = (Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker) - Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker)).normalized;
            Vector2 a = Custom.PerpendicularVector(-normalized);
            float num = Custom.AimFromOneVectorToAnother(-normalized, normalized);

            float num7 = -15f;

            num7 += 3f * Mathf.Abs(vector.x);
            for (int k = 0; k < 2; k++)
            {
                float d = distanceWingsTop * (0.2f + 0.8f * Mathf.Abs(vector.y)) * Mathf.Lerp(1f, 0.85f, Mathf.InverseLerp(0.5f, 0f, player.wingDeployment[k]));
                Vector2 vector3 = vector2 + normalized * num7 + a * d * (k == 0 ? -1f : 1f) + a * vector.x * Mathf.Lerp(-3f, -5f, Mathf.InverseLerp(0.5f, 0f, player.wingDeployment[k]));
                isScale = k == 0 && PlayerEx.playerSelf.input[0].x > 0 || k == 1 && PlayerEx.playerSelf.input[0].x < 0;


                if (Mathf.Abs(num) < 105)
                {
                    player.wingYAdjust = Mathf.Lerp(player.wingYAdjust, m6, 0.05f);
                }
                else
                {
                    player.wingYAdjust = Mathf.Lerp(player.wingYAdjust, 0, 0.05f);
                }

                for (int i = 0; i < 2; i++)
                {
                    //内层翅膀渐变颜色
                    sLeaser.sprites[player.WingSprite(k, i + 4)].color = (Color)Plugin.StripeColor.GetColor(self);
                    //外层翅膀颜色与不透明度
                    sLeaser.sprites[player.WingSprite(k, i + 2)].color = (Color)Plugin.AntennaeColor.GetColor(self);
                    sLeaser.sprites[player.WingSprite(k, i + 2)].alpha = 0.8f;
                }
                //飞行时
                if (player.wingDeployment[k] == 1f)
                {
                    float num10 = Mathf.Cos(player.wingTimeAdd * 3.1415927f / (2f * player.UpFlytime) + 0.35f);
                    float a2 = m1;
                    float b = m2;

                    num10 = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(num10 * 3.1415927f * 2f), 0.7f);
                    num10 = Mathf.Lerp(a2, b, num10);

                    for (int j = 0; j < 6; j += 2)
                    {
                        sLeaser.sprites[player.WingSprite(k, j)].isVisible = false;
                        sLeaser.sprites[player.WingSprite(k, j + 1)].isVisible = true;

                        sLeaser.sprites[player.WingSprite(k, j + 1)].x = vector3.x - camPos.x;
                        sLeaser.sprites[player.WingSprite(k, j + 1)].y = vector3.y - camPos.y;

                        sLeaser.sprites[player.WingSprite(k, j + 1)].x += animationOffset.x;
                        sLeaser.sprites[player.WingSprite(k, j + 1)].y += player.wingYAdjust + animationOffset.y;

                        sLeaser.sprites[player.WingSprite(k, j + 1)].rotation = num - 180f + num10 * (k == 0 ? 1f : -1f);
                        sLeaser.sprites[player.WingSprite(k, j + 1)].scaleX = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(Mathf.Abs(vector.y), 1f, Mathf.Abs(0.5f - num10) * 1.4f)), 1f) * (k == 0 ? 1f : -1f) * wingLength * (isScale ? moveScale : 1);
                        sLeaser.sprites[player.WingSprite(k, j + 1)].scaleY = 0.3f * Mathf.Cos((num - 180f + num10) / 180f * 3.1415927f) + 0.8f;
                    }
                }
                //不飞行时
                else
                {
                    for (int j = 0; j < 6; j += 2)
                    {
                        sLeaser.sprites[player.WingSprite(k, j + 1)].isVisible = false;
                        sLeaser.sprites[player.WingSprite(k, j)].isVisible = true;

                        sLeaser.sprites[player.WingSprite(k, j)].x = vector3.x - camPos.x;
                        sLeaser.sprites[player.WingSprite(k, j)].y = vector3.y - camPos.y;

                        sLeaser.sprites[player.WingSprite(k, j)].x += animationOffset.x;
                        sLeaser.sprites[player.WingSprite(k, j)].y += player.wingYAdjust + animationOffset.y;

                        sLeaser.sprites[player.WingSprite(k, j)].scaleX = (k == 0 ? 1f : -1f) * wingLength * (isScale ? Mathf.Lerp(moveScale, 1, 0.8f) : 1); ;
                        sLeaser.sprites[player.WingSprite(k, j)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker), vector3) - m5 * (k == 0 ? 1f : -1f);
                    }
                }
            }
        }

        //闪光
        private static void PlayerGraphics_DrawSprites_Flare(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsMoth)
            {
                return;
            }

            Vector2 vector = Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker);

            sLeaser.sprites[player.FlareSprite].scale = 2.5f;
            sLeaser.sprites[player.FlareSprite].x = vector.x - camPos.x;
            sLeaser.sprites[player.FlareSprite].y = vector.y - camPos.y;


            if (player.burning == 0f || self.player.room != rCam.room)
            {
                sLeaser.sprites[player.FlareSprite].isVisible = false;
                player.LightColor = player.NullColor;
            }
            else
            {
                sLeaser.sprites[player.FlareSprite].isVisible = true;
                sLeaser.sprites[player.FlareSprite].shader = rCam.room.game.rainWorld.Shaders["FlareBomb"];
                sLeaser.sprites[player.FlareSprite].x = vector.x - camPos.x + Mathf.Lerp(player.lastFlickerDir.x, player.flickerDir.x, timeStacker);
                sLeaser.sprites[player.FlareSprite].y = vector.y - camPos.y + Mathf.Lerp(player.lastFlickerDir.y, player.flickerDir.y, timeStacker);
                sLeaser.sprites[player.FlareSprite].scale = Mathf.Lerp(player.lastFlashRad, player.flashRad, timeStacker) / 16f;
                sLeaser.sprites[player.FlareSprite].color = (Color)Plugin.StripeColor.GetColor(self);
                sLeaser.sprites[player.FlareSprite].alpha = Mathf.Lerp(player.lastFlashAlpha, player.flashAplha, timeStacker);

                player.LightColor = sLeaser.sprites[player.FlareSprite].color;
            }
        }

        //尾巴
        private static void PlayerGraphics_DrawSprites_Stripe(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsMoth)
            {
                return;
            }

            Vector2 tailPos1 = self.tail[0].pos;
            Vector2 tailPos2 = self.tail[1].pos;

            //通过尾巴前两截的夹角判断移动
            var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, (tailPos1 - tailPos2).normalized), -22.5f, 22.5f);

            //设置是否可见
            for (int i = 0; i < 3; i++)
            {
                if (moveDeg > 10f)
                {
                    sLeaser.sprites[player.stripeSprite + i].isVisible = true;
                    sLeaser.sprites[player.stripeSprite + i + 3].isVisible = false;
                }
                else if (moveDeg < -10f)
                {
                    sLeaser.sprites[player.stripeSprite + i].isVisible = false;
                    sLeaser.sprites[player.stripeSprite + i + 3].isVisible = true;
                }
                else if (moveDeg < 5f && moveDeg > -5f)
                {
                    sLeaser.sprites[player.stripeSprite + i].isVisible = true;
                    sLeaser.sprites[player.stripeSprite + i + 3].isVisible = true;
                }
            }

            Vector2 vector = Vector3.Slerp(player.lastZRotation, player.zRotation, timeStacker);
            Vector2 normalized = (Vector2.Lerp(self.tail[0].lastPos, self.tail[0].pos, timeStacker) - Vector2.Lerp(self.tail[1].lastPos, self.tail[1].pos, timeStacker)).normalized;
            Vector2 a = Custom.PerpendicularVector(-normalized);

            for (int k = 0; k < 2; k++)
            {
                for (int i = 0; i < 3; i++)
                {
                    float d = (2f - i * 0.2f) * distanceWingsTop * (0.2f + 0.8f * Mathf.Abs(vector.y)) * Mathf.Lerp(1f, 0.85f, Mathf.InverseLerp(0.5f, 0f, player.wingDeployment[k]));
                    Vector2 drift = a * d * (k == 0 ? -1f : 1f);

                    sLeaser.sprites[player.stripeSprite + k * 3 + i].x = Mathf.Lerp(1.2f * self.tail[0].pos.x - 0.2f * self.tail[1].pos.x, 0.2f * self.tail[0].pos.x + 0.8f * self.tail[1].pos.x, i / 2f) - camPos.x + drift.x;
                    sLeaser.sprites[player.stripeSprite + k * 3 + i].y = Mathf.Lerp(1.2f * self.tail[0].pos.y - 0.2f * self.tail[1].pos.y, 0.2f * self.tail[0].pos.y + 0.8f * self.tail[1].pos.y, i / 2f) - camPos.y + drift.y;
                    sLeaser.sprites[player.stripeSprite + k * 3 + i].scaleX = 0.4f - 0.05f * i;
                    sLeaser.sprites[player.stripeSprite + k * 3 + i].scaleY = 0.4f - 0.05f * i;
                    sLeaser.sprites[player.stripeSprite + k * 3 + i].color = (Color)Plugin.StripeColor.GetColor(self);
                }
            }
        }

        //凤尾
        private static void PlayerGraphics_DrawSprites_SwallowTail(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsMoth)
            {
                return;
            }

            Vector2 drawPos1 = Vector2.Lerp(self.drawPositions[1, 1], self.drawPositions[1, 0], timeStacker);
            Vector2 hipsPos = Vector2.Lerp(Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker), Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker), 0.25f);

            int startSprite = player.swallowtailSprite;

            //通过身体角度判断移动
            var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, (hipsPos - drawPos1).normalized), -22.5f, 22.5f);

            //实际偏移
            var nowSwallowTailSpacing = player.SwallowTailSpacing * (Mathf.Abs(moveDeg) > 10 ? 0.3f : 1f);
            /*
            //还原图层
            for (int i = 0; i < 2; i++)
                sLeaser.sprites[player.swallowtailSprite + i].MoveInFrontOfOtherNode(sLeaser.sprites[player.swallowtailSprite - 1 + i]);

            //设置图层
            if (moveDeg > 10f)
                sLeaser.sprites[player.swallowtailSprite].MoveBehindOtherNode(sLeaser.sprites[0]);
            else if (moveDeg < -10f)
                sLeaser.sprites[player.swallowtailSprite + 1].MoveBehindOtherNode(sLeaser.sprites[0]);
            */
            //实际显示
            for (int k = 0; k < 2; k++)
            {
                var dir = Custom.DirVec(self.drawPositions[0, 0], self.drawPositions[1, 0]).normalized;
                var rootPos = Vector2.Lerp(self.owner.bodyChunks[1].pos, self.owner.bodyChunks[0].pos, 0.35f) + (k == 0 ? -1 : 1) * Custom.PerpendicularVector(dir).normalized * nowSwallowTailSpacing + dir * -0.2f;

                var lastDir = Custom.DirVec(self.drawPositions[0, 1], self.drawPositions[1, 1]).normalized;
                Vector2 vector2 = Vector2.Lerp(Vector2.Lerp(self.owner.bodyChunks[1].lastPos, self.owner.bodyChunks[0].lastPos, 0.35f) + (k == 0 ? -1 : 1) * Custom.PerpendicularVector(lastDir).normalized * nowSwallowTailSpacing + lastDir * 5f, rootPos, timeStacker);
                Vector2 vector4 = (vector2 * 3f + rootPos) / 4f;

                float d2 = 6f;
                int tailN = player.tailN;
                float SwallowTailWidth = player.SwallowTailWidth;

                bool OutLength = false;

                for (int i = 0; i < tailN; i++)
                {


                    Vector2 vector = Vector3.Slerp(player.lastZRotation, player.zRotation, timeStacker);
                    Vector2 a = Custom.PerpendicularVector(-(Vector2.Lerp(self.tail[0].lastPos, self.tail[0].pos, timeStacker) - Vector2.Lerp(self.tail[1].lastPos, self.tail[1].pos, timeStacker)).normalized);
                    float d = 2f * distanceWingsTop * (0.2f + 0.8f * Mathf.Abs(vector.y)) * Mathf.Lerp(1f, 0.85f, Mathf.InverseLerp(0.5f, 0f, player.wingDeployment[k]));
                    float amplitude = Mathf.Pow(2, 1.5f * i / tailN) - 1;
                    Vector2 wave = player.isFlying ? 0.16f * a * amplitude * player.wingTimeAdd * Mathf.Sin(0.4f * (i + (k == 0 ? 0 : 3)) + 0.2f * player.tailTimeAdd + 1.2f) : new Vector2(0, 0);
                    Vector2 drift = (i + 1) / 8f * (a + wave) * d * (k == 0 ? -1f : 1f);

                    Vector2 vector5 = Vector2.Lerp(player.SwallowTail[k * tailN + i].lastPos, player.SwallowTail[k * tailN + i].pos, timeStacker);
                    Vector2 normalized = (vector5 - vector4).normalized;
                    Vector2 widthDir = Custom.PerpendicularVector(normalized);
                    float d3 = Vector2.Distance(vector5, vector4) / 5f;




                    if (i == 0)
                    {
                        d3 = 0f;
                    }

                    if (i != 0 && !Custom.DistLess((sLeaser.sprites[startSprite + k] as TriangleMesh).vertices[i * 4], (sLeaser.sprites[startSprite + k] as TriangleMesh).vertices[i * 4 - 4], 35))
                        OutLength = true;

                    //设置坐标
                    (sLeaser.sprites[startSprite + k] as TriangleMesh).MoveVertice(i * 4, vector4 - widthDir * d2 * SwallowTailWidth + normalized * d3 - camPos + drift);
                    (sLeaser.sprites[startSprite + k] as TriangleMesh).MoveVertice(i * 4 + 1, vector4 + widthDir * d2 * SwallowTailWidth + normalized * d3 - camPos + drift);
                    if (i < tailN - 1)
                    {
                        (sLeaser.sprites[startSprite + k] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - widthDir * player.SwallowTail[k * tailN + i].StretchedRad * SwallowTailWidth - normalized * d3 - camPos + drift);
                        (sLeaser.sprites[startSprite + k] as TriangleMesh).MoveVertice(i * 4 + 3, vector5 + widthDir * player.SwallowTail[k * tailN + i].StretchedRad * SwallowTailWidth - normalized * d3 - camPos + drift);
                    }
                    else
                    {
                        (sLeaser.sprites[startSprite + k] as TriangleMesh).MoveVertice(i * 4 + 2, vector5 - camPos + drift);
                    }
                    d2 = player.SwallowTail[k * 4 + i].StretchedRad;
                    vector4 = vector5;

                    //防止穿模
                    if (i < self.tail.Length)
                    {
                        player.SwallowTail[k * 7 + i].terrainContact = self.tail[i].terrainContact;
                    }
                    else
                    {
                        player.SwallowTail[k * 7 + i].terrainContact = self.tail[3].terrainContact;
                    }

                }
                if (OutLength && sLeaser.sprites[startSprite + k].isVisible)
                    sLeaser.sprites[startSprite + k].isVisible = false;
                else if (!OutLength && !sLeaser.sprites[startSprite + k].isVisible)
                    sLeaser.sprites[startSprite + k].isVisible = true;
            }
        }
        #endregion
    }
}
