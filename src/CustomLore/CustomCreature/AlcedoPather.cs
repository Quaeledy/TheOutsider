using RWCustom;
using UnityEngine;

namespace TheOutsider.CustomLore.CustomCreature
{
    internal class AlcedoPather : BorderExitPather
    {
        public Alcedo alcedo => creature.realizedCreature as Alcedo;

        public AlcedoPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
            : base(AI, world, creature)
        {
        }

        public override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
        {
            return base.CheckConnectionCost(start, goal, connection, followingPath);
        }

        public override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
        {
            if (InThisRealizedRoom(cell.worldCoordinate))
            {
                if (base.lookingForImpossiblePath && !cell.reachable)
                {
                    return new PathCost(costToGoal.resistance * 2f, costToGoal.legality);
                }
                return new PathCost(costToGoal.resistance * (alcedo.AirBorne ? 0.5f : 0.1f) + Vector2.Distance(cell.worldCoordinate.Tile.ToVector2(), base.creaturePos.Tile.ToVector2()), costToGoal.legality);
            }
            return costToGoal;
        }

        public MovementConnection FollowPath(WorldCoordinate originPos, bool actuallyFollowingThisPath)
        {
            int num = int.MinValue;
            PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
            WorldCoordinate worldCoordinate = originPos;
            if (!originPos.TileDefined && !originPos.NodeDefined)
            {
                return default(MovementConnection);
            }
            WorldCoordinate coord = RestrictedOriginPos(originPos);
            if (actuallyFollowingThisPath && debugDrawer != null)
            {
                debugDrawer.Blink(coord);
            }
            PathingCell pathingCell = PathingCellAtWorldCoordinate(coord);
            if (pathingCell != null)
            {
                if (!pathingCell.reachable || !pathingCell.possibleToGetBackFrom)
                {
                    OutOfElement();
                }
                MovementConnection movementConnection = default(MovementConnection);
                PathCost pathCost2 = new PathCost(0f, PathCost.Legality.Unallowed);
                int num2 = -acceptablePathAge;
                PathCost.Legality legality = PathCost.Legality.Unallowed;
                int num3 = -acceptablePathAge;
                float num4 = float.MaxValue;
                int num5 = 0;
                while (true)
                {
                    MovementConnection movementConnection2 = ConnectionAtCoordinate(outGoing: true, coord, num5);
                    num5++;
                    if (movementConnection2 == default(MovementConnection))
                    {
                        break;
                    }
                    if (movementConnection2.destinationCoord.TileDefined && !Custom.InsideRect(movementConnection2.DestTile, coveredArea))
                    {
                        continue;
                    }
                    PathingCell pathingCell2 = PathingCellAtWorldCoordinate(movementConnection2.destinationCoord);
                    PathCost pathCost3 = CheckConnectionCost(pathingCell, pathingCell2, movementConnection2, followingPath: true);
                    if (!pathingCell2.possibleToGetBackFrom && !walkPastPointOfNoReturn)
                    {
                        pathCost3.legality = PathCost.Legality.Unallowed;
                    }
                    PathCost pathCost4 = pathingCell2.costToGoal + pathCost3;
                    if (creature.realizedCreature.shortcutDelay > 0 && (movementConnection2.type == MovementConnection.MovementType.ShortCut || movementConnection2.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze))
                    {
                        pathCost3.legality = PathCost.Legality.Unallowed;
                    }
                    if (movementConnection2.destinationCoord.TileDefined && destination.TileDefined && movementConnection2.destinationCoord.Tile == destination.Tile)
                    {
                        pathCost4.resistance = 0f;
                    }
                    if (pathingCell2.generation > num3)
                    {
                        num3 = pathingCell2.generation;
                        num4 = pathCost4.resistance;
                    }
                    else if (pathingCell2.generation == num3 && pathCost4.resistance < num4)
                    {
                        num4 = pathCost4.resistance;
                    }
                    if (pathCost3.legality < legality)
                    {
                        movementConnection = movementConnection2;
                        legality = pathCost3.legality;
                        num2 = pathingCell2.generation;
                        pathCost2 = pathCost4;
                    }
                    else if (pathCost3.legality == legality)
                    {
                        if (pathingCell2.generation > num2)
                        {
                            movementConnection = movementConnection2;
                            legality = pathCost3.legality;
                            num2 = pathingCell2.generation;
                            pathCost2 = pathCost4;
                        }
                        else if (pathingCell2.generation == num2 && pathCost4 <= pathCost2)
                        {
                            movementConnection = movementConnection2;
                            legality = pathCost3.legality;
                            num2 = pathingCell2.generation;
                            pathCost2 = pathCost4;
                        }
                    }
                }
                if (legality <= PathCost.Legality.Unwanted)
                {
                    if (actuallyFollowingThisPath)
                    {
                        if (movementConnection != default(MovementConnection) && movementConnection.type == MovementConnection.MovementType.ShortCut && realizedRoom.shortcutData(movementConnection.StartTile).shortCutType == ShortcutData.Type.RoomExit)
                        {
                            LeavingRoom();
                        }
                        creatureFollowingGeneration = num2;
                    }
                    int num6 = 30;
                    int num7 = 30;
                    if (alcedo.safariControlled)
                    {
                        num6 = 25;
                        num7 = 15;
                    }
                    bool flag = Custom.InsideRect(originPos.Tile, new IntRect(-num6, -num7, realizedRoom.TileWidth + num6, realizedRoom.TileHeight + 100));
                    if ((actuallyFollowingThisPath && movementConnection != default(MovementConnection) && movementConnection.type == MovementConnection.MovementType.OutsideRoom && !movementConnection.destinationCoord.TileDefined && alcedo.shortcutDelay < 1) || (alcedo.safariControlled && !flag && movementConnection != default(MovementConnection)))
                    {
                        if (!flag)
                        {
                            WorldCoordinate[] skyAccessNodes = world.skyAccessNodes;
                            foreach (WorldCoordinate worldCoordinate2 in skyAccessNodes)
                            {
                                PathingCell pathingCell3 = PathingCellAtWorldCoordinate(worldCoordinate2);
                                if (pathingCell3.generation > num)
                                {
                                    num = pathingCell3.generation;
                                    pathCost = pathingCell3.costToGoal;
                                    worldCoordinate = worldCoordinate2;
                                }
                                else if (pathingCell3.generation == num && pathingCell3.costToGoal < pathCost)
                                {
                                    pathCost = pathingCell3.costToGoal;
                                    worldCoordinate = worldCoordinate2;
                                }
                            }
                            if (!worldCoordinate.CompareDisregardingTile(movementConnection.destinationCoord))
                            {
                                alcedo.AccessSkyGate(movementConnection.destinationCoord, worldCoordinate);
                                if (worldCoordinate.room != base.creaturePos.room)
                                {
                                    LeavingRoom();
                                }
                                if (worldCoordinate.room != destination.room)
                                {
                                    Custom.Log("Alcedo flying to different room than destionation!");
                                    Custom.Log($"Flying to {worldCoordinate}");
                                    Custom.Log($"Dest {destination}");
                                    Custom.Log($"Flying from {movementConnection.destinationCoord}");
                                    PathingCell pathingCell4 = PathingCellAtWorldCoordinate(worldCoordinate);
                                    PathingCell pathingCell5 = PathingCellAtWorldCoordinate(destination);
                                    Custom.Log($"Flying to stats: g-{pathingCell4.generation} l-{pathingCell4.costToGoal.legality} r-{pathingCell4.costToGoal.resistance} a-{world.GetNode(worldCoordinate).type == AbstractRoomNode.Type.SkyExit}");
                                    Custom.Log($"Dest stats : g-{pathingCell5.generation} l-{pathingCell5.costToGoal.legality} r-{pathingCell5.costToGoal.resistance} a-{world.GetNode(destination).type == AbstractRoomNode.Type.SkyExit}");
                                    if (world.GetNode(destination).type != AbstractRoomNode.Type.SkyExit)
                                    {
                                        Custom.Log("looking for node in dest room with sky access");
                                        for (int j = 0; j < world.GetAbstractRoom(destination).nodes.Length; j++)
                                        {
                                            if (world.GetAbstractRoom(destination).nodes[j].type == AbstractRoomNode.Type.SkyExit)
                                            {
                                                PathingCell pathingCell6 = PathingCellAtWorldCoordinate(new WorldCoordinate(destination.room, -1, -1, j));
                                                Custom.Log($"Dest room with sky access stats : g-{pathingCell6.generation} l-{pathingCell6.costToGoal.legality} r-{pathingCell6.costToGoal.resistance} a-{world.GetNode(new WorldCoordinate(destination.room, -1, -1, j)).type == AbstractRoomNode.Type.SkyExit}");
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            return default(MovementConnection);
                        }
                        IntVector2 intVector = new IntVector2(0, 1);
                        if (movementConnection.startCoord.x == 0)
                        {
                            intVector = new IntVector2(-1, 0);
                        }
                        else if (movementConnection.startCoord.x == realizedRoom.TileWidth - 1)
                        {
                            intVector = new IntVector2(1, 0);
                        }
                        else if (movementConnection.startCoord.y == 0)
                        {
                            intVector = new IntVector2(0, -1);
                        }
                        return new MovementConnection(MovementConnection.MovementType.Standard, originPos, new WorldCoordinate(originPos.room, originPos.x + intVector.x, originPos.y + intVector.y, originPos.abstractNode), 1);
                    }
                    return movementConnection;
                }
            }
            return default(MovementConnection);
        }
    }
}
