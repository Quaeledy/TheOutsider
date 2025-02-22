using System.Globalization;

namespace TheOutsider.CustomLore.CustomObject.AlcedoMask
{
    public class AbstractAlcedoMask : AbstractPhysicalObject
    {
        public int colorSeed;

        public bool king;

        public bool scavKing;

        public string spriteOverride;

        public AbstractAlcedoMask(World world, AlcedoMask realizedObject, WorldCoordinate pos, EntityID ID, int colorSeed, bool king)
            : base(world, TheOutsiderEnums.AbstractObjectType.AlcedoMask, realizedObject, pos, ID)
        {
            this.colorSeed = colorSeed;
            this.king = king;
            spriteOverride = "";
        }

        public AbstractAlcedoMask(World world, AlcedoMask realizedObject, WorldCoordinate pos, EntityID ID, int colorSeed, bool king, bool scavKing, string spriteOverride)
            : this(world, realizedObject, pos, ID, colorSeed, king)
        {
            this.scavKing = scavKing;
            this.spriteOverride = spriteOverride;
        }

        public override string ToString()
        {
            string text = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}", ID.ToString(), type.ToString(), pos.SaveToString(), colorSeed, king ? "1" : "0");
            if (ModManager.MSC)
            {
                text += string.Format(CultureInfo.InvariantCulture, "<oA>{0}<oA>{1}", scavKing ? "1" : "0", spriteOverride);
            }
            text = SaveState.SetCustomData(this, text);
            return SaveUtils.AppendUnrecognizedStringAttrs(text, "<oA>", unrecognizedAttributes);
        }


        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
                realizedObject = new AlcedoMask(this, Room.realizedRoom.world);
        }
    }
}