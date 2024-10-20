using System;
using Debug = UnityEngine.Debug;
using DevConsole.Commands;
using MoreSlugcats;
using DevConsole;
using System.Linq;
using System.Collections.Generic;

namespace SlugpupStuff
{
    public class PupsPlusModCompat
    {
        public static void RegisterSpawnPupCommand()
        {
            string[] tags = ["Voidsea", "Winter", "Ignorecycle", "TentacleImmune", "Lavasafe", "AlternateForm", "PreCycle", "Night"];
            string[] variants = ["Aquatic", "Tundra", "Hunter", "Rotund", "Regular"];
            string[] tags2 = [.. tags, .. variants];
            string[] arguments = null;
            new CommandBuilder("spawn_pup")
                .RunGame((game, args) =>
                {
                    arguments = args;
                    try
                    {
                        EntityID? id = null;
                        if (args.Length != 0 && args[0].Contains('.'))
                        {
                            try
                            {
                                id = EntityID.FromString(args[0]);
                            }
                            catch
                            {
                                if (int.TryParse(args[0], out int idNum))
                                    id = new EntityID(0, idNum);
                            }
                        }

                        var abstractPup = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, GameConsole.TargetPos.Room.realizedRoom.GetWorldCoordinate(GameConsole.TargetPos.Pos), id ?? game.GetNewID());

                        if (args.Length != 0)
                        {
                            abstractPup.spawnData = "{" + string.Join(",", args.Select((string tag) => tags2.FirstOrDefault((string testTag) => tag.Equals(testTag, StringComparison.OrdinalIgnoreCase)) ?? tag)) + "}";
                        }
                        abstractPup.setCustomFlags();
                        GameConsole.TargetPos.Room.AddEntity(abstractPup);
                        if (GameConsole.TargetPos.Room.realizedRoom != null)
                        {
                            abstractPup.RealizeInRoom();
                        }


                    }
                    catch(Exception ex)
                    {
                        GameConsole.WriteLine("Failed to spawn pup! See console log for more info.");
                        Debug.Log("pup failed:" + ex.ToString());
                    }
                })
                .Help("spawn_pup [ID?] [variant?] [args...]")
                .AutoComplete(arguments => 
                {
                    if (arguments.Length == 0) return variants;
                    else if (arguments.Length == 1 && arguments[0].Contains('.')) return variants;
                    else if (arguments.Length == 1 && !arguments[0].Contains('.') || arguments.Length == 2) return tags;
                    else return null;
                })
                .Register();
        }
        public static void RegisterPupsPlusDebugCommands()
        {
            new CommandBuilder("slugpupDebuglabels_toggle")
                .RunGame((game, args) =>
                {
                    SlugpupDebugger.DebugLabelManager.ToggleLabels();
                })
                .Register();
        }
        public static void SetupDMSSprites()
        {
            new MonoMod.RuntimeDetour.Hook(
                typeof(DressMySlugcat.Utils).GetProperty(nameof(DressMySlugcat.Utils.ValidSlugcatNames), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetGetMethod(),
                PupsPlusModCompat.ValidPupNames);

            DressMySlugcat.SpriteDefinitions.AvailableSprites.Add(new DressMySlugcat.SpriteDefinitions.AvailableSprite
            {
                Name = "GILLS",
                Description = "Gills",
                GallerySprite = "LizardScaleA3",
                RequiredSprites = ["LizardScaleA3", "LizardScaleB3"],
                Slugcats = ["Aquaticpup"]
            });
        }
        public static List<string> ValidPupNames(Func<List<string>> orig)
        {
            var list = orig();
            list.AddRange(new List<string>
            {
                "Aquaticpup",
                "Tundrapup",
                "Hunterpup",
                "Rotundpup"
            });
            return list;
        }
        public static bool SimplifiedMovesetGourmand() => SimplifiedMoveset.MainModOptions.gourmand.Value;
        public static bool IsPearlpup(Player player) => Pearlcat.Hooks.IsPearlpup(player);
        public static float EmeraldsLegendaryChance => EmeraldsTweaksRemix.ModConfig.shinyLegendaryChance.Value;
    }
}
