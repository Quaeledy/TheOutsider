using System;
using DevConsole.Commands;
using MoreSlugcats;
using DevConsole;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.RuntimeDetour;
using static DevConsole.ObjectSpawner;
using RWCustom;

namespace TheOutsider
{
    internal class DevConsoleHooks
    {
        public static void Init()
        {
            RegisterSafeSpawners();
        }

        public static void RegisterSafeSpawners()
        {
            List<CreatureTemplate.Type> list = new List<CreatureTemplate.Type>();
            list.Add(TheOutsiderEnums.CreatureTemplateType.Mothpup);
            //list.Add(TheOutsiderEnums.CreatureTemplateType.Alcedo);
            CreatureTemplate.Type[] array2 = list.ToArray();
            string[] tags = new string[8] { "Voidsea", "Winter", "Ignorecycle", "TentacleImmune", "Lavasafe", "AlternateForm", "PreCycle", "Night" };
            Func<AbstractPhysicalObject.AbstractObjectType, string[], IEnumerable<string>> tagsAC = AutoCompleteTags(tags);
            CreatureTemplate.Type[] array3 = array2;
            foreach (CreatureTemplate.Type t in array3)
            {
                if ((int)(ExtEnum<CreatureTemplate.Type>)(object)t == -1)
                {
                    continue;
                }
                RegisterSpawner(t, new SimpleSpawnerInfo(delegate (AbstractPhysicalObject.AbstractObjectType type, string[] args)
                {
                    if (args.Length == 0)
                    {
                        string text = null;
                        if ((ExtEnum<CreatureTemplate.Type>)(object)StaticWorld.GetCreatureTemplate(t).TopAncestor().type == (ExtEnum<CreatureTemplate.Type>)(object)CreatureTemplate.Type.LizardTemplate)
                        {
                            text = "mean: float";
                        }
                        else if ((ExtEnum<CreatureTemplate.Type>)(object)t == (ExtEnum<CreatureTemplate.Type>)(object)CreatureTemplate.Type.Centipede)
                        {
                            text = "size: float";
                        }
                        else if ((ExtEnum<CreatureTemplate.Type>)(object)t == (ExtEnum<CreatureTemplate.Type>)(object)CreatureTemplate.Type.PoleMimic)
                        {
                            text = "length: int";
                        }
                        if (text != null)
                        {
                            return tagsAC(type, args).Concat(new string[1] { "help-" + text });
                        }
                        return tagsAC(type, args);
                    }
                    return args.Skip(((ExtEnum<CreatureTemplate.Type>)(object)StaticWorld.GetCreatureTemplate(t).TopAncestor().type == (ExtEnum<CreatureTemplate.Type>)(object)CreatureTemplate.Type.LizardTemplate) ? 1 : 0).All((string s) => tags.Contains(s, StringComparer.OrdinalIgnoreCase)) ? tagsAC(type, args) : null;
                }, delegate (AbstractPhysicalObject.AbstractObjectType type, string[] args, EntityID id, AbstractRoom room, WorldCoordinate pos)
                {
                    CreatureTemplate creatureTemplate = StaticWorld.GetCreatureTemplate(t);
                    if (!pos.NodeDefined || !creatureTemplate.mappedNodeTypes[pos.abstractNode])
                    {
                        Room realizedRoom = room.realizedRoom;
                        if (realizedRoom != null)
                        {
                            bool flag = (ExtEnum<CreatureTemplate.Type>)(object)t == (ExtEnum<CreatureTemplate.Type>)(object)CreatureTemplate.Type.TentaclePlant || 
                                        (ExtEnum<CreatureTemplate.Type>)(object)t == (ExtEnum<CreatureTemplate.Type>)(object)CreatureTemplate.Type.PoleMimic || 
                                        (ExtEnum<CreatureTemplate.Type>)(object)t == (ExtEnum<CreatureTemplate.Type>)(object)MoreSlugcatsEnums.CreatureTemplateType.StowawayBug;
                            float num = float.PositiveInfinity;
                            for (int j = 0; j < room.nodes.Length; j++)
                            {
                                AbstractRoomNode.Type type2 = room.nodes[j].type;
                                WorldCoordinate val3 = realizedRoom.LocalCoordinateOfNode(j);
                                val3.abstractNode = j;
                                if ((j < creatureTemplate.mappedNodeTypes.Length && creatureTemplate.mappedNodeTypes[j]) || 
                                    (flag && (ExtEnum<CreatureTemplate.Type>)(object)type2 == (ExtEnum<CreatureTemplate.Type>)(object)AbstractRoomNode.Type.Den) || 
                                    ((ExtEnum<CreatureTemplate.Type>)(object)t == (ExtEnum<CreatureTemplate.Type>)(object)CreatureTemplate.Type.GarbageWorm && 
                                     (ExtEnum<CreatureTemplate.Type>)(object)type2 == (ExtEnum<CreatureTemplate.Type>)(object)AbstractRoomNode.Type.GarbageHoles))
                                {
                                    IntVector2 tile = pos.Tile;
                                    float num2 = tile.FloatDist(val3.Tile);
                                    if (!pos.NodeDefined || num2 < num)
                                    {
                                        num = num2;
                                        pos.abstractNode = val3.abstractNode;
                                    }
                                }
                            }
                        }
                        if (!pos.NodeDefined)
                        {
                            pos.abstractNode = room.RandomRelevantNode(creatureTemplate);
                        }
                    }
                    AbstractCreature val4 = new AbstractCreature(room.world, creatureTemplate, (Creature)null, pos, id);
                    if (args.Length != 0)
                    {
                        int result2;
                        if ((ExtEnum<CreatureTemplate.Type>)(object)t == (ExtEnum<CreatureTemplate.Type>)(object)CreatureTemplate.Type.Centipede && float.TryParse(args[0], out var result))
                        {
                            val4.spawnData = $"{{{result}}}";
                        }
                        else if ((ExtEnum<CreatureTemplate.Type>)(object)t == (ExtEnum<CreatureTemplate.Type>)(object)CreatureTemplate.Type.PoleMimic && int.TryParse(args[0], out result2))
                        {
                            val4.spawnData = $"{{{result2}}}";
                        }
                        else
                        {
                            if ((ExtEnum<CreatureTemplate.Type>)(object)creatureTemplate.TopAncestor().type == (ExtEnum<CreatureTemplate.Type>)(object)CreatureTemplate.Type.LizardTemplate && float.TryParse(args[0], out var result3))
                            {
                                args[0] = $"Mean:{result3}";
                            }
                            val4.spawnData = "{" + string.Join(",", args.Select((string tag) => tags.FirstOrDefault((string testTag) => tag.Equals(testTag, StringComparison.OrdinalIgnoreCase)) ?? tag)) + "}";
                        }
                    }
                    val4.setCustomFlags();
                    ((AbstractPhysicalObject)val4).Move(pos);
                    return (AbstractPhysicalObject)(object)val4;
                }));
            }
        }

        private static Func<AbstractPhysicalObject.AbstractObjectType, string[], IEnumerable<string>> AutoCompleteTags(params string[] tags)
        {
            return (AbstractPhysicalObject.AbstractObjectType _, string[] args) => Enumerable.Except<string>(tags, args, StringComparer.OrdinalIgnoreCase);
        }
    }
}
