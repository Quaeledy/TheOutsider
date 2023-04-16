using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MoreSlugcats;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;
using System.Xml;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using SlugBase.DataTypes;
using Fisobs.Core;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using TheOutsider.World_Hooks;
using TheOutsider.Player_Hooks;
using TheOutsider.Oracle_Hooks;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace TheOutsider
{
    [BepInPlugin("Quaeledy.outsider", "The Outsider", "0.1.2")]
    public class Plugin : BaseUnityPlugin
    {
        static public readonly string MOD_ID = "Quaeledy.outsider";
        static public readonly string Name = "Outsider";

        // Add hooks
        public void OnEnable()
        {
            //On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            // Put your custom hooks here!
            On.RainWorld.OnModsInit += new On.RainWorld.hook_OnModsInit(this.RainWorld_OnModsInit);
        }

        public static readonly PlayerFeature<bool> MothOutsider = FeatureTypes.PlayerBool("TheOutsider/mothoutsider");
        public static readonly PlayerFeature<float> WingSpeed = FeatureTypes.PlayerFloat("TheOutsider/wingsspeed");
        public static readonly PlayerFeature<float> UpFlytime = FeatureTypes.PlayerFloat("TheOutsider/upflytime");
        public static readonly PlayerColor AntennaeColor = new PlayerColor("Antennae");
        public static readonly PlayerColor StripeColor = new PlayerColor("Stripes");
        
        private bool IsInit;

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig.Invoke(self);
            try
            {
                if (IsInit) return;
                IsInit = true;

                Futile.atlasManager.LoadAtlas("atlases/mothwings");
                Futile.atlasManager.LoadAtlas("atlases/mothantennaehead");
                Futile.atlasManager.LoadAtlas("atlases/mothcathands");

                MothEnums.RegisterValues();

                PlayerHooks.Init();
                PlayerGraphicsHooks.Init();
                FoodHooks.Init();
                RoomHooks.Init();
                
                _hooks = new List<HookBase>()
                {
                    HUDHooks.Instance(Logger),
                    RegionMergeFix.Instance(Logger),
                    CLOracleHooks.Instance(Logger),
                    SLOracleHooks.Instance(Logger)
                };
                
                foreach (var feature in _hooks)
                    feature.OnModsInit(self);
                
                Debug.Log($"Plugin {Plugin.MOD_ID} is loaded!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        static private List<HookBase> _hooks;


        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
        }
    }
}