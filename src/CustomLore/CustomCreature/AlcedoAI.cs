using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature
{
    sealed class AlcedoAI : ArtificialIntelligence, ILookingAtCreatures, IUseARelationshipTracker
    {
        public AlcedoAI(AbstractCreature creature, World world) : base(creature, world)
        {
            this.alcedo.AI = this;
            base.AddModule(new AlcedoPather(this, world, creature));
            base.pathFinder.accessibilityStepsPerFrame = 60;
            base.pathFinder.stepsPerFrame = (this.IsKing ? 50 : 30);
            base.AddModule(new Tracker(this, 10, 5, -1, 0.5f, 5, 5, 10));
            base.AddModule(new RainTracker(this));
            base.AddModule(new DenFinder(this, creature));
            base.AddModule(new PreyTracker(this, 5, this.IsMiros ? 1.8f : (this.IsKing ? 1.5f : 1.1f), 60f, this.IsMiros ? 600f : 800f, this.IsKing ? 0.2f : 0.75f));
            base.AddModule(new RelationshipTracker(this, base.tracker));
            this.disencouragedTracker = new AlcedoAI.DisencouragedTracker(this);
            base.AddModule(this.disencouragedTracker);
            base.AddModule(new StuckTracker(this, true, true));
            base.stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(base.stuckTracker));
            base.stuckTracker.AddSubModule(new StuckTracker.CloseToGoalButNotSeeingItTracker(base.stuckTracker, 5f));
            base.pathFinder.walkPastPointOfNoReturn = true;
            base.AddModule(new UtilityComparer(this));
            base.utilityComparer.AddComparedModule(base.preyTracker, null, 1f, 1.1f);
            base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
            base.utilityComparer.AddComparedModule(base.stuckTracker, null, 1f, 1.1f);
            base.utilityComparer.AddComparedModule(this.disencouragedTracker, null, 0.95f, 1.1f);
            this.creatureLooker = new CreatureLooker(this, base.tracker, creature.realizedCreature, 0.016666668f, 70);
        }
        public Alcedo alcedo
        {
            get
            {
                return this.creature.realizedCreature as Alcedo;
            }
        }
        private bool IsKing
        {
            get
            {
                return this.creature.creatureTemplate.type == CreatureTemplate.Type.KingVulture;
            }
        }
        private bool IsMiros
        {
            get
            {
                return ModManager.MSC && this.creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
            }
        }
        public float disencouraged
        {
            get
            {
                return this.disencouragedTracker.disencouraged;
            }
            set
            {
                this.disencouragedTracker.disencouraged = value;
            }
        }

        public override void NewRoom(Room room)
        {
            this.kingTuskShootPos = this.creature.pos;
            base.NewRoom(room);
            this.timeInRoom = 0;
        }

        public override void Update()
        {
            if (this.behavior == AlcedoAI.Behavior.Hunt && !RainWorldGame.RequestHeavyAi(this.alcedo))
            {
                return;
            }
            if (ModManager.MSC && this.alcedo.LickedByPlayer != null)
            {
                base.tracker.SeeCreature(this.alcedo.LickedByPlayer.abstractCreature);
                if (this.timeInRoom - 2 > 6000)
                {
                    this.timeInRoom -= 2;
                }
            }
            if (this.debugDestinationVisualizer != null)
            {
                this.debugDestinationVisualizer.Update();
            }
            if (this.creatureLooker != null)
            {
                this.creatureLooker.Update();
            }
            this.timeInRoom++;
            if (this.alcedo.room.game.IsStorySession && this.alcedo.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
            {
                this.timeInRoom++;
            }
            this.disencouraged = Mathf.Max(0f, this.disencouraged - 1f / Mathf.Lerp(600f, 4800f, this.disencouraged));
            this.preyInTuskChargeRange = false;
            this.behavior = AlcedoAI.Behavior.Idle;
            base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = 0.05f + 0.95f * Mathf.InverseLerp(this.IsMiros ? 4000f : 9600f, this.IsMiros ? 7600f : 6000f, (float)this.timeInRoom);
            if (this.IsMiros)
            {
                base.utilityComparer.GetUtilityTracker(this.disencouragedTracker).weight += Mathf.InverseLerp(2000f, 13600f, (float)this.timeInRoom);
            }
            if (ModManager.MMF && this.alcedo.bodyChunks[0].pos.y < -this.alcedo.bodyChunks[0].restrictInRoomRange + 1f)
            {
                this.creature.abstractAI.SetDestination(this.alcedo.room.GetWorldCoordinate(new Vector2(this.alcedo.bodyChunks[0].pos.x, 500f)));
                return;
            }
            AIModule aimodule = base.utilityComparer.HighestUtilityModule();
            if (base.utilityComparer.HighestUtility() > 0.01f && aimodule != null)
            {
                if (aimodule is PreyTracker)
                {
                    this.behavior = AlcedoAI.Behavior.Hunt;
                }
                if (aimodule is StuckTracker)
                {
                    this.behavior = AlcedoAI.Behavior.GetUnstuck;
                }
                if (aimodule is AlcedoAI.DisencouragedTracker)
                {
                    this.behavior = AlcedoAI.Behavior.Disencouraged;
                }
            }
            if (this.alcedo.grasps[0] != null && this.alcedo.grasps[0].grabbed is Creature && this.alcedo.Template.CreatureRelationship(this.alcedo.grasps[0].grabbed as Creature).type == CreatureTemplate.Relationship.Type.Eats)
            {
                this.behavior = ((base.denFinder.GetDenPosition() != null) ? AlcedoAI.Behavior.ReturnPrey : AlcedoAI.Behavior.Idle);
            }
            if (!this.IsMiros && (this.creature.abstractAI as AlcedoAbstractAI).lostMask != null && base.utilityComparer.HighestUtility() < 0.4f && (this.creature.abstractAI as AlcedoAbstractAI).lostMask.Room.realizedRoom == this.alcedo.room && (this.creature.abstractAI as AlcedoAbstractAI).lostMask.realizedObject != null)
            {
                this.behavior = AlcedoAI.Behavior.GoToMask;
                WorldCoordinate worldCoordinate = this.alcedo.room.GetWorldCoordinate((this.creature.abstractAI as AlcedoAbstractAI).lostMask.realizedObject.firstChunk.pos);
                if (this.creature.world.GetAbstractRoom(worldCoordinate.room).AttractionForCreature(this.creature.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
                {
                    base.SetDestination(worldCoordinate);
                }
            }
            if (!(this.behavior == AlcedoAI.Behavior.GoToMask))
            {
                if (this.behavior == AlcedoAI.Behavior.Idle)
                {
                    this.creature.abstractAI.AbstractBehavior(1);
                    if (this.creature.world.GetAbstractRoom(this.creature.abstractAI.destination.room).AttractionForCreature(this.creature.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden && this.creature.abstractAI.destination.room == this.creature.pos.room && this.creature.abstractAI.destination.NodeDefined && this.creature.world.GetNode(this.creature.abstractAI.destination).type == AbstractRoomNode.Type.SkyExit && (!this.creature.abstractAI.destination.TileDefined || this.creature.abstractAI.destination.Tile.FloatDist(this.creature.pos.Tile) < 10f))
                    {
                        RoomBorderExit roomBorderExit = this.alcedo.room.borderExits[this.creature.abstractAI.destination.abstractNode - this.alcedo.room.exitAndDenIndex.Length];
                        if (roomBorderExit.borderTiles.Length != 0)
                        {
                            IntVector2 intVector = roomBorderExit.borderTiles[UnityEngine.Random.Range(0, roomBorderExit.borderTiles.Length)];
                            IntVector2 intVector2 = new IntVector2(0, 1);
                            if (intVector.x == 0)
                            {
                                intVector2 = new IntVector2(-1, 0);
                            }
                            else if (intVector.x == this.alcedo.room.TileWidth - 1)
                            {
                                intVector2 = new IntVector2(1, 0);
                            }
                            else if (intVector.y == 0)
                            {
                                intVector2 = new IntVector2(0, -1);
                            }
                            intVector += intVector2 * ((intVector2.y == 1) ? UnityEngine.Random.Range(0, 40) : UnityEngine.Random.Range(0, 10));
                            this.creature.abstractAI.SetDestination(new WorldCoordinate(this.creature.abstractAI.destination.room, intVector.x, intVector.y, this.creature.abstractAI.destination.abstractNode));
                        }
                    }
                }
                else if (this.behavior == AlcedoAI.Behavior.ReturnPrey || this.behavior == AlcedoAI.Behavior.EscapeRain || this.behavior == AlcedoAI.Behavior.Disencouraged)
                {
                    this.focusCreature = null;
                    if (base.denFinder.GetDenPosition() != null)
                    {
                        this.creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
                    }
                }
                else if (this.behavior == AlcedoAI.Behavior.Hunt)
                {
                    this.focusCreature = base.preyTracker.MostAttractivePrey;
                    if (this.focusCreature.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks)
                    {
                        this.timeInRoom = 0;
                    }
                    WorldCoordinate worldCoordinate2 = this.focusCreature.BestGuessForPosition();
                    bool flag;
                    if (this.IsMiros)
                    {
                        flag = (this.focusCreature.representedCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture);
                    }
                    else
                    {
                        flag = this.focusCreature.representedCreature.creatureTemplate.TopAncestor().type == OutsiderEnums.CreatureTemplateType.Alcedo;
                    }
                    if (flag && this.focusCreature.VisualContact && this.focusCreature.representedCreature.realizedCreature != null)
                    {
                        worldCoordinate2 = this.alcedo.room.GetWorldCoordinate(this.focusCreature.representedCreature.realizedCreature.bodyChunks[4].pos);
                    }/*
                    if (!this.IsMiros && this.alcedo.kingTusks != null && this.alcedo.kingTusks.ReadyToShoot && this.focusCreature != null && this.focusCreature.TicksSinceSeen < 80 && this.alcedo.kingTusks.WantToShoot(false, true))
                    {
                        WorldCoordinate worldCoordinate3 = this.alcedo.room.GetWorldCoordinate(this.alcedo.room.MiddleOfTile(this.focusCreature.BestGuessForPosition()) + Custom.DegToVec(180f * UnityEngine.Random.value * UnityEngine.Random.value * ((UnityEngine.Random.value < 0.5f) ? -1f : 1f)) * Mathf.Lerp(KingTusks.Tusk.minShootRange, KingTusks.Tusk.shootRange, UnityEngine.Random.value));
                        float num = this.KingTuskShootPosScore(this.kingTuskShootPos);
                        if (this.KingTuskShootPosScore(worldCoordinate3) < num)
                        {
                            this.kingTuskShootPos = worldCoordinate3;
                        }
                        this.creature.abstractAI.SetDestination(this.kingTuskShootPos);
                    }*/
                    else if (this.creature.world.GetAbstractRoom(worldCoordinate2.room).AttractionForCreature(this.creature.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
                    {
                        this.creature.abstractAI.SetDestination(worldCoordinate2);
                    }
                    if (this.focusCreature.VisualContact)
                    {
                        Creature realizedCreature = this.focusCreature.representedCreature.realizedCreature;
                        if (realizedCreature.bodyChunks.Length != 0)
                        {
                            BodyChunk bodyChunk = realizedCreature.bodyChunks[UnityEngine.Random.Range(0, realizedCreature.bodyChunks.Length)];
                            this.preyInTuskChargeRange = Custom.DistLess(this.alcedo.mainBodyChunk.pos, bodyChunk.pos, 230f);
                            if ((!this.alcedo.AirBorne || UnityEngine.Random.value < 0.016666668f) && this.alcedo.tuskCharge == 1f && this.alcedo.snapFrames == 0 && !this.alcedo.isLaserActive() && !this.alcedo.safariControlled && Custom.DistLess(this.alcedo.mainBodyChunk.pos, bodyChunk.pos, 130f) && this.alcedo.room.VisualContact(this.alcedo.bodyChunks[4].pos, bodyChunk.pos))
                            {
                                this.alcedo.Snap(bodyChunk);
                            }
                        }
                    }
                }
                else if (this.behavior == AlcedoAI.Behavior.GetUnstuck)
                {
                    this.creature.abstractAI.SetDestination(base.stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
                }
            }
            base.Update();
        }

        public float KingTuskShootPosScore(WorldCoordinate test)
        {
            if (!base.pathFinder.CoordinateReachable(test))
            {
                return float.MaxValue;
            }
            float num = Vector2.Distance(this.alcedo.room.MiddleOfTile(test), this.alcedo.room.MiddleOfTile(this.focusCreature.BestGuessForPosition()));
            float num2 = Mathf.Abs(KingTusks.Tusk.shootRange * 0.9f - num);
            num2 += Vector2.Distance(this.alcedo.room.MiddleOfTile(test), this.alcedo.room.MiddleOfTile(this.kingTuskShootPos)) / 40f;
            if (test.y >= this.alcedo.room.TileHeight - 1)
            {
                num2 += 1000f;
            }
            num2 -= (float)Mathf.Min(this.alcedo.room.aimap.getTerrainProximity(test), 10) * 20f;
            num2 += Custom.LerpMap(num, KingTusks.Tusk.shootRange - 100f, KingTusks.Tusk.shootRange, 0f, 1000f);
            num2 += Custom.LerpMap(num, KingTusks.Tusk.minShootRange, KingTusks.Tusk.minShootRange / 2f, 0f, 1000f);
            if (!this.alcedo.room.VisualContact(this.alcedo.room.MiddleOfTile(test), this.alcedo.room.MiddleOfTile(this.focusCreature.BestGuessForPosition())))
            {
                num2 += 10000f;
            }
            return num2;
        }

        public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
        {
            float num = base.VisualScore(lookAtPoint, targetSpeed);
            if (!Custom.DistLess(this.alcedo.mainBodyChunk.pos, lookAtPoint, 40f))
            {
                num -= Mathf.Pow(Mathf.InverseLerp(1f, -0.3f, Vector2.Dot((this.alcedo.neck.tChunks[this.alcedo.neck.tChunks.Length - 1].pos - this.alcedo.bodyChunks[4].pos).normalized, (this.alcedo.neck.tChunks[this.alcedo.neck.tChunks.Length - 1].pos - lookAtPoint).normalized)), this.IsKing ? 0.5f : 0.15f);
            }
            return num;
        }

        public bool OnlyHurtDontGrab(PhysicalObject testObj)
        {
            return testObj is Creature && base.tracker.RepresentationForCreature((testObj as Creature).abstractCreature, false) != null && base.tracker.RepresentationForCreature((testObj as Creature).abstractCreature, false).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks;
        }

        public bool DoIWantToBiteCreature(AbstractCreature creature)
        {
            return this.IsMiros && !creature.creatureTemplate.smallCreature;
        }

        public override bool TrackerToDiscardDeadCreature(AbstractCreature crit)
        {
            return false;
        }

        public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
        {
            this.creatureLooker.ReevaluateLookObject(creatureRep, firstSpot ? 3f : 2f);
        }

        public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
        {
            Tracker.CreatureRepresentation result;
            if (otherCreature.creatureTemplate.smallCreature)
            {
                result = new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, false);
            }
            else
            {
                result = new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
            }
            return result;
        }

        AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
        {
            if (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Attacks)
            {
                return base.preyTracker;
            }
            return null;
        }

        RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
        {
            return null;
        }

        CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
        {
            if (!this.IsMiros && dRelation.trackerRep.representedCreature.creatureTemplate.TopAncestor().type == OutsiderEnums.CreatureTemplateType.Alcedo && dRelation.trackerRep.representedCreature.state.alive && (this.alcedo.State as Alcedo.AlcedoState).mask != (dRelation.trackerRep.representedCreature.state as Alcedo.AlcedoState).mask)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, (this.alcedo.State as Alcedo.AlcedoState).mask ? 0.8f : 0.1f);
            }
            if ((this.alcedo.State as Alcedo.AlcedoState).mask && !this.IsMiros)
            {
                CreatureTemplate.Relationship relationship = base.StaticRelationship(dRelation.trackerRep.representedCreature);
                if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                {
                    if (this.alcedo.room.game.IsStorySession && this.alcedo.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
                    {
                        relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f);
                    }
                    else
                    {
                        relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Custom.LerpMap(this.creature.world.game.session.difficulty, -1f, 1f, 0.2f, 0.5f) + (this.IsKing ? 0.15f : 0f));
                    }
                }
                if (this.IsKing && relationship.type == CreatureTemplate.Relationship.Type.Eats)
                {/*
                    if (this.alcedo.kingTusks.AnyCreatureImpaled)
                    {
                        relationship.intensity = (this.alcedo.kingTusks.ThisCreatureImpaled(dRelation.trackerRep.representedCreature) ? 1f : 0.5f);
                    }
                    else
                    {*/
                    relationship.intensity *= Custom.LerpMap(this.alcedo.room.aimap.AccessibilityForCreature(dRelation.trackerRep.BestGuessForPosition().Tile, this.alcedo.Template), 0.9f, 0.6f, 1f, 0.5f + 0.5f * Mathf.InverseLerp(180f, 10f, (float)dRelation.trackerRep.TicksSinceSeen));
                    /*if (this.alcedo.kingTusks.targetRep != null && this.alcedo.kingTusks.ReadyToShoot && this.alcedo.kingTusks.eyesHomeIn > 0f && this.alcedo.kingTusks.targetRep.TicksSinceSeen < 30)
                    {
                        relationship.intensity = Mathf.Pow(relationship.intensity, (this.alcedo.kingTusks.targetRep == dRelation.trackerRep) ? 0.5f : 1.5f);
                    }
                }*/
                }
                return relationship;
            }
            if (this.alcedo.State.socialMemory.GetLike(dRelation.trackerRep.representedCreature.ID) < -0.25f)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f);
            }
            if (this.IsMiros && dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f);
            }
            return new CreatureTemplate.Relationship(base.StaticRelationship(dRelation.trackerRep.representedCreature).type, base.StaticRelationship(dRelation.trackerRep.representedCreature).intensity * (this.IsMiros ? 1f : 0.1f));
        }

        public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
        {
            int num = this.IsKing ? 8 : 6;
            if (this.IsMiros)
            {
                num = 10;
            }
            int terrainProximity = this.alcedo.room.aimap.getTerrainProximity(connection.destinationCoord);
            cost.resistance += (float)Custom.IntClamp(num - terrainProximity, 0, num) * 5f;
            if (this.IsMiros && terrainProximity <= 3)
            {
                cost.resistance += 100f;
            }
            return cost;
        }

        public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
        {
            if (crit == this.focusCreature)
            {
                score *= 5f;
            }
            return score;
        }

        public Tracker.CreatureRepresentation ForcedLookCreature()
        {
            return null;
        }

        public void LookAtNothing()
        {
        }

        public DebugDestinationVisualizer debugDestinationVisualizer;
        public CreatureLooker creatureLooker;
        public Tracker.CreatureRepresentation focusCreature;
        public AlcedoAI.Behavior behavior;
        public new int timeInRoom;
        public WorldCoordinate kingTuskShootPos;
        public bool preyInTuskChargeRange;
        private AlcedoAI.DisencouragedTracker disencouragedTracker;

        public class Behavior : ExtEnum<AlcedoAI.Behavior>
        {
            public Behavior(string value, bool register = false) : base(value, register)
            {
            }

            public static readonly AlcedoAI.Behavior Idle = new AlcedoAI.Behavior("Idle", true);
            public static readonly AlcedoAI.Behavior Hunt = new AlcedoAI.Behavior("Hunt", true);
            public static readonly AlcedoAI.Behavior EscapeRain = new AlcedoAI.Behavior("EscapeRain", true);
            public static readonly AlcedoAI.Behavior ReturnPrey = new AlcedoAI.Behavior("ReturnPrey", true);
            public static readonly AlcedoAI.Behavior GetUnstuck = new AlcedoAI.Behavior("GetUnstuck", true);
            public static readonly AlcedoAI.Behavior Disencouraged = new AlcedoAI.Behavior("Disencouraged", true);
            public static readonly AlcedoAI.Behavior GoToMask = new AlcedoAI.Behavior("GoToMask", true);
        }

        public class DisencouragedTracker : AIModule
        {
            public DisencouragedTracker(ArtificialIntelligence AI) : base(AI)
            {
            }

            public override void Update()
            {
                base.Update();
                if (this.disencouraged > 1f)
                {
                    this.disencouraged = Mathf.Lerp(this.disencouraged, 1f, (this.AI as AlcedoAI).IsMiros ? 0.075f : 0.05f);
                }
            }

            public override float Utility()
            {
                return Mathf.Pow(Mathf.Clamp(this.disencouraged, 0f, 1f), 3f);
            }

            public float disencouraged;
        }
    }
}
