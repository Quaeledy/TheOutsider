using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CustomOracleTx;
using UnityEngine;
using Random = UnityEngine.Random;
using RWCustom;
using static CustomOracleTx.CustomOracleBehaviour;
using CustomDreamTx;
using TheOutsider.CustomLore.CustomDream;
using TheOutsider.Player_Hooks;

namespace TheOutsider.CustomLore.CustomOracle
{
    public class AMOracleBehaviour : CustomOracleBehaviour
    {
        public static CustomAction MeetOutsider_Init = new CustomAction("MeeetOutsider_Init", true);
        public static CustomAction MeetOutsider_GiveMark = new CustomAction("MeeetOutsider_GiveMark", true);
        public static CustomAction MeetOutsider_SternFarewell = new CustomAction("MeeetOutsider_SternFarewell", true);

        public static CustomAction MeetOutsider_TalkAfterGiveMark = new CustomAction("MeeetOutsider_TalkAfterGiveMark", true);
        public static CustomAction MeetOutsider_FarewellTalk = new CustomAction("MeeetOutsider_FarewellTalk", true);

        public static Conversation.ID Outsider_DreamTalk0 = new Conversation.ID("Outsider_DreamTalk0", true);
        public static Conversation.ID Outsider_DreamTalk1 = new Conversation.ID("Outsider_DreamTalk1", true);

        public static CustomSubBehaviour.CustomSubBehaviourID MeetOutsider = new CustomSubBehaviour.CustomSubBehaviourID("MeetOutsider", true);

        public int ConversationHad = 0;

        public override int NotWorkingPalette => 29116;
        public override int GetWorkingPalette => 29117;
        public override Vector2 GetToDir => Vector2.up;

        public AMOracleBehaviour(Oracle oracle) : base(oracle)
        {
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
        }

        public override void SeePlayer()
        {
            base.SeePlayer();
            Plugin.Log("Oracle see player");

            if (ConversationHad == 0)
            {
                ConversationHad++;
                NewAction(MeetOutsider_Init);
            }
        }


        public override void NewAction(CustomAction nextAction)
        {
            Plugin.Log(string.Concat(new string[]
            {
                "new action: ",
                nextAction.ToString(),
                " (from ",
                action.ToString(),
                ")"
            }));

            if (nextAction == action) return;

            CustomSubBehaviour.CustomSubBehaviourID customSubBehaviourID = null;

            if (nextAction == MeetOutsider_Init ||
                nextAction == MeetOutsider_GiveMark ||
                nextAction == MeetOutsider_TalkAfterGiveMark ||
                nextAction == MeetOutsider_SternFarewell)
            {
                customSubBehaviourID = MeetOutsider;
            }
            else
                customSubBehaviourID = CustomSubBehaviour.CustomSubBehaviourID.General;

            currSubBehavior.NewAction(action, nextAction);

            if (customSubBehaviourID != CustomSubBehaviour.CustomSubBehaviourID.General && customSubBehaviourID != currSubBehavior.ID)
            {
                CustomSubBehaviour subBehavior = null;
                for (int i = 0; i < allSubBehaviors.Count; i++)
                {
                    if (allSubBehaviors[i].ID == customSubBehaviourID)
                    {
                        subBehavior = allSubBehaviors[i];
                        break;
                    }
                }
                if (subBehavior == null)
                {
                    if (customSubBehaviourID == MeetOutsider)
                    {
                        subBehavior = new AMOracleMeetOutsider(this);
                    }
                    allSubBehaviors.Add(subBehavior);
                }
                subBehavior.Activate(action, nextAction);
                currSubBehavior.Deactivate();
                Plugin.Log("Switching subbehavior to: " + subBehavior.ID.ToString() + " from: " + this.currSubBehavior.ID.ToString());
                currSubBehavior = subBehavior;
            }
            inActionCounter = 0;
            action = nextAction;
        }

        public override void GeneralActionUpdate()
        {
            if (action == CustomAction.General_Idle)
            {
                if (movementBehavior != CustomMovementBehavior.Idle)
                {
                    movementBehavior = CustomMovementBehavior.Idle;
                }
                throwOutCounter = 0;
                if (player != null && player.room == oracle.room)
                {
                    discoverCounter++;
                    if (oracle.room.GetTilePosition(player.mainBodyChunk.pos).y < 32 && (discoverCounter > 220 || Custom.DistLess(player.mainBodyChunk.pos, oracle.firstChunk.pos, 150f) || !Custom.DistLess(player.mainBodyChunk.pos, oracle.room.MiddleOfTile(oracle.room.ShortcutLeadingToNode(1).StartTile), 150f)))
                    {
                        SeePlayer();
                    }
                }
            }
            else if (action == CustomAction.General_GiveMark)
            {
                //开始开光
                if (inActionCounter > 30 && inActionCounter < 300)
                {
                    movementBehavior = CustomMovementBehavior.Investigate;
                    player.Stun(20);
                    player.mainBodyChunk.vel += Vector2.ClampMagnitude(oracle.room.MiddleOfTile(24, 14) - player.mainBodyChunk.pos, 40f) / 40f * 3.2f * Mathf.InverseLerp(20f, 160f, (float)inActionCounter);
                }
                if (inActionCounter == 30)
                {
                    oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Telekenisis, 0f, 1f, 1f);
                }
                //开光一瞬
                if (inActionCounter == 300)
                {
                    player.Stun(40);
                    (oracle.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark = true;
                    for (int m = 0; m < 20; m++)
                    {
                        oracle.room.AddObject(new Spark(player.mainBodyChunk.pos, Custom.RNV() * Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                    }
                    oracle.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1f);
                }
                //轻轻放下
                if (inActionCounter > 300 && inActionCounter < 380 && player != null)
                {
                    player.mainBodyChunk.vel += 0.8f * Vector2.up + 0.05f * Vector2.up * Mathf.InverseLerp(300f, 380f, (float)inActionCounter);
                    player.bodyChunks[1].vel += 0.8f * Vector2.up;
                }
                //开光之后
                if (inActionCounter > 300 && player.graphicsModule != null)
                {
                    movementBehavior = CustomMovementBehavior.Talk;
                    (player.graphicsModule as PlayerGraphics).markAlpha = Mathf.Max((player.graphicsModule as PlayerGraphics).markAlpha, Mathf.InverseLerp(500f, 300f, (float)inActionCounter));
                }
                if (inActionCounter >= 500)
                {
                    NewAction(AMOracleBehaviour.MeetOutsider_TalkAfterGiveMark);
                    if (conversation != null)
                    {
                        conversation.paused = false;
                    }
                }
            }
        }

        public override void AddConversationEvents(CustomOracleConversation conv, Conversation.ID id)
        {
            int extralingerfactor = oracle.room.game.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Chinese ? 1 : 0;
            if (id == Outsider_DreamTalk0)
            {
                //（离群进入房间）
                //（打量片刻后开光）
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Ah, another curious little moth. Hello~"), 50 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You little guys are really a curious creature. However, did you come here alone? <LINE>Why are you alone and not staying with your companions?"), 140 * extralingerfactor));
                //（离群走来走去）（离群蹦蹦跳跳）（离群吐出某个物品，放到水银旁边）
                conv.events.Add(new CustomOracleConversation.PauseAndWaitForStillEvent(conv, conv.convBehav, 40));
                conv.events.Add(new Conversation.TextEvent(conv, 0, "......", 0));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("...Are you going to give this to me?"), 50 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Oh... haha~ you're such an interesting little guy."), 60 * extralingerfactor));
                //（物品漂浮到水银面前）
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("According to etiquette, I also have to give you a gift."), 70 * extralingerfactor));
                //（召唤信标）
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("This is a beacon used by my creators to call out to their vehicles for transportation. <LINE>Those vehicles are very sensitive to the light emitted by beacons, and when they appear, <LINE>your natural enemies will be afraid to escape."), 200 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("However, strong electromagnetic or physical impacts can damage its internal structure."), 90 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("If you break it, I won't compensate you for it~"), 60 * extralingerfactor));
                //（梦境结束）
            }
            else if (id == Outsider_DreamTalk1)
            {
                //（离群进入房间）
                conv.events.Add(new Conversation.TextEvent(conv, 0, "......", 0));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("...Didn't I already tell you?"), 80 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I can no longer take care of my garden. Cold is harmful to you, <LINE>and this place is no longer suitable for you to live in. Move away with your companions quickly."), 160 * extralingerfactor));
                //（离群靠近）
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("You can't do this...... I can't replace your peers. <LINE>Have you made any cognitive errors? Your ethnicity is your destination."), 120 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("...Are you listening to me, little moth?"), 80 * extralingerfactor));
                conv.events.Add(new Conversation.TextEvent(conv, 0, "......", 0));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("Don't come to me again! Leave!"), 40 * extralingerfactor));
                //（闪电爆鸣特效）（离群吓得跳开）（再次闪电爆鸣）（离群吓走，离开房间）
                conv.events.Add(new Conversation.TextEvent(conv, 0, "......", 0));
                conv.events.Add(new Conversation.TextEvent(conv, 0, Translate("I... I'm sorry..."), 40 * extralingerfactor));
                //（片刻后房间变红，水银摔到地上，警示亮起）（梦境结束）
            }
        }
    }

    
}
