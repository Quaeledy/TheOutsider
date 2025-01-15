using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature.Alcedo
{
    internal class AlcedoLeg : Tentacle
    {
        public Alcedo alcedo
        {
            get
            {
                return owner as Alcedo;
            }
        }
        public float tentacleDir
        {
            get
            {
                if (tentacleNumber == 0)
                {
                    return -1f;
                }
                if (!alcedo.IsMiros)
                {
                    return 1f;
                }
                if (tentacleNumber == 1)
                {
                    return 1f;
                }
                if (tentacleNumber == 2)
                {
                    return -4f;
                }
                return 4f;
            }
        }
        public bool hasAnyGrip
        {
            get
            {
                return attachedAtClaw || segmentsGrippingTerrain > 0;
            }
        }
        public float flyingMode
        {
            get
            {
                return fm;
            }
            set
            {
                fm = Mathf.Clamp(value, 0f, 1f);
            }
        }
        private AlcedoLeg OtherTentacle
        {
            get
            {
                if (!alcedo.IsMiros)
                {
                    return alcedo.legs[1 - tentacleNumber];
                }
                if (tentacleNumber % 2 == 0)
                {
                    return alcedo.legs[tentacleNumber + 1];
                }
                return alcedo.legs[tentacleNumber - 1];
            }
        }
        public int halfWingIndex => Mathf.FloorToInt(tChunks.Length / 2f);

        public float TentacleContour(float x)
        {
            float num = Mathf.Lerp(0.45f, 0.3f, flyingMode);
            float num2 = Mathf.Lerp(0.51f, 0.45f, flyingMode);
            float num3 = Mathf.Lerp(0.85f, 0.6f, flyingMode);

            float num4 = Mathf.Lerp(6.5f, 5.5f, flyingMode);
            float num5 = Mathf.Lerp(1.2f, 1.15f, flyingMode);
            float num6 = Mathf.Lerp(0.85f, 0.7f, flyingMode);
            float num7 = num6 + (1f - num6) * Mathf.Cos(Mathf.InverseLerp(num2, 1.2f, x) * 3.1415927f * 0.5f);

            float num8 = alcedo.IsKing ? 1.2f : 1f;
            float rad = 0f;
            if (x < num)
                rad = 2f * num4 * num5 * num8;
            if (x < num2)
                rad = num4 * Mathf.Lerp(num5, 1f, Custom.SCurve(Mathf.InverseLerp(num, num2, x), 0.1f)) * num8;
            if (x < num3)
                rad = num4 * num7 * num8;
            else
                rad = 1.2f * num4 * Mathf.Lerp(0.5f, 1f, Mathf.Cos(Mathf.Pow(Mathf.InverseLerp(num3, 1f, x), 4f) * 3.1415927f * 0.5f)) * num7 * num8;
            return rad;
        }

        public AlcedoLeg(Alcedo alcedo, BodyChunk chunk, BodyChunk otherTentacleChunk, float length, int tentacleNumber) : base(alcedo, chunk, length)
        {
            this.tentacleNumber = tentacleNumber;
            tProps = new TentacleProps(false, false, true, 0.5f, 0f, 0.2f, 1.2f, 0.2f, 1.2f, 10f, 0.25f, 5f, 15, 60, 12, 0);
            tChunks = new TentacleChunk[alcedo.IsKing ? 7 : 5];
            for (int i = 0; i < tChunks.Length; i++)
            {
                tChunks[i] = new TentacleChunk(this, i, (i + 1) / (float)tChunks.Length, 5f);
            }
            debugViz = false;
            this.otherTentacleChunk = otherTentacleChunk;
        }

        public override void NewRoom(Room room)
        {
            base.NewRoom(room);
            wooshSound = new StaticSoundLoop(SoundID.Vulture_Wing_Woosh_LOOP, tChunks[tChunks.Length - 1].pos, room, 1f, 1f);
            if (debugViz)
            {
                if (grabGoalSprites != null)
                {
                    grabGoalSprites[0].RemoveFromRoom();
                    grabGoalSprites[1].RemoveFromRoom();
                }
                grabGoalSprites = new DebugSprite[2];
                grabGoalSprites[0] = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel", true), room);
                grabGoalSprites[0].sprite.scale = 10f;
                grabGoalSprites[0].sprite.color = new Color(1f, 0f, 0f);
                grabGoalSprites[1] = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel", true), room);
                grabGoalSprites[1].sprite.scale = 10f;
                grabGoalSprites[1].sprite.color = new Color(0f, 5f, 0f);
            }
        }

        public override void Update()
        {
            base.Update();
            if (alcedo.enteringShortCut != null)
            {
                retractFac = Mathf.Min(0f, retractFac - 0.1f);
                for (int i = 0; i < tChunks.Length; i++)
                {
                    tChunks[i].vel += Vector2.ClampMagnitude(room.MiddleOfTile(alcedo.enteringShortCut.Value) - tChunks[i].pos, 50f) / 10f;
                }
                if (segments.Count > 1)
                {
                    segments.RemoveAt(segments.Count - 1);
                }
                return;
            }
            attachedAtClaw = false;
            idealLength = Mathf.Lerp(alcedo.legLength * 7f,
                                     alcedo.legLength * 5f,
                                     flyingMode);/*
            idealLength = Mathf.Lerp(this.alcedo.wingLength * (this.alcedo.IsKing ? 9f : 7f),
                                     this.alcedo.wingLength * (this.alcedo.IsKing ? 13.5f : 11f),
                                     flyingMode);*/
            if (stun > 0)
            {
                stun--;
            }
            if (Mathf.Pow(UnityEngine.Random.value, 0.25f) > 2f * (alcedo.State as Alcedo.AlcedoState).wingHealth[tentacleNumber])
            {
                stun = Math.Max(stun, (int)Mathf.Lerp(-2f, 12f, Mathf.Pow(UnityEngine.Random.value, 0.5f + 20f * Mathf.Max(0f, (alcedo.State as Alcedo.AlcedoState).wingHealth[tentacleNumber]))));
            }
            //注意，这里比翅膀多了个在飞行时无力
            limp = !alcedo.Consious || stun > 0 || alcedo.AirBorne;
            if (limp)
            {
                floatGrabDest = null;
                for (int j = 0; j < tChunks.Length; j++)
                {
                    tChunks[j].vel *= 0.9f;
                    if (alcedo.AirBorne)
                    {
                        TentacleChunk tentacleChunk = tChunks[j];
                        tentacleChunk.vel.y = tentacleChunk.vel.y - 0.5f;
                        tentacleChunk.vel += Custom.DirVec(alcedo.bodyChunks[0].pos, alcedo.bodyChunks[5].pos);
                    }
                }
            }
            for (int k = 0; k < tChunks.Length; k++)
            {
                tChunks[k].rad = TentacleContour(tChunks[k].tPos);// idealLength / (2f * this.tChunks.Length - 2f);//
                if (backtrackFrom == -1 || k < backtrackFrom)
                {
                    if (k > 1 && Custom.DistLess(tChunks[k].pos, tChunks[k - 2].pos, 30f))
                    {
                        tChunks[k].vel -= Custom.DirVec(tChunks[k].pos, tChunks[k - 2].pos) * (30f - Vector2.Distance(tChunks[k].pos, tChunks[k - 2].pos)) * 0.1f;
                    }
                    else if (k <= 1)
                    {
                        tChunks[k].vel = Custom.DirVec(OtherTentacle.connectedChunk.pos, connectedChunk.pos) * (k == 0 ? 2f : 1.2f) * (alcedo.AirBorne ? 1f : 0.1f);
                        //this.tChunks[k].vel = Custom.DirVec(this.OtherTentacle.connectedChunk.pos, this.connectedChunk.pos) * ((k == 0) ? 2f : 1.2f);
                    }
                }
                if (room.PointSubmerged(tChunks[k].pos))
                {
                    tChunks[k].vel *= 0.5f;
                }
                if (tChunks[k].contactPoint.x != 0 && tChunks[k].lastContactPoint.x == 0 && Mathf.Abs(tChunks[k].pos.x - tChunks[k].lastPos.x) > 6f)
                {
                    room.PlaySound(SoundID.Vulture_Tentacle_Collide_Terrain, tChunks[k].pos, Mathf.InverseLerp(6f, 16f, Mathf.Abs(tChunks[k].pos.x - tChunks[k].lastPos.x)), 1f);
                }
                else if (tChunks[k].contactPoint.y != 0 && tChunks[k].lastContactPoint.y == 0 && Mathf.Abs(tChunks[k].pos.y - tChunks[k].lastPos.y) > 6f)
                {
                    room.PlaySound(SoundID.Vulture_Tentacle_Collide_Terrain, tChunks[k].pos, Mathf.InverseLerp(6f, 16f, Mathf.Abs(tChunks[k].pos.y - tChunks[k].lastPos.y)), 1f);
                }
            }
            if (!limp)
            {
                if (floatGrabDest != null && Custom.DistLess(tChunks[tChunks.Length - 1].pos, floatGrabDest.Value, 2f * alcedo.legLength) && backtrackFrom == -1)
                {
                    tChunks[tChunks.Length - 1].pos = floatGrabDest.Value;
                    tChunks[tChunks.Length - 1].vel *= 0f;
                    attachedAtClaw = true;
                }
                flyingMode -= 0.025f;
                tChunks[tChunks.Length - 1].collideWithTerrain = !attachedAtClaw;
                UpdateDesiredGrabPos();
                bool flag = tentacleNumber % 2 == 0;
                BodyChunk bodyChunk = flag ? alcedo.bodyChunks[8] : alcedo.bodyChunks[7];
                segmentsGrippingTerrain = 0;
                for (int l = 0; l < tChunks.Length - 1; l++)
                {
                    tChunks[l].vel *= Mathf.Lerp(0.95f, 0.85f, Support());
                    if (attachedAtClaw && (backtrackFrom == -1 || l < backtrackFrom) && GripTerrain(l))
                    {
                        segmentsGrippingTerrain++;
                        for (int m = l - 1; m > 0; m--)
                        {
                            PushChunksApart(l, m);
                        }
                    }
                    else
                    {
                        TentacleChunk tentacleChunk2 = tChunks[l];
                        tentacleChunk2.vel.y = tentacleChunk2.vel.y + 0.1f;
                        tChunks[l].vel += connectedChunk.vel * 0.02f;
                        //this.tChunks[l].vel += this.connectedChunk.vel * 0.1f;
                        if (!hasAnyGrip)
                        {
                            if (floatGrabDest != null)
                            {
                                tChunks[l].vel += Custom.DirVec(tChunks[l].pos, floatGrabDest.Value) * 0.3f;
                            }
                            else
                            {
                                tChunks[l].vel += Custom.DirVec(tChunks[l].pos, desiredGrabPos + Custom.DirVec(FloatBase, desiredGrabPos) * 70f * alcedo.legLength / 20f) * 0.6f;
                            }
                        }
                    }
                    tChunks[l].vel += Custom.DirVec(bodyChunk.pos, tChunks[l].pos) * 0.5f / (l + 1f);
                }
                if (attachedAtClaw)
                {
                    framesWithoutReaching = 0;
                    if (SharedPhysics.RayTraceTilesForTerrain(room, BasePos, grabDest.Value))
                    {
                        if (!Custom.DistLess(tChunks[tChunks.Length - 1].pos, connectedChunk.pos, idealLength))
                        {
                            Vector2 a = Custom.DirVec(tChunks[tChunks.Length - 1].pos, connectedChunk.pos);
                            float num = Vector2.Distance(tChunks[tChunks.Length - 1].pos, connectedChunk.pos);
                            float num2 = idealLength * 0.9f;
                            connectedChunk.pos += a * (num2 - num) * 0.11f;
                            connectedChunk.vel += a * (num2 - num) * 0.11f;
                            otherTentacleChunk.pos += a * (num2 - num) * 0.09f;//设置this.otherTentacleChunk是为了让身体平衡
                            otherTentacleChunk.vel += a * (num2 - num) * 0.09f;
                            /*
                            for (int i = 0; i < this.alcedo.tentacles.Length; i++)
                            {
                                this.alcedo.tentacles[i].connectedChunk.pos += a * (num2 - num) * 0.05f;//为了让身体平衡
                            }*/
                        }
                        if (!Custom.DistLess(tChunks[tChunks.Length - 1].pos, connectedChunk.pos, idealLength * 0.9f))
                        {
                            alcedo.hangingInLeg = true;
                        }
                    }
                    if (playGrabSound)
                    {
                        room.PlaySound(SoundID.Vulture_Tentacle_Grab_Terrain, tChunks[tChunks.Length - 1].pos);
                        playGrabSound = false;
                    }
                }
                else
                {
                    playGrabSound = true;
                    FindGrabPos(ref scratchPath);
                    framesWithoutReaching++;
                    if (framesWithoutReaching > 60f && floatGrabDest == null)
                    {
                        framesWithoutReaching = 0;
                        SwitchMode(Mode.Fly);
                    }
                }
            }
            wooshSound.volume = Custom.SCurve(Mathf.InverseLerp(0.4f, 18f, Vector2.Distance(tChunks[tChunks.Length - 1].pos - connectedChunk.pos, tChunks[tChunks.Length - 1].lastPos - connectedChunk.lastPos)), 0.6f) * flyingMode;
            wooshSound.pitch = Mathf.Lerp(0.3f, 1.7f, Mathf.InverseLerp(-20f, 20f, tChunks[tChunks.Length - 1].lastPos.y - tChunks[tChunks.Length - 1].pos.y - (connectedChunk.lastPos.y - connectedChunk.pos.y)));
            wooshSound.pos = Vector2.Lerp(connectedChunk.pos, tChunks[tChunks.Length - 1].pos, 0.7f);
            wooshSound.Update();
            if (debugViz)
            {
                grabGoalSprites[1].pos = desiredGrabPos;
            }
        }

        public void SwitchMode(Mode newMode)
        {
            if (newMode == Mode.Fly)
            {
                if (alcedo.IsMiros)
                {
                    ReleaseGrip();
                }
                floatGrabDest = null;
            }
        }

        public void ReleaseGrip()
        {
            if (OtherTentacle.grabDelay < 1)
            {
                grabDelay = 10;
            }
            floatGrabDest = null;
        }

        private void UpdateDesiredGrabPos()
        {
            if (alcedo.hoverStill)
            {
                desiredGrabPos = alcedo.mainBodyChunk.pos + new Vector2(tentacleDir, -0.8f).normalized * idealLength * 0.7f;
                return;
            }
            desiredGrabPos = alcedo.mainBodyChunk.pos +
                (Vector2)Vector3.Slerp(alcedo.moveDirection, new Vector2(tentacleDir, -0.8f).normalized, 0.3f) * idealLength * 0.7f;
            /*
            if (this.alcedo.hoverStill)
            {
                this.desiredGrabPos = this.alcedo.mainBodyChunk.pos + new Vector2(this.tentacleDir, -0.8f).normalized * this.idealLength * 0.7f;
                return;
            }
            this.desiredGrabPos = this.alcedo.mainBodyChunk.pos + Vector3.Slerp(this.alcedo.moveDirection, new Vector2(this.tentacleDir, -0.8f).normalized, 0.3f).ToVector2InPoints() * this.idealLength * 0.7f;*/
        }

        private void FindGrabPos(ref List<IntVector2> path)
        {
            if (grabDelay > 0)
            {
                grabDelay--;
                return;
            }
            IntVector2? intVector = ClosestSolid(room.GetTilePosition(desiredGrabPos), 8, 8f);
            //IntVector2? intVector = this.ClosestSolid(this.room.GetTilePosition(this.desiredGrabPos), Mathf.FloorToInt(8 * this.alcedo.legLength / 20f), 8f * this.alcedo.legLength / 20f);
            if (intVector != null)
            {
                IntVector2? intVector2 = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, BasePos, intVector.Value);
                if (grabDest == null || GripPointAttractiveness(intVector2.Value) > GripPointAttractiveness(grabDest.Value))
                {
                    Vector2 newGrabDest = Custom.RestrictInRect(FloatBase,
                                                                FloatRect.MakeFromVector2(room.MiddleOfTile(intVector2.Value) - new Vector2(11f, 11f),
                                                                                          room.MiddleOfTile(intVector2.Value) + new Vector2(11f, 11f)));
                    MoveGrabDest(newGrabDest, ref path);
                }
            }
            Vector2 pos = desiredGrabPos + Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * idealLength;
            int num = room.RayTraceTilesList(BasePos.x, BasePos.y, room.GetTilePosition(pos).x, room.GetTilePosition(pos).y, ref path);
            int num2 = 0;
            while (num2 < num && !room.GetTile(path[num2]).Solid)
            {
                if ((room.GetTile(path[num2]).horizontalBeam || room.GetTile(path[num2]).verticalBeam) && (grabDest == null || GripPointAttractiveness(path[num2]) > GripPointAttractiveness(grabDest.Value)))
                {
                    MoveGrabDest(room.MiddleOfTile(path[num2]), ref path);
                    return;
                }
                num2++;
            }
        }

        public float ReleaseScore()
        {
            if (alcedo.AirBorne)
            {
                return float.MinValue;
            }
            float num = Vector2.Distance(tChunks[tChunks.Length - 1].pos, desiredGrabPos);
            if (floatGrabDest == null)
            {
                num *= 2f;
            }
            return num;
        }

        private bool GripTerrain(int chunk)
        {
            for (int i = 0; i < 4; i++)
            {
                if (room.GetTile(room.GetTilePosition(tChunks[chunk].pos) + Custom.fourDirections[i]).Solid)
                {
                    tChunks[chunk].vel *= 0.25f;
                    tChunks[chunk].vel += Custom.fourDirections[i].ToVector2() * 0.8f;
                    return tChunks[chunk].contactPoint.x != 0 || tChunks[chunk].contactPoint.y != 0;
                }
            }
            if (room.GetTile(tChunks[chunk].pos).horizontalBeam)
            {
                tChunks[chunk].vel *= 0.25f;
                TentacleChunk tentacleChunk = tChunks[chunk];
                tentacleChunk.vel.y = tentacleChunk.vel.y + (room.MiddleOfTile(tChunks[chunk].pos).y - tChunks[chunk].pos.y) * 0.3f;
                return true;
            }
            if (room.GetTile(tChunks[chunk].pos).verticalBeam)
            {
                tChunks[chunk].vel *= 0.25f;
                TentacleChunk tentacleChunk2 = tChunks[chunk];
                tentacleChunk2.vel.x = tentacleChunk2.vel.x + (room.MiddleOfTile(tChunks[chunk].pos).x - tChunks[chunk].pos.x) * 0.3f;
                return true;
            }
            return false;
        }

        private float GripPointAttractiveness(IntVector2 pos)
        {
            if (room.GetTile(pos).Solid)
            {
                return 100f / room.GetTilePosition(desiredGrabPos).FloatDist(pos);
            }
            return 65f / room.GetTilePosition(desiredGrabPos).FloatDist(pos);
        }

        public float Support()
        {
            if (stun > 0)
            {
                return 0f;
            }
            if (!alcedo.AirBorne)
            {
                return Mathf.Clamp((hasAnyGrip ? 0.5f : 0f) + segmentsGrippingTerrain / (float)tChunks.Length, 0f, 1f);
                //return Mathf.Clamp((this.hasAnyGrip ? (this.alcedo.IsMiros ? 4f : 0.5f) : 0f) + (float)this.segmentsGrippingTerrain / (float)this.tChunks.Length, 0f, 1f);
            }
            if (!alcedo.IsMiros)
            {
                return 0.5f;
            }
            return 1.2f;
        }

        private IntVector2? ClosestSolid(IntVector2 goal, int maxDistance, float maxDistFromBase)
        {
            if (room.GetTile(goal).Solid)
            {
                return new IntVector2?(goal);
            }
            for (int i = 1; i <= maxDistance; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (room.GetTile(goal + Custom.eightDirections[j] * i).Solid && BasePos.FloatDist(goal + Custom.eightDirections[j] * i) < maxDistFromBase)
                    {
                        return new IntVector2?(goal + Custom.eightDirections[j] * i);
                    }
                }
            }
            return null;
        }

        public bool WingSpace()
        {
            for (int i = -1; i <= 1; i++)
            {
                if (!SharedPhysics.RayTraceTilesForTerrain(room, room.GetTilePosition(connectedChunk.pos), room.GetTilePosition(connectedChunk.pos + new Vector2(tentacleDir * idealLength, 50f * i))))
                {
                    return false;
                }
            }
            return true;
        }

        public override IntVector2 GravityDirection()
        {
            if (UnityEngine.Random.value >= 0.5f)
            {
                return new IntVector2(0, -1);
            }
            return new IntVector2((int)tentacleDir, -1);
        }

        public void Damage(Creature.DamageType type, float damage, float stunBonus)
        {
            damage /= 2.7f;
            stunBonus /= 1.2f;
            stun = Math.Max(stun, (int)(damage * 30f + stunBonus));
            (alcedo.State as Alcedo.AlcedoState).wingHealth[tentacleNumber] -= damage;
        }

        public float Wave(int n)
        {
            return Mathf.Sin((float)Math.PI * 2f * (alcedo.wingFlap - tChunks[n].tPos * 0.5f));
        }

        private DebugSprite[] grabGoalSprites;
        public int tentacleNumber;
        public Vector2 desiredGrabPos;
        private bool attachedAtClaw;
        public int segmentsGrippingTerrain;
        public int framesWithoutReaching;
        public int otherTentacleIsFlying;
        public int grabDelay;
        public int framesOfHittingTerrain;
        public bool playGrabSound;
        public int stun;
        private float fm;
        public StaticSoundLoop wooshSound;
        //private List<IntVector2> scratchPath;
        public BodyChunk otherTentacleChunk;

        public class Mode : ExtEnum<Mode>
        {
            public Mode(string value, bool register = false) : base(value, register)
            {
            }
            public static readonly Mode Climb = new Mode("Climb", true);
            public static readonly Mode Fly = new Mode("Fly", true);
        }
    }
}
