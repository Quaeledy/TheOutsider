using Fisobs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheOutsider.CustomLore.CustomCreature
{
    public class CreatureTemplateType
    {
        public static void RegisterValues()
        {
            Content.Register(new QuetzalcoatlCritob());
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
}
