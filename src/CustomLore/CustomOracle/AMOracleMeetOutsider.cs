using CustomDreamTx;
using TheOutsider.CustomLore.CustomDream;
using TheOutsider.Player_Hooks;
using static CustomOracleTx.CustomOracleBehaviour;

namespace TheOutsider.CustomLore.CustomOracle
{
    public class AMOracleMeetOutsider : CustomConversationBehaviour
    {
        //public Simple3DObject scanned3DEffect;
        public int lastConvCount = 0;

        #region DreamTalk1
        public LightSource portShowLight;
        public static readonly int movePortCounter = 400;

        public int currentMovePortCounter = 0;
        #endregion

        public AMOracleMeetOutsider(AMOracleBehaviour owner) : base(owner, AMOracleBehaviour.MeetOutsider, AMOracleBehaviour.Outsider_DreamTalk0)
        {
            owner.getToWorking = 0f;
        }

        public override void Update()
        {
            base.Update();
            if (player == null) return;
            if (!PlayerHooks.PlayerData.TryGetValue(player, out var module))
            {
                return;
            }

            if (action == AMOracleBehaviour.MeetOutsider_Init)
            {
                movementBehavior = CustomMovementBehavior.Idle;
                if (inActionCounter > 20)
                {
                    if (CustomDreamRx.currentActivateNormalDream != null)
                    {
                        if (CustomDreamRx.currentActivateNormalDream.activateDreamID == OutsiderDream.OutsiderDream_0)
                        {
                            owner.NewAction(AMOracleBehaviour.MeetOutsider_GiveMark);
                            return;
                        }
                        else if (CustomDreamRx.currentActivateNormalDream.activateDreamID == OutsiderDream.OutsiderDream_1)
                        {
                            owner.NewAction(AMOracleBehaviour.MeetOutsider_SternFarewell);
                            return;
                        }
                    }

                    owner.NewAction(CustomAction.General_Idle);
                    return;
                }
            }
            else if (action == AMOracleBehaviour.MeetOutsider_GiveMark)
            {
                owner.NewAction(AMOracleBehaviour.CustomAction.General_GiveMark);
            }
            else if (action == AMOracleBehaviour.MeetOutsider_TalkAfterGiveMark)
            {
                if (owner.conversation != null)
                {
                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;

                        if (CustomDreamRx.currentActivateNormalDream != null)
                        {
                            CustomDreamRx.currentActivateNormalDream.EndDream(oracle.room.game);
                        }
                        return;
                    }
                }
            }
            else if (action == AMOracleBehaviour.MeetOutsider_SternFarewell)
            {
                if (owner.conversation != null)
                {
                    movementBehavior = CustomMovementBehavior.KeepDistance;
                    /*
                    if (PlayerHooks.PlayerData.TryGetValue(player, out var pmodule))
                    {
                        var overrides = pmodule.stateOverride;
                        if (currentMovePortCounter < movePortCounter)
                        {
                            currentMovePortCounter++;
                            overrides.connectToDMProggress = currentMovePortCounter / (float)movePortCounter;


                            if (portShowLight == null)
                            {
                                portShowLight = new LightSource(overrides.currentPortPos, false, new Color(255f / 255f, 67f / 255f, 115f / 255f), oracle) { alpha = 10f, rad = 200f };
                                oracle.room.AddObject(portShowLight);
                            }
                            portShowLight.pos = overrides.currentPortPos;
                        }
                        else
                        {
                            if (portShowLight != null)
                            {
                                oracle.room.PlaySound(SoundID.Gate_Clamp_Lock, player.mainBodyChunk, false, 1f, 2.2f + Random.value);
                                oracle.room.AddObject(new ExplosionSpikes(oracle.room, player.mainBodyChunk.pos, 5, 40f, 50, 10f, 20f, Color.white));

                                portShowLight.Destroy();
                                portShowLight = null;
                            }
                        }
                    }*/

                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;
                        if (CustomDreamRx.currentActivateNormalDream != null)
                        {
                            CustomDreamRx.currentActivateNormalDream.EndDream(oracle.room.game);
                        }
                        return;
                    }
                }
            }
            /*
            else if (action == AMOracleBehaviour.MeetOutsider_DreamTalk2)
            {
                if (owner.conversation != null)
                {
                    movementBehavior = CustomMovementBehavior.Talk;
                    
                    if (owner.conversation.events.Count < 5 && scanned3DEffect == null)//前三句话说完了
                    {
                        scanned3DEffect = new Simple3DObject(oracle.room, player);
                        oracle.room.AddObject(scanned3DEffect);
                    }
                    if (owner.conversation.slatedForDeletion)
                    {
                        owner.conversation = null;

                        if (CustomDreamRx.currentActivateNormalDream != null)
                        {
                            CustomDreamRx.currentActivateNormalDream.EndDream(oracle.room.game);
                        }
                        return;
                    }
                }
            }*/
        }

        public override void NewAction(CustomAction oldAction, CustomAction newAction)
        {
            base.NewAction(oldAction, newAction);
            if (newAction == AMOracleBehaviour.MeetOutsider_TalkAfterGiveMark)
            {
                owner.InitateConversation(AMOracleBehaviour.Outsider_DreamTalk0, this);
            }
            else if (newAction == AMOracleBehaviour.MeetOutsider_FarewellTalk)
            {
                owner.InitateConversation(AMOracleBehaviour.Outsider_DreamTalk1, this);
            }
            else if (newAction == CustomAction.General_GiveMark)
            {
                owner.getToWorking = 0f;
            }
            else if (newAction == CustomAction.General_Idle)
            {
                owner.getToWorking = 1f;
            }
        }
    }
}
