﻿using Mono.Cecil.Cil;
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
using SlugBase.DataTypes;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class SwallowTail
    {
        public static void Init()
        {
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
        }
        #region PlayerGraphics

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsOutsider)
            {
                return;
            }

            if (player.swallowtailSprite >= 1 && sLeaser.sprites.Length >= player.swallowtailSprite + 2)
            {
                var foregroundContainer = rCam.ReturnFContainer("Foreground");
                var midgroundContainer = rCam.ReturnFContainer("Midground");
                //让凤尾移到臀部后
                for (int i = 0; i < 2; i++)
                {
                    var sprite = sLeaser.sprites[player.swallowtailSprite + i];
                    foregroundContainer.RemoveChild(sprite);
                    midgroundContainer.AddChild(sprite);
                    sprite.MoveBehindOtherNode(sLeaser.sprites[0]);
                }
            }
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsOutsider)
            {
                return;
            }

            player.MothSwallowTail(self);
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsOutsider)
            {
                return;
            }

            player.swallowtailSprite = sLeaser.sprites.Length;

            Array.Resize(ref sLeaser.sprites, player.swallowtailSprite + 2);

            //凤尾
            for (int i = 0; i < 2; i++)
            {
                TriangleMesh.Triangle[] tris;
                if (self.player.playerState.isPup)
                {
                    tris = new TriangleMesh.Triangle[]
                    {
                        new TriangleMesh.Triangle(0, 1, 2),
                        new TriangleMesh.Triangle(1, 2, 3),
                        new TriangleMesh.Triangle(4, 5, 6),
                        new TriangleMesh.Triangle(5, 6, 7),
                        new TriangleMesh.Triangle(8, 9, 10),
                        new TriangleMesh.Triangle(9, 10, 11),
                        new TriangleMesh.Triangle(12, 13, 14),

                        new TriangleMesh.Triangle(2, 3, 4),
                        new TriangleMesh.Triangle(3, 4, 5),
                        new TriangleMesh.Triangle(6, 7, 8),
                        new TriangleMesh.Triangle(7, 8, 9),
                        new TriangleMesh.Triangle(10, 11, 12),
                        new TriangleMesh.Triangle(11, 12, 13)
                    };//一个带状mesh 结尾为三角
                }
                else
                {
                    tris = new TriangleMesh.Triangle[]
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
                }
                
                TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, true, false);
                sLeaser.sprites[player.swallowtailSprite + i] = triangleMesh;
            }

            self.AddToContainer(sLeaser, rCam, null);
        }

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsOutsider)
            {
                return;
            }

            //凤尾
            player.tailTimeAdd++;

            if(player.tailTimeAdd > float.MaxValue - 100)
                player.tailTimeAdd = 0;

            Vector2 drawPos1 = self.drawPositions[1, 1];
            Vector2 hipsPos = Vector2.Lerp(self.owner.bodyChunks[1].pos, self.owner.bodyChunks[0].pos, 0.25f);
            int tailN = player.tailN;

            //通过身体角度判断移动
            var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, (hipsPos - drawPos1).normalized), -22.5f, 22.5f);
            //实际凤尾偏移
            var nowSwallowTailSpacing = player.swallowTailSpacing * (Mathf.Abs(moveDeg) > 10 ? 0.3f : 1f);

            for (int i = 0; i < 2; i++)
            {
                var dir = Custom.DirVec(self.owner.bodyChunks[0].pos, hipsPos).normalized;
                var rootPos = hipsPos + (i == 0 ? -1 : 1) * Custom.PerpendicularVector(dir).normalized * nowSwallowTailSpacing + dir * -0.2f;

                var num3 = 1f - Mathf.Clamp((Mathf.Abs(Mathf.Lerp(self.owner.bodyChunks[1].vel.x, self.owner.bodyChunks[0].vel.x, 0.35f)) - 1f) * 0.5f, 0f, 1f);

                Vector2 vector2 = rootPos;
                Vector2 pos = rootPos;
                float num9 = 28f;

                player.swallowtail[i * tailN].connectedPoint = new Vector2?(rootPos);
                for (int k = 0; k < tailN; k++)
                {
                    player.swallowtail[k + i * tailN].Update();
                    player.swallowtail[k + i * tailN].vel *= Mathf.Lerp(0.75f, 0.95f, num3 * (1f - self.owner.bodyChunks[1].submersion));//水中减少速度

                    Vector2 drift = Drift(self, player, k, i);

                    TailSegment tailSegment = player.swallowtail[k + i * tailN];
                    tailSegment.vel.y = tailSegment.vel.y - Mathf.Lerp(0.1f, 0.5f, num3) * (1f - self.owner.bodyChunks[1].submersion) * self.owner.EffectiveRoomGravity;
                    num3 = (num3 * 10f + 1f) / 11f;

                    //超出长度限位
                    if (!Custom.DistLess(player.swallowtail[k + i * tailN].pos, rootPos, player.MaxLength * (k + 1)))
                    {
                        player.swallowtail[k + i * tailN].pos = rootPos + Custom.DirVec(rootPos, player.swallowtail[k + i * tailN].pos) * player.MaxLength * (k + 1);
                    }

                    player.swallowtail[k + i * tailN].pos += 0.25f * drift;

                    //让凤尾在水面上不再直立
                    player.swallowtail[k + i * tailN].vel += self.player.animation == Player.AnimationIndex.SurfaceSwim ? 1.5f * Vector2.down * Mathf.Min(1f / drift.magnitude, 1f) : Vector2.zero;

                    //让凤尾受空气阻力作用
                    Vector2 bodyVel = self.player.bodyChunks[0].pos - self.player.bodyChunks[0].lastPos;
                    Vector2 airDrag = player.isFlying ? Vector2.down * bodyVel.y * 0.2f : Vector2.zero;
                    player.swallowtail[k + i * tailN].vel += Vector2.Lerp(Vector2.zero ,airDrag, player.flightTime / player.upFlightTime);

                    player.swallowtail[k + i * tailN].vel += Custom.DirVec(vector2, player.swallowtail[k + i * tailN].pos) * num9 / Vector2.Distance(vector2, player.swallowtail[k + i * tailN].pos);
                    num9 *= 0.5f;
                    vector2 = pos;
                    pos = player.swallowtail[k + i * tailN].pos;
                }
            }
        }

        public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsOutsider)
            {
                return;
            }

            //凤尾着色
            int fadeLength = self.player.playerState.isPup ? 9 : 15;
            int speckleLength = self.player.playerState.isPup ? 12 : 24;
            for (int i = 0; i < 2; i++)
            {
                var mesh = sLeaser.sprites[player.swallowtailSprite + i] as TriangleMesh;
                for (int j = 0; j < fadeLength; j++)
                    mesh.verticeColors[j] = Color.Lerp((Color)Plugin.AntennaeColor.GetColor(self), sLeaser.sprites[0].color, Mathf.Pow(j / (float)(fadeLength - 1), 0.5f));
                for (int j = fadeLength; j < mesh.verticeColors.Length; j++)
                {
                    if (j != speckleLength)
                    {
                        mesh.verticeColors[j] = sLeaser.sprites[0].color;
                    }
                    else
                    {
                        mesh.verticeColors[j] = (Color)Plugin.SpeckleColor.GetColor(self);
                    }
                }
            }
        }

        //设置图层
        public static void SwallowtailLevel(PlayerEx player, RoomCamera.SpriteLeaser sLeaser, float bodyRotation)
        {
            //设置图层
            //俯冲
            if (bodyRotation < -1.6f || bodyRotation > 1.6f)
            {
                for (int i = 0; i < 2; i++)
                {
                    //让凤尾移到身体后
                    sLeaser.sprites[player.swallowtailSprite + i].MoveBehindOtherNode(sLeaser.sprites[0]);
                }
            }
            //侧身
            if (bodyRotation < -0.3f)
            {
                sLeaser.sprites[player.swallowtailSprite + 1].MoveBehindOtherNode(sLeaser.sprites[player.WingSprite(1, 1)]);
            }
            else if (bodyRotation > 0.3f)
            {
                sLeaser.sprites[player.swallowtailSprite].MoveBehindOtherNode(sLeaser.sprites[player.WingSprite(0, 1)]);
            }
            //平飞
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    //让凤尾移到身体后
                        sLeaser.sprites[player.swallowtailSprite + i].MoveBehindOtherNode(sLeaser.sprites[0]);
                }
            }
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            
            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsOutsider)
            {
                return;
            }

            Vector2 bodyPos = sLeaser.sprites[0].GetPosition();
            Vector2 hipsPos = sLeaser.sprites[1].GetPosition();
            
            //身体位置
            Vector2 drawPos1 = Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker);
            //臀部位置
            Vector2 drawPos2 = Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker);
            //身体至臀部方向的向量
            Vector2 dif = (drawPos1 - drawPos2).normalized;
            //身体旋转角度
            float bodyRotation = Mathf.Atan2(dif.x, dif.y);
            
            //设置图层
            SwallowtailLevel(player, sLeaser, bodyRotation);

            int startSprite = player.swallowtailSprite;

            //通过身体角度判断移动
            var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, (hipsPos - bodyPos).normalized), -22.5f, 22.5f);

            //实际偏移
            var nowSwallowTailSpacing = player.swallowTailSpacing * (Mathf.Abs(moveDeg) > 10 ? 0.3f : 1f);
            
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
                float swallowTailWidth = player.swallowTailWidth;

                bool OutLength = false;

                TriangleMesh swallowTail = sLeaser.sprites[startSprite + k] as TriangleMesh;

                for (int i = 0; i < tailN; i++)
                {
                    Vector2 vector5 = Vector2.Lerp(player.swallowtail[k * tailN + i].lastPos, player.swallowtail[k * tailN + i].pos, timeStacker);
                    Vector2 normalized = (vector5 - vector4).normalized;
                    Vector2 widthDir = Custom.PerpendicularVector(normalized);
                    float d3 = Vector2.Distance(vector5, vector4) / 5f;
                    
                    if (i == 0)
                    {
                        d3 = 0f;
                    }
                    
                    if (i != 0 && !Custom.DistLess(swallowTail.vertices[i * 4], swallowTail.vertices[i * 4 - 4], 100))
                        OutLength = true;
                    
                    //设置坐标
                    swallowTail.MoveVertice(i * 4    , vector4 - widthDir * d2 * swallowTailWidth + normalized * d3 - camPos);
                    swallowTail.MoveVertice(i * 4 + 1, vector4 + widthDir * d2 * swallowTailWidth + normalized * d3 - camPos);
                    /*
                    if (i > 0)
                    {
                        swallowTail.MoveVertice(i * 4    , ((swallowTail.vertices[i * 4    ]) + swallowTail.vertices[(i - 1) * 4 + 2]) / 2);
                        swallowTail.MoveVertice(i * 4 + 1, ((swallowTail.vertices[i * 4 + 1]) + swallowTail.vertices[(i - 1) * 4 + 3]) / 2);
                    }*/
                    if (i < tailN - 1)
                    {
                        swallowTail.MoveVertice(i * 4 + 2, vector5 - widthDir * player.swallowtail[k * tailN + i].StretchedRad * swallowTailWidth - normalized * d3 - camPos);
                        swallowTail.MoveVertice(i * 4 + 3, vector5 + widthDir * player.swallowtail[k * tailN + i].StretchedRad * swallowTailWidth - normalized * d3 - camPos);
                    }
                    else
                    {
                        swallowTail.MoveVertice(i * 4 + 2, vector5 - camPos);
                    }
                    d2 = player.swallowtail[k * 4 + i].StretchedRad;
                    vector4 = vector5;

                    //防止穿模
                    if (i < self.tail.Length)
                    {
                        player.swallowtail[k * tailN + i].terrainContact = self.tail[i].terrainContact;
                    }
                    else
                    {
                        player.swallowtail[k * tailN + i].terrainContact = self.tail[3].terrainContact;
                    }
                    
                }
                
                if ((OutLength && sLeaser.sprites[startSprite + k].isVisible || Plugin.optionsMenuInstance.hideSwallowTail.Value))
                    sLeaser.sprites[startSprite + k].isVisible = false;
                else if (!OutLength && !sLeaser.sprites[startSprite + k].isVisible)
                    sLeaser.sprites[startSprite + k].isVisible = true;
            }
        }
        /*
        private static Vector2 Drift(PlayerGraphics self, PlayerEx player, int i, int k, float timeStacker)
        {
            int tailN = player.tailN;

            //身体位置
            Vector2 drawPos1 = Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker);
            //臀部位置
            Vector2 drawPos2 = Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker);
            //身体至臀部方向的向量
            Vector2 dif = (drawPos1 - drawPos2).normalized;
            //身体旋转角度
            float bodyRotation = Mathf.Atan2(dif.x, dif.y);
            //垂直修正
            Vector2 swallowtailH = Mathf.Lerp(0, 7f, 1 - Mathf.Abs(Mathf.Cos(bodyRotation))) * Vector2.up;
            Vector2 a = Custom.PerpendicularVector(-(Vector2.Lerp(self.tail[0].lastPos, self.tail[0].pos, timeStacker) - Vector2.Lerp(self.tail[1].lastPos, self.tail[1].pos, timeStacker)).normalized);
            float d = 2f * distance * (0.2f + 0.8f * Mathf.Abs(dif.y));
            float amplitude = Mathf.Pow(2, 1.5f * i / tailN) - 1;
            Vector2 wave = player.isFlying ? 0.16f * a * amplitude * player.flutterTimeAdd * Mathf.Sin(0.4f * (i + (k == 0 ? 0 : 3)) + 0.2f * player.tailTimeAdd + 1.2f) : new Vector2(0, 0);
            Vector2 drift = i / 8f * (a + wave) * d * (k == 0 ? -1f : 1f) + swallowtailH;
            return drift;
        }
        */

        private static Vector2 Drift(PlayerGraphics self, PlayerEx player, int i, int k)
        {
            int tailN = player.tailN;

            //身体位置
            Vector2 drawPos1 = self.player.bodyChunks[0].lastPos;
            //臀部位置
            Vector2 drawPos2 = self.player.bodyChunks[1].lastPos;
            //身体至臀部方向的向量
            Vector2 dif = (drawPos1 - drawPos2).normalized;
            //身体旋转角度
            float bodyRotation = Mathf.Atan2(dif.x, dif.y);
            //垂直修正
            Vector2 swallowtailH = Mathf.Lerp(0, 7f, 1 - Mathf.Abs(Mathf.Cos(bodyRotation))) * Vector2.up;
            Vector2 a = Custom.PerpendicularVector(-(self.tail[0].lastPos - self.tail[1].lastPos).normalized);
            float d = 2f * distance * (0.2f + 0.8f * Mathf.Abs(dif.y));
            float amplitude = (Mathf.Pow(2, 1.5f * i / tailN) - 1) * self.player.bodyChunks[0].vel.magnitude / 10; ;
            Vector2 wave = 0.16f * a * amplitude * player.flutterTimeAdd * Mathf.Sin(0.4f * (i + (k == 0 ? 0 : 3)) + 0.2f * player.tailTimeAdd + 1.2f);
            Vector2 drift = i / 8f * (a + wave) * d * (k == 0 ? -1f : 1f) + swallowtailH;
            return drift;
        }

        //两凤尾间距
        static float distance = 3f;
        #endregion
    }
}