using BepInEx.Logging;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheOutsider.Oracle_Hooks;
using UnityEngine;
using MoreSlugcats;
using RWCustom;
using Random = UnityEngine.Random;
using TheOutsider.Player_Hooks;

namespace TheOutsider.Oracle_Hooks
{
    class CLOracleHooks
    {
        public static void Init()
        {
            On.MoreSlugcats.CLOracleBehavior.Update += CLOracleBehavior_Update;
            On.MoreSlugcats.CLOracleBehavior.InitateConversation += CLOracleBehavior_InitateConversation;
            On.MoreSlugcats.CLOracleBehavior.InterruptRain += CLOracleBehavior_InterruptRain;
        }
        
        public static void CLOracleBehavior_Update(On.MoreSlugcats.CLOracleBehavior.orig_Update orig, CLOracleBehavior self, bool eu)
        {
            if (self.oracle.room.world.game.session.characterStats.name == Plugin.SlugName)
            {
                /*
                if (self.halcyon != null && self.halcyon.room == self.oracle.room)
                {
                    self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen = false;
                }
                else if (self.player != null && (self.halcyon == null || self.halcyon.room != self.oracle.room) && !self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen)
                {
                    if (Random.value < 0.15f)
                    {
                        self.dialogBox.Interrupt(self.Translate("...Why..."), 60);
                    }
                    else if (Random.value < 0.15f)
                    {
                        self.dialogBox.Interrupt(self.Translate("...No..."), 60);
                    }
                    else if (Random.value < 0.15f)
                    {
                        self.dialogBox.Interrupt(self.Translate("...Stop..."), 60);
                    }
                    else
                    {
                        self.dialogBox.Interrupt(self.Translate("...Please..."), 60);
                    }
                }
                */
                self.oracle.stun = Math.Max(self.oracle.stun, Random.Range(2, 4));
            }

            orig(self, eu);
        }
        
        public static void CLOracleBehavior_InitateConversation(On.MoreSlugcats.CLOracleBehavior.orig_InitateConversation orig, CLOracleBehavior self)
        {
            if (self.player.abstractCreature.Room.world.game.session.characterStats.name != Plugin.SlugName)
            {
                orig(self);
            }
            else
            {
                self.dialogBox.NewMessage(self.Translate("..."), 200);
                if (self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen)
                {
                    if (Random.value < 0.15f)
                    {
                        self.dialogBox.NewMessage(self.Translate("...Why...."), 60);
                        return;
                    }
                    if (Random.value < 0.15f)
                    {
                        self.dialogBox.NewMessage(self.Translate("...No..."), 60);
                        return;
                    }
                    else if (Random.value < 0.15f)
                    {
                        self.dialogBox.Interrupt(self.Translate("...Stop..."), 60);
                    }
                    self.dialogBox.NewMessage(self.Translate("...Please..."), 60);
                    return;
                }
                else if (self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts > 0)
                {
                    if (Random.value < 0.15f)
                    {
                        self.dialogBox.NewMessage(self.Translate("...Go away..."), 60);
                        return;
                    }
                    if (Random.value < 0.15f)
                    {
                        self.dialogBox.NewMessage(self.Translate("...Not forgotten pain..."), 60);
                        return;
                    }
                    if (Random.value < 0.15f)
                    {
                        self.dialogBox.NewMessage(self.Translate("...So little... left. Why hurt... me more..."), 60);
                        return;
                    }
                    self.dialogBox.NewMessage(self.Translate("...Leave me... alone."), 60);
                    return;
                }
                else
                {
                    if (self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 0)
                    {
                        self.dialogBox.NewMessage(self.Translate("..."), 60);
                        self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad++;
                        return;
                    }
                    
                    else
                    {
                        if (Random.value < 0.15f)
                        {
                            self.dialogBox.NewMessage(self.Translate("...Why back?"), 60);
                            return;
                        }
                        if (Random.value < 0.15f)
                        {
                            self.dialogBox.NewMessage(self.Translate("...It is... brighter... today."), 60);
                            return;
                        }
                        if (Random.value < 0.15f)
                        {
                            self.dialogBox.NewMessage(self.Translate("...Nice to see..."), 60);
                            return;
                        }
                        self.dialogBox.NewMessage(self.Translate("...Thank you... for... company."), 60);
                        return;
                    }
                }
            }
        }

        public static void CLOracleBehavior_InterruptRain(On.MoreSlugcats.CLOracleBehavior.orig_InterruptRain orig, CLOracleBehavior self)
        {
            if (self.player.abstractCreature.Room.world.game.session.characterStats.name != Plugin.SlugName)
            {
                orig(self);
            }
            else
            {
                if (self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.halcyonStolen || self.oracle.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiThrowOuts > 0)
                {
                    return;
                }
                if (Random.value < 0.3f)
                {
                    self.dialogBox.Interrupt(self.Translate("..."), 5);
                    return;
                }
                if (Random.value < 0.3f)
                {
                    self.dialogBox.Interrupt(self.Translate("..."), 5);
                    return;
                }
                self.dialogBox.Interrupt(self.Translate("..."), 5);
            }
        }
    }
}
