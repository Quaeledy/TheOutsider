using RWCustom;
using System.Linq;
using UnityEngine;
using MoreSlugcats;
using static CreatureTemplate.Relationship.Type;
using System.Collections.Generic;
using Noise;
using System;

namespace TheOutsider.CustomLore.CustomCreature
{
    sealed class MothPupAI : SlugNPCAI, IUseItemTracker, IAINoiseReaction, IUseARelationshipTracker, IReactToSocialEvents, FriendTracker.IHaveFriendTracker
    {/*
        public class Food : ExtEnum<Food>
        {
            public static readonly Food DangleFruit = new Food("DangleFruit", register: true);

            public static readonly Food WaterNut = new Food("WaterNut", register: true);

            public static readonly Food JellyFish = new Food("JellyFish", register: true);

            public static readonly Food SlimeMold = new Food("SlimeMold", register: true);

            public static readonly Food EggBugEgg = new Food("EggBugEgg", register: true);

            public static readonly Food FireEgg = new Food("FireEgg", register: true);

            public static readonly Food Popcorn = new Food("Popcorn", register: true);

            public static readonly Food GooieDuck = new Food("GooieDuck", register: true);

            public static readonly Food LillyPuck = new Food("LillyPuck", register: true);

            public static readonly Food GlowWeed = new Food("GlowWeed", register: true);

            public static readonly Food DandelionPeach = new Food("DandelionPeach", register: true);

            public static readonly Food Neuron = new Food("Neuron", register: true);

            public static readonly Food Centipede = new Food("Centipede", register: true);

            public static readonly Food SmallCentipede = new Food("SmallCentipede", register: true);

            public static readonly Food VultureGrub = new Food("VultureGrub", register: true);

            public static readonly Food SmallNeedleWorm = new Food("SmallNeedleWorm", register: true);

            public static readonly Food Hazer = new Food("Hazer", register: true);

            public static readonly Food NotCounted = new Food("NotCounted", register: true);

            public Food(string value, bool register = false)
                : base(value, register)
            {
            }
        }

        public class BehaviorType : ExtEnum<BehaviorType>
        {
            public static readonly BehaviorType Idle = new BehaviorType("Idle", register: true);

            public static readonly BehaviorType Fleeing = new BehaviorType("Fleeing", register: true);

            public static readonly BehaviorType Following = new BehaviorType("Following", register: true);

            public static readonly BehaviorType GrabItem = new BehaviorType("GrabItem", register: true);

            public static readonly BehaviorType Attacking = new BehaviorType("Attacking", register: true);

            public static readonly BehaviorType BeingHeld = new BehaviorType("BeingHeld", register: true);

            public static readonly BehaviorType OnHead = new BehaviorType("OnHead", register: true);

            public static readonly BehaviorType Thrown = new BehaviorType("Thrown", register: true);

            public static readonly BehaviorType ZeroG = new BehaviorType("ZeroG", register: true);

            public static readonly BehaviorType DeerRide = new BehaviorType("DeerRide", register: true);

            public BehaviorType(string value, bool register = false)
                : base(value, register)
            {
            }
        }

        private class CombatDebug
        {
            private DebugSprite[] dbsprts;

            private AbstractCreature slug;

            private SlugNPCAI AI;

            private Color color;

            public CombatDebug(AbstractCreature slug, PathingAssist assist)
            {
                this.slug = slug;
                color = assist?.color ?? new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                dbsprts = new DebugSprite[42];
                dbsprts[0] = new DebugSprite(default(Vector2), new FSprite("pixel"), slug.Room.realizedRoom);
                dbsprts[0].sprite.scale = 6f;
                dbsprts[0].sprite.color = color;
                for (int i = 0; i < 20; i++)
                {
                    dbsprts[1 + i * 2] = new DebugSprite(default(Vector2), new FSprite("pixel"), slug.Room.realizedRoom);
                    dbsprts[1 + i * 2].sprite.scale = 6f;
                    dbsprts[1 + i * 2].sprite.color = color;
                    dbsprts[1 + i * 2 + 1] = new DebugSprite(default(Vector2), new FSprite("pixel"), slug.Room.realizedRoom);
                    dbsprts[1 + i * 2 + 1].sprite.scale = 4f;
                    dbsprts[1 + i * 2 + 1].sprite.color = new Color(0f, 1f - (float)i / 20f, 1f - (float)i / 20f);
                }
                dbsprts[41] = new DebugSprite(default(Vector2), new FSprite("pixel"), slug.Room.realizedRoom);
                dbsprts[41].sprite.scale = 4f;
                dbsprts[41].sprite.color = new Color(1f, 1f, 1f);
                for (int j = 0; j < dbsprts.Length; j++)
                {
                    slug.Room.realizedRoom.AddObject(dbsprts[j]);
                }
                AI = slug.abstractAI.RealAI as SlugNPCAI;
            }

            public void Update()
            {
                for (int i = 0; i < dbsprts.Length; i++)
                {
                    if (dbsprts[i].room != slug.Room.realizedRoom)
                    {
                        dbsprts[i].room.RemoveObject(dbsprts[i]);
                        slug.Room.realizedRoom.AddObject(dbsprts[i]);
                    }
                }
                List<IntVector2> previousAttackPositions = AI.previousAttackPositions;
                for (int j = 0; j < 20; j++)
                {
                    if (previousAttackPositions.Count > j)
                    {
                        dbsprts[1 + j * 2].sprite.isVisible = true;
                        dbsprts[1 + j * 2 + 1].sprite.isVisible = true;
                        dbsprts[1 + j * 2].pos = slug.Room.realizedRoom.MiddleOfTile(previousAttackPositions[j]);
                        dbsprts[1 + j * 2 + 1].pos = slug.Room.realizedRoom.MiddleOfTile(previousAttackPositions[j]);
                    }
                    else
                    {
                        dbsprts[1 + j * 2].sprite.isVisible = false;
                        dbsprts[1 + j * 2 + 1].sprite.isVisible = false;
                    }
                }
                dbsprts[0].pos = slug.realizedCreature.bodyChunks[1].pos - new Vector2(0f, 20f);
                dbsprts[41].pos = dbsprts[0].pos + new Vector2(AI.throwAtTarget * 8, 0f);
                dbsprts[41].sprite.isVisible = AI.throwAtTarget != 0;
            }
        }

        private class PersonalityDebug
        {
            private AbstractCreature slug;

            private FLabel[] labels;

            public PersonalityDebug(AbstractCreature slug)
            {
                AbstractCreature.Personality personality = slug.personality;
                labels = new FLabel[6];
                labels[0] = new FLabel(Custom.GetFont(), "SYMPATHY: " + personality.sympathy);
                labels[1] = new FLabel(Custom.GetFont(), "ENERGY: " + personality.energy);
                labels[2] = new FLabel(Custom.GetFont(), "BRAVERY: " + personality.bravery);
                labels[3] = new FLabel(Custom.GetFont(), "NERVOUS: " + personality.nervous);
                labels[4] = new FLabel(Custom.GetFont(), "AGGRESSION: " + personality.aggression);
                labels[5] = new FLabel(Custom.GetFont(), "DOMINANCE: " + personality.dominance);
                for (int i = 0; i < 6; i++)
                {
                    Futile.stage.AddChild(labels[i]);
                }
                this.slug = slug;
            }

            public void Update()
            {
                if (Input.GetKeyDown(KeyCode.U))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        labels[i].isVisible = false;
                    }
                }
                if (Input.GetKeyDown(KeyCode.J))
                {
                    for (int j = 0; j < 6; j++)
                    {
                        labels[j].isVisible = true;
                    }
                }
                for (int k = 0; k < 6; k++)
                {
                    labels[k].SetPosition(slug.realizedCreature.firstChunk.pos - new Vector2(0f, 12 * k));
                }
            }
        }

        private class PathingAssist
        {
            private DebugSprite[] dbsprts;

            private AbstractCreature slug;

            private SlugNPCAI AI;

            private FLabel behaviorType;

            private FLabel followCloseness;

            private FLabel toldToPlay;

            public Color color;

            public void Update()
            {
                for (int i = 0; i < dbsprts.Length; i++)
                {
                    if (dbsprts[i].room != slug.Room.realizedRoom)
                    {
                        dbsprts[i].room.RemoveObject(dbsprts[i]);
                        slug.Room.realizedRoom.AddObject(dbsprts[i]);
                    }
                }
                dbsprts[0].pos = slug.Room.realizedRoom.MiddleOfTile(slug.abstractAI.destination);
                dbsprts[1].pos = slug.Room.realizedRoom.MiddleOfTile(slug.abstractAI.destination);
                dbsprts[2].pos = slug.realizedCreature.firstChunk.pos + new Vector2(0f, 20f);
                dbsprts[2].sprite.color = ((slug.abstractAI.RealAI as SlugNPCAI).jumping ? new Color(0f, 1f, 1f) : ((slug.abstractAI.RealAI as SlugNPCAI).catchPoles ? new Color(0f, 0f, 1f) : color));
                if (!(slug.abstractAI.RealAI as SlugNPCAI).jumping && !(slug.abstractAI.RealAI as SlugNPCAI).catchPoles && !slug.abstractAI.RealAI.pathFinder.CoordinateViable(slug.pos))
                {
                    dbsprts[2].sprite.color = new Color(1f, 0f, 0f);
                }
                behaviorType.SetPosition(slug.realizedCreature.firstChunk.pos);
                string text = (slug.abstractAI.RealAI as SlugNPCAI).behaviorType.ToString();
                if ((slug.abstractAI.RealAI as SlugNPCAI).AttackingThreat())
                {
                    text += " (ANGRY!)";
                }
                behaviorType.text = text;
                followCloseness.SetPosition(slug.realizedCreature.firstChunk.pos - new Vector2(0f, 12f));
                text = "CURRENT FOLLOW CLOSENESS: " + (slug.abstractAI.RealAI as SlugNPCAI).followCloseness;
                followCloseness.text = text;
                toldToPlay.SetPosition(slug.realizedCreature.firstChunk.pos - new Vector2(0f, 24f));
                text = "TOLD TO PLAY: " + (slug.abstractAI.RealAI as SlugNPCAI).toldToPlay;
                toldToPlay.text = text;
            }

            public PathingAssist(AbstractCreature slug)
            {
                this.slug = slug;
                color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
                behaviorType = new FLabel(Custom.GetFont(), "NULL");
                behaviorType.color = new Color(1f, 1f, 1f);
                followCloseness = new FLabel(Custom.GetFont(), "NULL");
                followCloseness.color = new Color(1f, 1f, 1f);
                toldToPlay = new FLabel(Custom.GetFont(), "NULL");
                toldToPlay.color = new Color(1f, 1f, 1f);
                dbsprts = new DebugSprite[16];
                dbsprts[0] = new DebugSprite(default(Vector2), new FSprite("pixel"), slug.Room.realizedRoom);
                dbsprts[0].sprite.scale = 14f;
                dbsprts[0].sprite.color = color;
                dbsprts[1] = new DebugSprite(default(Vector2), new FSprite("pixel"), slug.Room.realizedRoom);
                dbsprts[1].sprite.scale = 10f;
                dbsprts[1].sprite.color = new Color(0f, 0f, 1f);
                dbsprts[2] = new DebugSprite(default(Vector2), new FSprite("pixel"), slug.Room.realizedRoom);
                dbsprts[2].sprite.scale = 6f;
                dbsprts[2].sprite.color = color;
                for (int i = 0; i < 5; i++)
                {
                    dbsprts[3 + i * 2] = new DebugSprite(default(Vector2), new FSprite("pixel"), slug.Room.realizedRoom);
                    dbsprts[3 + i * 2].sprite.scale = 8f;
                    dbsprts[3 + i * 2].sprite.color = color;
                    dbsprts[3 + i * 2 + 1] = new DebugSprite(default(Vector2), new FSprite("pixel"), slug.Room.realizedRoom);
                    dbsprts[3 + i * 2 + 1].sprite.scale = 6f;
                    dbsprts[3 + i * 2 + 1].sprite.color = new Color(0f, 1f, 0f);
                }
                for (int j = 0; j < 3; j++)
                {
                    dbsprts[13 + j] = new DebugSprite(default(Vector2), new FSprite("pixel"), slug.Room.realizedRoom);
                    dbsprts[13 + j].sprite.scale = 4f;
                    dbsprts[13 + j].sprite.color = new Color(1f, 1f, 1f);
                }
                for (int k = 0; k < dbsprts.Length; k++)
                {
                    slug.Room.realizedRoom.AddObject(dbsprts[k]);
                }
                Futile.stage.AddChild(behaviorType);
                Futile.stage.AddChild(followCloseness);
                Futile.stage.AddChild(toldToPlay);
                AI = slug.abstractAI.RealAI as SlugNPCAI;
            }

            public void VisualizeConnection()
            {
                List<MovementConnection> upcoming = AI.GetUpcoming();
                for (int i = 0; i < 5; i++)
                {
                    if (upcoming != null && upcoming.Count > i)
                    {
                        dbsprts[3 + i * 2].sprite.isVisible = true;
                        dbsprts[3 + i * 2 + 1].sprite.isVisible = true;
                        dbsprts[3 + i * 2].pos = slug.Room.realizedRoom.MiddleOfTile(upcoming[i].destinationCoord);
                        dbsprts[3 + i * 2 + 1].pos = slug.Room.realizedRoom.MiddleOfTile(upcoming[i].destinationCoord);
                    }
                    else
                    {
                        dbsprts[3 + i * 2].sprite.isVisible = false;
                        dbsprts[3 + i * 2 + 1].sprite.isVisible = false;
                    }
                }
            }

            public void VisualizeInput(Player.InputPackage input)
            {
                dbsprts[13].pos = dbsprts[2].pos + new Vector2(input.x * 8, 0f);
                dbsprts[13].sprite.isVisible = input.x != 0;
                dbsprts[14].pos = dbsprts[2].pos + new Vector2(0f, input.y * 8);
                dbsprts[14].sprite.isVisible = input.y != 0;
                dbsprts[15].pos = dbsprts[2].pos;
                dbsprts[15].sprite.isVisible = input.jmp;
            }
        }

        public class SlugNPCTrackState : RelationshipTracker.TrackedCreatureState
        {
            public bool holdingAFriend;

            public bool jawsOccupied;

            public bool hurtAFriend;

            public int annoyingThreat;
        }

        private CombatDebug combatDebug;
        private PersonalityDebug personalityDebug;
        private int foodReaction;
        private float[] foodPreference;
        private int transportDelay;
        private bool nap;
        public bool playWithItem;
        private Vector2 playPos;
        private int throwAtTarget;
        private int turnDelay;
        private float followCloseness;
        private int toldToPlay;
        public List<WorldCoordinate> alreadyIdledAt;
        public int idleCounter;
        public WorldCoordinate testIdlePos;
        public WorldCoordinate? lastIdleSpot;
        private int heldWiggle;
        private PathingAssist pathingAssist;
        private BehaviorType behaviorType;
        private bool jumping;
        private bool catchPoles;
        private int jumpDir;
        private int forceJump;
        private int catchDelay;
        private bool cutCorners;
        private PhysicalObject grabTarget;
        private readonly List<IntVector2> _cachedFloodFillList = new List<IntVector2>(50);
        private List<IntVector2> previousAttackPositions;
        private List<IntVector2> list = new List<IntVector2>(50);
        private WorldCoordinate attackPos;
        private WorldCoordinate testThrowPos;
        private int changeAttackPositionDelay;

        private bool IsFull => cat.playerState.foodInStomach >= cat.MaxFoodInStomach;

        private bool FunStuff
        {
            get
            {
                bool num;
                if (!(behaviorType == BehaviorType.BeingHeld))
                {
                    if (cat.bodyMode == Player.BodyModeIndex.Stand)
                    {
                        goto IL_00a2;
                    }
                    num = cat.bodyMode == Player.BodyModeIndex.Crawl;
                }
                else
                {
                    if (cat.grabbedBy.Count <= 0 || !(cat.grabbedBy[0].grabber is Player))
                    {
                        goto IL_00b1;
                    }
                    num = (cat.grabbedBy[0].grabber as Player).bodyMode == Player.BodyModeIndex.Stand;
                }
                if (num)
                {
                    goto IL_00a2;
                }
                goto IL_00b1;
            IL_00a2:
                return base.threatTracker.TotalTrackedThreats == 0;
            IL_00b1:
                return false;
            }
        }

        public MothPupAbstractAI abstractAI => creature.abstractAI as MothPupAbstractAI;

        public Player cat => creature.realizedCreature as Player;
        */
        public MothPupAI(AbstractCreature creature, World world) : base(creature, world)
        {
        }
        
        public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
        {
            return new PathCost(cost.resistance + base.threatTracker.ThreatOfTile(coord.destinationCoord, accountThreatCreatureAccessibility: true) * 100f, cost.legality);
        }

        public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
        {
            if (otherCreature.creatureTemplate.smallCreature)
            {
                return new Tracker.SimpleCreatureRepresentation(base.tracker, otherCreature, 0f, forgetWhenNotVisible: false);
            }
            return new Tracker.ElaborateCreatureRepresentation(base.tracker, otherCreature, 1f, 3);
        }
        /*
        public void AteFood(PhysicalObject food)
        {
            Food foodType = GetFoodType(food);
            float num = ((!(foodType != Food.NotCounted)) ? 0f : ((foodType.Index == -1) ? 0f : foodPreference[foodType.Index]));
            if (Mathf.Abs(num) > 0.4f)
            {
                foodReaction += (int)(num * 120f);
            }
            if (Mathf.Abs(num) > 0.85f && FunStuff)
            {
                cat.Stun((int)Mathf.Lerp(10f, 25f, Mathf.InverseLerp(0.85f, 1f, Mathf.Abs(num))));
            }
        }

        private Food GetFoodType(PhysicalObject food)
        {
            if (food is DangleFruit)
            {
                return Food.DangleFruit;
            }
            if (food is SwollenWaterNut)
            {
                return Food.WaterNut;
            }
            if (food is JellyFish)
            {
                return Food.JellyFish;
            }
            if (food is SlimeMold)
            {
                return Food.SlimeMold;
            }
            if (food is EggBugEgg)
            {
                return Food.EggBugEgg;
            }
            if (food is FireEgg)
            {
                return Food.FireEgg;
            }
            if (food is SeedCob || (food is SlimeMold && food.abstractPhysicalObject.type == MoreSlugcatsEnums.AbstractObjectType.Seed))
            {
                return Food.Popcorn;
            }
            if (food is GooieDuck)
            {
                return Food.GooieDuck;
            }
            if (food is LillyPuck)
            {
                return Food.LillyPuck;
            }
            if (food is GlowWeed)
            {
                return Food.GlowWeed;
            }
            if (food is DandelionPeach)
            {
                return Food.DandelionPeach;
            }
            if (food is OracleSwarmer)
            {
                return Food.Neuron;
            }
            if (food is Centipede)
            {
                if (!(food as Centipede).Small)
                {
                    return Food.Centipede;
                }
                return Food.SmallCentipede;
            }
            if (food is VultureGrub)
            {
                return Food.VultureGrub;
            }
            if (food is SmallNeedleWorm)
            {
                return Food.SmallNeedleWorm;
            }
            if (food is Hazer)
            {
                return Food.Hazer;
            }
            return Food.NotCounted;
        }

        private void SetupFoodPrefs()
        {
            UnityEngine.Random.State state = UnityEngine.Random.state;
            UnityEngine.Random.InitState(cat.abstractCreature.ID.RandomSeed);
            AbstractCreature.Personality personality = cat.abstractCreature.personality;
            foodPreference = new float[17];
            for (int i = 0; i < 17; i++)
            {
                float num = 0f;
                float num2 = 0f;
                switch (i)
                {
                    case 0:
                        num = personality.nervous;
                        num2 = personality.energy;
                        break;
                    case 1:
                        num = personality.sympathy;
                        num2 = personality.aggression;
                        break;
                    case 2:
                        num = personality.energy;
                        num2 = personality.nervous;
                        break;
                    case 3:
                        num = personality.energy;
                        num2 = personality.aggression;
                        break;
                    case 4:
                        num = personality.dominance;
                        num2 = personality.energy;
                        break;
                    case 5:
                        num = personality.aggression;
                        num2 = personality.sympathy;
                        break;
                    case 6:
                        num = personality.dominance;
                        num2 = personality.bravery;
                        break;
                    case 7:
                        num = personality.sympathy;
                        num2 = personality.bravery;
                        break;
                    case 8:
                        num = personality.aggression;
                        num2 = personality.nervous;
                        break;
                    case 9:
                        num = personality.nervous;
                        num2 = personality.energy;
                        break;
                    case 10:
                        num = personality.bravery;
                        num2 = personality.dominance;
                        break;
                    case 11:
                        num = personality.bravery;
                        num2 = personality.nervous;
                        break;
                    case 12:
                        num = personality.bravery;
                        num2 = personality.dominance;
                        break;
                    case 13:
                        num = personality.energy;
                        num2 = personality.aggression;
                        break;
                    case 14:
                        num = personality.dominance;
                        num2 = personality.bravery;
                        break;
                    case 15:
                        num = personality.aggression;
                        num2 = personality.sympathy;
                        break;
                    case 16:
                        num = personality.nervous;
                        num2 = personality.sympathy;
                        break;
                }
                num *= Custom.PushFromHalf(UnityEngine.Random.value, 2f);
                num2 *= Custom.PushFromHalf(UnityEngine.Random.value, 2f);
                foodPreference[i] = Mathf.Clamp(Mathf.Lerp(num - num2, Mathf.Lerp(-1f, 1f, Custom.PushFromHalf(UnityEngine.Random.value, 2f)), Custom.PushFromHalf(UnityEngine.Random.value, 2f)), -1f, 1f);
            }
            UnityEngine.Random.state = state;
        }

        private void Move()
        {
            Player.InputPackage input = default(Player.InputPackage);
            if (creature.controlled)
            {
                input.x = (cat.inputWithDiagonals.HasValue ? cat.inputWithDiagonals.Value.x : 0);
                input.y = (cat.inputWithDiagonals.HasValue ? cat.inputWithDiagonals.Value.y : 0);
                input.jmp = cat.inputWithDiagonals.HasValue && cat.inputWithDiagonals.Value.jmp;
                input.mp = cat.inputWithDiagonals.HasValue && cat.inputWithDiagonals.Value.mp;
                input.pckp = cat.inputWithDiagonals.HasValue && cat.inputWithDiagonals.Value.pckp;
                input.thrw = cat.inputWithDiagonals.HasValue && cat.inputWithDiagonals.Value.thrw;
                cat.input[0] = input;
                return;
            }
            if (behaviorType == BehaviorType.OnHead)
            {
                input.x = 0;
                cat.standing = true;
                cat.bodyMode = Player.BodyModeIndex.Default;
                grabTarget = null;
                catchPoles = false;
                cat.input[0] = input;
                return;
            }
            if (behaviorType == BehaviorType.BeingHeld)
            {
                input.x = (cat.grabbedBy[0].grabber as Player).input[0].x;
                input.y = (cat.grabbedBy[0].grabber as Player).input[0].y;
                if ((cat.grabbedBy[0].grabber as Player).bodyMode == Player.BodyModeIndex.Crawl && cat.standing)
                {
                    input.y = -1;
                }
                if ((cat.grabbedBy[0].grabber as Player).bodyMode == Player.BodyModeIndex.Stand && !cat.standing)
                {
                    input.y = 1;
                }
                input.jmp = (cat.grabbedBy[0].grabber as Player).input[0].jmp;
                input.pckp = false;
                grabTarget = null;
                catchPoles = false;
                if (heldWiggle > 0)
                {
                    int x = (int)Mathf.Sign(cat.mainBodyChunk.pos.x - cat.grabbedBy[0].grabber.mainBodyChunk.pos.x);
                    input.x = x;
                    input.jmp = UnityEngine.Random.value < 0.95f;
                    cat.slowMovementStun = 10;
                }
                cat.input[0] = input;
                return;
            }
            if (FunStuff && foodReaction > 0)
            {
                input.y = (int)Mathf.Sign(foodReaction);
                input.jmp = UnityEngine.Random.value < Mathf.Lerp(0.1f, 0.4f, (float)foodReaction / 120f);
            }
            MovementConnection movementConnection = (base.pathFinder as StandardPather).FollowPath(creature.pos, actuallyFollowingThisPath: true);
            if (pathingAssist != null)
            {
                pathingAssist.VisualizeConnection();
            }
            if (grabTarget == null && abstractAI.destination.room == abstractAI.parent.Room.index && (new Vector2(abstractAI.destination.x, abstractAI.destination.y) - new Vector2(abstractAI.parent.pos.x, abstractAI.parent.pos.y)).magnitude < 1.5f)
            {
                movementConnection = default(MovementConnection);
                if (UnityEngine.Random.value < Mathf.Lerp(0f, 0.01f, Mathf.InverseLerp(0.35f, 0f, cat.abstractCreature.personality.energy)) || (base.friendTracker.friend != null && base.friendTracker.friend.dead))
                {
                    cat.standing = false;
                }
                else if (UnityEngine.Random.value < Mathf.Lerp(0f, 0.01f, Mathf.InverseLerp(0.65f, 1f, cat.abstractCreature.personality.energy)))
                {
                    cat.standing = true;
                }
            }
            if ((HasEdible() & !IsFull) && (behaviorType == BehaviorType.Following || behaviorType == BehaviorType.Idle || (behaviorType == BehaviorType.Attacking && AttackingPrey())))
            {
                input.pckp = true;
            }
            else
            {
                if (jumping)
                {
                    input.y = (catchPoles ? 1 : 0);
                    input.x = jumpDir;
                    input.jmp = true;
                    if ((cat.bodyMode != Player.BodyModeIndex.Default || OnHorizontalBeam()) && forceJump == 0)
                    {
                        jumping = false;
                    }
                }
                else if (movementConnection != default(MovementConnection))
                {
                    catchPoles = false;
                    if (((movementConnection.type == MovementConnection.MovementType.ShortCut && cat.room.GetTile(movementConnection.startCoord.Tile).Terrain == Room.Tile.TerrainType.ShortcutEntrance && cat.room.shortcutData(movementConnection.startCoord.Tile).LeadingSomewhere) || (movementConnection.type == MovementConnection.MovementType.NPCTransportation && transportDelay <= 0)) && creature.pos == movementConnection.startCoord)
                    {
                        if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
                        {
                            cat.NPCTransportationDestination = movementConnection.destinationCoord;
                            transportDelay = 80;
                        }
                        cat.enteringShortCut = movementConnection.StartTile;
                    }
                    bool flag = false;
                    int num = 0;
                    bool flag2 = false;
                    bool flag3 = false;
                    bool flag4 = false;
                    bool flag5 = false;
                    bool flag6 = false;
                    bool flag7 = false;
                    bool flag8 = false;
                    WorldCoordinate startCoord = movementConnection.startCoord;
                    WorldCoordinate destinationCoord = movementConnection.destinationCoord;
                    List<MovementConnection> upcoming = GetUpcoming();
                    if (upcoming != null)
                    {
                        for (int i = 0; i < upcoming.Count; i++)
                        {
                            if (flag && !AnyClimb() && !TileClimbable(upcoming[i].destinationCoord) && Mathf.Abs(upcoming[i].destinationCoord.x - creature.pos.x) <= upcoming[i].destinationCoord.y - creature.pos.y + 1 && upcoming[i].destinationCoord.y > creature.pos.y && num == 0)
                            {
                                num = (int)Mathf.Sign(upcoming[i].destinationCoord.x - creature.pos.x);
                            }
                            if (!flag7 && ((!AnyClimb() && TileClimbable(upcoming[i].destinationCoord) && cat.room.GetTile(upcoming[i].destinationCoord).verticalBeam && upcoming[i].destinationCoord.y == upcoming[i].startCoord.y + 1 && upcoming[i].destinationCoord.x == upcoming[i].startCoord.x) || (!AnyClimb() && Tunnel(upcoming[i].destinationCoord) && upcoming[i].destinationCoord.y == upcoming[i].startCoord.y - 1) || cat.bodyMode == Player.BodyModeIndex.Default))
                            {
                                flag7 = true;
                                flag = false;
                            }
                            if (!flag && !flag7 && ((cat.animation == Player.AnimationIndex.LedgeGrab && i == 0) || (cutCorners && i < 2 && OnVerticalBeam() && upcoming[i].destinationCoord.x != creature.pos.x && VisualContact(cat.room.MiddleOfTile(upcoming[i].destinationCoord), 0f)) || (!AnyClimb() && !TileClimbable(upcoming[i].destinationCoord) && !Tunnel(upcoming[i].destinationCoord) && Mathf.Abs(upcoming[i].destinationCoord.x - creature.pos.x) <= upcoming[i].destinationCoord.y - creature.pos.y + 1 && upcoming[i].destinationCoord.y > creature.pos.y)))
                            {
                                flag = true;
                                flag4 = false;
                                num = ((upcoming[i].destinationCoord.x > creature.pos.x) ? 1 : (-1));
                                if (upcoming[i].destinationCoord.x == creature.pos.x)
                                {
                                    num = 0;
                                }
                                if (TileClimbable(upcoming[i].destinationCoord))
                                {
                                    flag2 = true;
                                }
                            }
                            if (!flag5 && cat.animation == Player.AnimationIndex.HangFromBeam)
                            {
                                flag5 = true;
                            }
                            if (!flag && !flag4 && upcoming[i].destinationCoord.y < upcoming[i].startCoord.y && Tunnel(upcoming[i].destinationCoord) && i == 1 && cat.bodyMode != Player.BodyModeIndex.CorridorClimb)
                            {
                                flag4 = true;
                            }
                        }
                    }
                    if (!flag && !flag7 && !AnyClimb() && movementConnection.startCoord.y < movementConnection.destinationCoord.y && movementConnection.startCoord.x == movementConnection.destinationCoord.x)
                    {
                        flag = true;
                        flag4 = false;
                        flag2 = true;
                    }
                    if (!flag8 && catchDelay == 0 && cat.room.GetTile(movementConnection.startCoord).horizontalBeam && cat.room.GetTile(movementConnection.destinationCoord).horizontalBeam && !OnHorizontalBeam())
                    {
                        flag8 = true;
                    }
                    if (!TileClimbable(movementConnection.destinationCoord) && OnVerticalBeam() && VisualContact(cat.room.MiddleOfTile(movementConnection.destinationCoord), 0f))
                    {
                        flag6 = true;
                    }
                    if (movementConnection.type == MovementConnection.MovementType.DropToClimb)
                    {
                        catchPoles = true;
                        catchDelay = 5;
                    }
                    if (flag)
                    {
                        Jump(num, flag2, ref input);
                    }
                    if (startCoord.x > destinationCoord.x)
                    {
                        input.x--;
                    }
                    if (startCoord.x < destinationCoord.x)
                    {
                        input.x++;
                    }
                    if (startCoord.y < destinationCoord.y && AnyClimb())
                    {
                        input.y++;
                    }
                    if ((startCoord.y > destinationCoord.y && startCoord.x == destinationCoord.x) || flag4)
                    {
                        input.y--;
                    }
                    if (flag6)
                    {
                        input.jmp = UnityEngine.Random.Range(0, 10) != 0;
                    }
                    if (flag3 && cat.bodyMode != Player.BodyModeIndex.Crawl)
                    {
                        input.y = ((UnityEngine.Random.Range(0, 2) != 0) ? (-1) : 0);
                    }
                    else if ((!flag3 && cat.bodyMode == Player.BodyModeIndex.Crawl) || flag8)
                    {
                        input.y = ((UnityEngine.Random.Range(0, 2) != 0) ? 1 : 0);
                    }
                    if ((OnAnyBeam() && movementConnection.type > MovementConnection.MovementType.SemiDiagonalReach && movementConnection.type < MovementConnection.MovementType.LizardTurn) || (OnHorizontalBeam() && movementConnection.startCoord.y > movementConnection.destinationCoord.y))
                    {
                        input.jmp = UnityEngine.Random.Range(0, 10) != 0;
                    }
                    if (movementConnection.destinationCoord.y < creature.pos.y && ((Tunnel(movementConnection.destinationCoord) && cat.bodyMode != Player.BodyModeIndex.CorridorClimb) || cat.room.GetTile(cat.room.GetTilePosition(cat.bodyChunks[1].pos) - new IntVector2(0, 1)).Terrain == Room.Tile.TerrainType.Floor))
                    {
                        input.y = ((UnityEngine.Random.Range(0, 10) != 0) ? (-1) : 0);
                    }
                    if (flag5 || (TileClimbable(movementConnection.startCoord) && !OnAnyBeam() && movementConnection.startCoord.y < movementConnection.destinationCoord.y) || (movementConnection.destinationCoord.y > movementConnection.startCoord.y && OnHorizontalBeam()))
                    {
                        input.y = ((UnityEngine.Random.Range(0, 2) != 0) ? 1 : 0);
                    }
                }
                else
                {
                    input.y = (((catchPoles && catchDelay == 0) || cat.room.PointSubmerged(cat.firstChunk.pos)) ? 1 : 0);
                }
                if (throwAtTarget != 0)
                {
                    if (cat.ThrowDirection == throwAtTarget)
                    {
                        input.thrw = true;
                        turnDelay = 5;
                        throwAtTarget = 0;
                    }
                    else
                    {
                        input.x = throwAtTarget;
                    }
                }
                if (turnDelay > 0)
                {
                    input.x = cat.ThrowDirection;
                }
                if (base.stuckTracker.Utility() > 0.1f)
                {
                    if (UnityEngine.Random.Range(0f, 1f) < UnityEngine.Random.Range(0f, 0.6f * base.stuckTracker.Utility()))
                    {
                        int num2 = UnityEngine.Random.Range(-1, 2);
                        input.x = ((num2 != 0) ? num2 : input.x);
                    }
                    if (UnityEngine.Random.Range(0f, 1f) < UnityEngine.Random.Range(0f, 0.6f * base.stuckTracker.Utility()))
                    {
                        int num3 = UnityEngine.Random.Range((!OnHorizontalBeam()) ? (-1) : 0, 2);
                        input.y = ((num3 != 0) ? num3 : input.y);
                    }
                    if (!OnAnyBeam() && UnityEngine.Random.Range(0f, 1f) < UnityEngine.Random.Range(0f, 0.6f * base.stuckTracker.Utility()))
                    {
                        input.jmp = UnityEngine.Random.Range(0, 2) == 0;
                    }
                }
            }
            cat.input[0] = input;
            if (pathingAssist != null)
            {
                pathingAssist.VisualizeInput(input);
            }
        }

        private bool AttackingPrey()
        {
            if (base.preyTracker.MostAttractivePrey != null && base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature != null)
            {
                return TheoreticallyEatMeat(base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature, excludeCentipedes: true);
            }
            return false;
        }

        private bool AttackingThreat()
        {
            for (int i = 0; i < base.relationshipTracker.relationships.Count; i++)
            {
                SlugNPCTrackState slugNPCTrackState = base.relationshipTracker.relationships[i].state as SlugNPCTrackState;
                Tracker.CreatureRepresentation trackerRep = base.relationshipTracker.relationships[i].trackerRep;
                if (base.threatTracker.mostThreateningCreature == trackerRep && ((float)slugNPCTrackState.annoyingThreat > Mathf.Lerp(600f, 6000f, 0.25f * creature.personality.aggression + 0.75f * (1f - creature.personality.sympathy)) || slugNPCTrackState.holdingAFriend || slugNPCTrackState.hurtAFriend) && trackerRep.representedCreature != null && trackerRep.representedCreature.realizedCreature != null && (HasLethal(trackerRep.representedCreature.realizedCreature, actuallyLethal: true) || (NearestLethalWeapon(trackerRep.representedCreature.realizedCreature) != null && LethalWeaponScore(NearestLethalWeapon(trackerRep.representedCreature.realizedCreature), trackerRep.representedCreature.realizedCreature) >= 1f)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool NeuronsLegal()
        {
            return cat.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.SSSwarmers) != null;
        }

        private float LethalWeaponScore(PhysicalObject obj, Creature target)
        {
            if (obj is PuffBall && target is InsectoidCreature)
            {
                return 5f;
            }
            if (obj is Spear)
            {
                if ((obj as Spear).stuckInWall.HasValue)
                {
                    return 0f;
                }
                if ((obj.abstractPhysicalObject as AbstractSpear).electric)
                {
                    return 2f;
                }
                if ((obj.abstractPhysicalObject as AbstractSpear).explosive)
                {
                    if (!(obj as ExplosiveSpear).Ignited)
                    {
                        return 0f;
                    }
                    return 3f;
                }
                return 1f;
            }
            if (obj is Rock)
            {
                if (!WantsToEatThis(target))
                {
                    return 0.3f;
                }
                return 10f;
            }
            if (obj is ScavengerBomb || obj is JokeRifle)
            {
                return 2f;
            }
            if (obj is SingularityBomb)
            {
                return 0.1f;
            }
            if (obj is FlareBomb && (target is Spider || target.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.BigSpider))
            {
                return 4f;
            }
            if (obj is LillyPuck)
            {
                return 0.75f;
            }
            if (obj is JellyFish)
            {
                return 0.5f;
            }
            return 0f;
        }

        private bool HasLethal(Creature creature)
        {
            return HasLethal(creature, actuallyLethal: false);
        }

        private bool HasLethal(Creature creature, bool actuallyLethal)
        {
            for (int i = 0; i < cat.grasps.Length; i++)
            {
                if (cat.grasps[i] != null && (actuallyLethal ? (LethalWeaponScore(cat.grasps[i].grabbed, creature) >= 1f) : (LethalWeaponScore(cat.grasps[i].grabbed, creature) > 0f)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool GoodAttackPos(Tracker.CreatureRepresentation target, int chunk)
        {
            BodyChunk bodyChunk = target.representedCreature.realizedCreature.bodyChunks[chunk];
            if (Custom.DirVec(cat.mainBodyChunk.pos, bodyChunk.pos).y > 0.05f || Custom.DirVec(cat.mainBodyChunk.pos, bodyChunk.pos).y < -0.2f || !VisualContact(bodyChunk))
            {
                return false;
            }
            for (int i = 0; i < base.tracker.CreaturesCount; i++)
            {
                if (base.tracker.GetRep(i).representedCreature.realizedCreature != null && CareAboutHitting(base.tracker.GetRep(i).representedCreature.realizedCreature, target.representedCreature.realizedCreature) && base.tracker.GetRep(i) != target && base.tracker.GetRep(i).representedCreature.realizedCreature.room == cat.room && HasLethal(base.tracker.GetRep(i).representedCreature.realizedCreature) && Custom.DistLess(base.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos, Custom.ClosestPointOnLineSegment(cat.mainBodyChunk.pos, bodyChunk.pos, base.tracker.GetRep(i).representedCreature.realizedCreature.mainBodyChunk.pos), 40f))
                {
                    return false;
                }
            }
            return true;
        }

        private WorldCoordinate AttackUpdate(ref WorldCoordinate coord, Tracker.CreatureRepresentation target)
        {
            if (target != null && target.representedCreature != null && target.representedCreature.realizedCreature != null)
            {
                if (WantsToEatThis(target.representedCreature.realizedCreature) && (base.pathFinder.CoordinateReachable(target.BestGuessForPosition()) || NearestLethalWeapon(target.representedCreature.realizedCreature) == null))
                {
                    coord = target.BestGuessForPosition();
                }
                else if (!HasLethal(target.representedCreature.realizedCreature))
                {
                    grabTarget = NearestLethalWeapon(target.representedCreature.realizedCreature);
                    coord = ((grabTarget != null) ? grabTarget.abstractPhysicalObject.pos : coord);
                }
                else
                {
                    FindAttackPosition(target);
                    coord = attackPos;
                    int num = UnityEngine.Random.Range(0, target.representedCreature.realizedCreature.bodyChunks.Length - 1);
                    if (GoodAttackPos(target, num))
                    {
                        BodyChunk bodyChunk = target.representedCreature.realizedCreature.bodyChunks[num];
                        throwAtTarget = (int)Mathf.Sign(bodyChunk.pos.x - cat.firstChunk.pos.x);
                    }
                }
            }
            return coord;
        }

        private bool CareAboutHitting(Creature crit, Creature intendedTarget)
        {
            if (!(crit is SmallNeedleWorm) && (crit.abstractCreature.creatureTemplate.smallCreature || !crit.canBeHitByWeapons))
            {
                if (AttackingPrey())
                {
                    return TheoreticallyEatMeat(crit, excludeCentipedes: true);
                }
                return false;
            }
            return true;
        }

        private PhysicalObject NearestLethalWeapon(Creature target)
        {
            float num = 0f;
            PhysicalObject result = null;
            for (int i = 0; i < base.itemTracker.ItemCount; i++)
            {
                ItemTracker.ItemRepresentation rep = base.itemTracker.GetRep(i);
                if (rep.representedItem.realizedObject != null && !HoldingThis(rep.representedItem.realizedObject) && rep.representedItem.realizedObject.grabbedBy.Count == 0 && base.pathFinder.CoordinateReachable(rep.representedItem.pos))
                {
                    float magnitude = (rep.representedItem.realizedObject.firstChunk.pos - cat.firstChunk.pos).magnitude;
                    float num2 = LethalWeaponScore(rep.representedItem.realizedObject, target) * Mathf.Clamp(1f - magnitude / 2000f, 0f, 1f);
                    if (num2 > num)
                    {
                        num = num2;
                        result = rep.representedItem.realizedObject;
                    }
                }
            }
            return result;
        }

        private void Communicate(Player player)
        {
            Player.InputPackage[] input = player.input;
            if (!input[0].jmp || input[1].jmp || !(player.bodyMode != Player.BodyModeIndex.Default))
            {
                return;
            }
            if (input[0].y == -1 && input[0].x == 0)
            {
                abstractAI.toldToStay = creature.pos;
                return;
            }
            if (input[0].y == 1 && input[0].x == 0 && player.bodyMode != Player.BodyModeIndex.ClimbingOnBeam)
            {
                abstractAI.toldToStay = null;
                toldToPlay = Mathf.Min(Mathf.Max(toldToPlay - 750, -4000), 0);
                return;
            }
            for (int i = 1; i < 9; i++)
            {
                if (input[i].jmp)
                {
                    toldToPlay = Mathf.Max(Mathf.Min(toldToPlay + 750, 4000), 0);
                    break;
                }
            }
        }

        private void DefineFollowCloseness()
        {
            if (base.friendTracker.friend.room != cat.room)
            {
                followCloseness = 1f;
            }
            else
            {
                followCloseness = Mathf.Clamp01(1f - Mathf.Clamp((float)toldToPlay / 2000f + (float)Mathf.Clamp(timeInRoom, 0, 6000) / 6000f - base.threatTracker.ThreatOfArea(creature.pos, accountThreatCreatureAccessibility: false) * 3f, -1f, 1f));
            }
        }

        private void PassingGrab()
        {
            if (behaviorType == BehaviorType.BeingHeld || behaviorType == BehaviorType.OnHead || behaviorType == BehaviorType.GrabItem)
            {
                return;
            }
            if (behaviorType == BehaviorType.Attacking)
            {
                if (!AttackingPrey() || (cat.grasps[0] != null && WantsToEatThis(cat.grasps[0].grabbed)))
                {
                    return;
                }
                if (WantsToEatThis(base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature) && CanGrabItem(base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature) && base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature.grabbedBy.Count == 0)
                {
                    cat.NPCForceGrab(base.preyTracker.MostAttractivePrey.representedCreature.realizedCreature);
                }
                else
                {
                    if (base.preyTracker.TotalTrackedPrey <= 0)
                    {
                        return;
                    }
                    for (int i = 0; i < base.preyTracker.TotalTrackedPrey; i++)
                    {
                        if (base.preyTracker.GetTrackedPrey(i).representedCreature.realizedCreature != null)
                        {
                            PhysicalObject realizedCreature = base.preyTracker.GetTrackedPrey(i).representedCreature.realizedCreature;
                            if (WantsToEatThis(realizedCreature) && CanGrabItem(realizedCreature) && realizedCreature.grabbedBy.Count == 0)
                            {
                                cat.NPCForceGrab(realizedCreature);
                                break;
                            }
                        }
                    }
                }
            }
            else if (behaviorType == BehaviorType.Idle || behaviorType == BehaviorType.Following)
            {
                if (base.itemTracker.ItemCount <= 0)
                {
                    return;
                }
                for (int j = 0; j < base.itemTracker.ItemCount; j++)
                {
                    PhysicalObject realizedObject = base.itemTracker.GetRep(j).representedItem.realizedObject;
                    if (!CanGrabItem(realizedObject) || realizedObject.grabbedBy.Count != 0)
                    {
                        continue;
                    }
                    if (WantsToEatThis(realizedObject) && (cat.grasps[0] == null || !WantsToEatThis(cat.grasps[0].grabbed)) && (!(realizedObject is OracleSwarmer) || NeuronsLegal()))
                    {
                        cat.NPCForceGrab(realizedObject);
                        break;
                    }
                    if (cat.grasps[0] != null)
                    {
                        break;
                    }
                    if (!(UnityEngine.Random.value < Mathf.Lerp(0f, 0.9f, Mathf.InverseLerp(0.4f, 1f, cat.abstractCreature.personality.bravery))))
                    {
                        if ((realizedObject is Spear || realizedObject is ScavengerBomb || realizedObject is SingularityBomb) && UnityEngine.Random.value < Mathf.Lerp(0f, 0.05f, Mathf.InverseLerp(0.4f, 1f, cat.abstractCreature.personality.aggression)))
                        {
                            cat.NPCForceGrab(realizedObject);
                            break;
                        }
                        if (UnityEngine.Random.value < Mathf.Lerp(0f, 0.05f * ((realizedObject is DataPearl || realizedObject is OverseerCarcass || realizedObject is NSHSwarmer || realizedObject is VultureMask) ? 3f : 1f), Mathf.InverseLerp(0.4f, 1f, cat.abstractCreature.personality.energy)))
                        {
                            cat.NPCForceGrab(realizedObject);
                            break;
                        }
                        if ((realizedObject is FirecrackerPlant || realizedObject is FlyLure || realizedObject is PuffBall || realizedObject is FlareBomb || realizedObject is GooieDuck || realizedObject is BubbleGrass || realizedObject is NeedleEgg) && UnityEngine.Random.value < Mathf.Lerp(0f, 0.05f, Mathf.InverseLerp(0.3f, 1f, cat.abstractCreature.personality.dominance)))
                        {
                            cat.NPCForceGrab(realizedObject);
                            break;
                        }
                        if ((realizedObject is Rock || realizedObject is PuffBall || realizedObject is FlareBomb || realizedObject is FirecrackerPlant || realizedObject is LillyPuck || realizedObject is JellyFish) && UnityEngine.Random.value < Mathf.Lerp(0f, 0.05f, Mathf.InverseLerp(0.3f, 1f, cat.abstractCreature.personality.nervous)))
                        {
                            cat.NPCForceGrab(realizedObject);
                            break;
                        }
                    }
                }
                if (base.tracker.CreaturesCount <= 0)
                {
                    return;
                }
                for (int k = 0; k < base.tracker.CreaturesCount; k++)
                {
                    Creature realizedCreature2 = base.tracker.GetRep(k).representedCreature.realizedCreature;
                    if (!CanGrabItem(realizedCreature2) || realizedCreature2.grabbedBy.Count != 0)
                    {
                        continue;
                    }
                    if (WantsToEatThis(realizedCreature2) && (cat.grasps[0] == null || !WantsToEatThis(cat.grasps[0].grabbed)))
                    {
                        if (realizedCreature2 == null || realizedCreature2.dead || !(creature.personality.sympathy > 0.8f))
                        {
                            cat.NPCForceGrab(realizedCreature2);
                            break;
                        }
                        continue;
                    }
                    if (realizedCreature2 == base.friendTracker.friend && realizedCreature2.dead && UnityEngine.Random.value < 0.01f)
                    {
                        cat.NPCForceGrab(realizedCreature2);
                        break;
                    }
                    if (cat.grasps[0] != null)
                    {
                        break;
                    }
                    if (!(UnityEngine.Random.value < Mathf.Lerp(0f, 0.9f, Mathf.InverseLerp(0.4f, 1f, cat.abstractCreature.personality.bravery))))
                    {
                        if (UnityEngine.Random.value < Mathf.Lerp(0f, 0.05f, Mathf.InverseLerp(0.4f, 1f, cat.abstractCreature.personality.energy)))
                        {
                            cat.NPCForceGrab(realizedCreature2);
                            break;
                        }
                        if ((realizedCreature2 is Hazer || realizedCreature2 is VultureGrub || realizedCreature2 is LanternMouse || realizedCreature2 is EggBug || realizedCreature2 is JetFish || realizedCreature2 is Yeek || realizedCreature2 is TubeWorm || realizedCreature2 is Snail) && !realizedCreature2.dead && UnityEngine.Random.value < Mathf.Lerp(0f, 0.05f, Mathf.InverseLerp(0.4f, 1f, cat.abstractCreature.personality.sympathy)))
                        {
                            cat.NPCForceGrab(realizedCreature2);
                            break;
                        }
                        if ((realizedCreature2 is VultureGrub || realizedCreature2 is Hazer || realizedCreature2 is LanternMouse || realizedCreature2 is Snail || realizedCreature2 is Cicada) && !realizedCreature2.dead && UnityEngine.Random.value < Mathf.Lerp(0f, 0.05f, Mathf.InverseLerp(0.3f, 1f, cat.abstractCreature.personality.dominance)))
                        {
                            cat.NPCForceGrab(realizedCreature2);
                            break;
                        }
                        if (realizedCreature2 is Hazer && !realizedCreature2.dead && UnityEngine.Random.value < Mathf.Lerp(0f, 0.05f, Mathf.InverseLerp(0.3f, 1f, cat.abstractCreature.personality.nervous)))
                        {
                            cat.NPCForceGrab(realizedCreature2);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (!(behaviorType == BehaviorType.Fleeing) || base.threatTracker.mostThreateningCreature == null || (cat.grasps[0] != null && HasLethal(base.threatTracker.mostThreateningCreature.representedCreature.realizedCreature)) || base.itemTracker.ItemCount <= 0)
                {
                    return;
                }
                for (int l = 0; l < base.itemTracker.ItemCount; l++)
                {
                    PhysicalObject realizedObject2 = base.itemTracker.GetRep(l).representedItem.realizedObject;
                    if (CanGrabItem(realizedObject2) && realizedObject2.grabbedBy.Count == 0 && LethalWeaponScore(realizedObject2, base.threatTracker.mostThreateningCreature.representedCreature.realizedCreature) > 0f && !(realizedObject2 is SingularityBomb))
                    {
                        cat.NPCForceGrab(realizedObject2);
                        break;
                    }
                }
            }
        }

        public override void Update()
        {
            if (cat.controller != null)
            {
                return;
            }
            base.Update();
            if (base.friendTracker.friend != null && base.friendTracker.friend is Player && VisualContact(base.friendTracker.friend.firstChunk) && cat.room == base.friendTracker.friend.room)
            {
                Communicate(base.friendTracker.friend as Player);
            }
            if (base.friendTracker.friend != null && base.friendTracker.friend is Player)
            {
                abstractAI.isTamed = true;
            }
            if (toldToPlay > 0)
            {
                toldToPlay = Mathf.Max(toldToPlay - 1, -4000);
            }
            else
            {
                toldToPlay = Mathf.Min(toldToPlay + 1, 4000);
            }
            if (transportDelay > 0)
            {
                transportDelay--;
            }
            if (base.friendTracker.friend != null)
            {
                DefineFollowCloseness();
            }
            if (pathingAssist != null)
            {
                pathingAssist.Update();
            }
            if (combatDebug != null)
            {
                combatDebug.Update();
            }
            if (personalityDebug != null)
            {
                personalityDebug.Update();
            }
            if (HoldingThis(grabTarget))
            {
                grabTarget = null;
            }
            if (playWithItem && playWithItem && cat.graphicsModule != null && cat.grasps[0] != null)
            {
                if (UnityEngine.Random.value < Mathf.Lerp(0.025f, 0.075f, Mathf.InverseLerp(0.85f, 1f, cat.abstractCreature.personality.energy)))
                {
                    playPos = Custom.RNV() * UnityEngine.Random.Range(0f, 5f) - new Vector2(0f, 7f);
                }
                (cat.graphicsModule as PlayerGraphics).hands[0].mode = Limb.Mode.HuntRelativePosition;
                (cat.graphicsModule as PlayerGraphics).hands[0].relativeHuntPos = playPos;
            }
            if (nap)
            {
                cat.standing = false;
                if (cat.graphicsModule != null)
                {
                    (cat.graphicsModule as PlayerGraphics).blink = 5;
                }
            }
            if (behaviorType == BehaviorType.OnHead || behaviorType == BehaviorType.BeingHeld)
            {
                cat.ReleaseGrasp(0);
                abstractAI.toldToStay = null;
                base.friendTracker.giftOfferedToMe = null;
                toldToPlay = Mathf.Min(toldToPlay, -2000);
            }
            foodReaction = Mathf.Clamp(foodReaction, -120, 120);
            if (foodReaction > 0)
            {
                foodReaction--;
                if (cat.graphicsModule != null && UnityEngine.Random.value < Mathf.Lerp(0.075f, 0.1f, Mathf.InverseLerp(0f, 120f, foodReaction)))
                {
                    (cat.graphicsModule as PlayerGraphics).blink = 3;
                }
            }
            else if (foodReaction < 0)
            {
                foodReaction++;
                if (FunStuff)
                {
                    cat.slowMovementStun = 5;
                }
                if (cat.graphicsModule != null)
                {
                    (cat.graphicsModule as PlayerGraphics).blink = 3;
                    (cat.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * ((float)foodReaction / -80f);
                }
            }
            if (base.friendTracker.giftOfferedToMe != null && base.friendTracker.giftOfferedToMe.item != null)
            {
                cat.ReleaseGrasp(0);
                grabTarget = base.friendTracker.giftOfferedToMe.item;
            }
            DecideBehavior();
            if (behaviorType == BehaviorType.BeingHeld && FunStuff && (cat.grabbedBy[0].grabber as Player).input[0].x == 0)
            {
                heldWiggle = Mathf.Max(heldWiggle - 1, 0);
                if (heldWiggle == 0 && creature.personality.energy > 0.6f && UnityEngine.Random.value < Custom.LerpMap(creature.personality.energy, 0.6f, 1f, 0.0025f, 0.01f))
                {
                    heldWiggle = Mathf.RoundToInt(UnityEngine.Random.Range(100, 300));
                }
            }
            else
            {
                heldWiggle = 0;
            }
            if (behaviorType != BehaviorType.Idle)
            {
                lastIdleSpot = null;
                idleCounter = 0;
            }
            if (grabTarget != null && CanGrabItem(grabTarget))
            {
                cat.NPCForceGrab(grabTarget);
            }
            PassingGrab();
            if (behaviorType == BehaviorType.BeingHeld)
            {
                creature.abstractAI.SetDestination(cat.abstractCreature.pos);
                forceJump = 0;
                catchDelay = 0;
                turnDelay = 0;
            }
            else
            {
                WorldCoordinate coord = creature.abstractAI.parent.pos;
                if (behaviorType == BehaviorType.Fleeing)
                {
                    coord = base.threatTracker.FleeTo(cat.abstractCreature.pos, 10, 30, considerLeavingRoom: true);
                    for (int i = 0; i < base.tracker.CreaturesCount; i++)
                    {
                        if (base.threatTracker.GetThreatCreature(base.tracker.GetRep(i).representedCreature) == null || base.tracker.GetRep(i).representedCreature.realizedCreature == null)
                        {
                            continue;
                        }
                        Creature realizedCreature = base.tracker.GetRep(i).representedCreature.realizedCreature;
                        if (realizedCreature.bodyChunks.Length != 0)
                        {
                            int num = UnityEngine.Random.Range(0, realizedCreature.bodyChunks.Length - 1);
                            if (HasLethal(realizedCreature) && GoodAttackPos(base.tracker.GetRep(i), num) && UnityEngine.Random.value < Mathf.Lerp(0.035f, 0.1f, Mathf.InverseLerp(0f, 1f, creature.personality.aggression)))
                            {
                                BodyChunk bodyChunk = realizedCreature.bodyChunks[num];
                                throwAtTarget = (int)Mathf.Sign(bodyChunk.pos.x - cat.firstChunk.pos.x);
                            }
                        }
                    }
                }
                else if (behaviorType == BehaviorType.Following)
                {
                    coord = base.friendTracker.friendDest;
                }
                else if (behaviorType == BehaviorType.GrabItem)
                {
                    if (grabTarget != null)
                    {
                        coord = grabTarget.abstractPhysicalObject.pos;
                    }
                }
                else if (behaviorType == BehaviorType.Attacking)
                {
                    AttackUpdate(ref coord, AttackingThreat() ? base.threatTracker.mostThreateningCreature : base.preyTracker.MostAttractivePrey);
                }
                else if (behaviorType == BehaviorType.Idle)
                {
                    if (abstractAI.toldToStay.HasValue)
                    {
                        coord = abstractAI.toldToStay.Value;
                    }
                    else
                    {
                        WorldCoordinate? worldCoordinate = IdleBehavior();
                        if (worldCoordinate.HasValue)
                        {
                            lastIdleSpot = worldCoordinate.Value;
                        }
                        if (lastIdleSpot.HasValue)
                        {
                            coord = lastIdleSpot.Value;
                        }
                    }
                }
                creature.abstractAI.SetDestination(coord);
                forceJump = Mathf.Max(forceJump - 1, 0);
                catchDelay = Mathf.Max(catchDelay - 1, 0);
                turnDelay = Mathf.Max(turnDelay - 1, 0);
            }
            Move();
            if (creature.controlled)
            {
                return;
            }
            if (cat.input[0].x == 0 && cat.input[0].y == 0 && !cat.input[0].jmp && !cat.input[0].pckp && cat.grasps[0] != null && !cat.HeavyCarry(cat.grasps[0].grabbed) && !(cat.grasps[0].grabbed is Spear))
            {
                if (UnityEngine.Random.value < Mathf.Lerp(0f, 0.01f, Mathf.InverseLerp(0.85f, 1f, cat.abstractCreature.personality.energy)))
                {
                    playWithItem = true;
                }
            }
            else
            {
                playWithItem = false;
            }
            if (cat.input[0].x == 0 && cat.input[0].y == 0 && !cat.input[0].jmp && !cat.input[0].pckp && FunStuff)
            {
                if (UnityEngine.Random.value < Mathf.Lerp(0f, 0.01f, Mathf.InverseLerp(0.15f, 0f, cat.abstractCreature.personality.energy)))
                {
                    nap = true;
                }
            }
            else
            {
                nap = false;
            }
        }

        private WorldCoordinate? IdleBehavior()
        {
            WorldCoordinate? result = null;
            WorldCoordinate worldCoordinate = cat.room.GetWorldCoordinate(cat.mainBodyChunk.pos + Custom.RNV() * UnityEngine.Random.value * 800f);
            if (UnityEngine.Random.value < 0.01f)
            {
                worldCoordinate.x = UnityEngine.Random.Range(0, cat.room.TileWidth);
                worldCoordinate.y = UnityEngine.Random.Range(0, cat.room.TileHeight);
            }
            if (IdleScore(worldCoordinate) > IdleScore(testIdlePos))
            {
                testIdlePos = worldCoordinate;
            }
            float num = 200 + idleCounter;
            if (testIdlePos != base.pathFinder.GetDestination && IdleScore(testIdlePos) > IdleScore(base.pathFinder.GetDestination) + num)
            {
                result = testIdlePos;
            }
            idleCounter--;
            if (cat.mainBodyChunk.vel.magnitude < 1f)
            {
                idleCounter -= 5;
            }
            if (idleCounter < 1)
            {
                alreadyIdledAt.Insert(0, testIdlePos);
                if (alreadyIdledAt.Count > 10)
                {
                    alreadyIdledAt.RemoveAt(alreadyIdledAt.Count - 1);
                }
                idleCounter = Mathf.RoundToInt((float)UnityEngine.Random.Range(200, 400) * Mathf.Lerp(6f, 1f, Mathf.Pow(creature.personality.energy, 3f)));
                result = testIdlePos;
            }
            worldCoordinate = base.pathFinder.GetDestination + Custom.fourDirections[UnityEngine.Random.Range(0, 4)];
            if (IdleScore(worldCoordinate) > IdleScore(base.pathFinder.GetDestination))
            {
                result = worldCoordinate;
            }
            return result;
        }

        private float IdleScore(WorldCoordinate tstPs)
        {
            if (!base.pathFinder.CoordinateViable(tstPs))
            {
                return float.MinValue;
            }
            float num = 0f;
            for (int i = 0; i < alreadyIdledAt.Count; i++)
            {
                if (tstPs.room == alreadyIdledAt[i].room)
                {
                    num -= Mathf.InverseLerp(15f, 3f, tstPs.Tile.FloatDist(alreadyIdledAt[i].Tile)) * Custom.LerpMap(i, 0f, alreadyIdledAt.Count - 1, 100f, 40f) * (0.5f + creature.personality.bravery);
                }
            }
            if ((double)creature.personality.bravery > 0.3)
            {
                num += Mathf.Clamp(cat.room.aimap.getAItile(tstPs).visibility, 0f, Custom.LerpMap(creature.personality.bravery * creature.personality.energy, 0.3f, 1f, 50f, 150f));
            }
            else if ((double)creature.personality.bravery < 0.15)
            {
                num -= Mathf.Clamp(cat.room.aimap.getAItile(tstPs).visibility, 0f, Custom.LerpMap(creature.personality.bravery, 0f, 0.15f, 50f, 300f));
            }
            for (int j = -1; j < 2; j++)
            {
                if (cat.room.aimap.getAItile(tstPs + new IntVector2(j, 0)).acc != AItile.Accessibility.Floor)
                {
                    num -= 10f;
                }
                if (cat.room.aimap.getAItile(tstPs + new IntVector2(j, 0)).narrowSpace)
                {
                    num -= 10f;
                }
            }
            if (!cat.room.GetTile(tstPs + new IntVector2(0, -1)).Solid)
            {
                num -= 10f;
            }
            if (cat.room.GetTile(tstPs).AnyWater)
            {
                num -= 500f;
            }
            int num2 = int.MaxValue;
            for (int k = 0; k < cat.room.abstractRoom.NodesRelevantToCreature(creature.creatureTemplate); k++)
            {
                int num3 = cat.room.aimap.ExitDistanceForCreature(tstPs.Tile, k, creature.creatureTemplate);
                if (num3 > 0 && num3 < num2)
                {
                    num2 = num3;
                }
            }
            return num + (float)Math.Min(num2, 100);
        }

        public bool WantsToEatThis(PhysicalObject obj)
        {
            if ((obj is IPlayerEdible && (obj as IPlayerEdible).Edible) || (obj is Creature && TheoreticallyEatMeat(obj as Creature, excludeCentipedes: false) && (obj as Creature).dead))
            {
                return !IsFull;
            }
            return false;
        }

        AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
        {
            CreatureTemplate.Relationship.Type type = relationship.type;
            if (type == CreatureTemplate.Relationship.Type.Eats || type == CreatureTemplate.Relationship.Type.Attacks)
            {
                return base.preyTracker;
            }
            if (type == CreatureTemplate.Relationship.Type.Afraid)
            {
                return base.threatTracker;
            }
            return base.tracker;
        }

        RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
        {
            return new SlugNPCTrackState();
        }

        CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
        {
            if ((creature.state as PlayerNPCState).slugcatCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f);
            }
            if (base.threatTracker.GetThreatCreature(dRelation.trackerRep.representedCreature) != null && dRelation.trackerRep.representedCreature.creatureTemplate.type != CreatureTemplate.Type.RedLizard && dRelation.trackerRep.representedCreature.creatureTemplate.type != CreatureTemplate.Type.RedCentipede && dRelation.trackerRep.representedCreature.creatureTemplate.type != CreatureTemplate.Type.DaddyLongLegs && dRelation.trackerRep.representedCreature.creatureTemplate.type != CreatureTemplate.Type.BrotherLongLegs && dRelation.trackerRep.representedCreature.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs && dRelation.trackerRep.representedCreature.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
            {
                (dRelation.state as SlugNPCTrackState).annoyingThreat++;
            }
            Creature realizedCreature = dRelation.trackerRep.representedCreature.realizedCreature;
            if (realizedCreature == null)
            {
                return StaticRelationship(dRelation.trackerRep.representedCreature);
            }
            if (dRelation.trackerRep.VisualContact)
            {
                bool holdingAFriend = false;
                if (!realizedCreature.abstractCreature.creatureTemplate.smallCreature && !(realizedCreature is Player) && realizedCreature.grasps != null && realizedCreature.grasps.Length != 0)
                {
                    for (int i = 0; i < realizedCreature.grasps.Length; i++)
                    {
                        if (realizedCreature.grasps[i] != null && realizedCreature.grasps[i].grabbed is Player)
                        {
                            holdingAFriend = true;
                        }
                    }
                }
                (dRelation.state as SlugNPCTrackState).holdingAFriend = holdingAFriend;
            }
            if (realizedCreature.abstractCreature.abstractAI != null && realizedCreature.abstractCreature.abstractAI.RealAI != null && realizedCreature.abstractCreature.abstractAI.RealAI.friendTracker != null && realizedCreature.abstractCreature.abstractAI.RealAI.friendTracker.friend != null && realizedCreature.abstractCreature.abstractAI.RealAI.friendTracker.friend == base.friendTracker.friend)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.5f);
            }
            if (WantsToEatThis(realizedCreature))
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, dRelation.state.alive ? 0.65f : 1f);
            }
            if (realizedCreature.dead && !(realizedCreature is Player))
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0f);
            }
            return StaticRelationship(dRelation.trackerRep.representedCreature);
        }

        private bool TheoreticallyEatMeat(Creature crit, bool excludeCentipedes)
        {
            if (cat.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint || cat.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear)
            {
                return false;
            }
            if (crit is IPlayerEdible)
            {
                return true;
            }
            if (crit.dead && crit.State.meatLeft > 0)
            {
                if (((!(crit.Template.type == CreatureTemplate.Type.Centipede) && !(crit.Template.type == CreatureTemplate.Type.Centiwing) && !(crit.Template.type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti) && !(crit.Template.type == CreatureTemplate.Type.RedCentipede)) || excludeCentipedes) && !(cat.SlugCatClass == SlugcatStats.Name.Red) && !(cat.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Artificer) && !(cat.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel))
                {
                    return cat.SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand;
                }
                return true;
            }
            return false;
        }

        private List<MovementConnection> GetUpcoming()
        {
            MovementConnection movementConnection = (base.pathFinder as StandardPather).FollowPath(creature.pos, actuallyFollowingThisPath: false);
            if (movementConnection != default(MovementConnection))
            {
                List<MovementConnection> list = new List<MovementConnection>();
                for (int i = 0; i < 5; i++)
                {
                    if (!(movementConnection != default(MovementConnection)))
                    {
                        break;
                    }
                    list.Add(movementConnection);
                    movementConnection = (base.pathFinder as StandardPather).FollowPath(movementConnection.destinationCoord, actuallyFollowingThisPath: false);
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (!(movementConnection != default(MovementConnection)))
                        {
                            break;
                        }
                        if (list[j].destinationCoord == movementConnection.destinationCoord)
                        {
                            movementConnection = default(MovementConnection);
                        }
                    }
                    if (movementConnection == default(MovementConnection))
                    {
                        break;
                    }
                }
                return list;
            }
            return null;
        }

        private void Jump(int direction, bool catchPoles, ref Player.InputPackage input)
        {
            jumping = true;
            jumpDir = direction;
            input.x = direction;
            this.catchPoles = catchPoles;
            forceJump = 10;
        }

        private bool OnVerticalBeam()
        {
            return cat.bodyMode == Player.BodyModeIndex.ClimbingOnBeam;
        }

        private bool OnHorizontalBeam()
        {
            if (!(cat.animation == Player.AnimationIndex.HangFromBeam))
            {
                return cat.animation == Player.AnimationIndex.StandOnBeam;
            }
            return true;
        }

        private bool OnAnyBeam()
        {
            if (!OnVerticalBeam())
            {
                return OnHorizontalBeam();
            }
            return true;
        }

        private bool CorridorClimbing()
        {
            return cat.bodyMode == Player.BodyModeIndex.CorridorClimb;
        }

        private bool AnyClimb()
        {
            if (!CorridorClimbing())
            {
                return OnAnyBeam();
            }
            return true;
        }

        private bool TileClimbable(WorldCoordinate coordinate)
        {
            if (cat.room.GetTile(coordinate).AnyBeam)
            {
                return !Tunnel(coordinate);
            }
            return false;
        }

        private bool Tunnel(WorldCoordinate coordinate)
        {
            return cat.room.aimap.getAItile(coordinate).narrowSpace;
        }

        public bool TrackItem(AbstractPhysicalObject obj)
        {
            if (obj.realizedObject != null && obj.realizedObject is Weapon)
            {
                return (obj.realizedObject as Weapon).mode != Weapon.Mode.StuckInWall;
            }
            return true;
        }

        public void SeeThrownWeapon(PhysicalObject obj, Creature thrower)
        {
            if (thrower != cat && base.tracker.RepresentationForObject(thrower, AddIfMissing: false) == null)
            {
                base.noiseTracker.mysteriousNoises += 20f;
                base.noiseTracker.mysteriousNoiseCounter = 200;
            }
        }

        public void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise)
        {
        }

        public void SocialEvent(SocialEventRecognizer.EventID ID, Creature subjectCrit, Creature objectCrit, PhysicalObject involvedItem)
        {
            Tracker.CreatureRepresentation creatureRepresentation = base.tracker.RepresentationForObject(subjectCrit, AddIfMissing: false);
            if (creatureRepresentation == null)
            {
                return;
            }
            Tracker.CreatureRepresentation creatureRepresentation2 = null;
            bool flag = objectCrit == cat;
            if (!flag)
            {
                creatureRepresentation2 = base.tracker.RepresentationForObject(objectCrit, AddIfMissing: false);
                if (creatureRepresentation2 == null)
                {
                    return;
                }
            }
            if ((!flag && cat.dead) || (creatureRepresentation2 != null && creatureRepresentation.TicksSinceSeen > 40 && creatureRepresentation2.TicksSinceSeen > 40))
            {
                return;
            }
            if ((ID == SocialEventRecognizer.EventID.LethalAttack || ID == SocialEventRecognizer.EventID.Killing) && objectCrit is Player && subjectCrit.abstractCreature.creatureTemplate.type != MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            {
                for (int i = 0; i < base.relationshipTracker.relationships.Count; i++)
                {
                    if (base.relationshipTracker.relationships[i].trackerRep != null && base.relationshipTracker.relationships[i].trackerRep.representedCreature != null && base.relationshipTracker.relationships[i].trackerRep.representedCreature.realizedCreature == subjectCrit)
                    {
                        (base.relationshipTracker.relationships[i].state as SlugNPCTrackState).hurtAFriend = true;
                    }
                }
            }
            if (ID == SocialEventRecognizer.EventID.ItemOffering && subjectCrit.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat && !(involvedItem is Player) && objectCrit == cat)
            {
                if (involvedItem.room.abstractRoom.name == "SL_AI" && involvedItem.firstChunk.pos.x > 1150f)
                {
                    Custom.Log("Reject gift due to moon proximity");
                }
                else if (involvedItem is Creature && (involvedItem as Creature).dead && ((involvedItem is IPlayerEdible && (involvedItem as IPlayerEdible).Edible) || ((involvedItem as Creature).State.meatLeft > 0 && !IsFull)))
                {
                    Custom.Log("got creature as gift");
                    base.friendTracker.giftOfferedToMe = involvedItem.room.socialEventRecognizer.ItemOwnership(involvedItem);
                }
                else if (!(involvedItem is Creature))
                {
                    Custom.Log("got item as gift");
                    base.friendTracker.giftOfferedToMe = involvedItem.room.socialEventRecognizer.ItemOwnership(involvedItem);
                }
                else
                {
                    Custom.Log("Reject unsuitable gift");
                }
            }
        }

        private void DecideBehavior()
        {
            if (cat.grabbedBy.Count > 0 && cat.grabbedBy[0].grabber is Player)
            {
                behaviorType = BehaviorType.BeingHeld;
            }
            else if (cat.onBack != null)
            {
                behaviorType = BehaviorType.OnHead;
            }
            else
            {
                if (behaviorType == BehaviorType.Thrown && cat.bodyMode == Player.BodyModeIndex.Default)
                {
                    return;
                }
                base.utilityComparer.GetUtilityTracker(base.preyTracker).weight = ((!abstractAI.toldToStay.HasValue && !IsFull) ? (((base.friendTracker.friend != null) ? Mathf.Clamp01(1f - followCloseness) : 1f) - base.friendTracker.Urgency * 0.5f) : (abstractAI.toldToStay.HasValue ? 0f : Mathf.Lerp(0f, 0.4f, Mathf.InverseLerp(0.6f, 1f, cat.abstractCreature.personality.aggression)))) * Mathf.InverseLerp(0.95f, 0.65f, cat.abstractCreature.personality.sympathy);
                base.utilityComparer.GetUtilityTracker(base.friendTracker).weight = (abstractAI.toldToStay.HasValue ? 0f : followCloseness);
                if (base.threatTracker.mostThreateningCreature != null && base.threatTracker.mostThreateningCreature.representedCreature != null && base.threatTracker.mostThreateningCreature.representedCreature.abstractAI != null && base.threatTracker.mostThreateningCreature.representedCreature.abstractAI.RealAI != null && base.threatTracker.mostThreateningCreature.representedCreature.abstractAI.RealAI.pathFinder != null && !base.threatTracker.mostThreateningCreature.representedCreature.abstractAI.RealAI.pathFinder.CoordinateReachable(abstractAI.parent.pos))
                {
                    base.utilityComparer.GetUtilityTracker(base.threatTracker).weight = 0.75f - 0.7f * Mathf.Pow(cat.abstractCreature.personality.bravery, 0.5f);
                }
                else
                {
                    base.utilityComparer.GetUtilityTracker(base.threatTracker).weight = 1f;
                }
                AIModule aIModule = base.utilityComparer.HighestUtilityModule();
                float num = base.utilityComparer.HighestUtility();
                if (aIModule != null && num > 0.2f)
                {
                    if (aIModule is ThreatTracker)
                    {
                        if (AttackingThreat())
                        {
                            behaviorType = BehaviorType.Attacking;
                        }
                        else
                        {
                            behaviorType = BehaviorType.Fleeing;
                        }
                    }
                    else if (aIModule is FriendTracker && !abstractAI.toldToStay.HasValue)
                    {
                        behaviorType = BehaviorType.Following;
                    }
                    else if (aIModule is PreyTracker && (aIModule as PreyTracker).MostAttractivePrey != null)
                    {
                        behaviorType = BehaviorType.Attacking;
                    }
                }
                else
                {
                    behaviorType = ((base.friendTracker.friend != null && followCloseness > 0f && !abstractAI.toldToStay.HasValue) ? BehaviorType.Following : BehaviorType.Idle);
                }
                if (grabTarget != null && (num <= 0.2f || behaviorType == BehaviorType.Following))
                {
                    behaviorType = BehaviorType.GrabItem;
                }
            }
        }

        private bool HasEdible()
        {
            for (int i = 0; i < cat.grasps.Length; i++)
            {
                if (cat.grasps[i] != null && WantsToEatThis(cat.grasps[i].grabbed) && (!(cat.grasps[i].grabbed is Creature) || (cat.grasps[i].grabbed as Creature).dead || (double)UnityEngine.Random.value < Math.Pow(Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0.9f, 0.7f, creature.personality.sympathy)), 0.10000000149011612)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanGrabItem(PhysicalObject obj)
        {
            if (cat.CanIPickThisUp(obj))
            {
                return cat.NPCGrabCheck(obj);
            }
            return false;
        }

        private bool HoldingThis(PhysicalObject obj)
        {
            for (int i = 0; i < cat.grasps.Length; i++)
            {
                if (cat.grasps[i] != null && cat.grasps[i].grabbed == obj)
                {
                    return true;
                }
            }
            return false;
        }

        public void GiftRecieved(SocialEventRecognizer.OwnedItemOnGround giftOfferedToMe)
        {
            SocialMemory.Relationship orInitiateRelationship = creature.realizedCreature.State.socialMemory.GetOrInitiateRelationship(giftOfferedToMe.owner.abstractCreature.ID);
            if (giftOfferedToMe.owner is Player)
            {
                orInitiateRelationship.InfluenceLike(1f);
                orInitiateRelationship.InfluenceTempLike(1f);
            }
            Custom.Log(orInitiateRelationship.ToString());
        }

        private float SpearThrowPositionScore(WorldCoordinate tst, IntVector2 creaturePosition, ref List<IntVector2> creatureMovementArea)
        {
            if (!base.pathFinder.CoordinateViable(tst))
            {
                return float.MinValue;
            }
            QuickConnectivity.FloodFill(cat.room, creature.creatureTemplate, tst.Tile, 40, 500, _cachedFloodFillList);
            int num = int.MinValue;
            int num2 = int.MaxValue;
            for (int i = 0; i < creatureMovementArea.Count; i++)
            {
                if (creatureMovementArea[i].y > num)
                {
                    num = creatureMovementArea[i].y;
                }
                if (creatureMovementArea[i].y < num2)
                {
                    num2 = creatureMovementArea[i].y;
                }
            }
            float num3 = 0f;
            for (int j = 0; j < _cachedFloodFillList.Count; j++)
            {
                if (_cachedFloodFillList[j].y < num2 || _cachedFloodFillList[j].y > num || !cat.room.VisualContact(_cachedFloodFillList[j], creaturePosition))
                {
                    continue;
                }
                for (int k = 0; k < creatureMovementArea.Count; k++)
                {
                    if (_cachedFloodFillList[j].y == creatureMovementArea[k].y && NoSolidTilesBetween(_cachedFloodFillList[j].x, creatureMovementArea[k].x, _cachedFloodFillList[j].y))
                    {
                        num3 += 1f;
                    }
                }
            }
            num3 = ((num3 != 0f) ? (num3 + 100f) : 1f);
            for (int l = 0; l < _cachedFloodFillList.Count; l++)
            {
                if (_cachedFloodFillList[l].FloatDist(creature.pos.Tile) < 3f)
                {
                    num3 *= 2f;
                    break;
                }
            }
            if (base.pathFinder.GetDestination.room == creature.pos.room)
            {
                for (int m = 0; m < _cachedFloodFillList.Count; m++)
                {
                    if (_cachedFloodFillList[m].FloatDist(base.pathFinder.GetDestination.Tile) < 3f)
                    {
                        num3 *= 2f;
                        break;
                    }
                }
            }
            num3 *= Custom.LerpMap(tst.Tile.FloatDist(creaturePosition), 30f, 60f, 1f, 0f);
            num3 *= Custom.LerpMap(tst.Tile.FloatDist(creaturePosition), 5f, 0f, 1f, 0.1f);
            num3 *= 1f - base.threatTracker.ThreatOfArea(tst, accountThreatCreatureAccessibility: true);
            num3 *= ((Math.Abs(tst.Tile.y - creaturePosition.y) >= 3) ? Mathf.InverseLerp(5f, 10f, tst.Tile.FloatDist(creaturePosition)) : 1f);
            if (base.pathFinder.GetDestination.Tile == tst.Tile)
            {
                num3 *= 0.1f;
            }
            for (int n = 1; n < previousAttackPositions.Count; n++)
            {
                num3 -= Custom.LerpMap(tst.Tile.FloatDist(previousAttackPositions[n]), 0f, 5f, 50f, 0f);
            }
            return num3;
        }

        private bool NoSolidTilesBetween(int xA, int xB, int y)
        {
            if (xB < xA)
            {
                int num = xA;
                xA = xB;
                xB = num;
            }
            for (int i = xA; i <= xB; i++)
            {
                if (cat.room.GetTile(i, y).Solid)
                {
                    return false;
                }
            }
            return true;
        }

        private void FindAttackPosition(Tracker.CreatureRepresentation target)
        {
            if (target.representedCreature.creatureTemplate.PreBakedPathingIndex < 0)
            {
                list = new List<IntVector2> { target.BestGuessForPosition().Tile };
            }
            else
            {
                QuickConnectivity.FloodFill(cat.room, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), target.BestGuessForPosition().Tile, 20, 500, list);
            }
            IntVector2 pos;
            if (UnityEngine.Random.value < 0.5f && list.Count > 0)
            {
                IntVector2 intVector = list[UnityEngine.Random.Range(0, list.Count)];
                pos = intVector;
                int num = ((UnityEngine.Random.value >= 0.5f) ? 1 : (-1));
                for (int i = 0; i < 40 && !cat.room.GetTile(intVector + new IntVector2(num * i, 0)).Solid; i++)
                {
                    if (i > 5 && base.pathFinder.CoordinateViable(cat.room.GetWorldCoordinate(intVector + new IntVector2(num * i, 0))))
                    {
                        pos = intVector + new IntVector2(num * i, 0);
                        break;
                    }
                }
            }
            else
            {
                pos = creature.pos.Tile + new IntVector2(UnityEngine.Random.Range(1, 10) * ((UnityEngine.Random.value >= 0.5f) ? 1 : (-1)), UnityEngine.Random.Range(1, 10) * ((UnityEngine.Random.value >= 0.5f) ? 1 : (-1)));
            }
            if (base.pathFinder.CoordinateViable(cat.room.GetWorldCoordinate(pos)) && SpearThrowPositionScore(cat.room.GetWorldCoordinate(pos), target.BestGuessForPosition().Tile, ref list) > SpearThrowPositionScore(testThrowPos, target.BestGuessForPosition().Tile, ref list))
            {
                testThrowPos = cat.room.GetWorldCoordinate(pos);
            }
            if (testThrowPos != attackPos && (Custom.ManhattanDistance(creature.pos, attackPos) < 3 || SpearThrowPositionScore(testThrowPos, target.BestGuessForPosition().Tile, ref list) > SpearThrowPositionScore(attackPos, target.BestGuessForPosition().Tile, ref list) + (float)changeAttackPositionDelay))
            {
                attackPos = testThrowPos;
                changeAttackPositionDelay = 400;
                previousAttackPositions.Insert(0, testThrowPos.Tile);
                if (previousAttackPositions.Count > 20)
                {
                    previousAttackPositions.RemoveAt(20);
                }
            }
            if (changeAttackPositionDelay > 0)
            {
                changeAttackPositionDelay--;
            }
        }*/
    }
}
