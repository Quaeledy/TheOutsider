using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheOutsider.CustomLore.CustomCreature.Alcedo
{
    public class AlcedoLimb : Limb
    {
        private AlcedoLimb otherLimbInPair;

        public bool reachingForTerrain;

        public int gripCounter;

        public bool currentlyDisabled;

        public float jointDist;

        public Vector2 grabPos;

        public float flip;

        private bool extraLongStep;

        private bool soundBool;

        private SoundID grabSound;

        private SoundID releaseSeound;

        public float health
        {
            get
            {
                if (!(owner as AlcedoGraphics).alcedo.Consious)
                {
                    return 0f;
                }
                return ((owner as AlcedoGraphics).alcedo.State as Alcedo.AlcedoState).limbHealth[limbNumber];
            }
        }

        private float StepLength => Mathf.Lerp(-0.5f, 0.5f, (owner as AlcedoGraphics).alcedo.stepLength * health);

        public AlcedoLimb(GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, AlcedoLimb otherLimbInPair)
            : base(owner, connectionChunk, num, rad, sfFric, aFric, huntSpeed, quickness)
        {
            this.otherLimbInPair = otherLimbInPair;
            base.mode = Mode.Retracted;
            jointDist = 25f * (((owner as AlcedoGraphics).alcedo.bodySizeFac + 1f) / 2f);
            if ((owner as AlcedoGraphics).alcedo.Template.type == CreatureTemplate.Type.GreenLizard)
            {
                grabSound = SoundID.Lizard_Green_Foot_Grab;
                releaseSeound = SoundID.Lizard_Green_Foot_Release;
            }/*
            else if ((owner as AlcedoGraphics).alcedo.alcedoParams.WallClimber)
            {
                grabSound = SoundID.Lizard_BlueWhite_Foot_Grab;
                releaseSeound = SoundID.Lizard_BlueWhite_Foot_Release;
            }*/
            else
            {
                grabSound = SoundID.Lizard_PinkYellowRed_Foot_Grab;
                releaseSeound = SoundID.Lizard_PinkYellowRed_Foot_Release;
            }
        }

        public override void Reset(Vector2 resetPoint)
        {
            retract = true;
            base.Reset(resetPoint);
        }

        public override void Update()
        {
            if ((!currentlyDisabled && Random.value > health && Random.value < 0.02f) || (owner as AlcedoGraphics).alcedo.Stunned)
            {
                currentlyDisabled = true;
            }
            else if ((currentlyDisabled && Random.value < health && Random.value < 0.02f) || health == 1f)
            {
                currentlyDisabled = false;
            }
            huntSpeed = (owner as AlcedoGraphics).alcedo.limbSpeed * Mathf.Lerp(0.5f, 1f, health);
            quickness = Mathf.Lerp(0.1f, (owner as AlcedoGraphics).alcedo.limbQuickness, health);
            if (currentlyDisabled || (owner as AlcedoGraphics).alcedo.swim > 0.5f)
            {
                base.mode = Mode.Dangle;
                vel.y -= 0.9f;
                retract = false;
                gripCounter = 0;
                reachingForTerrain = false;
            }
            else
            {
                if (base.mode == Mode.HuntAbsolutePosition)
                {
                    huntSpeed = defaultHuntSpeed + connection.vel.magnitude;
                }
                Vector2 a = Custom.DirVec(connection.rotationChunk.pos, connection.pos);
                if (connection.index == 2)
                {
                    a *= -1f;
                }
                a = Vector2.Lerp(a, Custom.DirVec(connection.pos, (owner as AlcedoGraphics).alcedo.limbsAimFor), 0.4f).normalized;
                float num = Custom.DistanceToLine(pos, connection.pos, connection.pos + Custom.PerpendicularVector(a));
                if (!reachingForTerrain)
                {
                    base.mode = Mode.HuntAbsolutePosition;
                    absoluteHuntPos = Vector2.Lerp(pos, connection.pos, (owner as AlcedoGraphics).alcedo.liftFeet) + a * (jointDist + 1f);
                    if (extraLongStep)
                    {
                        if (otherLimbInPair.currentlyDisabled)
                        {
                            extraLongStep = false;
                        }
                        if (otherLimbInPair.gripCounter > (owner as AlcedoGraphics).alcedo.limbGripDelay)
                        {
                            extraLongStep = false;
                        }
                    }
                    if (!extraLongStep && num < jointDist * (0f - StepLength))
                    {
                        reachingForTerrain = true;
                    }
                }
                else
                {
                    if (!base.OverLappingHuntPos)
                    {
                        a += Custom.PerpendicularVector(a) * (6f - 12f * (float)(limbNumber % 2)) * 0.2f;
                        a.y -= 0.3f * (owner as AlcedoGraphics).alcedo.feetDown;
                        a.x += ((limbNumber % 2 == 0) ? (-1f) : 1f) * (owner as AlcedoGraphics).alcedo.legPairDisplacement * flip;
                        FindGrip(owner.owner.room, connection.pos, connection.pos, jointDist - 1f, connection.pos + a * 50f, 2, 2, false);//(owner as AlcedoGraphics).alcedo.IsWallClimber;
                    }
                    else
                    {
                        if (soundBool)
                        {
                            owner.owner.room.PlaySound(grabSound, pos);
                            soundBool = false;
                        }
                        if (num > jointDist * -0.5f * ((owner as AlcedoGraphics).alcedo.stepLength + 0.1f) && !Custom.DistLess(pos, connection.pos, jointDist - 1f) && !Custom.DistLess(absoluteHuntPos, connection.pos, jointDist))
                        {
                            extraLongStep = otherLimbInPair != null && (owner as AlcedoGraphics).alcedo.smoothenLegMovement && otherLimbInPair.gripCounter < 1;
                            reachingForTerrain = false;
                        }
                        if ((owner as AlcedoGraphics).alcedo.smoothenLegMovement)
                        //if ((owner as AlcedoGraphics).alcedo.AI.runSpeed > 0.1f && (owner as AlcedoGraphics).alcedo.smoothenLegMovement)
                        {
                            bool flag = true;
                            AlcedoLimb[] limbs = (owner as AlcedoGraphics).limbs;
                            foreach (AlcedoLimb alcedoLimb in limbs)
                            {
                                if (alcedoLimb.gripCounter > gripCounter || alcedoLimb.gripCounter == 0)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                reachingForTerrain = false;
                            }
                        }
                        if (!reachingForTerrain && !soundBool)
                        {
                            owner.owner.room.PlaySound(releaseSeound, pos);
                            soundBool = true;
                        }
                    }
                    if (base.mode == Mode.Dangle)
                    {
                        reachingForTerrain = false;
                    }
                }
            }
            if (base.mode == Mode.Dangle)
            {
                vel += Custom.DirVec(pos, connection.pos + Custom.RotateAroundOrigo(new Vector2(30f - 60f * (float)(limbNumber % 2), 10f), Custom.AimFromOneVectorToAnother(connection.rotationChunk.pos, connection.pos))) * 0.6f;
            }
            if (retract && reachedSnapPosition)
            {
                base.mode = Mode.HuntAbsolutePosition;
                reachingForTerrain = false;
                retract = false;
            }
            if (otherLimbInPair != null && Custom.DistLess(pos, otherLimbInPair.pos, rad * 3f))
            {
                float num2 = Vector2.Distance(pos, otherLimbInPair.pos);
                Vector2 vector = Custom.DirVec(pos, otherLimbInPair.pos);
                vel -= (rad * 3f - num2) * vector * 0.5f;
                otherLimbInPair.vel += (rad * 3f - num2) * vector * 0.5f;
            }
            base.Update();
            ConnectToPoint(connection.pos, jointDist, push: false, 0f, connection.vel, 0f, 0f);
            if (base.mode == Mode.HuntAbsolutePosition && reachingForTerrain && (reachedSnapPosition || (base.OverLappingHuntPos && terrainContact)))
            {
                gripCounter++;
            }
            else
            {
                gripCounter = 0;
            }
        }

        public override void GrabbedTerrain()
        {
            grabPos = absoluteHuntPos;
            reachingForTerrain = true;
        }
    }

}
