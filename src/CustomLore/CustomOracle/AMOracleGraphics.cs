using CustomOracleTx;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheOutsider.CustomLore.CustomOracle
{
    public class AMOracleGraphics : CustomOracleGraphic
    {
        public GownCover[] cover;
        public int coverSprite;

        public GownRibbon[] ribbon;
        public int ribbonSprite;

        public int halo_firstSprite;
        public int halo_lastSprite;

        public AMOracleGraphics(PhysicalObject ow) : base(ow)
        {
            callBaseApplyPalette = false;
            callBaseInitiateSprites = false;

            Random.State state = Random.state;
            Random.InitState(42);
            totalSprites = 0;
            armJointGraphics = new ArmJointGraphics[oracle.arm.joints.Length];

            for (int i = 0; i < oracle.arm.joints.Length; i++)
            {
                armJointGraphics[i] = new ArmJointGraphics(this, oracle.arm.joints[i], totalSprites);
                totalSprites += armJointGraphics[i].totalSprites;
            }


            firstUmbilicalSprite = totalSprites;
            umbCord = new UbilicalCord(this, totalSprites);
            totalSprites += umbCord.totalSprites;


            firstBodyChunkSprite = totalSprites;
            totalSprites += 2;
            neckSprite = totalSprites;
            totalSprites++;
            firstFootSprite = totalSprites;
            totalSprites += 4;

            //光环
            halo = new Halo(this, totalSprites);
            totalSprites += halo.totalSprites;

            //袍子
            gowns = new OracleGraphics.Gown[1];
            gowns[0] = new Gown(this, 0);
            robeSprite = totalSprites;
            totalSprites++;


            firstHandSprite = totalSprites;
            totalSprites += 4;
            head = new GenericBodyPart(this, 5f, 0.5f, 0.995f, oracle.firstChunk);
            firstHeadSprite = totalSprites;
            totalSprites += 10;
            fadeSprite = totalSprites;
            totalSprites++;

            killSprite = totalSprites;
            totalSprites++;

            hands = new GenericBodyPart[2];

            for (int j = 0; j < 2; j++)
            {
                hands[j] = new GenericBodyPart(this, 2f, 0.5f, 0.98f, oracle.firstChunk);
            }
            feet = new GenericBodyPart[2];
            for (int k = 0; k < 2; k++)
            {
                feet[k] = new GenericBodyPart(this, 2f, 0.5f, 0.98f, oracle.firstChunk);
            }
            knees = new Vector2[2, 2];
            for (int l = 0; l < 2; l++)
            {
                for (int m = 0; m < 2; m++)
                {
                    knees[l, m] = oracle.firstChunk.pos;
                }
            }
            firstArmBaseSprite = totalSprites;
            armBase = new ArmBase(this, firstArmBaseSprite);
            totalSprites += armBase.totalSprites;

            //披肩
            cover = new GownCover[2];
            for (int i = 0; i < 2; i++)
                cover[i] = new GownCover(this);
            coverSprite = totalSprites;
            totalSprites += 2;

            //飘带
            ribbon = new GownRibbon[2];
            for (int i = 0; i < 2; i++)
                ribbon[i] = new GownRibbon(this);
            ribbonSprite = totalSprites;
            totalSprites += 2;

            voiceFreqSamples = new float[64];
            Random.state = state;
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            /*
            SLArmBaseColA = new Color(0.52156866f, 0.52156866f, 0.5137255f);
            SLArmHighLightColA = new Color(0.5686275f, 0.5686275f, 0.54901963f);
            SLArmBaseColB = palette.texture.GetPixel(5, 1);
            SLArmHighLightColB = palette.texture.GetPixel(5, 2);
            */
            for (int i = 0; i < armJointGraphics.Length; i++)
            {
                armJointGraphics[i].ApplyPalette(sLeaser, rCam, palette);
                armJointGraphics[i].metalColor = palette.blackColor;
            }
            Color color = AMOracleColor.GrayishYellow;

            for (int j = 0; j < base.owner.bodyChunks.Length; j++)
            {
                sLeaser.sprites[firstBodyChunkSprite + j].color = color;
            }
            sLeaser.sprites[neckSprite].color = color;
            sLeaser.sprites[HeadSprite].color = color;
            sLeaser.sprites[ChinSprite].color = color;

            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[EyeSprite(k)].color = AMOracleColor.DarkBlue;
            }

            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[PhoneSprite(k, 0)].color = AMOracleColor.VeryDarkGrey;
                sLeaser.sprites[PhoneSprite(k, 1)].color = AMOracleColor.VeryDarkGrey;
                sLeaser.sprites[PhoneSprite(k, 2)].color = AMOracleColor.VeryDarkGrey;


                sLeaser.sprites[HandSprite(k, 0)].color = color;
                if (gowns != null)
                {
                    for (int l = 0; l < 4; l++)
                    {
                        Color handColor = Gown_Color(gowns[0], (float)l / 7f);
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 1] = AMOracleColor.Blue;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 2] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 3] = AMOracleColor.Blue;
                    }
                    for (int l = 4; l < 7; l++)
                    {
                        Color handColor = Gown_Color(gowns[0], (float)l / 7f);
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 1] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 2] = handColor;
                        (sLeaser.sprites[HandSprite(k, 1)] as TriangleMesh).verticeColors[l * 4 + 3] = handColor;
                    }
                }
                else
                {
                    sLeaser.sprites[HandSprite(k, 1)].color = color;
                }
                sLeaser.sprites[FootSprite(k, 0)].color = color;
                sLeaser.sprites[FootSprite(k, 1)].color = color;
            }
            if (umbCord != null)
            {
                umbCord.ApplyPalette(sLeaser, rCam, palette);
                sLeaser.sprites[firstUmbilicalSprite].color = palette.blackColor;
            }
            else if (discUmbCord != null)
            {
                discUmbCord.ApplyPalette(sLeaser, rCam, palette);
            }
            if (armBase != null)
            {
                armBase.ApplyPalette(sLeaser, rCam, palette);
            }
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[totalSprites];

            for (int i = 0; i < base.owner.bodyChunks.Length; i++)
            {
                sLeaser.sprites[firstBodyChunkSprite + i] = new FSprite("Circle20", true);
                sLeaser.sprites[firstBodyChunkSprite + i].scale = base.owner.bodyChunks[i].rad / 10f;
                sLeaser.sprites[firstBodyChunkSprite + i].color = new Color(1f, (i == 0) ? 0.5f : 0f, (i == 0) ? 0.5f : 0f);
            }

            for (int j = 0; j < armJointGraphics.Length; j++)
            {
                armJointGraphics[j].InitiateSprites(sLeaser, rCam);
            }

            if (gowns != null)
            {
                for (int j = 0; j < this.gowns.Length; j++)
                    gowns[j].InitiateSprite(robeSprite, sLeaser, rCam);
            }

            if (halo != null)
            {
                halo.InitiateSprites(sLeaser, rCam);
            }

            if (armBase != null)
            {
                armBase.InitiateSprites(sLeaser, rCam);
            }
            sLeaser.sprites[neckSprite] = new FSprite("pixel", true);
            sLeaser.sprites[neckSprite].scaleX = 3f;
            sLeaser.sprites[neckSprite].anchorY = 0f;
            sLeaser.sprites[HeadSprite] = new FSprite("Circle20", true);
            sLeaser.sprites[ChinSprite] = new FSprite("Circle20", true);
            for (int k = 0; k < 2; k++)
            {
                sLeaser.sprites[EyeSprite(k)] = new FSprite("pixel", true);

                sLeaser.sprites[PhoneSprite(k, 0)] = new FSprite("Circle20", true);
                sLeaser.sprites[PhoneSprite(k, 1)] = new FSprite("Circle20", true);
                sLeaser.sprites[PhoneSprite(k, 2)] = new FSprite("LizardScaleA1", true);
                sLeaser.sprites[PhoneSprite(k, 2)].anchorY = 0f;
                sLeaser.sprites[PhoneSprite(k, 2)].scaleY = 1.2f;//耳机长度
                sLeaser.sprites[PhoneSprite(k, 2)].scaleX = ((k == 0) ? -1f : 1f) * 0.75f;

                sLeaser.sprites[HandSprite(k, 0)] = new FSprite("haloGlyph-1", true);
                sLeaser.sprites[HandSprite(k, 1)] = TriangleMesh.MakeLongMesh(7, false, true);
                sLeaser.sprites[FootSprite(k, 0)] = new FSprite("haloGlyph-1", true);
                sLeaser.sprites[FootSprite(k, 1)] = TriangleMesh.MakeLongMesh(7, false, true);
            }

            if (umbCord != null)
            {
                umbCord.InitiateSprites(sLeaser, rCam);
            }
            else if (discUmbCord != null)
            {
                discUmbCord.InitiateSprites(sLeaser, rCam);
            }

            sLeaser.sprites[HeadSprite].scaleX = head.rad / 9f;
            sLeaser.sprites[HeadSprite].scaleY = head.rad / 11f;
            sLeaser.sprites[ChinSprite].scale = head.rad / 15f;
            //光晕
            sLeaser.sprites[fadeSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[fadeSprite].scale = 12.5f;
            sLeaser.sprites[fadeSprite].color = AMOracleColor.Blue;
            sLeaser.sprites[fadeSprite].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
            sLeaser.sprites[fadeSprite].alpha = 0.5f;

            sLeaser.sprites[killSprite] = new FSprite("Futile_White", true);
            sLeaser.sprites[killSprite].shader = rCam.game.rainWorld.Shaders["FlatLight"];

            for (int i = 0; i < 2; i++)
                cover[i].InitiateSprites(coverSprite + i, sLeaser, rCam);
            for (int i = 0; i < 2; i++)
                ribbon[i].InitiateSprites(ribbonSprite + i, sLeaser, rCam);

            base.InitiateSprites(sLeaser, rCam);

            for (int i = 0; i < 2; i++)
            {
                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[coverSprite + i]);
                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[ribbonSprite + i]);
                sLeaser.sprites[ribbonSprite + i].MoveBehindOtherNode(sLeaser.sprites[robeSprite]);
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            for (int i = 0; i < 2; i++)
                cover[i].DrawSprites(i, robeSprite, coverSprite + i, sLeaser, rCam, timeStacker, camPos);
            for (int i = 0; i < 2; i++)
                ribbon[i].DrawSprites(i, ribbonSprite + i, sLeaser, rCam, timeStacker, camPos);
            /*
            for(int i = robeSprite; i < robeSprite + 1; i++)
            {
                Plugin.Log(i + " : " + sLeaser.sprites[i].color + "   " + sLeaser.sprites[i].isVisible);
                sLeaser.sprites[i].isVisible = false;
            }*/
        }

        public override void Update()
        {
            base.Update();
            for (int i = 0; i < 2; i++)
                cover[i].Update();
            for (int i = 0; i < 2; i++)
                ribbon[i].Update(i);
        }

        #region 继承的颜色
        public override Color ArmJoint_HighLightColor(ArmJointGraphics armJointGraphics, Vector2 pos)
        {
            return AMOracleColor.DarkGrey;
        }

        public override Color ArmJoint_BaseColor(ArmJointGraphics armJointGraphics, Vector2 pos)
        {
            return AMOracleColor.VeryDarkGrey;
        }

        public override Color UbilicalCord_WireCol_1(UbilicalCord ubilicalCord)
        {
            return AMOracleColor.Rose;
        }

        public override Color UbilicalCord_WireCol_2(UbilicalCord ubilicalCord)
        {
            return AMOracleColor.Blue;
        }

        public override Color Gown_Color(Gown gown, float f)
        {
            if (f <= 0.1f)
                return AMOracleColor.Blue;
            else if (f == 1f)
                return AMOracleColor.LightBlue;
            else
                return Color.Lerp(AMOracleColor.LightBlue, AMOracleColor.DarkBlue, f + 0.1f);
        }
        #endregion

        public class GownCover
        {
            public OracleGraphics owner;

            public int divs = 11;
            public float sleeveWidth = 3f;

            public Vector2[] collarPos;
            public Vector2[] leftSleevePos;
            public Vector2[] rightSleevePos;
            public Vector2[] midColPos;

            public GownCover(OracleGraphics owner)
            {
                this.owner = owner;

                collarPos = new Vector2[divs];
                leftSleevePos = new Vector2[10];
                rightSleevePos = new Vector2[10];

                midColPos = new Vector2[divs];
            }

            public Color Color(int y)
            {
                if (y < 10)
                    return UnityEngine.Color.Lerp(AMOracleColor.Blue, AMOracleColor.DarkBlue, (float)y / 9f);
                else if (y == 10)
                    return AMOracleColor.LightBlue;
                else
                    return UnityEngine.Color.Lerp(AMOracleColor.Blue, AMOracleColor.DarkBlue, (float)y / 9f);
            }

            public void InitiateSprites(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites[sprite] = TriangleMesh.MakeGridMesh("Futile_White", divs - 1);

                for (int x = 0; x < divs; x++)
                {
                    for (int y = 0; y < divs; y++)
                    {
                        (sLeaser.sprites[sprite] as TriangleMesh).verticeColors[y * divs + x] = Color(y);
                    }
                }
            }

            public void Update()
            {
                for (int x = 0; x < divs; x++)
                {
                    collarPos[x] = owner.gowns[0].clothPoints[x, 0, 0];
                }

                for (int y = 0; y < divs; y++)
                {
                    Vector2 delta = owner.gowns[0].clothPoints[5, y, 0] - owner.gowns[0].clothPoints[5, 0, 0];
                    midColPos[y] = owner.gowns[0].clothPoints[5, 0, 0] + delta / 2f;
                }
            }

            public void DrawSprites(int i, int robeSprite, int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 smoothedBodyPos = Vector2.Lerp(owner.owner.firstChunk.lastPos, owner.owner.firstChunk.pos, timeStacker);
                Vector2 bodyDir = Custom.DirVec(Vector2.Lerp(owner.owner.bodyChunks[1].lastPos, owner.owner.bodyChunks[1].pos, timeStacker), smoothedBodyPos);
                Vector2 perpBodyDir = Custom.PerpendicularVector(bodyDir);

                for (int k = 0; k < 2; k++)
                {
                    Vector2 smoothedHandPos = Vector2.Lerp(owner.hands[k].lastPos, owner.hands[k].pos, timeStacker);
                    Vector2 shoulderPos = smoothedBodyPos + perpBodyDir * 4f * ((k == 1) ? -1f : 1f);
                    Vector2 cB = smoothedHandPos + Custom.DirVec(smoothedHandPos, shoulderPos) * 3f + bodyDir;
                    Vector2 cA = shoulderPos + perpBodyDir * 5f * ((k == 1) ? -1f : 1f);

                    Vector2 vector14 = shoulderPos - perpBodyDir * 2f * ((k == 1) ? -1f : 1f);


                    for (int m = 0; m < 5; m++)
                    {

                        float f = (float)m / 6f;
                        Vector2 sleevePosOnBezier = Custom.Bezier(shoulderPos, cA, smoothedHandPos, cB, f);
                        Vector2 vector16 = Custom.DirVec(vector14, sleevePosOnBezier);
                        Vector2 vector17 = Custom.PerpendicularVector(vector16) * ((k == 0) ? -1f : 1f);
                        float num6 = Vector2.Distance(vector14, sleevePosOnBezier);

                        Vector2 posA = sleevePosOnBezier - vector16 * num6 * 0.3f + vector17 * sleeveWidth - camPos;
                        Vector2 posB = sleevePosOnBezier + vector17 * sleeveWidth - camPos;

                        if (k == 0)
                        {
                            leftSleevePos[m * 2] = posA;
                            leftSleevePos[m * 2 + 1] = posB;
                        }
                        else
                        {
                            rightSleevePos[(m * 2)] = posA;
                            rightSleevePos[(m * 2) + 1] = posB;
                        }
                        vector14 = sleevePosOnBezier;
                    }
                }

                TriangleMesh gown = sLeaser.sprites[robeSprite] as TriangleMesh;
                TriangleMesh cover = sLeaser.sprites[sprite] as TriangleMesh;

                //衣领（披肩上侧）
                for (int x = 0; x < divs; x++)//draw collar
                {
                    cover.MoveVertice(0 * divs + x, collarPos[x] - camPos);
                }

                //披肩两侧
                for (int y = 1; y < divs; y++)//draw left and right
                {
                    //用袍子的前6个位置来对应披肩的11个位置
                    int floor = (int)Mathf.Floor(y * 6 / (divs - 1));//整数部分
                    float fraction = y * 6 / (divs - 1) - floor;     //小数部分
                    //Vector2.Lerp(this.clothPoints[i, j, 1], this.clothPoints[i, j, 0], timeStacker) - camPos;
                    Vector2 leftGownInterpolation = Vector2.Lerp(gown.vertices[(divs - 1) * divs + floor], gown.vertices[(divs - 1) * divs + floor + 1], fraction);
                    Vector2 rightGownInterpolation = Vector2.Lerp(gown.vertices[0 * divs + floor], gown.vertices[0 * divs + floor + 1], fraction);

                    //判断披肩是否要加上袍子多于披肩的宽度
                    float gradientAngle = 0.1f;//渐变角度范围（弧度制）
                    float leftAngle = Vector2.SignedAngle(leftGownInterpolation, leftSleevePos[y - 1]);
                    float leftExpandScale = Mathf.Min(1f, -leftAngle / gradientAngle + 1);
                    Vector2 leftExpandWidth = leftGownInterpolation - leftSleevePos[y - 1];
                    Vector2 leftExpand = ((leftAngle < gradientAngle) ? leftExpandScale : 0f) * leftExpandWidth;

                    float rightAngle = Vector2.SignedAngle(rightGownInterpolation, rightSleevePos[y - 1]);
                    float rightExpandScale = Mathf.Min(1f, rightAngle / gradientAngle + 1);
                    Vector2 rightExpandWidth = rightGownInterpolation - rightSleevePos[y - 1];
                    Vector2 rightExpand = ((rightAngle > -gradientAngle) ? rightExpandScale : 0f) * rightExpandWidth;

                    //让披肩再往外一点
                    Vector2 expand = perpBodyDir * 7f * y / (divs - 1);

                    cover.MoveVertice(y * divs + 0, leftSleevePos[y - 1] + leftExpand + expand);
                    cover.MoveVertice(y * divs + divs - 1, rightSleevePos[y - 1] + rightExpand - expand);
                }

                //披肩中间
                for (int y = 1; y < divs; y++)//draw mid
                {
                    cover.MoveVertice(y * divs + 5, midColPos[y] - camPos);
                }

                for (int x = 1; x < 5; x++)
                {
                    for (int y = 1; y < divs; y++)
                    {
                        Vector2 left = cover.vertices[y * divs + 0];
                        Vector2 mid = cover.vertices[y * divs + 5];
                        float t = Mathf.InverseLerp(0f, 5f, x);

                        cover.MoveVertice(y * divs + x, Vector2.Lerp(left, mid, t));
                    }
                }

                for (int x = 6; x < divs - 1; x++)
                {
                    for (int y = 1; y < divs; y++)
                    {
                        Vector2 right = cover.vertices[y * divs + divs - 1];
                        Vector2 mid = cover.vertices[y * divs + 5];
                        float t = Mathf.InverseLerp(5, divs, x);

                        cover.MoveVertice(y * divs + x, Vector2.Lerp(mid, right, t));
                    }
                }

                //将两侧披肩各只留一半
                int half = (int)((divs - 1) / 2);
                for (int y = 1; y < divs; y++)
                {
                    for (int x = 0; x < divs; x++)//draw collar
                    {
                        if (i == 0 && x <= half)//玩家视角的右侧
                            cover.MoveVertice(y * divs + x, cover.vertices[y * divs + half + 1]);
                        else if (i == 1 && x >= half)//玩家视角的左侧
                            cover.MoveVertice(y * divs + x, cover.vertices[y * divs + half - 1]);
                    }
                }

                //当手抬高时披肩高度变小
                for (int k = 0; k < 2; k++)//玩家视角，左0右1
                {
                    Vector2 smoothedHandPos = Vector2.Lerp(owner.hands[k].lastPos, owner.hands[k].pos, timeStacker);
                    Vector2 fromBodyToHand = Custom.DirVec(smoothedBodyPos, smoothedHandPos);//手的方向
                    float angleBetweenHandAndBody = Vector2.SignedAngle(-bodyDir, fromBodyToHand);//手与身体夹角（手贴在身体两侧时为0，举在头顶为±180）
                    float angleScale = Mathf.Abs(angleBetweenHandAndBody);
                    float scale = (angleScale >= 90) ? Mathf.Lerp(0, 1, (angleScale - 80) / 100) : 0;
                    for (int y = 1; y < divs; y++)
                    {
                        for (int x = 0; x < divs; x++)//draw collar
                        {
                            if (i == 0 && k == 1)//玩家视角的右侧
                                cover.MoveVertice(y * divs + x, Vector2.Lerp(cover.vertices[y * divs + x], cover.vertices[0 * divs + x], scale));
                            else if (i == 1 && k == 0)//玩家视角的左侧
                                cover.MoveVertice(y * divs + x, Vector2.Lerp(cover.vertices[y * divs + x], cover.vertices[0 * divs + x], scale));
                        }
                    }
                }
            }
        }

        public class GownRibbon
        {
            public OracleGraphics owner;
            public TailSegment[] ribbon;
            public int divs = 11;
            public int timeAdd;
            static readonly float distance = 3f;//飘带间距
            public readonly float width = 0.35f;//飘带宽度
            public readonly float spacing = 6f;
            public readonly float maxLength = 9f;

            public GownRibbon(OracleGraphics owner)
            {
                this.owner = owner;

                ribbon = new TailSegment[divs];
                ribbon[0] = new TailSegment(owner, 5f, 4f, null, 0.85f, 1f, 3f, true);
                for (int j = 1; j < divs; j++)
                    ribbon[j] = new TailSegment(owner, 5f, 7f, ribbon[j - 1], 0.55f, 1f, 0.5f, true);
            }

            public Color Color(int y)
            {
                return UnityEngine.Color.Lerp(AMOracleColor.DarkBlue, AMOracleColor.LightGreen, (float)y / (float)((divs - 1) * 4 + 3));
            }

            public void InitiateSprites(int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                TriangleMesh mesh = TriangleMesh.MakeLongMesh(divs, false, true, "Futile_White");
                sLeaser.sprites[sprite] = mesh;
                for (int j = 0; j < mesh.verticeColors.Length; j++)
                {
                    (sLeaser.sprites[sprite] as TriangleMesh).verticeColors[j] = Color(j);
                }
            }

            public void Update(int i)
            {
                timeAdd++;
                if (timeAdd > float.MaxValue - 100)
                    timeAdd = 0;

                Vector2 smoothedBodyPos = owner.owner.firstChunk.lastPos;
                Vector2 bodyDir = Custom.DirVec(owner.owner.bodyChunks[1].lastPos, smoothedBodyPos);
                Vector2 perpBodyDir = Custom.PerpendicularVector(bodyDir);

                //通过身体角度判断移动
                var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, bodyDir), -22.5f, 22.5f);
                //实际凤尾偏移
                var nowSwallowTailSpacing = spacing * (Mathf.Abs(moveDeg) > 10 ? 0.3f : 1f);

                var rootPos = smoothedBodyPos + (i == 0 ? -1 : 1) * perpBodyDir.normalized * nowSwallowTailSpacing + bodyDir * -0.2f;

                var num3 = 1f - Mathf.Clamp((Mathf.Abs(Mathf.Lerp(owner.owner.bodyChunks[1].vel.x, owner.owner.bodyChunks[0].vel.x, 0.35f)) - 1f) * 0.5f, 0f, 1f);

                Vector2 vector2 = rootPos;
                Vector2 pos = rootPos;
                float num9 = 28f;

                ribbon[0].connectedPoint = new Vector2?(rootPos);
                for (int k = 0; k < divs; k++)
                {
                    ribbon[k].Update();
                    ribbon[k].vel *= Mathf.Lerp(0.75f, 0.95f, num3 * (1f - owner.owner.bodyChunks[1].submersion));//水中减少速度

                    Vector2 drift = Drift(owner, this, k, i);

                    TailSegment tailSegment = ribbon[k];
                    tailSegment.vel.y = tailSegment.vel.y - Mathf.Lerp(0.1f, 0.5f, num3) * (1f - owner.owner.bodyChunks[1].submersion) * owner.owner.EffectiveRoomGravity;
                    num3 = (num3 * 10f + 1f) / 11f;

                    //超出长度限位
                    if (!Custom.DistLess(ribbon[k].pos, rootPos, maxLength * (k + 1)))
                    {
                        ribbon[k].pos = rootPos + Custom.DirVec(rootPos, ribbon[k].pos) * maxLength * (k + 1);
                    }

                    ribbon[k].pos += 0.25f * drift;
                    ribbon[k].vel += 0.5f * Vector2.down * Mathf.Min(1f / drift.magnitude, 1f);
                    ribbon[k].vel += Custom.DirVec(vector2, ribbon[k].pos) * num9 / Vector2.Distance(vector2, ribbon[k].pos);
                    num9 *= 0.5f;
                    vector2 = pos;
                    pos = ribbon[k].pos;
                }
            }

            public void DrawSprites(int i, int sprite, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 smoothedBodyPos = Vector2.Lerp(owner.owner.firstChunk.lastPos, owner.owner.firstChunk.pos, timeStacker);
                Vector2 bodyDir = Custom.DirVec(Vector2.Lerp(owner.owner.bodyChunks[1].lastPos, owner.owner.bodyChunks[1].pos, timeStacker), smoothedBodyPos);
                Vector2 perpBodyDir = Custom.PerpendicularVector(bodyDir);

                //身体位置
                Vector2 drawPos1 = Vector2.Lerp(owner.owner.bodyChunks[0].lastPos, owner.owner.bodyChunks[0].pos, timeStacker);
                //臀部位置
                Vector2 drawPos2 = Vector2.Lerp(owner.owner.bodyChunks[1].lastPos, owner.owner.bodyChunks[1].pos, timeStacker);
                //身体至臀部方向的向量
                Vector2 dif = (drawPos1 - drawPos2).normalized;
                //身体旋转角度
                float bodyRotation = Mathf.Atan2(dif.x, dif.y);

                //设置图层
                //SwallowtailLevel(player, sLeaser, bodyRotation);

                //通过身体角度判断移动
                var moveDeg = Mathf.Clamp(Custom.AimFromOneVectorToAnother(Vector2.zero, (drawPos2 - drawPos1).normalized), -22.5f, 22.5f);

                //实际偏移
                var nowSpacing = spacing * (Mathf.Abs(moveDeg) > 10 ? 0.3f : 1f);

                //实际显示
                var dir = Custom.DirVec(owner.owner.bodyChunks[0].pos, owner.owner.bodyChunks[1].pos).normalized;
                var rootPos = Vector2.Lerp(owner.owner.bodyChunks[1].pos, owner.owner.bodyChunks[0].pos, 0.35f) + (i == 0 ? -1 : 1) * Custom.PerpendicularVector(dir).normalized * nowSpacing + dir * -0.2f;

                var lastDir = Custom.DirVec(owner.owner.bodyChunks[0].lastPos, owner.owner.bodyChunks[1].lastPos).normalized;
                Vector2 vector2 = Vector2.Lerp(Vector2.Lerp(owner.owner.bodyChunks[1].lastPos, owner.owner.bodyChunks[0].lastPos, 0.35f) + (i == 0 ? -1 : 1) * Custom.PerpendicularVector(lastDir).normalized * nowSpacing + lastDir * 5f, rootPos, timeStacker);
                Vector2 vector4 = (vector2 * 3f + rootPos) / 4f;

                float d2 = 6f;

                bool OutLength = false;

                TriangleMesh ribbonMesh = sLeaser.sprites[sprite] as TriangleMesh;

                for (int j = 0; j < divs; j++)
                {
                    Vector2 vector5 = Vector2.Lerp(ribbon[j].lastPos, ribbon[j].pos, timeStacker);
                    Vector2 normalized = (vector5 - vector4).normalized;
                    Vector2 widthDir = Custom.PerpendicularVector(normalized);
                    float d3 = Vector2.Distance(vector5, vector4) / 5f;

                    if (j == 0)
                    {
                        d3 = 0f;
                    }

                    if (j != 0 && !Custom.DistLess(ribbonMesh.vertices[j * 4], ribbonMesh.vertices[j * 4 - 4], 40))
                        OutLength = true;

                    //设置坐标
                    ribbonMesh.MoveVertice(j * 4, vector4 - widthDir * d2 * width + normalized * d3 - camPos);
                    ribbonMesh.MoveVertice(j * 4 + 1, vector4 + widthDir * d2 * width + normalized * d3 - camPos);
                    /*
                    if (j > 0)
                    {
                        swallowTail.MoveVertice(j * 4, ((swallowTail.vertices[j * 4]) + swallowTail.vertices[(j - 1) * 4 + 2]) / 2);
                        swallowTail.MoveVertice(j * 4 + 1, ((swallowTail.vertices[j * 4 + 1]) + swallowTail.vertices[(j - 1) * 4 + 3]) / 2);
                    }*/
                    if (j < divs - 1)
                    {
                        ribbonMesh.MoveVertice(j * 4 + 2, vector5 - widthDir * ribbon[j].StretchedRad * width - normalized * d3 - camPos);
                        ribbonMesh.MoveVertice(j * 4 + 3, vector5 + widthDir * ribbon[j].StretchedRad * width - normalized * d3 - camPos);
                    }
                    else
                    {
                        ribbonMesh.MoveVertice(j * 4 + 2, vector5 - camPos);
                        ribbonMesh.MoveVertice(j * 4 + 3, vector5 - camPos);
                    }
                    //d2 = ribbon[i * 4 + j].StretchedRad;
                    d2 = ribbon[j].StretchedRad;
                    vector4 = vector5;
                }

                if ((OutLength && sLeaser.sprites[sprite].isVisible))
                    sLeaser.sprites[sprite].isVisible = false;
                else if (!OutLength && !sLeaser.sprites[sprite].isVisible)
                    sLeaser.sprites[sprite].isVisible = true;
            }

            private Vector2 Drift(OracleGraphics owner, GownRibbon self, int i, int k)
            {
                //身体位置
                Vector2 drawPos1 = owner.owner.bodyChunks[0].lastPos;
                //臀部位置
                Vector2 drawPos2 = owner.owner.bodyChunks[1].lastPos;
                //身体至臀部方向的向量
                Vector2 dif = (drawPos1 - drawPos2).normalized;
                //身体旋转角度
                float bodyRotation = Mathf.Atan2(dif.x, dif.y);
                //垂直修正
                Vector2 swallowtailH = Mathf.Lerp(0, 7f, 1 - Mathf.Abs(Mathf.Cos(bodyRotation))) * Vector2.up;
                Vector2 a = Custom.PerpendicularVector(dif).normalized;
                float d = 2f * distance * (0.2f + 0.8f * Mathf.Abs(dif.y));
                float amplitude = (Mathf.Pow(2, 1.5f * i / divs) - 1) * owner.owner.bodyChunks[0].vel.magnitude;
                Vector2 wave = 0.16f * a * amplitude * Mathf.Sin(0.4f * (i + (k == 0 ? 0 : 3)) + 0.2f * timeAdd + 1.2f);
                Vector2 drift = i / 8f * (a + wave) * d * (k == 0 ? -1f : 1f) + swallowtailH;
                return drift;
            }
        }
    }
}
