using MoreSlugcats;
using Noise;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature
{
    internal class Alcedo : AirBreatherCreature, IFlyingCreature, PhysicalObject.IHaveAppendages
    {
        public bool IsKing
        {
            get
            {
                return base.Template.type == CreatureTemplate.Type.KingVulture;
            }
        }
        public bool IsMiros
        {
            get
            {
                return ModManager.MSC && base.Template.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
            }
        }
        public float jetFuel
        {
            get
            {
                return this.jf;
            }
            set
            {
                this.jf = Mathf.Clamp(value, 0f, 1f);
            }
        }
        public bool Snapping
        {
            get
            {
                return this.snapFrames > 0 && this.snapFrames <= 21;
            }
        }
        public bool ChargingSnap
        {
            get
            {
                return this.snapFrames > 0 && this.snapFrames > 21;
            }
        }
        public float TusksStuck
        {
            get
            {
                if ((float)this.snapFrames <= 0f && base.grasps[0] == null)
                {
                    return this.tuskCharge;
                }
                return 1f;
            }
        }
        public bool AirBorne
        {
            get
            {
                return this.tentacles[0].mode == AlcedoTentacle.Mode.Fly && this.tentacles[1].mode == AlcedoTentacle.Mode.Fly;
            }
        }
        public override Vector2 VisionPoint
        {
            get
            {
                return base.bodyChunks[4].pos;
            }
        }
        public override Vector2 DangerPos
        {
            get
            {/*
                if (this.IsKing)
                {
                    if (this.kingTusks.tusks[0].mode == KingTusks.Tusk.Mode.ShootingOut || this.kingTusks.tusks[0].mode == KingTusks.Tusk.Mode.Charging)
                    {
                        return this.kingTusks.tusks[0].chunkPoints[0, 0];
                    }
                    if (this.kingTusks.tusks[1].mode == KingTusks.Tusk.Mode.ShootingOut || this.kingTusks.tusks[1].mode == KingTusks.Tusk.Mode.Charging)
                    {
                        return this.kingTusks.tusks[1].chunkPoints[0, 0];
                    }
                }*/
                return base.mainBodyChunk.pos;
            }
        }

        public Alcedo(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
            base.bodyChunks = new BodyChunk[5];
            float num = this.IsKing ? 1.4f : 1f;
            base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 9.5f, this.IsMiros ? 1.8f : (1.2f * num));
            base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 9.5f, this.IsMiros ? 1.8f : (1.2f * num));
            base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 9.5f, this.IsMiros ? 1.8f : (1.2f * num));
            base.bodyChunks[3] = new BodyChunk(this, 3, new Vector2(0f, 0f), 9.5f, this.IsMiros ? 1.8f : (1.2f * num));
            base.bodyChunks[4] = new BodyChunk(this, 4, new Vector2(0f, 0f), 6.5f, 0.3f * num);
            for (int i = 0; i < base.bodyChunks.Length; i++)
            {
                base.bodyChunks[i].restrictInRoomRange = 2400f;
                base.bodyChunks[i].defaultRestrictInRoomRange = 2400f;
            }
            this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[(ModManager.MMF && !this.IsMiros) ? 8 : 7];
            float num2 = 40f;
            float num3 = 26f;
            float num4 = 10f;
            this.bodyChunkConnections[0] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], num3, PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
            this.bodyChunkConnections[1] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[3], num2, PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
            this.bodyChunkConnections[2] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[1], Mathf.Sqrt(Mathf.Pow(num4, 2f) + Mathf.Pow(num2 / 2f, 2f)), PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
            this.bodyChunkConnections[3] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[1], base.bodyChunks[3], Mathf.Sqrt(Mathf.Pow(num4, 2f) + Mathf.Pow(num2 / 2f, 2f)), PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
            this.bodyChunkConnections[4] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[2], base.bodyChunks[0], Mathf.Sqrt(Mathf.Pow(num3 - num4, 2f) + Mathf.Pow(num2 / 2f, 2f)), PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
            this.bodyChunkConnections[5] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[3], base.bodyChunks[0], Mathf.Sqrt(Mathf.Pow(num3 - num4, 2f) + Mathf.Pow(num2 / 2f, 2f)), PhysicalObject.BodyChunkConnection.Type.Normal, 1f, 0.5f);
            this.bodyChunkConnections[6] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[4], this.IsKing ? 70f : 60f, PhysicalObject.BodyChunkConnection.Type.Pull, 0.6f, 0f);
            if (ModManager.MMF && !this.IsMiros)
            {
                this.bodyChunkConnections[7] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[4], base.bodyChunks[0], this.IsKing ? 75f : 65f, PhysicalObject.BodyChunkConnection.Type.Pull, 1f, -1f);
            }
            this.tentacles = new AlcedoTentacle[this.IsMiros ? 4 : 2];
            for (int j = 0; j < this.tentacles.Length; j++)
            {
                this.tentacles[j] = new AlcedoTentacle(this, base.bodyChunks[2 + j % 2], (this.IsKing ? 9f : 7f) * 20f, j);
            }
            this.neck = new Tentacle(this, base.bodyChunks[0], (this.IsKing ? 6f : 5f) * 20f);
            this.neck.tProps = new Tentacle.TentacleProps(false, false, true, 0.5f, 0f, 0.5f, 1.8f, 0.2f, 1.2f, 10f, 0.25f, 3f, 15, 20, 6, 0);
            this.neck.tChunks = new Tentacle.TentacleChunk[4];
            for (int k = 0; k < this.neck.tChunks.Length; k++)
            {
                this.neck.tChunks[k] = new Tentacle.TentacleChunk(this.neck, k, (float)(k + 1) / (float)this.neck.tChunks.Length, this.IsKing ? 6f : 5f);
            }
            this.neck.tChunks[this.neck.tChunks.Length - 1].rad = 7f;
            this.neck.stretchAndSqueeze = 0f;
            this.appendages = new List<PhysicalObject.Appendage>();
            this.appendages.Add(new PhysicalObject.Appendage(this, 0, this.neck.tChunks.Length + 2));
            for (int l = 0; l < this.tentacles.Length; l++)
            {
                this.appendages.Add(new PhysicalObject.Appendage(this, l + 1, this.tentacles[l].tChunks.Length + 1));
            }
            this.lastConnection = new MovementConnection(MovementConnection.MovementType.Standard, new WorldCoordinate(0, 0, 0, 0), new WorldCoordinate(0, 0, 0, 0), 0);
            this.thrusters = new Alcedo.AlcedoThruster[4];
            this.jetFuel = 1f;
            this.thrusters[0] = new Alcedo.AlcedoThruster(this, 2, 0, 0.5f, 15f);
            this.thrusters[1] = new Alcedo.AlcedoThruster(this, 3, 0, 0.5f, -15f);
            this.thrusters[2] = new Alcedo.AlcedoThruster(this, 2, 1, 0.2f, 100f);
            this.thrusters[3] = new Alcedo.AlcedoThruster(this, 3, 1, 0.2f, -100f);
            base.GoThroughFloors = true;
            this.wingFlapAmplitude = 1f;
            this.mouseTilePos = abstractCreature.pos.Tile;
            base.airFriction = 0.99f;
            base.gravity = 0.9f;
            this.bounce = 0.1f;
            this.surfaceFriction = 0.35f;
            this.collisionLayer = 1;
            base.waterFriction = 0.9f;
            base.buoyancy = 0.92f;/*
            if (this.IsKing)
            {
                this.kingTusks = new KingTusks(this);
            }*/
        }

        public override void InitiateGraphicsModule()
        {
            if (base.graphicsModule == null)
            {
                base.graphicsModule = new AlcedoGraphics(this);
            }
        }

        public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
        {
            base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
            Vector2 a = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
            for (int i = 0; i < base.bodyChunks.Length; i++)
            {
                base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - a * ((i == 1) ? 10f : 5f) + Custom.DegToVec(UnityEngine.Random.value * 360f);
                base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
                base.bodyChunks[i].vel = a * 5f;
            }
            for (int j = 0; j < this.tentacles.Length; j++)
            {
                this.tentacles[j].Reset(this.tentacles[j].connectedChunk.pos);
            }
            this.neck.Reset(base.mainBodyChunk.pos);
            this.shortcutDelay = 80;
            if (base.graphicsModule != null)
            {
                base.graphicsModule.Reset();
            }
        }

        public void SpawnFlyingCreature(WorldCoordinate coord)
        {
        }

        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            if (base.graphicsModule != null)
            {
                base.graphicsModule.Reset();
            }
        }

        public override void NewRoom(Room room)
        {
            for (int i = 0; i < this.tentacles.Length; i++)
            {
                this.tentacles[i].NewRoom(room);
            }
            this.neck.NewRoom(room);/*
            if (this.kingTusks != null)
            {
                this.kingTusks.NewRoom(room);
            }*/
            base.NewRoom(room);
        }

        public override void Update(bool eu)
        {
            this.CheckFlip();
            base.Update(eu);
            if (this.room == null)
            {
                return;
            }
            if (this.temporarilyAllowInForbiddenTiles && this.room.aimap.TileAccessibleToCreature(this.room.GetTilePosition(base.mainBodyChunk.pos), base.Template))
            {
                this.temporarilyAllowInForbiddenTiles = false;
            }
            if (this.room.game.devToolsActive && Input.GetKey("b") && this.room.game.cameras[0].room == this.room)
            {
                base.bodyChunks[1].vel += Custom.DirVec(base.bodyChunks[1].pos, Futile.mousePosition.ToVector2InPoints() + this.room.game.cameras[0].pos) * 14f;
                for (int i = 0; i < base.bodyChunks.Length; i++)
                {
                    base.bodyChunks[i].vel *= 0.9f;
                }
                this.Stun(12);
            }
            if (this.IsMiros && this.laserCounter > 0)
            {
                this.laserCounter--;
                if (this.laserCounter == 10 && !base.dead)
                {
                    this.LaserExplosion();
                    if (this.LaserLight != null)
                    {
                        this.LaserLight.Destroy();
                    }
                    this.laserCounter--;
                }
                if (base.dead || !this.MostlyConsious || base.graphicsModule == null || (base.graphicsModule as AlcedoGraphics).shadowMode)
                {
                    if (this.LaserLight != null)
                    {
                        this.LaserLight.Destroy();
                    }
                    this.laserCounter = 0;
                }
                else if (this.LaserLight != null)
                {
                    Vector2 pos = this.Head().pos;
                    Vector2 a = Custom.DirVec(this.neck.Tip.pos, pos);
                    a *= -1f;
                    Vector2 corner = Custom.RectCollision(pos, pos - a * 100000f, this.room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
                    IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(this.room, pos, corner);
                    if (intVector != null)
                    {
                        corner = Custom.RectCollision(corner, pos, this.room.TileRect(intVector.Value)).GetCorner(FloatRect.CornerLabel.D);
                        this.LaserLight.HardSetPos(corner);
                        this.LaserLight.HardSetRad((float)this.laserCounter);
                        this.LaserLight.color = new Color((200f - (float)this.laserCounter) / 200f, (float)this.laserCounter / 200f, 0.1f);
                        this.LaserLight.HardSetAlpha(1f);
                    }
                }
            }
            this.hangingInTentacle = false;
            for (int j = 0; j < this.tentacles.Length; j++)
            {
                this.tentacles[j].Update();
            }
            if (this.hangingInTentacle)
            {
                this.cantFindNewGripCounter += 2;
                if (this.cantFindNewGripCounter > (this.IsMiros ? 200 : 400))
                {
                    for (int k = 0; k < this.tentacles.Length; k++)
                    {
                        if (this.tentacles[k].hasAnyGrip)
                        {
                            this.tentacles[k].ReleaseGrip();
                        }
                    }
                }
            }
            else if (this.cantFindNewGripCounter > 0)
            {
                this.cantFindNewGripCounter--;
            }
            if (this.enteringShortCut == null)
            {
                this.UpdateNeck();
            }
            if (this.landingBrake > 0)
            {
                this.landingBrake--;
                for (int l = 0; l < base.bodyChunks.Length; l++)
                {
                    base.bodyChunks[l].vel *= 0.7f;
                }
                base.bodyChunks[1].vel += Vector2.ClampMagnitude(this.landingBrakePos - base.bodyChunks[1].pos, 40f) / 20f;
            }
            bool flag = true;
            float num = 0f;
            for (int m = 0; m < this.thrusters.Length; m++)
            {
                this.thrusters[m].Update(eu);
                if (this.thrusters[m].Active)
                {
                    flag = false;
                    if (this.thrusters[m].Force > num)
                    {
                        num = this.thrusters[m].Force;
                    }
                }
            }
            if (flag)
            {
                this.jetFuel += 0.008333334f;
            }
            if (UnityEngine.Random.value * 0.75f > (base.State as HealthState).health)
            {
                this.Stun(10);
            }
            if (this.jetSound != null)
            {
                if (num == 0f)
                {
                    this.jetSound.alive = false;
                    this.jetSound = null;
                }
                else
                {
                    this.jetSound.alive = true;
                    this.jetSound.volume = Mathf.InverseLerp(0f, 0.1f, num);
                    this.jetSound.pitch = Mathf.Lerp(0.4f, 2.2f, num);
                }
            }
            else if (num > 0f)
            {
                this.jetSound = this.room.PlaySound(SoundID.Vulture_Jet_LOOP, base.mainBodyChunk);
                this.jetSound.requireActiveUpkeep = true;
            }
            if (this.tuskChargeSound != null)
            {
                if (!this.ChargingSnap)
                {
                    this.tuskChargeSound.alive = false;
                    this.tuskChargeSound = null;
                }
                else
                {
                    this.tuskChargeSound.alive = true;
                }
            }
            else if (this.ChargingSnap)
            {
                this.tuskChargeSound = this.room.PlaySound(SoundID.Vulture_Jaws_Carged_LOOP, base.mainBodyChunk);
                this.tuskChargeSound.requireActiveUpkeep = true;
            }
            if (this.room.game.devToolsActive && Input.GetKey("g"))
            {
                BodyChunk mainBodyChunk = base.mainBodyChunk;
                mainBodyChunk.vel.y = mainBodyChunk.vel.y + 20f;
            }
            if (base.grasps[0] != null)
            {
                this.Carry();
            }
            if (base.Consious)
            {
                this.Act(eu);
            }/*
            if (this.kingTusks != null)
            {
                this.kingTusks.Update();
            }*/
        }

        public void Act(bool eu)
        {
            this.AI.Update();
            if (this.IsMiros)
            {
                this.lastJawOpen = this.jawOpen;
                if (base.grasps[0] != null)
                {
                    this.jawOpen = 0.15f;
                }
                else if (this.jawSlamPause > 0)
                {
                    this.jawSlamPause--;
                }
                else
                {
                    if (this.isLaserActive())
                    {
                        this.jawKeepOpenPause = 10;
                        this.keepJawOpenPos = 1f;
                    }
                    if (this.jawVel == 0f)
                    {
                        this.jawVel = 0.15f;
                    }
                    if (base.abstractCreature.controlled && this.jawVel >= 0f && this.jawVel < 1f && !this.controlledJawSnap)
                    {
                        this.jawVel = 0f;
                        this.jawOpen = 0f;
                    }
                    this.jawOpen += this.jawVel;
                    if (this.jawKeepOpenPause > 0)
                    {
                        this.jawKeepOpenPause--;
                        this.jawOpen = Mathf.Clamp(Mathf.Lerp(this.jawOpen, this.keepJawOpenPos, UnityEngine.Random.value * 0.5f), 0f, 1f);
                    }
                    else if (UnityEngine.Random.value < 1f / ((!base.Blinded) ? 40f : 15f) && !base.abstractCreature.controlled)
                    {
                        this.jawKeepOpenPause = UnityEngine.Random.Range(10, UnityEngine.Random.Range(10, 60));
                        this.keepJawOpenPos = ((UnityEngine.Random.value >= 0.5f) ? 1f : 0f);
                        this.jawVel = Mathf.Lerp(-0.4f, 0.4f, UnityEngine.Random.value);
                        this.jawOpen = Mathf.Clamp(this.jawOpen, 0f, 1f);
                    }
                    else if (this.jawOpen <= 0f)
                    {
                        this.jawOpen = 0f;
                        if (this.jawVel < -0.4f)
                        {
                            this.JawSlamShut();
                            this.controlledJawSnap = false;
                        }
                        this.jawVel = 0.15f;
                        this.jawSlamPause = 5;
                    }
                    else if (this.jawOpen >= 1f)
                    {
                        this.jawOpen = 1f;
                        this.jawVel = -0.5f;
                    }
                }
            }
            if (!this.AirBorne)
            {
                float num = 100f;
                if (base.mainBodyChunk.pos.x < -num || base.mainBodyChunk.pos.y < -num || base.mainBodyChunk.pos.x > this.room.PixelWidth + num || base.mainBodyChunk.pos.y > this.room.PixelHeight + num)
                {
                    for (int i = 0; i < this.tentacles.Length; i++)
                    {
                        this.tentacles[i].SwitchMode(AlcedoTentacle.Mode.Fly);
                    }
                }
            }
            if (this.wingFlap < 0.5f)
            {
                this.wingFlap += 0.033333335f;
            }
            else
            {
                this.wingFlap += 0.02f;
            }
            if (this.wingFlap > 1f)
            {
                this.wingFlap -= 1f;
            }
            float num2 = 0f;
            for (int j = 0; j < this.tentacles.Length; j++)
            {
                num2 += this.tentacles[j].Support() * (this.IsMiros ? 0.75f : 0.5f);
            }
            num2 = Mathf.Pow(num2, 0.5f);
            num2 = Mathf.Max(num2, 0.1f);
            this.hoverStill = false;
            IntVector2 intVector = this.room.GetTilePosition(base.mainBodyChunk.pos);
            for (int k = 0; k < 5; k++)
            {
                if (this.room.aimap.TileAccessibleToCreature(intVector + Custom.fourDirectionsAndZero[k], base.Template))
                {
                    intVector += Custom.fourDirectionsAndZero[k];
                }
            }
            if (this.room == null)
            {
                return;
            }
            MovementConnection movementConnection = (this.AI.pathFinder as AlcedoPather).FollowPath(this.room.GetWorldCoordinate(intVector), true);
            AlcedoTentacle.Mode a = AlcedoTentacle.Mode.Climb;
            if (base.safariControlled)
            {
                bool flag = false;
                MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
                if (movementConnection == default(MovementConnection) || !this.AllowableControlledAIOverride(movementConnection.type) || movementConnection.type == MovementConnection.MovementType.OutsideRoom || movementConnection.type == MovementConnection.MovementType.OffScreenMovement)
                {
                    movementConnection = default(MovementConnection);
                    if (this.room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                    {
                        type = MovementConnection.MovementType.ShortCut;
                    }
                    else
                    {
                        for (int l = 0; l < Custom.fourDirections.Length; l++)
                        {
                            if (this.room.GetTile(base.mainBodyChunk.pos + Custom.fourDirections[l].ToVector2() * 20f).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                            {
                                type = MovementConnection.MovementType.BigCreatureShortCutSqueeze;
                                break;
                            }
                        }
                    }
                    flag = true;
                }
                if (this.inputWithDiagonals != null)
                {
                    if ((!this.IsMiros || this.isLaserActive()) && this.inputWithDiagonals.Value.thrw && (this.inputWithDiagonals.Value.x != 0 || this.inputWithDiagonals.Value.y != 0))
                    {
                        Vector2 p = base.bodyChunks[4].pos + new Vector2((float)this.inputWithDiagonals.Value.x, (float)this.inputWithDiagonals.Value.y) * 200f;
                        base.bodyChunks[4].vel += Custom.DirVec(base.bodyChunks[4].pos, p) * 15f;
                        this.neck.tChunks[this.neck.tChunks.Length - 1].vel -= Custom.DirVec(base.bodyChunks[4].pos, p) * num2;
                    }
                    else if ((this.inputWithDiagonals.Value.x != 0 || this.inputWithDiagonals.Value.y != 0) && flag)
                    {
                        movementConnection = new MovementConnection(type, this.room.GetWorldCoordinate(base.mainBodyChunk.pos), this.room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2((float)this.inputWithDiagonals.Value.x, (float)this.inputWithDiagonals.Value.y) * 40f), 2);
                    }
                    if (this.inputWithDiagonals.Value.jmp)
                    {
                        a = AlcedoTentacle.Mode.Fly;
                        if (!this.lastInputWithDiagonals.Value.jmp)
                        {
                            bool flag2 = false;
                            for (int m = 0; m < this.tentacles.Length; m++)
                            {
                                if (this.tentacles[m].mode == AlcedoTentacle.Mode.Climb)
                                {
                                    flag2 = true;
                                    break;
                                }
                            }
                            if (flag2)
                            {
                                this.TakeOff();
                            }
                        }
                        for (int n = 0; n < this.tentacles.Length; n++)
                        {
                            this.tentacles[n].SwitchMode(AlcedoTentacle.Mode.Fly);
                        }
                    }
                    if (this.IsMiros && this.inputWithDiagonals.Value.pckp && !this.lastInputWithDiagonals.Value.pckp)
                    {
                        this.controlledJawSnap = true;
                    }
                    if (!this.IsMiros && this.inputWithDiagonals.Value.pckp && !this.lastInputWithDiagonals.Value.pckp && this.snapFrames == 0)
                    {
                        if (this.AI.focusCreature != null && this.AI.focusCreature.VisualContact)
                        {
                            Creature realizedCreature = this.AI.focusCreature.representedCreature.realizedCreature;
                            if (realizedCreature.bodyChunks.Length != 0)
                            {
                                BodyChunk bodyChunk = realizedCreature.bodyChunks[UnityEngine.Random.Range(0, realizedCreature.bodyChunks.Length)];
                                this.Snap(bodyChunk);
                            }
                        }
                        else if (this.inputWithDiagonals.Value.AnyDirectionalInput)
                        {
                            this.SnapTowards(base.mainBodyChunk.pos + new Vector2((float)this.inputWithDiagonals.Value.x, (float)this.inputWithDiagonals.Value.y) * 200f);
                        }
                        else
                        {
                            this.SnapTowards(base.mainBodyChunk.pos + Custom.RNV() * 200f);
                        }
                    }
                    if (this.inputWithDiagonals.Value.thrw && !this.lastInputWithDiagonals.Value.thrw)
                    {
                        if (base.grasps[0] != null)
                        {
                            this.LoseAllGrasps();
                        }
                        else if (this.IsMiros && !this.isLaserActive())
                        {
                            this.FireLaser();
                        }
                    }
                    if (flag)
                    {
                        if (this.inputWithDiagonals.Value.y < 0)
                        {
                            base.GoThroughFloors = true;
                        }
                        else
                        {
                            base.GoThroughFloors = false;
                        }
                    }
                }
            }
            if (movementConnection == default(MovementConnection) || Custom.ManhattanDistance(this.room.GetWorldCoordinate(base.mainBodyChunk.pos), this.AI.pathFinder.GetDestination) < 2)
            {
                this.hoverStill = true;
            }
            this.neck.retractFac = Mathf.Clamp(this.neck.retractFac + 0.033333335f, 0f, 0.6f);
            base.bodyChunks[4].vel *= 0.9f;
            for (int num3 = 0; num3 < 4; num3++)
            {
                if (this.AirBorne)
                {
                    base.bodyChunks[num3].vel *= 0.98f;
                    num2 = 0f;
                    for (int num4 = 0; num4 < this.tentacles.Length; num4++)
                    {
                        num2 += ((this.tentacles[num4].stun >= 5) ? 0f : (1f / (float)this.tentacles.Length));
                    }
                }
                else
                {
                    base.bodyChunks[num3].vel *= Mathf.Lerp(0.98f, 0.9f, num2);
                    if (num2 > 0.1f)
                    {
                        BodyChunk bodyChunk2 = base.bodyChunks[num3];
                        bodyChunk2.vel.y = bodyChunk2.vel.y + Mathf.Lerp(1.2f, 0.5f, num2);
                    }
                }
            }
            BodyChunk bodyChunk3 = base.bodyChunks[1];
            bodyChunk3.vel.y = bodyChunk3.vel.y + 1.9f * num2 * Mathf.InverseLerp(1f, 7f, base.mainBodyChunk.vel.magnitude);
            BodyChunk bodyChunk4 = base.bodyChunks[0];
            bodyChunk4.vel.y = bodyChunk4.vel.y - 1.9f * num2 * Mathf.InverseLerp(1f, 7f, base.mainBodyChunk.vel.magnitude);
            if (!this.hoverStill && (movementConnection == default(MovementConnection) || (movementConnection.DestTile == this.lastConnection.DestTile && this.room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile))))
            {
                this.stuck++;
                if (this.stuck > 60)
                {
                    this.stuckShake = this.stuckShakeDuration;
                    this.stuckShakeDuration += 30;
                }
            }
            else
            {
                this.stuck = 0;
                if (this.stuckShakeDuration > 30)
                {
                    this.stuckShakeDuration--;
                }
            }
            if (this.room == null)
            {
                return;
            }
            for (int num5 = 0; num5 < 5; num5++)
            {
                if (this.room.GetTile(base.abstractCreature.pos.Tile + Custom.fourDirectionsAndZero[num5]).wormGrass)
                {
                    this.stuckShake = Math.Max(this.stuckShake, 40);
                    base.mainBodyChunk.vel -= Custom.fourDirectionsAndZero[num5].ToVector2() * 2f + Custom.RNV() * 6f + new Vector2(0f, 6f);
                }
            }
            if (this.AI.stuckTracker.Utility() > 0.9f)
            {
                this.stuckShake = Math.Max(this.stuckShake, 5);
            }
            if (this.stuckShake > 0)
            {
                this.stuckShake--;
                this.StuckBehavior();
                return;
            }
            if (!this.hoverStill)
            {
                bool flag3 = true;
                for (int num6 = 0; num6 < this.tentacles.Length; num6++)
                {
                    flag3 = (flag3 && (this.tentacles[num6].hasAnyGrip || this.tentacles[num6].mode != AlcedoTentacle.Mode.Climb));
                }
                if (this.hangingInTentacle && flag3)
                {
                    this.releaseGrippingTentacle++;
                    if (this.releaseGrippingTentacle > 5 && this.CheckTentacleModeAnd(AlcedoTentacle.Mode.Climb))
                    {
                        this.tentacles[this.TentacleMaxReleaseInd()].ReleaseGrip();
                    }
                    else if (this.releaseGrippingTentacle > 50)
                    {
                        this.tentacles[this.TentacleMaxReleaseInd()].ReleaseGrip();
                    }
                }
                else
                {
                    this.releaseGrippingTentacle = 0;
                }
                bool flag4 = true;
                for (int num7 = 0; num7 < this.tentacles.Length; num7++)
                {
                    flag4 = (flag4 && this.tentacles[num7].WingSpace());
                }
                if (!base.safariControlled && this.IsMiros && this.isLaserActive() && this.CheckTentacleModeOr(AlcedoTentacle.Mode.Climb))
                {
                    if (this.timeSinceLastTakeoff >= 40)
                    {
                        this.TakeOff();
                    }
                    else
                    {
                        for (int num8 = 0; num8 < this.tentacles.Length; num8++)
                        {
                            this.tentacles[num8].SwitchMode(AlcedoTentacle.Mode.Fly);
                        }
                    }
                    this.dontSwitchModesCounter = 200;
                }
                this.timeSinceLastTakeoff++;
                if (this.dontSwitchModesCounter > 0)
                {
                    this.dontSwitchModesCounter--;
                }
                else if (this.IsMiros)
                {
                    if (!this.hoverStill && this.room.aimap.getTerrainProximity(movementConnection.DestTile) > 5 && this.CheckTentacleModeAnd(AlcedoTentacle.Mode.Climb) && base.mainBodyChunk.vel.y > 4f && this.moveDirection.y > 0f && SharedPhysics.RayTraceTilesForTerrain(this.room, this.room.GetTilePosition(base.mainBodyChunk.pos), this.room.GetTilePosition(base.mainBodyChunk.pos + this.moveDirection * 400f)) && flag4)
                    {
                        this.TakeOff();
                        this.dontSwitchModesCounter = 200;
                    }
                    else if (!this.hoverStill && this.room.aimap.getTerrainProximity(movementConnection.DestTile) > 4 && this.CheckTentacleModeOr(AlcedoTentacle.Mode.Climb) && (!base.safariControlled || a == AlcedoTentacle.Mode.Fly))
                    {
                        for (int num9 = 0; num9 < this.tentacles.Length; num9++)
                        {
                            this.tentacles[num9].SwitchMode(AlcedoTentacle.Mode.Fly);
                        }
                        this.dontSwitchModesCounter = 200;
                    }
                    else if (this.room.aimap.getTerrainProximity(movementConnection.DestTile) <= (this.IsMiros ? 4 : 8) && this.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y != -1 && this.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y > movementConnection.DestTile.y - 10 && this.CheckTentacleModeAnd(AlcedoTentacle.Mode.Fly) && (!base.safariControlled || a == AlcedoTentacle.Mode.Climb))
                    {
                        for (int num10 = 0; num10 < this.tentacles.Length; num10++)
                        {
                            this.tentacles[num10].SwitchMode(AlcedoTentacle.Mode.Climb);
                        }
                        this.AirBrake(30);
                        this.dontSwitchModesCounter = 200;
                    }
                }
                else if (this.room.aimap.getTerrainProximity(movementConnection.DestTile) <= (this.IsMiros ? 4 : 8) && this.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y != -1 && this.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y > movementConnection.DestTile.y - 10 && this.CheckTentacleModeAnd(AlcedoTentacle.Mode.Fly) && (!base.safariControlled || a == AlcedoTentacle.Mode.Climb))
                {
                    for (int num11 = 0; num11 < this.tentacles.Length; num11++)
                    {
                        this.tentacles[num11].SwitchMode(AlcedoTentacle.Mode.Climb);
                    }
                    this.AirBrake(30);
                    this.dontSwitchModesCounter = 200;
                }
                else if (!this.hoverStill && this.room.aimap.getTerrainProximity(movementConnection.DestTile) > 5 && this.CheckTentacleModeAnd(AlcedoTentacle.Mode.Climb) && base.mainBodyChunk.vel.y > 4f && this.moveDirection.y > 0f && SharedPhysics.RayTraceTilesForTerrain(this.room, this.room.GetTilePosition(base.mainBodyChunk.pos), this.room.GetTilePosition(base.mainBodyChunk.pos + this.moveDirection * 400f)) && flag4 && (!base.safariControlled || a == AlcedoTentacle.Mode.Fly))
                {
                    this.TakeOff();
                    this.dontSwitchModesCounter = 200;
                }
            }
            bool flag5 = true;
            for (int num12 = 0; num12 < this.tentacles.Length; num12++)
            {
                flag5 = (flag5 && !this.tentacles[num12].hasAnyGrip);
            }
            if (base.mainBodyChunk.vel.y < -10f && this.CheckTentacleModeAnd(AlcedoTentacle.Mode.Climb) && flag5 && this.landingBrake < 1 && (!base.safariControlled || a == AlcedoTentacle.Mode.Fly))
            {
                for (int num13 = 0; num13 < this.tentacles.Length; num13++)
                {
                    this.tentacles[num13].SwitchMode(AlcedoTentacle.Mode.Fly);
                }
                if (base.graphicsModule != null)
                {
                    (base.graphicsModule as AlcedoGraphics).MakeColorWave(UnityEngine.Random.Range(10, 20));
                }
            }
            bool flag6 = true;
            for (int num14 = 0; num14 < this.tentacles.Length; num14++)
            {
                flag6 = (flag6 && this.tentacles[num14].mode != AlcedoTentacle.Mode.Fly);
            }
            if (this.CheckTentacleModeOr(AlcedoTentacle.Mode.Fly))
            {
                this.wingFlapAmplitude = Mathf.Clamp(this.wingFlapAmplitude + 0.033333335f, 0f, 1f);
            }
            else if (flag6)
            {
                this.wingFlapAmplitude = 0f;
            }
            else
            {
                this.wingFlapAmplitude = Mathf.Clamp(this.wingFlapAmplitude + 0.0125f, 0f, 0.5f);
            }
            if (this.hoverStill)
            {
                if (!this.lastHoverStill || !Custom.DistLess(base.mainBodyChunk.pos, this.room.MiddleOfTile(this.hoverPos), 60f))
                {
                    this.hoverPos = this.room.GetTilePosition(base.mainBodyChunk.pos);
                }
                BodyChunk bodyChunk5 = base.bodyChunks[1];
                bodyChunk5.vel.y = bodyChunk5.vel.y + 0.1f * num2;
                for (int num15 = 0; num15 < 4; num15++)
                {
                    base.bodyChunks[num15].vel *= Mathf.Lerp(1f, 0.9f, num2);
                    base.bodyChunks[num15].vel += 0.6f * num2 * Vector2.ClampMagnitude(this.room.MiddleOfTile(this.hoverPos) - base.mainBodyChunk.pos, 10f) / 10f;
                }
            }
            else if (movementConnection != default(MovementConnection))
            {
                Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, this.room.MiddleOfTile(movementConnection.destinationCoord));
                vector = Vector2.Lerp(vector, IntVector2.ClampAtOne(movementConnection.DestTile - movementConnection.StartTile).ToVector2(), 0.5f);
                if (this.AirBorne)
                {
                    if (this.room.IsPositionInsideBoundries(base.abstractCreature.pos.Tile) || vector.y < 0f)
                    {
                        vector.y *= 0.5f;
                        if (vector.y < 0f)
                        {
                            vector.y = 0f;
                        }
                    }
                    if (movementConnection.destinationCoord.y > movementConnection.startCoord.y || !movementConnection.destinationCoord.TileDefined)
                    {
                        BodyChunk bodyChunk6 = base.bodyChunks[1];
                        bodyChunk6.vel.y = bodyChunk6.vel.y + 3.5f;
                    }
                    else if (movementConnection.destinationCoord.y < movementConnection.startCoord.y)
                    {
                        this.wingFlap -= 0.014285714f;
                    }
                }
                else if (!this.room.IsPositionInsideBoundries(movementConnection.DestTile))
                {
                    if (movementConnection.destinationCoord.y > this.room.TileHeight)
                    {
                        this.TakeOff();
                    }
                    else if (!base.safariControlled || a == AlcedoTentacle.Mode.Fly)
                    {
                        for (int num16 = 0; num16 < this.tentacles.Length; num16++)
                        {
                            this.tentacles[num16].SwitchMode(AlcedoTentacle.Mode.Fly);
                        }
                    }
                }
                for (int num17 = 0; num17 < 4; num17++)
                {
                    base.bodyChunks[num17].vel += vector * (this.AirBorne ? 0.6f : (this.IsKing ? 1.9f : 1.2f)) * num2;
                }
                MovementConnection movementConnection2 = movementConnection;
                for (int num18 = 0; num18 < 3; num18++)
                {
                    MovementConnection movementConnection3 = (this.AI.pathFinder as AlcedoPather).FollowPath(movementConnection2.destinationCoord, false);
                    movementConnection2 = movementConnection3;
                }
                if (movementConnection2 == movementConnection)
                {
                    this.moveDirection = (this.moveDirection + vector * 0.1f).normalized;
                }
                else
                {
                    this.moveDirection = (this.moveDirection + Custom.DirVec(base.mainBodyChunk.pos, this.room.MiddleOfTile(movementConnection2.destinationCoord)) * 0.5f).normalized;
                }
            }
            bool flag7 = false;
            for (int num19 = 0; num19 < this.tentacles.Length; num19++)
            {
                flag7 = (flag7 || !this.tentacles[num19].hasAnyGrip);
            }
            if (!this.hoverStill && !this.hangingInTentacle && flag7)
            {
                float num20 = 0f;
                int num21 = -1;
                for (int num22 = 0; num22 < this.thrusters.Length; num22++)
                {
                    float num23 = this.thrusters[num22].Utility();
                    if (num23 > num20)
                    {
                        num20 = num23;
                        num21 = num22;
                    }
                }
                num20 *= this.jetFuel;
                if (num20 > 0.05f)
                {
                    this.thrusters[num21].Activate(10 + (int)(Mathf.InverseLerp(0.05f, 0.4f, num20) * 20f));
                }
            }
            if (this.snapFrames == 0)
            {
                if (this.AI.preyInTuskChargeRange || this.AirBorne)
                {
                    this.tuskCharge = Mathf.Clamp(this.tuskCharge + 0.025f, 0f, 1f);
                }
                else
                {
                    this.tuskCharge = Mathf.Clamp(this.tuskCharge - 0.011111111f, 0f, 1f);
                }
            }
            else
            {
                Vector2 pos = this.snapAtPos;
                if (this.snapAt != null)
                {
                    pos = this.snapAt.pos;
                }
                if (this.Snapping)
                {
                    base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, pos) * 1f;
                }
                else if (this.ChargingSnap)
                {
                    base.bodyChunks[1].vel -= Custom.DirVec(base.bodyChunks[1].pos, pos) * 0.5f;
                    if (!this.AirBorne)
                    {
                        for (int num24 = 0; num24 < 4; num24++)
                        {
                            base.bodyChunks[num24].vel *= Mathf.Lerp(1f, 0.2f, num2);
                        }
                    }
                }
                this.snapFrames--;
            }
            this.lastHoverStill = this.hoverStill;
            if (movementConnection != default(MovementConnection))
            {
                this.lastConnection = movementConnection;
            }
        }

        private void StuckBehavior()
        {
            bool flag = false;
            for (int i = 0; i < 4; i++)
            {
                if (base.bodyChunks[i].ContactPoint.x != 0 || base.bodyChunks[i].ContactPoint.y != 0 || this.room.GetTile(base.bodyChunks[i].pos).wormGrass)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                for (int j = 2; j < 5; j++)
                {
                    for (float num = 0f; num < 360f; num += 36f)
                    {
                        if (SharedPhysics.RayTraceTilesForTerrain(this.room, this.room.GetTilePosition(base.mainBodyChunk.pos), this.room.GetTilePosition(base.mainBodyChunk.pos + Custom.DegToVec(num) * 20f * (float)j)))
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                base.bodyChunks[k].vel += Custom.DegToVec(num);
                            }
                            break;
                        }
                    }
                }
            }
            Vector2 b = Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f;
            for (int l = 0; l < 4; l++)
            {
                base.bodyChunks[l].vel += b;
            }
            bool flag2 = true;
            for (int m = 0; m < this.thrusters.Length; m++)
            {
                if (this.thrusters[m].Active)
                {
                    flag2 = false;
                    break;
                }
            }
            if (this.jetFuel > 0.2f && flag2 && this.thrusters.Length != 0)
            {
                this.thrusters[UnityEngine.Random.Range(0, this.thrusters.Length)].Activate(5 + UnityEngine.Random.Range(0, 10));
            }
        }

        public void AccessSkyGate(WorldCoordinate start, WorldCoordinate dest)
        {
            this.room.game.shortcuts.CreatureTakeFlight(this, AbstractRoomNode.Type.SkyExit, start, dest);
            if (ModManager.CoopAvailable)
            {
                foreach (Creature.Grasp grasp in base.grasps)
                {
                    if (grasp != null && grasp.grabbed != null && grasp.grabbed is Player)
                    {
                        (grasp.grabbed as Player).PermaDie();
                    }
                }
            }
        }

        public void AirBrake(int frames)
        {
            this.landingBrake = frames;
            this.landingBrakePos = base.bodyChunks[1].pos;
            if (frames > 5)
            {
                this.room.PlaySound(SoundID.Vulture_Jets_Air_Brake, base.mainBodyChunk);
            }
        }

        private void TakeOff()
        {
            this.timeSinceLastTakeoff = 0;
            for (int i = 0; i < this.tentacles.Length; i++)
            {
                this.tentacles[i].SwitchMode(AlcedoTentacle.Mode.Fly);
            }
            if (base.graphicsModule != null)
            {
                (base.graphicsModule as AlcedoGraphics).MakeColorWave(UnityEngine.Random.Range(30, 60));
            }
            this.AirBrake(5);
            this.thrusters[0].Activate(30);
            this.thrusters[1].Activate(30);
        }

        private void WaterBehavior()
        {
        }

        private void CheckFlip()
        {
            if (Custom.DistanceToLine(base.bodyChunks[2].pos, base.bodyChunks[0].pos, base.bodyChunks[1].pos) < 0f)
            {
                Vector2 pos = base.bodyChunks[2].pos;
                Vector2 vel = base.bodyChunks[2].vel;
                Vector2 lastPos = base.bodyChunks[2].lastPos;
                base.bodyChunks[2].pos = base.bodyChunks[3].pos;
                base.bodyChunks[2].vel = base.bodyChunks[3].vel;
                base.bodyChunks[2].lastPos = base.bodyChunks[3].lastPos;
                base.bodyChunks[3].pos = pos;
                base.bodyChunks[3].vel = vel;
                base.bodyChunks[3].lastPos = lastPos;
            }
        }

        public void Snap(BodyChunk snapAt)
        {
            this.tuskCharge = 0f;
            this.snapAt = snapAt;
            this.snapFrames = 49;
            this.room.PlaySound(SoundID.Vulture_Peck, base.bodyChunks[4]);
        }

        private void UpdateNeck()
        {
            this.neck.Update();
            if (this.AI.stuckTracker.closeToGoalButNotSeeingItTracker.counter > this.AI.stuckTracker.closeToGoalButNotSeeingItTracker.counterMin)
            {
                List<IntVector2> list = null;
                float num = this.AI.stuckTracker.closeToGoalButNotSeeingItTracker.Stuck;
                this.neck.MoveGrabDest(this.room.MiddleOfTile(this.AI.pathFinder.GetDestination), ref list);
                base.bodyChunks[4].vel += Custom.DirVec(base.bodyChunks[4].pos, this.room.MiddleOfTile(this.AI.pathFinder.GetDestination)) * 10f * num;
                base.bodyChunks[4].pos += Custom.DirVec(base.bodyChunks[4].pos, this.room.MiddleOfTile(this.AI.pathFinder.GetDestination)) * 10f * num;
                for (int i = 0; i < this.neck.tChunks.Length; i++)
                {
                    this.neck.tChunks[i].vel += Custom.DirVec(this.neck.tChunks[i].pos, this.room.MiddleOfTile(this.AI.pathFinder.GetDestination)) * 5f * num;
                }
                if (num > 0.95f)
                {
                    base.bodyChunks[4].collideWithTerrain = false;
                    return;
                }
            }
            base.bodyChunks[4].collideWithTerrain = true;
            for (int j = 0; j < this.neck.tChunks.Length; j++)
            {
                this.neck.tChunks[j].vel *= 0.95f;
                Tentacle.TentacleChunk tentacleChunk = this.neck.tChunks[j];
                tentacleChunk.vel.y = tentacleChunk.vel.y - (this.neck.limp ? 0.7f : 0.1f);
                this.neck.tChunks[j].vel += Custom.DirVec(base.bodyChunks[1].pos, base.bodyChunks[0].pos) * ((j == 0) ? 1.2f : 0.8f);
                this.neck.tChunks[j].vel -= this.neck.connectedChunk.vel;
                this.neck.tChunks[j].vel *= (this.AirBorne ? 0.2f : 0.75f);
                this.neck.tChunks[j].vel += this.neck.connectedChunk.vel;
            }
            this.neck.limp = !base.Consious;
            float num2 = (this.neck.backtrackFrom == -1) ? 0.5f : 0f;
            if (base.grasps[0] == null)
            {
                Vector2 a = Custom.DirVec(base.bodyChunks[4].pos, this.neck.tChunks[this.neck.tChunks.Length - 1].pos);
                float num3 = Vector2.Distance(base.bodyChunks[4].pos, this.neck.tChunks[this.neck.tChunks.Length - 1].pos);
                base.bodyChunks[4].pos -= (6f - num3) * a * (1f - num2);
                base.bodyChunks[4].vel -= (6f - num3) * a * (1f - num2);
                this.neck.tChunks[this.neck.tChunks.Length - 1].pos += (6f - num3) * a * num2;
                this.neck.tChunks[this.neck.tChunks.Length - 1].vel += (6f - num3) * a * num2;
                base.bodyChunks[4].vel += Custom.DirVec(this.neck.tChunks[this.neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * (this.AirBorne ? 2f : 6f) * (1f - num2);
                base.bodyChunks[4].vel += Custom.DirVec(this.neck.tChunks[this.neck.tChunks.Length - 1].pos, base.bodyChunks[4].pos) * (this.AirBorne ? 2f : 6f) * (1f - num2);
                this.neck.tChunks[this.neck.tChunks.Length - 1].vel -= Custom.DirVec(this.neck.tChunks[this.neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * (this.AirBorne ? 1f : 3f) * num2;
                this.neck.tChunks[this.neck.tChunks.Length - 2].vel -= Custom.DirVec(this.neck.tChunks[this.neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * (this.AirBorne ? 1f : 3f) * num2;
            }
            if (!base.Consious)
            {
                return;
            }
            Vector2 pos = this.snapAtPos;
            if (this.snapAt != null)
            {
                pos = this.snapAt.pos;
            }
            if (this.ChargingSnap)
            {
                base.bodyChunks[4].vel += (base.mainBodyChunk.pos + Custom.DirVec(base.mainBodyChunk.pos, pos) * 50f - base.bodyChunks[4].pos) / 6f;
                this.neck.tChunks[this.neck.tChunks.Length - 1].vel -= Custom.DirVec(base.bodyChunks[4].pos, pos) * 10f * num2;
                return;
            }
            if (this.Snapping)
            {
                base.bodyChunks[4].vel += Custom.DirVec(base.bodyChunks[4].pos, pos) * 15f;
                this.neck.tChunks[this.neck.tChunks.Length - 1].vel -= Custom.DirVec(base.bodyChunks[4].pos, pos) * num2;
                return;
            }
            Vector2 vector;
            if (this.AI.creatureLooker.lookCreature == null)
            {
                vector = this.room.MiddleOfTile(this.AI.pathFinder.GetDestination);
            }
            else if (this.AI.creatureLooker.lookCreature.VisualContact)
            {
                vector = this.AI.creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos;
            }
            else
            {
                vector = this.room.MiddleOfTile(this.AI.creatureLooker.lookCreature.BestGuessForPosition());
            }
            if (Custom.DistLess(vector, base.mainBodyChunk.pos, 220f) && !this.room.VisualContact(vector, base.bodyChunks[4].pos))
            {
                List<IntVector2> list2 = null;
                this.neck.MoveGrabDest(vector, ref list2);
            }
            else if (this.neck.backtrackFrom == -1)
            {
                this.neck.floatGrabDest = null;
            }
            Vector2 a2 = Custom.DirVec(base.bodyChunks[4].pos, vector);
            if (base.grasps[0] == null)
            {
                this.neck.tChunks[this.neck.tChunks.Length - 1].vel += a2 * num2;
                this.neck.tChunks[this.neck.tChunks.Length - 2].vel -= a2 * 0.5f * num2;
                base.bodyChunks[4].vel += a2 * 4f * (1f - num2);
            }
            else
            {
                this.neck.tChunks[this.neck.tChunks.Length - 1].vel += a2 * 2f * num2;
                this.neck.tChunks[this.neck.tChunks.Length - 2].vel -= a2 * 2f * num2;
                base.grasps[0].grabbedChunk.vel += a2 / base.grasps[0].grabbedChunk.mass;
            }
            if (Custom.DistLess(base.bodyChunks[4].pos, vector, 80f))
            {
                for (int k = 0; k < this.neck.tChunks.Length; k++)
                {
                    this.neck.tChunks[k].vel -= a2 * Mathf.InverseLerp(80f, 20f, Vector2.Distance(base.bodyChunks[4].pos, vector)) * 8f * num2;
                }
            }
        }

        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);
            if (!this.IsMiros && this.Snapping && myChunk == 4 && base.grasps[0] == null)
            {
                if (this.AI.OnlyHurtDontGrab(otherObject))
                {
                    if (otherObject is Creature)
                    {
                        (otherObject as Creature).Violence(base.bodyChunks[myChunk], new Vector2?(base.bodyChunks[myChunk].vel * 2f), otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Bite, 1.1f, 30f);
                    }
                }
                else
                {
                    this.Grab(otherObject, 0, otherChunk, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, true, true);
                    this.AI.creatureLooker.LookAtNothing();
                    if (otherObject is Creature)
                    {
                        (otherObject as Creature).Violence(base.bodyChunks[myChunk], new Vector2?(base.bodyChunks[myChunk].vel * 2f), otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Bite, 0.4f, 20f);
                    }
                }
                this.room.PlaySound((otherObject is Player) ? SoundID.Vulture_Grab_Player : SoundID.Vulture_Grab_NPC, base.bodyChunks[4]);
                this.snapFrames = 0;
            }
        }

        public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType type, float damage, float stunBonus)
        {
            if (this.room == null)
            {
                return;
            }
            if (!this.IsMiros)
            {
                this.AI.disencouraged += (damage / 2f + stunBonus / 460f) * (this.IsKing ? 0.3f : 1f) * ((this.room.game.IsStorySession && this.room.game.StoryCharacter == SlugcatStats.Name.Yellow) ? 1.5f : 1f);
            }
            if (hitChunk != null && hitChunk.index == 4)
            {
                stunBonus += 20f + 20f * damage;
                if (damage > 0.1f || base.stun > 10)
                {
                    this.snapFrames = 0;
                }
                bool flag = directionAndMomentum != null && source != null && !this.SpearStick(source.owner as Spear, damage, hitChunk, onAppendagePos, directionAndMomentum.Value.normalized);
                /*if (this.kingTusks != null && source != null && UnityEngine.Random.value < (base.dead ? 0.2f : 0.8f) && source.owner is Spear && directionAndMomentum != null && this.kingTusks.HitBySpear(directionAndMomentum.Value))
                {
                    if (directionAndMomentum != null)
                    {
                        hitChunk.vel += directionAndMomentum.Value * 0.8f;
                    }
                    damage *= 0.1f;
                }
                else */
                if (!flag)
                {
                    if (!this.IsMiros && (base.State as Alcedo.AlcedoState).mask && (damage > 0.9f || (ModManager.MSC && source != null && source.owner is LillyPuck)) && (source == null || !(source.owner is Weapon) || (source.owner as Weapon).meleeHitChunk == null))
                    {
                        this.DropMask(((directionAndMomentum != null) ? (directionAndMomentum.Value / 5f) : new Vector2(0f, 0f)) + Custom.RNV() * 7f * UnityEngine.Random.value);
                    }
                    damage *= 1.5f;
                }
                else
                {
                    Vector2 pos = (source != null) ? Vector2.Lerp(hitChunk.pos, source.pos, 0.5f) : hitChunk.pos;
                    if (damage > 0.1f || stunBonus > 30f)
                    {
                        this.room.AddObject(new StationaryEffect(pos, new Color(1f, 1f, 1f), null, StationaryEffect.EffectType.FlashingOrb));
                        for (int i = 0; i < 3 + (int)Mathf.Min(damage * 3f, 9f); i++)
                        {
                            this.room.AddObject(new Spark(pos, Custom.RNV() * UnityEngine.Random.value * 12f, new Color(1f, 1f, 1f), null, 6, 16));
                        }
                    }
                    if (directionAndMomentum != null)
                    {
                        hitChunk.vel += directionAndMomentum.Value * 0.8f;
                    }
                    damage *= 0.1f;
                }
            }
            if (onAppendagePos != null)
            {
                if (onAppendagePos.appendage.appIndex == 0 && type != Creature.DamageType.Blunt)
                {
                    damage *= 2f;
                }
                else if (onAppendagePos.appendage.appIndex > 0 && (!this.IsMiros || type != Creature.DamageType.Explosion))
                {
                    this.tentacles[onAppendagePos.appendage.appIndex - 1].Damage(type, damage, stunBonus);
                    damage /= 10f;
                    stunBonus /= 20f;
                }
            }
            if (this.IsMiros && !base.dead && base.grasps[0] == null && type != Creature.DamageType.Explosion)
            {
                this.FireLaser();
            }
            base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
        }

        public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos appPos, Vector2 direction)
        {
            return this.IsMiros || ((chunk == null || chunk.index != 4 || base.dead || !(base.State as Alcedo.AlcedoState).mask || Vector2.Dot(direction.normalized, Custom.DirVec(this.neck.tChunks[this.neck.tChunks.Length - 1].pos, chunk.pos)) >= -0.88f) && base.SpearStick(source, dmg, chunk, appPos, direction));
        }

        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            base.TerrainImpact(chunk, direction, speed, firstContact);
            if (speed > 1.5f && firstContact)
            {
                float num = Mathf.InverseLerp(6f, 14f, speed);
                if (this.IsMiros)
                {
                    if (num < 1f)
                    {
                        this.room.PlaySound(SoundID.Miros_Light_Terrain_Impact, base.mainBodyChunk, false, 1f - num, 1f);
                    }
                    if (num > 0f)
                    {
                        this.room.PlaySound(SoundID.Miros_Heavy_Terrain_Impact, base.mainBodyChunk, false, num, 1f);
                        return;
                    }
                }
                else
                {
                    if (num < 1f)
                    {
                        this.room.PlaySound(SoundID.Vulture_Light_Terrain_Impact, base.mainBodyChunk, false, 1f - num, 1f);
                    }
                    if (num > 0f)
                    {
                        this.room.PlaySound(SoundID.Vulture_Heavy_Terrain_Impact, base.mainBodyChunk, false, num, 1f);
                    }
                }
            }
        }

        public void DropMask(Vector2 violenceDir)
        {
            if (!(base.State as Alcedo.AlcedoState).mask)
            {
                return;
            }
            (base.State as Alcedo.AlcedoState).mask = false;
            AbstractPhysicalObject abstractPhysicalObject = new VultureMask.AbstractVultureMask(this.room.world, null, this.abstractPhysicalObject.pos, this.room.game.GetNewID(), base.abstractCreature.ID.RandomSeed, this.IsKing);
            this.room.abstractRoom.AddEntity(abstractPhysicalObject);
            abstractPhysicalObject.pos = base.abstractCreature.pos;
            abstractPhysicalObject.RealizeInRoom();
            abstractPhysicalObject.realizedObject.firstChunk.HardSetPosition(base.bodyChunks[4].pos);
            abstractPhysicalObject.realizedObject.firstChunk.vel = base.bodyChunks[4].vel + violenceDir;
            (abstractPhysicalObject.realizedObject as VultureMask).fallOffVultureMode = 1f;
            if (this.killTag != null)
            {
                SocialMemory.Relationship orInitiateRelationship = base.State.socialMemory.GetOrInitiateRelationship(this.killTag.ID);
                orInitiateRelationship.like = -1f;
                orInitiateRelationship.tempLike = -1f;
                orInitiateRelationship.know = 1f;
            }
        }

        public override void Stun(int st)
        {
            this.snapFrames = 0;
            if (this.IsMiros)
            {
                this.LoseAllGrasps();
            }
            base.Stun(st);
        }

        public void Carry()
        {
            if (!base.Consious)
            {
                this.LoseAllGrasps();
                return;
            }
            BodyChunk grabbedChunk = base.grasps[0].grabbedChunk;
            float num = 1f;
            if (ModManager.MSC && grabbedChunk.owner is Player && (grabbedChunk.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                num = 10f;
            }
            if (UnityEngine.Random.value < 0.008333334f * num && (!(grabbedChunk.owner is Creature) || base.Template.CreatureRelationship((grabbedChunk.owner as Creature).Template).type != CreatureTemplate.Relationship.Type.Eats))
            {
                this.LoseAllGrasps();
                return;
            }
            float num2 = grabbedChunk.mass / (grabbedChunk.mass + base.bodyChunks[4].mass);
            float num3 = grabbedChunk.mass / (grabbedChunk.mass + base.bodyChunks[0].mass);
            if (this.neck.backtrackFrom != -1 || this.enteringShortCut != null)
            {
                num2 = 0f;
                num3 = 0f;
            }
            if (!Custom.DistLess(grabbedChunk.pos, this.neck.tChunks[this.neck.tChunks.Length - 1].pos, 20f))
            {
                Vector2 a = Custom.DirVec(grabbedChunk.pos, this.neck.tChunks[this.neck.tChunks.Length - 1].pos);
                float num4 = Vector2.Distance(grabbedChunk.pos, this.neck.tChunks[this.neck.tChunks.Length - 1].pos);
                grabbedChunk.pos -= (20f - num4) * a * (1f - num2);
                grabbedChunk.vel -= (20f - num4) * a * (1f - num2);
                this.neck.tChunks[this.neck.tChunks.Length - 1].pos += (20f - num4) * a * num2;
                this.neck.tChunks[this.neck.tChunks.Length - 1].vel += (20f - num4) * a * num2;
            }
            if (this.enteringShortCut == null)
            {
                base.bodyChunks[4].pos = Vector2.Lerp(this.neck.tChunks[this.neck.tChunks.Length - 1].pos, grabbedChunk.pos, 0.1f);
                base.bodyChunks[4].vel = this.neck.tChunks[this.neck.tChunks.Length - 1].vel;
            }
            float num5 = 70f;
            if (!Custom.DistLess(base.mainBodyChunk.pos, grabbedChunk.pos, num5))
            {
                Vector2 a2 = Custom.DirVec(grabbedChunk.pos, base.bodyChunks[0].pos);
                float num6 = Vector2.Distance(grabbedChunk.pos, base.bodyChunks[0].pos);
                grabbedChunk.pos -= (num5 - num6) * a2 * (1f - num3);
                grabbedChunk.vel -= (num5 - num6) * a2 * (1f - num3);
                base.bodyChunks[0].pos += (num5 - num6) * a2 * num3;
                base.bodyChunks[0].vel += (num5 - num6) * a2 * num3;
            }
        }

        public override void Die()
        {
            this.surfaceFriction = 0.3f;
            base.Die();
        }

        public override Color ShortCutColor()
        {
            if (base.graphicsModule != null)
            {
                return HSLColor.Lerp((base.graphicsModule as AlcedoGraphics).ColorA, (base.graphicsModule as AlcedoGraphics).ColorB, 0.5f).rgb;
            }
            return base.ShortCutColor();
        }

        public Vector2 AppendagePosition(int appendage, int segment)
        {
            segment--;
            if (appendage == 0)
            {
                if (segment < 0)
                {
                    return base.mainBodyChunk.pos;
                }
                if (segment >= this.neck.tChunks.Length)
                {
                    return base.bodyChunks[4].pos;
                }
                return this.neck.tChunks[segment].pos;
            }
            else
            {
                if (segment < 0)
                {
                    return this.tentacles[appendage - 1].connectedChunk.pos;
                }
                return this.tentacles[appendage - 1].tChunks[segment].pos;
            }
        }

        public void ApplyForceOnAppendage(PhysicalObject.Appendage.Pos pos, Vector2 momentum)
        {
            if (pos.appendage.appIndex != 0)
            {
                if (pos.prevSegment > 0)
                {
                    this.tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment - 1].pos += momentum / 0.04f * (1f - pos.distanceToNext);
                    this.tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment - 1].vel += momentum / 0.04f * (1f - pos.distanceToNext);
                }
                else
                {
                    this.tentacles[pos.appendage.appIndex - 1].connectedChunk.pos += momentum / this.tentacles[pos.appendage.appIndex - 1].connectedChunk.mass * (1f - pos.distanceToNext);
                    this.tentacles[pos.appendage.appIndex - 1].connectedChunk.vel += momentum / this.tentacles[pos.appendage.appIndex - 1].connectedChunk.mass * (1f - pos.distanceToNext);
                }
                this.tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment].pos += momentum / 0.04f * pos.distanceToNext;
                this.tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment].vel += momentum / 0.04f * pos.distanceToNext;
                return;
            }
            if (pos.prevSegment > 0)
            {
                this.neck.tChunks[pos.prevSegment - 1].pos += momentum / 0.1f * (1f - pos.distanceToNext);
                this.neck.tChunks[pos.prevSegment - 1].vel += momentum / 0.05f * (1f - pos.distanceToNext);
            }
            else
            {
                base.mainBodyChunk.pos += momentum / base.mainBodyChunk.mass * (1f - pos.distanceToNext);
                base.mainBodyChunk.vel += momentum / base.mainBodyChunk.mass * (1f - pos.distanceToNext);
            }
            if (pos.prevSegment < this.neck.tChunks.Length - 1)
            {
                this.neck.tChunks[pos.prevSegment].pos += momentum / 0.1f * pos.distanceToNext;
                this.neck.tChunks[pos.prevSegment].vel += momentum / 0.05f * pos.distanceToNext;
                return;
            }
            base.bodyChunks[4].pos += momentum / base.bodyChunks[4].mass * pos.distanceToNext;
            base.bodyChunks[4].vel += momentum / base.bodyChunks[4].mass * pos.distanceToNext;
        }

        public void SnapTowards(Vector2 pos)
        {
            this.snapAtPos = pos;
            this.Snap(null);
        }

        public bool MostlyConsious
        {
            get
            {
                return base.stun < 40 && !base.dead;
            }
        }

        public bool CheckTentacleModeOr(AlcedoTentacle.Mode mode)
        {
            bool flag = false;
            for (int i = 0; i < this.tentacles.Length; i++)
            {
                flag = (flag || this.tentacles[i].mode == mode);
            }
            return flag;
        }

        public bool CheckTentacleModeAnd(AlcedoTentacle.Mode mode)
        {
            bool flag = true;
            for (int i = 0; i < this.tentacles.Length; i++)
            {
                flag = (flag && this.tentacles[i].mode == mode);
            }
            return flag;
        }

        public int TentacleMaxReleaseInd()
        {
            float num = -1f;
            int result = -1;
            for (int i = 0; i < this.tentacles.Length; i++)
            {
                if (this.tentacles[i].ReleaseScore() > num || num == -1f)
                {
                    num = this.tentacles[i].ReleaseScore();
                    result = i;
                }
            }
            return result;
        }

        public BodyChunk Head()
        {
            return base.bodyChunks[4];
        }

        public void JawSlamShut()
        {
            Vector2 a = Custom.DirVec(this.neck.Tip.pos, this.Head().pos);
            this.neck.Tip.vel -= a * 10f;
            this.neck.Tip.pos += a * 20f;
            this.Head().pos += a * 20f;
            int num = 0;
            int num2 = 0;
            while (num2 < this.room.abstractRoom.creatures.Count && base.grasps[0] == null)
            {
                Creature realizedCreature = this.room.abstractRoom.creatures[num2].realizedCreature;
                if (this.room.abstractRoom.creatures[num2] != base.abstractCreature && this.AI.DoIWantToBiteCreature(this.room.abstractRoom.creatures[num2]) && realizedCreature != null && realizedCreature.enteringShortCut == null && !realizedCreature.inShortcut)
                {
                    int num3 = 0;
                    while (num3 < realizedCreature.bodyChunks.Length && base.grasps[0] == null)
                    {
                        if (Custom.DistLess(this.Head().pos + a * 20f, realizedCreature.bodyChunks[num3].pos, 20f + realizedCreature.bodyChunks[num3].rad) && this.room.VisualContact(this.Head().pos, realizedCreature.bodyChunks[num3].pos))
                        {
                            if (realizedCreature == null)
                            {
                                break;
                            }
                            num = ((!(realizedCreature is Player)) ? 1 : 2);
                            if (!this.AI.OnlyHurtDontGrab(realizedCreature))
                            {
                                this.Grab(realizedCreature, 0, num3, Creature.Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, true, true);
                                this.AI.creatureLooker.LookAtNothing();
                                this.jawOpen = 0.15f;
                                this.jawVel = 0f;
                                realizedCreature.Violence(this.Head(), new Vector2?(Custom.DirVec(this.Head().pos, realizedCreature.bodyChunks[num3].pos) * 4f), realizedCreature.bodyChunks[num3], null, Creature.DamageType.Bite, 1.2f, 30f);
                                break;
                            }
                            realizedCreature.Violence(this.Head(), new Vector2?(Custom.DirVec(this.Head().pos, realizedCreature.bodyChunks[num3].pos) * 4f), realizedCreature.bodyChunks[num3], null, Creature.DamageType.Bite, 1.2f, 20f);
                            break;
                        }
                        else
                        {
                            num3++;
                        }
                    }
                    if (realizedCreature is DaddyLongLegs)
                    {
                        for (int i = 0; i < (realizedCreature as DaddyLongLegs).tentacles.Length; i++)
                        {
                            for (int j = 0; j < (realizedCreature as DaddyLongLegs).tentacles[i].tChunks.Length; j++)
                            {
                                if (Custom.DistLess(this.Head().pos + a * 20f, (realizedCreature as DaddyLongLegs).tentacles[i].tChunks[j].pos, 20f))
                                {
                                    (realizedCreature as DaddyLongLegs).tentacles[i].stun = UnityEngine.Random.Range(10, 70);
                                    for (int k = j; k < (realizedCreature as DaddyLongLegs).tentacles[i].tChunks.Length; k++)
                                    {
                                        (realizedCreature as DaddyLongLegs).tentacles[i].tChunks[k].vel += Custom.DirVec((realizedCreature as DaddyLongLegs).tentacles[i].tChunks[k].pos, (realizedCreature as DaddyLongLegs).tentacles[i].connectedChunk.pos) * Mathf.Lerp(10f, 50f, UnityEngine.Random.value);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                num2++;
            }
            if (num == 0)
            {
                this.room.PlaySound(SoundID.Miros_Beak_Snap_Miss, this.Head());
                return;
            }
            if (num == 1)
            {
                this.room.PlaySound(SoundID.Miros_Beak_Snap_Hit_Slugcat, this.Head());
                return;
            }
            this.room.PlaySound(SoundID.Miros_Beak_Snap_Hit_Other, this.Head());
        }

        public bool isLaserActive()
        {
            return !base.dead && this.laserCounter > 0 && base.grasps[0] == null;
        }

        private void FireLaser()
        {
            if (this.laserCounter <= 0 && !base.dead && this.MostlyConsious && base.graphicsModule != null && !(base.graphicsModule as AlcedoGraphics).shadowMode)
            {
                this.laserCounter = 200;
                if (this.room != null)
                {
                    this.LaserLight = new LightSource(new Vector2(-1000f, -1000f), false, new Color(0.1f, 1f, 0.1f), this);
                    this.room.AddObject(this.LaserLight);
                    this.LaserLight.HardSetAlpha(1f);
                    this.LaserLight.HardSetRad(200f);
                }
            }
        }

        public void LaserExplosion()
        {
            if (this.room == null)
            {
                return;
            }
            Vector2 pos = this.Head().pos;
            Vector2 a = Custom.DirVec(this.neck.Tip.pos, pos);
            a *= -1f;
            Vector2 corner = Custom.RectCollision(pos, pos - a * 100000f, this.room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
            IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(this.room, pos, corner);
            if (intVector != null)
            {
                Color color = new Color(1f, 0.4f, 0.3f);
                corner = Custom.RectCollision(corner, pos, this.room.TileRect(intVector.Value)).GetCorner(FloatRect.CornerLabel.D);
                this.room.AddObject(new Explosion(this.room, this, corner, 7, 250f, 6.2f, 2f, 280f, 0.25f, this, 0.3f, 160f, 1f));
                this.room.AddObject(new Explosion.ExplosionLight(corner, 280f, 1f, 7, color));
                this.room.AddObject(new Explosion.ExplosionLight(corner, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                this.room.AddObject(new ShockWave(corner, 330f, 0.045f, 5, false));
                for (int i = 0; i < 25; i++)
                {
                    Vector2 a2 = Custom.RNV();
                    if (this.room.GetTile(corner + a2 * 20f).Solid)
                    {
                        if (!this.room.GetTile(corner - a2 * 20f).Solid)
                        {
                            a2 *= -1f;
                        }
                        else
                        {
                            a2 = Custom.RNV();
                        }
                    }
                    for (int j = 0; j < 3; j++)
                    {
                        this.room.AddObject(new Spark(corner + a2 * Mathf.Lerp(30f, 60f, UnityEngine.Random.value), a2 * Mathf.Lerp(7f, 38f, UnityEngine.Random.value) + Custom.RNV() * 20f * UnityEngine.Random.value, Color.Lerp(color, new Color(1f, 1f, 1f), UnityEngine.Random.value), null, 11, 28));
                    }
                    this.room.AddObject(new Explosion.FlashingSmoke(corner + a2 * 40f * UnityEngine.Random.value, a2 * Mathf.Lerp(4f, 20f, Mathf.Pow(UnityEngine.Random.value, 2f)), 1f + 0.05f * UnityEngine.Random.value, new Color(1f, 1f, 1f), color, UnityEngine.Random.Range(3, 11)));
                }
                for (int k = 0; k < 6; k++)
                {
                    this.room.AddObject(new ScavengerBomb.BombFragment(corner, Custom.DegToVec(((float)k + UnityEngine.Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, UnityEngine.Random.value)));
                }
                this.room.ScreenMovement(new Vector2?(corner), default(Vector2), 0.9f);
                for (int l = 0; l < this.abstractPhysicalObject.stuckObjects.Count; l++)
                {
                    this.abstractPhysicalObject.stuckObjects[l].Deactivate();
                }
                this.room.PlaySound(SoundID.Bomb_Explode, corner);
                this.room.InGameNoise(new InGameNoise(corner, 9000f, this, 1f));
            }
        }

        public AlcedoAI AI;
        public AlcedoTentacle[] tentacles;
        public Tentacle neck;
        public IntVector2 mouseTilePos;
        public Vector2 moveDirection;
        public bool hangingInTentacle;
        public int cantFindNewGripCounter;
        private int releaseGrippingTentacle;
        public bool hoverStill;
        public bool lastHoverStill;
        public IntVector2 hoverPos;
        public int dontSwitchModesCounter;
        public int timeSinceLastTakeoff;
        public float wingFlapAmplitude;
        public float wingFlap;
        public int landingBrake;
        public Vector2 landingBrakePos;
        private Alcedo.AlcedoThruster[] thrusters;
        private float jf;
        public int stuck;
        public int stuckShake;
        public int stuckShakeDuration;
        private MovementConnection lastConnection;
        public float tuskCharge;
        public BodyChunk snapAt;
        public int snapFrames;
        private bool temporarilyAllowInForbiddenTiles;
        public ChunkSoundEmitter jetSound;
        public ChunkSoundEmitter tuskChargeSound;
        //public KingTusks kingTusks;
        public float jawOpen;
        public float lastJawOpen;
        public float jawVel;
        private float keepJawOpenPos;
        private int jawSlamPause;
        private int jawKeepOpenPause;
        public int laserCounter;
        public LightSource LaserLight;
        public bool controlledJawSnap;
        public float drown;
        public Vector2 snapAtPos;

        private class AlcedoThruster
        {
            public Vector2 ThrustVector
            {
                get
                {
                    return Custom.DegToVec(Custom.AimFromOneVectorToAnother(this.alcedo.bodyChunks[1].pos, this.alcedo.bodyChunks[0].pos) + this.angle);
                }
            }

            public bool Active
            {
                get
                {
                    return this.ThrustersControlled && (this.alcedo.landingBrake > 0 || this.thrust > 0);
                }
            }

            public void Activate(int frames)
            {
                if (!this.ThrustersControlled)
                {
                    return;
                }
                if (this.thrust < frames)
                {
                    this.thrust = frames;
                }
            }

            public Vector2 ExhaustPos
            {
                get
                {
                    return Vector2.Lerp(this.smokeChunkA.pos, this.smokeChunkB.pos, this.smokeChunkLerp) + this.ThrustVector * 14f;
                }
            }

            public float Force
            {
                get
                {
                    float num = Mathf.Min(1f, this.alcedo.jetFuel * 2f);
                    num *= Mathf.Min(1f, (float)this.thrust / 5f);
                    num = Mathf.Pow(num, 0.4f);
                    if (this.alcedo.landingBrake <= 0 || num >= 0.5f)
                    {
                        return num;
                    }
                    if (!this.alcedo.IsMiros)
                    {
                        return 0.5f;
                    }
                    return 2.7f;
                }
            }

            public bool ThrustersControlled
            {
                get
                {
                    return !this.alcedo.safariControlled || (this.alcedo.inputWithDiagonals != null && (this.alcedo.inputWithDiagonals.Value.AnyDirectionalInput || this.alcedo.inputWithDiagonals.Value.jmp));
                }
            }

            public AlcedoThruster(Alcedo alcedo, int smokeChunkA, int smokeChunkB, float smokeChunkLerp, float angle)
            {
                this.alcedo = alcedo;
                this.smokeChunkA = alcedo.bodyChunks[smokeChunkA];
                this.smokeChunkB = alcedo.bodyChunks[smokeChunkB];
                this.smokeChunkLerp = smokeChunkLerp;
                this.angle = angle;
            }

            public void Update(bool eu)
            {
                if (this.thrust > 0)
                {
                    this.thrust--;
                }
                if (this.Active)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        this.alcedo.bodyChunks[i].vel -= this.ThrustVector * (this.alcedo.IsKing ? 1.2f : 0.8f) * this.Force;
                    }
                    this.alcedo.jetFuel -= 1f / (this.alcedo.IsKing ? 60f : 40f);
                    if (this.alcedo.room.BeingViewed)
                    {
                        if (!this.alcedo.room.PointSubmerged(this.ExhaustPos))
                        {/*
                            if (this.smoke == null)
                            {
                                this.StartSmoke();
                            }
                            this.smoke.MoveTo(this.ExhaustPos, eu);
                            this.smoke.EmitSmoke(Vector2.Lerp(this.smokeChunkA.vel, this.smokeChunkB.vel, this.smokeChunkLerp) + this.ThrustVector * (this.alcedo.IsKing ? 55f : 45f), this.Force);*/
                        }
                        else
                        {
                            this.alcedo.room.AddObject(new Bubble(this.ExhaustPos, this.ThrustVector * 45f * this.Force, false, false));
                        }
                    }
                    if (!this.lastActive)
                    {
                        this.alcedo.room.PlaySound(SoundID.Vulture_Jet_Start, this.smokeChunkA);
                    }
                }
                else if (this.lastActive)
                {
                    this.alcedo.room.PlaySound(SoundID.Vulture_Jet_Stop, this.smokeChunkA);
                }/*
                if (this.smoke != null)
                {
                    for (int j = 0; j < this.alcedo.tentacles.Length; j++)
                    {
                        if (this.alcedo.tentacles[j].mode == AlcedoTentacle.Mode.Fly)
                        {
                            int num = this.alcedo.tentacles[j].tChunks.Length - 1;
                            this.smoke.WindDrag(this.alcedo.tentacles[j].tChunks[num].pos, Custom.DirVec(this.alcedo.tentacles[j].connectedChunk.pos, this.alcedo.tentacles[j].tChunks[num].pos) * 2f * Mathf.Sin(6.2831855f * this.alcedo.wingFlap) + this.alcedo.tentacles[j].tChunks[num].vel * 0.4f, 120f);
                            if (this.alcedo.wingFlap > 0.2f && this.alcedo.wingFlap < 0.4f)
                            {
                                this.smoke.WindPuff(this.alcedo.tentacles[j].tChunks[num].pos, 4f, 240f);
                            }
                        }
                    }
                    if (this.smoke.slatedForDeletetion || this.alcedo.room != this.smoke.room)
                    {
                        this.smoke = null;
                    }
                }*/
                this.lastActive = this.Active;
            }

            public float Utility()
            {
                Vector2 a = new Vector2(0f, 0f);
                for (int i = 0; i < this.alcedo.bodyChunks.Length; i++)
                {
                    a += this.alcedo.bodyChunks[i].vel;
                }
                a /= (float)this.alcedo.bodyChunks.Length;
                return Mathf.Max(0f, -Vector2.Dot(this.alcedo.moveDirection, a.normalized)) * Mathf.Max(0f, Vector2.Dot(this.alcedo.moveDirection, -this.ThrustVector));
            }

            private void StartSmoke()
            {/*
                this.smoke = new NewAlcedoSmoke(this.alcedo.room, this.ExhaustPos, this.alcedo);
                this.alcedo.room.AddObject(this.smoke);*/
            }

            public Alcedo alcedo;
            public BodyChunk smokeChunkA;
            public BodyChunk smokeChunkB;
            public float smokeChunkLerp;
            public float angle;
            private int thrust;
            public bool lastActive;
        }

        public class AlcedoState : HealthState
        {
            public AlcedoState(AbstractCreature creature) : base(creature)
            {
                bool flag = ModManager.MSC && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
                this.wingHealth = new float[flag ? 4 : 2];
                for (int i = 0; i < this.wingHealth.Length; i++)
                {
                    this.wingHealth[i] = 1f;
                }
                this.mask = !flag;
            }
            public override string ToString()
            {
                string text = base.HealthBaseSaveString() + (this.mask ? "" : "<cB>NOMASK");
                foreach (KeyValuePair<string, string> keyValuePair in this.unrecognizedSaveStrings)
                {
                    text = string.Concat(new string[]
                    {
                    text,
                    "<cB>",
                    keyValuePair.Key,
                    "<cC>",
                    keyValuePair.Value
                    });
                }
                return text;
            }
            public override void LoadFromString(string[] s)
            {
                base.LoadFromString(s);
                for (int i = 0; i < s.Length; i++)
                {
                    string text = Regex.Split(s[i], "<cC>")[0];
                    if (text != null && text == "NOMASK")
                    {
                        this.mask = false;
                    }
                }
                this.unrecognizedSaveStrings.Remove("NOMASK");
            }

            public float[] wingHealth;
            public bool mask;
        }
    }
}
