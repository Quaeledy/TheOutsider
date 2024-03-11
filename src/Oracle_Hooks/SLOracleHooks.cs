using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOutsider.Player_Hooks;
using RWCustom;
using MoreSlugcats;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheOutsider.Oracle_Hooks
{
    class SLOracleHooks
    {
        public static void Init()
        {
            On.SLOracleBehaviorHasMark.Update += SLOracleBehaviorHasMark_Update;
            On.SLOracleBehaviorHasMark.InitateConversation += SLOracleBehaviorHasMark_InitateConversation;
            On.RainWorldGame.IsMoonActive += RainWorldGame_IsMoonActive;
            On.RainWorldGame.IsMoonHeartActive += RainWorldGame_IsMoonHeartActive;
            On.SaveState.ctor += SaveState_ctor;
        }


        public static void SLOracleBehaviorHasMark_Update(On.SLOracleBehaviorHasMark.orig_Update orig, SLOracleBehaviorHasMark self, bool eu)
        {
            orig(self, eu);
            if (self.oracle.room.world.game.session.characterStats.name != Plugin.SlugName)
            {
                return;
            }
            if (self.player != null && self.hasNoticedPlayer)
            {
                if (self.sayHelloDelay < 0)
                {
                    self.sayHelloDelay = 30;
                }
                if (!self.rainInterrupt && self.player.room == self.oracle.room && self.oracle.room.world.rainCycle.TimeUntilRain < 1600 && self.oracle.room.world.rainCycle.pause < 1)
                {
                    if (ModManager.MSC)
                    {
                        if (self.currentConversation == null)
                        {
                            self.InterruptRain();
                            self.rainInterrupt = true;
                        }
                    }
                }
            }
        }

        public static void SLOracleBehaviorHasMark_InitateConversation(On.SLOracleBehaviorHasMark.orig_InitateConversation orig, SLOracleBehaviorHasMark self)
        {
            if (self.player.abstractCreature.Room.world.game.session.characterStats.name != Plugin.SlugName)
            {
                orig(self);
            }
            else
            {
                if (!(self as SLOracleBehavior).State.SpeakingTerms)
                {
                    self.dialogBox.NewMessage("...", 10);
                    return;
                }
                int num = 0;
                for (int i = 0; i < self.player.grasps.Length; i++)
                {
                    if (self.player.grasps[i] != null && self.player.grasps[i].grabbed is SSOracleSwarmer)
                    {
                        num++;
                    }
                }
                if ((self as SLOracleBehavior).State.playerEncountersWithMark <= 0)
                {
                    if ((self as SLOracleBehavior).State.playerEncounters < 0)
                    {
                        (self as SLOracleBehavior).State.playerEncounters = 0;
                    }
                    self.dialogBox.NewMessage(self.Translate("..."), 60);
                    self.dialogBox.NewMessage(self.Translate("Hello, little creature."), 60);
                    self.dialogBox.NewMessage(self.Translate("You seem to have met one of us before."), 60);
                    self.dialogBox.NewMessage(self.Translate("From your peculiar appearance, I guess you probably come from a distant place. What brought you here?"), 60);
                    self.dialogBox.NewMessage(self.Translate("..."), 60);
                    self.dialogBox.NewMessage(self.Translate("Did you lose contact with your companions during your migration? I haven't seen your ethnic group. I haven't even seen your distant relatives for a long time."), 60);
                    self.dialogBox.NewMessage(self.Translate("I'm sorry, no matter what you want, I can't help you."), 60);
                    self.dialogBox.NewMessage(self.Translate("Our era is about to pass. The old world is coming to an end. It's not advisable to stay here for a long time. You'd better leave here as soon as possible."), 60);
                    return;
                }
                else
                {
                    if (num > 0)
                    {
                        self.PlayerHoldingSSNeuronsGreeting();
                        return;
                    }
                    if ((self as SLOracleBehavior).State.playerEncountersWithMark != 1)
                    {
                        self.ThirdAndUpGreeting();
                        return;
                    }
                    self.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(Conversation.ID.MoonSecondPostMarkConversation, self, SLOracleBehaviorHasMark.MiscItemType.NA);
                    return;
                }
            }
        }

        public static bool RainWorldGame_IsMoonActive(On.RainWorldGame.orig_IsMoonActive orig, RainWorldGame self)
        {
            if(self.session.characterStats.name != Plugin.SlugName)
            {
                return(orig(self));
            }
            else
            {
                return(!self.GetStorySession.saveState.deathPersistentSaveData.ripMoon && self.GetStorySession.saveState.miscWorldSaveData.SLOracleState.neuronsLeft > 0);
            }
        }

        public static bool RainWorldGame_IsMoonHeartActive(On.RainWorldGame.orig_IsMoonHeartActive orig, RainWorldGame self)
        {
            if (self.session.characterStats.name != Plugin.SlugName)
            {
                return(orig(self));
            }
            else
            {
                return (!self.GetStorySession.saveState.deathPersistentSaveData.ripMoon);
            }
        }

        private static void SaveState_ctor(On.SaveState.orig_ctor orig, SaveState self, SlugcatStats.Name saveStateNumber, PlayerProgression progression)
        {
            orig(self, saveStateNumber, progression);
            if (saveStateNumber == Plugin.SlugName)
            {
                self.miscWorldSaveData.SLOracleState.neuronsLeft = 7;
                self.miscWorldSaveData.moonGivenRobe = true;
            }
        }
    }
}
