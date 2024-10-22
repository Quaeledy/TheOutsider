using System.IO;

namespace TheOutsider.World_Hooks
{
    class RegionHooks
    {
        public static void Init()
        {
            /*On.ModManager.ModMerger.WriteMergedFile += ModMerger_WriteMergedFile;
            On.ModManager.ModMerger.ExecutePendingMerge += ModMerger_ExecutePendingMerge;*/
            On.SlugcatStats.getSlugcatStoryRegions += SlugcatStats_getSlugcatStoryRegions;
            On.Region.GetProperRegionAcronym += Region_GetProperRegionAcronym;
        }
        /*
        private void ModMerger_WriteMergedFile(On.ModManager.ModMerger.orig_WriteMergedFile orig, ModManager.Mod sourceMod, string sourcePath, string[] mergeLines)
        {
            if (sourceMod.id == Plugin.MOD_ID)
            {
                List<string> mergedlist = new List<string>(mergeLines);
                try
                {
                    var fileName = sourcePath.Substring(sourcePath.LastIndexOf("\\") + 1);
                    if (fileName.Contains("world"))
                    {

                        var path = AssetManager.ResolveDirectory("modify/world") + "/" + fileName.Substring(fileName.LastIndexOf("_") + 1, fileName.LastIndexOf(".") - fileName.LastIndexOf("_") - 1) + "/" + fileName;
                        var text = new List<string>(File.ReadAllLines(path));

                        _log.LogMessage("[Merger Fix] World Creature Spawn Fixing (" + fileName + ")");
                        ModManager.ModMerger.WorldFile worldFile = new ModManager.ModMerger.WorldFile(File.ReadAllLines(path));
                        ModManager.ModMerger.WorldFile worldFile2 = new ModManager.ModMerger.WorldFile(mergeLines);

                        List<ModManager.ModMerger.WorldRoomSpawn> list2 = new List<ModManager.ModMerger.WorldRoomSpawn>();
                        List<ModManager.ModMerger.WorldRoomSpawn> list3 = new List<ModManager.ModMerger.WorldRoomSpawn>();

                        if(fileName == "world_cl.txt")
                        {
                            for (int i = 0; i < worldFile.creatures.Count; i++)
                            {
                                worldFile.creatures.RemoveAt(i);
                            }
                        }

                        for (int num9 = 0; num9 < worldFile2.creatures.Count; num9++)
                        {
                            if (worldFile2.creatures[num9].excludeMode)
                            {
                                list2.Add(worldFile2.creatures[num9]);
                            }
                            else
                            {
                                list3.Add(worldFile2.creatures[num9]);
                            }
                        }
                        for (int num10 = 0; num10 < list2.Count; num10++)
                        {
                            ModManager.ModMerger.WorldRoomSpawn worldRoomSpawn = list2[num10];
                            bool flag5 = false;
                            string[] source = worldRoomSpawn.character.Split(new char[] { ',' });

                            for (int num11 = worldFile.creatures.Count - 1; num11 >= 0; num11--)
                            {
                                if (worldFile.creatures[num11].roomName == worldRoomSpawn.roomName && worldFile.creatures[num11].lineageDen == worldRoomSpawn.lineageDen)
                                {
                                    if (worldFile.creatures[num11].excludeMode)
                                    {
                                        flag5 = true;
                                        worldFile.creatures[num11] = worldRoomSpawn;
                                    }
                                    else if (worldFile.creatures[num11].character == "" || !source.Contains(worldFile.creatures[num11].character))
                                    {
                                        worldFile.creatures.RemoveAt(num11);
                                    }
                                }
                            }
                            if (!flag5)
                            {
                                worldFile.creatures.Add(worldRoomSpawn);
                            }

                        }
                        for (int num12 = 0; num12 < list3.Count; num12++)
                        {
                            ModManager.ModMerger.WorldRoomSpawn worldRoomSpawn2 = list3[num12];
                            if (worldRoomSpawn2.lineageDen >= 0 && worldRoomSpawn2.roomName == "OFFSCREEN")
                            {
                                worldFile.creatures.Add(worldRoomSpawn2);
                            }
                            else
                            {
                                bool flag6 = false;
                                for (int num13 = worldFile.creatures.Count - 1; num13 >= 0; num13--)
                                {
                                    if (worldFile.creatures[num13].roomName == worldRoomSpawn2.roomName && worldFile.creatures[num13].lineageDen == worldRoomSpawn2.lineageDen)
                                    {
                                        if (!worldFile.creatures[num13].excludeMode && worldFile.creatures[num13].character == worldRoomSpawn2.character)
                                        {
                                            worldFile.creatures[num13] = worldRoomSpawn2;
                                            flag6 = true;
                                        }
                                        if (worldFile.creatures[num13].excludeMode && worldRoomSpawn2.character != "" && !worldFile.creatures[num13].character.Split(new char[] { ',' }).Contains(worldRoomSpawn2.character))
                                        {
                                            ModManager.ModMerger.WorldRoomSpawn worldRoomSpawn3 = worldFile.creatures[num13];
                                            worldRoomSpawn3.character = worldRoomSpawn3.character + "," + worldRoomSpawn2.character;
                                        }
                                    }
                                }
                                if (!flag6)
                                {
                                    worldFile.creatures.Add(worldRoomSpawn2);
                                }
                            }
                        }
                        foreach (string item in worldFile2.migrationBlockages)
                        {
                            if (!worldFile.migrationBlockages.Contains(item))
                            {
                                worldFile.migrationBlockages.Add(item);
                            }
                        }
                        foreach (string item2 in worldFile2.unknownContextLines)
                        {
                            if (!worldFile.unknownContextLines.Contains(item2))
                            {
                                worldFile.unknownContextLines.Add(item2);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _log.LogError("[Merger Fix] Error" + e.Message + "\n" + e.StackTrace);
                }
            }
            orig(sourceMod, sourcePath, mergeLines);
        }

        public void ModMerger_ExecutePendingMerge(On.ModManager.ModMerger.orig_ExecutePendingMerge orig, ModManager.ModMerger self, ModManager.ModApplyer applyer)
        {
            applyer.applyFileInd = 0;
            applyer.applyFileLength = 0;
            foreach (KeyValuePair<string, List<ModManager.ModMerger.PendingApply>> keyValuePair in self.moddedFiles)
            {
                List<ModManager.ModMerger.PendingApply> value = keyValuePair.Value;
                if (value.Count > 1 && (value.Count != 2 || !value[0].isVanilla || value[1].isModification))
                {
                    applyer.applyFileLength++;
                }
            }
            foreach (KeyValuePair<string, List<ModManager.ModMerger.PendingApply>> keyValuePair2 in self.moddedFiles)
            {
                string key = keyValuePair2.Key;
                List<ModManager.ModMerger.PendingApply> value2 = keyValuePair2.Value;
                if (value2.Count > 1 && (value2.Count != 2 || !value2[0].isVanilla || value2[1].isModification))
                {
                    applyer.applyFileInd++;
                    bool flag = false;
                    for (int i = 0; i < value2.Count; i++)
                    {
                        if (value2[i].filePath.Contains("strings.txt"))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        ModManager.ModMerger.PendingApply pendingApply = null;
                        if (value2[0].isVanilla)
                        {
                            pendingApply = value2[0];
                        }
                        ModManager.ModMerger.PendingApply pendingApply2 = null;
                        List<ModManager.ModMerger.PendingApply> list = new List<ModManager.ModMerger.PendingApply>();
                        List<ModManager.ModMerger.PendingApply> list2 = new List<ModManager.ModMerger.PendingApply>();
                        for (int j = (pendingApply == null) ? 0 : 1; j < value2.Count; j++)
                        {
                            if (value2[j].mergeLines != null)
                            {
                                list.Add(value2[j]);
                            }
                            else if (value2[j].isModification)
                            {
                                list2.Add(value2[j]);
                            }
                            else
                            {
                                pendingApply2 = value2[j];
                            }
                        }
                        if (pendingApply != null || pendingApply2 != null)
                        {
                            string text = (Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "mergedmods" + key).ToLowerInvariant();
                            if (pendingApply2 != null)
                            {
                                if (text.Contains("strings.txt"))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(text));
                                    File.Copy(value2[0].filePath, text, true);
                                    for (int k = 1; k < value2.Count; k++)
                                    {
                                        string input = InGameTranslator.EncryptDecryptFile(value2[k].filePath, false, true);
                                        ModManager.ModMerger.MergeShortStrings(value2[k].modApplyFrom, text, Regex.Split(input, "\r\n"));
                                    }
                                }
                                else
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(text));
                                    File.Copy(pendingApply2.filePath, text, true);
                                }
                            }
                            else
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(text));
                                File.Copy(pendingApply.filePath, text, true);
                            }
                            for (int l = 0; l < 2; l++)
                            {
                                foreach (ModManager.ModMerger.PendingApply pendingApply4 in ((l == 0) ? list : list2))
                                {
                                    pendingApply4.ApplyModifications(text);
                                }
                            }
                        }
                    }
                }
            }
            applyer.applyFileLength = 0;
            applyer.applyFileInd = 0;

            orig(self, applyer);
        }
        */
        public static string[] SlugcatStats_getSlugcatStoryRegions(On.SlugcatStats.orig_getSlugcatStoryRegions orig, SlugcatStats.Name i)
        {
            string[] result = orig(i);
            if (i.value == Plugin.SlugName.value)
            {
                result = new string[]
                {
                    "CC",
                    "CL",
                    "DS",
                    "GW",
                    "HI",
                    "LF",
                    //"MS",//把这个注释是因为朝圣者成就跳过了沉没巨构，导致算出来的回响少一个，因此会在见了回响存档时导致游戏崩溃
                    "SB",
                    "SI",
                    "SL",
                    "SU",
                    "VS",
                };
            }
            return result;
        }

        public static string Region_GetProperRegionAcronym(On.Region.orig_GetProperRegionAcronym orig, SlugcatStats.Name character, string baseAcronym)
        {
            if (character.value != Plugin.SlugName.value)
            {
                return (orig(character, baseAcronym));
            }
            else
            {
                string text = baseAcronym;
                if (text == "UX")
                {
                    text = "UW";
                }
                else if (text == "SX")
                {
                    text = "SS";
                }
                if (ModManager.MSC && character != null)
                {
                    if (text == "SH")
                    {
                        text = "CL";
                    }
                }
                string[] array = AssetManager.ListDirectory("World", true, false);
                for (int i = 0; i < array.Length; i++)
                {
                    string path = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                "World",
                Path.DirectorySeparatorChar.ToString(),
                Path.GetFileName(array[i]),
                Path.DirectorySeparatorChar.ToString(),
                "equivalences.txt"
                    }));
                    if (File.Exists(path))
                    {
                        string[] array2 = File.ReadAllText(path).Trim().Split(new char[]
                        {
                    ','
                        });
                        for (int j = 0; j < array2.Length; j++)
                        {
                            string text2 = null;
                            string a = array2[j];
                            if (array2[j].Contains("-"))
                            {
                                a = array2[j].Split(new char[]
                                {
                            '-'
                                })[0];
                                text2 = array2[j].Split(new char[]
                                {
                            '-'
                                })[1];
                            }
                            if (a == baseAcronym && (text2 == null || character.value.ToLower() == text2.ToLower()))
                            {
                                text = Path.GetFileName(array[i]).ToUpper();
                            }
                        }
                    }
                }
                return text;
            }
        }

        static RegionHooks _instance;
    }
}
