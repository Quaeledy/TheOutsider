using MoreSlugcats;
using System.Collections.Generic;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature
{
    sealed class AlcedoAbstractAI : AbstractCreatureAI
    {
        public AlcedoAbstractAI(World world, AbstractCreature parent) : base(world, parent)
        {
            this.checkRooms = new List<WorldCoordinate>();
            this.AddRandomCheckRoom();
        }

        public bool IsMiros
        {
            get
            {
                return ModManager.MSC && this.parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
            }
        }

        public override void AbstractBehavior(int time)
        {
            if (this.path.Count > 0 && this.parent.realizedCreature == null)
            {
                base.FollowPath(time);
                return;
            }
            if (!this.IsMiros && !(this.parent.state as Alcedo.AlcedoState).mask)
            {
                if (this.lostMask != null)
                {
                    bool flag = this.RoomViableRoamDestination(this.lostMask.pos.room);
                    if (this.parent.pos.room != this.lostMask.pos.room && base.destination.room != this.lostMask.pos.room && base.MigrationDestination.room != this.lostMask.pos.room && flag)
                    {
                        this.GoToRoom(this.lostMask.pos.room);
                        if (this.parent.realizedCreature == null && this.parent.Room.realizedRoom == null && this.parent.pos.room != base.destination.room && base.destination.room == this.lostMask.pos.room)
                        {
                            this.parent.Move(base.destination);
                        }
                    }
                    if (flag)
                    {
                        return;
                    }
                }
                else if (!this.checkedForLostMask)
                {
                    this.checkedForLostMask = true;
                    int num = 0;
                    while (num < this.world.NumberOfRooms && this.lostMask == null)
                    {
                        AbstractRoom abstractRoom = this.world.GetAbstractRoom(this.world.firstRoomIndex + num);
                        for (int i = 0; i < abstractRoom.entities.Count; i++)
                        {
                            if (abstractRoom.entities[i] is AbstractPhysicalObject && abstractRoom.entities[i].ID == this.parent.ID &&
                                (abstractRoom.entities[i] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
                            {
                                Plugin.Log("MASK FOUND");
                                this.lostMask = (abstractRoom.entities[i] as AbstractPhysicalObject);
                                break;
                            }
                        }
                        num++;
                    }
                }
            }
            if (!this.IsMiros && this.world.skyAccessNodes.Length == 0)
            {
                return;
            }
            if (this.IsMiros && this.world.skyAccessNodes.Length == 0 && this.world.sideAccessNodes.Length == 0)
            {
                return;
            }
            if (this.checkRooms.Count == 0)
            {
                this.AddRandomCheckRoom();
                return;
            }
            if (base.destination.CompareDisregardingTile(this.checkRooms[0]))
            {
                this.timeInRoom += time;
                if (this.timeInRoom > 620)
                {
                    this.checkRooms.RemoveAt(0);
                    this.timeInRoom -= 620;
                    return;
                }
            }
            else
            {
                base.SetDestination(this.checkRooms[0]);
            }
        }

        private void AddRandomCheckRoom()
        {
            bool flag = false;
            if (UnityEngine.Random.value > 0.5f && this.parent.pos.room != this.dontGoToThisRoom && UnityEngine.Random.value < this.world.GetAbstractRoom(this.parent.pos).AttractionValueForCreature(this.parent.creatureTemplate.type))
            {
                for (int i = 0; i < this.world.GetAbstractRoom(this.parent.pos).nodes.Length; i++)
                {
                    if (this.world.GetAbstractRoom(this.parent.pos).nodes[i].type == AbstractRoomNode.Type.SkyExit || (this.IsMiros && this.world.GetAbstractRoom(this.parent.pos).nodes[i].type == AbstractRoomNode.Type.SideExit))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                this.dontGoToThisRoom = this.parent.pos.room;
                this.AddRoomClusterToCheckList(this.world.GetAbstractRoom(this.parent.pos));
                return;
            }
            float num = 0f;
            for (int j = 0; j < this.world.skyAccessNodes.Length; j++)
            {
                if (this.RoomViableRoamDestination(this.world.skyAccessNodes[j].room))
                {
                    num += Mathf.Pow(this.world.GetAbstractRoom(this.world.skyAccessNodes[j]).SizeDependentAttractionValueForCreature(this.parent.creatureTemplate.type), 2f);
                }
            }
            float num2 = UnityEngine.Random.value * num;
            for (int k = 0; k < this.world.skyAccessNodes.Length; k++)
            {
                if (this.RoomViableRoamDestination(this.world.skyAccessNodes[k].room))
                {
                    float num3 = Mathf.Pow(this.world.GetAbstractRoom(this.world.skyAccessNodes[k]).SizeDependentAttractionValueForCreature(this.parent.creatureTemplate.type), 2f);
                    if (num2 < num3)
                    {
                        this.AddRoomClusterToCheckList(this.world.GetAbstractRoom(this.world.skyAccessNodes[k]));
                        break;
                    }
                    num2 -= num3;
                }
            }
            if (this.IsMiros)
            {
                num = 0f;
                for (int l = 0; l < this.world.sideAccessNodes.Length; l++)
                {
                    if (this.RoomViableRoamDestination(this.world.sideAccessNodes[l].room))
                    {
                        num += Mathf.Pow(this.world.GetAbstractRoom(this.world.sideAccessNodes[l]).SizeDependentAttractionValueForCreature(this.parent.creatureTemplate.type), 2f);
                    }
                }
                num2 = UnityEngine.Random.value * num;
                for (int m = 0; m < this.world.sideAccessNodes.Length; m++)
                {
                    if (this.RoomViableRoamDestination(this.world.sideAccessNodes[m].room))
                    {
                        float num4 = Mathf.Pow(this.world.GetAbstractRoom(this.world.sideAccessNodes[m]).SizeDependentAttractionValueForCreature(this.parent.creatureTemplate.type), 2f);
                        if (num2 < num4)
                        {
                            this.AddRoomClusterToCheckList(this.world.GetAbstractRoom(this.world.sideAccessNodes[m]));
                            return;
                        }
                        num2 -= num4;
                    }
                }
            }
        }

        private void AddRoomClusterToCheckList(AbstractRoom originalRoom)
        {
            List<AlcedoAbstractAI.CoordAndInt> list = new List<AlcedoAbstractAI.CoordAndInt>();
            if (originalRoom.index != this.parent.pos.room && (originalRoom.AnySkyAccess || (this.IsMiros && originalRoom.AnySideAccess)) && this.RoomViableRoamDestination(originalRoom.index))
            {
                list.Add(new AlcedoAbstractAI.CoordAndInt(new WorldCoordinate(originalRoom.index, -1, -1, originalRoom.RandomRelevantNode(this.parent.creatureTemplate)), 100000 + UnityEngine.Random.Range(0, 50)));
            }
            for (int i = 0; i < originalRoom.connections.Length; i++)
            {
                if (originalRoom.connections[i] > -1 && this.RoomViableRoamDestination(originalRoom.connections[i]) && UnityEngine.Random.value < this.world.GetAbstractRoom(originalRoom.connections[i]).AttractionValueForCreature(this.parent.creatureTemplate.type) * 1.5f)
                {
                    AbstractRoom abstractRoom = this.world.GetAbstractRoom(originalRoom.connections[i]);
                    if (abstractRoom.AnySkyAccess || (this.IsMiros && originalRoom.AnySideAccess))
                    {
                        int num = UnityEngine.Random.Range(50, 500 + 2000 * (int)abstractRoom.SizeDependentAttractionValueForCreature(this.parent.creatureTemplate.type));
                        list.Add(new AlcedoAbstractAI.CoordAndInt(new WorldCoordinate(abstractRoom.index, -1, -1, abstractRoom.RandomRelevantNode(this.parent.creatureTemplate)), num + UnityEngine.Random.Range(0, 50)));
                    }
                }
            }
            while (list.Count > 0)
            {
                int num2 = -1;
                int num3 = int.MinValue;
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].coord.NodeDefined && list[j].i > num3)
                    {
                        num3 = list[j].i;
                        num2 = j;
                    }
                }
                if (num2 > -1)
                {
                    this.checkRooms.Add(list[num2].coord);
                }
                list.RemoveAt(num2);
            }
        }

        private bool RoomViableRoamDestination(int room)
        {
            if (ModManager.MSC && this.world.GetAbstractRoom(room).offScreenDen && this.world.game.IsArenaSession && this.world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
            {
                return false;
            }
            if (this.IsMiros && this.world.scavengersWorldAI != null)
            {
                for (int i = 0; i < this.world.scavengersWorldAI.outPosts.Count; i++)
                {
                    if (this.world.scavengersWorldAI.outPosts[i].room == room)
                    {
                        return false;
                    }
                }
            }
            if (this.world.GetAbstractRoom(room).AttractionForCreature(this.parent.creatureTemplate.type) == AbstractRoom.CreatureRoomAttraction.Forbidden)
            {
                return false;
            }
            for (int j = 0; j < this.world.GetAbstractRoom(room).nodes.Length; j++)
            {
                if ((this.world.GetAbstractRoom(room).nodes[j].type == AbstractRoomNode.Type.SkyExit || (this.IsMiros && this.world.GetAbstractRoom(room).nodes[j].type == AbstractRoomNode.Type.SideExit)) && this.world.GetAbstractRoom(room).nodes[j].entranceWidth > 4)
                {
                    return true;
                }
            }
            return false;
        }

        private void GoToRoom(int room)
        {
            if (!this.RoomViableRoamDestination(room))
            {
                return;
            }
            List<int> list = new List<int>();
            for (int i = 0; i < this.world.GetAbstractRoom(room).nodes.Length; i++)
            {
                if ((this.world.GetAbstractRoom(room).nodes[i].type == AbstractRoomNode.Type.SkyExit || (this.IsMiros && this.world.GetAbstractRoom(room).nodes[i].type == AbstractRoomNode.Type.SideExit)) && this.world.GetAbstractRoom(room).nodes[i].entranceWidth > 4)
                {
                    list.Add(i);
                }
            }
            if (list.Count == 0)
            {
                return;
            }
            base.SetDestination(new WorldCoordinate(room, -1, -1, list[UnityEngine.Random.Range(0, list.Count)]));
        }

        public List<WorldCoordinate> checkRooms;
        public int timeInRoom;
        public int dontGoToThisRoom = -1;
        public AbstractPhysicalObject lostMask;
        private bool checkedForLostMask;

        private struct CoordAndInt
        {
            public CoordAndInt(WorldCoordinate coord, int i)
            {
                this.coord = coord;
                this.i = i;
            }

            public WorldCoordinate coord;
            public int i;
        }
    }
}
