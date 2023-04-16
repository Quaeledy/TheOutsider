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

namespace TheOutsider
{
    class CLOracleHooks : HookBase
    {
        CLOracleHooks(ManualLogSource log) : base(log)
        {
        }

        static public CLOracleHooks Instance(ManualLogSource log = null)
        {
            if (_instance == null)
                _instance = new CLOracleHooks(log);
            return _instance;
        }

        public override void OnModsInit(RainWorld rainWorld)
        {
            On.MoreSlugcats.CLOracleBehavior.Update += CLOracleBehavior_Update;
            On.MoreSlugcats.CLOracleBehavior.InitateConversation += CLOracleBehavior_InitateConversation;
            On.MoreSlugcats.CLOracleBehavior.InterruptRain += CLOracleBehavior_InterruptRain;
        }

        private void CLOracleBehavior_Update(On.MoreSlugcats.CLOracleBehavior.orig_Update orig, CLOracleBehavior self, bool eu)
        {
            if (!PlayerHooks.PlayerData.TryGetValue(PlayerEx.playerSelf, out var player) || !player.IsMoth)
            {
                orig(self, eu);
            }
            else
            {
                if (self.voice != null)
                {
                    self.voice.alive = true;
                    if (self.voice.slatedForDeletetion)
                    {
                        self.voice = null;
                    }
                }
                if (ModManager.MSC && self.oracle.room != null && self.oracle.room.game.rainWorld.safariMode)
                {
                    self.safariCreature = null;
                    float num = float.MaxValue;
                    for (int i = 0; i < self.oracle.room.abstractRoom.creatures.Count; i++)
                    {
                        if (self.oracle.room.abstractRoom.creatures[i].realizedCreature != null)
                        {
                            Creature realizedCreature = self.oracle.room.abstractRoom.creatures[i].realizedCreature;
                            float num2 = Custom.Dist(self.oracle.firstChunk.pos, realizedCreature.mainBodyChunk.pos);
                            if (num2 < num)
                            {
                                num = num2;
                                self.safariCreature = realizedCreature;
                            }
                        }
                    }
                }
                self.FindPlayer();
                if (!self.initiated)
                {
                    if (self.oracle.myScreen == null)
                    {
                        self.oracle.myScreen = new OracleProjectionScreen(self.oracle.room, self);
                    }
                    self.initiated = true;
                }
                if (self.dontHoldKnees > 0)
                {
                    self.dontHoldKnees--;
                }
                if (self.InSitPosition && self.oracle.arm != null)
                {
                    for (int i = 0; i < self.oracle.arm.joints.Length; i++)
                    {
                        if (self.oracle.arm.joints[i].vel.magnitude > 0.05f)
                        {
                            self.oracle.arm.joints[i].vel *= 0.98f;
                        }
                    }
                }
                if (self.FocusedOnHalcyon)
                {
                    self.halcyon.hoverPos = new Vector2?(self.oracle.firstChunk.pos + new Vector2(-40f, 5f));
                    self.lookPoint = self.halcyon.firstChunk.pos;
                }
                else if (self.halcyon != null)
                {
                    self.halcyon.hoverPos = null;
                    if (self.halcyon.room == null)
                    {
                        self.halcyon = null;
                        Debug.Log("halcyon pearl room nulled");
                    }
                }
                if (!self.oracle.Consious)
                {
                    return;
                }
                if (self.halcyon == null)
                {
                    for (int j = 0; j < self.oracle.room.physicalObjects.Length; j++)
                    {
                        for (int k = 0; k < self.oracle.room.physicalObjects[j].Count; k++)
                        {
                            if (self.oracle.room.physicalObjects[j][k] is HalcyonPearl)
                            {
                                self.halcyon = (self.oracle.room.physicalObjects[j][k] as HalcyonPearl);
                                break;
                            }
                        }
                        if (self.halcyon != null)
                        {
                            break;
                        }
                    }
                }
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
                if (!self.FocusedOnHalcyon && self.player != null && self.hasNoticedPlayer)
                {
                    self.lookPoint = self.player.DangerPos;
                }
                if (self.player != null && self.player.mainBodyChunk.pos.x >= 1430f && self.player.mainBodyChunk.pos.x <= 1660f && self.oracle.firstChunk.pos.x > self.player.mainBodyChunk.pos.x)
                {
                    self.timeOutOfSitZone = 0;
                }
                else
                {
                    self.timeOutOfSitZone++;
                }
                self.dehabilitateTime--;
                if (!self.hasNoticedPlayer)
                {
                    if (self.safariCreature != null)
                    {
                        self.lookPoint = self.safariCreature.mainBodyChunk.pos;
                    }
                    else if (self.InSitPosition)
                    {
                        self.lookPoint = self.oracle.firstChunk.pos + new Vector2(-145f, -45f);
                    }
                    else
                    {
                        self.lookPoint = self.OracleGetToPos;
                    }
                    if (self.player != null && self.player.room == self.oracle.room && self.player.mainBodyChunk.pos.x > 2248f)
                    {
                        self.hasNoticedPlayer = true;
                        self.oracle.firstChunk.vel += Custom.DegToVec(45f) * 3f;
                        self.oracle.bodyChunks[1].vel += Custom.DegToVec(-90f) * 2f;
                    }
                }
                else if (!self.saidHello)
                {
                    if (self.helloDelay < 40)
                    {
                        self.InitateConversation();
                        self.saidHello = true;
                    }
                    else
                    {
                        self.helloDelay++;
                    }
                }
                else if (self.hasNoticedPlayer && !self.rainInterrupt && self.player.room == self.oracle.room && self.oracle.room.world.rainCycle.TimeUntilRain < 1600 && self.oracle.room.world.rainCycle.pause < 1 && self.saidHello && self.noConversationTime >= 80 && self.player != null && !self.player.dead)
                {
                    self.InterruptRain();
                    self.rainInterrupt = true;
                    if (self.currentConversation != null)
                    {
                        self.currentConversation.Destroy();
                    }
                }
                else if (self.hasNoticedPlayer && self.player != null && self.saidHello)
                {
                    self.lookPoint = self.player.firstChunk.pos;
                    if (self.player.dead)
                    {
                        self.TalkToDeadPlayer();
                    }
                }
                if (self.currentConversation == null && (self.dialogBox == null || self.dialogBox.messages.Count == 0))
                {
                    self.noConversationTime++;
                }
                else
                {
                    self.noConversationTime = 0;
                }
                self.UpdateNormal(eu);
                self.oracle.arm.isActive = false;

                self.oracle.stun = Math.Max(self.oracle.stun, Random.Range(2, 4));
            }
        }

        private void CLOracleBehavior_InitateConversation(On.MoreSlugcats.CLOracleBehavior.orig_InitateConversation orig, CLOracleBehavior self)
        {
            if (!PlayerHooks.PlayerData.TryGetValue(PlayerEx.playerSelf, out var player) || !player.IsMoth)
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

        private void CLOracleBehavior_InterruptRain(On.MoreSlugcats.CLOracleBehavior.orig_InterruptRain orig, CLOracleBehavior self)
        {
            if (!PlayerHooks.PlayerData.TryGetValue(PlayerEx.playerSelf, out var player) || !player.IsMoth)
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

        static private CLOracleHooks _instance;
    }
}
