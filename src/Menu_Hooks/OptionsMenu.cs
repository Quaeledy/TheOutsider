using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace TheOutsider.Menu_Hooks
{
    public class OptionsMenu : OptionInterface
    {
        public OptionsMenu(Plugin plugin)
        {
            // Tab 1
            infiniteFlight = config.Bind("Quaeledy_Outsider_Bool_InfiniteFlight", false);
            immuneSporeCloud = config.Bind("Quaeledy_Outsider_Bool_ImmuneSporeCloud", false);
            neverFlare = config.Bind("Quaeledy_Outsider_Bool_NeverFlare", false);
            flareKeyCode[0] = config.Bind("Quaeledy_Outsider_KeyCode_FlareKeyCode1", new KeyCode());
            flareKeyCode[1] = config.Bind("Quaeledy_Outsider_KeyCode_FlareKeyCode2", new KeyCode());
            flareKeyCode[2] = config.Bind("Quaeledy_Outsider_KeyCode_FlareKeyCode3", new KeyCode());
            flareKeyCode[3] = config.Bind("Quaeledy_Outsider_KeyCode_FlareKeyCode4", new KeyCode());
            flyKeyCode[0] = config.Bind("Quaeledy_Outsider_KeyCode_FlyKeyCode1", new KeyCode());
            flyKeyCode[1] = config.Bind("Quaeledy_Outsider_KeyCode_FlyKeyCode2", new KeyCode());
            flyKeyCode[2] = config.Bind("Quaeledy_Outsider_KeyCode_FlyKeyCode3", new KeyCode());
            flyKeyCode[3] = config.Bind("Quaeledy_Outsider_KeyCode_FlyKeyCode4", new KeyCode());

            // Tab 2
            hideFlare = config.Bind("Quaeledy_Outsider_Bool_HideFlare", false);
            hideSwallowTail = config.Bind("Quaeledy_Outsider_Bool_HideSwallowTail", false);
            hideAntennae = config.Bind("Quaeledy_Outsider_Bool_HideAntennae", false);
            hideSpeckle = config.Bind("Quaeledy_Outsider_Bool_HideSpeckle", false);
            hideWing = config.Bind("Quaeledy_Outsider_Bool_HideWing", false);
            hideWingWhenFolded = config.Bind("Quaeledy_Outsider_Bool_HideWingWhenFolded", false);
            handWing = config.Bind("Quaeledy_Outsider_Bool_HandWing", false);

            // Tab 3
            allowMothPupInOtherTimeLine = config.Bind("Quaeledy_Outsider_Bool_AllowMothPupInOtherTimeLine", false);
            mothPupGenerationProbability = config.Bind("Quaeledy_Outsider_Int_MothPupGenerationProbability", 0);
        }
        public override void Initialize()
        {
            var opTab1 = new OpTab(this, Custom.rainWorld.inGameTranslator.Translate("Capacity"));//InGameTranslator.Translate("Capacity")
            var opTab2 = new OpTab(this, Custom.rainWorld.inGameTranslator.Translate("Appearance"));
            var opTab3 = new OpTab(this, Custom.rainWorld.inGameTranslator.Translate("Other"));
            Tabs = new[] { opTab1, opTab2, opTab3 };

            OpContainer tab1Container = new OpContainer(new Vector2(0, 0));
            opTab1.AddItems(tab1Container);
            // Tab 1：能力
            UIelement[] UIArrayElements = null;
            if (ModManager.CoopAvailable)
            {
                UIArrayElements = new UIelement[] //create an array of ui elements
                {
                new OpLabel(10f, 550f, Custom.rainWorld.inGameTranslator.Translate("Capacity"), true),
                new OpLabel(180f, 550f, Custom.rainWorld.inGameTranslator.Translate("These options mainly exist for the gaming experience.")),
                //飞行不消耗食物
                new OpCheckBox(infiniteFlight, 50, 500),
                new OpLabel(80, 500, Custom.rainWorld.inGameTranslator.Translate("Flight Does not Consume Food")),
                //免疫烟雾果
                new OpCheckBox(immuneSporeCloud, 50, 450),
                new OpLabel(80, 450, Custom.rainWorld.inGameTranslator.Translate("Immune Spore Cloud")),
                //禁用闪光
                new OpCheckBox(neverFlare, 50, 400),
                new OpLabel(80, 400, Custom.rainWorld.inGameTranslator.Translate("Disable Flare")),
                //自定义飞行按键
                new OpLabel(50, 290, Custom.rainWorld.inGameTranslator.Translate("Set a Key Code to Fly")),
                new OpKeyBinder(flyKeyCode[0], new Vector2(50,220), new Vector2(100,20)),
                new OpLabel(80, 260, Custom.rainWorld.inGameTranslator.Translate("Player 1")),
                new OpKeyBinder(flyKeyCode[1], new Vector2(180,220), new Vector2(100,20)),
                new OpLabel(210, 260, Custom.rainWorld.inGameTranslator.Translate("Player 2")),
                new OpKeyBinder(flyKeyCode[2], new Vector2(310,220), new Vector2(100,20)),
                new OpLabel(340, 260, Custom.rainWorld.inGameTranslator.Translate("Player 3")),
                new OpKeyBinder(flyKeyCode[3], new Vector2(440,220), new Vector2(100,20)),
                new OpLabel(470, 260, Custom.rainWorld.inGameTranslator.Translate("Player 4")),
                //自定义闪光按键
                new OpLabel(50, 150, Custom.rainWorld.inGameTranslator.Translate("Set a Key Code to Flare")),
                new OpKeyBinder(flareKeyCode[0], new Vector2(50,80), new Vector2(100,20)),
                new OpLabel(80, 120, Custom.rainWorld.inGameTranslator.Translate("Player 1")),
                new OpKeyBinder(flareKeyCode[1], new Vector2(180,80), new Vector2(100,20)),
                new OpLabel(210, 120, Custom.rainWorld.inGameTranslator.Translate("Player 2")),
                new OpKeyBinder(flareKeyCode[2], new Vector2(310,80), new Vector2(100,20)),
                new OpLabel(340, 120, Custom.rainWorld.inGameTranslator.Translate("Player 3")),
                new OpKeyBinder(flareKeyCode[3], new Vector2(440,80), new Vector2(100,20)),
                new OpLabel(470, 120, Custom.rainWorld.inGameTranslator.Translate("Player 4")),
                };
            }
            else
            {
                UIArrayElements = new UIelement[] //create an array of ui elements
                {
                new OpLabel(10f, 550f, Custom.rainWorld.inGameTranslator.Translate("Capacity"), true),
                new OpLabel(180f, 550f, Custom.rainWorld.inGameTranslator.Translate("These options mainly exist for the gaming experience.")),
                //飞行不消耗食物
                new OpCheckBox(infiniteFlight, 50, 500),
                new OpLabel(80, 500, Custom.rainWorld.inGameTranslator.Translate("Flight Does not Consume Food")),
                //免疫烟雾果
                new OpCheckBox(immuneSporeCloud, 50, 450),
                new OpLabel(80, 450, Custom.rainWorld.inGameTranslator.Translate("Immune Spore Cloud")),
                //禁用闪光
                new OpCheckBox(neverFlare, 50, 400),
                new OpLabel(80, 400, Custom.rainWorld.inGameTranslator.Translate("Disable Flare")),
                //自定义飞行按键
                new OpKeyBinder(flyKeyCode[0], new Vector2(50,250), new Vector2(120,20)),
                new OpLabel(50, 290, Custom.rainWorld.inGameTranslator.Translate("Set a Key Code to Fly")),
                //自定义闪光按键
                new OpKeyBinder(flareKeyCode[0], new Vector2(50,150), new Vector2(120,20)),
                new OpLabel(50, 190, Custom.rainWorld.inGameTranslator.Translate("Set a Key Code to Flare")),
                };
            }
            opTab1.AddItems(UIArrayElements);


            // Tab 2：外观
            OpContainer containerTab2 = new OpContainer(new Vector2(0, 0));
            opTab2.AddItems(containerTab2);
            UIArrayElements = new UIelement[]
            {
                    new OpLabel(10f, 550f, Custom.rainWorld.inGameTranslator.Translate("Appearance"), true),
                    new OpLabel(180f, 550f, Custom.rainWorld.inGameTranslator.Translate("These options mainly exist to dress up your slugcat.")),
                    //隐藏外观
                    new OpCheckBox(hideFlare, 50, 500),
                    new OpLabel(80, 500, Custom.rainWorld.inGameTranslator.Translate("Hide the Flaring Light")),
                    new OpCheckBox(hideSwallowTail, 50, 450),
                    new OpLabel(80, 450, Custom.rainWorld.inGameTranslator.Translate("Hide the Swallow Tail")),
                    new OpCheckBox(hideAntennae, 50, 400),
                    new OpLabel(80, 400, Custom.rainWorld.inGameTranslator.Translate("Hide the Antennae")),
                    new OpCheckBox(hideSpeckle, 50, 350),
                    new OpLabel(80, 350, Custom.rainWorld.inGameTranslator.Translate("Hide the Speckles")),
                    new OpCheckBox(hideWing, 50, 300),
                    new OpLabel(80, 300, Custom.rainWorld.inGameTranslator.Translate("Hide the Wing all the time")),
                    new OpCheckBox(hideWingWhenFolded, 50, 250),
                    new OpLabel(80, 250, Custom.rainWorld.inGameTranslator.Translate("Hide the Wing when it is folded")),
                    new OpCheckBox(handWing, 50, 200),
                    new OpLabel(80, 200, Custom.rainWorld.inGameTranslator.Translate("Replacing Wings with Hands (Chiroptera)")),
            };
            opTab2.AddItems(UIArrayElements);

            // Tab 3：其他
            OpContainer containerTab3 = new OpContainer(new Vector2(0, 0));
            opTab3.AddItems(containerTab3);
            UIArrayElements = new UIelement[]
            {
                    new OpLabel(10f, 550f, Custom.rainWorld.inGameTranslator.Translate("Other"), true),
                    new OpLabel(180f, 550f, Custom.rainWorld.inGameTranslator.Translate("Other settings about this mod.")),
                    //允许蛾猫崽在不是离群者的时间线生成
                    new OpCheckBox(allowMothPupInOtherTimeLine, 50, 500),
                    new OpLabel(80, 500, Custom.rainWorld.inGameTranslator.Translate("Allow Mothpups to generate in other campaigns that have slugpups")),
                    new OpSlider(mothPupGenerationProbability, new Vector2(50, 450), 100){max = 100, hideLabel = false, description = Custom.rainWorld.inGameTranslator.Translate("Mothpups generation probability (The probability of other pups turning into mothpups)")},
                    new OpLabel(180, 450, Custom.rainWorld.inGameTranslator.Translate("Mothpups generation probability in other campaigns that have slugpups")),
            };
            opTab3.AddItems(UIArrayElements);
        }

        public override void Update()
        {
            base.Update();
        }

        // Tab 1
        public readonly Configurable<bool> infiniteFlight;
        public readonly Configurable<bool> immuneSporeCloud;
        public readonly Configurable<bool> neverFlare;
        public readonly Configurable<KeyCode>[] flyKeyCode = new Configurable<KeyCode>[4];
        public readonly Configurable<KeyCode>[] flareKeyCode = new Configurable<KeyCode>[4];
        // Tab 2
        public readonly Configurable<bool> hideFlare;
        public readonly Configurable<bool> hideSwallowTail;
        public readonly Configurable<bool> hideAntennae;
        public readonly Configurable<bool> hideSpeckle;
        public readonly Configurable<bool> hideWing;
        public readonly Configurable<bool> hideWingWhenFolded;
        public readonly Configurable<bool> handWing;
        // Tab 3
        public readonly Configurable<bool> allowMothPupInOtherTimeLine;
        public readonly Configurable<int> mothPupGenerationProbability;
    }
}
