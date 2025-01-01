using Menu;
using System.IO;
using UnityEngine;

namespace TheOutsider.Player_Hooks
{
    public class SceneHooks
    {
        public static void Init()
        {
            //On.DreamsState.StaticEndOfCycleProgress += DreamsState_StaticEndOfCycleProgress;
            //On.Menu.DreamScreen.SceneFromDream += DreamScreen_SceneFromDream;
            On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
        }

        public static readonly DreamsState.DreamID moth1 = new DreamsState.DreamID("Dream_Sleep_Outsider1", true);
        public static readonly DreamsState.DreamID moth2 = new DreamsState.DreamID("Dream_Sleep_Outsider2", true);
        public static readonly DreamsState.DreamID moth3 = new DreamsState.DreamID("Dream_Sleep_Outsider3", true);
        public static readonly DreamsState.DreamID moth4 = new DreamsState.DreamID("Dream_Sleep_Outsider4", true);
        /*
        public static readonly MenuScene.SceneID Dream_Sleep_Outsider1 = new MenuScene.SceneID("Dream_Sleep_Outsider1", false);
        public static readonly MenuScene.SceneID Dream_Sleep_Outsider2 = new MenuScene.SceneID("Dream_Sleep_Outsider2", false);
        */
        /*
        private static void DreamsState_StaticEndOfCycleProgress(On.DreamsState.orig_StaticEndOfCycleProgress orig, SaveState saveState, string currentRegion, string denPosition, ref int cyclesSinceLastDream, ref int cyclesSinceLastFamilyDream, ref int cyclesSinceLastGuideDream, ref int inGWOrSHCounter, ref DreamsState.DreamID upcomingDream, ref DreamsState.DreamID eventDream, ref bool everSleptInSB, ref bool everSleptInSB_S01, ref bool guideHasShownHimselfToPlayer, ref int guideThread, ref bool guideHasShownMoonThisRound, ref int familyThread)
        {
            orig(saveState, currentRegion, denPosition, ref cyclesSinceLastDream, ref cyclesSinceLastFamilyDream, ref cyclesSinceLastGuideDream, ref inGWOrSHCounter, ref upcomingDream, ref eventDream, ref everSleptInSB, ref everSleptInSB_S01, ref guideHasShownHimselfToPlayer, ref guideThread, ref guideHasShownMoonThisRound, ref familyThread);

            if (!PlayerHooks.PlayerData.TryGetValue(TheOutsider.playerRef, out var player) || !player.isMoth)
            {
                return;
            }

            DreamsState.DreamID dreamID_moth = null;
            switch (familyThread)
            {
                case 0:
                    if (cyclesSinceLastFamilyDream > 0)
                    {
                        dreamID_moth = new DreamsState.DreamID("Dream_Sleep_Outsider1", false);
                    }
                    break;
                case 1:
                    if (cyclesSinceLastFamilyDream > 0)
                    {
                        dreamID_moth = new DreamsState.DreamID("Dream_Sleep_Outsider2", false);
                    }
                    break;
                case 2:
                    if (cyclesSinceLastFamilyDream > 0)
                    {
                        dreamID_moth = new DreamsState.DreamID("Dream_Sleep_Outsider3", false);
                    }
                    break;
                case 3:
                    if (cyclesSinceLastFamilyDream > 0)
                    {
                        dreamID_moth.value = "Dream_Sleep_Outsider4";
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
        
        */
        /*
        public static MenuScene.SceneID DreamScreen_SceneFromDream(On.Menu.DreamScreen.orig_SceneFromDream orig, DreamScreen self, DreamsState.DreamID dreamID)
        {
            if (dreamID == moth1)
            {
                return CustomScene.Dream_Sleep_Outsider1;
            }
            if (dreamID == moth2)
            {
                return CustomScene.Dream_Sleep_Outsider2;
            }
            orig(self, dreamID);
            return MenuScene.SceneID.Empty;
        }
        */

        private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, MenuScene self)
        {
            orig(self);

            if (self.sceneID.value == "Dream_Sleep_Outsider1" || self.sceneID.value == "Dream_Sleep_Outsider2")
            {
                self.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + "OutsiderDream - Sleep - Outsider";
                if (self.flatMode)
                {
                    self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "OutsiderDream - Sleep - Flat", new Vector2(683f, 384f), false, true));
                    if (self.sceneID == MenuScene.SceneID.Dream_Sleep_Fade)
                    {
                        self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, "Dream - Sleep - Flat2", new Vector2(683f, 384f), false, true));
                    }
                }
                else
                {
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - 5", new Vector2(71f, 49f), 5f, MenuDepthIllustration.MenuShader.Normal));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - 4", new Vector2(71f, 49f), 3f, MenuDepthIllustration.MenuShader.Lighten));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - LoneSlugcat", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - 3", new Vector2(71f, 49f), 2f, MenuDepthIllustration.MenuShader.Normal));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - 2", new Vector2(543f, 51f), 1.9f, MenuDepthIllustration.MenuShader.Lighten));
                    self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, "OutsiderDreamSleep - 1", new Vector2(475f, 126f), 1.8f, MenuDepthIllustration.MenuShader.Lighten));
                }
            }
        }
    }
}
