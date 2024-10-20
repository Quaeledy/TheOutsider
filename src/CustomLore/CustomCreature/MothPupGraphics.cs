using UnityEngine;
using RWCustom;

namespace TheOutsider.CustomLore.CustomCreature
{
    sealed class MothPupGraphics : PlayerGraphics
    {/*
        const int meshSegs = 9;

        const float squeeze = -0.1f;
        const float squirmAdd = 0;
        const float squirmWidth = 0;
        const float squirmAmp = 0;

        readonly MothPup quet;
        readonly Vector2[,] body = new Vector2[2, 3];
        readonly float[,] squirm = new float[meshSegs, 3];
        readonly float sizeFac;

        float squirmOffset;
        float darkness;
        float lastDarkness;
        Color yellow;
        float wingFlap;
        float lastWingFlap;
        RoomPalette roomPalette;
        ChunkSoundEmitter? soundLoop;

        readonly TriangleMesh[] m = new TriangleMesh[2]; // mesh sprites 0 and 1
        readonly CustomFSprite[] w = new CustomFSprite[2]; // wing sprites 0 and 1
        */
        public MothPupGraphics(Player mothPup) : base(mothPup)
        {/*
            quet = mothPup;

            var state = Random.state;
            Random.InitState(mothPup.abstractCreature.ID.RandomSeed);
            sizeFac = Custom.ClampedRandomVariation(8f, 2f, 0.5f);
            body = new Vector2[2, 3];
            Random.state = state;*/
        }
        
        public override void Reset()
        {
            base.Reset();
        }

        public override void Update()
        {
            base.Update();
        }
        
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            base.AddToContainer(sLeaser, rCam, newContainer);
            /*
            newContainer ??= rCam.ReturnFContainer("Midground");

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                newContainer.AddChild(sLeaser.sprites[i]);
            }*/
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
        }
    }

}
