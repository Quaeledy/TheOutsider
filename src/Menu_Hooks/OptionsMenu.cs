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
            flareKeyCode = config.Bind("Quaeledy_Outsider_KeyCode_FlareKeyCode", new KeyCode());
            flyKeyCode = config.Bind("Quaeledy_Outsider_KeyCode_FlyKeyCode", new KeyCode());

            // Tab 2
            hideFlare = config.Bind("Quaeledy_Outsider_Bool_HideFlare", false);
            hideSwallowTail = config.Bind("Quaeledy_Outsider_Bool_HideSwallowTail", false);
            hideAntennae = config.Bind("Quaeledy_Outsider_Bool_HideAntennae", false);
            hideSpeckle = config.Bind("Quaeledy_Outsider_Bool_HideSpeckle", false);
            hideWing = config.Bind("Quaeledy_Outsider_Bool_HideWing", false);
            hideWingWhenFolded = config.Bind("Quaeledy_Outsider_Bool_HideWingWhenFolded", false);
            handWing = config.Bind("Quaeledy_Outsider_Bool_HandWing", false);
        }
        public override void Initialize()
        {
            var opTab1 = new OpTab(this, Custom.rainWorld.inGameTranslator.Translate("Capacity"));//InGameTranslator.Translate("Capacity")
            var opTab2 = new OpTab(this, Custom.rainWorld.inGameTranslator.Translate("Appearance"));
            Tabs = new[] { opTab1, opTab2 };

            OpContainer tab1Container = new OpContainer(new Vector2(0, 0));
            opTab1.AddItems(tab1Container);

            // Tab 1：能力
            UIelement[] UIArrayElements = new UIelement[] //create an array of ui elements
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
                new OpKeyBinder(flyKeyCode, new Vector2(50,250), new Vector2(120,20)),
                new OpLabel(50, 290, Custom.rainWorld.inGameTranslator.Translate("Set a Key Code to Fly")),
                //自定义闪光按键
                new OpKeyBinder(flareKeyCode, new Vector2(50,150), new Vector2(120,20)),
                new OpLabel(50, 190, Custom.rainWorld.inGameTranslator.Translate("Set a Key Code to Flare")),
            };
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
        }

        public override void Update()
        {
            base.Update();
        }

        // Tab 1
        public readonly Configurable<bool> infiniteFlight;
        public readonly Configurable<bool> immuneSporeCloud;
        public readonly Configurable<bool> neverFlare;
        public readonly Configurable<KeyCode> flareKeyCode;
        public readonly Configurable<KeyCode> flyKeyCode;
        // Tab 2
        public readonly Configurable<bool> hideFlare;
        public readonly Configurable<bool> hideSwallowTail;
        public readonly Configurable<bool> hideAntennae;
        public readonly Configurable<bool> hideSpeckle;
        public readonly Configurable<bool> hideWing;
        public readonly Configurable<bool> hideWingWhenFolded;
        public readonly Configurable<bool> handWing;
    }
}
