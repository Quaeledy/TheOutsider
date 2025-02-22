using Fisobs.Core;
using System;
using TheOutsider.CustomLore.CustomCreature.Alcedo;
using TheOutsider.CustomLore.CustomCreature.MothPup;
using TheOutsider.CustomLore.CustomObject.AlcedoMask;

namespace TheOutsider
{
    public static class TheOutsiderEnums
    {
        public static void RegisterAllEnumExtensions()
        {
            AbstractObjectType.RegisterValues();
            CreatureTemplateType.RegisterValues();
            OutsiderSoundID.RegisterValues();

            Plugin.Log("TheOutsiderEnums RegisterValues!");
        }

        public static void UnregisterAllEnumExtensions()
        {
            AbstractObjectType.UnregisterValues();
            CreatureTemplateType.UnregisterValues();
            OutsiderSoundID.UnregisterValues();
        }

        public static void RegisterAllFisobsContent()
        {
            try
            {
                CreatureTemplateType.RegisterFisobsContent();
                AbstractObjectType.RegisterFisobsContent();

                Plugin.Log("TheOutsiderEnums Register Fisobs Content!");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                throw;
            }
        }

        private static void Unregister<T>(ExtEnum<T> extEnum) where T : ExtEnum<T>
        {
            if (extEnum != null)
            {
                extEnum.Unregister();
            }
        }


        internal class CreatureTemplateType
        {
            public static void RegisterFisobsContent()
            {
                Content.Register(new MothPupCritob());
                //Content.Register(new AlcedoCritob());
                //Content.Register(new QuetzalcoatlCritob());
            }

            public static void RegisterValues()
            {
                /*
                Quetzalcoatl = new CreatureTemplate.Type("Quetzalcoatl", true);
                GuardQuetzalcoatl = new CreatureTemplate.Type("GuardQuetzalcoatl", true);
                CorruptQuetzalcoatl = new CreatureTemplate.Type("CorruptQuetzalcoatl", true);
                Alcedo = new CreatureTemplate.Type("Alcedo", true);
                TerrorSpider = new CreatureTemplate.Type("TerrorSpider", true);
                MothSlugNPC = new CreatureTemplate.Type("MothSlugNPC", true);
                Coral = new CreatureTemplate.Type("Coral", true);
                CoralMatrix = new CreatureTemplate.Type("CoralMatrix", true);
                FluorescentButterfly = new CreatureTemplate.Type("FluorescentButterfly", true);*/
            }

            public static void UnregisterValues()
            {
                Unregister(Mothpup);
                Unregister(Alcedo);
            }

            public static readonly CreatureTemplate.Type Mothpup = MothPupCritob.Mothpup;
            public static readonly CreatureTemplate.Type Alcedo = AlcedoCritob.Alcedo;
            public static readonly MultiplayerUnlocks.SandboxUnlockID MothPupUnlock = MothPupCritob.MothPupUnlock;
            public static readonly MultiplayerUnlocks.SandboxUnlockID AlcedoUnlock = AlcedoCritob.AlcedoUnlock;

            /*
            public static void UnregisterValues()
            {
                //羽蛇
                CreatureTemplate.Type quetzalcoatl = Quetzalcoatl;
                if (quetzalcoatl != null)
                {
                    quetzalcoatl.Unregister();
                }
                Quetzalcoatl = null;

                CreatureTemplate.Type guardQuetzalcoatl = GuardQuetzalcoatl;
                if (guardQuetzalcoatl != null)
                {
                    guardQuetzalcoatl.Unregister();
                }
                GuardQuetzalcoatl = null;

                CreatureTemplate.Type corruptQuetzalcoatl = CorruptQuetzalcoatl;
                if (corruptQuetzalcoatl != null)
                {
                    corruptQuetzalcoatl.Unregister();
                }
                CorruptQuetzalcoatl = null;

                //翠鸟
                CreatureTemplate.Type alcedo = Alcedo;
                if (alcedo != null)
                {
                    alcedo.Unregister();
                }
                Alcedo = null;

                //恐怖狼蛛
                CreatureTemplate.Type terrorSpider = TerrorSpider;
                if (terrorSpider != null)
                {
                    terrorSpider.Unregister();
                }
                TerrorSpider = null;

                //蛾猫崽
                CreatureTemplate.Type mothSlugNPC = MothSlugNPC;
                if (mothSlugNPC != null)
                {
                    mothSlugNPC.Unregister();
                }
                MothSlugNPC = null;

                //子珊瑚虫
                CreatureTemplate.Type coral = Coral;
                if (coral != null)
                {
                    coral.Unregister();
                }
                Coral = null;

                //母珊瑚虫
                CreatureTemplate.Type coralMatrix = CoralMatrix;
                if (coralMatrix != null)
                {
                    coralMatrix.Unregister();
                }
                CoralMatrix = null;

                //荧光蝴蝶
                CreatureTemplate.Type fluorescentButterfly = FluorescentButterfly;
                if (fluorescentButterfly != null)
                {
                    fluorescentButterfly.Unregister();
                }
                FluorescentButterfly = null;
            }

            public static CreatureTemplate.Type Quetzalcoatl;
            public static CreatureTemplate.Type GuardQuetzalcoatl;
            public static CreatureTemplate.Type CorruptQuetzalcoatl;
            public static CreatureTemplate.Type Alcedo;
            public static CreatureTemplate.Type TerrorSpider;
            public static CreatureTemplate.Type MothSlugNPC;
            public static CreatureTemplate.Type Coral;
            public static CreatureTemplate.Type CoralMatrix;
            public static CreatureTemplate.Type FluorescentButterfly;*/
        }

        internal class AbstractObjectType
        {
            public static void RegisterFisobsContent()
            {
                //Content.Register(new AlcedoMaskFisob());
            }

            public static void RegisterValues()
            {/*
                AlcedoMask = new("AlcedoMask", true);
                AlcedoMaskUnlock = new("AlcedoMask", true);*/
            }

            public static void UnregisterValues()
            {
                Unregister(AlcedoMask);
                Unregister(AlcedoMaskUnlock);
            }

            public static AbstractPhysicalObject.AbstractObjectType AlcedoMask;
            public static MultiplayerUnlocks.SandboxUnlockID AlcedoMaskUnlock;
        }

        internal class OutsiderSoundID
        {
            public static void RegisterValues()
            {
                MothBuzz = new SoundID("mothbuzz", true);
            }

            public static void UnregisterValues()
            {
                Unregister(MothBuzz);
            }

            public static SoundID MothBuzz;
        }
    }
}