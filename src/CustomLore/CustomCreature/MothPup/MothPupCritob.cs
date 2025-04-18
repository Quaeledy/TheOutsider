﻿using DevInterface;
using Fisobs.Creatures;
using Fisobs.Properties;
using Fisobs.Sandbox;
using MoreSlugcats;
using System.Collections.Generic;
using UnityEngine;
using Watcher;
using static PathCost.Legality;
using System.Linq;

namespace TheOutsider.CustomLore.CustomCreature.MothPup
{
    internal sealed class MothPupCritob : Critob
    {
        public static readonly CreatureTemplate.Type Mothpup = new("Mothpup", true);
        public static readonly MultiplayerUnlocks.SandboxUnlockID MothPupUnlock = new("Mothpup", true);

        public MothPupCritob() : base(TheOutsiderEnums.CreatureTemplateType.Mothpup)
        {
            LoadedPerformanceCost = 10f;
            SandboxPerformanceCost = new(linear: 0.6f, exponential: 0.1f);
            ShelterDanger = ShelterDanger.Safe;
            CreatureName = "Mothpup";
            Icon = new MothPupIcon();
            RegisterUnlock(killScore: KillScore.Configurable(2), TheOutsiderEnums.CreatureTemplateType.MothPupUnlock, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);//DLCSharedEnums.SandboxUnlockID.SlugNPC
        }

        public override CreatureTemplate CreateTemplate()
        {
            // CreatureFormula does most of the ugly work for you when creating a new CreatureTemplate,
            // but you can construct a CreatureTemplate manually if you need to.
            //创建新的CreatureTemplate时，CreatureFormula会为您完成大部分丑陋的工作，
            //但如果需要，您可以手动构建一个CreatureTemplate。

            CreatureTemplate t = new CreatureFormula(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, this)
            {
                DefaultRelationship = new(CreatureTemplate.Relationship.Type.Ignores, 0.5f),
                HasAI = true,
                InstantDeathDamage = 1,
                Pathing = Plugin.optionsMenuInstance.infiniteFlight.Value ?
                          PreBakedPathing.Ancestral(CreatureTemplate.Type.CicadaB) :
                          PreBakedPathing.Ancestral(DLCSharedEnums.CreatureTemplateType.ScavengerElite),
                //Pathing = PreBakedPathing.Ancestral(DLCSharedEnums.CreatureTemplateType.ScavengerElite),
                TileResistances = new()
                {
                    Air = new (4.5f, Allowed),
                    OffScreen = new(1f, Allowed),
                    Floor = new(1f, Allowed),
                    Corridor = new(1f, Allowed),
                    Climb = new(2.5f, Allowed),
                },
                ConnectionResistances = new()
                {
                    Standard = new(1f, Allowed),
                    OpenDiagonal = new(3f, Allowed),
                    ReachOverGap = new(3f, Allowed),
                    ReachUp = new(2f, Allowed),
                    DoubleReachUp = new(2f, Allowed),
                    ReachDown = new(2f, Allowed),
                    SemiDiagonalReach = new(2f, Allowed),
                    DropToFloor = new(10f, Allowed),
                    DropToClimb = new(20f, Allowed),
                    DropToWater = new(10f, Allowed),
                    ShortCut = new(2.5f, Allowed),
                    BetweenRooms = new(4f, Allowed),
                    Slope = new(1.5f, Allowed),
                    NPCTransportation = new(20f, Allowed)
                },
                DamageResistances = new()
                {
                    Base = 1f,
                },
                StunResistances = new()
                {
                    Base = 1f,
                }
            }.IntoTemplate();

            // The below properties are derived from vanilla creatures, so you should have your copy of the decompiled source code handy.
            // 以下属性源自原版生物，因此您应该随身携带反编译的源代码副本。
            // Some notes on the fields of CreatureTemplate:
            //关于CreatureTemplate字段的一些注意事项：
            // offScreenSpeed       生物在抽象房间之间移动的速度
            // abstractLaziness     生物开始迁徙需要多长时间
            // smallCreature        决定岩石是否会被破坏，大型捕食者是否会忽视它，等等
            // dangerToPlayer       蓝香菇为0.85，蜘蛛为0.1，拟态草为0.5
            // waterVision          0..1 生物在水中的视力
            // throughSurfaceVision 0..1 生物透过水面的视力
            // movementBasedVision  0..1 移动生物的视觉奖励
            // lungCapacity         滴答作响，直到该生物因溺水而失去知觉
            // quickDeath           决定生物是否应该按照Creature.Violence()死亡。如果为false，则必须定义自定义死亡逻辑
            // saveCreature         决定该生物是否在循环结束后被保存。对监视者和垃圾虫来说是false的
            // hibernateOffScreen   适用于雨鹿、钢鸟、利维坦、秃鹫和拾荒者
            // bodySize             蝙蝠是0.1，蛋虫是0.4，蓝香菇是5.5，蛞蝓是1
            t.grasps = 2;//
            t.offScreenSpeed = 0.5f;//生物在抽象房间之间移动的速度
            t.bodySize = 1f;//蝙蝠是0.1，蛋虫是0.4，蓝香菇是5.5，蛞蝓是1
            t.shortcutSegments = 2;
            t.doPreBakedPathing = false;
            t.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;//水生类型：空气和水面
            t.waterPathingResistance = 2f;
            t.canSwim = true;
            t.requireAImap = true;
            t.socialMemory = true;
            t.interestInOtherAncestorsCatches = 0f;
            t.interestInOtherCreaturesCatches = 0f;
            t.meatPoints = 2;
            t.jumpAction = "Jump";
            t.pickupAction = "Pick up / Eat";
            t.throwAction = "Throw";
            t.visualRadius = 1200f;//猫崽是800f
            t.saveCreature = false;//决定该生物是否在循环结束后被保存。对监视者和垃圾虫来说是false的

            t.smallCreature = false;//决定岩石是否会被破坏，大型捕食者是否会忽视它，等等
            /*t.abstractedLaziness = 200;//生物开始迁徙需要多长时间
            t.dangerousToPlayer = 0f;//蓝香菇为0.85，蜘蛛为0.1，拟态草为0.5
            t.waterVision = 0.75f;//生物在水中的视力
            t.throughSurfaceVision = 0.95f;//生物透过水面的视力
            t.lungCapacity = 10000f;//滴答作响，直到该生物因溺水而失去知觉
            t.quickDeath = true;//决定生物是否应该按照Creature.Violence()死亡。如果为false，则必须定义自定义死亡逻辑
            t.hibernateOffScreen = true;//适用于雨鹿、钢鸟、利维坦、秃鹫和拾荒者
            t.roamBetweenRoomsChance = 0.5f;
            t.stowFoodInDen = false;
            t.movementBasedVision = 0.65f;
            t.communityInfluence = 0.5f;
            t.canFly = false;*/

            t.SetDoubleReachUpConnectionParams(AItile.Accessibility.Climb, AItile.Accessibility.Air, AItile.Accessibility.Climb);

            return t;
        }

        public override void EstablishRelationships()
        {
            // You can use StaticWorld.EstablishRelationship, but the Relationships class exists to make this process more ergonomic.
            // 您可以使用 StaticWorld.EstablishRelationship，但Relationships类的存在是为了使此过程更符合人体工程学。
            Relationships self = new(TheOutsiderEnums.CreatureTemplateType.Mothpup);
            CreatureTemplate slugpupTemplate = StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC);
            foreach (var template in StaticWorld.creatureTemplates)
            {
                if (template.quantified)
                {/*
                    self.Ignores(template.type);
                    self.IgnoredBy(template.type);*/
                    StaticWorld.EstablishRelationship(template.type, TheOutsiderEnums.CreatureTemplateType.Mothpup, template.relationships[slugpupTemplate.index]);
                    StaticWorld.EstablishRelationship(TheOutsiderEnums.CreatureTemplateType.Mothpup, template.type, slugpupTemplate.relationships[template.index]);
                }
            }

            self.IsInPack(TheOutsiderEnums.CreatureTemplateType.Mothpup, 0.5f);

            //原捕食，现忽略
            self.Ignores(CreatureTemplate.Type.Fly);
            self.Ignores(CreatureTemplate.Type.EggBug);
            self.Ignores(CreatureTemplate.Type.SmallCentipede);
            self.Ignores(CreatureTemplate.Type.SmallNeedleWorm);
            self.Ignores(CreatureTemplate.Type.VultureGrub);

            //恐吓
            self.FearedBy(CreatureTemplate.Type.Fly, 0.5f);
            self.FearedBy(CreatureTemplate.Type.LanternMouse, 0.3f);

            //被攻击

            //被捕食
            self.EatenBy(CreatureTemplate.Type.Spider, 1f);
            self.EatenBy(CreatureTemplate.Type.LizardTemplate, 0.5f);
            self.EatenBy(CreatureTemplate.Type.Vulture, 0.3f);
            self.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);
            self.EatenBy(CreatureTemplate.Type.MirosBird, 0.6f);
            self.EatenBy(CreatureTemplate.Type.BigSpider, 0.4f);
            self.EatenBy(DLCSharedEnums.CreatureTemplateType.MirosVulture, 1f);
            self.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.5f);

            //害怕
            self.Fears(CreatureTemplate.Type.Vulture, 1f);
            self.Fears(CreatureTemplate.Type.BigEel, 1f);
            self.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
            self.Fears(CreatureTemplate.Type.TentaclePlant, 1f);
            self.Fears(CreatureTemplate.Type.MirosBird, 1f);
            self.Fears(CreatureTemplate.Type.Centipede, 0.5f);
            self.Fears(CreatureTemplate.Type.Centiwing, 0.4f);
            self.Fears(CreatureTemplate.Type.LizardTemplate, 0.6f);
            self.Fears(CreatureTemplate.Type.Spider, 0.5f);
            self.Fears(CreatureTemplate.Type.BigSpider, 0.5f);
            self.Fears(CreatureTemplate.Type.SpitterSpider, 0.8f);
            self.Fears(CreatureTemplate.Type.DropBug, 0.5f);
            self.Fears(CreatureTemplate.Type.RedCentipede, 1f);
            self.Fears(DLCSharedEnums.CreatureTemplateType.MirosVulture, 1f);

            //独立社会关系
            self.HasDynamicRelationship(CreatureTemplate.Type.CicadaA, 1f);
            self.HasDynamicRelationship(CreatureTemplate.Type.CicadaB, 1f);
            self.HasDynamicRelationship(CreatureTemplate.Type.JetFish, 1f);
            self.HasDynamicRelationship(CreatureTemplate.Type.Scavenger, 1f);
            self.HasDynamicRelationship(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1f);
            
            if (ModManager.ActiveMods.Any(mod => mod.id == "watcher"))//ModManager.Watcher
            {
                self.Ignores(WatcherEnums.CreatureTemplateType.BigMoth);
                self.IgnoredBy(WatcherEnums.CreatureTemplateType.BigMoth);
                self.Ignores(WatcherEnums.CreatureTemplateType.SmallMoth);
                self.IgnoredBy(WatcherEnums.CreatureTemplateType.SmallMoth);
            }
        }

        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new SlugNPCAI(acrit, acrit.world);
        }

        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            //acrit.state = new PlayerNPCState(acrit, 0);
            //acrit.abstractAI = new SlugNPCAbstractAI(acrit.world, acrit);
            Player mothPup = new Player(acrit, acrit.world);
            return mothPup;
        }

        public override CreatureState CreateState(AbstractCreature acrit)
        {
            return new PlayerNPCState(acrit, 0);
        }

        public override AbstractCreatureAI CreateAbstractAI(AbstractCreature acrit)
        {
            return new SlugNPCAbstractAI(acrit.world, acrit);
        }
        /*
        public override void ConnectionIsAllowed(AImap map, MovementConnection connection, ref bool? allowed)
        {
            // DLLs don't travel through shortcuts that start and end in the same room—they only travel through room exits.
            // To emulate this behavior, use something like:
            // 香菇不通过在同一个房间中开始和结束的快捷方式，它们只通过房间出口。
            // 要模仿这种行为，请使用以下内容：

            ShortcutData.Type n = ShortcutData.Type.Normal;
            if (connection.type == MovementConnection.MovementType.ShortCut)
            {
                allowed &=
                    connection.startCoord.TileDefined && map.room.shortcutData(connection.StartTile).shortCutType == n ||
                    connection.destinationCoord.TileDefined && map.room.shortcutData(connection.DestTile).shortCutType == n
                    ;
            }
            else if (connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
            {
                allowed &=
                    map.room.GetTile(connection.startCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.StartTile).shortCutType == n ||
                    map.room.GetTile(connection.destinationCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && map.room.shortcutData(connection.DestTile).shortCutType == n;
            }
        }

        public override void TileIsAllowed(AImap map, IntVector2 tilePos, ref bool? allowed)
        {
            // Large creatures like vultures, miros birds, and DLLs need 2 tiles of free space to move around in. Leviathans need 4! None of them can fit in one-tile tunnels.
            // To emulate this behavior, use something like:
            //大型生物，如秃鹫、钢鸟和香菇，需要2块空闲空间才能在其中移动。利维坦需要4块！没有一个能放在一个瓷砖隧道里。
            //要模仿这种行为，请使用以下内容：

            allowed &= map.IsFreeSpace(tilePos, tilesOfFreeSpace: 1);

            // DLLs can fit into shortcuts despite being fat.
            // To emulate this behavior, use something like:
            // DLL可以放入快捷方式，尽管它很胖。
            // 要模仿这种行为，请使用以下内容：

            allowed |= map.room.GetTile(tilePos).Terrain == Room.Tile.TerrainType.ShortcutEntrance;
        }*/

        public override IEnumerable<string> WorldFileAliases()
        {
            yield return "mothPup";
        }

        public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
        {
            yield return RoomAttractivenessPanel.Category.Flying;
            yield return RoomAttractivenessPanel.Category.LikesInside;
            yield return RoomAttractivenessPanel.Category.LikesOutside;
        }

        public override string DevtoolsMapName(AbstractCreature acrit)
        {
            return "Mothpup";
        }

        public override Color DevtoolsMapColor(AbstractCreature acrit)
        {
            return new Color(40f / 255f, 102f / 255f, 141f / 255f);
        }

        public override ItemProperties Properties(Creature crit)
        {
            // If you need to use the forObject parameter, pass it to your ItemProperties class's constructor.
            // The Mosquitoes example demonstrates this.
            // 如果需要使用forObject参数，请将其传递给ItemProperties类的构造函数。
            // 蚊子的例子说明了这一点。
            if (crit is Player mothPup && (crit as Player).abstractCreature.creatureTemplate.type == TheOutsiderEnums.CreatureTemplateType.Mothpup)
            {
                return new MothPupProperties(mothPup);
            }
            return null;


            // If you don't need the `forObject` parameter, store one ItemProperties instance as a static object and return that.
            // The CentiShields example demonstrates this.
            // 如果不需要“forObject”参数，请将一个ItemProperties实例存储为静态对象并返回。
            // CentiShields的例子说明了这一点。
            //return properties;
        }
        //private static readonly CentiShieldProperties properties = new();
    }
}
