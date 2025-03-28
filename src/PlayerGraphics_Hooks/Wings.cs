using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class Wings : OutsiderGraphics
    {
        public GenericBodyPart[] wings;
        private int wingSprite;
        private int wingSpriteLength;

        //手臂替代贴图
        private int handWingSprite;

        //翅膀长度及宽度
        private float wingLength;
        private float wingWidth;

        //飞行时，内层翅膀最大/最小旋转角度
        private float innerRotationMin = -70f;
        private float innerRotationMax = 50f;
        //飞行时，外层翅膀最大/最小旋转角度
        private float outerRotationMin = -70f;
        private float outerRotationMax = 50f;

        //内层翅膀和外层翅膀骨架
        private Vector2 innerWing;
        private Vector2 outerWing;

        //内层翅膀旋转角度
        private float innerRotation;
        //外层翅膀旋转角度
        private float outerRotation;
        //身体至臀部方向的向量
        private Vector2 dif;
        //身体旋转角度
        private float bodyRotation;
        //振翅时翅膀旋转角度
        private float flutterRotation;
        //收翅时翅膀旋转角度
        private float foldScale;

        public Wings(PlayerGraphics self, TheOutsider outsider) : base(self, outsider)
        {
            wingSpriteLength = 6;

            if (isPup)
            {
                wingLength = 10f;
                wingWidth = 14f;
            }
            else
            {
                wingLength = 15f;
                wingWidth = 20f;
            }

            wings = new GenericBodyPart[36];
            for (int i = 0; i < wings.Length; i++)
            {
                wings[i] = new GenericBodyPart(self, 1f, 0.5f, 0.9f, self.player.bodyChunks[0]);
            }
        }

        public void Reset()
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            //防止拉丝
            for (int i = 0; i < wings.Length; i++)
            {
                wings[i].Reset((self as GraphicsModule).owner.bodyChunks[0].pos);
            }
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            if (wingSprite >= 1 && sLeaser.sprites.Length >= wingSprite + wingSpriteLength)
            {
                var foregroundContainer = rCam.ReturnFContainer("Foreground");
                var midgroundContainer = newContatiner != null ? newContatiner : rCam.ReturnFContainer("Midground");

                //让翅膀移到中景
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var sprite = sLeaser.sprites[WingSprite(i, j)];
                        foregroundContainer.RemoveChild(sprite);
                        midgroundContainer.AddChild(sprite);
                    }
                    //翼手
                    if (Plugin.optionsMenuInstance.handWing.Value)
                    {
                        var sprite = sLeaser.sprites[handWingSprite + i];
                        foregroundContainer.RemoveChild(sprite);
                        midgroundContainer.AddChild(sprite);
                    }
                }
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            wingSprite = sLeaser.sprites.Length;
            Array.Resize(ref sLeaser.sprites, wingSprite + wingSpriteLength);

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

                        triangleMesh.UVvertices[0] = Futile.atlasManager.GetElementWithName("MothWing" + "A" + j).uvTopLeft;
                        triangleMesh.UVvertices[2] = Futile.atlasManager.GetElementWithName("MothWing" + "A" + j).uvTopRight;
                        triangleMesh.UVvertices[3] = Futile.atlasManager.GetElementWithName("MothWing" + "A" + j).uvBottomRight;
                        triangleMesh.UVvertices[5] = Futile.atlasManager.GetElementWithName("MothWing" + "A" + j).uvBottomLeft;
                        triangleMesh.UVvertices[1] = Vector2.Lerp(triangleMesh.UVvertices[0], triangleMesh.UVvertices[2], 0.5f);
                        triangleMesh.UVvertices[4] = Vector2.Lerp(triangleMesh.UVvertices[3], triangleMesh.UVvertices[5], 0.5f);

                        sLeaser.sprites[WingSprite(i, j)] = triangleMesh;

                        if (j == 2)
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

                        triangleMesh.UVvertices[0] = Futile.atlasManager.GetElementWithName("MothWing" + "A" + j).uvTopLeft;
                        triangleMesh.UVvertices[2] = Futile.atlasManager.GetElementWithName("MothWing" + "A" + j).uvTopRight;
                        triangleMesh.UVvertices[3] = Futile.atlasManager.GetElementWithName("MothWing" + "A" + j).uvBottomRight;
                        triangleMesh.UVvertices[5] = Futile.atlasManager.GetElementWithName("MothWing" + "A" + j).uvBottomLeft;
                        triangleMesh.UVvertices[1] = Vector2.Lerp(triangleMesh.UVvertices[0], triangleMesh.UVvertices[2], 0.5f);
                        triangleMesh.UVvertices[4] = Vector2.Lerp(triangleMesh.UVvertices[3], triangleMesh.UVvertices[5], 0.5f);

                        triangleMesh.alpha = 0.8f;

                        sLeaser.sprites[WingSprite(i, j)] = triangleMesh;
                    }
                }
            }


            //翼手
            if (Plugin.optionsMenuInstance.handWing.Value)
            {
                handWingSprite = sLeaser.sprites.Length;
                Array.Resize(ref sLeaser.sprites, handWingSprite + 2);
                //0 为左手， 1 为右手（与翅膀相反）
                for (int i = 0; i < 2; i++)
                {
                    var hand = sLeaser.sprites[6 - i];
                    var handWing = sLeaser.sprites[handWingSprite + i];
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

                    sLeaser.sprites[handWingSprite + i] = triangleMesh;
                    //sLeaser.sprites[handWingSprite + i].anchorX = 0.9f;
                }
            }
        }

        public void Update()
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            Vector2 drawPos1;
            //身体位置
            Vector2 bodyPos = self.player.bodyChunks[0].lastPos;
            //头部位置
            Vector2 headPos = self.head.lastPos;

            if (outsider.isFlying || outsider.spreadWings)
            {
                drawPos1 = bodyPos;
            }
            else
            {
                drawPos1 = Vector2.Lerp(headPos, bodyPos, 0.3f);
            }
            //臀部位置
            Vector2 drawPos2 = self.player.bodyChunks[1].lastPos;  //身体至臀部方向的向量
            dif = wingWidth * (drawPos1 - drawPos2).normalized;
            //身体旋转角度
            bodyRotation = Mathf.Atan2(dif.x, dif.y);

            //飞行展开翅膀
            if (outsider.isFlying)
            {
                innerRotationMin = Custom.LerpMap(self.owner.EffectiveRoomGravity, 0.9f, 0.5f, -70f, -20f);
                innerRotationMax = Custom.LerpMap(self.owner.EffectiveRoomGravity, 0.9f, 0.5f, 50f, 20f);
                outerRotationMin = Custom.LerpMap(self.owner.EffectiveRoomGravity, 0.9f, 0.5f, -70f, -20f);
                outerRotationMax = Custom.LerpMap(self.owner.EffectiveRoomGravity, 0.9f, 0.5f, 50f, 20f);
                if (outsider.flightTime == 0)
                {
                    outsider.flutterTimeAdd = 0f;
                }
                //无重力时扑腾翅膀
                else if (self.owner.EffectiveRoomGravity <= 0.5 && self.player.Consious && !self.player.Stunned)
                {
                    outsider.flutterTimeAdd += 1f;
                    if (outsider.flutterTimeAdd >= outsider.upFlightTime)
                    {
                        outsider.flutterTimeAdd = 0f;
                    }
                }
                else if (outsider.flutterTimeAdd <= outsider.upFlightTime)
                {
                    outsider.flutterTimeAdd += 1f;
                }
            }
            //不是飞行状态
            else
            {
                //站在杆子上时展开翅膀
                if ((self.player.animation == Player.AnimationIndex.HangFromBeam || self.player.animation == Player.AnimationIndex.StandOnBeam) &&
                    self.player.input[0].x != 0 && self.player.Consious && !self.player.Stunned)
                {
                    outsider.flutterTimeAdd += 1f;
                    outsider.flightTime += 1;
                    if (outsider.flutterTimeAdd >= 2 * outsider.upFlightTime)
                    {
                        outsider.flutterTimeAdd = outsider.upFlightTime;
                    }
                    if (outsider.flightTime >= outsider.upFlightTime)
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
                    outsider.spreadWings = true;
                    outsider.foldUpWings = false;
                }
                //飞行结束后收起翅膀
                else if (outsider.flutterTimeAdd > 0f)// && self.canJump > 0
                {
                    if (outsider.flutterTimeAdd > outsider.upFlightTime)
                    {
                        outsider.flutterTimeAdd = outsider.upFlightTime;
                    }
                    outsider.flutterTimeAdd -= 1f;
                    outsider.spreadWings = false;
                    outsider.foldUpWings = true;
                }
                else
                {
                    outsider.flightTime = 0;
                    outsider.flutterTimeAdd = 0f;
                    outsider.spreadWings = false;
                    outsider.foldUpWings = false;
                }
            }

            //飞行动画计算
            if (!outsider.isFlying && !outsider.spreadWings)
            {
                outerRotation = Mathf.Lerp(outerRotation, outerRotation + bodyRotation / 3, Mathf.Abs((1 - Mathf.Cos(bodyRotation))));
            }
            for (int i = 0; i < 2; i++)
            {
                WingPos(dif, bodyRotation, innerRotation, outerRotation, i);
            }

            //翼手
            if (Plugin.optionsMenuInstance.handWing.Value && outsider.flutterTimeAdd > 0)
            {
                //0 为左手， 1 为右手（与翅膀相反）
                self.hands[1].pos = Vector2.Lerp(Vector2.Lerp(wings[18 * 0 + 1].pos, wings[18 * 0 + 2].pos, 0.6f), Vector2.Lerp(wings[18 * 0 + 4].pos, wings[18 * 0 + 3].pos, 0.6f), 0.5f);
                self.hands[0].pos = Vector2.Lerp(Vector2.Lerp(wings[18 * 1 + 1].pos, wings[18 * 1 + 2].pos, 0.6f), Vector2.Lerp(wings[18 * 1 + 4].pos, wings[18 * 1 + 3].pos, 0.6f), 0.5f); ;
                self.hands[1].mode = Limb.Mode.HuntRelativePosition;
                self.hands[0].mode = Limb.Mode.HuntRelativePosition;
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            Vector2 drawPos1;

            //身体位置
            Vector2 bodyPos = Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker);
            //头部位置
            Vector2 headPos = Vector2.Lerp(self.head.lastPos, self.head.pos, timeStacker);

            if (outsider.isFlying || outsider.spreadWings)
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
            dif = wingWidth * (drawPos1 - drawPos2).normalized;
            //身体旋转角度
            bodyRotation = Mathf.Atan2(dif.x, dif.y);

            //抖动修正
            Vector2 shake = sLeaser.sprites[0].GetPosition() - bodyPos + camPos;

            //设置图层
            WingLevel(sLeaser, bodyRotation);
            if (!outsider.isFlying)
            {
                sLeaser.sprites[WingSprite(0, 2)].isVisible = false;
                sLeaser.sprites[WingSprite(1, 2)].isVisible = false;
            }

            //i = 0为右侧翅膀， i = 1 为左侧翅膀
            for (int i = 0; i < 2; i++)
            {
                //内层翅膀渐变颜色
                sLeaser.sprites[WingSprite(i, 2)].color = outsider.GetSpeckleColor();
                //外层翅膀颜色与不透明度
                sLeaser.sprites[WingSprite(i, 1)].color = outsider.GetLepidoticWingColor();
                sLeaser.sprites[WingSprite(i, 1)].alpha = 0.8f;


                //设置位置
                for (int j = 0; j < 3; j++)
                {
                    var wingMesh = sLeaser.sprites[WingSprite(i, j)] as TriangleMesh;
                    for (int k = 0; k < 6; k++)
                    {
                        wingMesh.MoveVertice(k, shake + Vector2.Lerp(wings[i * 18 + j * 6 + k].lastPos, wings[i * 18 + j * 6 + k].pos, timeStacker) - camPos);
                    }
                }
                //飞行时抬一下腿
                if (outsider.isFlying)
                {
                    sLeaser.sprites[4].SetPosition(sLeaser.sprites[4].GetPosition().x, sLeaser.sprites[4].GetPosition().y + 4f * Mathf.Abs(Mathf.Sin(bodyRotation)));
                }

                //翼手
                if (Plugin.optionsMenuInstance.handWing.Value)
                {
                    //0 为左手， 1 为右手（与翅膀相反）
                    var hand = sLeaser.sprites[6 - i];
                    var handWing = sLeaser.sprites[handWingSprite + i] as TriangleMesh;
                    var innerWing = sLeaser.sprites[WingSprite(i, 0)] as TriangleMesh;
                    var outerWing = sLeaser.sprites[WingSprite(i, 1)] as TriangleMesh;
                    handWing.color = hand.color;
                    if (outsider.isFlying)
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
                    else if (outsider.flightTime == -1)
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
                        sLeaser.sprites[WingSprite(i, j)].isVisible = false;
            }
        }

        //设置骨架
        private void WingAnimation(Vector2 dif, float bodyRotation, float innerRotation, float outerRotation, int i)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;

            //翅膀骨架向量
            innerWing = 0.8f * wingLength * new Vector2(Mathf.Cos(innerRotation - bodyRotation), Mathf.Sin(innerRotation - bodyRotation));
            outerWing = 1.0f * wingLength * new Vector2(Mathf.Cos(outerRotation - bodyRotation), Mathf.Sin(outerRotation - bodyRotation));

            //左和右决定是否镜像
            innerWing = i == 0 ? innerWing : 2 * Vector2.Dot(innerWing, (dif).normalized) * (dif).normalized - innerWing;
            outerWing = i == 0 ? outerWing : 2 * Vector2.Dot(outerWing, (dif).normalized) * (dif).normalized - outerWing;

            //细节修正
            innerRotation = Mathf.Atan2(innerWing.x, innerWing.y);
            float drift = Mathf.Lerp(0, innerRotation, 2 * Mathf.Abs(Mathf.Sin(bodyRotation)));
            float driftScale = ((i - 0.5f) * bodyRotation <= 0) ? Mathf.Lerp(1f, 0f, Mathf.Cos(drift)) : 0f;
            //侧身飞行
            if (outsider.isFlying)
            {
                float rotation = 0f;
                if (Mathf.Abs(bodyRotation) <= 90f / 180f * 3.1415926f)
                    rotation = -1.5f * bodyRotation;
                else
                    rotation = 1.5f * bodyRotation - 3f * 90f / 180f * 3.1415926f;
                //透视
                innerWing = PlayerGraphicsHooks.VectorRotation(innerWing, Mathf.Lerp(0f, rotation, driftScale));
                outerWing = PlayerGraphicsHooks.VectorRotation(outerWing, Mathf.Lerp(0f, rotation, driftScale));
                //随着身体旋转而向身后折拢
                innerWing = PlayerGraphicsHooks.VectorRotation(innerWing, Mathf.Abs(bodyRotation) / 2 * (i == 0 ? 1f : -1f));
                outerWing = PlayerGraphicsHooks.VectorRotation(outerWing, Mathf.Abs(bodyRotation) / 2 * (i == 0 ? 1f : -1f));
                //减少内外层翅膀的角度差距
                outerWing = Vector2.Lerp(outerWing, innerWing, Mathf.Abs(Mathf.Sin(bodyRotation)));
            }
            //侧身爬行
            else
            {
                //翅膀镜像
                innerWing = Vector2.Lerp(innerWing, 2 * Vector2.Dot(innerWing, (dif).normalized) * (dif).normalized - innerWing, driftScale);
                outerWing = Vector2.Lerp(outerWing, 2 * Vector2.Dot(outerWing, (dif).normalized) * (dif).normalized - outerWing, driftScale);
            }

            //俯冲
            if (Mathf.Abs(bodyRotation) > 90f / 180f * 3.1415926f)
            {
                //调整翅膀大小
                innerWing = Vector2.Lerp(0.5f * innerWing, innerWing, Mathf.Abs(Mathf.Sin(bodyRotation * 2 - 3.1415926f / 2)));
                outerWing = Vector2.Lerp(0.5f * outerWing, outerWing, Mathf.Abs(Mathf.Sin(bodyRotation * 2 - 3.1415926f / 2)));
            }

            //Debug.Log("inner: " + innerWing);
            //Debug.Log("outer: " + outerWing);
        }

        //设置翅膀位置
        private void WingPos(Vector2 dif, float bodyRotation, float innerRotation, float outerRotation, int i)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            for (int j = 0; j < 18; j++)
            {
                wings[i * 18 + j].lastPos = wings[i * 18 + j].pos;
                wings[i * 18 + j].Update();
            }

            //垂直修正
            float wingH = Mathf.Lerp(-3f, 3f, Mathf.Abs(Mathf.Sin(bodyRotation)));

            //幼崽翅膀移动一些
            if (isPup)
            {
                wingH += Mathf.Lerp(3f, -1f, Mathf.Abs(Mathf.Sin(bodyRotation)));
            }

            //水平修正
            float wingW;
            if (outsider.isFlying)
                wingW = 0f;
            else if (self.player.input[0].x > 0)
                wingW = Mathf.Lerp(0f, isPup ? 8f : 5f, Mathf.Abs(Mathf.Sin(bodyRotation * 2f)));
            else if (self.player.input[0].x < 0)
                wingW = -Mathf.Lerp(0f, isPup ? 8f : 5f, Mathf.Abs(Mathf.Sin(bodyRotation * 2f)));
            else
                wingW = 0;

            //飞行时
            if (outsider.isFlying)
            {
                //时间插值
                float t = outsider.flutterTimeAdd / outsider.upFlightTime * 3.1415927f;
                //内层翅膀旋转角度
                flutterRotation = Mathf.Cos(t + 0.5f);
                flutterRotation = Mathf.Abs(flutterRotation);
                innerRotation = 3.1415927f / 180f * Mathf.Lerp(innerRotationMin, innerRotationMax, flutterRotation);
                //外层翅膀旋转角度（滞后于内层翅膀）
                flutterRotation = Mathf.Cos(t + 0.2f);
                flutterRotation = Mathf.Abs(flutterRotation);
                outerRotation = 3.1415927f / 180f * Mathf.Lerp(outerRotationMin, outerRotationMax, flutterRotation);
                //设置骨架
                WingAnimation(dif, bodyRotation, innerRotation, outerRotation, i);

                //设置位置
                for (int j = 0; j < 3; j++)
                {
                    int startNum = i * 18 + j * 6;
                    //内层翅膀
                    if (j != 1)
                    {
                        wings[startNum + 0].pos = self.player.bodyChunks[0].pos + wingH * Vector2.up;
                        wings[startNum + 5].pos = wings[startNum + 0].pos - dif + wingH * Vector2.up;
                        wings[startNum + 1].pos = wings[startNum + 0].pos + 1.5f * innerWing + Custom.RNV() * Random.value * 1f;
                        wings[startNum + 2].pos = wings[startNum + 1].pos + 0.5f * outerWing + Custom.RNV() * Random.value * 1f;
                        wings[startNum + 4].pos = wings[startNum + 5].pos + outerWing;
                        wings[startNum + 3].pos = wings[startNum + 4].pos + 1.5f * innerWing;
                    }
                    //外层翅膀
                    else
                    {
                        wings[startNum + 0].pos = wings[i * 18 + 0 * 6 + 1].pos;
                        wings[startNum + 5].pos = wings[i * 18 + 0 * 6 + 5].pos;
                        wings[startNum + 2].pos = wings[startNum + 0].pos + 2.0f * outerWing;
                        wings[startNum + 1].pos = Vector2.Lerp(wings[startNum + 1].pos, wings[startNum + 2].pos, 0.5f);
                        wings[startNum + 4].pos = wings[startNum + 5].pos + 1.0f * innerWing;
                        wings[startNum + 3].pos = wings[startNum + 4].pos + 2.0f * innerWing;
                    }
                }
            }
            //不飞行时
            else
            {
                float t;
                //时间插值
                if (outsider.foldUpWings || outsider.spreadWings)
                {
                    t = outsider.flutterTimeAdd / outsider.upFlightTime * 3.1415927f;
                }
                else
                {
                    t = 0;
                }
                t = outsider.upFlightTime - t;
                foldScale = Mathf.Clamp01((outsider.upFlightTime - outsider.flutterTimeAdd) / outsider.upFlightTime);

                //内层翅膀旋转角度
                flutterRotation = Mathf.Cos(t + 0.5f + outsider.upFlightTime);
                flutterRotation = Mathf.Abs(flutterRotation);
                innerRotation = 3.1415927f / 180f * Mathf.Lerp(Mathf.Lerp(innerRotationMin, 0, foldScale), Mathf.Lerp(innerRotationMax, innerRotationMax - 20, foldScale), flutterRotation);

                //外层翅膀旋转角度（滞后于内层翅膀）
                flutterRotation = Mathf.Cos(t + 0.2f + outsider.upFlightTime);
                flutterRotation = Mathf.Abs(flutterRotation);
                outerRotation = 3.1415927f / 180f * Mathf.Lerp(Mathf.Lerp(outerRotationMin, outerRotationMin - 20, foldScale), Mathf.Lerp(outerRotationMax, outerRotationMax - 150, foldScale), flutterRotation);

                //长度修正
                float foldLength = Mathf.Lerp(1, 2, foldScale);

                //沿身体方向位置修正
                float difScale = Mathf.Lerp(0f, 0.6f, Mathf.Abs(1 - Mathf.Cos(bodyRotation)));

                //设置骨架
                WingAnimation(dif, bodyRotation, innerRotation, outerRotation, i);

                //设置位置
                for (int j = 0; j < 3; j++)
                {
                    int startNum = i * 18 + j * 6;
                    float width = 5f * (foldLength - 1) * (i == 0 ? 1 : -1);
                    //内层翅膀
                    if (j != 1)
                    {

                        wings[startNum + 0].pos = self.player.bodyChunks[0].pos - wingH * Vector2.up * (isPup ? (0.1f - foldScale) : (1f - foldScale)) + wingW * Vector2.left * foldScale - 0.8f * difScale * dif;
                        wings[startNum + 5].pos = wings[startNum + 0].pos - dif - wingH * Vector2.up * (isPup ? (0.1f - foldScale) : (1f - foldScale)) + wingW * Vector2.left * foldScale - 0.2f * difScale * dif;
                        wings[startNum + 1].pos = wings[startNum + 0].pos + 1.5f / foldLength * innerWing + width * Vector2.right * 0.5f;
                        wings[startNum + 2].pos = wings[startNum + 1].pos + 0.5f * foldLength * outerWing + width * Vector2.right * 0.5f;
                        wings[startNum + 4].pos = wings[startNum + 5].pos + 1.0f / foldLength * outerWing + width * Vector2.right * 0.5f;
                        wings[startNum + 3].pos = wings[startNum + 4].pos + 1.5f / foldLength * innerWing + width * Vector2.right;
                        //爬行姿态调整
                        wings[startNum + 2].pos = Vector2.Lerp(wings[startNum + 2].pos, wings[startNum + 0].pos, 0.5f * Mathf.Abs((1 - Mathf.Cos(bodyRotation))) / 2);
                        wings[startNum + 3].pos = Vector2.Lerp(wings[startNum + 3].pos, wings[startNum + 2].pos, 0.75f * Mathf.Abs((1 - Mathf.Cos(bodyRotation))) / 2);
                    }
                    //外层翅膀
                    else
                    {
                        wings[startNum + 0].pos = wings[i * 18 + 0 * 6 + 1].pos;
                        wings[startNum + 5].pos = wings[i * 18 + 0 * 6 + 5].pos - 0.5f * (foldLength - 1) * dif;
                        wings[startNum + 2].pos = wings[startNum + 0].pos + 2.0f * outerWing + width * Vector2.right * 0.5f;
                        wings[startNum + 1].pos = Vector2.Lerp(Vector2.Lerp(wings[startNum + 1].pos, wings[startNum + 2].pos, 0.5f), wings[i * 18 + 0 * 6 + 2].pos, foldScale);
                        wings[startNum + 4].pos = wings[startNum + 5].pos + 1.0f / (3.5f * foldLength) * innerWing + width * Vector2.right * 0.3f;
                        wings[startNum + 3].pos = wings[startNum + 4].pos + 2.0f / (1.5f * foldLength) * innerWing + width * Vector2.right * 0.5f;
                        //爬行姿态调整
                        wings[startNum + 2].pos = Vector2.Lerp(wings[startNum + 2].pos, wings[startNum + 0].pos, 0.5f * Mathf.Abs((1 - Mathf.Cos(bodyRotation))) / 2);
                        wings[startNum + 3].pos = Vector2.Lerp(wings[startNum + 3].pos, wings[startNum + 5].pos, 0.5f * Mathf.Abs((1 - Mathf.Cos(bodyRotation))) / 2);
                    }
                }
            }
        }

        //设置图层
        private void WingLevel(RoomCamera.SpriteLeaser sLeaser, float bodyRotation)
        {
            if (!playerRef.TryGetTarget(out PlayerGraphics self) || !outsiderRef.TryGetTarget(out TheOutsider outsider))
                return;
            if (Plugin.optionsMenuInstance.hideWingWhenFolded.Value)
            {
                if (!(outsider.isFlying || outsider.foldUpWings || outsider.spreadWings))
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 3; j++)
                            sLeaser.sprites[WingSprite(i, j)].isVisible = false;
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                        for (int j = 0; j < 3; j++)
                            sLeaser.sprites[WingSprite(i, j)].isVisible = true;
                }
            }
            //设置图层
            //俯冲
            if (outsider.isFlying && bodyRotation < -1.8f || bodyRotation > 1.8f)
            {
                sLeaser.sprites[WingSprite(1, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[WingSprite(1, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(1, 1)]);
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[WingSprite(i, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                    sLeaser.sprites[WingSprite(i, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(i, 1)]);
                    sLeaser.sprites[WingSprite(i, 2)].isVisible = false;
                }
            }
            //侧飞
            else if (outsider.isFlying && (bodyRotation < -0.5f || (bodyRotation < -0.3f && outsider.flutterTimeAdd <= outsider.upFlightTime)))
            {
                sLeaser.sprites[WingSprite(0, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[WingSprite(0, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(0, 1)]);
                sLeaser.sprites[WingSprite(0, 2)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(0, 0)]);
                sLeaser.sprites[WingSprite(0, 2)].isVisible = true;
            }
            else if (outsider.isFlying && (bodyRotation > 0.5f || (bodyRotation > 0.3f && outsider.flutterTimeAdd <= outsider.upFlightTime)))
            {
                sLeaser.sprites[WingSprite(1, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[WingSprite(1, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(1, 1)]);
                sLeaser.sprites[WingSprite(1, 2)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(1, 0)]);
                sLeaser.sprites[WingSprite(1, 2)].isVisible = true;
            }
            //倒立
            else if (!outsider.isFlying && bodyRotation < -2.35f || bodyRotation > 2.35f)
            {
                sLeaser.sprites[WingSprite(1, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[WingSprite(1, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(1, 1)]);
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[WingSprite(i, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                    sLeaser.sprites[WingSprite(i, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(i, 1)]);
                    sLeaser.sprites[WingSprite(i, 2)].isVisible = false;
                }
            }
            //侧身
            else if (!outsider.isFlying && self.player.bodyMode != Player.BodyModeIndex.WallClimb && bodyRotation < -0.12f)
            {
                sLeaser.sprites[WingSprite(0, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[WingSprite(0, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(0, 1)]);
                sLeaser.sprites[WingSprite(0, 2)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(0, 0)]);
                sLeaser.sprites[WingSprite(0, 2)].isVisible = true;
            }
            else if (!outsider.isFlying && self.player.bodyMode != Player.BodyModeIndex.WallClimb && bodyRotation > 0.12f)
            {
                sLeaser.sprites[WingSprite(1, 1)].MoveInFrontOfOtherNode(sLeaser.sprites[9]);
                sLeaser.sprites[WingSprite(1, 0)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(1, 1)]);
                sLeaser.sprites[WingSprite(1, 2)].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(1, 0)]);
                sLeaser.sprites[WingSprite(1, 2)].isVisible = true;
            }
            //平飞、正立、爬墙
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    //让翅膀移到身体后
                    for (int j = 0; j < 3; j++)
                    {
                        sLeaser.sprites[WingSprite(i, j)].MoveBehindOtherNode(sLeaser.sprites[0]);
                    }
                    //让外层翅膀移到内层翅膀后
                    sLeaser.sprites[WingSprite(i, 1)].MoveBehindOtherNode(sLeaser.sprites[WingSprite(i, 0)]);
                    sLeaser.sprites[WingSprite(i, 2)].isVisible = true;
                }
            }
            //翼手
            if (Plugin.optionsMenuInstance.handWing.Value)
            {
                //让手移到翅膀前
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[handWingSprite + i].MoveInFrontOfOtherNode(sLeaser.sprites[WingSprite(i, 2)]);
                }
            }
        }

        public int WingSprite(int side, int wings)
        {
            return wingSprite + side + wings + wings;
        }
    }
}
