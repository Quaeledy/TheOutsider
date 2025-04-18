using BepInEx;
using SlugBase.DataTypes;
using System;
using System.Linq;
using System.Security;
using System.Security.Permissions;
/*
using TheOutsider.CustomLore.CustomOracle;
using TheOutsider.CustomLore.CustomDream;*/
using TheOutsider.HUD_Hooks;
using TheOutsider.Menu_Hooks;
using TheOutsider.MothPup_Hooks;
using TheOutsider.Oracle_Hooks;
using TheOutsider.Player_Hooks;
using TheOutsider.PlayerGraphics_Hooks;
using TheOutsider.World_Hooks;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

//暂时隐藏了翠鸟羽毛，拉长了翠鸟身体
//暂时将翠鸟、翠鸟面具相关注册改成了false

namespace TheOutsider
{
    [BepInPlugin("Quaeledy.outsider", "The Outsider", "0.2.17")]
    public class Plugin : BaseUnityPlugin
    {
        public static readonly string MOD_ID = "Quaeledy.outsider";
        public static SlugcatStats.Name SlugName = new SlugcatStats.Name("Outsider", false);
        public static SlugcatStats.Name Mothpup = new SlugcatStats.Name("Mothpup", false);
        private bool IsInit;

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            //需要注册的变量
            TheOutsiderEnums.RegisterAllEnumExtensions();
            TheOutsiderEnums.RegisterAllFisobsContent();
            // Put your custom hooks here!
            On.RainWorld.OnModsInit += new On.RainWorld.hook_OnModsInit(this.RainWorld_OnModsInit);
            On.RainWorld.PostModsInit += new On.RainWorld.hook_PostModsInit(this.RainWorld_PostModsInit);
        }

        public static readonly PlayerColor AntennaeColor = new PlayerColor("Antennae");
        public static readonly PlayerColor LepidoticWingColor = new PlayerColor("LepidoticWing");
        public static readonly PlayerColor SpeckleColor = new PlayerColor("Speckles");
        public static readonly PlayerColor FlareColor = new PlayerColor("FlaringLight");
        public static OptionsMenu optionsMenuInstance;

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);

            if (IsInit) return;
            IsInit = true;

            try
            {
                OtherHooks.InitIL();
                WorldHooks.InitIL();
                MothPup_Hooks.PlayerHooks.InitIL();
                Player_Hooks.PlayerHooks.InitIL();

                //Remix配置菜单
                optionsMenuInstance = new OptionsMenu(this);
                MachineConnector.SetRegisteredOI(MOD_ID, optionsMenuInstance);

                //对其它mod的适配
                if (ModManager.ActiveMods.Any(mod => mod.id == "dressmyslugcat"))
                {
                    DressMySlugcatHooks.Init();
                }

                //我的hook们
                Player_Hooks.PlayerHooks.Init();
                PlayerGraphicsHooks.Init();
                FoodHooks.Init();
                WorldHooks.Init();
                HUDHooks.Init();
                RoomHooks.Init();
                RegionHooks.Init();
                CLOracleHooks.Init();
                SLOracleHooks.Init();
                HUDHooks.Init();
                JollyCoopHooks.Init();
                //蛾猫崽相关
                MothPup_Hooks.PlayerHooks.Init();
                MothPup_Hooks.SlugNPCAIHooks.Init();
                MothPup_Hooks.OtherHooks.Init();
                
                /*
                SceneHooks.Init();
                IntroRollHooks.Init();

                //基于EmgTx的内容
                CustomDreamRx.ApplyTreatment( new OutsiderDream());
                CustomOracleRx.ApplyTreatment(new AMOracleRegistry());
                */
                //DEBUG
                //On.RWCustom.Custom.Log += Custom_Log;

                UnityEngine.Debug.Log($"Plugin {Plugin.MOD_ID} is loaded!");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                throw;
            }
        }

        private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            if (ModManager.ActiveMods.Any(mod => mod.id == "slime-cubed.devconsole"))
            {
                DevConsoleHooks.Init();
            }
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadAtlas("atlases/mothantennae1");
            Futile.atlasManager.LoadAtlas("atlases/mothantennae2");
            Futile.atlasManager.LoadAtlas("atlases/mothwings");
            Futile.atlasManager.LoadAtlas("atlases/AlcedoFeather");
            Futile.atlasManager.LoadAtlas("atlases/AlcedoMask");
            Futile.atlasManager.LoadAtlas("atlases/AlcedoClaw");
            Futile.atlasManager.LoadAtlas("atlases/AlcedoScale");
            Futile.atlasManager.LoadAtlas("atlases/OutsiderGuidanceSlugcat");
            Futile.atlasManager.LoadAtlas("atlases/Kill_Slugcat_Outsider");
            Futile.atlasManager.LoadAtlas("atlases/icon_Quetzalcoatl");
            Futile.atlasManager.LoadAtlas("atlases/icon_MothPup");
            Futile.atlasManager.LoadAtlas("atlases/icon_Alcedo");
            Futile.atlasManager.LoadAtlas("atlases/icon_AlcedoMask");
            Log("Load Resources!");
        }

        public static void Log(string m)
        {
            UnityEngine.Debug.Log("[Outsider] " + m);
        }

        public static void Log(string f, params object[] args)
        {
            UnityEngine.Debug.Log("[Outsider] " + string.Format(f, args));
        }

        public static void Custom_Log(On.RWCustom.Custom.orig_Log orig, params string[] values)
        {
            orig(values);
            for (int i = 0; i < values.Length; i++)
            {
                UnityEngine.Debug.Log(string.Format(values[i]));
            }
        }
    }
}

/* json文件内容
"intro_slideshow": "Outsider_Intro",
*/