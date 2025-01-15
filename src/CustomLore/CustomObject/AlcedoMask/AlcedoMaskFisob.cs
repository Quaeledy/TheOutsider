using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace TheOutsider.CustomLore.CustomObject.AlcedoMask
{
    sealed class AlcedoMaskFisob : Fisob
    {
        public AlcedoMaskFisob() : base(OutsiderEnums.AbstractObjectType.AlcedoMask)
        {
            // Fisobs auto-loads the `icon_AlcedoMask` embedded resource as a texture.
            // See `AlcedoMasks.csproj` for how you can add embedded resources to your project.

            // If you want a simple grayscale icon, you can omit the following line.
            Icon = new AlcedoMaskIcon();

            SandboxPerformanceCost = new(linear: 0.35f, exponential: 0f);

            RegisterUnlock(OutsiderEnums.AbstractObjectType.AlcedoMaskUnlock, parent: OutsiderEnums.CreatureTemplateType.AlcedoUnlock, data: 70);
        }
        
        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
        {
            // Centi shield data is just floats separated by ; characters.
            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 5)
            {
                p = new string[5];
            }

            var result = new AbstractAlcedoMask(world, null, saveData.Pos, saveData.ID, world.game.GetNewID().RandomSeed, false);
            /*
            // If this is coming from a sandbox unlock, the hue and size should depend on the data value (see AlcedoMaskIcon below).
            if (unlock is SandboxUnlock u)
            {
                result.hue = u.Data / 1000f;

                if (u.Data == 0)
                {
                    result.scaleX += 0.2f;
                    result.scaleY += 0.2f;
                }
            }*/

            return result;
        }
        
        private static readonly AlcedoMaskProperties properties = new();

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            // If you need to use the forObject parameter, pass it to your ItemProperties class's constructor.
            // The Mosquitoes example demonstrates this.
            return properties;
        }
    }
}