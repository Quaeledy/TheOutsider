using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class Antennaes : OutsiderGraphics
    {
        private GenericBodyPart[] antennaes;
        private int antennaeSprite;
        private int antennaeSpriteLength;
        private float antennaeLength;//触须长度
        private float vibrate;
        private float lastVibrate;
        private float flip;
        private float lastFlip;

        public Antennaes(PlayerGraphics self, TheOutsider outsider) : base(self, outsider)
        {
            antennaeSpriteLength = 2;

            if (isPup)
            {
                antennaeLength = 0.1f;
            }
            else
            {
                antennaeLength = 0.3f;
            }

            antennaes = new GenericBodyPart[2];
            for (int i = 0; i < antennaes.Length; i++)
            {
                antennaes[i] = new GenericBodyPart(self, 1f, 0.5f, 0.9f, self.player.bodyChunks[0]);
            }
        }

        public void Reset()
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            for (int i = 0; i < antennaes.Length; i++)
            {
                antennaes[i].Reset((self as GraphicsModule).owner.bodyChunks[0].pos);
            }
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            if (antennaeSprite >= 1 && sLeaser.sprites.Length >= antennaeSprite + antennaeSpriteLength)
            {
                var foregroundContainer = rCam.ReturnFContainer("Foreground");
                var midgroundContainer = newContatiner != null ? newContatiner : rCam.ReturnFContainer("Midground");

                //触须的添加
                for (int i = 0; i < 2; i++)
                {
                    foregroundContainer.RemoveChild(sLeaser.sprites[antennaeSprite + i]);
                    midgroundContainer.AddChild(sLeaser.sprites[antennaeSprite + i]);
                }
                //让手移到触须前面
                for (int k = 5; k <= 6; k++)
                {
                    var sprite = sLeaser.sprites[k];
                    sprite.MoveInFrontOfOtherNode(sLeaser.sprites[antennaeSprite + 1]);
                }
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            antennaeSprite = sLeaser.sprites.Length;
            Array.Resize(ref sLeaser.sprites, antennaeSprite + antennaeSpriteLength);

            for (int i = 0; i < 2; i++)
            {
                TriangleMesh mesh = TriangleMesh.MakeLongMesh(8, false, true, "MothAntennae" + (i + 1));
                mesh.alpha = 0.8f;
                sLeaser.sprites[antennaeSprite + i] = mesh;
            }
        }

        public void Update()
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
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
            for (int i = 0; i < antennaes.Length; i++)
            {
                antennaes[i].Update();
                Vector2 vector3 = (Custom.DirVec(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos) + Custom.PerpendicularVector(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos) * flip * 0.5f + Custom.PerpendicularVector(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos) * ((i == 0) ? -1f : 1f) * (1f - Mathf.Abs(flip)) * Mathf.Lerp(0.35f, 1.2f, vibrate)).normalized;
                if (num2 > 0f)
                {
                    vector3 = Vector3.Slerp(vector3, Custom.DirVec(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos), num2 * 0.8f);
                }
                antennaes[i].ConnectToPoint(self.player.mainBodyChunk.pos, Mathf.Lerp(50f, 40f, num2) * antennaeLength, false, 0f, self.player.mainBodyChunk.vel, 0.05f, 0f);
                antennaes[i].vel += vector3 * Custom.LerpMap(Vector2.Distance(antennaes[i].pos, self.player.mainBodyChunk.pos + vector3 * Mathf.Lerp(50f, 40f, num2) * antennaeLength), 10f, 150f, 0f, 14f, 0.7f);
                if (self.player.Consious && Random.value > num2)
                {
                    antennaes[i].vel += Custom.RNV() * Random.value * Mathf.Lerp(1f, 0.1f, num2);
                }
                if (vibrate > 0f)
                {
                    antennaes[i].vel -= vector * 5f * vibrate;
                }
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            if (Plugin.optionsMenuInstance.hideAntennae.Value)
            {
                sLeaser.sprites[antennaeSprite].isVisible = false;
                sLeaser.sprites[antennaeSprite + 1].isVisible = false;
            }
            else
            {
                sLeaser.sprites[antennaeSprite].isVisible = true;
                sLeaser.sprites[antennaeSprite + 1].isVisible = true;
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
                var antennaeMesh = sLeaser.sprites[antennaeSprite + i] as TriangleMesh;
                sLeaser.sprites[antennaeSprite + i].color = outsider.GetAntennaeColor();
                sLeaser.sprites[antennaeSprite + i].alpha = 0.8f;

                //位置修正
                Vector2 antennaeH = 4f * Vector2.up + ((i == 0) ? -3f : 3f) * Vector2.right;
                Vector2 normalizedH = 4f * Vector2.up + ((i == 0) ? -1f : 1f) * Vector2.right;

                //位置修正
                Vector2 antennaePos = Vector2.Lerp(antennaes[i].lastPos, antennaes[i].pos, timeStacker);
                antennaePos = new Vector2(-3f * headToFace.x, -2.5f * headToFace.y) + headPos - 3 * dif + PlayerGraphicsHooks.VectorRotation(antennaePos - headPos, -bodyRotation * (0.4f * (1 - Mathf.Abs(Mathf.Cos(bodyRotation))) + 1)) + 0.5f * antennaeH + Mathf.Lerp(0, 15f, 1 - Mathf.Abs(Mathf.Cos(bodyRotation))) * ((bodyRotation > 0) ? -1f : 1f) * Vector2.right;

                Vector2 normalized = (Custom.DirVec(hipsPos, bodyPos) + Custom.PerpendicularVector(hipsPos, bodyPos) * ((i == 0) ? -1f : 1f) * 0.175f).normalized;
                normalized = PlayerGraphicsHooks.VectorRotation(normalized, -bodyRotation);

                Vector2 lastBezier = headPos;
                lastBezier = new Vector2(1f * headToFace.x, 1f * headToFace.y) + headPos + PlayerGraphicsHooks.VectorRotation(lastBezier - headPos, -bodyRotation) + normalizedH * (1 - 0.5f * Mathf.Abs(Mathf.Sin(bodyRotation))) - Mathf.Lerp(0, 10f, 1 - 1 / (1 + Mathf.Pow(Mathf.Sin(bodyRotation), 2))) * ((bodyRotation > 0) ? -1f : 1f) * (outsider.isFlying ? 1.5f : 1f) * Vector2.right;

                for (int k = 0; k < 8; k++)
                {
                    float ratio = Mathf.InverseLerp(0f, 7f, (float)k);
                    float width = 6.5f * Mathf.Lerp(1f, 0.5f, ratio);


                    //贝塞尔曲线
                    Vector2 bezier = Custom.Bezier(lastBezier, lastBezier + normalized * 30f * antennaeLength, antennaePos + normalized * 50f * antennaeLength, antennaePos, ratio);
                    Vector2 a = Custom.PerpendicularVector(bezier, lastBezier);

                    antennaeMesh.MoveVertice(k * 4, (lastBezier + bezier) / 2f - a * (lastWidth + width) * 0.5f - camPos);
                    antennaeMesh.MoveVertice(k * 4 + 1, (lastBezier + bezier) / 2f + a * (lastWidth + width) * 0.5f - camPos);
                    antennaeMesh.MoveVertice(k * 4 + 2, bezier - a * width - camPos);
                    antennaeMesh.MoveVertice(k * 4 + 3, bezier + a * width - camPos);
                    lastBezier = bezier;
                    lastWidth = width;

                    //侧身时对侧触须修正
                    var antennaeMesh1 = sLeaser.sprites[antennaeSprite + 0] as TriangleMesh;
                    var antennaeMesh2 = sLeaser.sprites[antennaeSprite + 1] as TriangleMesh;
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
}
