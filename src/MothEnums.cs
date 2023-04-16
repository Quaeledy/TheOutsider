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

namespace TheOutsider
{
    public static class MothEnums
    {
        public static void RegisterValues()
        {
            MothBuzz = new SoundID("mothbuzz", true);
            //MothFlower = new AbstractPhysicalObject.AbstractObjectType("MothFlower", true);
        }

        public static void UnregisterValues()
        {
            Unregister(MothBuzz);
        }

        private static void Unregister<T>(ExtEnum<T> extEnum) where T : ExtEnum<T>
        {
            if (extEnum != null)
            {
                extEnum.Unregister();
            }
        }

        public static SoundID MothBuzz;

        //public static AbstractPhysicalObject.AbstractObjectType MothFlower;
    }
}