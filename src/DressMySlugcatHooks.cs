using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TheOutsider
{
    internal class DressMySlugcatHooks
    {
        private static BindingFlags propFlags = BindingFlags.Static | BindingFlags.Public;
        private static BindingFlags methodFlags = BindingFlags.Static | BindingFlags.NonPublic;
        public delegate List<string> orig_DressMySlugcat_Utils_ValidSlugcatNames();
        public delegate Color orig_DressMySlugcat_Utils_DefaultColorForSprite(string slugcat, string sprite);

        public static void Init()
        {
            DressMySlugcatForOutsider();
            Hook hook1 = new Hook(typeof(DressMySlugcat.Utils).GetProperty(nameof(DressMySlugcat.Utils.ValidSlugcatNames), DressMySlugcatHooks.propFlags).GetGetMethod(),
                                 typeof(DressMySlugcatHooks).GetMethod(nameof(DressMySlugcat_Utils_ValidSlugcatNames), DressMySlugcatHooks.methodFlags));
            Hook hook2 = new Hook(typeof(DressMySlugcat.Utils).GetMethod(nameof(DressMySlugcat.Utils.DefaultColorForSprite), DressMySlugcatHooks.propFlags),
                                 typeof(DressMySlugcatHooks).GetMethod(nameof(DressMySlugcat_Utils_DefaultColorForSprite), DressMySlugcatHooks.methodFlags));
        }

        private static List<string> DressMySlugcat_Utils_ValidSlugcatNames(DressMySlugcatHooks.orig_DressMySlugcat_Utils_ValidSlugcatNames orig)
        {
            var list = orig();
            list.Add("Mothpup");
            return list;
        }

        private static Color DressMySlugcat_Utils_DefaultColorForSprite(DressMySlugcatHooks.orig_DressMySlugcat_Utils_DefaultColorForSprite orig, string slugcat, string sprite)
        {
            var result = orig(slugcat, sprite);
            if (slugcat == Plugin.SlugName.value || slugcat == Plugin.Mothpup.value)
            {
                switch (sprite)
                {
                    case "FACE":
                        result = DressMySlugcat.Utils.DefaultEyeColor(slugcat);
                        break;
                    case "HEAD":
                    case "BODY":
                    case "ARMS":
                    case "HIPS":
                    case "LEGS":
                    case "TAIL":
                    case "PIXEL":
                    case "WING":
                        result = DressMySlugcat.Utils.DefaultBodyColor(slugcat);
                        break;
                    case "ANTENNAE1":
                    case "ANTENNAE2":
                    case "LEPIDOTICWING":
                        result = PlayerEx.LightGreen;
                        break;
                    case "WINGGRADIENT":
                    case "SPECKLE":
                        result = PlayerEx.LightBlue;
                        break;
                    default:
                        result = DressMySlugcat.Utils.DefaultExtraColor(slugcat);
                        break;
                }
            }
            return result;
        }

        public static void DressMySlugcatForOutsider()
        {
            //DressMySlugcat.Utils.DefaultColorForSprite();

            DressMySlugcat.SpriteDefinitions.AvailableSprites.Add(new DressMySlugcat.SpriteDefinitions.AvailableSprite
            {
                Name = "ANTENNAE1",
                Description = "Antennae1",
                GallerySprite = "MothAntennae1",
                RequiredSprites = { "MothAntennae1" },
                Slugcats = { "Outsider", "Mothpup" }
            });
            DressMySlugcat.SpriteDefinitions.AvailableSprites.Add(new DressMySlugcat.SpriteDefinitions.AvailableSprite
            {
                Name = "ANTENNAE2",
                Description = "Antennae2",
                GallerySprite = "MothAntennae2",
                RequiredSprites = { "MothAntennae2" },
                Slugcats = { "Outsider", "Mothpup" }
            });
            DressMySlugcat.SpriteDefinitions.AvailableSprites.Add(new DressMySlugcat.SpriteDefinitions.AvailableSprite
            {
                Name = "WING",
                Description = "Wing",
                GallerySprite = "MothWingA0",
                RequiredSprites = { "MothWingA0" },
                Slugcats = { "Outsider", "Mothpup" }
            });
            DressMySlugcat.SpriteDefinitions.AvailableSprites.Add(new DressMySlugcat.SpriteDefinitions.AvailableSprite
            {
                Name = "LEPIDOTICWING",
                Description = "LepidoticWing",
                GallerySprite = "MothWingA1",
                RequiredSprites = { "MothWingA1" },
                Slugcats = { "Outsider", "Mothpup" }
            });
            DressMySlugcat.SpriteDefinitions.AvailableSprites.Add(new DressMySlugcat.SpriteDefinitions.AvailableSprite
            {
                Name = "WINGGRADIENT",
                Description = "WingGradient",
                GallerySprite = "MothWingA2",
                RequiredSprites = { "MothWingA2" },
                Slugcats = { "Outsider", "Mothpup" }
            });
            DressMySlugcat.SpriteDefinitions.AvailableSprites.Add(new DressMySlugcat.SpriteDefinitions.AvailableSprite
            {
                Name = "SPECKLE",
                Description = "Speckle",
                GallerySprite = "Pebble5",
                RequiredSprites = { "Pebble5" },
                Slugcats = { "Outsider", "Mothpup" }
            });
        }
    }
}
