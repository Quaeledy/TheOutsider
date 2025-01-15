﻿using Fisobs.Core;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomObject.AlcedoMask
{
    sealed class AlcedoMaskIcon : Icon
    {
        // Vanilla only gives you one int field to store all your custom data.
        // Here, that int field is used to store the shield's hue, scaled by 1000.
        // So, 0 is red and 70 is orange.
        public override int Data(AbstractPhysicalObject apo)
        {
            return 0;//apo is AbstractAlcedoMask shield ? (int)(shield.hue * 1000f) : 0;
        }

        public override Color SpriteColor(int data)
        {
            return new Color(210f / 255f, 202f / 255f, 108f / 255f);
            //return RWCustom.Custom.HSL2RGB(data / 1000f, 0.65f, 0.4f);
        }

        public override string SpriteName(int data)
        {
            // Fisobs autoloads the file in the mod folder named "icon_{Type}.png"
            // To use that, just remove the png suffix: "icon_AlcedoMask"
            return "icon_AlcedoMask";
        }
    }
}