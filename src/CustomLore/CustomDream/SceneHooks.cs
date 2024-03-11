using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Menu;
using RWCustom;
using SlugBase.Features;
using SlugBase.Assets;
using UnityEngine.PlayerLoop;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Runtime.CompilerServices;

namespace TheOutsider.CustomLore.CustomDream
{
    public class SceneHooks
    {
        public static void Init()
        {
            //On.DreamsState.StaticEndOfCycleProgress += DreamsState_StaticEndOfCycleProgress;
            //On.Menu.DreamScreen.SceneFromDream += DreamScreen_SceneFromDream;
            On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
            On.Menu.InteractiveMenuScene.Update += InteractiveMenuScene_Update;
            /*On.Menu.SlideShow.ctor += SlideShow_ctor;
             * On.Menu.MenuDepthIllustration.ctor += MenuDepthIllustration_ctor;
            On.Menu.SlideShow.CommunicateWithUpcomingProcess += SlideShow_CommunicateWithUpcomingProcess;
            On.Menu.SlideShowMenuScene.ApplySceneSpecificAlphas += SlideShowMenuScene_ApplySceneSpecificAlphas;*/
        }

        public static ConditionalWeakTable<WeakReference<Player>, PlayerEx> PlayerData = new();

        public static readonly SlideShow.SlideShowID OutsiderIntro = new SlideShow.SlideShowID("OutsiderIntro", true);
        /*
        public static readonly DreamsState.DreamID moth1 = new DreamsState.DreamID("Dream_Sleep_Outsider1", true);
        public static readonly DreamsState.DreamID moth2 = new DreamsState.DreamID("Dream_Sleep_Outsider2", true);
        */
        public static readonly MenuScene.SceneID Dream_Sleep_Outsider1 = new MenuScene.SceneID("Dream_Sleep_Outsider1", false);
        public static readonly MenuScene.SceneID Dream_Sleep_Outsider2 = new MenuScene.SceneID("Dream_Sleep_Outsider2", false);
        /*
        private static void DreamsState_StaticEndOfCycleProgress(On.DreamsState.orig_StaticEndOfCycleProgress orig, SaveState saveState, string currentRegion, string denPosition, ref int cyclesSinceLastDream, ref int cyclesSinceLastFamilyDream, ref int cyclesSinceLastGuideDream, ref int inGWOrSHCounter, ref DreamsState.DreamID upcomingDream, ref DreamsState.DreamID eventDream, ref bool everSleptInSB, ref bool everSleptInSB_S01, ref bool guideHasShownHimselfToPlayer, ref int guideThread, ref bool guideHasShownMoonThisRound, ref int familyThread)
        {
            orig(saveState, currentRegion, denPosition, ref cyclesSinceLastDream, ref cyclesSinceLastFamilyDream, ref cyclesSinceLastGuideDream, ref inGWOrSHCounter, ref upcomingDream, ref eventDream, ref everSleptInSB, ref everSleptInSB_S01, ref guideHasShownHimselfToPlayer, ref guideThread, ref guideHasShownMoonThisRound, ref familyThread);

            if (!PlayerData.TryGetValue(PlayerEx.playerRef, out var player) || !player.IsOutsider)
            {
                return;
            }

            
            DreamsState.DreamID dreamID_moth = null;
            switch (familyThread)
            {
                case 0:
                    if (cyclesSinceLastFamilyDream > 5)
                    {
                        dreamID_moth = moth1;
                    }
                    break;
                case 1:
                    if (cyclesSinceLastFamilyDream > 5)
                    {
                        dreamID_moth = moth2;
                    }
                    break;
            }
            if (dreamID_moth != null)
            {
                familyThread++;
                upcomingDream = dreamID_moth;
                cyclesSinceLastDream = 0;
                cyclesSinceLastFamilyDream = 0;
                return;
            }
        }
        
        private static MenuScene.SceneID DreamScreen_SceneFromDream(On.Menu.DreamScreen.orig_SceneFromDream orig, DreamScreen self, DreamsState.DreamID dreamID)
        {
            if (dreamID == moth1)
            {
                return Dream_Sleep_Outsider1;
            }
            if (dreamID == moth2)
            {
                return Dream_Sleep_Outsider2;
            }
            orig(self, dreamID);
            return MenuScene.SceneID.Empty;
        }*/

        private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
        {
            orig(self);

            if (self.sceneID.value == "Dream_Sleep_Outsider1" || self.sceneID.value == "Dream_Sleep_Outsider2")
            {
                self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "Dream - Sleep - Outsider";
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "OutsiderDream - Sleep - Flat", new Vector2(683f, 384f), false, true));
                    if (self.sceneID.value == "Dream_Sleep_Outsider2")
                    {
                        self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "OutsiderDream - Sleep - Flat2", new Vector2(683f, 384f), false, true));
                    }
                }
                else
                {
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - 5", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - 4", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Lighten));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - LoneSlugcat", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - 3", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - 2", new Vector2(-50f, 0f), 1.9f, MenuDepthIllustration.MenuShader.Lighten));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - 1", new Vector2(475f, 126f), 1.8f, MenuDepthIllustration.MenuShader.Lighten));
                }
            }
        }
        /*
        private static void SlideShow_ctor(On.Menu.SlideShow.orig_ctor orig, SlideShow self, ProcessManager manager, SlideShow.SlideShowID slideShowID)
        {
            orig(self, manager, slideShowID);
            if (slideShowID.value == "OutsiderIntro")
            {
                if (manager.musicPlayer != null)
                {
                    self.waitForMusic = "RW_Intro_Theme";
                    self.stall = true;
                    manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 40f);
                }
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_1_Tree, self.ConvertTime(0, 0, 20), self.ConvertTime(0, 3, 26), self.ConvertTime(0, 8, 6)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, self.ConvertTime(0, 9, 6), 0f, 0f));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_2_Branch, self.ConvertTime(0, 9, 19), self.ConvertTime(0, 10, 19), self.ConvertTime(0, 16, 2)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_3_In_Tree, self.ConvertTime(0, 17, 21), self.ConvertTime(0, 18, 10), self.ConvertTime(0, 24, 3)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_4_Walking, self.ConvertTime(0, 24, 26), self.ConvertTime(0, 25, 19), self.ConvertTime(0, 32, 2)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, self.ConvertTime(0, 34, 6), 0f, 0f));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_5_Hunting, self.ConvertTime(0, 35, 50), self.ConvertTime(0, 36, 54), self.ConvertTime(0, 42, 15)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_6_7_Rain_Drop, self.ConvertTime(0, 43, 0), self.ConvertTime(0, 44, 0), self.ConvertTime(0, 49, 29)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_8_Climbing, self.ConvertTime(0, 50, 19), self.ConvertTime(0, 51, 9), self.ConvertTime(0, 55, 21)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, self.ConvertTime(0, 56, 24), 0f, 0f));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_9_Rainy_Climb, self.ConvertTime(0, 57, 2), self.ConvertTime(0, 57, 80), self.ConvertTime(1, 1, 1)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_10_Fall, self.ConvertTime(1, 1, 1), self.ConvertTime(1, 1, 1), self.ConvertTime(1, 4, 0)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_10_5_Separation, self.ConvertTime(1, 4, 29), self.ConvertTime(1, 5, 18), self.ConvertTime(1, 11, 10)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_11_Drowning, self.ConvertTime(1, 11, 25), self.ConvertTime(1, 12, 10), self.ConvertTime(1, 17, 28)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_12_Waking, self.ConvertTime(1, 19, 2), self.ConvertTime(1, 20, 6), self.ConvertTime(1, 21, 29)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_13_Alone, self.ConvertTime(1, 22, 22), self.ConvertTime(1, 23, 15), self.ConvertTime(1, 26, 24)));

                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Intro_14_Title, self.ConvertTime(1, 27, 24), self.ConvertTime(1, 31, 34), self.ConvertTime(1, 33, 60)));
                for (int i = 1; i < self.playList.Count; i++)
                {
                    self.playList[i].startAt += 0.6f;
                    self.playList[i].fadeInDoneAt += 0.6f;
                    self.playList[i].fadeOutStartAt += 0.6f;
                }
                self.processAfterSlideShow = ProcessManager.ProcessID.Game;
            }
            else if (slideShowID.value == "OutsiderOutro")
            {
                if (manager.musicPlayer != null)
                {
                    self.waitForMusic = "RW_Outro_Theme";
                    self.stall = true;
                    manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 10f);
                }
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, 0f, 0f, 0f));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Outro_1_Left_Swim, self.ConvertTime(0, 1, 20), self.ConvertTime(0, 5, 0), self.ConvertTime(0, 17, 0)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Outro_2_Up_Swim, self.ConvertTime(0, 21, 0), self.ConvertTime(0, 25, 0), self.ConvertTime(0, 37, 0)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Outro_3_Face, self.ConvertTime(0, 41, 10), self.ConvertTime(0, 45, 20), self.ConvertTime(0, 46, 60)));
                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Outro_4_Tree, self.ConvertTime(0, 48, 20), self.ConvertTime(0, 51, 0), self.ConvertTime(0, 55, 0)));

                self.playList.Add(new SlideShow.Scene(MenuScene.SceneID.Empty, self.ConvertTime(1, 1, 0), self.ConvertTime(1, 1, 0), self.ConvertTime(1, 6, 0)));
                for (int j = 1; j < self.playList.Count; j++)
                {
                    self.playList[j].startAt -= 1.1f;
                    self.playList[j].fadeInDoneAt -= 1.1f;
                    self.playList[j].fadeOutStartAt -= 1.1f;
                }

                self.processAfterSlideShow = ProcessManager.ProcessID.Credits;

                if (ModManager.MSC)
                {
                    manager.statsAfterCredits = true;
                }
            }
        }
        
        private static void SlideShow_CommunicateWithUpcomingProcess(On.Menu.SlideShow.orig_CommunicateWithUpcomingProcess orig, SlideShow self, MainLoopProcess nextProcess)
        {
            orig(self, nextProcess);
            if (nextProcess is EndCredits && (self.slideShowID.value == "OutsiderOutro"))
            {
                self.manager.CueAchievement(RainWorld.AchievementID.Win, 5f);
            }
        }
        
        private static void SlideShowMenuScene_ApplySceneSpecificAlphas(On.Menu.SlideShowMenuScene.orig_ApplySceneSpecificAlphas orig, SlideShowMenuScene self)
        {
            orig(self);
            float num;
            if (self.sceneID == MenuScene.SceneID.Intro_6_7_Rain_Drop)
            {
                num = Mathf.InverseLerp(0.52f, 0.58f, self.displayTime);
                if (self.flatMode)
                {
                    self.flatIllustrations[1].setAlpha = new float?(num);
                    return;
                }
                float p = 0.5f;
                self.depthIllustrations[0].setAlpha = new float?(Mathf.Pow(1f - num, p));
                self.depthIllustrations[1].setAlpha = new float?(Mathf.Pow(num, p));
                self.depthIllustrations[2].setAlpha = new float?(Mathf.Pow(1f - num, p));
                self.depthIllustrations[3].setAlpha = new float?(Mathf.Pow(num, p));
                self.depthIllustrations[4].setAlpha = new float?(Mathf.Pow(num, p));
            }
            else if (self.sceneID == MenuScene.SceneID.Intro_9_Rainy_Climb)
            {
                if (!self.flatMode)
                {
                    self.depthIllustrations[self.depthIllustrations.Count - 1].setAlpha = new float?(Mathf.InverseLerp(0.96f, 0.99f, self.displayTime));
                    return;
                }
            }
            else if (self.sceneID == MenuScene.SceneID.Intro_10_Fall)
            {
                if (!self.flatMode)
                {
                    self.depthIllustrations[self.depthIllustrations.Count - 1].setAlpha = new float?(Mathf.InverseLerp(0.04f, 0.01f, self.displayTime));
                    return;
                }
            }
            else if (self.sceneID == MenuScene.SceneID.Outro_4_Tree)
            {
                if (!self.flatMode)
                {
                    self.depthIllustrations[self.depthIllustrations.Count - 2].setAlpha = new float?(Mathf.InverseLerp(0.22f, 0.44f, self.displayTime));
                    self.depthIllustrations[self.depthIllustrations.Count - 3].setAlpha = new float?(Mathf.InverseLerp(0.21f, 0.45f, self.displayTime));
                    return;
                }
            }
            else
            {
                if (self.sceneID == MenuScene.SceneID.Intro_14_Title)
                {
                    self.flatIllustrations[self.flatIllustrations.Count - 1].setAlpha = new float?(Mathf.InverseLerp(1f, 0.7f, self.displayTime));
                    return;
                }
                if (self.sceneID == MenuScene.SceneID.Yellow_Intro_A)
                {
                    num = Mathf.InverseLerp(0.51f, 0.55f, self.displayTime);
                    if (self.flatMode)
                    {
                        self.flatIllustrations[1].setAlpha = new float?(num);
                        return;
                    }
                    self.depthIllustrations[self.depthIllustrations.Count - 2].alpha = Mathf.Pow(num, 1.5f) * 0.65f;
                    self.depthIllustrations[self.depthIllustrations.Count - 1].alpha = Mathf.Pow(num, 0.5f);
                    return;
                }
                else if (self.sceneID == MenuScene.SceneID.Yellow_Intro_B)
                {
                    if (!self.flatMode)
                    {
                        self.depthIllustrations[1].alpha = Custom.LerpMap(self.displayTime, 0.1f, 0.9f, 1f, 0.2f, 0.7f);
                        self.depthIllustrations[2].alpha = Custom.LerpMap(self.displayTime, 0.1f, 0.5f, 0.5f, 1f, 1.2f);
                        return;
                    }
                }
            }
            return;
        }
        */
        private static void InteractiveMenuScene_Update(On.Menu.InteractiveMenuScene.orig_Update orig, InteractiveMenuScene self)
        {
            orig(self);

            if (self.sceneID.value == "Dream_Sleep_Outsider2")
            {
                if (self.timer < 0)
                {
                    self.timer++;
                }
                float num = Custom.SCurve(Mathf.InverseLerp(60f, 160f, self.timer), 0.6f);
                if (self.flatMode)
                {
                    self.flatIllustrations[1].setAlpha = new float?(num);
                }
                else
                {
                    self.depthIllustrations[self.depthIllustrations.Count - 1].setAlpha = new float?(0.1f + Mathf.Pow(1f - num, 0.5f) * 0.6f);
                    self.depthIllustrations[self.depthIllustrations.Count - 2].setAlpha = new float?(Mathf.Pow(1f - num, 0.5f));
                    self.depthIllustrations[self.depthIllustrations.Count - 3].setAlpha = new float?(Mathf.Pow(1f - num, 0.5f));
                    self.depthIllustrations[self.depthIllustrations.Count - 4].setAlpha = new float?(Mathf.Pow(num, 0.5f));
                    self.depthIllustrations[self.depthIllustrations.Count - 5].setAlpha = new float?(0.5f + 0.5f * Mathf.Pow(1f - num, 0.5f));
                    self.depthIllustrations[self.depthIllustrations.Count - 6].setAlpha = new float?(Mathf.Pow(1f - num, 0.5f));
                }
            }
        }
        /*
        private static void MenuDepthIllustration_ctor(On.Menu.MenuDepthIllustration.orig_ctor orig, MenuDepthIllustration self, Menu.Menu menu, MenuObject owner, string folderName, string fileName, Vector2 pos, float depth, MenuDepthIllustration.MenuShader shader)
        {
            orig(self, menu, owner, folderName, fileName, pos, depth, shader);
            var MenuScene = owner as MenuScene;
            if (MenuScene != null && MenuScene.sceneID == Outro_Outsider_5_Reunion)
            {
                self.sprite.scaleX /= 2.8f;
                self.sprite.scaleY /= 2.8f;
            }
        }
        */
        /*
        public static void AddIntroSlideShow(string name, string music, SlideShow.SlideShowID ID, Action<SlideShow> buildSlideAction)
        {
            SceneHooks.OnModsInit();
            SlideShowArg item;
            item.name = name;
            item.music = music;
            item.id = ID;
            item.buildSlideAction = buildSlideAction;
            SceneHooks.slideIntroArgs.Add(item);
        }

        public static void AddOutroSlideShow(string name, string music, SlideShow.SlideShowID ID, Action<SlideShow> buildSlideAction)
        {
            SceneHooks.OnModsInit();
            SlideShowArg item;
            item.name = name;
            item.music = music;
            item.id = ID;
            item.buildSlideAction = buildSlideAction;
            SceneHooks.slideOutroArgs.Add(item);
        }

        public static void AddScene(MenuScene.SceneID id, Action<MenuScene> action)
        {
            SceneHooks.OnModsInit();
            SceneHooks.sceneArgs.Add(id, action);
        }

        private static void OnModsInit()
        {
            bool flag = !SceneHooks.loaded;
            if (flag)
            {
                IL.Menu.SlugcatSelectMenu.StartGame += SlugcatSelectMenu_StartGameIL;
                IL.RainWorldGame.ExitToVoidSeaSlideShow += RainWorldGame_ExitToVoidSeaSlideShowIL;
                On.Menu.SlideShow.ctor += SlideShow_ctor;
                On.Menu.SlideShow.NextScene += SlideShow_NextScene;
                On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
                SceneHooks.loaded = true;
            }
        }

        private static void RainWorldGame_ExitToVoidSeaSlideShowIL(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            ILCursor ilcursor2 = ilcursor;
            MoveType moveType = (MoveType)2;
            Func<Instruction, bool>[] array = new Func<Instruction, bool>[2];
            array[0] = ((Instruction i) => ILPatternMatchingExt.MatchLdfld<MainLoopProcess>(i, "manager"));
            array[1] = ((Instruction i) => ILPatternMatchingExt.MatchLdsfld<SlideShow.SlideShowID>(i, "WhiteOutro"));
            bool flag = ilcursor2.TryGotoNext(moveType, array);
            if (flag)
            {
                ilcursor.Emit(OpCodes.Ldarg_0);
                ilcursor.EmitDelegate<Func<SlideShow.SlideShowID, RainWorldGame, SlideShow.SlideShowID>>(delegate (SlideShow.SlideShowID id, RainWorldGame game)
                {
                    foreach (SlideShowArg slideShowArg in SceneHooks.slideOutroArgs)
                    {
                        bool flag2 = slideShowArg.name == game.session.characterStats.name.value;
                        if (flag2)
                        {
                            return slideShowArg.id;
                        }
                    }
                    return id;
                });
            }
        }

        private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
        {
            orig.Invoke(self);
            bool flag = self.sceneID != null && SceneHooks.sceneArgs.ContainsKey(self.sceneID);
            if (flag)
            {
                SceneHooks.sceneArgs[self.sceneID](self);
            }
        }

        private static void SlideShow_NextScene(On.Menu.SlideShow.orig_NextScene orig, SlideShow self)
        {
            bool flag = self.preloadedScenes.Length == 0;
            if (!flag)
            {
                orig.Invoke(self);
            }
        }

        private static void SlideShow_ctor(On.Menu.SlideShow.orig_ctor orig, SlideShow self, ProcessManager manager, SlideShow.SlideShowID slideShowID)
        {
            try
            {
                foreach (SlideShowArg slideShowArg in SceneHooks.slideIntroArgs)
                {
                    bool flag = slideShowArg.id == slideShowID;
                    if (flag)
                    {
                        self.waitForMusic = slideShowArg.music;
                        self.stall = true;
                        manager.musicPlayer.MenuRequestsSong(self.waitForMusic, 1.5f, 10f);
                        break;
                    }
                }
            }
            catch
            {
                Debug.LogError("[Nutils] Yeah slide show has some bugs, but i don't want to fix");
            }
            orig.Invoke(self, manager, slideShowID);
            try
            {
                self.processAfterSlideShow = ProcessManager.ProcessID.Game;
                foreach (SlideShowArg slideShowArg2 in SceneHooks.slideIntroArgs)
                {
                    bool flag2 = slideShowArg2.id == slideShowID;
                    if (flag2)
                    {
                        bool flag3 = slideShowArg2.buildSlideAction == null;
                        if (flag3)
                        {
                            return;
                        }
                        slideShowArg2.buildSlideAction(self);
                        self.preloadedScenes = new SlideShowMenuScene[self.playList.Count];
                        for (int i = 0; i < self.preloadedScenes.Length; i++)
                        {
                            self.preloadedScenes[i] = new SlideShowMenuScene(self, self.pages[0], self.playList[i].sceneID);
                            self.preloadedScenes[i].Hide();
                        }
                        break;
                    }
                }
            }
            catch
            {
                Debug.LogError("[Nutils] Yeah slide show has some bugs, but i don't want to fix");
            }
            self.NextScene();
        }

        private static void SlugcatSelectMenu_StartGameIL(ILContext il)
        {
            ILCursor ilcursor = new ILCursor(il);
            ILCursor ilcursor2 = ilcursor;
            MoveType moveType = (MoveType)2;
            Func<Instruction, bool>[] array = new Func<Instruction, bool>[2];
            array[0] = ((Instruction i) => ILPatternMatchingExt.MatchLdfld<MainLoopProcess>(i, "manager"));
            array[1] = ((Instruction i) => ILPatternMatchingExt.MatchLdsfld<ProcessManager.ProcessID>(i, "Game"));
            bool flag = ilcursor2.TryGotoNext(moveType, array);
            if (flag)
            {
                ilcursor.Emit(OpCodes.Ldarg_0);
                ilcursor.Emit(OpCodes.Ldarg_1);
                ilcursor.EmitDelegate<Func<ProcessManager.ProcessID, SlugcatSelectMenu, SlugcatStats.Name, ProcessManager.ProcessID>>(delegate (ProcessManager.ProcessID id, SlugcatSelectMenu self, SlugcatStats.Name name)
                {
                    foreach (SlideShowArg slideShowArg in SceneHooks.slideIntroArgs)
                    {
                        bool flag2 = slideShowArg.name == name.value;
                        if (flag2)
                        {
                            self.manager.nextSlideshow = slideShowArg.id;
                            return ProcessManager.ProcessID.SlideShow;
                        }
                    }
                    return id;
                });
            }
        }

        private static bool loaded = false;
        private static List<SlideShowArg> slideIntroArgs = new List<SlideShowArg>();
        private static List<SlideShowArg> slideOutroArgs = new List<SlideShowArg>();
        private static Dictionary<MenuScene.SceneID, Action<MenuScene>> sceneArgs = new Dictionary<MenuScene.SceneID, Action<MenuScene>>();
        public struct SlideShowArg
        {
            public string music;
            public string name;
            public SlideShow.SlideShowID id;
            public Action<SlideShow> buildSlideAction;
        }
        */
    }
}
