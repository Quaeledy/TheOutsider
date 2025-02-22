using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheOutsider.CustomLore.CustomObject.AlcedoMask
{
    public class AlcedoMask : PlayerCarryableItem, IDrawable
    {
        public class MaskType : ExtEnum<MaskType>
        {
            public static readonly MaskType NORMAL = new MaskType("NORMAL", register: true);

            public static readonly MaskType KING = new MaskType("KING", register: true);

            public static readonly MaskType SCAVKING = new MaskType("SCAVKING", register: true);

            public MaskType(string value, bool register = false)
                : base(value, register)
            {
            }
        }

        public Vector2 rotationA;

        public Vector2 lastRotationA;

        public Vector2 rotationB;

        public Vector2 lastRotationB;

        public Vector2 rotVel;

        public int onGroundPos;

        public float donned;

        public float lastDonned;

        public float viewFromSide;

        public float lastViewFromSide;

        public float fallOffAlcedoMode;

        public AlcedoMaskGraphics maskGfx;

        public AbstractAlcedoMask AbstrMsk => abstractPhysicalObject as AbstractAlcedoMask;

        public bool King => maskGfx.maskType == MaskType.KING;

        public AlcedoMask(AbstractPhysicalObject abstractPhysicalObject, World world)
            : base(abstractPhysicalObject)
        {
            bodyChunks = new BodyChunk[1];
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.14f);
            bodyChunkConnections = new BodyChunkConnection[0];
            airFriction = 0.999f;
            gravity = 0.9f;
            bounce = 0.4f;
            surfaceFriction = 0.3f;
            collisionLayer = 2;
            waterFriction = 0.98f;
            buoyancy = 0.6f;
            maskGfx = new AlcedoMaskGraphics(this, AbstrMsk, 0);
            maskGfx.GenerateColor(AbstrMsk.colorSeed);
        }

        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
            onGroundPos = Random.Range(0, 3) - 1;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            lastRotationA = rotationA;
            lastRotationB = rotationB;
            lastDonned = donned;
            lastViewFromSide = viewFromSide;
            float to = 0f;
            float to2 = 0f;
            rotationA = Custom.DegToVec(Custom.VecToDeg(rotationA) + rotVel.x);
            rotationB = Custom.DegToVec(Custom.VecToDeg(rotationB) + rotVel.y);
            rotVel = Vector2.ClampMagnitude(rotVel, 50f);
            rotVel *= Custom.LerpMap(rotVel.magnitude, 5f, 50f, 1f, 0.8f);
            fallOffAlcedoMode = Mathf.Max(0f, fallOffAlcedoMode - 1f / 160f);
            CollideWithTerrain = grabbedBy.Count == 0;
            CollideWithObjects = grabbedBy.Count == 0;
            if (grabbedBy.Count > 0)
            {
                Vector2 vector = Custom.PerpendicularVector(firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos);
                if (grabbedBy[0].grabber is Player)
                {
                    vector *= Mathf.Sign(Custom.DistanceToLine(firstChunk.pos, grabbedBy[0].grabber.bodyChunks[0].pos, grabbedBy[0].grabber.bodyChunks[1].pos));
                    if ((grabbedBy[0].grabber as Player).graphicsModule != null &&
                        (grabbedBy[0].grabber as Player).standing &&
                        ((grabbedBy[0].grabber as Player).bodyMode != Player.BodyModeIndex.ClimbingOnBeam || (grabbedBy[0].grabber as Player).animation == Player.AnimationIndex.StandOnBeam) && (grabbedBy[0].grabber as Player).bodyMode != Player.BodyModeIndex.Swimming &&
                        (grabbedBy[0].graspUsed == 1 || grabbedBy[0].grabber.grasps[1] == null || grabbedBy[0].grabber.grasps[1].grabbed.abstractPhysicalObject.type != TheOutsiderEnums.AbstractObjectType.AlcedoMask))
                    {
                        to = Mathf.InverseLerp(15f, 10f, Vector2.Distance((grabbedBy[0].grabber.graphicsModule as PlayerGraphics).hands[grabbedBy[0].graspUsed].pos, grabbedBy[0].grabber.mainBodyChunk.pos));
                        if ((grabbedBy[0].grabber as Player).input[0].x != 0 && Mathf.Abs(grabbedBy[0].grabber.bodyChunks[1].lastPos.x - grabbedBy[0].grabber.bodyChunks[1].pos.x) > 2f)
                        {
                            to2 = (grabbedBy[0].grabber as Player).input[0].x;
                        }
                    }
                }
                rotationA = Vector3.Slerp(rotationA, vector, 0.5f);
                rotationB = new Vector2(0f, 1f);
            }
            else if (firstChunk.ContactPoint.y < 0)
            {
                Vector2 b;
                Vector2 b2;
                if (onGroundPos == 0)
                {
                    b = new Vector2(0f, 1f);
                    b2 = new Vector2(0f, -1f);
                }
                else
                {
                    b = Custom.DegToVec(15f * onGroundPos);
                    b2 = Custom.DegToVec(120f * onGroundPos);
                }
                rotationA = Vector2.Lerp(rotationA, b, Random.value);
                rotationB = Vector2.Lerp(rotationB, b2, Random.value);
                rotVel *= Random.value;
            }
            else if (Vector2.Distance(firstChunk.lastPos, firstChunk.pos) > 5f && rotVel.magnitude < 7f)
            {
                rotVel += Custom.RNV() * (Mathf.Lerp(7f, 25f, Random.value) + firstChunk.vel.magnitude * 2f);
                onGroundPos = Random.Range(0, 3) - 1;
            }
            donned = Custom.LerpAndTick(donned, to, 0.11f, 1f / 30f);
            viewFromSide = Custom.LerpAndTick(viewFromSide, to2, 0.11f, 1f / 30f);
            maskGfx.rotationA = rotationA;
            maskGfx.rotationB = rotationB;
            maskGfx.fallOffAlcedoMode = fallOffAlcedoMode;
            maskGfx.Update();
        }

        public override void PickedUp(Creature upPicker)
        {
            room.PlaySound(SoundID.Vulture_Mask_Pick_Up, firstChunk);
        }

        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            base.TerrainImpact(chunk, direction, speed, firstContact);
            if (grabbedBy.Count == 0 && speed > 4f && firstContact)
            {
                room.PlaySound(SoundID.Vulture_Mask_Terrain_Impact, firstChunk, loop: false, Custom.LerpMap(speed, 4f, 9f, 0.2f, 1f), 1f);
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[maskGfx.TotalSprites];
            maskGfx.InitiateSprites(sLeaser, rCam);
            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
            float num = Mathf.Lerp(lastDonned, donned, timeStacker);
            Vector2 vector2 = Vector3.Slerp(lastRotationA, rotationA, timeStacker);
            Vector2 vector3 = Vector3.Slerp(lastRotationB, rotationB, timeStacker);
            if (num > 0f && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && grabbedBy[0].grabber.graphicsModule is PlayerGraphics playerGraphics)
            {
                float num2 = Mathf.Lerp(lastViewFromSide, viewFromSide, timeStacker);
                Vector2 vector4 = Custom.DirVec(Vector2.Lerp(playerGraphics.drawPositions[1, 1], playerGraphics.drawPositions[1, 0], timeStacker), Vector2.Lerp(playerGraphics.drawPositions[0, 1], playerGraphics.drawPositions[0, 0], timeStacker));
                Vector2 a = Vector2.Lerp(playerGraphics.drawPositions[0, 1], playerGraphics.drawPositions[0, 0], timeStacker) + vector4 * 3f;
                a = Vector2.Lerp(a, Vector2.Lerp(playerGraphics.head.lastPos, playerGraphics.head.pos, timeStacker) + vector4 * 3f, 0.5f);
                a += Vector2.Lerp(playerGraphics.lastLookDir, playerGraphics.lookDirection, timeStacker) * 1.5f;
                vector2 = Vector3.Slerp(vector2, vector4, num);
                if ((playerGraphics.owner as Player).eatCounter < 35)
                {
                    vector3 = Vector3.Slerp(vector3, new Vector2(0f, -1f), num);
                    a += vector4 * Mathf.InverseLerp(35f, 15f, (playerGraphics.owner as Player).eatCounter) * 7f;
                }
                else
                {
                    vector3 = Vector3.Slerp(vector3, new Vector2(0f, 1f), num);
                }
                if (num2 != 0f)
                {
                    vector2 = Custom.DegToVec(Custom.VecToDeg(vector2) - 20f * num2);
                    vector3 = Vector3.Slerp(vector3, Custom.DegToVec(-50f * num2), Mathf.Abs(num2));
                    a += vector4 * 2f * Mathf.Abs(num2);
                    a -= Custom.PerpendicularVector(vector4) * 4f * num2;
                }
                vector = Vector2.Lerp(vector, a, num);
            }
            maskGfx.overrideDrawVector = vector;
            maskGfx.overrideRotationVector = vector2;
            maskGfx.overrideAnchorVector = vector3;
            maskGfx.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            maskGfx.ApplyPalette(sLeaser, rCam, palette);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            maskGfx.AddToContainer(sLeaser, rCam, newContatiner);
        }
    }

    public class AlcedoMaskGraphics
    {
        public class CosmeticPearlString
        {
            public Vector2 pos;

            public List<Vector2> pearlPositions;

            public List<Vector2> pearlVelocities;

            public List<float> connectionRads;

            public List<float> pearlGlimmers;

            public List<float> glimmerProg;

            public List<float> glimmerWait;

            public List<float> glimmerSpeed;

            public int startSprite;

            public float darkness;

            public float submersion;

            public Color color;

            public float gravity;

            public int layer;

            public Vector2 lastPos;

            public List<Color> pearlColors;

            public int TotalSprites => pearlPositions.Count * 4;

            public CosmeticPearlString(Vector2 pos, float length, int startSprite)
            {
                this.pos = pos;
                lastPos = pos;
                this.startSprite = startSprite;
                Vector2 vector = new Vector2(this.pos.x, this.pos.y);
                pearlPositions = new List<Vector2>();
                pearlVelocities = new List<Vector2>();
                connectionRads = new List<float>();
                pearlGlimmers = new List<float>();
                glimmerProg = new List<float>();
                glimmerWait = new List<float>();
                glimmerSpeed = new List<float>();
                pearlColors = new List<Color>();
                float num = 0f;
                int i = 0;
                Random.State state = Random.state;
                Random.InitState(startSprite + 3);
                for (; i < length; i++)
                {
                    float num2 = Random.Range(5f, 15f);
                    if (Random.value < 0.33f)
                    {
                        num2 = 5f;
                    }
                    num += num2;
                    vector.y -= num2;
                    pearlPositions.Add(new Vector2(vector.x, vector.y));
                    pearlVelocities.Add(Vector2.zero);
                    float value = Random.value;
                    glimmerProg.Add(value);
                    pearlGlimmers.Add(Mathf.Sin(value * (float)Math.PI) * Random.value);
                    glimmerWait.Add(Random.Range(20, 40));
                    glimmerSpeed.Add(1f / Mathf.Lerp(5f, 15f, Random.value));
                    float value2 = Random.value;
                    if (value2 < 0.33f || num >= length * 0.67f)
                    {
                        pearlColors.Add(new Color(1f, 0.6f, 0.9f));
                    }
                    else if (value2 < 0.67f)
                    {
                        pearlColors.Add(new Color(0.9f, 0.9f, 0.9f));
                    }
                    else
                    {
                        pearlColors.Add(new Color(0.9f, 0.9f, 0.6f));
                    }
                    connectionRads.Add(num2);
                    if (num > length)
                    {
                        break;
                    }
                }
                Random.state = state;
            }

            public void Update()
            {
                for (int i = 1; i < pearlPositions.Count; i++)
                {
                    float num = Vector2.Distance(pearlPositions[i], pearlPositions[i - 1]);
                    if (num > connectionRads[i])
                    {
                        Vector2 normalized = (pearlPositions[i] - pearlPositions[i - 1]).normalized;
                        pearlPositions[i] += normalized * (connectionRads[i] - num) * 0.98f;
                        pearlPositions[i - 1] -= normalized * (connectionRads[i] - num) * 0.98f;
                        pearlVelocities[i] += normalized * (connectionRads[i] - num) * 0.98f;
                        pearlVelocities[i - 1] -= normalized * (connectionRads[i] - num) * 0.98f;
                    }
                }
                Attach();
                for (int num2 = pearlPositions.Count - 2; num2 >= 0; num2--)
                {
                    float num3 = Vector2.Distance(pearlPositions[num2], pearlPositions[num2 + 1]);
                    if (num3 > connectionRads[num2])
                    {
                        Vector2 normalized2 = (pearlPositions[num2] - pearlPositions[num2 + 1]).normalized;
                        pearlPositions[num2] += normalized2 * (connectionRads[num2] - num3) * 0.98f;
                        pearlPositions[num2 + 1] -= normalized2 * (connectionRads[num2] - num3) * 0.98f;
                        pearlVelocities[num2] += normalized2 * (connectionRads[num2] - num3) * 0.98f;
                        pearlVelocities[num2 + 1] -= normalized2 * (connectionRads[num2] - num3) * 0.98f;
                    }
                }
                Attach();
                for (int j = 0; j < pearlVelocities.Count; j++)
                {
                    pearlVelocities[j] = new Vector2(pearlVelocities[j].x, pearlVelocities[j].y - gravity);
                    pearlPositions[j] += pearlVelocities[j];
                }
                for (int k = 0; k < pearlGlimmers.Count; k++)
                {
                    pearlGlimmers[k] = Mathf.Sin(glimmerProg[k] * (float)Math.PI) * Random.value;
                    if (glimmerProg[k] < 1f)
                    {
                        glimmerProg[k] = Mathf.Min(1f, glimmerProg[k] + glimmerSpeed[k]);
                        continue;
                    }
                    if (glimmerWait[k] > 0f)
                    {
                        glimmerWait[k] -= 1f;
                        continue;
                    }
                    glimmerWait[k] = Random.Range(20, 40);
                    glimmerProg[k] = 0f;
                    glimmerSpeed[k] = 1f / Mathf.Lerp(5f, 15f, Random.value);
                }
            }

            private void Attach()
            {
                Vector2 normalized = (pearlPositions[0] - pos).normalized;
                float num = Vector2.Distance(pearlPositions[0], pos);
                pearlPositions[0] += normalized * (connectionRads[0] - num);
                pearlVelocities[0] += normalized * (connectionRads[0] - num);
            }

            public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                for (int i = startSprite; i < startSprite + TotalSprites; i++)
                {
                    if ((i - startSprite) % 4 == 0)
                    {
                        sLeaser.sprites[i] = new FSprite("pixel");
                        sLeaser.sprites[i].anchorY = 0f;
                    }
                    else if ((i - startSprite) % 4 == 1)
                    {
                        sLeaser.sprites[i] = new FSprite("JetFishEyeA");
                    }
                    else if ((i - startSprite) % 4 == 2)
                    {
                        sLeaser.sprites[i] = new FSprite("tinyStar");
                    }
                    else if ((i - startSprite) % 4 == 3)
                    {
                        sLeaser.sprites[i] = new FSprite("Futile_White");
                        sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["FlatLightBehindTerrain"];
                    }
                }
                AddToContainer(sLeaser, rCam, null);
            }

            public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 vector = pos;
                int num = 0;
                for (int i = startSprite; i < startSprite + TotalSprites; i++)
                {
                    Vector2 vector2 = pearlPositions[num];
                    float num2 = pearlGlimmers[num];
                    if ((i - startSprite) % 4 == 0)
                    {
                        sLeaser.sprites[i].x = vector2.x - camPos.x;
                        sLeaser.sprites[i].y = vector2.y - camPos.y;
                        sLeaser.sprites[i].scaleY = Vector2.Distance(vector2, vector);
                        sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
                        sLeaser.sprites[i].isVisible = layer != 0;
                        vector = vector2;
                    }
                    else if ((i - startSprite) % 4 == 1)
                    {
                        sLeaser.sprites[i].x = vector.x - camPos.x;
                        sLeaser.sprites[i].y = vector.y - camPos.y;
                        sLeaser.sprites[i].color = Color.Lerp(Custom.RGB2RGBA(pearlColors[num] * Mathf.Lerp(1f, 0.2f, darkness), 1f), new Color(1f, 1f, 1f), num2);
                        if (num2 > 0.9f && submersion == 1f)
                        {
                            sLeaser.sprites[i].color = new Color(0f, 0.003921569f, 0f);
                        }
                        sLeaser.sprites[i].scale = 0.5f;
                        sLeaser.sprites[i].isVisible = layer != 0;
                    }
                    else if ((i - startSprite) % 4 == 2)
                    {
                        sLeaser.sprites[i].x = vector.x - camPos.x - 0.25f;
                        sLeaser.sprites[i].y = vector.y - camPos.y + 0.75f;
                        sLeaser.sprites[i].color = Color.Lerp(Custom.RGB2RGBA(pearlColors[num] * Mathf.Lerp(1.3f, 0.5f, darkness), 1f), new Color(1f, 1f, 1f), Mathf.Lerp(0.5f + 0.5f * num2, 0.2f + 0.8f * num2, darkness));
                        if (num2 > 0.9f && submersion == 1f)
                        {
                            sLeaser.sprites[i].color = new Color(0f, 0.003921569f, 0f);
                        }
                        sLeaser.sprites[i].scale = 0.5f;
                        sLeaser.sprites[i].isVisible = layer != 0;
                    }
                    else if ((i - startSprite) % 4 == 3)
                    {
                        sLeaser.sprites[i].x = vector.x - camPos.x;
                        sLeaser.sprites[i].y = vector.y - camPos.y;
                        sLeaser.sprites[i].alpha = num2 * 0.5f;
                        sLeaser.sprites[i].scale = 20f * num2 * 1f / 32f;
                        sLeaser.sprites[i].isVisible = layer != 0;
                        num++;
                    }
                }
            }

            public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                for (int i = startSprite; i < startSprite + TotalSprites; i++)
                {
                    if ((i - startSprite) % 4 == 0)
                    {
                        sLeaser.sprites[i].color = palette.blackColor;
                    }
                }
            }

            public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
            {
                if (newContatiner == null)
                {
                    newContatiner = rCam.ReturnFContainer("Items");
                }
                for (int i = startSprite; i < startSprite + TotalSprites; i++)
                {
                    sLeaser.sprites[i].RemoveFromContainer();
                    if ((i - startSprite) % 4 == 3)
                    {
                        rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
                    }
                    else
                    {
                        newContatiner.AddChild(sLeaser.sprites[i]);
                    }
                }
            }
        }

        public Vector2? overrideDrawVector;

        public PhysicalObject attachedTo;

        public AlcedoMask.MaskType maskType;

        public Vector2 rotationA;

        public Vector2 lastRotationA;

        public Vector2 rotationB;

        public Vector2 lastRotationB;

        public float fallOffAlcedoMode;

        public int firstSprite;

        public Color color;

        private HSLColor ColorA;

        private HSLColor ColorB;

        private Color blackColor;

        public Vector2? overrideRotationVector;

        public Vector2? overrideAnchorVector;

        public string overrideSprite;

        public List<CosmeticPearlString> pearlStrings;

        public bool King => maskType == AlcedoMask.MaskType.KING;

        public bool ScavKing
        {
            get
            {
                if (ModManager.MSC)
                {
                    return maskType == AlcedoMask.MaskType.SCAVKING;
                }
                return false;
            }
        }

        public int BaseTotalSprites
        {
            get
            {
                if (!King)
                {
                    return 3;
                }
                return 4;
            }
        }

        public int TotalSprites
        {
            get
            {
                int num = BaseTotalSprites;
                for (int i = 0; i < pearlStrings.Count; i++)
                {
                    num += pearlStrings[i].TotalSprites;
                }
                return num;
            }
        }

        public int SpriteIndex
        {
            get
            {
                Vector2 value = rotationB;
                if (overrideAnchorVector.HasValue)
                {
                    value = overrideAnchorVector.Value;
                }
                return Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(Custom.VecToDeg(value) / 180f) * 8f), 0, 8);
            }
        }

        public AlcedoMaskGraphics(PhysicalObject attached, AbstractAlcedoMask abstractMask, int firstSprite)
        {
            attachedTo = attached;
            if (abstractMask.king)
            {
                maskType = AlcedoMask.MaskType.KING;
            }
            else if (ModManager.MSC && abstractMask.scavKing)
            {
                maskType = AlcedoMask.MaskType.SCAVKING;
            }
            else
            {
                maskType = AlcedoMask.MaskType.NORMAL;
            }
            this.firstSprite = firstSprite;
            overrideSprite = abstractMask.spriteOverride;
            pearlStrings = new List<CosmeticPearlString>();
            if (!ScavKing)
            {
                return;
            }
            overrideSprite = "";
            int num = this.firstSprite + BaseTotalSprites;
            for (int i = 0; i < 4; i++)
            {
                float length = 17f;
                if (i >= 2)
                {
                    length = 25f;
                }
                CosmeticPearlString cosmeticPearlString = new CosmeticPearlString(PearlAttachPos(i), length, num);
                pearlStrings.Add(cosmeticPearlString);
                num += cosmeticPearlString.TotalSprites;
            }
        }

        public AlcedoMaskGraphics(PhysicalObject attached, AlcedoMask.MaskType type, int firstSprite, string overrideSprite)
        {
            attachedTo = attached;
            maskType = type;
            this.firstSprite = firstSprite;
            this.overrideSprite = overrideSprite;
            pearlStrings = new List<CosmeticPearlString>();
            if (!ScavKing)
            {
                return;
            }
            this.overrideSprite = "";
            int num = firstSprite + BaseTotalSprites;
            for (int i = 0; i < 4; i++)
            {
                float length = 17f;
                if (i >= 2)
                {
                    length = 25f;
                }
                CosmeticPearlString cosmeticPearlString = new CosmeticPearlString(PearlAttachPos(i), length, num);
                pearlStrings.Add(cosmeticPearlString);
                num += cosmeticPearlString.TotalSprites;
            }
        }

        public void GenerateColor(int colorSeed)
        {
            if (ModManager.MSC && maskType == AlcedoMask.MaskType.SCAVKING)
            {
                ColorA = new HSLColor(1f, 1f, 1f);
                ColorB = new HSLColor(1f, 1f, 1f);
                return;
            }
            Random.State state = Random.state;
            Random.InitState(colorSeed);
            if (King)
            {
                ColorB = new HSLColor(Mathf.Lerp(0.93f, 1.07f, Random.value), Mathf.Lerp(0.8f, 1f, 1f - Random.value * Random.value), Mathf.Lerp(0.45f, 1f, Random.value * Random.value));
                ColorA = new HSLColor(ColorB.hue + Mathf.Lerp(-0.25f, 0.25f, Random.value), Mathf.Lerp(0.5f, 0.7f, Random.value), Mathf.Lerp(0.7f, 0.8f, Random.value));
            }
            else
            {
                ColorA = new HSLColor(Mathf.Lerp(0.9f, 1.6f, Random.value), Mathf.Lerp(0.5f, 0.7f, Random.value), Mathf.Lerp(0.7f, 0.8f, Random.value));
                ColorB = new HSLColor(ColorA.hue + Mathf.Lerp(-0.25f, 0.25f, Random.value), Mathf.Lerp(0.8f, 1f, 1f - Random.value * Random.value), Mathf.Lerp(0.45f, 1f, Random.value * Random.value));
            }
            Random.state = state;
        }

        public void Update()
        {
            lastRotationA = rotationA;
            lastRotationB = rotationB;
            for (int i = 0; i < pearlStrings.Count; i++)
            {
                pearlStrings[i].submersion = attachedTo.firstChunk.submersion;
                pearlStrings[i].lastPos = pearlStrings[i].pos;
                pearlStrings[i].pos = PearlAttachPos(i);
                for (int j = 0; j < pearlStrings[i].pearlPositions.Count; j++)
                {
                    Vector2 vector = pearlStrings[i].pos - pearlStrings[i].lastPos;
                    pearlStrings[i].pearlPositions[j] += vector * Custom.LerpQuadEaseIn(0.5f, 1f, Mathf.Lerp(0f, 1f, vector.magnitude / 30f));
                }
                pearlStrings[i].gravity = attachedTo.gravity;
                pearlStrings[i].Update();
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites[firstSprite] = new FSprite("pixel");
            sLeaser.sprites[firstSprite + 1] = new FSprite("pixel");
            sLeaser.sprites[firstSprite + 2] = new FSprite("pixel");
            if (King)
            {
                sLeaser.sprites[firstSprite + 3] = new FSprite("pixel");
                for (int i = 0; i < TotalSprites; i++)
                {
                    sLeaser.sprites[firstSprite + i].scale = 1.15f;
                }
            }
            for (int j = 0; j < pearlStrings.Count; j++)
            {
                pearlStrings[j].InitiateSprites(sLeaser, rCam);
            }
            AddToContainer(sLeaser, rCam, null);
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.zero;
            Vector2 v = Vector3.Slerp(lastRotationA, rotationA, timeStacker);
            Vector2 v2 = Vector3.Slerp(lastRotationB, rotationB, timeStacker);
            if (overrideRotationVector.HasValue)
            {
                v = overrideRotationVector.Value;
            }
            if (overrideAnchorVector.HasValue)
            {
                v2 = overrideAnchorVector.Value;
            }
            if (overrideDrawVector.HasValue)
            {
                pos = overrideDrawVector.Value;
            }
            else if (attachedTo != null)
            {
                pos = Vector2.Lerp(attachedTo.firstChunk.lastPos, attachedTo.firstChunk.pos, timeStacker);
            }
            float num = rCam.room.Darkness(pos) * (1f - rCam.room.LightSourceExposure(pos)) * 0.8f * (1f - fallOffAlcedoMode);
            float num2 = Custom.VecToDeg(v2);
            int num3 = Custom.IntClamp(Mathf.RoundToInt(Mathf.Abs(num2 / 180f) * 8f), 0, 8);
            float num4 = King ? 1.15f : 1f;
            for (int i = 0; i < (King ? 4 : 3); i++)
            {
                if (overrideSprite != null && overrideSprite != "")
                {
                    sLeaser.sprites[firstSprite + i].element = Futile.atlasManager.GetElementWithName(overrideSprite + num3);
                }
                else if (ScavKing)
                {
                    sLeaser.sprites[firstSprite + i].element = Futile.atlasManager.GetElementWithName("KingMask" + num3);
                }
                else
                {
                    sLeaser.sprites[firstSprite + i].element = Futile.atlasManager.GetElementWithName((i != 3 ? "AlcedoMaskA" : "KrakenArrow") + num3);
                }
                sLeaser.sprites[firstSprite + i].scaleX = Mathf.Sign(num2) * num4;
                sLeaser.sprites[firstSprite + i].anchorY = Custom.LerpMap(Mathf.Abs(num2), 0f, 100f, 0.5f, 0.675f, 2.1f);
                sLeaser.sprites[firstSprite + i].anchorX = 0.5f - v2.x * 0.1f * Mathf.Sign(num2);
                sLeaser.sprites[firstSprite + i].rotation = Custom.VecToDeg(v);
                sLeaser.sprites[firstSprite + i].x = pos.x - camPos.x;
                sLeaser.sprites[firstSprite + i].y = pos.y - camPos.y;
            }
            sLeaser.sprites[firstSprite + 1].scaleX *= 0.85f * num4;
            sLeaser.sprites[firstSprite + 1].scaleY = 0.9f * num4;
            sLeaser.sprites[firstSprite + 2].scaleY = 1.1f * num4;
            sLeaser.sprites[firstSprite + 2].anchorY += 0.015f;
            if (attachedTo is PlayerCarryableItem && (attachedTo as PlayerCarryableItem).blink > 0 && Random.value < 0.5f)
            {
                for (int j = 0; j < (!King ? 3 : 4); j++)
                {
                    sLeaser.sprites[firstSprite + j].color = new Color(1f, 1f, 1f);
                }
                return;
            }
            color = Color.Lerp(Color.Lerp(ColorA.rgb, new Color(1f, 1f, 1f), 0.35f * fallOffAlcedoMode), blackColor, Mathf.Lerp(0.2f, 1f, Mathf.Pow(num, 2f)));
            sLeaser.sprites[firstSprite].color = color;
            sLeaser.sprites[firstSprite + 1].color = Color.Lerp(color, blackColor, Mathf.Lerp(0.75f, 1f, num));
            sLeaser.sprites[firstSprite + 2].color = Color.Lerp(color, blackColor, Mathf.Lerp(0.75f, 1f, num));
            if (King)
            {
                sLeaser.sprites[firstSprite + 3].color = Color.Lerp(Color.Lerp(Color.Lerp(HSLColor.Lerp(ColorA, ColorB, 0.8f - 0.3f * fallOffAlcedoMode).rgb, blackColor, 0.53f), Color.Lerp(ColorA.rgb, new Color(1f, 1f, 1f), 0.35f), 0.1f), blackColor, 0.6f * num);
            }
            for (int k = 0; k < pearlStrings.Count; k++)
            {
                int num5 = k;
                if (Mathf.Sign(v2.x) < 0f)
                {
                    switch (num5)
                    {
                        case 0:
                            num5 = 1;
                            break;
                        case 1:
                            num5 = 0;
                            break;
                        case 2:
                            num5 = 3;
                            break;
                        case 3:
                            num5 = 2;
                            break;
                    }
                }
                pearlStrings[k].layer = stringLayers(SpriteIndex)[num5];
                pearlStrings[k].DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            blackColor = palette.blackColor;
            for (int i = 0; i < pearlStrings.Count; i++)
            {
                pearlStrings[i].ApplyPalette(sLeaser, rCam, palette);
            }
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Items");
            }
            for (int i = 0; i < TotalSprites; i++)
            {
                sLeaser.sprites[firstSprite + i].RemoveFromContainer();
            }
            newContatiner.AddChild(sLeaser.sprites[firstSprite + 2]);
            newContatiner.AddChild(sLeaser.sprites[firstSprite + 1]);
            newContatiner.AddChild(sLeaser.sprites[firstSprite]);
            if (King)
            {
                newContatiner.AddChild(sLeaser.sprites[firstSprite + 3]);
            }
            for (int j = 0; j < pearlStrings.Count; j++)
            {
                pearlStrings[j].AddToContainer(sLeaser, rCam, newContatiner);
            }
        }

        public void SetVisible(RoomCamera.SpriteLeaser sLeaser, bool visible)
        {
            for (int i = 0; i < TotalSprites; i++)
            {
                sLeaser.sprites[firstSprite + i].isVisible = visible;
            }
        }

        public Vector2 PearlAttachPos(int i)
        {
            Vector2 vector = Vector2.zero;
            if (overrideDrawVector.HasValue)
            {
                vector = overrideDrawVector.Value;
            }
            else if (attachedTo != null)
            {
                vector = attachedTo.firstChunk.pos;
            }
            Vector2 value = rotationB;
            if (overrideAnchorVector.HasValue)
            {
                value = overrideAnchorVector.Value;
            }
            Vector2 value2 = rotationA;
            if (overrideRotationVector.HasValue)
            {
                value2 = overrideRotationVector.Value;
            }
            int num = i;
            if (Mathf.Sign(value.x) < 0f)
            {
                switch (num)
                {
                    case 0:
                        num = 1;
                        break;
                    case 1:
                        num = 0;
                        break;
                    case 2:
                        num = 3;
                        break;
                    case 3:
                        num = 2;
                        break;
                }
            }
            Vector2 vector2 = stringOffsets(SpriteIndex)[num];
            float f = (float)Math.PI / 180f * (0f - Custom.VecToDeg(value2)) * Mathf.Sign(value.x);
            Vector2 vector3 = new Vector2(vector2.x * Mathf.Cos(f) - vector2.y * Mathf.Sin(f), vector2.x * Mathf.Sin(f) + vector2.y * Mathf.Cos(f));
            return vector + new Vector2(vector3.x * Mathf.Sign(value.x), vector3.y);
        }

        public Vector2[] stringOffsets(int ind)
        {
            return ind switch
            {
                0 => new Vector2[4]
                {
                new Vector2(-27f, 13f),
                new Vector2(22f, 13f),
                new Vector2(-15f, 25f),
                new Vector2(12f, 25f)
                },
                1 => new Vector2[4]
                {
                new Vector2(-20f, 9f),
                new Vector2(19f, 12f),
                new Vector2(-13f, 25f),
                new Vector2(9f, 23f)
                },
                2 => new Vector2[4]
                {
                new Vector2(0f, 0f),
                new Vector2(24f, 6f),
                new Vector2(-5f, 20f),
                new Vector2(14f, 20f)
                },
                3 => new Vector2[4]
                {
                new Vector2(0f, 0f),
                new Vector2(20f, 0f),
                new Vector2(-6f, 20f),
                new Vector2(15f, 19f)
                },
                4 => new Vector2[4]
                {
                new Vector2(0f, 0f),
                new Vector2(21f, -2f),
                new Vector2(11f, 12f),
                new Vector2(16f, 10f)
                },
                5 => new Vector2[4]
                {
                new Vector2(0f, 0f),
                new Vector2(19f, -19f),
                new Vector2(22f, -3f),
                new Vector2(22f, -5f)
                },
                6 => new Vector2[4]
                {
                new Vector2(0f, 0f),
                new Vector2(17f, -20f),
                new Vector2(21f, -7f),
                new Vector2(0f, 0f)
                },
                7 => new Vector2[4]
                {
                new Vector2(0f, 0f),
                new Vector2(26f, 4f),
                new Vector2(10f, 15f),
                new Vector2(19f, 15f)
                },
                _ => new Vector2[4]
                {
                new Vector2(-21f, 2f),
                new Vector2(19f, 2f),
                new Vector2(-14f, 8f),
                new Vector2(12f, 8f)
                },
            };
        }

        public int[] stringLayers(int ind)
        {
            return ind switch
            {
                0 => new int[4] { 1, 1, 1, 1 },
                1 => new int[4] { 1, 1, 1, 1 },
                2 => new int[4] { 0, 1, -1, 1 },
                3 => new int[4] { 0, 1, -1, -1 },
                4 => new int[4] { 0, 1, -1, -1 },
                5 => new int[4] { 0, 1, -1, -1 },
                6 => new int[4] { 0, 1, -1, 0 },
                7 => new int[4] { 0, 1, -1, -1 },
                _ => new int[4] { 1, 1, -1, -1 },
            };
        }
    }
}
