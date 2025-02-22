using CustomOracleTx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TheOutsider.CustomLore.CustomOracle
{
    public class AMPearlRegistry : CustomOraclePearlTx
    {
        public static AbstractPhysicalObject.AbstractObjectType AMPearl = new AbstractPhysicalObject.AbstractObjectType("AMPearl", true);
        public static DataPearl.AbstractDataPearl.DataPearlType DataPearl_AM = new DataPearl.AbstractDataPearl.DataPearlType("AMPearl", true);
        public static Conversation.ID pearlConvID_AM = new Conversation.ID("AMPearl", true);
        public AMPearlRegistry() : base(AMPearl, DataPearl_AM, pearlConvID_AM)
        {
        }

        public override CustomOrbitableOraclePearl RealizeDataPearl(AbstractPhysicalObject abstractPhysicalObject, World world)
        {
            return new AMPearl(abstractPhysicalObject, world);
        }
    }

    public class AMPearl : CustomOrbitableOraclePearl
    {
        public AMPearl(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
        }

        public override void DataPearlApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            int num = Random.Range(0, 3);
            if (rCam.room.world.game.IsStorySession)
            {
                num = (abstractPhysicalObject as CustomOrbitableOraclePearl.AbstractCustomOraclePearl).color;
            }

            Color color = AMOracleColor.Blue;
            switch (num)
            {
                default:
                case 0:
                    color = AMOracleColor.DarkBlue;
                    break;
                case 1:
                    color = AMOracleColor.LightGreen;
                    break;
                case 2:
                    color = AMOracleColor.Orange;
                    break;
                case 3:
                    color = AMOracleColor.GrayishYellow;
                    break;
            }
            darkness = 0f;
            this.color = color;
        }
    }
}
