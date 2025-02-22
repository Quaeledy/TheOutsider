using CustomOracleTx;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomOracle
{
    public class AMOracleRegistry : CustomOracleTx.CustomOracleTx
    {
        public static Oracle.OracleID AMOracle = new Oracle.OracleID("OSAM", true);
        public static float AMTalkPitch = 0.75f;

        public override string LoadRoom => "OSAM_AI";
        public override Oracle.OracleID OracleID => AMOracle;
        public override Oracle.OracleID InheritOracleID => Oracle.OracleID.SS;

        public AMOracleRegistry()
        {
            gravity = 0f;
            startPos = new Vector2(350f, 350f);

            pearlRegistry = new AMPearlRegistry();
            CustomOraclePearlRx.ApplyTreatment(pearlRegistry);
        }

        public override void LoadBehaviourAndSurroundings(ref Oracle oracle, Room room)
        {
            base.LoadBehaviourAndSurroundings(ref oracle, room);

            oracle.oracleBehavior = new AMOracleBehaviour(oracle);

            oracle.SetUpMarbles();
            room.gravity = 0f;
            for (int n = 0; n < room.updateList.Count; n++)
            {
                if (room.updateList[n] is AntiGravity)
                {
                    (room.updateList[n] as AntiGravity).active = false;
                    break;
                }
            }
            oracle.arm = new Oracle.OracleArm(oracle);
            Plugin.Log("Successfully load behaviours and surroundings!");
        }

        public override OracleGraphics InitCustomOracleGraphic(PhysicalObject ow)
        {
            return new AMOracleGraphics(ow);
        }
    }

    public class AMOracleColor
    {
        public static readonly Color Blue = new Color(40f / 255f, 102f / 255f, 141f / 255f);
        public static readonly Color LightBlue = new Color(145f / 255f, 255f / 255f, 255f / 255f);
        public static readonly Color DarkBlue = new Color(8f / 255f, 44f / 255f, 98f / 255f);

        public static readonly Color LightGreen = new Color(106f / 255f, 229f / 255f, 191f / 255f);
        public static readonly Color GrayishYellow = new Color(218f / 255f, 215f / 255f, 196f / 255f);
        public static readonly Color Orange = new Color(254f / 255f, 166f / 255f, 92f / 255f);

        public static readonly Color DarkGrey = new Color(71f / 256f, 76f / 256f, 86f / 256f);
        public static readonly Color VeryDarkGrey = new Color(44f / 256f, 48f / 256f, 57f / 256f);
        public static readonly Color Rose = new Color(255f / 255f, 67f / 255f, 115f / 255f);
        public static readonly Color Purple = new Color(111f / 255f, 28f / 255f, 213f / 255f);
    }

}
