using RWCustom;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature
{
    internal class AlcedoAppendage : BodyPart
    {
        public AlcedoGraphics kGraphics;

        public int side;

        public int index;

        public new float rad;

        public float stretchedRad;

        public float conRad;

        public AlcedoAppendage(AlcedoGraphics kGraphics, int side, int index, float rad, float conRad)
            : base(kGraphics)
        {
            this.kGraphics = kGraphics;
            this.side = side;
            this.index = index;
            this.rad = rad;
            this.conRad = conRad;
        }

        public override void Update()
        {
            base.Update();
            lastPos = pos;
            pos += vel;
            vel *= 0.98f;
            vel.y += (kGraphics.owner.room.PointSubmerged(pos) ? 0.2f : (-0.9f));
            vel -= kGraphics.alcedo.mainBodyChunk.vel;
            vel *= 0.9f;
            vel += kGraphics.alcedo.mainBodyChunk.vel;
            if (index < 4)
            {
                vel += (kGraphics.AppendageConnectionPos(side, 1f) - kGraphics.alcedo.bodyChunks[3 - side].pos).normalized * 3.5f / (index + 1);
            }
            if (index == 0)
            {
                Vector2 vector = Custom.DirVec(pos, kGraphics.AppendageConnectionPos(side, 1f));
                float num = Vector2.Distance(pos, kGraphics.AppendageConnectionPos(side, 1f));
                float num2 = conRad;
                pos -= vector * (num2 - num);
                vel -= vector * (num2 - num);
                stretchedRad = rad * Mathf.Clamp(Mathf.Pow(num2 / num, 0.1f), 0.5f, 1.5f);
            }
            else
            {
                Vector2 vector2 = Custom.DirVec(pos, kGraphics.appendages[side][index - 1].pos);
                float num3 = Vector2.Distance(pos, kGraphics.appendages[side][index - 1].pos);
                float num4 = conRad;
                pos -= vector2 * (num4 - num3) * 0.5f;
                vel -= vector2 * (num4 - num3) * 0.5f;
                kGraphics.appendages[side][index - 1].pos += vector2 * (num4 - num3) * 0.5f;
                kGraphics.appendages[side][index - 1].vel += vector2 * (num4 - num3) * 0.5f;
                stretchedRad = rad * Mathf.Clamp(Mathf.Pow(num4 / num3, 0.1f), 0.5f, 1.5f);
            }
            if (!kGraphics.alcedo.IsMiros && !Custom.DistLess(pos, kGraphics.AppendageConnectionPos(side, 1f), 25f + 7f * (float)index))
            {
                Vector2 vector3 = Custom.DirVec(pos, kGraphics.AppendageConnectionPos(side, 1f));
                float num5 = Vector2.Distance(pos, kGraphics.AppendageConnectionPos(side, 1f));
                float num6 = 25f + 7f * (float)index;
                pos -= vector3 * (num6 - num5);
                vel -= vector3 * (num6 - num5);
            }
            for (int i = 0; i < 4; i++)
            {
                PushFromPoint(kGraphics.alcedo.bodyChunks[i].pos, kGraphics.alcedo.bodyChunks[i].rad, 1f);
            }
            if (!OnOtherSideOfTerrain(kGraphics.AppendageConnectionPos(side, 1f), 100f))
            {
                PushOutOfTerrain(kGraphics.alcedo.room, kGraphics.AppendageConnectionPos(side, 1f));
            }
        }

        public float FromBaseRadius()
        {
            if (index == 0)
            {
                return conRad;
            }
            return (owner as AlcedoGraphics).appendages[side][index - 1].FromBaseRadius() + conRad;
        }
    }
}
