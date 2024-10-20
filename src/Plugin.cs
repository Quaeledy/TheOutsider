using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using System.Collections.Generic;
using SlugBase.Features;
using SlugBase.DataTypes;
using TheOutsider.World_Hooks;
using TheOutsider.Player_Hooks;
using TheOutsider.PlayerGraphics_Hooks;
using TheOutsider.Oracle_Hooks;
/*
using TheOutsider.CustomLore.CustomOracle;
using TheOutsider.CustomLore.CustomDream;
using TheOutsider.CustomLore.CustomCreature;
using TheOutsider.CustomOracleTx;
using TheOutsider.CustomDreamTx;*/
using TheOutsider.Menu_Hooks;
using TheOutsider.HUD_Hooks;
//using TheOutsider.MothPup;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace TheOutsider
{
    [BepInPlugin("Quaeledy.outsider", "The Outsider", "0.2.11")]
    public class Plugin : BaseUnityPlugin
    {
        static public readonly string MOD_ID = "Quaeledy.outsider";
        static public SlugcatStats.Name SlugName = new SlugcatStats.Name("Outsider", false);
        public static readonly SlugcatStats.Name MothPup = new SlugcatStats.Name("MothPup", false);
        private bool IsInit;

        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            // Put your custom hooks here!
            On.RainWorld.OnModsInit += new On.RainWorld.hook_OnModsInit(this.RainWorld_OnModsInit);
            //CreatureTemplateType.RegisterValues();
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
                //Remix配置菜单
                optionsMenuInstance = new OptionsMenu(this);
                MachineConnector.SetRegisteredOI(MOD_ID, optionsMenuInstance);

                //需要注册的变量
                MothEnums.RegisterValues();
                //CreatureTemplateType.RegisterValues();

                //我的hook们
                PlayerHooks.Init();
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
                //MothPupHooks.Init();
                /*
                SlugpupGraphics.Init();
                SlugpupStuff.Init();*/
                /*
                SceneHooks.Init();
                IntroRollHooks.Init();

                //基于EmgTx的内容
                CustomDreamRx.ApplyTreatment( new OutsiderDream());
                CustomOracleRx.ApplyTreatment(new AMOracleRegistry());
                */

                Debug.Log($"Plugin {Plugin.MOD_ID} is loaded!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }


        public static void Log(string m)
        {
            UnityEngine.Debug.Log("[Outsider] " + m);
        }

        public static void Log(string f, params object[] args)
        {
            UnityEngine.Debug.Log("[Outsider] " + string.Format(f, args));
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            Futile.atlasManager.LoadAtlas("atlases/mothantennae1");
            Futile.atlasManager.LoadAtlas("atlases/mothantennae2");
            Futile.atlasManager.LoadAtlas("atlases/mothwings");
            Futile.atlasManager.LoadAtlas("atlases/OutsiderGuidanceSlugcat");
            Futile.atlasManager.LoadAtlas("atlases/Kill_Slugcat_Outsider");
            Futile.atlasManager.LoadAtlas("icon_Quetzalcoatl");
            Futile.atlasManager.LoadAtlas("icon_MothPup");
        }
    }
}

/* json文件内容
"intro_slideshow": "Outsider_Intro",
*/