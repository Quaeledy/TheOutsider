using RWCustom;
using TheOutsider.Player_Hooks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace TheOutsider.PlayerGraphics_Hooks
{
    public class PlayerGraphicsHooks
    {
        public static void Init()
        {
            //Antennae.Init();
            //Wings.Init();
            //SwallowTail.Init();
            //Speckles.Init();
            //Flare.Init();


            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.Reset += PlayerGraphics_Reset;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.JollyUniqueColorMenu += PlayerGraphics_JollyUniqueColorMenu;
        }


        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var outsider))
            {
                outsider.wings = new Wings(self, outsider);
                outsider.antennaes = new Antennaes(self, outsider);
                outsider.swallowTails = new SwallowTails(self, outsider);
                outsider.speckles = new Speckles(self, outsider);
                outsider.flare = new Flare(self, outsider);
            }
        }

        private static void PlayerGraphics_Reset(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
        {
            orig(self);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var outsider))
            {
                //防止拉丝
                outsider.wings.Reset();
                outsider.antennaes.Reset();
                outsider.swallowTails.Reset();
            }
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var outsider))
            {
                outsider.wings.InitiateSprites(sLeaser, rCam);
                outsider.antennaes.InitiateSprites(sLeaser, rCam);
                outsider.swallowTails.InitiateSprites(sLeaser, rCam);
                outsider.speckles.InitiateSprites(sLeaser, rCam);
                outsider.flare.InitiateSprites(sLeaser, rCam);

                self.AddToContainer(sLeaser, rCam, null);
            }
        }

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var outsider))
            {
                outsider.wings.AddToContainer(sLeaser, rCam, newContatiner);
                outsider.antennaes.AddToContainer(sLeaser, rCam, newContatiner);
                outsider.swallowTails.AddToContainer(sLeaser, rCam, newContatiner);
                outsider.speckles.AddToContainer(sLeaser, rCam, newContatiner);
            }
        }

        public static void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var outsider))
            {
                outsider.swallowTails.ApplyPalette(sLeaser, rCam, palette);
            }
        }

        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var outsider))
            {
                outsider.wings.Update();
                outsider.antennaes.Update();
                outsider.swallowTails.Update();
            }
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (PlayerHooks.PlayerData.TryGetValue(self.player, out var outsider))
            {
                for (int i = 0; i <= 8; i++)
                    sLeaser.sprites[0].color = outsider.GetBodyColor();
                sLeaser.sprites[9].color = outsider.GetEyesColor();

                outsider.wings.DrawSprites(sLeaser, rCam, timeStacker, camPos);
                outsider.antennaes.DrawSprites(sLeaser, rCam, timeStacker, camPos);
                outsider.swallowTails.DrawSprites(sLeaser, rCam, timeStacker, camPos);
                outsider.speckles.DrawSprites(sLeaser, rCam, timeStacker, camPos);
                outsider.flare.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }
        }

        private static Color PlayerGraphics_JollyUniqueColorMenu(On.PlayerGraphics.orig_JollyUniqueColorMenu orig, SlugcatStats.Name slugName, SlugcatStats.Name reference, int playerNumber)
        {
            Color result = orig(slugName, reference, playerNumber);
            if (slugName == Plugin.SlugName)
            {
                result = new Color(106f / 255f, 229f / 255f, 191f / 255f);
                if (playerNumber == 0 && Custom.rainWorld.options.jollyColorMode != Options.JollyColorMode.CUSTOM)
                {
                    return result;
                }
                if ((Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.AUTO && playerNumber > 0) || Custom.rainWorld.options.jollyColorMode == Options.JollyColorMode.CUSTOM)
                {
                    return PlayerGraphics.JollyColor(playerNumber, 2);
                }
            }
            return result;
        }

        //向量旋转
        public static Vector2 VectorRotation(Vector2 vector, float rotation)
        {
            Vector2 newVector = new Vector2(vector.x * rotation, vector.y * rotation);
            newVector.x = vector.x * Mathf.Cos(rotation) + vector.y * Mathf.Sin(rotation);
            newVector.y = -vector.x * Mathf.Sin(rotation) + vector.y * Mathf.Cos(rotation);
            return newVector;
        }
    }
}
