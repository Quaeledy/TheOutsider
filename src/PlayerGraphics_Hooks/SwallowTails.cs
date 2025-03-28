using RWCustom;
using System;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class SwallowTails : OutsiderGraphics
    {
        private TailSegment[,] swallowtail;
        private int swallowtailSprite;
        private int swallowtailSpriteLength;
        private int tailN;
        private readonly float swallowTailSpacing = 6f;
        private float nowSwallowTailSpacing;
        private float moveDeg;
        private readonly float MaxLength = 15f;
        private readonly float swallowTailWidth = 0.4f;
        private float tailTimeAdd;
        private float distance = 3f;//两凤尾间距

        public SwallowTails(PlayerGraphics self, TheOutsider outsider) : base(self, outsider)
        {
            swallowtailSpriteLength = 2;

            if (isPup)
            {
                tailN = 4;
            }
            else
            {
                tailN = 7;
            }

            swallowtail = new TailSegment[2, tailN];
            for (int i = 0; i < 2; i++)
            {
                if (isPup)
                {
                    swallowtail[i, 0] = new TailSegment(self, 5f, 4f * (isPup ? 0.8f : 1f), null, 0.85f, 1f, 3f, true);
                    swallowtail[i, 1] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 0], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 2] = new TailSegment(self, 7f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 1], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 3] = new TailSegment(self, 6f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 2], 0.55f, 1f, 0.5f, true);
                }
                else
                {
                    swallowtail[i, 0] = new TailSegment(self, 5f, 4f * (isPup ? 0.8f : 1f), null, 0.85f, 1f, 3f, true);
                    swallowtail[i, 1] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 0], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 2] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 1], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 3] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 2], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 4] = new TailSegment(self, 3f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 3], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 5] = new TailSegment(self, 7f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 4], 0.55f, 1f, 0.5f, true);
                    swallowtail[i, 6] = new TailSegment(self, 6f, 7f * (isPup ? 0.8f : 1f), swallowtail[i, 5], 0.55f, 1f, 0.5f, true);
                }
            }
        }

        public void Reset()
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            for (int i = 0; i < swallowtail.GetLength(0); i++)
                for (int j = 0; j < swallowtail.GetLength(1); j++)
                    swallowtail[i, j].Reset((self as GraphicsModule).owner.bodyChunks[1].pos);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            if (swallowtailSprite >= 1 && sLeaser.sprites.Length >= swallowtailSprite + swallowtailSpriteLength)
            {
                var foregroundContainer = rCam.ReturnFContainer("Foreground");
                var midgroundContainer = newContatiner != null ? newContatiner : rCam.ReturnFContainer("Midground");
                //让凤尾移到臀部后
                for (int i = 0; i < 2; i++)
                {
                    var sprite = sLeaser.sprites[swallowtailSprite + i];
                    foregroundContainer.RemoveChild(sprite);
                    midgroundContainer.AddChild(sprite);
                    sprite.MoveBehindOtherNode(sLeaser.sprites[0]);
                }
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            swallowtailSprite = sLeaser.sprites.Length;

            Array.Resize(ref sLeaser.sprites, swallowtailSprite + swallowtailSpriteLength);

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
                sLeaser.sprites[swallowtailSprite + i] = triangleMesh;
            }
        }

        public void Update()
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;

            //凤尾
            tailTimeAdd++;

            if (tailTimeAdd > 3.14159f * 2000f / 0.2f)
                tailTimeAdd = 0;

            Vector2 drawPos1 = self.drawPositions[1, 1];
            Vector2 hipsPos = Vector2.Lerp(self.owner.bodyChunks[1].pos, self.owner.bodyChunks[0].pos, 0.25f);
            Vector2 bodyVel = Vector2.Lerp(self.player.bodyChunks[0].pos - self.player.bodyChunks[0].lastPos, self.player.bodyChunks[1].pos - self.player.bodyChunks[1].lastPos, 0.5f);
            //通过身体角度判断移动
            float bodyAngle = Custom.VecToDeg(hipsPos - drawPos1);
            float moveDeg = self.player.animation == Player.AnimationIndex.StandOnBeam ? 60f : 22.5f;
            var moveScale = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, (hipsPos - drawPos1).normalized), -moveDeg, moveDeg);
            //实际凤尾偏移
            nowSwallowTailSpacing = swallowTailSpacing * Custom.LerpMap(Mathf.Abs(moveScale), 0, 10, 1f, 0.3f);

            for (int i = 0; i < swallowtail.GetLength(0); i++)
            {
                var dir = Custom.DirVec(self.owner.bodyChunks[0].pos, hipsPos).normalized;
                var rootPos = hipsPos + (i == 0 ? -1 : 1) * Custom.PerpendicularVector(dir).normalized * nowSwallowTailSpacing + dir * -0.2f;

                var num3 = 1f - Mathf.Clamp((Mathf.Abs(Mathf.Lerp(self.owner.bodyChunks[1].vel.x, self.owner.bodyChunks[0].vel.x, 0.35f)) - 1f) * 0.5f, 0f, 1f);
                /*
                Vector2 vector2 = rootPos;
                Vector2 pos = rootPos;
                float num9 = 28f;*/
                swallowtail[i, 0].connectedPoint = new Vector2?(rootPos);
                Vector2 lastNormalized = dir;
                for (int k = 0; k < swallowtail.GetLength(1); k++)
                {
                    //超出长度限位
                    if (!Custom.DistLess(swallowtail[i, k].pos, rootPos, MaxLength))
                    {
                        swallowtail[i, k].pos = rootPos + Custom.DirVec(rootPos, swallowtail[i, k].pos) * MaxLength;// * (1 + t);if (k > 1)
                        swallowtail[i, k].vel *= 0.5f;
                    }

                    float t = (float)k / (float)(swallowtail.GetLength(1) - 1);//在单根触须上的长度占比
                    swallowtail[i, k].Update();
                    swallowtail[i, k].vel *= Mathf.Lerp(0.75f, 0.95f, num3 * (1f - self.owner.bodyChunks[1].submersion));//水中减少速度

                    Vector2 drift = Drift(k, i, bodyVel) * Mathf.InverseLerp(outsider.isFlying ? 0f : 2f, outsider.isFlying ? 4f : 8f, bodyVel.magnitude);
                    swallowtail[i, k].vel.y = swallowtail[i, k].vel.y - Mathf.Lerp(0.1f, 0.5f, num3) * (1f - self.owner.bodyChunks[1].submersion) * self.owner.EffectiveRoomGravity;
                    num3 = (num3 * 10f + 1f) / 11f;

                    swallowtail[i, k].pos += 0.1f * drift;

                    //让凤尾在水面上不再直立
                    //swallowtail[k + i * tailN].vel += self.player.animation == Player.AnimationIndex.SurfaceSwim ? 1.5f * Vector2.down * Mathf.Min(1f / drift.magnitude, 1f) : Vector2.zero;
                    //swallowtail[k + i * tailN].vel += Custom.DirVec(vector2, swallowtail[k + i * tailN].pos) * num9 / Vector2.Distance(vector2, swallowtail[k + i * tailN].pos);

                    swallowtail[i, k].vel *= Custom.LerpMap(swallowtail[i, k].vel.magnitude, 1f, 1f + 1f * bodyVel.magnitude, 0.999f, 0.9f, Mathf.Lerp(1.4f, 0.4f, t));
                    swallowtail[i, k].vel += (Random.value <= 0.2f) ? Custom.RNV() * (1f - t) * (outsider.isFlying ? 1f : Mathf.InverseLerp(1f, 10f, bodyVel.magnitude)) : Vector2.zero;
                    if (!self.player.room.PointSubmerged(swallowtail[i, k].pos))
                    {
                        swallowtail[i, k].vel *= 0.99f;
                        swallowtail[i, k].vel.y -= self.player.room.gravity * 0.6f * Mathf.InverseLerp(outsider.isFlying ? 4f : 8f, 1f, bodyVel.magnitude) * (1f - t);
                    }

                    if (outsider.isFlying && Random.value < 0.1f && outsider.flutterTimeAdd <= outsider.upFlightTime && outsider.flightTime > 0)
                        swallowtail[i, k].vel += Custom.RNV() * 10f * Mathf.InverseLerp(0f, outsider.upFlightTime, outsider.flutterTimeAdd) * Mathf.InverseLerp(outsider.upFlightTime, 0f, outsider.flutterTimeAdd);

                    //让凤尾受空气阻力作用
                    Vector2 airDrag = outsider.isFlying ? -bodyVel * 0.2f : Vector2.zero;
                    swallowtail[i, k].vel += Vector2.Lerp(Vector2.zero, airDrag, outsider.flutterTimeAdd / outsider.upFlightTime);

                    float idealDist = swallowtail[i, k].connectionRad;
                    if (k > 0)
                    {
                        Vector2 normalized = (swallowtail[i, k].pos - swallowtail[i, k - 1].pos).normalized;
                        float dist = Vector2.Distance(swallowtail[i, k].pos, swallowtail[i, k - 1].pos);
                        float distScale = idealDist - dist;
                        //float distScale = Mathf.Sign(idealDist - dist) * Mathf.Pow(Mathf.Abs(idealDist - dist), 2f);
                        float influence = Mathf.Lerp(0.5f, 1f, t);
                        swallowtail[i, k].pos += normalized * distScale * influence * Mathf.InverseLerp(1f, 2f, bodyVel.magnitude);
                        swallowtail[i, k].vel += normalized * distScale * influence * Mathf.InverseLerp(1f, 2f, bodyVel.magnitude);
                        swallowtail[i, k - 1].pos -= normalized * distScale * influence * Mathf.InverseLerp(1f, 2f, bodyVel.magnitude);
                        swallowtail[i, k - 1].vel -= normalized * distScale * influence * Mathf.InverseLerp(1f, 2f, bodyVel.magnitude);
                        if (k > 1)
                        {
                            Vector2 newNormalized = (swallowtail[i, k].pos - swallowtail[i, k - 2].pos).normalized;
                            swallowtail[i, k].vel += newNormalized * influence;
                            swallowtail[i, k - 2].vel -= newNormalized * 0.2f;
                        }/*
                            else // k == 1
                            {
                                swallowtail[i, k].vel += 4f * Custom.DirVec(normalized, dir) * Mathf.InverseLerp(0.1f, 2f, bodyVel.magnitude);
                            }*/
                        if (!outsider.isFlying)
                            swallowtail[i, k].vel += 6f * (1 - t) * Custom.DirVec(normalized, lastNormalized) * Mathf.InverseLerp(0.1f, 2f, bodyVel.magnitude);
                        //lastNormalized = normalized;

                        swallowtail[i, k].vel *= Custom.LerpMap(swallowtail[i, k].vel.magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                        swallowtail[i, k].vel += Custom.RNV() * 0.2f;
                    }
                    //触须根部位置
                    else
                    {/*
                        float num4 = 0f;
                        float width = 8f;
                        Vector2 vector6 = rootPos;
                        Vector2 vector7 = rotation * num4;
                        Vector2 perp = width * Custom.PerpendicularVector(rotation);
                        swallowtail[k + i * tailN].pos = vector6 + vector7 + perp * (i == 0 ? 1f : -1f) * (j == 0 ? 0.5f : 1f);*/
                        swallowtail[i, k].pos = rootPos;
                        swallowtail[i, k].vel *= 0f;
                    }
                    rootPos = swallowtail[i, k].pos;
                    /*
                    num9 *= 0.5f;
                    pos = swallowtail[i, k].pos;*/
                }
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            //凤尾着色
            int fadeLength = self.player.playerState.isPup ? 9 : 15;
            int speckleLength = self.player.playerState.isPup ? 12 : 24;
            for (int i = 0; i < 2; i++)
            {
                var mesh = sLeaser.sprites[swallowtailSprite + i] as TriangleMesh;
                for (int j = 0; j < fadeLength; j++)
                    mesh.verticeColors[j] = Color.Lerp(outsider.GetAntennaeColor(), sLeaser.sprites[0].color, Mathf.Pow(j / (float)(fadeLength - 1), 0.5f));
                for (int j = fadeLength; j < mesh.verticeColors.Length; j++)
                {
                    if (j != speckleLength)
                    {
                        mesh.verticeColors[j] = sLeaser.sprites[0].color;
                    }
                    else
                    {
                        mesh.verticeColors[j] = outsider.GetSpeckleColor();
                    }
                }
            }
        }

        //设置图层
        public void SwallowtailLevel(RoomCamera.SpriteLeaser sLeaser, float bodyRotation)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            //设置图层
            //俯冲
            if (bodyRotation < -1.6f || bodyRotation > 1.6f)
            {
                for (int i = 0; i < 2; i++)
                {
                    //让凤尾移到身体后
                    sLeaser.sprites[swallowtailSprite + i].MoveBehindOtherNode(sLeaser.sprites[0]);
                }
            }
            //侧身
            if (bodyRotation < -0.3f)
            {
                sLeaser.sprites[swallowtailSprite + 1].MoveBehindOtherNode(sLeaser.sprites[outsider.wings.WingSprite(1, 1)]);
            }
            else if (bodyRotation > 0.3f)
            {
                sLeaser.sprites[swallowtailSprite].MoveBehindOtherNode(sLeaser.sprites[outsider.wings.WingSprite(0, 1)]);
            }
            //平飞
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    //让凤尾移到身体后
                    sLeaser.sprites[swallowtailSprite + i].MoveBehindOtherNode(sLeaser.sprites[0]);
                }
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;

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
            SwallowtailLevel(sLeaser, bodyRotation);

            int startSprite = swallowtailSprite;

            //实际显示
            for (int k = 0; k < swallowtail.GetLength(0); k++)
            {
                var dir = Custom.DirVec(self.drawPositions[0, 0], self.drawPositions[1, 0]).normalized;
                var rootPos = Vector2.Lerp(self.owner.bodyChunks[1].pos, self.owner.bodyChunks[0].pos, 0.35f) +
                    (k == 0 ? -1 : 1) * Custom.PerpendicularVector(dir).normalized * nowSwallowTailSpacing + dir; //dir * -0.2f;

                var lastDir = Custom.DirVec(self.drawPositions[0, 1], self.drawPositions[1, 1]).normalized;
                Vector2 lastRootPos = Vector2.Lerp(self.owner.bodyChunks[1].lastPos, self.owner.bodyChunks[0].lastPos, 0.35f) +
                    (k == 0 ? -1 : 1) * Custom.PerpendicularVector(lastDir).normalized * nowSwallowTailSpacing + lastDir;// lastDir * 5f;
                Vector2 nowRootPos = Vector2.Lerp(lastRootPos, rootPos, timeStacker);
                //Vector2 lastSegPos = (nowRootPos * 3f + rootPos) / 4f;
                Vector2 lastSegPos = nowRootPos;

                float lastDist = 6f;

                bool OutLength = false;

                TriangleMesh swallowTail = sLeaser.sprites[startSprite + k] as TriangleMesh;

                for (int i = 0; i < swallowtail.GetLength(1); i++)
                {
                    Vector2 segPos = Vector2.Lerp(swallowtail[k, i].lastPos, swallowtail[k, i].pos, timeStacker);
                    Vector2 normalized = (segPos - lastSegPos).normalized;
                    Vector2 widthDir = Custom.PerpendicularVector(normalized);
                    float dist = Vector2.Distance(segPos, lastSegPos) / 5f;

                    if (i == 0)
                    {
                        dist = 0f;
                    }

                    if (i != 0 && !Custom.DistLess(swallowTail.vertices[i * 4], swallowTail.vertices[i * 4 - 4], 100))
                        OutLength = true;

                    //设置坐标
                    swallowTail.MoveVertice(i * 4 + 0, lastSegPos - widthDir * lastDist * swallowTailWidth + normalized * dist - camPos);
                    swallowTail.MoveVertice(i * 4 + 1, lastSegPos + widthDir * lastDist * swallowTailWidth + normalized * dist - camPos);
                    /*
                    if (i > 0)
                    {
                        swallowTail.MoveVertice(i * 4    , ((swallowTail.vertices[i * 4    ]) + swallowTail.vertices[(i - 1) * 4 + 2]) / 2);
                        swallowTail.MoveVertice(i * 4 + 1, ((swallowTail.vertices[i * 4 + 1]) + swallowTail.vertices[(i - 1) * 4 + 3]) / 2);
                    }*/
                    if (i < swallowtail.GetLength(1) - 1)
                    {
                        swallowTail.MoveVertice(i * 4 + 2, segPos - widthDir * swallowtail[k, i].StretchedRad * swallowTailWidth - normalized * dist - camPos);//swallowTail.MoveVertice(i * 4 + 2, segPos - widthDir * swallowtail[k, i].StretchedRad * swallowTailWidth - normalized * dist - camPos);
                        swallowTail.MoveVertice(i * 4 + 3, segPos + widthDir * swallowtail[k, i].StretchedRad * swallowTailWidth - normalized * dist - camPos);//swallowTail.MoveVertice(i * 4 + 3, segPos + widthDir * swallowtail[k, i].StretchedRad * swallowTailWidth - normalized * dist - camPos); 
                    }
                    else
                    {
                        swallowTail.MoveVertice(i * 4 + 2, segPos - camPos);
                    }
                    lastDist = swallowtail[k, i].StretchedRad;//swallowtail[k, i].StretchedRad;
                    lastSegPos = segPos;
                    /*
                    //防止穿模
                    if (i < self.tail.Length)
                    {
                        swallowtail[k * tailN + i].terrainContact = self.tail[i].terrainContact;
                    }
                    else
                    {
                        swallowtail[k * tailN + i].terrainContact = self.tail[3].terrainContact;
                    }*/

                }

                if ((OutLength && sLeaser.sprites[startSprite + k].isVisible || Plugin.optionsMenuInstance.hideSwallowTail.Value))
                    sLeaser.sprites[startSprite + k].isVisible = false;
                else if (!OutLength && !sLeaser.sprites[startSprite + k].isVisible)
                    sLeaser.sprites[startSprite + k].isVisible = true;
            }
        }
        /*
        private static Vector2 Drift(PlayerGraphics self, TheOutsider player, int i, int k, float timeStacker)
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
            Vector2 wave = player.isFlying ? 0.16f * a * amplitude * player.flutterTimeAdd * Mathf.Sin(0.4f * (i + (k == 0 ? 0 : 3)) + 0.2f * tailTimeAdd + 1.2f) : new Vector2(0, 0);
            Vector2 drift = i / 8f * (a + wave) * d * (k == 0 ? -1f : 1f) + swallowtailH;
            return drift;
        }
        */

        private Vector2 Drift(int i, int k, Vector2 bodyVel)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return Vector2.zero;

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
            float amplitude = (Mathf.Pow(2, 1.5f * i / tailN) - 1);// * Mathf.InverseLerp(1f, 10f, bodyVel.magnitude);
            Vector2 wave = 0.16f * a * amplitude * outsider.flutterTimeAdd * Mathf.Sin(0.4f * (i + (k == 0 ? 0 : 3)) + 0.2f * tailTimeAdd + 1.2f);
            Vector2 drift = (float)i / 8f * (a + wave) * d * (k == 0 ? -1f : 1f);// + swallowtailH;
            return drift;
        }
    }
}
