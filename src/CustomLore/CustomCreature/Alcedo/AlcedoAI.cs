using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature.Alcedo
{
    sealed class AlcedoAI : ArtificialIntelligence, ILookingAtCreatures, IUseARelationshipTracker
    {
        public AlcedoAI(AbstractCreature creature, World world) : base(creature, world)
        {
            alcedo.AI = this;
            AddModule(new AlcedoPather(this, world, creature));
            pathFinder.accessibilityStepsPerFrame = 60;
            pathFinder.stepsPerFrame = IsKing ? 50 : 30;
            AddModule(new Tracker(this, 10, 5, -1, 0.5f, 5, 5, 10));
            AddModule(new RainTracker(this));
            AddModule(new DenFinder(this, creature));
            AddModule(new PreyTracker(this, 5, IsMiros ? 1.8f : IsKing ? 1.5f : 1.1f, 60f, IsMiros ? 600f : 800f, IsKing ? 0.2f : 0.75f));
            AddModule(new RelationshipTracker(this, tracker));
            disencouragedTracker = new DisencouragedTracker(this);
            AddModule(disencouragedTracker);
            AddModule(new StuckTracker(this, true, true));
            stuckTracker.AddSubModule(new StuckTracker.GetUnstuckPosCalculator(stuckTracker));
            stuckTracker.AddSubModule(new StuckTracker.CloseToGoalButNotSeeingItTracker(stuckTracker, 5f));
            pathFinder.walkPastPointOfNoReturn = true;
            AddModule(new UtilityComparer(this));
            utilityComparer.AddComparedModule(preyTracker, null, 1f, 1.1f);
            utilityComparer.AddComparedModule(rainTracker, null, 1f, 1.1f);
            utilityComparer.AddComparedModule(stuckTracker, null, 1f, 1.1f);
            utilityComparer.AddComparedModule(disencouragedTracker, null, 0.95f, 1.1f);
            creatureLooker = new CreatureLooker(this, tracker, creature.realizedCreature, 0.016666668f, 70);
        }
        public Alcedo alcedo
        {
            get
            {
                return creature.realizedCreature as Alcedo;
            }
        }
        private bool IsKing
        {
            get
            {
                return creature.creatureTemplate.type == CreatureTemplate.Type.KingVulture;
            }
        }
        private bool IsMiros
        {
            get
            {
                return ModManager.MSC && creature.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.MirosVulture;
            }
        }
        public float disencouraged
        {
            get
            {
                return disencouragedTracker.disencouraged;
            }
            set
            {
                disencouragedTracker.disencouraged = value;
            }
        }

        public override void NewRoom(Room room)
        {
            kingTuskShootPos = creature.pos;
            base.NewRoom(room);
            timeInRoom = 0;
        }

        public override void Update()
        {
            if (behavior == Behavior.Hunt && !RainWorldGame.RequestHeavyAi(alcedo))
            {
                return;
            }
            if (ModManager.MSC && alcedo.LickedByPlayer != null)
            {
                tracker.SeeCreature(alcedo.LickedByPlayer.abstractCreature);
                if (timeInRoom - 2 > 6000)
                {
                    timeInRoom -= 2;
                }
            }
            if (debugDestinationVisualizer != null)
            {
                debugDestinationVisualizer.Update();
            }
            if (creatureLooker != null)
            {
                creatureLooker.Update();
            }
            timeInRoom++;
            if (alcedo.room.game.IsStorySession && alcedo.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
            {
                timeInRoom++;
            }
            disencouraged = Mathf.Max(0f, disencouraged - 1f / Mathf.Lerp(600f, 4800f, disencouraged));
            preyInTuskChargeRange = false;
            behavior = Behavior.Idle;
            utilityComparer.GetUtilityTracker(preyTracker).weight = 0.05f + 0.95f * Mathf.InverseLerp(IsMiros ? 4000f : 9600f, IsMiros ? 7600f : 6000f, timeInRoom);
            if (IsMiros)
            {
                utilityComparer.GetUtilityTracker(disencouragedTracker).weight += Mathf.InverseLerp(2000f, 13600f, timeInRoom);
            }
            if (ModManager.MMF && alcedo.bodyChunks[0].pos.y < -alcedo.bodyChunks[0].restrictInRoomRange + 1f)
            {
                creature.abstractAI.SetDestination(alcedo.room.GetWorldCoordinate(new Vector2(alcedo.bodyChunks[0].pos.x, 500f)));
                return;
            }
            AIModule aimodule = utilityComparer.HighestUtilityModule();
            if (utilityComparer.HighestUtility() > 0.01f && aimodule != null)
            {
                if (aimodule is PreyTracker)
                {
                    behavior = Behavior.Hunt;
                }
                if (aimodule is StuckTracker)
                {
                    behavior = Behavior.GetUnstuck;
                }
                if (aimodule is DisencouragedTracker)
                {
                    behavior = Behavior.Disencouraged;
                }
            }
            if (alcedo.grasps[0] != null && alcedo.grasps[0].grabbed is Creature && alcedo.Template.CreatureRelationship(alcedo.grasps[0].grabbed as Creature).type == CreatureTemplate.Relationship.Type.Eats)
            {
                behavior = denFinder.GetDenPosition() != null ? Behavior.ReturnPrey : Behavior.Idle;
            }
            if (!IsMiros && (creature.abstractAI as AlcedoAbstractAI).lostMask != null && utilityComparer.HighestUtility() < 0.4f && (creature.abstractAI as AlcedoAbstractAI).lostMask.Room.realizedRoom == alcedo.room && (creature.abstractAI as AlcedoAbstractAI).lostMask.realizedObject != null)
            {
                behavior = Behavior.GoToMask;
                WorldCoordinate worldCoordinate = alcedo.room.GetWorldCoordinate((creature.abstractAI as AlcedoAbstractAI).lostMask.realizedObject.firstChunk.pos);
                if (creature.world.GetAbstractRoom(worldCoordinate.room).AttractionForCreature(creature.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
                {
                    SetDestination(worldCoordinate);
                }
            }
            if (!(behavior == Behavior.GoToMask))
            {
                if (behavior == Behavior.Idle)
                {
                    creature.abstractAI.AbstractBehavior(1);
                    if (creature.world.GetAbstractRoom(creature.abstractAI.destination.room).AttractionForCreature(creature.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden && creature.abstractAI.destination.room == creature.pos.room && creature.abstractAI.destination.NodeDefined && creature.world.GetNode(creature.abstractAI.destination).type == AbstractRoomNode.Type.SkyExit && (!creature.abstractAI.destination.TileDefined || creature.abstractAI.destination.Tile.FloatDist(creature.pos.Tile) < 10f))
                    {
                        RoomBorderExit roomBorderExit = alcedo.room.borderExits[creature.abstractAI.destination.abstractNode - alcedo.room.exitAndDenIndex.Length];
                        if (roomBorderExit.borderTiles.Length != 0)
                        {
                            IntVector2 intVector = roomBorderExit.borderTiles[Random.Range(0, roomBorderExit.borderTiles.Length)];
                            IntVector2 intVector2 = new IntVector2(0, 1);
                            if (intVector.x == 0)
                            {
                                intVector2 = new IntVector2(-1, 0);
                            }
                            else if (intVector.x == alcedo.room.TileWidth - 1)
                            {
                                intVector2 = new IntVector2(1, 0);
                            }
                            else if (intVector.y == 0)
                            {
                                intVector2 = new IntVector2(0, -1);
                            }
                            intVector += intVector2 * (intVector2.y == 1 ? Random.Range(0, 40) : Random.Range(0, 10));
                            creature.abstractAI.SetDestination(new WorldCoordinate(creature.abstractAI.destination.room, intVector.x, intVector.y, creature.abstractAI.destination.abstractNode));
                        }
                    }
                }
                else if (behavior == Behavior.ReturnPrey || behavior == Behavior.EscapeRain || behavior == Behavior.Disencouraged)
                {
                    focusCreature = null;
                    if (denFinder.GetDenPosition() != null)
                    {
                        creature.abstractAI.SetDestination(denFinder.GetDenPosition().Value);
                    }
                }
                else if (behavior == Behavior.Hunt)
                {
                    focusCreature = preyTracker.MostAttractivePrey;
                    if (focusCreature.dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks)
                    {
                        timeInRoom = 0;
                    }
                    WorldCoordinate worldCoordinate2 = focusCreature.BestGuessForPosition();
                    bool flag;
                    if (IsMiros)
                    {
                        flag = focusCreature.representedCreature.creatureTemplate.type == DLCSharedEnums.CreatureTemplateType.MirosVulture;
                    }
                    else
                    {
                        flag = focusCreature.representedCreature.creatureTemplate.TopAncestor().type == TheOutsiderEnums.CreatureTemplateType.Alcedo;
                    }
                    if (flag && focusCreature.VisualContact && focusCreature.representedCreature.realizedCreature != null)
                    {
                        worldCoordinate2 = alcedo.room.GetWorldCoordinate(focusCreature.representedCreature.realizedCreature.bodyChunks[4].pos);
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
                    else if (creature.world.GetAbstractRoom(worldCoordinate2.room).AttractionForCreature(creature.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
                    {
                        creature.abstractAI.SetDestination(worldCoordinate2);
                    }
                    if (focusCreature.VisualContact)
                    {
                        Creature realizedCreature = focusCreature.representedCreature.realizedCreature;
                        if (realizedCreature.bodyChunks.Length != 0)
                        {
                            BodyChunk bodyChunk = realizedCreature.bodyChunks[Random.Range(0, realizedCreature.bodyChunks.Length)];
                            preyInTuskChargeRange = Custom.DistLess(alcedo.mainBodyChunk.pos, bodyChunk.pos, 230f);
                            if ((!alcedo.AirBorne || Random.value < 0.016666668f) && alcedo.tuskCharge == 1f && alcedo.snapFrames == 0 && !alcedo.safariControlled && Custom.DistLess(alcedo.mainBodyChunk.pos, bodyChunk.pos, 130f) && alcedo.room.VisualContact(alcedo.bodyChunks[4].pos, bodyChunk.pos))
                            {
                                alcedo.Snap(bodyChunk);
                            }
                        }
                    }
                }
                else if (behavior == Behavior.GetUnstuck)
                {
                    creature.abstractAI.SetDestination(stuckTracker.getUnstuckPosCalculator.unstuckGoalPosition);
                }
            }
            base.Update();
        }

        public float KingTuskShootPosScore(WorldCoordinate test)
        {
            if (!pathFinder.CoordinateReachable(test))
            {
                return float.MaxValue;
            }
            float num = Vector2.Distance(alcedo.room.MiddleOfTile(test), alcedo.room.MiddleOfTile(focusCreature.BestGuessForPosition()));
            float num2 = Mathf.Abs(KingTusks.Tusk.shootRange * 0.9f - num);
            num2 += Vector2.Distance(alcedo.room.MiddleOfTile(test), alcedo.room.MiddleOfTile(kingTuskShootPos)) / 40f;
            if (test.y >= alcedo.room.TileHeight - 1)
            {
                num2 += 1000f;
            }
            num2 -= Mathf.Min(alcedo.room.aimap.getTerrainProximity(test), 10) * 20f;
            num2 += Custom.LerpMap(num, KingTusks.Tusk.shootRange - 100f, KingTusks.Tusk.shootRange, 0f, 1000f);
            num2 += Custom.LerpMap(num, KingTusks.Tusk.minShootRange, KingTusks.Tusk.minShootRange / 2f, 0f, 1000f);
            if (!alcedo.room.VisualContact(alcedo.room.MiddleOfTile(test), alcedo.room.MiddleOfTile(focusCreature.BestGuessForPosition())))
            {
                num2 += 10000f;
            }
            return num2;
        }

        public override float VisualScore(Vector2 lookAtPoint, float targetSpeed)
        {
            float num = base.VisualScore(lookAtPoint, targetSpeed);
            if (!Custom.DistLess(alcedo.mainBodyChunk.pos, lookAtPoint, 40f))
            {
                num -= Mathf.Pow(Mathf.InverseLerp(1f, -0.3f, Vector2.Dot((alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].pos - alcedo.bodyChunks[4].pos).normalized, (alcedo.neck.tChunks[alcedo.neck.tChunks.Length - 1].pos - lookAtPoint).normalized)), IsKing ? 0.5f : 0.15f);
            }
            return num;
        }

        public bool OnlyHurtDontGrab(PhysicalObject testObj)
        {
            return testObj is Creature && tracker.RepresentationForCreature((testObj as Creature).abstractCreature, false) != null &&
                tracker.RepresentationForCreature((testObj as Creature).abstractCreature, false).dynamicRelationship.currentRelationship.type == CreatureTemplate.Relationship.Type.Attacks;
        }

        public bool DoIWantToBiteCreature(AbstractCreature creature)
        {
            return IsMiros && !creature.creatureTemplate.smallCreature;
        }

        public override bool TrackerToDiscardDeadCreature(AbstractCreature crit)
        {
            return false;
        }

        public override void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
        {
            creatureLooker.ReevaluateLookObject(creatureRep, firstSpot ? 3f : 2f);
        }

        public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
        {
            Tracker.CreatureRepresentation result;
            if (otherCreature.creatureTemplate.smallCreature)
            {
                result = new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0f, false);
            }
            else
            {
                result = new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 1f, 3);
            }
            return result;
        }

        AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
        {
            if (relationship.type == CreatureTemplate.Relationship.Type.Eats || relationship.type == CreatureTemplate.Relationship.Type.Attacks)
            {
                return preyTracker;
            }
            return null;
        }

        RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
        {
            return null;
        }

        CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
        {
            if (!IsMiros && dRelation.trackerRep.representedCreature.creatureTemplate.TopAncestor().type == TheOutsiderEnums.CreatureTemplateType.Alcedo && dRelation.trackerRep.representedCreature.state.alive && (alcedo.State as Alcedo.AlcedoState).mask != (dRelation.trackerRep.representedCreature.state as Alcedo.AlcedoState).mask)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, (alcedo.State as Alcedo.AlcedoState).mask ? 0.8f : 0.1f);
            }
            if ((alcedo.State as Alcedo.AlcedoState).mask && !IsMiros)
            {
                CreatureTemplate.Relationship relationship = StaticRelationship(dRelation.trackerRep.representedCreature);
                if (dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                {
                    if (alcedo.room.game.IsStorySession && alcedo.room.game.StoryCharacter == SlugcatStats.Name.Yellow)
                    {
                        relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.15f);
                    }
                    else
                    {
                        relationship = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, Custom.LerpMap(creature.world.game.session.difficulty, -1f, 1f, 0.2f, 0.5f) + (IsKing ? 0.15f : 0f));
                    }
                }
                if (IsKing && relationship.type == CreatureTemplate.Relationship.Type.Eats)
                {/*
                    if (this.alcedo.kingTusks.AnyCreatureImpaled)
                    {
                        relationship.intensity = (this.alcedo.kingTusks.ThisCreatureImpaled(dRelation.trackerRep.representedCreature) ? 1f : 0.5f);
                    }
                    else
                    {*/
                    relationship.intensity *= Custom.LerpMap(alcedo.room.aimap.AccessibilityForCreature(dRelation.trackerRep.BestGuessForPosition().Tile, alcedo.Template), 0.9f, 0.6f, 1f, 0.5f + 0.5f * Mathf.InverseLerp(180f, 10f, dRelation.trackerRep.TicksSinceSeen));
                    /*if (this.alcedo.kingTusks.targetRep != null && this.alcedo.kingTusks.ReadyToShoot && this.alcedo.kingTusks.eyesHomeIn > 0f && this.alcedo.kingTusks.targetRep.TicksSinceSeen < 30)
                    {
                        relationship.intensity = Mathf.Pow(relationship.intensity, (this.alcedo.kingTusks.targetRep == dRelation.trackerRep) ? 0.5f : 1.5f);
                    }
                }*/
                }
                return relationship;
            }
            if (alcedo.State.socialMemory.GetLike(dRelation.trackerRep.representedCreature.ID) < -0.25f)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f);
            }
            if (IsMiros && dRelation.trackerRep.representedCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f);
            }
            return new CreatureTemplate.Relationship(StaticRelationship(dRelation.trackerRep.representedCreature).type, StaticRelationship(dRelation.trackerRep.representedCreature).intensity * (IsMiros ? 1f : 0.1f));
        }

        public override PathCost TravelPreference(MovementConnection connection, PathCost cost)
        {
            int num = IsKing ? 8 : 6;
            if (IsMiros)
            {
                num = 10;
            }
            int terrainProximity = alcedo.room.aimap.getTerrainProximity(connection.destinationCoord);
            cost.resistance += Custom.IntClamp(num - terrainProximity, 0, num) * 5f;
            if (IsMiros && terrainProximity <= 3)
            {
                cost.resistance += 100f;
            }
            return cost;
        }

        public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
        {
            if (crit == focusCreature)
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
        public Behavior behavior;
        public new int timeInRoom;
        public WorldCoordinate kingTuskShootPos;
        public bool preyInTuskChargeRange;
        private DisencouragedTracker disencouragedTracker;

        public class Behavior : ExtEnum<Behavior>
        {
            public Behavior(string value, bool register = false) : base(value, register)
            {
            }

            public static readonly Behavior Idle = new Behavior("Idle", true);
            public static readonly Behavior Hunt = new Behavior("Hunt", true);
            public static readonly Behavior EscapeRain = new Behavior("EscapeRain", true);
            public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", true);
            public static readonly Behavior GetUnstuck = new Behavior("GetUnstuck", true);
            public static readonly Behavior Disencouraged = new Behavior("Disencouraged", true);
            public static readonly Behavior GoToMask = new Behavior("GoToMask", true);
        }

        public class DisencouragedTracker : AIModule
        {
            public DisencouragedTracker(ArtificialIntelligence AI) : base(AI)
            {
            }

            public override void Update()
            {
                base.Update();
                if (disencouraged > 1f)
                {
                    disencouraged = Mathf.Lerp(disencouraged, 1f, (AI as AlcedoAI).IsMiros ? 0.075f : 0.05f);
                }
            }

            public override float Utility()
            {
                return Mathf.Pow(Mathf.Clamp(disencouraged, 0f, 1f), 3f);
            }

            public float disencouraged;
        }
    }
}
