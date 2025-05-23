﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheOutsider.Player_Hooks
{
    /// <summary>
    /// 用于添加或禁止猫食用部分物品
    /// </summary>
    public class CustomEdibleData
    {
        public SlugcatStats.Name name;

        public FoodData[] edibleDatas;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">猫的名字</param>
        /// <param name="edibleDatas">可食用与禁止食用配置，可传递多组</param>
        public CustomEdibleData(SlugcatStats.Name name, params FoodData[] edibleDatas)
        {
            this.name = name;
            this.edibleDatas = edibleDatas;
        }

        public class FoodData
        {
            public AbstractPhysicalObject.AbstractObjectType edibleType;
            public AbstractPhysicalObject.AbstractObjectType forbidType;
            public int food;
            public int qFood;

            /// <summary>
            /// 添加新的禁止项
            /// </summary>
            /// <param name="forbidType">禁止的食物的类型</param>
            public FoodData(AbstractPhysicalObject.AbstractObjectType forbidType)
            {
                this.forbidType = forbidType;
                food = qFood = -1;
            }


            /// <summary>
            /// 添加新的可食用项
            /// </summary>
            /// <param name="edibleType">可食用的物体类型</param>
            /// <param name="food">食用回复的整数饱食度</param>
            /// <param name="quarterFood">食用回复的小数饱食度(1/4格)</param>
            public FoodData(AbstractPhysicalObject.AbstractObjectType edibleType, int food, int quarterFood)
            {
                this.edibleType = edibleType;
                this.food = food;
                this.qFood = quarterFood;
            }
        }
    }

    public static class CustomEdible
    {
        public static void Register(CustomEdibleData data)
        {
            CustomEdibleHook.OnModInit();
            if (!edibleDatas.ContainsKey(data.name))
            {
                edibleDatas.Add(data.name, data);
            }
            else
            {
                Debug.Log("Already register for cat : " + data.name);
            }
        }

        public static Dictionary<SlugcatStats.Name, CustomEdibleData> edibleDatas =
            new Dictionary<SlugcatStats.Name, CustomEdibleData>();


        public static SlugcatStats.Name? AdditionalConditions(Player player)
        {
            if (PlayerHooks.PlayerData.TryGetValue(player, out var playerEX))
                return Plugin.SlugName;
            return null;
        }
    }
    static class CustomEdibleHook
    {
        private static bool isLoaded;

        public static void OnModInit()
        {
            if (!isLoaded)
            {
                IL.Player.GrabUpdate += Player_GrabUpdate_EdibleIL;
                On.Player.BiteEdibleObject += Player_BiteEdibleObject;
                isLoaded = true;
            }
        }


        private static void Player_GrabUpdate_EdibleIL(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                ILLabel label = c.DefineLabel();
                ILLabel label2 = c.DefineLabel();
                c.TryGotoNext(MoveType.Before, i => i.MatchLdarg(0),
                                           i => i.MatchCall<Creature>("get_grasps"),
                                           i => i.MatchLdloc(13),
                                           i => i.MatchLdelemRef(),
                                           i => i.MatchLdfld<Creature.Grasp>("grabbed"),
                                           i => i.MatchIsinst<IPlayerEdible>());
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_S, (byte)13);
                c.EmitDelegate<Func<Player, int, bool>>(EdibleForCat);
                c.Emit(OpCodes.Brtrue_S, label);

                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_S, (byte)13);
                c.EmitDelegate<Func<Player, int, bool>>((self, index) =>
                {
                    if ((CustomEdible.edibleDatas.ContainsKey(self.slugcatStats.name) &&
                         CustomEdible.edibleDatas[self.slugcatStats.name].edibleDatas.Any(i =>
                            i.forbidType == self.grasps[index].grabbed.abstractPhysicalObject.type)) ||
                        (CustomEdible.AdditionalConditions(self) != null &&
                         CustomEdible.edibleDatas.ContainsKey(CustomEdible.AdditionalConditions(self)) &&
                         CustomEdible.edibleDatas[CustomEdible.AdditionalConditions(self)].edibleDatas.Any(i =>
                            i.forbidType == self.grasps[index].grabbed.abstractPhysicalObject.type)))
                        return false;
                    return true;
                });
                c.Emit(OpCodes.Brfalse_S, label2);
                c.TryGotoNext(MoveType.Before, i => i.MatchLdloc(13),
                                            i => i.MatchStloc(6),
                                            i => i.MatchLdloc(13));
                c.MarkLabel(label);
                c.TryGotoNext(MoveType.After, i => i.MatchStloc(6));
                c.MarkLabel(label2);

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static bool EdibleForCat(Player player, int index)
        {
            if (!(CustomEdible.edibleDatas.ContainsKey(player.slugcatStats.name) ||
                 (CustomEdible.AdditionalConditions(player) != null && CustomEdible.edibleDatas.ContainsKey(CustomEdible.AdditionalConditions(player)))))
                return false;
            var grasp = player.grasps[index];

            if (grasp != null)
            {
                if ((CustomEdible.edibleDatas.ContainsKey(player.slugcatStats.name) &&
                     CustomEdible.edibleDatas[player.slugcatStats.name].edibleDatas.Any(i => i.edibleType == grasp.grabbed.abstractPhysicalObject.type)) ||
                    (CustomEdible.AdditionalConditions(player) != null &&
                     CustomEdible.edibleDatas.ContainsKey(CustomEdible.AdditionalConditions(player)) &&
                     CustomEdible.edibleDatas[CustomEdible.AdditionalConditions(player)].edibleDatas.Any(i => i.edibleType == grasp.grabbed.abstractPhysicalObject.type)))
                    return true;
            }

            return false;
        }

        private static void Player_BiteEdibleObject(On.Player.orig_BiteEdibleObject orig, Player self, bool eu)
        {
            bool canBitOther = self.grasps.All(i => !(i?.grabbed is IPlayerEdible));
            orig(self, eu);
            if (canBitOther)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (self.grasps[i] != null && self.grasps[i].grabbed != null &&
                        ((CustomEdible.edibleDatas.ContainsKey(self.slugcatStats.name) &&
                         CustomEdible.edibleDatas[self.slugcatStats.name].edibleDatas.
                            Any(d => d.edibleType == self.grasps[i].grabbed.abstractPhysicalObject.type)) ||
                        (CustomEdible.AdditionalConditions(self) != null &&
                         CustomEdible.edibleDatas.ContainsKey(CustomEdible.AdditionalConditions(self)) &&
                         CustomEdible.edibleDatas[CustomEdible.AdditionalConditions(self)].edibleDatas.
                            Any(d => d.edibleType == self.grasps[i].grabbed.abstractPhysicalObject.type))))
                    {
                        var obj = self.grasps[i].grabbed;
                        var slugName = CustomEdible.edibleDatas.ContainsKey(self.slugcatStats.name) ? self.slugcatStats.name : CustomEdible.AdditionalConditions(self);
                        var data = CustomEdible.edibleDatas[slugName].edibleDatas.First(d => d.edibleType == obj.abstractPhysicalObject.type);
                        bool eaten = (FoodHooks.PhysicalObjectData.TryGetValue(obj, out var physicalObject) && physicalObject.bitesLeft <= 1) ||
                                    !FoodHooks.PhysicalObjectData.TryGetValue(obj, out _);
                        if (eaten)
                        {
                            if (self.SessionRecord != null)
                                self.SessionRecord.AddEat(obj);
                        }
                        if (self.graphicsModule as PlayerGraphics != null)
                            (self.graphicsModule as PlayerGraphics).BiteFly(i);
                        if (FoodHooks.PhysicalObjectData.TryGetValue(obj, out physicalObject))
                        {
                            physicalObject.bitesLeft--;
                            self.room.PlaySound(SoundID.Slugcat_Bite_Dangle_Fruit, self.grasps[i].grabbed.firstChunk.pos);
                        }
                        if (eaten)
                        {
                            self.AddFood(data.food);
                            for (int j = 0; j < data.qFood; j++)
                                self.AddQuarterFood();
                            #region 此处加入离群者特有效果
                            //吃鞭炮草回复寒冷值
                            if (obj.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant)
                            {
                                self.Hypothermia = Mathf.Min(0, self.Hypothermia - 0.3f);
                            }
                            //吃烟雾果会死
                            else if (obj.abstractPhysicalObject.type == AbstractPhysicalObject.AbstractObjectType.PuffBall && !Plugin.optionsMenuInstance.immuneSporeCloud.Value)
                            {
                                self.Die();
                            }
                            #endregion
                            self.room.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, self.grasps[i].grabbed.firstChunk.pos);
                            self.grasps[i].Release();
                            obj.Destroy();
                        }
                        break;
                    }
                }
            }
        }
    }
}


