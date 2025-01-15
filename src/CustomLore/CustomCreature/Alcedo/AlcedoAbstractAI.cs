using MoreSlugcats;
using System.Collections.Generic;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature.Alcedo
{
    sealed class AlcedoAbstractAI : AbstractCreatureAI
    {
        public AlcedoAbstractAI(World world, AbstractCreature parent) : base(world, parent)
        {
            checkRooms = new List<WorldCoordinate>();
            AddRandomCheckRoom();
        }

        public bool IsMiros
        {
            get
            {
                return ModManager.MSC && parent.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture;
            }
        }

        public override void AbstractBehavior(int time)
        {
            if (path.Count > 0 && parent.realizedCreature == null)
            {
                FollowPath(time);
                return;
            }
            if (!IsMiros && !(parent.state as Alcedo.AlcedoState).mask)
            {
                if (lostMask != null)
                {
                    bool flag = RoomViableRoamDestination(lostMask.pos.room);
                    if (parent.pos.room != lostMask.pos.room && destination.room != lostMask.pos.room && MigrationDestination.room != lostMask.pos.room && flag)
                    {
                        GoToRoom(lostMask.pos.room);
                        if (parent.realizedCreature == null && parent.Room.realizedRoom == null && parent.pos.room != destination.room && destination.room == lostMask.pos.room)
                        {
                            parent.Move(destination);
                        }
                    }
                    if (flag)
                    {
                        return;
                    }
                }
                else if (!checkedForLostMask)
                {
                    checkedForLostMask = true;
                    int num = 0;
                    while (num < world.NumberOfRooms && lostMask == null)
                    {
                        AbstractRoom abstractRoom = world.GetAbstractRoom(world.firstRoomIndex + num);
                        for (int i = 0; i < abstractRoom.entities.Count; i++)
                        {
                            if (abstractRoom.entities[i] is AbstractPhysicalObject && abstractRoom.entities[i].ID == parent.ID &&
                                (abstractRoom.entities[i] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
                            {
                                Plugin.Log("MASK FOUND");
                                lostMask = abstractRoom.entities[i] as AbstractPhysicalObject;
                                break;
                            }
                        }
                        num++;
                    }
                }
            }
            if (!IsMiros && world.skyAccessNodes.Length == 0)
            {
                return;
            }
            if (IsMiros && world.skyAccessNodes.Length == 0 && world.sideAccessNodes.Length == 0)
            {
                return;
            }
            if (checkRooms.Count == 0)
            {
                AddRandomCheckRoom();
                return;
            }
            if (destination.CompareDisregardingTile(checkRooms[0]))
            {
                timeInRoom += time;
                if (timeInRoom > 620)
                {
                    checkRooms.RemoveAt(0);
                    timeInRoom -= 620;
                    return;
                }
            }
            else
            {
                SetDestination(checkRooms[0]);
            }
        }

        private void AddRandomCheckRoom()
        {
            bool flag = false;
            if (Random.value > 0.5f && parent.pos.room != dontGoToThisRoom && Random.value < world.GetAbstractRoom(parent.pos).AttractionValueForCreature(parent.creatureTemplate.type))
            {
                for (int i = 0; i < world.GetAbstractRoom(parent.pos).nodes.Length; i++)
                {
                    if (world.GetAbstractRoom(parent.pos).nodes[i].type == AbstractRoomNode.Type.SkyExit || IsMiros && world.GetAbstractRoom(parent.pos).nodes[i].type == AbstractRoomNode.Type.SideExit)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (flag)
            {
                dontGoToThisRoom = parent.pos.room;
                AddRoomClusterToCheckList(world.GetAbstractRoom(parent.pos));
                return;
            }
            float num = 0f;
            for (int j = 0; j < world.skyAccessNodes.Length; j++)
            {
                if (RoomViableRoamDestination(world.skyAccessNodes[j].room))
                {
                    num += Mathf.Pow(world.GetAbstractRoom(world.skyAccessNodes[j]).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type), 2f);
                }
            }
            float num2 = Random.value * num;
            for (int k = 0; k < world.skyAccessNodes.Length; k++)
            {
                if (RoomViableRoamDestination(world.skyAccessNodes[k].room))
                {
                    float num3 = Mathf.Pow(world.GetAbstractRoom(world.skyAccessNodes[k]).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type), 2f);
                    if (num2 < num3)
                    {
                        AddRoomClusterToCheckList(world.GetAbstractRoom(world.skyAccessNodes[k]));
                        break;
                    }
                    num2 -= num3;
                }
            }
            if (IsMiros)
            {
                num = 0f;
                for (int l = 0; l < world.sideAccessNodes.Length; l++)
                {
                    if (RoomViableRoamDestination(world.sideAccessNodes[l].room))
                    {
                        num += Mathf.Pow(world.GetAbstractRoom(world.sideAccessNodes[l]).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type), 2f);
                    }
                }
                num2 = Random.value * num;
                for (int m = 0; m < world.sideAccessNodes.Length; m++)
                {
                    if (RoomViableRoamDestination(world.sideAccessNodes[m].room))
                    {
                        float num4 = Mathf.Pow(world.GetAbstractRoom(world.sideAccessNodes[m]).SizeDependentAttractionValueForCreature(parent.creatureTemplate.type), 2f);
                        if (num2 < num4)
                        {
                            AddRoomClusterToCheckList(world.GetAbstractRoom(world.sideAccessNodes[m]));
                            return;
                        }
                        num2 -= num4;
                    }
                }
            }
        }

        private void AddRoomClusterToCheckList(AbstractRoom originalRoom)
        {
            List<CoordAndInt> list = new List<CoordAndInt>();
            if (originalRoom.index != parent.pos.room && (originalRoom.AnySkyAccess || IsMiros && originalRoom.AnySideAccess) && RoomViableRoamDestination(originalRoom.index))
            {
                list.Add(new CoordAndInt(new WorldCoordinate(originalRoom.index, -1, -1, originalRoom.RandomRelevantNode(parent.creatureTemplate)), 100000 + Random.Range(0, 50)));
            }
            for (int i = 0; i < originalRoom.connections.Length; i++)
            {
                if (originalRoom.connections[i] > -1 && RoomViableRoamDestination(originalRoom.connections[i]) && Random.value < world.GetAbstractRoom(originalRoom.connections[i]).AttractionValueForCreature(parent.creatureTemplate.type) * 1.5f)
                {
                    AbstractRoom abstractRoom = world.GetAbstractRoom(originalRoom.connections[i]);
                    if (abstractRoom.AnySkyAccess || IsMiros && originalRoom.AnySideAccess)
                    {
                        int num = Random.Range(50, 500 + 2000 * (int)abstractRoom.SizeDependentAttractionValueForCreature(parent.creatureTemplate.type));
                        list.Add(new CoordAndInt(new WorldCoordinate(abstractRoom.index, -1, -1, abstractRoom.RandomRelevantNode(parent.creatureTemplate)), num + Random.Range(0, 50)));
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
                    checkRooms.Add(list[num2].coord);
                }
                list.RemoveAt(num2);
            }
        }

        private bool RoomViableRoamDestination(int room)
        {
            if (ModManager.MSC && world.GetAbstractRoom(room).offScreenDen && world.game.IsArenaSession && world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
            {
                return false;
            }
            if (IsMiros && world.scavengersWorldAI != null)
            {
                for (int i = 0; i < world.scavengersWorldAI.outPosts.Count; i++)
                {
                    if (world.scavengersWorldAI.outPosts[i].room == room)
                    {
                        return false;
                    }
                }
            }
            if (world.GetAbstractRoom(room).AttractionForCreature(parent.creatureTemplate.type) == AbstractRoom.CreatureRoomAttraction.Forbidden)
            {
                return false;
            }
            for (int j = 0; j < world.GetAbstractRoom(room).nodes.Length; j++)
            {
                if ((world.GetAbstractRoom(room).nodes[j].type == AbstractRoomNode.Type.SkyExit || IsMiros && world.GetAbstractRoom(room).nodes[j].type == AbstractRoomNode.Type.SideExit) && world.GetAbstractRoom(room).nodes[j].entranceWidth > 4)
                {
                    return true;
                }
            }
            return false;
        }

        private void GoToRoom(int room)
        {
            if (!RoomViableRoamDestination(room))
            {
                return;
            }
            List<int> list = new List<int>();
            for (int i = 0; i < world.GetAbstractRoom(room).nodes.Length; i++)
            {
                if ((world.GetAbstractRoom(room).nodes[i].type == AbstractRoomNode.Type.SkyExit || IsMiros && world.GetAbstractRoom(room).nodes[i].type == AbstractRoomNode.Type.SideExit) && world.GetAbstractRoom(room).nodes[i].entranceWidth > 4)
                {
                    list.Add(i);
                }
            }
            if (list.Count == 0)
            {
                return;
            }
            SetDestination(new WorldCoordinate(room, -1, -1, list[Random.Range(0, list.Count)]));
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
