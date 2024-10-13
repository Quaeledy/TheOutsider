using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
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
using TheOutsider;
using TheOutsider.Player_Hooks;
using System.Numerics;
using IL.ScavengerCosmetic;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class Wing
    {
        public static FAtlas wingAtlas;

        //翅膀长度

        //飞行时，内层翅膀最大/最小旋转角度
        static float innerRotationMin = -70f;
        static float innerRotationMax = 50f;
        //飞行时，外层翅膀最大/最小旋转角度
        static float outerRotationMin = -70f;
        static float outerRotationMax = 50f;

        //内层翅膀和外层翅膀骨架
        static Vector2 innerWing;
        static Vector2 outerWing;

        //内层翅膀旋转角度
        static float innerRotation;
        //外层翅膀旋转角度
        static float outerRotation;
        //身体至臀部方向的向量
        static Vector2 dif;
        //身体旋转角度
        static float bodyRotation;
        //振翅时翅膀旋转角度
        static float flutterRotation;
        //收翅时翅膀旋转角度
        static float foldScale;

        public static void Init()
        {
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
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

            if (player.wingSprite >= 1 && sLeaser.sprites.Length >= player.wingSprite + 6)
            {
                var foregroundContainer = rCam.ReturnFContainer("Foreground");
                var midgroundContainer = rCam.ReturnFContainer("Midground");
                
                //让翅膀移到中景
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var sprite = sLeaser.sprites[player.WingSprite(i, j)];
                        foregroundContainer.RemoveChild(sprite);
                        midgroundContainer.AddChild(sprite);
                    }
                    //翼手
                    if (Plugin.optionsMenuInstance.handWing.Value)
                    {
                        var sprite = sLeaser.sprites[player.handWingSprite + i];
                        foregroundContainer.RemoveChild(sprite);
                        midgroundContainer.AddChild(sprite);
                    }
                }
            }
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsOutsider)
            {
                return;
            }

            player.wingSprite = sLeaser.sprites.Length;
            Array.Resize(ref sLeaser.sprites, player.wingSprite + 6);

            wingAtlas = Futile.atlasManager.LoadAtlas("atlases/mothwings");

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    //内侧翅膀
                    if (j != 1)
                    {
                        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
                        {
                            new TriangleMesh.Triangle(0, 1, 5),
                            new TriangleMesh.Triangle(1, 2, 3),
                            new TriangleMesh.Triangle(1, 3, 4),
                            new TriangleMesh.Triangle(1, 4, 5)
                        };
                        TriangleMesh triangleMesh = new TriangleMesh("MothWing" + "A" + j, tris, true, true);
                        
                        triangleMesh.UVvertices[0] = wingAtlas._elementsByName["MothWing" + "A" + j].uvTopLeft;
                        triangleMesh.UVvertices[2] = wingAtlas._elementsByName["MothWing" + "A" + j].uvTopRight;
                        triangleMesh.UVvertices[3] = wingAtlas._elementsByName["MothWing" + "A" + j].uvBottomRight;
                        triangleMesh.UVvertices[5] = wingAtlas._elementsByName["MothWing" + "A" + j].uvBottomLeft;
                        triangleMesh.UVvertices[1] = Vector2.Lerp(triangleMesh.UVvertices[0], triangleMesh.UVvertices[2], 0.5f);
                        triangleMesh.UVvertices[4] = Vector2.Lerp(triangleMesh.UVvertices[3], triangleMesh.UVvertices[5], 0.5f);
                        
                        sLeaser.sprites[player.WingSprite(i, j)] = triangleMesh;

                        if(j == 2)
                        {
                            triangleMesh._alpha = 0.8f;
                        }
                    }
                    //外侧翅膀
                    else
                    {
                        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
                        {
                            new TriangleMesh.Triangle(0, 1, 2),
                            new TriangleMesh.Triangle(0, 2, 3),
                            new TriangleMesh.Triangle(0, 3, 4),
                            new TriangleMesh.Triangle(0, 4, 5)
                        };
                        TriangleMesh triangleMesh = new TriangleMesh("MothWing" + "A" + j, tris, true, true);
                        
                        triangleMesh.UVvertices[0] = wingAtlas._elementsByName["MothWing" + "A" + j].uvTopLeft;
                        triangleMesh.UVvertices[2] = wingAtlas._elementsByName["MothWing" + "A" + j].uvTopRight;
                        triangleMesh.UVvertices[3] = wingAtlas._elementsByName["MothWing" + "A" + j].uvBottomRight;
                        triangleMesh.UVvertices[5] = wingAtlas._elementsByName["MothWing" + "A" + j].uvBottomLeft;
                        triangleMesh.UVvertices[1] = Vector2.Lerp(triangleMesh.UVvertices[0], triangleMesh.UVvertices[2], 0.5f);
                        triangleMesh.UVvertices[4] = Vector2.Lerp(triangleMesh.UVvertices[3], triangleMesh.UVvertices[5], 0.5f);

                        triangleMesh.alpha = 0.8f;

                        sLeaser.sprites[player.WingSprite(i, j)] = triangleMesh;
                    }
                }
            }


            //翼手
            if (Plugin.optionsMenuInstance.handWing.Value)
            {
                player.handWingSprite = sLeaser.sprites.Length;
                Array.Resize(ref sLeaser.sprites, player.handWingSprite + 2);
                //0 为左手， 1 为右手（与翅膀相反）
                for (int i = 0; i < 2; i++)
                {
                    var hand = sLeaser.sprites[6 - i];
                    var handWing = sLeaser.sprites[player.handWingSprite + i];
                    TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
                        {
                            new TriangleMesh.Triangle(0, 1, 2),
                            new TriangleMesh.Triangle(0, 2, 3),
                            new TriangleMesh.Triangle(0, 3, 4),
                            new TriangleMesh.Triangle(0, 4, 5)
                        };
                    TriangleMesh triangleMesh = new TriangleMesh(hand._element.name, tris, true, true);

                    triangleMesh.UVvertices[0] = 0.2f * hand._element.uvTopLeft + 0.8f * hand._element.uvTopRight;
                    triangleMesh.UVvertices[2] = hand._element.uvTopRight;
                    triangleMesh.UVvertices[3] = hand._element.uvBottomRight;
                    triangleMesh.UVvertices[5] = 0.2f * hand._element.uvBottomLeft + 0.8f * hand._element.uvBottomRight;
                    triangleMesh.UVvertices[1] = Vector2.Lerp(triangleMesh.UVvertices[0], triangleMesh.UVvertices[2], 0.5f);
                    triangleMesh.UVvertices[4] = Vector2.Lerp(triangleMesh.UVvertices[3], triangleMesh.UVvertices[5], 0.5f);

                    triangleMesh.alpha = hand.alpha;

                    sLeaser.sprites[player.handWingSprite + i] = triangleMesh;
                    //sLeaser.sprites[player.handWingSprite + i].anchorX = 0.9f;
                }
            }
            
            self.AddToContainer(sLeaser, rCam, null);
        }
        
        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsOutsider)
            {
                return;
            }

            player.MothWing(self);
        }

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsOutsider)
            {
                return;
            }

            Vector2 drawPos1;
            //身体位置
            Vector2 bodyPos = self.player.bodyChunks[0].lastPos;
            //头部位置
            Vector2 headPos = self.head.lastPos;

            if (player.isFlying || player.spreadWings)
            {
                drawPos1 = bodyPos;
            }
            else
            {
                drawPos1 = Vector2.Lerp(headPos, bodyPos, 0.3f);
            }
            //臀部位置
            Vector2 drawPos2 = self.player.bodyChunks[1].lastPos;  //身体至臀部方向的向量
            dif = player.wingWidth * (drawPos1 - drawPos2).normalized;
            //身体旋转角度
            bodyRotation = Mathf.Atan2(dif.x, dif.y);

            //飞行展开翅膀
            if (player.isFlying)
            {
                if (player.flightTime == 0)
                {
                    player.flutterTimeAdd = 0f;

                    if (self.player.room.gravity <= 0.5f)
                    {
                        //飞行时，内层翅膀最大/最小旋转角度
                        innerRotationMin = -20f;
                        innerRotationMax = 20f;
                        //飞行时，外层翅膀最大/最小旋转角度
                        outerRotationMin = -20f;
                        outerRotationMax = 20f;
                    }
                    else
                    {
                        //飞行时，内层翅膀最大/最小旋转角度
                        innerRotationMin = -70f;
                        innerRotationMax = 50f;
                        //飞行时，外层翅膀最大/最小旋转角度
                        outerRotationMin = -70f;
                        outerRotationMax = 50f;
                    }
                }
                //无重力时扑腾翅膀
                else if (self.player.room.gravity <= 0.5 && self.player.Consious && !self.player.Stunned)
                {
                    player.flutterTimeAdd += 1f;
                    if (player.flutterTimeAdd >= player.upFlightTime)
                    {
                        player.flutterTimeAdd = 0f;
                    }
                }
                else if (player.flutterTimeAdd <= player.upFlightTime)
                {
                    player.flutterTimeAdd += 1f;
                }
            }
            //不是飞行状态
            else
            {
                //站在杆子上时展开翅膀
                if ((self.player.animation == Player.AnimationIndex.HangFromBeam || self.player.animation == Player.AnimationIndex.StandOnBeam) && self.player.input[0].x != 0 && self.player.Consious && !self.player.Stunned)
                {
                    player.flutterTimeAdd += 1f;
                    player.flightTime += 1;
                    if (player.flutterTimeAdd >= 2 * player.upFlightTime)
                    {
                        player.flutterTimeAdd = player.upFlightTime;
                    }
                    if (player.flightTime >= player.upFlightTime)
                    {
                        //不飞行时，内层翅膀最大/最小旋转角度
                        innerRotationMin = 0f;
                        innerRotationMax = 20f;
                        //不飞行时，外层翅膀最大/最小旋转角度
                        outerRotationMin = 0f;
                        outerRotationMax = 20f;
                    }
                    else
                    {
                        //飞行时，内层翅膀最大/最小旋转角度
                        innerRotationMin = -70f;
                        innerRotationMax = 50f;
                        //飞行时，外层翅膀最大/最小旋转角度
                        outerRotationMin = -70f;
                        outerRotationMax = 50f;
                    }
                    player.spreadWings = true;
                    player.foldUpWings = false;
                }
                //飞行结束后收起翅膀
                else if (player.flutterTimeAdd > 0f)// && self.player.canJump > 0
                {
                    if (player.flutterTimeAdd > player.upFlightTime)
                    {
                        player.flutterTimeAdd = player.upFlightTime;
                    }
                    player.flutterTimeAdd -= 1f;
                    player.spreadWings = false;
                    player.foldUpWings = true;
                }
                else
                {
                    player.flightTime = 0;
                    player.flutterTimeAdd = 0f;
                    player.spreadWings = false;
                    player.foldUpWings = false;
                }
            }

            //飞行动画计算
            if (!player.isFlying && !player.spreadWings)
            {
                outerRotation = Mathf.Lerp(outerRotation, outerRotation + bodyRotation / 3, Mathf.Abs((1 - Mathf.Cos(bodyRotation))));
            }
            for (int i = 0; i < 2; i++)
            {
                WingPos(self, player, dif, bodyRotation, innerRotation, outerRotation, i);
            }

            //翼手
            if (Plugin.optionsMenuInstance.handWing.Value && player.flutterTimeAdd > 0)
            {
                //0 为左手， 1 为右手（与翅膀相反）
                self.hands[1].pos = Vector2.Lerp(Vector2.Lerp(player.wing[18 * 0 + 1].pos, player.wing[18 * 0 + 2].pos, 0.6f), Vector2.Lerp(player.wing[18 * 0 + 4].pos, player.wing[18 * 0 + 3].pos, 0.6f), 0.5f);
                self.hands[0].pos = Vector2.Lerp(Vector2.Lerp(player.wing[18 * 1 + 1].pos, player.wing[18 * 1 + 2].pos, 0.6f), Vector2.Lerp(player.wing[18 * 1 + 4].pos, player.wing[18 * 1 + 3].pos, 0.6f), 0.5f); ;
                self.hands[1].mode = Limb.Mode.HuntRelativePosition;
                self.hands[0].mode = Limb.Mode.HuntRelativePosition;
            }
        }

        //设置骨架
        public static void WingAnimation(PlayerEx player, Vector2 dif, float bodyRotation, float innerRotation, float outerRotation, int i)
        {
            if (!player.isFlying && !player.spreadWings)
            {
                outerRotation = Mathf.Lerp(outerRotation, outerRotation + bodyRotation / 3, Mathf.Abs((1 - Mathf.Cos(bodyRotation))));
            }

            //翅膀骨架向量
            innerWing = 0.8f * player.wingLength * new Vector2(Mathf.Cos(innerRotation - bodyRotation), Mathf.Sin(innerRotation - bodyRotation));
            outerWing = 1.0f * player.wingLength * new Vector2(Mathf.Cos(outerRotation - bodyRotation), Mathf.Sin(outerRotation - bodyRotation));

            //左和右决定是否镜像
            innerWing = i == 0 ? innerWing : 2 * Vector2.Dot(innerWing, (dif).normalized) * (dif).normalized - innerWing;
            outerWing = i == 0 ? outerWing : 2 * Vector2.Dot(outerWing, (dif).normalized) * (dif).normalized - outerWing;
            
            //细节修正
            //侧身飞行
            innerRotation = Mathf.Atan2(innerWing.x, innerWing.y);
            float drift = Mathf.Lerp(0, innerRotation, 2 * Mathf.Abs(Mathf.Sin(bodyRotation)));
            float driftScale = Mathf.Lerp(1, 0, Mathf.Cos(drift));
            //胡乱透视(翅膀镜像)
            innerWing = Vector2.Lerp(innerWing, 2 * Vector2.Dot(innerWing, (dif).normalized) * (dif).normalized - innerWing, driftScale);
            outerWing = Vector2.Lerp(outerWing, 2 * Vector2.Dot(outerWing, (dif).normalized) * (dif).normalized - outerWing, 0.5f * driftScale);
            //旋转
            innerWing = PlayerGraphicsHooks.VectorRotation(innerWing, -bodyRotation / 2);
            outerWing = PlayerGraphicsHooks.VectorRotation(outerWing, -bodyRotation / 2);
            //减少内外层翅膀的角度差距
            if (player.isFlying)
            {
                outerWing = Vector2.Lerp(outerWing, innerWing, Mathf.Abs(Mathf.Sin(bodyRotation)));
            }
            
            //俯冲
            if (Mathf.Abs(bodyRotation) > 100f / 180f * 3.1415926f)
            {
                //把翅膀镜像回去
                float scale = Mathf.InverseLerp(100f / 180f * 3.1415926f, 110f / 180f * 3.1415926f, Mathf.Abs(bodyRotation));
                innerWing = i == 0 ? innerWing : Vector2.Lerp(innerWing, 2 * Vector2.Dot(innerWing, (dif).normalized) * (dif).normalized - innerWing, scale);
                outerWing = i == 0 ? outerWing : Vector2.Lerp(outerWing, 2 * Vector2.Dot(outerWing, (dif).normalized) * (dif).normalized - outerWing, scale);
                //调整翅膀大小
                innerWing = Vector2.Lerp(0.5f * innerWing, innerWing, Mathf.Abs(Mathf.Sin(bodyRotation * 2 - 3.1415926f / 2)));
                outerWing = Vector2.Lerp(0.5f * outerWing, outerWing, Mathf.Abs(Mathf.Sin(bodyRotation * 2 - 3.1415926f / 2)));
            }

            //Debug.Log("inner: " + innerWing);
            //Debug.Log("outer: " + outerWing);
        }

        //设置翅膀位置
        public static void WingPos(PlayerGraphics self, PlayerEx player, Vector2 dif, float bodyRotation, float innerRotation, float outerRotation, int i)
        {
            for (int j = 0; j < 18; j++)
            {
                player.wing[i * 18 + j].lastPos = player.wing[i * 18 + j].pos;
                player.wing[i * 18 + j].Update();
            }

            //垂直修正
            float wingH = Mathf.Lerp(0f, 3f, Mathf.Abs(Mathf.Sin(bodyRotation)));

            //幼崽翅膀下移一些
            if (self.player.playerState.isPup)
            {
                wingH -= 4f;
            }

            //水平修正
            float wingW;
            if (self.player.input[0].x > 0)
            {
                wingW = Mathf.Lerp(0f, 5f, Mathf.Abs(Mathf.Sin(bodyRotation * 2f)));
            }
            else if (self.player.input[0].x < 0)
            {
                wingW = -Mathf.Lerp(0f, 5f, Mathf.Abs(Mathf.Sin(bodyRotation * 2f)));
            }
            else
            {
                wingW = 0;
            }

            //飞行时
            if (player.isFlying)
            {
                //时间插值
                float t = player.flutterTimeAdd / player.upFlightTime * 3.1415927f;
                //内层翅膀旋转角度
                flutterRotation = Mathf.Cos(t + 0.5f);
                flutterRotation = Mathf.Abs(flutterRotation);
                innerRotation = 3.1415927f / 180f * Mathf.Lerp(innerRotationMin, innerRotationMax, flutterRotation);
                //外层翅膀旋转角度（滞后于内层翅膀）
                flutterRotation = Mathf.Cos(t + 0.2f);
                flutterRotation = Mathf.Abs(flutterRotation);
                outerRotation = 3.1415927f / 180f * Mathf.Lerp(outerRotationMin, outerRotationMax, flutterRotation);
                //设置骨架
                WingAnimation(player, dif, bodyRotation, innerRotation, outerRotation, i);

                //设置位置
                for (int j = 0; j < 3; j++)
                {
                    int startNum = i * 18 + j * 6;
                    //内层翅膀
                    if (j != 1)
                    {
                        player.wing[startNum + 0].pos = self.player.bodyChunks[0].pos + wingH * Vector2.up;
                        player.wing[startNum + 5].pos = player.wing[startNum + 0].pos - dif + wingH * Vector2.up;
                        player.wing[startNum + 1].pos = player.wing[startNum + 0].pos + 1.5f * innerWing + Custom.RNV() * Random.value * 1f;
                        player.wing[startNum + 2].pos = player.wing[startNum + 1].pos + 0.5f * outerWing + Custom.RNV() * Random.value * 1f;
                        player.wing[startNum + 4].pos = player.wing[startNum + 5].pos + outerWing;
                        player.wing[startNum + 3].pos = player.wing[startNum + 4].pos + 1.5f * innerWing;
                    }
                    //外层翅膀
                    else
                    {
                        player.wing[startNum + 0].pos = player.wing[i * 18 + 0 * 6 + 1].pos;
                        player.wing[startNum + 5].pos = player.wing[i * 18 + 0 * 6 + 5].pos;
                        player.wing[startNum + 2].pos = player.wing[startNum + 0].pos + 2.0f * outerWing;
                        player.wing[startNum + 1].pos = Vector2.Lerp(player.wing[startNum + 1].pos, player.wing[startNum + 2].pos, 0.5f);
                        player.wing[startNum + 4].pos = player.wing[startNum + 5].pos + 1.0f * innerWing;
                        player.wing[startNum + 3].pos = player.wing[startNum + 4].pos + 2.0f * innerWing;
                    }
                }
            }
            //不飞行时
            else
            {
                float t;
                //时间插值
                if (player.foldUpWings || player.spreadWings)
                {
                    t = player.flutterTimeAdd / player.upFlightTime * 3.1415927f;
                }
                else
                {
                    t = 0;
                }
                t = player.upFlightTime - t;
                foldScale = (player.upFlightTime - player.flutterTimeAdd) / player.upFlightTime;

                //内层翅膀旋转角度
                flutterRotation = Mathf.Cos(t + 0.5f + player.upFlightTime);
                flutterRotation = Mathf.Abs(flutterRotation);
                innerRotation = 3.1415927f / 180f * Mathf.Lerp(Mathf.Lerp(innerRotationMin, 0, foldScale), Mathf.Lerp(innerRotationMax, innerRotationMax - 10, foldScale), flutterRotation);

                //外层翅膀旋转角度（滞后于内层翅膀）
                flutterRotation = Mathf.Cos(t + 0.2f + player.upFlightTime);
                flutterRotation = Mathf.Abs(flutterRotation);
                outerRotation = 3.1415927f / 180f * Mathf.Lerp(Mathf.Lerp(outerRotationMin, outerRotationMin - 20, foldScale), Mathf.Lerp(outerRotationMax, outerRotationMax - 130, foldScale), flutterRotation);

                //长度修正
                float foldLength = Mathf.Lerp(1, 2, foldScale);

                //沿身体方向位置修正
                float difScale = Mathf.Lerp(0f, 0.6f, Mathf.Abs(1 - Mathf.Cos(bodyRotation)));

                //设置骨架
                WingAnimation(player, dif, bodyRotation, innerRotation, outerRotation, i);

                //设置位置
                for (int j = 0; j < 3; j++)
                {
                    int startNum = i * 18 + j * 6;
                    float width = 5f * (foldLength - 1) * (i == 0 ? 1 : -1);
                    //内层翅膀
                    if (j != 1)
                    {

                        player.wing[startNum + 0].pos = self.player.bodyChunks[0].pos - wingH * Vector2.up * (self.player.playerState.isPup ? (0.1f - foldScale) : (1f - foldScale)) + wingW * Vector2.left * foldScale - 0.8f * difScale * dif;
                        player.wing[startNum + 5].pos = player.wing[startNum + 0].pos - dif - wingH * Vector2.up * (self.player.playerState.isPup ? (0.1f - foldScale) : (1f - foldScale)) + wingW * Vector2.left * foldScale - 0.2f * difScale * dif;
                        player.wing[startNum + 1].pos = player.wing[startNum + 0].pos + 1.5f / foldLength * innerWing + width * Vector2.right * 0.5f;
                        player.wing[startNum + 2].pos = player.wing[startNum + 1].pos + 0.5f * foldLength * outerWing + width * Vector2.right * 0.5f;
                        player.wing[startNum + 4].pos = player.wing[startNum + 5].pos + 1.0f / foldLength * outerWing + width * Vector2.right * 0.5f;
                        player.wing[startNum + 3].pos = player.wing[startNum + 4].pos + 1.5f / foldLength * innerWing + width * Vector2.right;
                        //爬行姿态调整
                        player.wing[startNum + 2].pos = Vector2.Lerp(player.wing[startNum + 2].pos, player.wing[startNum + 0].pos, Mathf.Abs((1 - Mathf.Cos(bodyRotation))) / 2);
                        player.wing[startNum + 3].pos = Vector2.Lerp(player.wing[startNum + 3].pos, player.wing[startNum + 2].pos, Mathf.Abs((1 - Mathf.Cos(bodyRotation))) / 2);
                    }
                    //外层翅膀
                    else
                    {
                        player.wing[startNum + 0].pos = player.wing[i * 18 + 0 * 6 + 1].pos;
                        player.wing[startNum + 5].pos = player.wing[i * 18 + 0 * 6 + 5].pos - 0.5f * (foldLength - 1) * dif;
                        player.wing[startNum + 2].pos = player.wing[startNum + 0].pos + 2.0f * outerWing + width * Vector2.right * 0.5f;
                        player.wing[startNum + 1].pos = Vector2.Lerp(Vector2.Lerp(player.wing[startNum + 1].pos, player.wing[startNum + 2].pos, 0.5f), player.wing[i * 18 + 0 * 6 + 2].pos, foldScale);
                        player.wing[startNum + 4].pos = player.wing[startNum + 5].pos + 1.0f / (3.5f * foldLength) * innerWing + width * Vector2.right * 0.3f;
                        player.wing[startNum + 3].pos = player.wing[startNum + 4].pos + 2.0f / (1.5f * foldLength) * innerWing + width * Vector2.right * 0.5f;
                        //爬行姿态调整
                        player.wing[startNum + 2].pos = Vector2.Lerp(player.wing[startNum + 2].pos, player.wing[startNum + 0].pos, Mathf.Abs((1 - Mathf.Cos(bodyRotation))) / 2);
                        player.wing[startNum + 3].pos = Vector2.Lerp(player.wing[startNum + 3].pos, player.wing[startNum + 5].pos, Mathf.Abs((1 - Mathf.Cos(bodyRotation))) / 2);
                    }
                }
            }
        }

        //设置图层
        public static void WingLevel(PlayerEx player, RoomCamera.SpriteLeaser sLeaser, float bodyRotation)
        {
            if (Plugin.optionsMenuInstance.hideWingWhenFolded.Value)
            {
                if (!(player.isFlying || player.foldUpWings || player.spreadWings))
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 3; j++)
                            sLeaser.sprites[player.WingSprite(i, j)].isVisible = false;
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 3; j++)
                            sLeaser.sprites[player.WingSprite(i, j)].isVisible = true;
                }
            }
            //设置图层
            //俯冲
            if (bodyRotation < -1.6f || bodyRotation > 1.6f)
            {
                sLeaser.sprites[player.WingSprite(1, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[player.WingSprite(1, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(1, 1)]);
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[player.WingSprite(i, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                    sLeaser.sprites[player.WingSprite(i, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(i, 1)]);
                    sLeaser.sprites[player.WingSprite(i, 2)].isVisible = false;
                }
            }
            //侧飞
            else if (player.isFlying && (bodyRotation < -0.8f || (bodyRotation < -0.3f && player.flutterTimeAdd <= player.upFlightTime)))
            {
                sLeaser.sprites[player.WingSprite(1, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[player.WingSprite(1, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(1, 1)]);
                sLeaser.sprites[player.WingSprite(1, 2)].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(1, 0)]);
                sLeaser.sprites[player.WingSprite(1, 2)].isVisible = true;
            }
            else if (player.isFlying && (bodyRotation > 0.8f || (bodyRotation > 0.3f && player.flutterTimeAdd <= player.upFlightTime)))
            {
                sLeaser.sprites[player.WingSprite(0, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[player.WingSprite(0, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(0, 1)]);
                sLeaser.sprites[player.WingSprite(0, 2)].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(0, 0)]);
                sLeaser.sprites[player.WingSprite(0, 2)].isVisible = true;
            }
            //侧身
            else if (!player.isFlying && bodyRotation < -0.3f && bodyRotation > -1f)
            {
                sLeaser.sprites[player.WingSprite(1, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[player.WingSprite(1, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(1, 1)]);
                sLeaser.sprites[player.WingSprite(1, 2)].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(1, 0)]);
                sLeaser.sprites[player.WingSprite(1, 2)].isVisible = true;
            }
            else if (!player.isFlying && bodyRotation > 0.3f && bodyRotation < 1f)
            {
                sLeaser.sprites[player.WingSprite(0, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[player.WingSprite(0, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(0, 1)]);
                sLeaser.sprites[player.WingSprite(0, 2)].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(0, 0)]);
                sLeaser.sprites[player.WingSprite(0, 2)].isVisible = true;
            }
            //平飞
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    //让翅膀移到身体后
                    for (int j = 0; j < 3; j++)
                    {
                        sLeaser.sprites[player.WingSprite(i, j)].MoveBehindOtherNode(sLeaser.sprites[0]);
                    }
                    //让外层翅膀移到内层翅膀后
                    sLeaser.sprites[player.WingSprite(i, 1)].MoveBehindOtherNode(sLeaser.sprites[player.WingSprite(i, 0)]);
                    sLeaser.sprites[player.WingSprite(i, 2)].isVisible = true;
                }
            }
            //翼手
            if (Plugin.optionsMenuInstance.handWing.Value)
            {
                //让手移到翅膀前
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[player.handWingSprite + i].MoveInFrontOfOtherNode(sLeaser.sprites[player.WingSprite(i, 2)]);
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

            Vector2 drawPos1;

            //身体位置
            Vector2 bodyPos = Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker);
            //头部位置
            Vector2 headPos = Vector2.Lerp(self.head.lastPos, self.head.pos, timeStacker);

            if (player.isFlying || player.spreadWings)
            {
                drawPos1 = bodyPos;
            }
            else
            {
                drawPos1 = Vector2.Lerp(headPos, bodyPos, 0.3f);
            }
            //臀部位置
            Vector2 drawPos2 = Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker);
            //身体至臀部方向的向量
            dif = player.wingWidth * (drawPos1 - drawPos2).normalized;
            //身体旋转角度
            bodyRotation = Mathf.Atan2(dif.x, dif.y);

            //抖动修正
            Vector2 shake = sLeaser.sprites[0].GetPosition() - bodyPos + camPos;

            //设置图层
            if (player.isFlying)
            {
                WingLevel(player, sLeaser, bodyRotation);
            }
            else
            {
                if (Mathf.Abs(bodyRotation) < 105f / 180f * 3.1415926f / 2.5f)
                {
                    WingLevel(player, sLeaser, -bodyRotation * 2.5f);
                }
                else
                {
                    WingLevel(player, sLeaser, bodyRotation * 2.5f);
                }
                sLeaser.sprites[player.WingSprite(0, 2)].isVisible = false;
                sLeaser.sprites[player.WingSprite(1, 2)].isVisible = false;
            }

            //i = 0为右侧翅膀， i = 1 为左侧翅膀
            for (int i = 0; i < 2; i++)
            {
                //内层翅膀渐变颜色
                sLeaser.sprites[player.WingSprite(i, 2)].color = (Color)Plugin.SpeckleColor.GetColor(self);
                //外层翅膀颜色与不透明度
                sLeaser.sprites[player.WingSprite(i, 1)].color = (Color)Plugin.LepidoticWingColor.GetColor(self);
                sLeaser.sprites[player.WingSprite(i, 1)].alpha = 0.8f;


                //设置位置
                for (int j = 0; j < 3; j++)
                {
                    var wing = sLeaser.sprites[player.WingSprite(i, j)] as TriangleMesh;
                    for (int k = 0; k < 6; k++)
                    {
                        wing.MoveVertice(k, shake + Vector2.Lerp(player.wing[i * 18 + j * 6 + k].lastPos, player.wing[i * 18 + j * 6 + k].pos, timeStacker) - camPos);
                    }
                }
                //飞行时抬一下腿
                if (player.isFlying)
                {
                    sLeaser.sprites[4].SetPosition(sLeaser.sprites[4].GetPosition().x, sLeaser.sprites[4].GetPosition().y + 4f * Mathf.Abs(Mathf.Sin(bodyRotation)));
                }

                //翼手
                if (Plugin.optionsMenuInstance.handWing.Value)
                {
                    //0 为左手， 1 为右手（与翅膀相反）
                    var hand = sLeaser.sprites[6 - i];
                    var handWing = sLeaser.sprites[player.handWingSprite + i] as TriangleMesh;
                    var innerWing = sLeaser.sprites[player.WingSprite(i, 0)] as TriangleMesh;
                    var outerWing = sLeaser.sprites[player.WingSprite(i, 1)] as TriangleMesh;
                    handWing.color = hand.color;
                    if (player.isFlying)
                    {
                        //手部的贴图扭曲
                        string[] names = handWing._element.name.Split('_');
                        string name = handWing._element.name.Replace(names[names.Length - 1], hand._element.name);
                        int num = 12;
                        float scale = 0.8f;
                        if (int.TryParse(hand._element.name.Substring(hand._element.name.IndexOf("PlayerArm") + 9), out num))
                        {
                            scale = 0.9f - 0.008f * num;
                        }
                        if (handWing._element.atlas._elementsByName.ContainsKey(name))
                        {
                            hand.isVisible = false;
                            handWing.isVisible = true;
                            //更换贴图
                            handWing._element = handWing._element.atlas._elementsByName[name];
                            handWing.UVvertices[0] = Vector2.Lerp(handWing._element.uvTopLeft, handWing._element.uvTopRight, scale);
                            handWing.UVvertices[2] = handWing._element.uvTopRight;
                            handWing.UVvertices[3] = handWing._element.uvBottomRight;
                            handWing.UVvertices[5] = Vector2.Lerp(handWing._element.uvBottomLeft, handWing._element.uvBottomRight, scale);
                            handWing.UVvertices[1] = Vector2.Lerp(handWing.UVvertices[0], handWing.UVvertices[2], 0.5f);
                            handWing.UVvertices[4] = Vector2.Lerp(handWing.UVvertices[3], handWing.UVvertices[5], 0.5f);
                            //移动位置
                            handWing.MoveVertice(0, sLeaser.sprites[0].GetPosition() + 2.0f * (innerWing.vertices[0] - sLeaser.sprites[0].GetPosition()) + 1.25f * dif * Custom.LerpMap(Mathf.Abs(bodyRotation), 0f, 3.1415f, 1f, 0f));
                            handWing.MoveVertice(5, innerWing.vertices[0] + 2.0f * (innerWing.vertices[5] - innerWing.vertices[0]) - 0.5f * dif * Custom.LerpMap(Mathf.Abs(bodyRotation), 0f, 3.1415f, 1f, 0f));
                            handWing.MoveVertice(1, handWing.vertices[0] + 1.3f * (innerWing.vertices[1] - innerWing.vertices[0]));//innerWing.vertices[1] - innerWing.vertices[0]: 1.5f * Wing.innerWing + Custom.RNV() * Random.value * 1f
                            handWing.MoveVertice(2, handWing.vertices[1] + 4.0f * (innerWing.vertices[2] - innerWing.vertices[1]));//innerWing.vertices[2] - innerWing.vertices[1]: 0.5f * Wing.outerWing + Custom.RNV() * Random.value * 1f
                            handWing.MoveVertice(4, handWing.vertices[5] + 2.5f * (outerWing.vertices[4] - outerWing.vertices[5]));//outerWing.vertices[4] - outerWing.vertices[5]: Wing.innerWing
                            handWing.MoveVertice(3, handWing.vertices[4] + 2.5f * (innerWing.vertices[4] - innerWing.vertices[5]));//innerWing.vertices[4] - innerWing.vertices[5]: Wing.outerWing
                        }
                        else
                        {
                            hand.isVisible = true;
                            handWing.isVisible = false;
                        }
                    }
                    else if (player.flightTime == -1)
                    {
                        hand.isVisible = true;
                        handWing.isVisible = false;
                    }
                }
            }

            if (Plugin.optionsMenuInstance.hideWing.Value)
            {
                for (int i = 0; i < 2; i++)
                    for (int j = 0; j < 3; j++)
                        sLeaser.sprites[player.WingSprite(i, j)].isVisible = false;
            }
        }
        #endregion
    }
}
