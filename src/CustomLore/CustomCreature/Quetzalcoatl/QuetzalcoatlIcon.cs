﻿using Fisobs.Core;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature.Quetzalcoatl
{
    sealed class QuetzalcoatlIcon : Icon
    {
        ////Vanilla只提供一个int字段来存储所有自定义数据。
        //这里，int字段用于存储盾牌的色调，按1000缩放。
        //所以，0是红色，70是橙色。

        public override int Data(AbstractPhysicalObject apo)
        {/*
            QuetzalcoatlAbstract centiShieldAbstract = apo as QuetzalcoatlAbstract;
            if (centiShieldAbstract == null)
            {
                return 0;
            }
            return (int)(centiShieldAbstract.hue * 1000f);*/
            return 0;
        }

        public override Color SpriteColor(int data)
        {
            //return Custom.HSL2RGB((float)data / 1000f, 0.65f, 0.4f);
            return new Color(32f / 255f, 227f / 255f, 187f / 255f);
        }

        public override string SpriteName(int data)
        {
            // Fisobs自动加载名为“icon_｛Type｝.png”的mod文件夹中的文件
            // 要使用它，只需删除png后缀：“icon_CentiShield”
            return "icon_Quetzalcoatl";
        }
    }
}
