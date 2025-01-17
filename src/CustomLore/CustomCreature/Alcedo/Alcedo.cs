using MoreSlugcats;
using Noise;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TheOutsider.CustomLore.CustomObject.AlcedoMask;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature.Alcedo
{
    internal class Alcedo : AirBreatherCreature, IFlyingCreature
    {
        public bool IsKing
        {
            get
            {
                return Template.type == CreatureTemplate.Type.KingVulture;
            }
        }
        public bool IsMiros
        {
            get
            {
                return ModManager.MSC && Template.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
            }
        }
        public float jetFuel
        {
            get
            {
                return jf;
            }
            set
            {
                jf = Mathf.Clamp(value, 0f, 1f);
            }
        }
        public bool Snapping
        {
            get
            {
                return snapFrames > 0 && snapFrames <= 21;
            }
        }
        public bool ChargingSnap
        {
            get
            {
                return snapFrames > 0 && snapFrames > 21;
            }
        }
        public float TusksStuck
        {
            get
            {
                if (snapFrames <= 0f && grasps[0] == null)
                {
                    return tuskCharge;
                }
                return 1f;
            }
        }
        public bool AirBorne
        {
            get
            {
                return tentacles[0].mode == AlcedoTentacle.Mode.Fly && tentacles[1].mode == AlcedoTentacle.Mode.Fly;
            }
        }
        public override Vector2 VisionPoint
        {
            get
            {
                return bodyChunks[4].pos;
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
                return mainBodyChunk.pos;
            }
        }

        public Alcedo(AbstractCreature abstractCreature, World world) : base(abstractCreature, world)
        {
            wingLength = 10f;//10f//6.5f;
            legLength = 4f;//5f;
            bodyChunks = new BodyChunk[9];
            float num = 0.5f;//this.IsKing ? 1.4f : 1f;
            //胸部
            bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.7f * num);
            //背部
            bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 6f, 0.7f * num);
            //翅膀根部
            bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 3f, 0.6f * num);
            bodyChunks[3] = new BodyChunk(this, 3, new Vector2(0f, 0f), 3f, 0.6f * num);
            //4大概是头
            bodyChunks[4] = new BodyChunk(this, 4, new Vector2(0f, 0f), 3.5f, 0.3f * num);/*
            base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 9.5f, this.IsMiros ? 1.8f : (1.2f * num));
            base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 9.5f, this.IsMiros ? 1.8f : (1.2f * num));
            base.bodyChunks[2] = new BodyChunk(this, 2, new Vector2(0f, 0f), 9.5f, this.IsMiros ? 1.8f : (1.2f * num));
            base.bodyChunks[3] = new BodyChunk(this, 3, new Vector2(0f, 0f), 9.5f, this.IsMiros ? 1.8f : (1.2f * num));
            base.bodyChunks[4] = new BodyChunk(this, 4, new Vector2(0f, 0f), 6.5f, 0.3f * num);*/
            //腹部
            bodyChunks[5] = new BodyChunk(this, 5, new Vector2(0f, 0f), 5f, 0.4f * num);
            //臀部
            bodyChunks[6] = new BodyChunk(this, 6, new Vector2(0f, 0f), 5f, 0.4f * num);
            //后腿根部
            bodyChunks[7] = new BodyChunk(this, 7, new Vector2(0f, 0f), 2.5f, 0.4f * num);
            bodyChunks[8] = new BodyChunk(this, 8, new Vector2(0f, 0f), 2.5f, 0.4f * num);
            for (int i = 0; i < bodyChunks.Length; i++)
            {
                bodyChunks[i].restrictInRoomRange = 2400f;
                bodyChunks[i].defaultRestrictInRoomRange = 2400f;
            }
            bodyChunkConnections = new BodyChunkConnection[18];
            float wingRootSpacing = 10f;//40f
            float legRootSpacing = 8f;
            float UpperAndLowerBodySpacing = 15f;//26f
            float ForeAndHindBodySpacing = 20f;//80f;//20f;
            float wingInBodyPos = Mathf.Lerp(0f, UpperAndLowerBodySpacing, 0.5f);//越接近0越靠上//10f
            //上半身
            bodyChunkConnections[0] = new BodyChunkConnection(bodyChunks[0], bodyChunks[1], UpperAndLowerBodySpacing, BodyChunkConnection.Type.Normal, 1f, 0.5f);
            bodyChunkConnections[1] = new BodyChunkConnection(bodyChunks[2], bodyChunks[3], wingRootSpacing, BodyChunkConnection.Type.Normal, 1f, 0.5f);
            bodyChunkConnections[2] = new BodyChunkConnection(bodyChunks[2], bodyChunks[1], Mathf.Sqrt(Mathf.Pow(wingInBodyPos, 2f) + Mathf.Pow(wingRootSpacing / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
            bodyChunkConnections[3] = new BodyChunkConnection(bodyChunks[1], bodyChunks[3], Mathf.Sqrt(Mathf.Pow(wingInBodyPos, 2f) + Mathf.Pow(wingRootSpacing / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
            bodyChunkConnections[4] = new BodyChunkConnection(bodyChunks[2], bodyChunks[0], Mathf.Sqrt(Mathf.Pow(UpperAndLowerBodySpacing - wingInBodyPos, 2f) + Mathf.Pow(wingRootSpacing / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
            bodyChunkConnections[5] = new BodyChunkConnection(bodyChunks[3], bodyChunks[0], Mathf.Sqrt(Mathf.Pow(UpperAndLowerBodySpacing - wingInBodyPos, 2f) + Mathf.Pow(wingRootSpacing / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
            //头
            bodyChunkConnections[6] = new BodyChunkConnection(bodyChunks[1], bodyChunks[4], 0.7f * (IsKing ? 70f : 60f), BodyChunkConnection.Type.Pull, 0.6f, 0f);
            bodyChunkConnections[7] = new BodyChunkConnection(bodyChunks[4], bodyChunks[1], 0.7f * (IsKing ? 75f : 65f), BodyChunkConnection.Type.Pull, 1f, -1f);
            //下半身
            bodyChunkConnections[8] = new BodyChunkConnection(bodyChunks[0], bodyChunks[5], ForeAndHindBodySpacing, BodyChunkConnection.Type.Normal, 0.8f, 0.5f);
            bodyChunkConnections[9] = new BodyChunkConnection(bodyChunks[5], bodyChunks[0], ForeAndHindBodySpacing, BodyChunkConnection.Type.Normal, 0.8f, 0.5f);
            bodyChunkConnections[10] = new BodyChunkConnection(bodyChunks[1], bodyChunks[6], ForeAndHindBodySpacing, BodyChunkConnection.Type.Normal, 0.6f, 0.5f);
            bodyChunkConnections[11] = new BodyChunkConnection(bodyChunks[6], bodyChunks[1], ForeAndHindBodySpacing, BodyChunkConnection.Type.Normal, 0.6f, 0.5f);
            bodyChunkConnections[12] = new BodyChunkConnection(bodyChunks[5], bodyChunks[6], UpperAndLowerBodySpacing, BodyChunkConnection.Type.Normal, 1f, 0.5f);
            //后腿
            bodyChunkConnections[13] = new BodyChunkConnection(bodyChunks[7], bodyChunks[8], legRootSpacing, BodyChunkConnection.Type.Normal, 1f, 0.5f);
            bodyChunkConnections[14] = new BodyChunkConnection(bodyChunks[7], bodyChunks[6], Mathf.Sqrt(Mathf.Pow(wingInBodyPos, 2f) + Mathf.Pow(legRootSpacing / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
            bodyChunkConnections[15] = new BodyChunkConnection(bodyChunks[6], bodyChunks[8], Mathf.Sqrt(Mathf.Pow(wingInBodyPos, 2f) + Mathf.Pow(legRootSpacing / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
            bodyChunkConnections[16] = new BodyChunkConnection(bodyChunks[7], bodyChunks[5], Mathf.Sqrt(Mathf.Pow(UpperAndLowerBodySpacing - wingInBodyPos, 2f) + Mathf.Pow(legRootSpacing / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
            bodyChunkConnections[17] = new BodyChunkConnection(bodyChunks[8], bodyChunks[5], Mathf.Sqrt(Mathf.Pow(UpperAndLowerBodySpacing - wingInBodyPos, 2f) + Mathf.Pow(legRootSpacing / 2f, 2f)), BodyChunkConnection.Type.Normal, 1f, 0.5f);
            tentacles = new AlcedoTentacle[IsMiros ? 4 : 2];
            for (int j = 0; j < tentacles.Length; j++)
            {
                tentacles[j] = new AlcedoTentacle(this, bodyChunks[2 + j % 2], bodyChunks[2 + (1 - j % 2)], (IsKing ? 9f : 7f) * wingLength * 4f, j);
            }
            legs = new AlcedoLeg[2];
            for (int j = 0; j < legs.Length; j++)
            {
                legs[j] = new AlcedoLeg(this, bodyChunks[7 + j % 2], bodyChunks[7 + (1 - j % 2)], (IsKing ? 9f : 7f) * wingLength * 1.5f, j);
            }
            waist = new Tentacle(this, bodyChunks[0], Mathf.Max(20f, UpperAndLowerBodySpacing));//长度不能小于20f
            waist.tProps = new Tentacle.TentacleProps(false, true, true, 0.5f, 0f, 0.5f, 1.8f, 0.2f, 1.2f, 10f, 0.25f, 3f, 15, 20, 6, 0);
            waist.tChunks = new Tentacle.TentacleChunk[4];
            for (int k = 0; k < waist.tChunks.Length; k++)
            {
                waist.tChunks[k] = new Tentacle.TentacleChunk(waist, k, (k + 1) / (float)waist.tChunks.Length, 9f);// (this.IsKing ? 6f : 5f)
                waist.tChunks[k].collideWithTerrain = false;
            }
            waist.tChunks[waist.tChunks.Length - 1].rad = 11f;
            waist.stretchAndSqueeze = 0f;
            neck = new Tentacle(this, bodyChunks[1], 50f); //0.7f * (this.IsKing ? 6f : 5f) * 20f
            neck.tProps = new Tentacle.TentacleProps(false, false, true, 0.5f, 0f, 0.5f, 1.8f, 0.2f, 1.2f, 10f, 0.25f, 3f, 15, 20, 6, 0);
            neck.tChunks = new Tentacle.TentacleChunk[4];
            for (int k = 0; k < neck.tChunks.Length; k++)
            {
                neck.tChunks[k] = new Tentacle.TentacleChunk(neck, k, (k + 1) / (float)neck.tChunks.Length, 5f);// (this.IsKing ? 6f : 5f)
            }
            neck.tChunks[0].rad = 8f;
            neck.tChunks[1].rad = 7f;
            neck.tChunks[neck.tChunks.Length - 1].rad = 7f;
            neck.stretchAndSqueeze = 0f;
            lastConnection = new MovementConnection(MovementConnection.MovementType.Standard, new WorldCoordinate(0, 0, 0, 0), new WorldCoordinate(0, 0, 0, 0), 0);
            thrusters = new AlcedoThruster[4];
            jetFuel = 1f;
            thrusters[0] = new AlcedoThruster(this, 2, 0, 0.5f, 15f);
            thrusters[1] = new AlcedoThruster(this, 3, 0, 0.5f, -15f);
            thrusters[2] = new AlcedoThruster(this, 2, 1, 0.2f, 100f);
            thrusters[3] = new AlcedoThruster(this, 3, 1, 0.2f, -100f);
            GoThroughFloors = true;
            wingFlapAmplitude = 1f;
            mouseTilePos = abstractCreature.pos.Tile;
            airFriction = 0.99f;
            gravity = 0.9f;
            bounce = 0.1f;
            surfaceFriction = 0.35f;
            collisionLayer = 1;
            waterFriction = 0.9f;
            buoyancy = 0.92f;/*
            if (this.IsKing)
            {
                this.kingTusks = new KingTusks(this);
            }*/
        }

        public override void InitiateGraphicsModule()
        {
            if (graphicsModule == null)
            {
                graphicsModule = new AlcedoGraphics(this);
            }
        }

        public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
        {
            base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
            Vector2 a = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
            for (int i = 0; i < bodyChunks.Length; i++)
            {
                bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - a * (i == 1 ? 10f : 5f) + Custom.DegToVec(UnityEngine.Random.value * 360f);
                bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
                bodyChunks[i].vel = a * 5f;
            }
            for (int j = 0; j < tentacles.Length; j++)
            {
                tentacles[j].Reset(tentacles[j].connectedChunk.pos);
            }
            for (int j = 0; j < legs.Length; j++)
            {
                legs[j].Reset(legs[j].connectedChunk.pos);
            }
            waist.Reset(mainBodyChunk.pos);
            neck.Reset(mainBodyChunk.pos);
            shortcutDelay = 80;
            if (graphicsModule != null)
            {
                graphicsModule.Reset();
            }
        }

        public void SpawnFlyingCreature(WorldCoordinate coord)
        {
        }

        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);
            if (graphicsModule != null)
            {
                graphicsModule.Reset();
            }
        }

        public override void NewRoom(Room room)
        {
            for (int i = 0; i < tentacles.Length; i++)
            {
                tentacles[i].NewRoom(room);
            }
            for (int i = 0; i < legs.Length; i++)
            {
                legs[i].NewRoom(room);
            }
            waist.NewRoom(room);
            neck.NewRoom(room);/*
            if (this.kingTusks != null)
            {
                this.kingTusks.NewRoom(room);
            }*/
            base.NewRoom(room);
        }

        public override void Update(bool eu)
        {
            CheckFlip();
            base.Update(eu);
            if (room == null)
            {
                return;
            }
            if (temporarilyAllowInForbiddenTiles && room.aimap.TileAccessibleToCreature(room.GetTilePosition(mainBodyChunk.pos), Template))
            {
                temporarilyAllowInForbiddenTiles = false;
            }
            if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
            {
                bodyChunks[1].vel += Custom.DirVec(bodyChunks[1].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 24f;//*14f
                for (int i = 0; i < bodyChunks.Length; i++)
                {
                    bodyChunks[i].vel *= 0.9f;
                }
                Stun(12);
            }
            hangingInTentacle = false;
            hangingInLeg = false;
            for (int j = 0; j < tentacles.Length; j++)
            {
                tentacles[j].Update();
            }
            for (int j = 0; j < legs.Length; j++)
            {
                legs[j].Update();
            }
            if (hangingInTentacle)
            {
                cantFindNewGripCounter += 2;
                if (cantFindNewGripCounter > (IsMiros ? 200 : 400))
                {
                    for (int k = 0; k < tentacles.Length; k++)
                    {
                        if (tentacles[k].hasAnyGrip)
                        {
                            tentacles[k].ReleaseGrip();
                        }
                    }
                }
            }
            else if (cantFindNewGripCounter > 0)
            {
                cantFindNewGripCounter--;
            }
            if (hangingInTentacle)
            {
                cantFindLegNewGripCounter += 2;
                if (cantFindLegNewGripCounter > (IsMiros ? 200 : 400))
                {
                    for (int k = 0; k < legs.Length; k++)
                    {
                        if (legs[k].hasAnyGrip)
                        {
                            legs[k].ReleaseGrip();
                        }
                    }
                }
            }
            else if (cantFindLegNewGripCounter > 0)
            {
                cantFindLegNewGripCounter--;
            }
            if (enteringShortCut == null)
            {
                UpdateNeck();
                UpdateWaist();
            }
            if (landingBrake > 0)
            {
                landingBrake--;
                for (int l = 0; l < bodyChunks.Length; l++)
                {
                    bodyChunks[l].vel *= 0.7f;
                }
                bodyChunks[1].vel += Vector2.ClampMagnitude(landingBrakePos - bodyChunks[1].pos, 40f) / 20f;
            }
            bool flag = true;
            float num = 0f;
            for (int m = 0; m < thrusters.Length; m++)
            {
                thrusters[m].Update(eu);
                if (thrusters[m].Active)
                {
                    flag = false;
                    if (thrusters[m].Force > num)
                    {
                        num = thrusters[m].Force;
                    }
                }
            }
            if (flag)
            {
                jetFuel += 0.008333334f;
            }
            if (UnityEngine.Random.value * 0.75f > (State as HealthState).health)
            {
                Stun(10);
            }
            if (jetSound != null)
            {
                if (num == 0f)
                {
                    jetSound.alive = false;
                    jetSound = null;
                }
                else
                {
                    jetSound.alive = true;
                    jetSound.volume = Mathf.InverseLerp(0f, 0.1f, num);
                    jetSound.pitch = Mathf.Lerp(0.4f, 2.2f, num);
                }
            }
            else if (num > 0f)
            {
                jetSound = room.PlaySound(SoundID.Vulture_Jet_LOOP, mainBodyChunk);
                jetSound.requireActiveUpkeep = true;
            }
            if (tuskChargeSound != null)
            {
                if (!ChargingSnap)
                {
                    tuskChargeSound.alive = false;
                    tuskChargeSound = null;
                }
                else
                {
                    tuskChargeSound.alive = true;
                }
            }
            else if (ChargingSnap)
            {
                tuskChargeSound = room.PlaySound(SoundID.Vulture_Jaws_Carged_LOOP, mainBodyChunk);
                tuskChargeSound.requireActiveUpkeep = true;
            }
            if (room.game.devToolsActive && Input.GetKey("g"))
            {
                BodyChunk mainBodyChunk = base.mainBodyChunk;
                mainBodyChunk.vel.y = mainBodyChunk.vel.y + 20f;
            }
            if (grasps[0] != null)
            {
                Carry();
            }
            if (Consious)
            {
                Act(eu);
            }
        }

        public void Act(bool eu)
        {
            AI.Update();
            if (IsMiros)
            {
                lastJawOpen = jawOpen;
                if (grasps[0] != null)
                {
                    jawOpen = 0.15f;
                }
                else if (jawSlamPause > 0)
                {
                    jawSlamPause--;
                }
                else
                {
                    if (isLaserActive())
                    {
                        jawKeepOpenPause = 10;
                        keepJawOpenPos = 1f;
                    }
                    if (jawVel == 0f)
                    {
                        jawVel = 0.15f;
                    }
                    if (abstractCreature.controlled && jawVel >= 0f && jawVel < 1f && !controlledJawSnap)
                    {
                        jawVel = 0f;
                        jawOpen = 0f;
                    }
                    jawOpen += jawVel;
                    if (jawKeepOpenPause > 0)
                    {
                        jawKeepOpenPause--;
                        jawOpen = Mathf.Clamp(Mathf.Lerp(jawOpen, keepJawOpenPos, UnityEngine.Random.value * 0.5f), 0f, 1f);
                    }
                    else if (UnityEngine.Random.value < 1f / (!Blinded ? 40f : 15f) && !abstractCreature.controlled)
                    {
                        jawKeepOpenPause = UnityEngine.Random.Range(10, UnityEngine.Random.Range(10, 60));
                        keepJawOpenPos = UnityEngine.Random.value >= 0.5f ? 1f : 0f;
                        jawVel = Mathf.Lerp(-0.4f, 0.4f, UnityEngine.Random.value);
                        jawOpen = Mathf.Clamp(jawOpen, 0f, 1f);
                    }
                    else if (jawOpen <= 0f)
                    {
                        jawOpen = 0f;
                        if (jawVel < -0.4f)
                        {
                            JawSlamShut();
                            controlledJawSnap = false;
                        }
                        jawVel = 0.15f;
                        jawSlamPause = 5;
                    }
                    else if (jawOpen >= 1f)
                    {
                        jawOpen = 1f;
                        jawVel = -0.5f;
                    }
                }
            }
            if (!AirBorne)
            {
                float num = 100f;
                if (mainBodyChunk.pos.x < -num || mainBodyChunk.pos.y < -num || mainBodyChunk.pos.x > room.PixelWidth + num || mainBodyChunk.pos.y > room.PixelHeight + num)
                {
                    for (int i = 0; i < tentacles.Length; i++)
                    {
                        tentacles[i].SwitchMode(AlcedoTentacle.Mode.Fly);
                    }
                }
            }
            if (wingFlap < 0.5f)
            {
                wingFlap += 1f / 20f;//1f / 30f;
            }
            else
            {
                wingFlap += 1f / 35f;//1f / 50f;
            }
            if (wingFlap > 1f)
            {
                wingFlap -= 1f;
            }
            upForceByWing = 0f;
            upForceByLeg = 0f;
            for (int j = 0; j < tentacles.Length; j++)
            {
                upForceByWing += tentacles[j].Support() * 0.5f;
                //upForceByWing += this.tentacles[j].Support() * (this.IsMiros ? 0.75f : 0.5f);
            }
            for (int j = 0; j < legs.Length; j++)
            {
                upForceByLeg += legs[j].Support() * 0.5f;
            }
            upForceByWing = Mathf.Pow(upForceByWing, 0.5f);
            upForceByWing = Mathf.Max(upForceByWing, 0.1f);
            upForceByLeg = Mathf.Pow(upForceByLeg, 0.5f);
            upForceByLeg = Mathf.Max(upForceByLeg, 0.1f);
            hoverStill = false;
            IntVector2 intVector = room.GetTilePosition(mainBodyChunk.pos);
            for (int k = 0; k < 5; k++)
            {
                if (room.aimap.TileAccessibleToCreature(intVector + Custom.fourDirectionsAndZero[k], Template))
                {
                    intVector += Custom.fourDirectionsAndZero[k];
                }
            }
            if (room == null)
            {
                return;
            }
            MovementConnection movementConnection = (AI.pathFinder as AlcedoPather).FollowPath(room.GetWorldCoordinate(intVector), true);
            AlcedoTentacle.Mode climb = AlcedoTentacle.Mode.Climb;
            if (safariControlled)
            {
                bool flag = false;
                MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
                if (movementConnection == default || !AllowableControlledAIOverride(movementConnection.type) || movementConnection.type == MovementConnection.MovementType.OutsideRoom || movementConnection.type == MovementConnection.MovementType.OffScreenMovement)
                {
                    movementConnection = default;
                    if (room.GetTile(mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                    {
                        type = MovementConnection.MovementType.ShortCut;
                    }
                    else
                    {
                        for (int l = 0; l < Custom.fourDirections.Length; l++)
                        {
                            if (room.GetTile(mainBodyChunk.pos + Custom.fourDirections[l].ToVector2() * 20f).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                            {
                                type = MovementConnection.MovementType.BigCreatureShortCutSqueeze;
                                break;
                            }
                        }
                    }
                    flag = true;
                }
                if (inputWithDiagonals != null)
                {
                    if ((!IsMiros || isLaserActive()) && inputWithDiagonals.Value.thrw && (inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0))
                    {
                        Vector2 p = bodyChunks[4].pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 200f;
                        bodyChunks[4].vel += Custom.DirVec(bodyChunks[4].pos, p) * 15f;
                        neck.tChunks[neck.tChunks.Length - 1].vel -= Custom.DirVec(bodyChunks[4].pos, p) * upForceByWing;
                    }
                    else if ((inputWithDiagonals.Value.x != 0 || inputWithDiagonals.Value.y != 0) && flag)
                    {
                        movementConnection = new MovementConnection(type, room.GetWorldCoordinate(mainBodyChunk.pos), room.GetWorldCoordinate(mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
                    }
                    if (inputWithDiagonals.Value.jmp)
                    {
                        climb = AlcedoTentacle.Mode.Fly;
                        if (!lastInputWithDiagonals.Value.jmp)
                        {
                            bool flag2 = false;
                            for (int m = 0; m < tentacles.Length; m++)
                            {
                                if (tentacles[m].mode == AlcedoTentacle.Mode.Climb)
                                {
                                    flag2 = true;
                                    break;
                                }
                            }
                            if (flag2)
                            {
                                TakeOff();
                            }
                        }
                        for (int n = 0; n < tentacles.Length; n++)
                        {
                            tentacles[n].SwitchMode(AlcedoTentacle.Mode.Fly);
                        }
                    }
                    if (IsMiros && inputWithDiagonals.Value.pckp && !lastInputWithDiagonals.Value.pckp)
                    {
                        controlledJawSnap = true;
                    }
                    if (!IsMiros && inputWithDiagonals.Value.pckp && !lastInputWithDiagonals.Value.pckp && snapFrames == 0)
                    {
                        if (AI.focusCreature != null && AI.focusCreature.VisualContact)
                        {
                            Creature realizedCreature = AI.focusCreature.representedCreature.realizedCreature;
                            if (realizedCreature.bodyChunks.Length != 0)
                            {
                                BodyChunk bodyChunk = realizedCreature.bodyChunks[UnityEngine.Random.Range(0, realizedCreature.bodyChunks.Length)];
                                Snap(bodyChunk);
                            }
                        }
                        else if (inputWithDiagonals.Value.AnyDirectionalInput)
                        {
                            SnapTowards(mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 200f);
                        }
                        else
                        {
                            SnapTowards(mainBodyChunk.pos + Custom.RNV() * 200f);
                        }
                    }
                    if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
                    {
                        if (grasps[0] != null)
                        {
                            LoseAllGrasps();
                        }
                        else if (IsMiros && !isLaserActive())
                        {
                            FireLaser();
                        }
                    }
                    if (flag)
                    {
                        if (inputWithDiagonals.Value.y < 0)
                        {
                            GoThroughFloors = true;
                        }
                        else
                        {
                            GoThroughFloors = false;
                        }
                    }
                }
            }
            if (movementConnection == default || Custom.ManhattanDistance(room.GetWorldCoordinate(mainBodyChunk.pos), AI.pathFinder.GetDestination) < 2)
            {
                hoverStill = true;
            }
            neck.retractFac = Mathf.Clamp(neck.retractFac + 0.033333335f, 0f, 0.6f);
            bodyChunks[4].vel *= 0.9f;
            for (int num3 = 0; num3 < 4; num3++)
            {
                if (AirBorne)
                {
                    bodyChunks[num3].vel *= 0.98f;
                    upForceByWing = 0f;
                    for (int num4 = 0; num4 < tentacles.Length; num4++)
                    {
                        upForceByWing += 2f * (tentacles[num4].stun >= 5 ? 0f : 1f / tentacles.Length);
                        //upForceByWing += ((this.tentacles[num4].stun >= 5) ? 0f : (1f / (float)this.tentacles.Length));
                    }
                }
                else
                {
                    bodyChunks[num3].vel *= Mathf.Lerp(0.98f, 0.9f, upForceByWing);
                    if (upForceByWing > 0.1f)
                    {
                        BodyChunk bodyChunk2 = bodyChunks[num3];
                        bodyChunk2.vel.y = bodyChunk2.vel.y + Mathf.Lerp(1.2f, 0.5f, upForceByWing);
                    }
                }
            }
            for (int num3 = 5; num3 < 9; num3++)
            {
                if (AirBorne)
                {
                    bodyChunks[num3].vel *= 0.98f;
                    upForceByLeg = 0f;
                    bodyChunks[num3].vel.y -= 0.5f;
                    bodyChunks[num3 - 5].vel.y += 0.5f;
                }
                else
                {
                    bodyChunks[num3].vel *= Mathf.Lerp(0.98f, 0.9f, upForceByLeg);
                    if (upForceByLeg > 0.1f)
                    {
                        BodyChunk bodyChunk2 = bodyChunks[num3];
                        bodyChunk2.vel.y = bodyChunk2.vel.y + Mathf.Lerp(1.2f, 0.5f, upForceByLeg);
                    }
                }
            }
            bodyChunks[1].vel.y = bodyChunks[1].vel.y + 1.9f * upForceByWing * Mathf.InverseLerp(1f, 7f, mainBodyChunk.vel.magnitude);
            bodyChunks[0].vel.y = bodyChunks[0].vel.y - 1.9f * upForceByWing * Mathf.InverseLerp(1f, 7f, mainBodyChunk.vel.magnitude);
            bodyChunks[6].vel.y = bodyChunks[6].vel.y + 1.9f * upForceByLeg * Mathf.InverseLerp(1f, 7f, mainBodyChunk.vel.magnitude);
            bodyChunks[5].vel.y = bodyChunks[5].vel.y - 1.9f * upForceByLeg * Mathf.InverseLerp(1f, 7f, mainBodyChunk.vel.magnitude);
            /*
            BodyChunk bodyChunk3 = base.bodyChunks[1];
            bodyChunk3.vel.y = bodyChunk3.vel.y + 1.9f * upForceByWing * Mathf.InverseLerp(1f, 7f, base.mainBodyChunk.vel.magnitude);
            BodyChunk bodyChunk4 = base.bodyChunks[0];
            bodyChunk4.vel.y = bodyChunk4.vel.y - 1.9f * upForceByWing * Mathf.InverseLerp(1f, 7f, base.mainBodyChunk.vel.magnitude);*/
            if (!hoverStill &&
                (movementConnection == default || movementConnection.DestTile == lastConnection.DestTile && room.IsPositionInsideBoundries(abstractCreature.pos.Tile)))
            {
                stuck++;
                if (stuck > 60)
                {
                    stuckShake = stuckShakeDuration;
                    stuckShakeDuration += 30;
                }
            }
            else
            {
                stuck = 0;
                if (stuckShakeDuration > 30)
                {
                    stuckShakeDuration--;
                }
            }
            if (room == null)
            {
                return;
            }
            for (int num5 = 0; num5 < 5; num5++)
            {
                if (room.GetTile(abstractCreature.pos.Tile + Custom.fourDirectionsAndZero[num5]).wormGrass)
                {
                    stuckShake = Math.Max(stuckShake, 40);
                    mainBodyChunk.vel -= Custom.fourDirectionsAndZero[num5].ToVector2() * 2f + Custom.RNV() * 6f + new Vector2(0f, 6f);
                }
            }
            if (AI.stuckTracker.Utility() > 0.9f)
            {
                stuckShake = Math.Max(stuckShake, 5);
            }
            if (stuckShake > 0)
            {
                stuckShake--;
                StuckBehavior();
                return;
            }
            if (!hoverStill)
            {
                #region 何时动脚
                bool bothTentacleHasGrip = true;
                for (int num6 = 0; num6 < tentacles.Length; num6++)
                {
                    bothTentacleHasGrip = bothTentacleHasGrip && (tentacles[num6].hasAnyGrip || tentacles[num6].mode != AlcedoTentacle.Mode.Climb);
                }/*
                if (this.hangingInTentacle && bothTentacleHasGrip)
                {
                    this.releaseGrippingTentacle++;
                    if (this.releaseGrippingTentacle > 5 && this.CheckTentacleModeAnd(AlcedoTentacle.Mode.Climb))
                    {
                        int i = this.TentacleMaxReleaseInd();
                        this.tentacles[i].ReleaseGrip();
                        //int otherDir = 1 - i % 2 + Mathf.FloorToInt((float)i / 2f);
                        //this.legs[otherDir].ReleaseGrip();
                    }
                    else if (this.releaseGrippingTentacle > 50)
                    {
                        int i = this.TentacleMaxReleaseInd();
                        this.tentacles[i].ReleaseGrip();
                        //int otherDir = 1 - i % 2 + Mathf.FloorToInt((float)i / 2f);
                        //this.legs[otherDir].ReleaseGrip();
                    }
                }
                else
                {
                    this.releaseGrippingTentacle = 0;
                }*/
                bool bothLegHasGrip = true;
                for (int num6 = 0; num6 < legs.Length; num6++)
                {
                    bothLegHasGrip = bothLegHasGrip && legs[num6].hasAnyGrip;
                }/*
                if (this.hangingInLeg && bothLegHasGrip)
                {
                    this.releaseGrippingLeg++;
                    if (this.releaseGrippingLeg > 5)
                    {
                        int i = this.LegMaxReleaseInd();
                        this.legs[i].ReleaseGrip();
                        //int otherDir = 1 - i % 2 + Mathf.FloorToInt((float)i / 2f);
                        //this.legs[otherDir].ReleaseGrip();
                    }
                    else if (this.releaseGrippingLeg > 50)
                    {
                        int i = this.LegMaxReleaseInd();
                        this.legs[i].ReleaseGrip();
                        //int otherDir = 1 - i % 2 + Mathf.FloorToInt((float)i / 2f);
                        //this.legs[otherDir].ReleaseGrip();
                    }
                }
                else
                {
                    this.releaseGrippingLeg = 0;
                }
                */
                if (hangingInTentacle && hangingInLeg && bothTentacleHasGrip && bothLegHasGrip)
                {
                    releaseGrippingTentacle++;
                    if (releaseGrippingTentacle > 5)
                    {
                        var limb = TentacleAndLegMaxRelease();
                        if (limb is AlcedoTentacle)
                        {
                            if (CheckTentacleModeAnd(AlcedoTentacle.Mode.Climb) || releaseGrippingTentacle > 50)
                                (limb as AlcedoTentacle).ReleaseGrip();
                        }
                        else if (limb is AlcedoLeg)
                        {
                            (limb as AlcedoLeg).ReleaseGrip();
                        }
                    }
                }
                else
                {
                    releaseGrippingTentacle = 0;
                }
                #endregion

                bool flag4 = true;
                for (int num7 = 0; num7 < tentacles.Length; num7++)
                {
                    flag4 = flag4 && tentacles[num7].WingSpace();
                }
                if (!safariControlled && IsMiros && isLaserActive() && CheckTentacleModeOr(AlcedoTentacle.Mode.Climb))
                {
                    if (timeSinceLastTakeoff >= 40)
                    {
                        TakeOff();
                    }
                    else
                    {
                        for (int num8 = 0; num8 < tentacles.Length; num8++)
                        {
                            tentacles[num8].SwitchMode(AlcedoTentacle.Mode.Fly);
                        }
                    }
                    dontSwitchModesCounter = 200;
                }
                timeSinceLastTakeoff++;
                if (dontSwitchModesCounter > 0)
                {
                    dontSwitchModesCounter--;
                }/*
                else if (this.IsMiros)
                {
                    if (!this.hoverStill && this.room.aimap.getTerrainProximity(movementConnection.DestTile) > 5 && this.CheckTentacleModeAnd(AlcedoTentacle.Mode.Climb) && base.mainBodyChunk.vel.y > 4f && this.moveDirection.y > 0f && SharedPhysics.RayTraceTilesForTerrain(this.room, this.room.GetTilePosition(base.mainBodyChunk.pos), this.room.GetTilePosition(base.mainBodyChunk.pos + this.moveDirection * 400f)) && flag4)
                    {
                        this.TakeOff();
                        this.dontSwitchModesCounter = 200;
                    }
                    else if (!this.hoverStill && this.room.aimap.getTerrainProximity(movementConnection.DestTile) > 4 && this.CheckTentacleModeOr(AlcedoTentacle.Mode.Climb) && (!base.safariControlled || climb == AlcedoTentacle.Mode.Fly))
                    {
                        for (int num9 = 0; num9 < this.tentacles.Length; num9++)
                        {
                            this.tentacles[num9].SwitchMode(AlcedoTentacle.Mode.Fly);
                        }
                        this.dontSwitchModesCounter = 200;
                    }
                    else if (this.room.aimap.getTerrainProximity(movementConnection.DestTile) <= (this.IsMiros ? 4 : 8) && this.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y != -1 && this.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y > movementConnection.DestTile.y - 10 && this.CheckTentacleModeAnd(AlcedoTentacle.Mode.Fly) && (!base.safariControlled || climb == AlcedoTentacle.Mode.Climb))
                    {
                        for (int num10 = 0; num10 < this.tentacles.Length; num10++)
                        {
                            this.tentacles[num10].SwitchMode(AlcedoTentacle.Mode.Climb);
                        }
                        this.AirBrake(30);
                        this.dontSwitchModesCounter = 200;
                    }
                }
                */
                else if (room.aimap.getTerrainProximity(movementConnection.DestTile) <= (IsMiros ? 4 : 8) &&
                    room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y != -1 &&
                    room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y > movementConnection.DestTile.y - 5 && //this.room.aimap.getAItile(movementConnection.DestTile).fallRiskTile.y > movementConnection.DestTile.y - 10 &&
                    CheckTentacleModeAnd(AlcedoTentacle.Mode.Fly) && (!safariControlled || climb == AlcedoTentacle.Mode.Climb))
                {
                    for (int num11 = 0; num11 < tentacles.Length; num11++)
                    {
                        tentacles[num11].SwitchMode(AlcedoTentacle.Mode.Climb);
                    }
                    AirBrake(30);
                    dontSwitchModesCounter = 100;//200
                }
                else if (!hoverStill && room.aimap.getTerrainProximity(movementConnection.DestTile) > 5 &&
                    CheckTentacleModeAnd(AlcedoTentacle.Mode.Climb) && mainBodyChunk.vel.y > 4f && moveDirection.y > 0f &&
                    SharedPhysics.RayTraceTilesForTerrain(room, room.GetTilePosition(mainBodyChunk.pos), room.GetTilePosition(mainBodyChunk.pos + moveDirection * 400f)) &&
                    flag4 && (!safariControlled || climb == AlcedoTentacle.Mode.Fly))
                {
                    TakeOff();
                    dontSwitchModesCounter = 100;//200;
                }
            }
            bool flag5 = true;
            for (int num12 = 0; num12 < tentacles.Length; num12++)
            {
                flag5 = flag5 && !tentacles[num12].hasAnyGrip;
            }
            if (mainBodyChunk.vel.y < -10f && CheckTentacleModeAnd(AlcedoTentacle.Mode.Climb) && flag5 && landingBrake < 1 && (!safariControlled || climb == AlcedoTentacle.Mode.Fly))
            {
                for (int num13 = 0; num13 < tentacles.Length; num13++)
                {
                    tentacles[num13].SwitchMode(AlcedoTentacle.Mode.Fly);
                }
                if (graphicsModule != null)
                {
                    (graphicsModule as AlcedoGraphics).MakeColorWave(UnityEngine.Random.Range(10, 20));
                }
            }
            bool flag6 = true;
            for (int num14 = 0; num14 < tentacles.Length; num14++)
            {
                flag6 = flag6 && tentacles[num14].mode != AlcedoTentacle.Mode.Fly;
            }
            if (CheckTentacleModeOr(AlcedoTentacle.Mode.Fly))
            {
                wingFlapAmplitude = Mathf.Clamp(wingFlapAmplitude + 0.033333335f, 0f, 1f);
            }
            else if (flag6)
            {
                wingFlapAmplitude = 0f;
            }
            else
            {
                wingFlapAmplitude = Mathf.Clamp(wingFlapAmplitude + 0.0125f, 0f, 0.5f);
            }
            if (hoverStill)
            {
                if (!lastHoverStill || !Custom.DistLess(mainBodyChunk.pos, room.MiddleOfTile(hoverPos), 60f))
                {
                    hoverPos = room.GetTilePosition(mainBodyChunk.pos);
                }
                bodyChunks[1].vel.y = bodyChunks[1].vel.y + 0.1f * upForceByWing;
                for (int num15 = 0; num15 < 4; num15++)
                {
                    bodyChunks[num15].vel *= Mathf.Lerp(1f, 0.9f, upForceByWing);
                    bodyChunks[num15].vel += 0.6f * upForceByWing * Vector2.ClampMagnitude(room.MiddleOfTile(hoverPos) - mainBodyChunk.pos, 10f) / 10f;
                }
                bodyChunks[6].vel.y = bodyChunks[6].vel.y + 0.1f * upForceByLeg;
                for (int num15 = 5; num15 < 9; num15++)
                {
                    bodyChunks[num15].vel *= Mathf.Lerp(1f, 0.9f, upForceByLeg);
                    bodyChunks[num15].vel += 0.6f * upForceByLeg * Vector2.ClampMagnitude(room.MiddleOfTile(hoverPos) - mainBodyChunk.pos, 10f) / 10f;
                }
            }
            else if (movementConnection != default)
            {
                Vector2 destinationPos = Custom.DirVec(mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord));
                destinationPos = Vector2.Lerp(destinationPos, IntVector2.ClampAtOne(movementConnection.DestTile - movementConnection.StartTile).ToVector2(), 0.5f);
                if (AirBorne)
                {
                    if (room.IsPositionInsideBoundries(abstractCreature.pos.Tile) || destinationPos.y < 0f)
                    {
                        destinationPos.y *= 0.5f;
                        if (destinationPos.y < 0f)
                        {
                            destinationPos.y = 0f;
                        }
                    }
                    if (movementConnection.destinationCoord.y > movementConnection.startCoord.y || !movementConnection.destinationCoord.TileDefined)
                    {
                        BodyChunk bodyChunk6 = bodyChunks[1];
                        bodyChunk6.vel.y = bodyChunk6.vel.y + 3.5f;
                        //bodyChunk6.vel.y = bodyChunk6.vel.y + 3.5f;
                    }
                    else if (movementConnection.destinationCoord.y < movementConnection.startCoord.y)
                    {
                        wingFlap -= 1f / 230f;//1f / 70f;
                        //this.wingFlap -= 0.014285714f;
                    }
                }
                else if (!room.IsPositionInsideBoundries(movementConnection.DestTile))
                {
                    if (movementConnection.destinationCoord.y > room.TileHeight)
                    {
                        TakeOff();
                    }
                    else if (!safariControlled || climb == AlcedoTentacle.Mode.Fly)
                    {
                        for (int num16 = 0; num16 < tentacles.Length; num16++)
                        {
                            tentacles[num16].SwitchMode(AlcedoTentacle.Mode.Fly);
                        }
                    }
                }
                for (int num17 = 0; num17 < 4; num17++)
                {
                    bodyChunks[num17].vel += destinationPos * (AirBorne ? 0.6f : IsKing ? 1.9f : 1.2f) * upForceByWing;
                    //base.bodyChunks[num17].vel += destinationPos * (this.AirBorne ? 0.6f : (this.IsKing ? 1.9f : 1.2f)) * upForceByWing;
                }
                for (int num17 = 5; num17 < 9; num17++)
                {
                    bodyChunks[num17].vel += destinationPos * (AirBorne ? 0.6f : IsKing ? 1.9f : 1.2f) * upForceByLeg;
                }
                MovementConnection movementConnection2 = movementConnection;
                for (int num18 = 0; num18 < 3; num18++)
                {
                    MovementConnection movementConnection3 = (AI.pathFinder as AlcedoPather).FollowPath(movementConnection2.destinationCoord, false);
                    movementConnection2 = movementConnection3;
                }
                if (movementConnection2 == movementConnection)
                {
                    moveDirection = (moveDirection + destinationPos * 0.1f).normalized;
                }
                else
                {
                    moveDirection = (moveDirection + Custom.DirVec(mainBodyChunk.pos, room.MiddleOfTile(movementConnection2.destinationCoord)) * 0.5f).normalized;
                }
            }
            bool flag7 = false;
            for (int num19 = 0; num19 < tentacles.Length; num19++)
            {
                flag7 = flag7 || !tentacles[num19].hasAnyGrip;
            }
            if (!hoverStill && !hangingInTentacle && flag7)
            {
                float num20 = 0f;
                int num21 = -1;
                for (int num22 = 0; num22 < thrusters.Length; num22++)
                {
                    float num23 = thrusters[num22].Utility();
                    if (num23 > num20)
                    {
                        num20 = num23;
                        num21 = num22;
                    }
                }
                num20 *= jetFuel;
                if (num20 > 0.05f)
                {
                    thrusters[num21].Activate(10 + (int)(Mathf.InverseLerp(0.05f, 0.4f, num20) * 20f));
                }
            }
            if (snapFrames == 0)
            {
                if (AI.preyInTuskChargeRange || AirBorne)
                {
                    tuskCharge = Mathf.Clamp(tuskCharge + 0.025f, 0f, 1f);
                }
                else
                {
                    tuskCharge = Mathf.Clamp(tuskCharge - 0.011111111f, 0f, 1f);
                }
            }
            else
            {
                Vector2 pos = snapAtPos;
                if (snapAt != null)
                {
                    pos = snapAt.pos;
                }
                if (Snapping)
                {
                    mainBodyChunk.vel += Custom.DirVec(mainBodyChunk.pos, pos) * 1f;
                }
                else if (ChargingSnap)
                {
                    bodyChunks[1].vel -= Custom.DirVec(bodyChunks[1].pos, pos) * 0.5f;
                    if (!AirBorne)
                    {
                        for (int num24 = 0; num24 < 4; num24++)
                        {
                            bodyChunks[num24].vel *= Mathf.Lerp(1f, 0.2f, upForceByWing);
                        }
                        for (int num24 = 5; num24 < 9; num24++)
                        {
                            bodyChunks[num24].vel *= 0.9f * Mathf.Lerp(1f, 0.2f, upForceByLeg);
                        }
                    }
                }
                snapFrames--;
            }
            lastHoverStill = hoverStill;
            if (movementConnection != default)
            {
                lastConnection = movementConnection;
            }
        }

        private void StuckBehavior()
        {
            bool flag = false;
            for (int i = 0; i < 4; i++)
            {
                if (bodyChunks[i].ContactPoint.x != 0 || bodyChunks[i].ContactPoint.y != 0 || room.GetTile(bodyChunks[i].pos).wormGrass)
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
                        if (SharedPhysics.RayTraceTilesForTerrain(room, room.GetTilePosition(mainBodyChunk.pos), room.GetTilePosition(mainBodyChunk.pos + Custom.DegToVec(num) * 20f * j)))
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                bodyChunks[k].vel += Custom.DegToVec(num);
                            }
                            break;
                        }
                    }
                }
            }
            Vector2 b = Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f;
            for (int l = 0; l < 4; l++)
            {
                bodyChunks[l].vel += b;
            }
            bool flag2 = true;
            for (int m = 0; m < thrusters.Length; m++)
            {
                if (thrusters[m].Active)
                {
                    flag2 = false;
                    break;
                }
            }
            if (jetFuel > 0.2f && flag2 && thrusters.Length != 0)
            {
                thrusters[UnityEngine.Random.Range(0, thrusters.Length)].Activate(5 + UnityEngine.Random.Range(0, 10));
            }
        }

        public void AccessSkyGate(WorldCoordinate start, WorldCoordinate dest)
        {
            room.game.shortcuts.CreatureTakeFlight(this, AbstractRoomNode.Type.SkyExit, start, dest);
            if (ModManager.CoopAvailable)
            {
                foreach (Grasp grasp in grasps)
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
            landingBrake = frames;
            landingBrakePos = bodyChunks[1].pos;
            if (frames > 5)
            {
                room.PlaySound(SoundID.Vulture_Jets_Air_Brake, mainBodyChunk);
            }
        }

        private void TakeOff()
        {
            timeSinceLastTakeoff = 0;
            for (int i = 0; i < tentacles.Length; i++)
            {
                tentacles[i].SwitchMode(AlcedoTentacle.Mode.Fly);
            }
            for (int i = 0; i < legs.Length; i++)
            {
                legs[i].ReleaseGrip();
            }
            if (graphicsModule != null)
            {
                (graphicsModule as AlcedoGraphics).MakeColorWave(UnityEngine.Random.Range(30, 60));
            }
            AirBrake(5);
            thrusters[0].Activate(30);
            thrusters[1].Activate(30);
        }

        private void WaterBehavior()
        {
        }

        private void CheckFlip()
        {
            if (Custom.DistanceToLine(bodyChunks[2].pos, bodyChunks[0].pos, bodyChunks[1].pos) < 0f)
            {
                Vector2 pos = bodyChunks[2].pos;
                Vector2 vel = bodyChunks[2].vel;
                Vector2 lastPos = bodyChunks[2].lastPos;
                bodyChunks[2].pos = bodyChunks[3].pos;
                bodyChunks[2].vel = bodyChunks[3].vel;
                bodyChunks[2].lastPos = bodyChunks[3].lastPos;
                bodyChunks[3].pos = pos;
                bodyChunks[3].vel = vel;
                bodyChunks[3].lastPos = lastPos;
            }
            if (Custom.DistanceToLine(bodyChunks[7].pos, bodyChunks[5].pos, bodyChunks[6].pos) < 0f)
            {
                Vector2 pos = bodyChunks[7].pos;
                Vector2 vel = bodyChunks[7].vel;
                Vector2 lastPos = bodyChunks[7].lastPos;
                bodyChunks[7].pos = bodyChunks[8].pos;
                bodyChunks[7].vel = bodyChunks[8].vel;
                bodyChunks[7].lastPos = bodyChunks[8].lastPos;
                bodyChunks[8].pos = pos;
                bodyChunks[8].vel = vel;
                bodyChunks[8].lastPos = lastPos;
            }
        }

        public void Snap(BodyChunk snapAt)
        {
            tuskCharge = 0f;
            this.snapAt = snapAt;
            snapFrames = 49;
            room.PlaySound(SoundID.Vulture_Peck, bodyChunks[4]);
        }

        private void UpdateWaist()
        {
            waist.tChunks[waist.tChunks.Length - 1].pos = bodyChunks[5].pos;
            waist.tChunks[waist.tChunks.Length - 1].vel = bodyChunks[5].vel;
            waist.Update();
            //base.bodyChunks[5].collideWithTerrain = true;
            return;
            for (int j = 0; j < waist.tChunks.Length; j++)
            {
                waist.tChunks[j].vel *= 0.95f;/*
                Tentacle.TentacleChunk tentacleChunk = this.waist.tChunks[j];
                tentacleChunk.vel.y = tentacleChunk.vel.y - (this.waist.limp ? 0.7f : 0.1f);
                this.waist.tChunks[j].vel += Custom.DirVec(base.bodyChunkConnections[8].chunk1.pos, base.bodyChunks[5].pos) * ((j == 0) ? 1.2f : 0.8f);
                this.waist.tChunks[j].vel -= this.waist.connectedChunk.vel;
                this.waist.tChunks[j].vel *= (this.AirBorne ? 0.2f : 0.75f);
                this.waist.tChunks[j].vel += this.waist.connectedChunk.vel;*/
            }
            waist.limp = !Consious;
            float num2 = 0.5f;
            bodyChunks[5].collideWithObjects = true;
            //float num2 = (this.waist.backtrackFrom == -1) ? 0.5f : 0f;
            if (true)//base.grasps[0] == null
            {
                waist.tChunks[waist.tChunks.Length - 1].pos = bodyChunks[5].pos;
                waist.tChunks[waist.tChunks.Length - 1].vel = bodyChunks[5].vel;
                Vector2 dir = Custom.DirVec(bodyChunkConnections[8].chunk1.pos, bodyChunks[5].pos);
                Vector2 dir2 = Custom.DirVec(bodyChunks[4].pos, bodyChunks[0].pos);
                float num32 = Vector2.Distance(bodyChunks[5].pos, bodyChunkConnections[8].chunk1.pos);
                if (!AirBorne && (hangingInTentacle || hoverStill))
                {
                    num32 = Mathf.Abs(bodyChunks[5].pos.x - bodyChunkConnections[8].chunk1.pos.x);
                    dir = new Vector2(dir.x, 0).normalized;
                    dir2 = new Vector2(dir2.x, 0).normalized;
                    bodyChunks[5].collideWithObjects = false;
                }
                bodyChunks[5].pos -= (bodyChunkConnections[8].distance - num32) * dir * (1f - num2);
                bodyChunks[5].vel -= (bodyChunkConnections[8].distance - num32) * dir * (1f - num2);
                bodyChunkConnections[8].chunk1.pos += (bodyChunkConnections[8].distance - num32) * dir * num2;
                bodyChunkConnections[8].chunk1.vel += (bodyChunkConnections[8].distance - num32) * dir * num2;
                bodyChunks[5].vel += dir2 * (AirBorne ? 2f : 6f) * (1f - num2);
                bodyChunkConnections[8].chunk1.vel -= dir2 * (AirBorne ? 2f : 6f) * num2;
            }
            if (!Consious)
            {
                return;
            }/*
            Vector2 a2 = Custom.DirVec(base.bodyChunks[5].pos, this.waist.connectedChunk.pos);
            this.waist.tChunks[this.waist.tChunks.Length - 1].vel += a2 * num2;
            this.waist.tChunks[this.waist.tChunks.Length - 2].vel -= a2 * 0.5f * num2;
            //base.bodyChunks[5].vel += a2 * 4f * (1f - num2);
            
            if (Custom.DistLess(base.bodyChunks[5].pos, this.waist.connectedChunk.pos, 80f))
            {
                for (int k = 0; k < this.waist.tChunks.Length; k++)
                {
                    this.waist.tChunks[k].vel -= a2 * Mathf.InverseLerp(80f, 20f, Vector2.Distance(base.bodyChunks[5].pos, this.waist.connectedChunk.pos)) * 8f * num2;
                }
            }*/
        }

        private void UpdateNeck()
        {
            neck.Update();
            if (AI.stuckTracker.closeToGoalButNotSeeingItTracker.counter > AI.stuckTracker.closeToGoalButNotSeeingItTracker.counterMin)
            {
                List<IntVector2> list = null;
                float num = AI.stuckTracker.closeToGoalButNotSeeingItTracker.Stuck;
                neck.MoveGrabDest(room.MiddleOfTile(AI.pathFinder.GetDestination), ref list);
                bodyChunks[4].vel += Custom.DirVec(bodyChunks[4].pos, room.MiddleOfTile(AI.pathFinder.GetDestination)) * 10f * num;
                bodyChunks[4].pos += Custom.DirVec(bodyChunks[4].pos, room.MiddleOfTile(AI.pathFinder.GetDestination)) * 10f * num;
                for (int i = 0; i < neck.tChunks.Length; i++)
                {
                    neck.tChunks[i].vel += Custom.DirVec(neck.tChunks[i].pos, room.MiddleOfTile(AI.pathFinder.GetDestination)) * 5f * num;
                }
                if (num > 0.95f)
                {
                    bodyChunks[4].collideWithTerrain = false;
                    bodyChunks[5].collideWithObjects = false;
                    bodyChunks[6].collideWithObjects = false;
                    bodyChunks[7].collideWithObjects = false;
                    bodyChunks[8].collideWithObjects = false;
                    return;
                }
            }
            bodyChunks[4].collideWithTerrain = true;
            bodyChunks[5].collideWithObjects = true;
            bodyChunks[6].collideWithObjects = true;
            bodyChunks[7].collideWithObjects = true;
            bodyChunks[8].collideWithObjects = true;
            for (int j = 0; j < neck.tChunks.Length; j++)
            {
                neck.tChunks[j].vel *= 0.95f;
                Tentacle.TentacleChunk tentacleChunk = neck.tChunks[j];
                tentacleChunk.vel.y = tentacleChunk.vel.y - (neck.limp ? 0.7f : 0.1f);
                neck.tChunks[j].vel += Custom.DirVec(bodyChunks[0].pos, bodyChunks[1].pos) * (j == 0 ? 1.2f : 0.8f);
                neck.tChunks[j].vel -= neck.connectedChunk.vel;
                neck.tChunks[j].vel *= 0.75f;//(this.AirBorne ? 0.2f : 0.75f);
                neck.tChunks[j].vel += neck.connectedChunk.vel;
            }
            neck.limp = !Consious;
            float num2 = neck.backtrackFrom == -1 ? 0.5f : 0f;
            if (grasps[0] == null)
            {
                Vector2 a = Custom.DirVec(bodyChunks[4].pos, neck.tChunks[neck.tChunks.Length - 1].pos);
                float num3 = Vector2.Distance(bodyChunks[4].pos, neck.tChunks[neck.tChunks.Length - 1].pos);
                bodyChunks[4].pos -= (6f - num3) * a * (1f - num2);
                bodyChunks[4].vel -= (6f - num3) * a * (1f - num2);
                neck.tChunks[neck.tChunks.Length - 1].pos += (6f - num3) * a * num2;
                neck.tChunks[neck.tChunks.Length - 1].vel += (6f - num3) * a * num2;
                bodyChunks[4].vel += Custom.DirVec(neck.tChunks[neck.tChunks.Length - 2].pos, bodyChunks[4].pos) * (AirBorne ? 5f : 6f) * (1f - num2);
                bodyChunks[4].vel += Custom.DirVec(neck.tChunks[neck.tChunks.Length - 1].pos, bodyChunks[4].pos) * (AirBorne ? 5f : 6f) * (1f - num2);
                neck.tChunks[neck.tChunks.Length - 1].vel -= Custom.DirVec(neck.tChunks[neck.tChunks.Length - 2].pos, bodyChunks[4].pos) * (AirBorne ? 2.5f : 3f) * num2;
                neck.tChunks[neck.tChunks.Length - 2].vel -= Custom.DirVec(neck.tChunks[neck.tChunks.Length - 2].pos, bodyChunks[4].pos) * (AirBorne ? 2.5f : 3f) * num2;/*
                base.bodyChunks[4].vel += Custom.DirVec(this.neck.tChunks[this.neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * (this.AirBorne ? 2f : 6f) * (1f - num2);
                base.bodyChunks[4].vel += Custom.DirVec(this.neck.tChunks[this.neck.tChunks.Length - 1].pos, base.bodyChunks[4].pos) * (this.AirBorne ? 2f : 6f) * (1f - num2);
                this.neck.tChunks[this.neck.tChunks.Length - 1].vel -= Custom.DirVec(this.neck.tChunks[this.neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * (this.AirBorne ? 1f : 3f) * num2;
                this.neck.tChunks[this.neck.tChunks.Length - 2].vel -= Custom.DirVec(this.neck.tChunks[this.neck.tChunks.Length - 2].pos, base.bodyChunks[4].pos) * (this.AirBorne ? 1f : 3f) * num2;*/
            }
            if (!Consious)
            {
                return;
            }
            Vector2 pos = snapAtPos;
            if (snapAt != null)
            {
                pos = snapAt.pos;
            }
            if (ChargingSnap)
            {
                bodyChunks[4].vel += (mainBodyChunk.pos + Custom.DirVec(mainBodyChunk.pos, pos) * 50f - bodyChunks[4].pos) / 6f;
                neck.tChunks[neck.tChunks.Length - 1].vel -= Custom.DirVec(bodyChunks[4].pos, pos) * 10f * num2;
                return;
            }
            if (Snapping)
            {
                bodyChunks[4].vel += Custom.DirVec(bodyChunks[4].pos, pos) * 15f;
                neck.tChunks[neck.tChunks.Length - 1].vel -= Custom.DirVec(bodyChunks[4].pos, pos) * num2;
                return;
            }
            Vector2 vector;
            if (AI.creatureLooker.lookCreature == null)
            {
                vector = room.MiddleOfTile(AI.pathFinder.GetDestination);
            }
            else if (AI.creatureLooker.lookCreature.VisualContact)
            {
                vector = AI.creatureLooker.lookCreature.representedCreature.realizedCreature.DangerPos;
            }
            else
            {
                vector = room.MiddleOfTile(AI.creatureLooker.lookCreature.BestGuessForPosition());
            }
            if (Custom.DistLess(vector, mainBodyChunk.pos, 220f) && !room.VisualContact(vector, bodyChunks[4].pos))
            {
                List<IntVector2> list2 = null;
                neck.MoveGrabDest(vector, ref list2);
            }
            else if (neck.backtrackFrom == -1)
            {
                neck.floatGrabDest = null;
            }
            Vector2 a2 = Custom.DirVec(bodyChunks[4].pos, vector);
            if (grasps[0] == null)
            {
                neck.tChunks[neck.tChunks.Length - 1].vel += a2 * num2;
                neck.tChunks[neck.tChunks.Length - 2].vel -= a2 * 0.5f * num2;
                bodyChunks[4].vel += a2 * 4f * (1f - num2);
            }
            else
            {
                neck.tChunks[neck.tChunks.Length - 1].vel += a2 * 2f * num2;
                neck.tChunks[neck.tChunks.Length - 2].vel -= a2 * 2f * num2;
                grasps[0].grabbedChunk.vel += a2 / grasps[0].grabbedChunk.mass;
            }
            if (Custom.DistLess(bodyChunks[4].pos, vector, 80f))
            {
                for (int k = 0; k < neck.tChunks.Length; k++)
                {
                    neck.tChunks[k].vel -= a2 * Mathf.InverseLerp(80f, 20f, Vector2.Distance(bodyChunks[4].pos, vector)) * 8f * num2;
                }
            }
        }

        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);
            if (!IsMiros && Snapping && myChunk == 4 && grasps[0] == null)
            {
                if (AI.OnlyHurtDontGrab(otherObject))
                {
                    if (otherObject is Creature)
                    {
                        (otherObject as Creature).Violence(bodyChunks[myChunk], new Vector2?(bodyChunks[myChunk].vel * 2f), otherObject.bodyChunks[otherChunk], null, DamageType.Bite, 1.1f, 30f);
                    }
                }
                else
                {
                    Grab(otherObject, 0, otherChunk, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, true, true);
                    AI.creatureLooker.LookAtNothing();
                    if (otherObject is Creature)
                    {
                        (otherObject as Creature).Violence(bodyChunks[myChunk], new Vector2?(bodyChunks[myChunk].vel * 2f), otherObject.bodyChunks[otherChunk], null, DamageType.Bite, 0.4f, 20f);
                    }
                }
                room.PlaySound(otherObject is Player ? SoundID.Vulture_Grab_Player : SoundID.Vulture_Grab_NPC, bodyChunks[4]);
                snapFrames = 0;
            }
        }

        public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
        {
            if (room == null)
            {
                return;
            }
            if (!IsMiros)
            {
                AI.disencouraged += (damage / 2f + stunBonus / 460f) * (IsKing ? 0.3f : 1f) * (room.game.IsStorySession && room.game.StoryCharacter == SlugcatStats.Name.Yellow ? 1.5f : 1f);
            }
            if (hitChunk != null && hitChunk.index == 4)
            {
                stunBonus += 20f + 20f * damage;
                if (damage > 0.1f || stun > 10)
                {
                    snapFrames = 0;
                }
                bool flag = directionAndMomentum != null && source != null && !SpearStick(source.owner as Spear, damage, hitChunk, onAppendagePos, directionAndMomentum.Value.normalized);
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
                    if (!IsMiros && (State as AlcedoState).mask && (damage > 0.9f || ModManager.MSC && source != null && source.owner is LillyPuck) && (source == null || !(source.owner is Weapon) || (source.owner as Weapon).meleeHitChunk == null))
                    {
                        DropMask((directionAndMomentum != null ? directionAndMomentum.Value / 5f : new Vector2(0f, 0f)) + Custom.RNV() * 7f * UnityEngine.Random.value);
                    }
                    damage *= 1.5f;
                }
                else
                {
                    Vector2 pos = source != null ? Vector2.Lerp(hitChunk.pos, source.pos, 0.5f) : hitChunk.pos;
                    if (damage > 0.1f || stunBonus > 30f)
                    {
                        room.AddObject(new StationaryEffect(pos, new Color(1f, 1f, 1f), null, StationaryEffect.EffectType.FlashingOrb));
                        for (int i = 0; i < 3 + (int)Mathf.Min(damage * 3f, 9f); i++)
                        {
                            room.AddObject(new Spark(pos, Custom.RNV() * UnityEngine.Random.value * 12f, new Color(1f, 1f, 1f), null, 6, 16));
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
                if (onAppendagePos.appendage.appIndex == 0 && type != DamageType.Blunt)
                {
                    damage *= 2f;
                }
                else if (onAppendagePos.appendage.appIndex > 0 && (!IsMiros || type != DamageType.Explosion))
                {
                    tentacles[onAppendagePos.appendage.appIndex - 1].Damage(type, damage, stunBonus);
                    damage /= 10f;
                    stunBonus /= 20f;
                }
            }
            if (IsMiros && !dead && grasps[0] == null && type != DamageType.Explosion)
            {
                FireLaser();
            }
            base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
        }

        public override bool SpearStick(Weapon source, float dmg, BodyChunk chunk, Appendage.Pos appPos, Vector2 direction)
        {
            return IsMiros ||
                (chunk == null || chunk.index != 4 || dead || !(State as AlcedoState).mask ||
                  Vector2.Dot(direction.normalized, Custom.DirVec(neck.tChunks[neck.tChunks.Length - 1].pos, chunk.pos)) >= -0.88f) &&
                 base.SpearStick(source, dmg, chunk, appPos, direction);
        }

        public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
        {
            base.TerrainImpact(chunk, direction, speed, firstContact);
            if (speed > 1.5f && firstContact)
            {
                float num = Mathf.InverseLerp(6f, 14f, speed);
                if (IsMiros)
                {
                    if (num < 1f)
                    {
                        room.PlaySound(SoundID.Miros_Light_Terrain_Impact, mainBodyChunk, false, 1f - num, 1f);
                    }
                    if (num > 0f)
                    {
                        room.PlaySound(SoundID.Miros_Heavy_Terrain_Impact, mainBodyChunk, false, num, 1f);
                        return;
                    }
                }
                else
                {
                    if (num < 1f)
                    {
                        room.PlaySound(SoundID.Vulture_Light_Terrain_Impact, mainBodyChunk, false, 1f - num, 1f);
                    }
                    if (num > 0f)
                    {
                        room.PlaySound(SoundID.Vulture_Heavy_Terrain_Impact, mainBodyChunk, false, num, 1f);
                    }
                }
            }
        }

        public void DropMask(Vector2 violenceDir)
        {
            if (!(State as AlcedoState).mask)
            {
                return;
            }
            (State as AlcedoState).mask = false;
            AbstractPhysicalObject abstractPhysicalObject = new AbstractAlcedoMask(room.world, null,
                this.abstractPhysicalObject.pos, room.game.GetNewID(),
                abstractCreature.ID.RandomSeed, IsKing);
            room.abstractRoom.AddEntity(abstractPhysicalObject);
            abstractPhysicalObject.pos = abstractCreature.pos;
            abstractPhysicalObject.RealizeInRoom();
            abstractPhysicalObject.realizedObject.firstChunk.HardSetPosition(bodyChunks[4].pos);
            abstractPhysicalObject.realizedObject.firstChunk.vel = bodyChunks[4].vel + violenceDir;
            (abstractPhysicalObject.realizedObject as AlcedoMask).fallOffAlcedoMode = 1f;
            if (killTag != null)
            {
                SocialMemory.Relationship orInitiateRelationship = State.socialMemory.GetOrInitiateRelationship(killTag.ID);
                orInitiateRelationship.like = -1f;
                orInitiateRelationship.tempLike = -1f;
                orInitiateRelationship.know = 1f;
            }
        }

        public override void Stun(int st)
        {
            snapFrames = 0;
            if (IsMiros)
            {
                LoseAllGrasps();
            }
            base.Stun(st);
        }

        public void Carry()
        {
            if (!Consious)
            {
                LoseAllGrasps();
                return;
            }
            BodyChunk grabbedChunk = grasps[0].grabbedChunk;
            float num = 1f;
            if (ModManager.MSC && grabbedChunk.owner is Player && (grabbedChunk.owner as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                num = 10f;
            }
            if (UnityEngine.Random.value < 0.008333334f * num && (!(grabbedChunk.owner is Creature) || Template.CreatureRelationship((grabbedChunk.owner as Creature).Template).type != CreatureTemplate.Relationship.Type.Eats))
            {
                LoseAllGrasps();
                return;
            }
            float num2 = grabbedChunk.mass / (grabbedChunk.mass + bodyChunks[4].mass);
            float num3 = grabbedChunk.mass / (grabbedChunk.mass + bodyChunks[0].mass);
            if (neck.backtrackFrom != -1 || enteringShortCut != null)
            {
                num2 = 0f;
                num3 = 0f;
            }
            if (!Custom.DistLess(grabbedChunk.pos, neck.tChunks[neck.tChunks.Length - 1].pos, 20f))
            {
                Vector2 a = Custom.DirVec(grabbedChunk.pos, neck.tChunks[neck.tChunks.Length - 1].pos);
                float num4 = Vector2.Distance(grabbedChunk.pos, neck.tChunks[neck.tChunks.Length - 1].pos);
                grabbedChunk.pos -= (20f - num4) * a * (1f - num2);
                grabbedChunk.vel -= (20f - num4) * a * (1f - num2);
                neck.tChunks[neck.tChunks.Length - 1].pos += (20f - num4) * a * num2;
                neck.tChunks[neck.tChunks.Length - 1].vel += (20f - num4) * a * num2;
            }
            if (enteringShortCut == null)
            {
                bodyChunks[4].pos = Vector2.Lerp(neck.tChunks[neck.tChunks.Length - 1].pos, grabbedChunk.pos, 0.1f);
                bodyChunks[4].vel = neck.tChunks[neck.tChunks.Length - 1].vel;
            }
            float num5 = 70f;
            if (!Custom.DistLess(mainBodyChunk.pos, grabbedChunk.pos, num5))
            {
                Vector2 a2 = Custom.DirVec(grabbedChunk.pos, bodyChunks[0].pos);
                float num6 = Vector2.Distance(grabbedChunk.pos, bodyChunks[0].pos);
                grabbedChunk.pos -= (num5 - num6) * a2 * (1f - num3);
                grabbedChunk.vel -= (num5 - num6) * a2 * (1f - num3);
                bodyChunks[0].pos += (num5 - num6) * a2 * num3;
                bodyChunks[0].vel += (num5 - num6) * a2 * num3;
            }
        }

        public override void Die()
        {
            surfaceFriction = 0.3f;
            base.Die();
        }

        public override Color ShortCutColor()
        {
            if (graphicsModule != null)
            {
                return HSLColor.Lerp((graphicsModule as AlcedoGraphics).ColorA, (graphicsModule as AlcedoGraphics).ColorB, 0.5f).rgb;
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
                    return mainBodyChunk.pos;
                }
                if (segment >= neck.tChunks.Length)
                {
                    return bodyChunks[4].pos;
                }
                return neck.tChunks[segment].pos;
            }
            else
            {
                if (segment < 0)
                {
                    return tentacles[appendage - 1].connectedChunk.pos;
                }
                return tentacles[appendage - 1].tChunks[segment].pos;
            }
        }

        public void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum)
        {
            if (pos.appendage.appIndex != 0)
            {
                if (pos.prevSegment > 0)
                {
                    tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment - 1].pos += momentum / 0.04f * (1f - pos.distanceToNext);
                    tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment - 1].vel += momentum / 0.04f * (1f - pos.distanceToNext);
                }
                else
                {
                    tentacles[pos.appendage.appIndex - 1].connectedChunk.pos += momentum / tentacles[pos.appendage.appIndex - 1].connectedChunk.mass * (1f - pos.distanceToNext);
                    tentacles[pos.appendage.appIndex - 1].connectedChunk.vel += momentum / tentacles[pos.appendage.appIndex - 1].connectedChunk.mass * (1f - pos.distanceToNext);
                }
                tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment].pos += momentum / 0.04f * pos.distanceToNext;
                tentacles[pos.appendage.appIndex - 1].tChunks[pos.prevSegment].vel += momentum / 0.04f * pos.distanceToNext;
                return;
            }
            if (pos.prevSegment > 0)
            {
                neck.tChunks[pos.prevSegment - 1].pos += momentum / 0.1f * (1f - pos.distanceToNext);
                neck.tChunks[pos.prevSegment - 1].vel += momentum / 0.05f * (1f - pos.distanceToNext);
            }
            else
            {
                mainBodyChunk.pos += momentum / mainBodyChunk.mass * (1f - pos.distanceToNext);
                mainBodyChunk.vel += momentum / mainBodyChunk.mass * (1f - pos.distanceToNext);
            }
            if (pos.prevSegment < neck.tChunks.Length - 1)
            {
                neck.tChunks[pos.prevSegment].pos += momentum / 0.1f * pos.distanceToNext;
                neck.tChunks[pos.prevSegment].vel += momentum / 0.05f * pos.distanceToNext;
                return;
            }
            bodyChunks[4].pos += momentum / bodyChunks[4].mass * pos.distanceToNext;
            bodyChunks[4].vel += momentum / bodyChunks[4].mass * pos.distanceToNext;
        }

        public void SnapTowards(Vector2 pos)
        {
            snapAtPos = pos;
            Snap(null);
        }

        public bool MostlyConsious
        {
            get
            {
                return stun < 40 && !dead;
            }
        }

        public bool CheckTentacleModeOr(AlcedoTentacle.Mode mode)
        {
            bool flag = false;
            for (int i = 0; i < tentacles.Length; i++)
            {
                flag = flag || tentacles[i].mode == mode;
            }
            return flag;
        }

        public bool CheckTentacleModeAnd(AlcedoTentacle.Mode mode)
        {
            bool flag = true;
            for (int i = 0; i < tentacles.Length; i++)
            {
                flag = flag && tentacles[i].mode == mode;
            }
            return flag;
        }

        public int TentacleMaxReleaseInd()
        {
            float num = -1f;
            int result = -1;
            for (int i = 0; i < tentacles.Length; i++)
            {
                if (tentacles[i].ReleaseScore() > num || num == -1f)
                {
                    num = tentacles[i].ReleaseScore();
                    result = i;
                }
            }
            return result;
        }

        public int LegMaxReleaseInd()
        {
            float num = -1f;
            int result = -1;
            for (int i = 0; i < legs.Length; i++)
            {
                if (legs[i].ReleaseScore() > num || num == -1f)
                {
                    num = legs[i].ReleaseScore();
                    result = i;
                }
            }
            return result;
        }

        public Tentacle TentacleAndLegMaxRelease()
        {
            Tentacle result = null;
            float num = -1f;
            int index = -1;
            bool isWing = false;
            for (int i = 0; i < tentacles.Length; i++)
            {
                if (tentacles[i].ReleaseScore() > num || num == -1f)
                {
                    num = tentacles[i].ReleaseScore();
                    index = i;
                    isWing = true;
                }
            }
            for (int i = 0; i < legs.Length; i++)
            {
                if (legs[i].ReleaseScore() > num || num == -1f)
                {
                    num = legs[i].ReleaseScore();
                    index = i;
                    isWing = false;
                }
            }
            if (isWing)
                result = tentacles[index];
            else
                result = legs[index];
            return result;
        }

        public BodyChunk Head()
        {
            return bodyChunks[4];
        }

        public void JawSlamShut()
        {
            Vector2 a = Custom.DirVec(neck.Tip.pos, Head().pos);
            neck.Tip.vel -= a * 10f;
            neck.Tip.pos += a * 20f;
            Head().pos += a * 20f;
            int num = 0;
            int num2 = 0;
            while (num2 < room.abstractRoom.creatures.Count && grasps[0] == null)
            {
                Creature realizedCreature = room.abstractRoom.creatures[num2].realizedCreature;
                if (room.abstractRoom.creatures[num2] != abstractCreature && AI.DoIWantToBiteCreature(room.abstractRoom.creatures[num2]) && realizedCreature != null && realizedCreature.enteringShortCut == null && !realizedCreature.inShortcut)
                {
                    int num3 = 0;
                    while (num3 < realizedCreature.bodyChunks.Length && grasps[0] == null)
                    {
                        if (Custom.DistLess(Head().pos + a * 20f, realizedCreature.bodyChunks[num3].pos, 20f + realizedCreature.bodyChunks[num3].rad) && room.VisualContact(Head().pos, realizedCreature.bodyChunks[num3].pos))
                        {
                            if (realizedCreature == null)
                            {
                                break;
                            }
                            num = !(realizedCreature is Player) ? 1 : 2;
                            if (!AI.OnlyHurtDontGrab(realizedCreature))
                            {
                                Grab(realizedCreature, 0, num3, Grasp.Shareability.CanOnlyShareWithNonExclusive, 1f, true, true);
                                AI.creatureLooker.LookAtNothing();
                                jawOpen = 0.15f;
                                jawVel = 0f;
                                realizedCreature.Violence(Head(), new Vector2?(Custom.DirVec(Head().pos, realizedCreature.bodyChunks[num3].pos) * 4f), realizedCreature.bodyChunks[num3], null, DamageType.Bite, 1.2f, 30f);
                                break;
                            }
                            realizedCreature.Violence(Head(), new Vector2?(Custom.DirVec(Head().pos, realizedCreature.bodyChunks[num3].pos) * 4f), realizedCreature.bodyChunks[num3], null, DamageType.Bite, 1.2f, 20f);
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
                                if (Custom.DistLess(Head().pos + a * 20f, (realizedCreature as DaddyLongLegs).tentacles[i].tChunks[j].pos, 20f))
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
                room.PlaySound(SoundID.Miros_Beak_Snap_Miss, Head());
                return;
            }
            if (num == 1)
            {
                room.PlaySound(SoundID.Miros_Beak_Snap_Hit_Slugcat, Head());
                return;
            }
            room.PlaySound(SoundID.Miros_Beak_Snap_Hit_Other, Head());
        }

        public bool isLaserActive()
        {
            return !dead && laserCounter > 0 && grasps[0] == null;
        }

        private void FireLaser()
        {
            if (laserCounter <= 0 && !dead && MostlyConsious && graphicsModule != null && !(graphicsModule as AlcedoGraphics).shadowMode)
            {
                laserCounter = 200;
                if (room != null)
                {
                    LaserLight = new LightSource(new Vector2(-1000f, -1000f), false, new Color(0.1f, 1f, 0.1f), this);
                    room.AddObject(LaserLight);
                    LaserLight.HardSetAlpha(1f);
                    LaserLight.HardSetRad(200f);
                }
            }
        }

        public void LaserExplosion()
        {
            if (room == null)
            {
                return;
            }
            Vector2 pos = Head().pos;
            Vector2 a = Custom.DirVec(neck.Tip.pos, pos);
            a *= -1f;
            Vector2 corner = Custom.RectCollision(pos, pos - a * 100000f, room.RoomRect.Grow(200f)).GetCorner(FloatRect.CornerLabel.D);
            IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, pos, corner);
            if (intVector != null)
            {
                Color color = new Color(1f, 0.4f, 0.3f);
                corner = Custom.RectCollision(corner, pos, room.TileRect(intVector.Value)).GetCorner(FloatRect.CornerLabel.D);
                room.AddObject(new Explosion(room, this, corner, 7, 250f, 6.2f, 2f, 280f, 0.25f, this, 0.3f, 160f, 1f));
                room.AddObject(new Explosion.ExplosionLight(corner, 280f, 1f, 7, color));
                room.AddObject(new Explosion.ExplosionLight(corner, 230f, 1f, 3, new Color(1f, 1f, 1f)));
                room.AddObject(new ShockWave(corner, 330f, 0.045f, 5, false));
                for (int i = 0; i < 25; i++)
                {
                    Vector2 a2 = Custom.RNV();
                    if (room.GetTile(corner + a2 * 20f).Solid)
                    {
                        if (!room.GetTile(corner - a2 * 20f).Solid)
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
                        room.AddObject(new Spark(corner + a2 * Mathf.Lerp(30f, 60f, UnityEngine.Random.value), a2 * Mathf.Lerp(7f, 38f, UnityEngine.Random.value) + Custom.RNV() * 20f * UnityEngine.Random.value, Color.Lerp(color, new Color(1f, 1f, 1f), UnityEngine.Random.value), null, 11, 28));
                    }
                    room.AddObject(new Explosion.FlashingSmoke(corner + a2 * 40f * UnityEngine.Random.value, a2 * Mathf.Lerp(4f, 20f, Mathf.Pow(UnityEngine.Random.value, 2f)), 1f + 0.05f * UnityEngine.Random.value, new Color(1f, 1f, 1f), color, UnityEngine.Random.Range(3, 11)));
                }
                for (int k = 0; k < 6; k++)
                {
                    room.AddObject(new ScavengerBomb.BombFragment(corner, Custom.DegToVec((k + UnityEngine.Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, UnityEngine.Random.value)));
                }
                room.ScreenMovement(new Vector2?(corner), default, 0.9f);
                for (int l = 0; l < abstractPhysicalObject.stuckObjects.Count; l++)
                {
                    abstractPhysicalObject.stuckObjects[l].Deactivate();
                }
                room.PlaySound(SoundID.Bomb_Explode, corner);
                room.InGameNoise(new InGameNoise(corner, 9000f, this, 1f));
            }
        }

        public AlcedoAI AI;
        public AlcedoTentacle[] tentacles;
        public AlcedoLeg[] legs;
        public Tentacle neck;
        public Tentacle waist;
        public IntVector2 mouseTilePos;
        public Vector2 moveDirection;
        public bool hangingInTentacle;
        public bool hangingInLeg;
        public int cantFindNewGripCounter;
        public int cantFindLegNewGripCounter;
        private int releaseGrippingTentacle;
        private int releaseGrippingLeg;
        public bool hoverStill;
        public bool lastHoverStill;
        public IntVector2 hoverPos;
        public int dontSwitchModesCounter;
        public int timeSinceLastTakeoff;
        public float wingFlapAmplitude;
        public float wingFlap;
        public float wingLength;
        public float legLength;
        public int landingBrake;
        public Vector2 landingBrakePos;
        private AlcedoThruster[] thrusters;
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
        public float swim;
        public float upForceByWing;
        public float upForceByLeg;

        public float limbSize;
        public float limbThickness;
        public float stepLength;
        public float liftFeet;
        public float feetDown;
        public float noGripSpeed;
        public float limbSpeed;
        public float limbQuickness;
        public int limbGripDelay;
        public bool smoothenLegMovement;
        public float legPairDisplacement;
        public float bodySizeFac; 
        public Vector2 limbsAimFor;

        private class AlcedoThruster
        {
            public Vector2 ThrustVector
            {
                get
                {
                    return Custom.DegToVec(Custom.AimFromOneVectorToAnother(alcedo.bodyChunks[1].pos, alcedo.bodyChunks[0].pos) + angle);
                }
            }

            public bool Active
            {
                get
                {
                    return ThrustersControlled && (alcedo.landingBrake > 0 || thrust > 0);
                }
            }

            public void Activate(int frames)
            {
                if (!ThrustersControlled)
                {
                    return;
                }
                if (thrust < frames)
                {
                    thrust = frames;
                }
            }

            public Vector2 ExhaustPos
            {
                get
                {
                    return Vector2.Lerp(smokeChunkA.pos, smokeChunkB.pos, smokeChunkLerp) + ThrustVector * 14f;
                }
            }

            public float Force
            {
                get
                {
                    float num = Mathf.Min(1f, alcedo.jetFuel * 2f);
                    num *= Mathf.Min(1f, thrust / 5f);
                    num = Mathf.Pow(num, 0.4f);
                    if (alcedo.landingBrake <= 0 || num >= 0.5f)
                    {
                        return num;
                    }
                    if (!alcedo.IsMiros)
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
                    return !alcedo.safariControlled || alcedo.inputWithDiagonals != null && (alcedo.inputWithDiagonals.Value.AnyDirectionalInput || alcedo.inputWithDiagonals.Value.jmp);
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
                if (thrust > 0)
                {
                    thrust--;
                }
                if (Active)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        alcedo.bodyChunks[i].vel -= ThrustVector * (alcedo.IsKing ? 1.2f : 0.8f) * Force;
                    }
                    alcedo.jetFuel -= 1f / (alcedo.IsKing ? 60f : 40f);
                    if (alcedo.room.BeingViewed)
                    {
                        if (!alcedo.room.PointSubmerged(ExhaustPos))
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
                            alcedo.room.AddObject(new Bubble(ExhaustPos, ThrustVector * 45f * Force, false, false));
                        }
                    }
                    if (!lastActive)
                    {
                        alcedo.room.PlaySound(SoundID.Vulture_Jet_Start, smokeChunkA);
                    }
                }
                else if (lastActive)
                {
                    alcedo.room.PlaySound(SoundID.Vulture_Jet_Stop, smokeChunkA);
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
                lastActive = Active;
            }

            public float Utility()
            {
                Vector2 a = new Vector2(0f, 0f);
                for (int i = 0; i < alcedo.bodyChunks.Length; i++)
                {
                    a += alcedo.bodyChunks[i].vel;
                }
                a /= alcedo.bodyChunks.Length;
                return Mathf.Max(0f, -Vector2.Dot(alcedo.moveDirection, a.normalized)) * Mathf.Max(0f, Vector2.Dot(alcedo.moveDirection, -ThrustVector));
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
                wingHealth = new float[flag ? 4 : 2];
                for (int i = 0; i < wingHealth.Length; i++)
                {
                    wingHealth[i] = 1f;
                }
                limbHealth = new float[4];
                for (int i = 0; i < wingHealth.Length; i++)
                {
                    limbHealth[i] = 1f;
                }
                mask = !flag;
            }
            public override string ToString()
            {
                string text = HealthBaseSaveString() + (mask ? "" : "<cB>NOMASK");
                foreach (KeyValuePair<string, string> keyValuePair in unrecognizedSaveStrings)
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
                        mask = false;
                    }
                }
                unrecognizedSaveStrings.Remove("NOMASK");
            }

            public float[] wingHealth; 
            public float[] limbHealth;
            public bool mask;
        }
    }
}
