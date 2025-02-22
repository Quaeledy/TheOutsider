using CustomDreamTx;
using Menu;
using RWCustom;

namespace TheOutsider.CustomLore.CustomDream
{

    public class OutsiderDream : CustomNormalDreamTx
    {
        public static readonly DreamsState.DreamID OutsiderDream_0 = new DreamsState.DreamID("OutsiderDream_0", true);
        public static readonly DreamsState.DreamID OutsiderDream_1 = new DreamsState.DreamID("OutsiderDream_1", true);
        public static readonly DreamsState.DreamID OutsiderDream_2 = new DreamsState.DreamID("OutsiderDream_2", true);
        public static readonly DreamsState.DreamID OutsiderDream_3 = new DreamsState.DreamID("OutsiderDream_3", true);
        public static readonly DreamsState.DreamID OutsiderDream_4 = new DreamsState.DreamID("OutsiderDream_4", true);
        public static readonly DreamsState.DreamID OutsiderDream_5 = new DreamsState.DreamID("OutsiderDream_5", true);

        public static readonly MenuScene.SceneID Dream_Sleep_Outsider1 = new MenuScene.SceneID("Dream_Sleep_Outsider1", false);
        public static readonly MenuScene.SceneID Dream_Sleep_Outsider2 = new MenuScene.SceneID("Dream_Sleep_Outsider2", false);

        public OutsiderDream() : base(Plugin.SlugName)
        {
        }

        public override void DecideDreamID(
            SaveState saveState,
            string currentRegion,
            string denPosition,
            ref int cyclesSinceLastDream,
            ref int cyclesSinceLastFamilyDream,
            ref int cyclesSinceLastGuideDream,
            ref int inGWOrSHCounter,
            ref DreamsState.DreamID upcomingDream,
            ref DreamsState.DreamID eventDream,
            ref bool everSleptInSB,
            ref bool everSleptInSB_S01,
            ref bool guideHasShownHimselfToPlayer,
            ref int guideThread,
            ref bool guideHasShownMoonThisRound,
            ref int familyThread)
        {
            if (dreamFinished) return;

            upcomingDream = null;
            cyclesSinceLastFamilyDream = 0;//屏蔽FamilyDream计数，防止被原本的方法干扰

            Plugin.Log("[Outsider] DreamState : cycleSinceLastDream{0},FamilyThread{1}", cyclesSinceLastDream, familyThread);

            switch (familyThread)
            {
                case 0:
                    if (saveState.cycleNumber >= 0 && cyclesSinceLastDream > 0)
                    {
                        saveState.deathPersistentSaveData.theMark = false;
                        upcomingDream = OutsiderDream_0;
                    }
                    break;
                case 1:
                    if (cyclesSinceLastDream > 0)
                        upcomingDream = OutsiderDream_1;
                    break;
                case 2:
                    if (cyclesSinceLastDream > 0)
                        upcomingDream = OutsiderDream_2;
                    break;
                case 3:
                    if (cyclesSinceLastDream > 0)
                        upcomingDream = OutsiderDream_3;
                    break;
                case 4:
                    if (cyclesSinceLastDream > 0)
                        upcomingDream = OutsiderDream_4;
                    break;
                case 5:
                    if (cyclesSinceLastDream > 0)
                        upcomingDream = OutsiderDream_5;
                    break;
            }
            if (upcomingDream != null)
            {
                familyThread++;
                cyclesSinceLastDream = 0;
            }
        }

        public override bool IsPerformDream => (activateDreamID == OutsiderDream_0 || activateDreamID == OutsiderDream_5);

        public override CustomDreamRx.BuildDreamWorldParams GetBuildDreamWorldParams()
        {
            if (activateDreamID == OutsiderDream_0 ||
                activateDreamID == OutsiderDream_5)
            {
                return new CustomDreamRx.BuildDreamWorldParams()
                {
                    firstRoom = "OSAM_AI",//OSAM_AI
                    singleRoomWorld = false,

                    overridePlayerPos = new IntVector2(29, 33),

                    playAs = Plugin.SlugName,
                };
            }
            else if (activateDreamID == OutsiderDream_1 ||
                     activateDreamID == OutsiderDream_2 ||
                     activateDreamID == OutsiderDream_3 ||
                     activateDreamID == OutsiderDream_4)
            {
                return null;
            }
            else
            {
                return null;
            }
        }

        public override MenuScene.SceneID SceneFromDream(DreamsState.DreamID dreamID)
        {
            if (dreamID == OutsiderDream_1)
            {
                return Dream_Sleep_Outsider1;
            }
            if (dreamID == OutsiderDream_2)
            {
                return Dream_Sleep_Outsider2;
            }
            if (dreamID == OutsiderDream_3)
            {
                return Dream_Sleep_Outsider1;
            }
            if (dreamID == OutsiderDream_4)
            {
                return Dream_Sleep_Outsider2;
            }
            return MenuScene.SceneID.Empty;
        }
    }
}
