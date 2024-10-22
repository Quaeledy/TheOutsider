using RWCustom;
using System;
using TheOutsider.Player_Hooks;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class Antennae
    {
        private static float vibrate;

        private static float lastVibrate;

        private static float flip;

        private static float lastFlip;

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

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {
                if (player.antennaeSprite >= 1 && sLeaser.sprites.Length >= player.antennaeSprite + 2)
                {
                    var foregroundContainer = rCam.ReturnFContainer("Foreground");
                    var midgroundContainer = newContatiner != null ? newContatiner : rCam.ReturnFContainer("Midground");

                    //触须的添加
                    for (int i = 0; i < 2; i++)
                    {
                        foregroundContainer.RemoveChild(sLeaser.sprites[player.antennaeSprite + i]);
                        midgroundContainer.AddChild(sLeaser.sprites[player.antennaeSprite + i]);
                    }
                    //让手移到触须前面
                    for (int k = 5; k <= 6; k++)
                    {
                        var sprite = sLeaser.sprites[k];
                        sprite.MoveInFrontOfOtherNode(sLeaser.sprites[player.antennaeSprite + 1]);
                    }
                }
            }
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {
                player.MothAntennae(self);
            }
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {

                player.antennaeSprite = sLeaser.sprites.Length;
                Array.Resize(ref sLeaser.sprites, player.antennaeSprite + 2);

                //触须

                /*
                for (int i = 0; i < 2; i++)
                {
                    TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
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

                    TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, true, false);
                    sLeaser.sprites[player.antennaeSprite + i] = triangleMesh;
                }
                */

                for (int i = 0; i < 2; i++)
                {
                    TriangleMesh mesh = TriangleMesh.MakeLongMesh(8, false, true, "MothAntennae" + (i + 1));
                    mesh.alpha = 0.8f;
                    sLeaser.sprites[player.antennaeSprite + i] = mesh;
                }

                self.AddToContainer(sLeaser, rCam, null);
            }
        }

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {
                //倾斜度
                lastVibrate = vibrate;
                vibrate = Custom.LerpAndTick(vibrate, 0.01f, 0.2f, 0.01f);

                lastFlip = flip;

                float num3 = 10f;
                /*
                for (int n = 0; n < this.legs.GetLength(0); n++)
                {
                    for (int num5 = 0; num5 < this.legs.GetLength(1); num5++)
                    {
                        num3 += Custom.DistanceToLine(this.legs[n, num5].pos, self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos);
                    }
                }*/
                if (!float.IsNaN(num3))
                {
                    num3 = Mathf.Lerp(num3, 0f, 0.5f);
                    flip = Custom.LerpAndTick(flip, Mathf.Clamp(num3 / 40f, -1f, 1f), 0.07f, 0.1f);
                }

                //float num = Custom.AimFromOneVectorToAnother(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos);
                Vector2 vector = Custom.DirVec(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos);
                float num2 = 0.8f;
                for (int i = 0; i < player.antennae.Length; i++)
                {
                    player.antennae[i].Update();
                    Vector2 vector3 = (Custom.DirVec(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos) + Custom.PerpendicularVector(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos) * flip * 0.5f + Custom.PerpendicularVector(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos) * ((i == 0) ? -1f : 1f) * (1f - Mathf.Abs(flip)) * Mathf.Lerp(0.35f, 1.2f, vibrate)).normalized;
                    if (num2 > 0f)
                    {
                        vector3 = Vector3.Slerp(vector3, Custom.DirVec(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos), num2 * 0.8f);
                    }
                    player.antennae[i].ConnectToPoint(self.player.mainBodyChunk.pos, Mathf.Lerp(50f, 40f, num2) * player.antennaeLength, false, 0f, self.player.mainBodyChunk.vel, 0.05f, 0f);
                    player.antennae[i].vel += vector3 * Custom.LerpMap(Vector2.Distance(player.antennae[i].pos, self.player.mainBodyChunk.pos + vector3 * Mathf.Lerp(50f, 40f, num2) * player.antennaeLength), 10f, 150f, 0f, 14f, 0.7f);
                    if (self.player.Consious && Random.value > num2)
                    {
                        player.antennae[i].vel += Custom.RNV() * Random.value * Mathf.Lerp(1f, 0.1f, num2);
                    }
                    if (vibrate > 0f)
                    {
                        player.antennae[i].vel -= vector * 5f * vibrate;
                    }
                }
            }
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var player))
            {
                if (Plugin.optionsMenuInstance.hideAntennae.Value)
                {
                    sLeaser.sprites[player.antennaeSprite].isVisible = false;
                    sLeaser.sprites[player.antennaeSprite + 1].isVisible = false;
                }
                else
                {
                    sLeaser.sprites[player.antennaeSprite].isVisible = true;
                    sLeaser.sprites[player.antennaeSprite + 1].isVisible = true;
                }

                float num = Mathf.Lerp(lastFlip, flip, timeStacker);
                num = 0.5f;
                float d = Mathf.Lerp(lastVibrate, vibrate, timeStacker);

                Vector2 bodyPos = Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker);
                Vector2 hipsPos = Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker);// + Custom.RNV() * Random.value * 10f * d;
                Vector2 headPos = Vector2.Lerp(self.head.lastPos, self.head.pos, timeStacker);
                //头部至脸部方向的向量(转动修正)
                Vector2 headToFace = new Vector2(sLeaser.sprites[9].x - sLeaser.sprites[3].x, sLeaser.sprites[9].y - sLeaser.sprites[3].y);
                //身体至臀部方向的向量
                Vector2 dif = (bodyPos - hipsPos).normalized;
                //身体旋转角度
                float bodyRotation;
                if (self.player.animation != Player.AnimationIndex.ZeroGSwim && self.player.animation != Player.AnimationIndex.ZeroGPoleGrab)
                    bodyRotation = Mathf.Atan2(dif.x, dif.y);
                else
                    bodyRotation = 0;

                for (int i = 0; i < 2; i++)
                {
                    float lastWidth = 5f;
                    var antennaeMesh = sLeaser.sprites[player.antennaeSprite + i] as TriangleMesh;
                    sLeaser.sprites[player.antennaeSprite + i].color = player.GetAntennaeColor();
                    sLeaser.sprites[player.antennaeSprite + i].alpha = 0.8f;

                    //位置修正
                    Vector2 antennaeH = 4f * Vector2.up + ((i == 0) ? -3f : 3f) * Vector2.right;
                    Vector2 normalizedH = 4f * Vector2.up + ((i == 0) ? -1f : 1f) * Vector2.right;

                    //位置修正
                    Vector2 antennaePos = Vector2.Lerp(player.antennae[i].lastPos, player.antennae[i].pos, timeStacker);
                    antennaePos = new Vector2(-3f * headToFace.x, -2.5f * headToFace.y) + headPos - 3 * dif + PlayerGraphicsHooks.VectorRotation(antennaePos - headPos, -bodyRotation * (0.4f * (1 - Mathf.Abs(Mathf.Cos(bodyRotation))) + 1)) + 0.5f * antennaeH + Mathf.Lerp(0, 15f, 1 - Mathf.Abs(Mathf.Cos(bodyRotation))) * ((bodyRotation > 0) ? -1f : 1f) * Vector2.right;

                    Vector2 normalized = (Custom.DirVec(hipsPos, bodyPos) + Custom.PerpendicularVector(hipsPos, bodyPos) * ((i == 0) ? -1f : 1f) * 0.175f).normalized;
                    normalized = PlayerGraphicsHooks.VectorRotation(normalized, -bodyRotation);

                    Vector2 lastBezier = headPos;
                    lastBezier = new Vector2(1f * headToFace.x, 1f * headToFace.y) + headPos + PlayerGraphicsHooks.VectorRotation(lastBezier - headPos, -bodyRotation) + normalizedH * (1 - 0.5f * Mathf.Abs(Mathf.Sin(bodyRotation))) - Mathf.Lerp(0, 10f, 1 - 1 / (1 + Mathf.Pow(Mathf.Sin(bodyRotation), 2))) * ((bodyRotation > 0) ? -1f : 1f) * (player.isFlying ? 1.5f : 1f) * Vector2.right;

                    for (int k = 0; k < 8; k++)
                    {
                        float ratio = Mathf.InverseLerp(0f, 7f, (float)k);
                        float width = 6.5f * Mathf.Lerp(1f, 0.5f, ratio);


                        //贝塞尔曲线
                        Vector2 bezier = Custom.Bezier(lastBezier, lastBezier + normalized * 30f * player.antennaeLength, antennaePos + normalized * 50f * player.antennaeLength, antennaePos, ratio);
                        Vector2 a = Custom.PerpendicularVector(bezier, lastBezier);

                        antennaeMesh.MoveVertice(k * 4, (lastBezier + bezier) / 2f - a * (lastWidth + width) * 0.5f - camPos);
                        antennaeMesh.MoveVertice(k * 4 + 1, (lastBezier + bezier) / 2f + a * (lastWidth + width) * 0.5f - camPos);
                        antennaeMesh.MoveVertice(k * 4 + 2, bezier - a * width - camPos);
                        antennaeMesh.MoveVertice(k * 4 + 3, bezier + a * width - camPos);
                        lastBezier = bezier;
                        lastWidth = width;

                        //侧身时对侧触须修正
                        var antennaeMesh1 = sLeaser.sprites[player.antennaeSprite + 0] as TriangleMesh;
                        var antennaeMesh2 = sLeaser.sprites[player.antennaeSprite + 1] as TriangleMesh;
                        float scale = 0.5f * Mathf.Abs(Mathf.Sin(bodyRotation));
                        if (i == 0 && bodyRotation < 0)
                        {
                            antennaeMesh1.MoveVertice(k * 4, Vector2.Lerp(antennaeMesh1.vertices[k * 4], antennaeMesh2.vertices[k * 4], scale));
                            antennaeMesh1.MoveVertice(k * 4 + 1, Vector2.Lerp(antennaeMesh1.vertices[k * 4 + 1], antennaeMesh2.vertices[k * 4 + 1], scale));
                            antennaeMesh1.MoveVertice(k * 4 + 2, Vector2.Lerp(antennaeMesh1.vertices[k * 4 + 2], antennaeMesh2.vertices[k * 4 + 2], scale));
                            antennaeMesh1.MoveVertice(k * 4 + 3, Vector2.Lerp(antennaeMesh1.vertices[k * 4 + 3], antennaeMesh2.vertices[k * 4 + 3], scale));
                        }
                        else if (i == 1 && bodyRotation > 0)
                        {
                            antennaeMesh2.MoveVertice(k * 4, Vector2.Lerp(antennaeMesh2.vertices[k * 4], antennaeMesh1.vertices[k * 4], scale));
                            antennaeMesh2.MoveVertice(k * 4 + 1, Vector2.Lerp(antennaeMesh2.vertices[k * 4 + 1], antennaeMesh1.vertices[k * 4 + 1], scale));
                            antennaeMesh2.MoveVertice(k * 4 + 2, Vector2.Lerp(antennaeMesh2.vertices[k * 4 + 2], antennaeMesh1.vertices[k * 4 + 2], scale));
                            antennaeMesh2.MoveVertice(k * 4 + 3, Vector2.Lerp(antennaeMesh2.vertices[k * 4 + 3], antennaeMesh1.vertices[k * 4 + 3], scale));
                        }
                    }
                }
            }
        }

        #endregion
    }
}
