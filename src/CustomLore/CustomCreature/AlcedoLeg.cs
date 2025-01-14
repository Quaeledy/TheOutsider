using RWCustom;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature
{
    internal class AlcedoLeg : Tentacle
    {
        public Alcedo alcedo
        {
            get
            {
                return this.owner as Alcedo;
            }
        }
        public float tentacleDir
        {
            get
            {
                if (this.tentacleNumber == 0)
                {
                    return -1f;
                }
                if (!this.alcedo.IsMiros)
                {
                    return 1f;
                }
                if (this.tentacleNumber == 1)
                {
                    return 1f;
                }
                if (this.tentacleNumber == 2)
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
                return this.attachedAtClaw || this.segmentsGrippingTerrain > 0;
            }
        }
        public float flyingMode
        {
            get
            {
                return this.fm;
            }
            set
            {
                this.fm = Mathf.Clamp(value, 0f, 1f);
            }
        }
        private AlcedoLeg OtherTentacle
        {
            get
            {
                if (!this.alcedo.IsMiros)
                {
                    return this.alcedo.legs[1 - this.tentacleNumber];
                }
                if (this.tentacleNumber % 2 == 0)
                {
                    return this.alcedo.legs[this.tentacleNumber + 1];
                }
                return this.alcedo.legs[this.tentacleNumber - 1];
            }
        }
        public int halfWingIndex => Mathf.FloorToInt((float)this.tChunks.Length / 2f);

        public float TentacleContour(float x)
        {
            float num = Mathf.Lerp(0.45f, 0.3f, this.flyingMode);
            float num2 = Mathf.Lerp(0.51f, 0.45f, this.flyingMode);
            float num3 = Mathf.Lerp(0.85f, 0.6f, this.flyingMode);

            float num4 = Mathf.Lerp(6.5f, 5.5f, this.flyingMode);
            float num5 = Mathf.Lerp(1.2f, 1.15f, this.flyingMode);
            float num6 = Mathf.Lerp(0.85f, 0.7f, this.flyingMode);
            float num7 = num6 + (1f - num6) * Mathf.Cos(Mathf.InverseLerp(num2, 1.2f, x) * 3.1415927f * 0.5f);

            float num8 = this.alcedo.IsKing ? 1.2f : 1f;
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
            this.tProps = new Tentacle.TentacleProps(false, false, true, 0.5f, 0f, 0.2f, 1.2f, 0.2f, 1.2f, 10f, 0.25f, 5f, 15, 60, 12, 0);
            this.tChunks = new Tentacle.TentacleChunk[alcedo.IsKing ? 7 : 5];
            for (int i = 0; i < this.tChunks.Length; i++)
            {
                this.tChunks[i] = new Tentacle.TentacleChunk(this, i, (float)(i + 1) / (float)this.tChunks.Length, 5f);
            }
            this.debugViz = false;
            this.otherTentacleChunk = otherTentacleChunk;
        }

        public override void NewRoom(Room room)
        {
            base.NewRoom(room);
            this.wooshSound = new StaticSoundLoop(SoundID.Vulture_Wing_Woosh_LOOP, this.tChunks[this.tChunks.Length - 1].pos, room, 1f, 1f);
            if (this.debugViz)
            {
                if (this.grabGoalSprites != null)
                {
                    this.grabGoalSprites[0].RemoveFromRoom();
                    this.grabGoalSprites[1].RemoveFromRoom();
                }
                this.grabGoalSprites = new DebugSprite[2];
                this.grabGoalSprites[0] = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel", true), room);
                this.grabGoalSprites[0].sprite.scale = 10f;
                this.grabGoalSprites[0].sprite.color = new Color(1f, 0f, 0f);
                this.grabGoalSprites[1] = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel", true), room);
                this.grabGoalSprites[1].sprite.scale = 10f;
                this.grabGoalSprites[1].sprite.color = new Color(0f, 5f, 0f);
            }
        }

        public override void Update()
        {
            base.Update();
            if (this.alcedo.enteringShortCut != null)
            {
                base.retractFac = Mathf.Min(0f, base.retractFac - 0.1f);
                for (int i = 0; i < this.tChunks.Length; i++)
                {
                    this.tChunks[i].vel += Vector2.ClampMagnitude(this.room.MiddleOfTile(this.alcedo.enteringShortCut.Value) - this.tChunks[i].pos, 50f) / 10f;
                }
                if (this.segments.Count > 1)
                {
                    this.segments.RemoveAt(this.segments.Count - 1);
                }
                return;
            }
            this.attachedAtClaw = false;
            idealLength = Mathf.Lerp(this.alcedo.legLength * 7f,
                                     this.alcedo.legLength * 5f,
                                     flyingMode);/*
            idealLength = Mathf.Lerp(this.alcedo.wingLength * (this.alcedo.IsKing ? 9f : 7f),
                                     this.alcedo.wingLength * (this.alcedo.IsKing ? 13.5f : 11f),
                                     flyingMode);*/
            if (this.stun > 0)
            {
                this.stun--;
            }
            if (Mathf.Pow(UnityEngine.Random.value, 0.25f) > 2f * (this.alcedo.State as Alcedo.AlcedoState).wingHealth[this.tentacleNumber])
            {
                this.stun = Math.Max(this.stun, (int)Mathf.Lerp(-2f, 12f, Mathf.Pow(UnityEngine.Random.value, 0.5f + 20f * Mathf.Max(0f, (this.alcedo.State as Alcedo.AlcedoState).wingHealth[this.tentacleNumber]))));
            }
            //注意，这里比翅膀多了个在飞行时无力
            this.limp = (!this.alcedo.Consious || this.stun > 0 || this.alcedo.AirBorne);
            if (this.limp)
            {
                this.floatGrabDest = null;
                for (int j = 0; j < this.tChunks.Length; j++)
                {
                    this.tChunks[j].vel *= 0.9f;
                    if (this.alcedo.AirBorne)
                    {
                        Tentacle.TentacleChunk tentacleChunk = this.tChunks[j];
                        tentacleChunk.vel.y = tentacleChunk.vel.y - 0.5f;
                        tentacleChunk.vel += (this.alcedo.bodyChunks[5].pos - this.alcedo.bodyChunks[0].pos).normalized;
                    }
                }
            }
            for (int k = 0; k < this.tChunks.Length; k++)
            {
                this.tChunks[k].rad = this.TentacleContour(this.tChunks[k].tPos);// idealLength / (2f * this.tChunks.Length - 2f);//
                if (this.backtrackFrom == -1 || k < this.backtrackFrom)
                {
                    if (k > 1 && Custom.DistLess(this.tChunks[k].pos, this.tChunks[k - 2].pos, 30f))
                    {
                        this.tChunks[k].vel -= Custom.DirVec(this.tChunks[k].pos, this.tChunks[k - 2].pos) * (30f - Vector2.Distance(this.tChunks[k].pos, this.tChunks[k - 2].pos)) * 0.1f;
                    }
                    else if (k <= 1)
                    {
                        this.tChunks[k].vel = Custom.DirVec(this.OtherTentacle.connectedChunk.pos, this.connectedChunk.pos) * ((k == 0) ? 2f : 1.2f) * (this.alcedo.AirBorne ? 1f : 0.1f);
                        //this.tChunks[k].vel = Custom.DirVec(this.OtherTentacle.connectedChunk.pos, this.connectedChunk.pos) * ((k == 0) ? 2f : 1.2f);
                    }
                }
                if (this.room.PointSubmerged(this.tChunks[k].pos))
                {
                    this.tChunks[k].vel *= 0.5f;
                }
                if (this.tChunks[k].contactPoint.x != 0 && this.tChunks[k].lastContactPoint.x == 0 && Mathf.Abs(this.tChunks[k].pos.x - this.tChunks[k].lastPos.x) > 6f)
                {
                    this.room.PlaySound(SoundID.Vulture_Tentacle_Collide_Terrain, this.tChunks[k].pos, Mathf.InverseLerp(6f, 16f, Mathf.Abs(this.tChunks[k].pos.x - this.tChunks[k].lastPos.x)), 1f);
                }
                else if (this.tChunks[k].contactPoint.y != 0 && this.tChunks[k].lastContactPoint.y == 0 && Mathf.Abs(this.tChunks[k].pos.y - this.tChunks[k].lastPos.y) > 6f)
                {
                    this.room.PlaySound(SoundID.Vulture_Tentacle_Collide_Terrain, this.tChunks[k].pos, Mathf.InverseLerp(6f, 16f, Mathf.Abs(this.tChunks[k].pos.y - this.tChunks[k].lastPos.y)), 1f);
                }
            }
            if (!this.limp)
            {
                if (this.floatGrabDest != null && Custom.DistLess(this.tChunks[this.tChunks.Length - 1].pos, this.floatGrabDest.Value, 0.2f * idealLength) && this.backtrackFrom == -1)
                {
                    this.tChunks[this.tChunks.Length - 1].pos = this.floatGrabDest.Value;
                    this.tChunks[this.tChunks.Length - 1].vel *= 0f;
                    this.attachedAtClaw = true;
                }
                this.flyingMode -= 0.025f;
                this.tChunks[this.tChunks.Length - 1].collideWithTerrain = !this.attachedAtClaw;
                this.UpdateDesiredGrabPos();
                bool flag = this.tentacleNumber % 2 == 0;
                BodyChunk bodyChunk = flag ? this.alcedo.bodyChunks[8] : this.alcedo.bodyChunks[7];
                this.segmentsGrippingTerrain = 0;
                for (int l = 0; l < this.tChunks.Length - 1; l++)
                {
                    this.tChunks[l].vel *= Mathf.Lerp(0.95f, 0.85f, this.Support());
                    if (this.attachedAtClaw && (this.backtrackFrom == -1 || l < this.backtrackFrom) && this.GripTerrain(l))
                    {
                        this.segmentsGrippingTerrain++;
                        for (int m = l - 1; m > 0; m--)
                        {
                            base.PushChunksApart(l, m);
                        }
                    }
                    else
                    {
                        Tentacle.TentacleChunk tentacleChunk2 = this.tChunks[l];
                        tentacleChunk2.vel.y = tentacleChunk2.vel.y + 0.1f;
                        this.tChunks[l].vel += this.connectedChunk.vel * 0.1f;
                        if (!this.hasAnyGrip)
                        {
                            if (this.floatGrabDest != null)
                            {
                                this.tChunks[l].vel += Custom.DirVec(this.tChunks[l].pos, this.floatGrabDest.Value) * 0.3f;
                            }
                            else
                            {
                                this.tChunks[l].vel += Custom.DirVec(this.tChunks[l].pos, this.desiredGrabPos + Custom.DirVec(base.FloatBase, this.desiredGrabPos) * 70f) * 0.6f;
                            }
                        }
                    }
                    this.tChunks[l].vel += Custom.DirVec(bodyChunk.pos, this.tChunks[l].pos) * 0.5f / ((float)l + 1f);
                }
                if (this.attachedAtClaw)
                {
                    this.framesWithoutReaching = 0;
                    if (SharedPhysics.RayTraceTilesForTerrain(this.room, base.BasePos, base.grabDest.Value))
                    {
                        if (!Custom.DistLess(this.tChunks[this.tChunks.Length - 1].pos, this.connectedChunk.pos, this.idealLength))
                        {
                            Vector2 a = Custom.DirVec(this.tChunks[this.tChunks.Length - 1].pos, this.connectedChunk.pos);
                            float num = Vector2.Distance(this.tChunks[this.tChunks.Length - 1].pos, this.connectedChunk.pos);
                            float num2 = this.idealLength * 0.9f;
                            this.connectedChunk.pos += a * (num2 - num) * 0.11f;
                            this.connectedChunk.vel += a * (num2 - num) * 0.11f;
                            this.otherTentacleChunk.pos += a * (num2 - num) * 0.09f;//设置this.otherTentacleChunk是为了让身体平衡
                            this.otherTentacleChunk.vel += a * (num2 - num) * 0.09f;
                            /*
                            for (int i = 0; i < this.alcedo.tentacles.Length; i++)
                            {
                                this.alcedo.tentacles[i].connectedChunk.pos += a * (num2 - num) * 0.05f;//为了让身体平衡
                            }*/
                        }
                        if (!Custom.DistLess(this.tChunks[this.tChunks.Length - 1].pos, this.connectedChunk.pos, this.idealLength * 0.9f))
                        {
                            this.alcedo.hangingInLeg = true;
                        }
                    }
                    if (this.playGrabSound)
                    {
                        this.room.PlaySound(SoundID.Vulture_Tentacle_Grab_Terrain, this.tChunks[this.tChunks.Length - 1].pos);
                        this.playGrabSound = false;
                    }
                }
                else
                {
                    this.playGrabSound = true;
                    this.FindGrabPos(ref this.scratchPath);
                    this.framesWithoutReaching++;
                    if ((float)this.framesWithoutReaching > 60f && this.floatGrabDest == null)
                    {
                        this.framesWithoutReaching = 0;
                        this.SwitchMode(AlcedoLeg.Mode.Fly);
                    }
                }
            }
            this.wooshSound.volume = Custom.SCurve(Mathf.InverseLerp(0.4f, 18f, Vector2.Distance(this.tChunks[this.tChunks.Length - 1].pos - this.connectedChunk.pos, this.tChunks[this.tChunks.Length - 1].lastPos - this.connectedChunk.lastPos)), 0.6f) * this.flyingMode;
            this.wooshSound.pitch = Mathf.Lerp(0.3f, 1.7f, Mathf.InverseLerp(-20f, 20f, this.tChunks[this.tChunks.Length - 1].lastPos.y - this.tChunks[this.tChunks.Length - 1].pos.y - (this.connectedChunk.lastPos.y - this.connectedChunk.pos.y)));
            this.wooshSound.pos = Vector2.Lerp(this.connectedChunk.pos, this.tChunks[this.tChunks.Length - 1].pos, 0.7f);
            this.wooshSound.Update();
            if (this.debugViz)
            {
                this.grabGoalSprites[1].pos = this.desiredGrabPos;
            }
        }

        public void SwitchMode(AlcedoLeg.Mode newMode)
        {
            if (newMode == AlcedoLeg.Mode.Fly)
            {
                if (this.alcedo.IsMiros)
                {
                    this.ReleaseGrip();
                }
                this.floatGrabDest = null;
            }
        }

        public void ReleaseGrip()
        {
            if (this.OtherTentacle.grabDelay < 1)
            {
                this.grabDelay = 10;
            }
            this.floatGrabDest = null;
        }

        private void UpdateDesiredGrabPos()
        {
            if (this.alcedo.hoverStill)
            {
                this.desiredGrabPos = this.alcedo.mainBodyChunk.pos + new Vector2(this.tentacleDir, -0.8f).normalized * this.idealLength * 0.7f;
                return;
            }
            this.desiredGrabPos = this.alcedo.mainBodyChunk.pos +
                (Vector2)Vector3.Slerp(this.alcedo.moveDirection, new Vector2(this.tentacleDir, -0.8f).normalized, 0.3f) * this.idealLength * 0.7f;
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
            if (this.grabDelay > 0)
            {
                this.grabDelay--;
                return;
            }
            IntVector2? intVector = this.ClosestSolid(this.room.GetTilePosition(this.desiredGrabPos), 8, 8f);
            //IntVector2? intVector = this.ClosestSolid(this.room.GetTilePosition(this.desiredGrabPos), Mathf.FloorToInt(8 * this.alcedo.legLength / 20f), 8f * this.alcedo.legLength / 20f);
            if (intVector != null)
            {
                IntVector2? intVector2 = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(this.room, base.BasePos, intVector.Value);
                if (base.grabDest == null || this.GripPointAttractiveness(intVector2.Value) > this.GripPointAttractiveness(base.grabDest.Value))
                {
                    Vector2 newGrabDest = Custom.RestrictInRect(base.FloatBase, 
                                                                FloatRect.MakeFromVector2(this.room.MiddleOfTile(intVector2.Value) - new Vector2(11f, 11f), 
                                                                                          this.room.MiddleOfTile(intVector2.Value) + new Vector2(11f, 11f)));
                    base.MoveGrabDest(newGrabDest, ref path);
                }
            }
            Vector2 pos = this.desiredGrabPos + Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * this.idealLength;
            int num = this.room.RayTraceTilesList(base.BasePos.x, base.BasePos.y, this.room.GetTilePosition(pos).x, this.room.GetTilePosition(pos).y, ref path);
            int num2 = 0;
            while (num2 < num && !this.room.GetTile(path[num2]).Solid)
            {
                if ((this.room.GetTile(path[num2]).horizontalBeam || this.room.GetTile(path[num2]).verticalBeam) && (base.grabDest == null || this.GripPointAttractiveness(path[num2]) > this.GripPointAttractiveness(base.grabDest.Value)))
                {
                    base.MoveGrabDest(this.room.MiddleOfTile(path[num2]), ref path);
                    return;
                }
                num2++;
            }
        }

        public float ReleaseScore()
        {
            if (this.alcedo.AirBorne)
            {
                return float.MinValue;
            }
            float num = Vector2.Distance(this.tChunks[this.tChunks.Length - 1].pos, this.desiredGrabPos);
            if (this.floatGrabDest == null)
            {
                num *= 2f;
            }
            return num;
        }

        private bool GripTerrain(int chunk)
        {
            for (int i = 0; i < 4; i++)
            {
                if (this.room.GetTile(this.room.GetTilePosition(this.tChunks[chunk].pos) + Custom.fourDirections[i]).Solid)
                {
                    this.tChunks[chunk].vel *= 0.25f;
                    this.tChunks[chunk].vel += Custom.fourDirections[i].ToVector2() * 0.8f;
                    return this.tChunks[chunk].contactPoint.x != 0 || this.tChunks[chunk].contactPoint.y != 0;
                }
            }
            if (this.room.GetTile(this.tChunks[chunk].pos).horizontalBeam)
            {
                this.tChunks[chunk].vel *= 0.25f;
                Tentacle.TentacleChunk tentacleChunk = this.tChunks[chunk];
                tentacleChunk.vel.y = tentacleChunk.vel.y + (this.room.MiddleOfTile(this.tChunks[chunk].pos).y - this.tChunks[chunk].pos.y) * 0.3f;
                return true;
            }
            if (this.room.GetTile(this.tChunks[chunk].pos).verticalBeam)
            {
                this.tChunks[chunk].vel *= 0.25f;
                Tentacle.TentacleChunk tentacleChunk2 = this.tChunks[chunk];
                tentacleChunk2.vel.x = tentacleChunk2.vel.x + (this.room.MiddleOfTile(this.tChunks[chunk].pos).x - this.tChunks[chunk].pos.x) * 0.3f;
                return true;
            }
            return false;
        }

        private float GripPointAttractiveness(IntVector2 pos)
        {
            if (this.room.GetTile(pos).Solid)
            {
                return 100f / this.room.GetTilePosition(this.desiredGrabPos).FloatDist(pos);
            }
            return 65f / this.room.GetTilePosition(this.desiredGrabPos).FloatDist(pos);
        }

        public float Support()
        {
            if (this.stun > 0)
            {
                return 0f;
            }
            if (!this.alcedo.AirBorne)
            {
                return Mathf.Clamp((this.hasAnyGrip ? 0.5f : 0f) + (float)this.segmentsGrippingTerrain / (float)this.tChunks.Length, 0f, 1f);
                //return Mathf.Clamp((this.hasAnyGrip ? (this.alcedo.IsMiros ? 4f : 0.5f) : 0f) + (float)this.segmentsGrippingTerrain / (float)this.tChunks.Length, 0f, 1f);
            }
            if (!this.alcedo.IsMiros)
            {
                return 0.5f;
            }
            return 1.2f;
        }

        private IntVector2? ClosestSolid(IntVector2 goal, int maxDistance, float maxDistFromBase)
        {
            if (this.room.GetTile(goal).Solid)
            {
                return new IntVector2?(goal);
            }
            for (int i = 1; i <= maxDistance; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (this.room.GetTile(goal + Custom.eightDirections[j] * i).Solid && base.BasePos.FloatDist(goal + Custom.eightDirections[j] * i) < maxDistFromBase)
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
                if (!SharedPhysics.RayTraceTilesForTerrain(this.room, this.room.GetTilePosition(this.connectedChunk.pos), this.room.GetTilePosition(this.connectedChunk.pos + new Vector2(this.tentacleDir * this.idealLength, 50f * (float)i))))
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
            return new IntVector2((int)this.tentacleDir, -1);
        }

        public void Damage(Creature.DamageType type, float damage, float stunBonus)
        {
            damage /= 2.7f;
            stunBonus /= 1.2f;
            this.stun = Math.Max(this.stun, (int)(damage * 30f + stunBonus));
            (this.alcedo.State as Alcedo.AlcedoState).wingHealth[this.tentacleNumber] -= damage;
        }

        public float Wave(int n)
        {
            return Mathf.Sin((float)Math.PI * 2f * (this.alcedo.wingFlap - this.tChunks[n].tPos * 0.5f));
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

        public class Mode : ExtEnum<AlcedoLeg.Mode>
        {
            public Mode(string value, bool register = false) : base(value, register)
            {
            }
            public static readonly AlcedoLeg.Mode Climb = new AlcedoLeg.Mode("Climb", true);
            public static readonly AlcedoLeg.Mode Fly = new AlcedoLeg.Mode("Fly", true);
        }
    }
}
