using RWCustom;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature
{
    public class Limb : BodyPart
    {
        public class Mode : ExtEnum<Mode>
        {
            public static readonly Mode HuntRelativePosition = new Mode("HuntRelativePosition", register: true);

            public static readonly Mode HuntAbsolutePosition = new Mode("HuntAbsolutePosition", register: true);

            public static readonly Mode Retracted = new Mode("Retracted", register: true);

            public static readonly Mode Dangle = new Mode("Dangle", register: true);

            public Mode(string value, bool register = false)
                : base(value, register)
            {
            }
        }

        public BodyChunk connection;

        public int limbNumber;

        protected float defaultHuntSpeed;

        public float huntSpeed;

        public Vector2 relativeHuntPos;

        public Vector2 absoluteHuntPos;

        protected float defaultQuickness;

        public float quickness;

        public bool reachedSnapPosition;

        public bool retract;

        public bool pushOutOfTerrain = true;

        public Mode mode { get; set; }

        public bool OverLappingHuntPos
        {
            get
            {
                if (reachedSnapPosition)
                {
                    return true;
                }
                return Custom.DistLess(pos, absoluteHuntPos, rad + 1f);
            }
        }

        public Limb(GraphicsModule owner, BodyChunk connection, int num, float rad, float sfFric, float aFric, float defaultHuntSpeed, float defaultQuickness)
            : base(owner)
        {
            base.rad = rad;
            this.connection = connection;
            surfaceFric = sfFric;
            airFriction = aFric;
            limbNumber = num;
            mode = Mode.HuntRelativePosition;
            this.defaultHuntSpeed = defaultHuntSpeed;
            huntSpeed = defaultHuntSpeed;
            this.defaultQuickness = defaultQuickness;
            quickness = defaultQuickness;
            Reset(connection.pos);
            absoluteHuntPos = pos;
        }

        public override void Update()
        {
            lastPos = pos;
            if (retract && mode != Mode.Retracted)
            {
                mode = Mode.HuntAbsolutePosition;
                absoluteHuntPos = connection.pos;
                if (Custom.DistLess(absoluteHuntPos, pos, huntSpeed))
                {
                    mode = Mode.Retracted;
                }
            }
            if (mode == Mode.HuntRelativePosition)
            {
                absoluteHuntPos = connection.pos + Custom.RotateAroundOrigo(relativeHuntPos, Custom.AimFromOneVectorToAnother(connection.rotationChunk.pos, connection.pos));
            }
            if (mode == Mode.HuntRelativePosition || mode == Mode.HuntAbsolutePosition)
            {
                if (Custom.DistLess(absoluteHuntPos, pos, huntSpeed))
                {
                    vel = absoluteHuntPos - pos;
                    reachedSnapPosition = true;
                }
                else
                {
                    vel = Vector2.Lerp(vel, Custom.DirVec(pos, absoluteHuntPos) * huntSpeed, quickness);
                    reachedSnapPosition = false;
                }
            }
            else if (mode == Mode.Retracted)
            {
                vel = connection.vel;
                pos = connection.pos;
                reachedSnapPosition = true;
            }
            else if (mode == Mode.Dangle)
            {
                reachedSnapPosition = false;
            }
            quickness = defaultQuickness;
            huntSpeed = defaultHuntSpeed;
            if (mode != Mode.Retracted)
            {
                pos += vel;
                if (mode == Mode.HuntRelativePosition)
                {
                    pos += connection.vel;
                }
                vel *= airFriction;
                if (pushOutOfTerrain)
                {
                    PushOutOfTerrain(owner.owner.room, connection.pos);
                }
            }
        }

        public override void Reset(Vector2 resetPoint)
        {
            base.Reset(resetPoint);
        }

        public void FindGrip(Room room, Vector2 attachedPos, Vector2 searchFromPos, float maximumRadiusFromAttachedPos, Vector2 goalPos, int forbiddenXDirs, int forbiddenYDirs, bool behindWalls)
        {
            if (!Custom.DistLess(attachedPos, searchFromPos, maximumRadiusFromAttachedPos))
            {
                searchFromPos = attachedPos + Custom.DirVec(attachedPos, searchFromPos) * (maximumRadiusFromAttachedPos - 1f);
            }
            if (!Custom.DistLess(attachedPos, goalPos, maximumRadiusFromAttachedPos))
            {
                goalPos = attachedPos + Custom.DirVec(attachedPos, goalPos) * maximumRadiusFromAttachedPos;
            }
            IntVector2 tilePosition = room.GetTilePosition(searchFromPos);
            Vector2 vector = new Vector2(-100000f, -100000f);
            for (int i = 0; i < 9; i++)
            {
                if (Custom.eightDirectionsAndZero[i].x == forbiddenXDirs || Custom.eightDirectionsAndZero[i].y == forbiddenYDirs)
                {
                    continue;
                }
                Vector2 vector2 = room.MiddleOfTile(tilePosition + Custom.eightDirectionsAndZero[i]);
                Vector2 vector3 = new Vector2(Mathf.Clamp(goalPos.x, vector2.x - 10f, vector2.x + 10f), Mathf.Clamp(goalPos.y, vector2.y - 10f, vector2.y + 10f));
                if (room.GetTile(tilePosition + Custom.eightDirectionsAndZero[i]).Terrain == Room.Tile.TerrainType.Solid)
                {
                    if (Custom.eightDirectionsAndZero[i].x != 0 && room.GetTile(tilePosition + Custom.eightDirectionsAndZero[i] + new IntVector2(-Custom.eightDirectionsAndZero[i].x, 0)).Terrain != Room.Tile.TerrainType.Solid)
                    {
                        vector3.x = vector2.x - (float)Custom.eightDirectionsAndZero[i].x * 10f;
                    }
                    if (Custom.eightDirectionsAndZero[i].y != 0 && room.GetTile(tilePosition + Custom.eightDirectionsAndZero[i] + new IntVector2(0, -Custom.eightDirectionsAndZero[i].y)).Terrain != Room.Tile.TerrainType.Solid)
                    {
                        vector3.y = vector2.y - (float)Custom.eightDirectionsAndZero[i].y * 10f;
                    }
                    if (Custom.DistNoSqrt(goalPos, vector3) < Custom.DistNoSqrt(goalPos, vector) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                    {
                        vector = vector3;
                    }
                }
                else if (room.GetTile(tilePosition + Custom.eightDirectionsAndZero[i]).Terrain == Room.Tile.TerrainType.Floor)
                {
                    vector3.y = vector2.y + 10f;
                    if (Custom.DistNoSqrt(goalPos, vector3) < Custom.DistNoSqrt(goalPos, vector) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                    {
                        vector = vector3;
                    }
                }
                else if (room.GetTile(tilePosition + Custom.eightDirectionsAndZero[i]).Terrain == Room.Tile.TerrainType.Slope)
                {
                    if (room.IdentifySlope(tilePosition + Custom.eightDirectionsAndZero[i]) == Room.SlopeDirection.DownLeft || room.IdentifySlope(tilePosition + Custom.eightDirectionsAndZero[i]) == Room.SlopeDirection.UpRight)
                    {
                        vector3.y = vector2.y + 10f - (vector3.x - (vector2.x - 10f));
                    }
                    else if (room.IdentifySlope(tilePosition + Custom.eightDirectionsAndZero[i]) == Room.SlopeDirection.DownRight || room.IdentifySlope(tilePosition + Custom.eightDirectionsAndZero[i]) == Room.SlopeDirection.UpLeft)
                    {
                        vector3.y = vector2.y - 10f + (vector3.x - (vector2.x - 10f));
                    }
                    if (Custom.DistNoSqrt(goalPos, vector3) < Custom.DistNoSqrt(goalPos, vector) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                    {
                        vector = vector3;
                    }
                }
                else if (room.GetTile(tilePosition + Custom.eightDirectionsAndZero[i]).Terrain == Room.Tile.TerrainType.Air && behindWalls && room.GetTile(tilePosition + Custom.eightDirectionsAndZero[i]).wallbehind && vector == new Vector2(-100000f, -100000f) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                {
                    vector = vector3;
                }
                if (room.GetTile(tilePosition + Custom.eightDirectionsAndZero[i]).horizontalBeam)
                {
                    vector3 = new Vector2(Mathf.Clamp(goalPos.x, vector2.x - 10f, vector2.x + 10f), vector2.y);
                    if (Custom.DistNoSqrt(goalPos, vector3) < Custom.DistNoSqrt(goalPos, vector) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                    {
                        vector = vector3;
                    }
                }
                if (room.GetTile(tilePosition + Custom.eightDirectionsAndZero[i]).verticalBeam)
                {
                    vector3 = new Vector2(vector2.x, Mathf.Clamp(goalPos.y, vector2.y - 10f, vector2.y + 10f));
                    if (Custom.DistNoSqrt(goalPos, vector3) < Custom.DistNoSqrt(goalPos, vector) && Custom.DistLess(attachedPos, vector3, maximumRadiusFromAttachedPos))
                    {
                        vector = vector3;
                    }
                }
            }
            if (vector.x != -100000f && vector.y != -100000f)
            {
                mode = Mode.HuntAbsolutePosition;
                absoluteHuntPos = vector;
                GrabbedTerrain();
            }
        }

        public virtual void GrabbedTerrain()
        {
        }
    }
}
