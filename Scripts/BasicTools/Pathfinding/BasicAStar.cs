// https://www.youtube.com/watch?v=-L-WgKMFuhE

using System.Collections.Generic;
using UnityEngine;

namespace BasicTools.Pathfinding
{
    /// <summary>
    /// basic A* pathfinding algorithm. works only with a privided close list (cost map will not redirect and has limited influence). Can only move in simple directions (up, down, right, left), no diagonal movement
    /// </summary>
    public static class BasicAStar
    {
        private class AStarNode
        {
            public AStarNode(PositionAStarRef predecessor, Vector2Int position, int[,] costMap, Vector2Int destination, int gCost)
            {
                m_predecessor = predecessor;
                m_gCost = gCost;
                m_hCost = calculateHCoast(costMap, position, destination);
                m_fCost = m_gCost + m_hCost;
                //Debug.Log("AStarNode(): m_fCost = m_gCost + m_hCost: " + m_fCost + ";" + m_gCost + ";" + m_hCost);
            }

            private PositionAStarRef m_predecessor;
            public PositionAStarRef predecessor
            {
                get { return m_predecessor; }
            }
            private int m_gCost; // distance from start
            public int gCost
            {
                get { return m_gCost; }
            }
            private readonly int m_hCost; // distance to destination
            private int m_fCost; // sum
            public int fCoast
            {
                get { return m_fCost; }
            }

            public void updatePredecessorAndCost(PositionAStarRef predecessor, int gCost)
            {
                m_gCost = gCost;
                m_fCost = m_gCost + m_hCost;
                m_predecessor = predecessor;
                //Debug.Log("updatePredecessorAndCost(): m_fCost = m_gCost + m_hCost: " + m_fCost + ";" + m_gCost + ";" + m_hCost);
            }

            private int calculateHCoast(int[,] costMap, Vector2Int start, Vector2Int destination, int iterationOutCount = 100000)
            {
                int iterationOutCounter = 0;
                int coastCounter = 0;
                Vector2 tempDirection;
                Vector2Int nextPosition = new Vector2Int();
                float biggestDirectionValue;

                Vector2Int currentPos = start;

                while (!currentPos.Equals(destination) && iterationOutCounter < iterationOutCount)
                {
                    coastCounter += costMap[currentPos.x, currentPos.y];

                    tempDirection = ((Vector2)destination) - ((Vector2)currentPos);

                    // x > 0
                    biggestDirectionValue = tempDirection.x;
                    nextPosition = currentPos + new Vector2Int(1, 0);

                    // y > 0
                    if (tempDirection.y > biggestDirectionValue)
                    {
                        biggestDirectionValue = tempDirection.y;
                        nextPosition = currentPos + new Vector2Int(0, 1);
                    }

                    // x < 0
                    if (Mathf.Abs(tempDirection.x) > biggestDirectionValue)
                    {
                        biggestDirectionValue = Mathf.Abs(tempDirection.x);
                        nextPosition = currentPos + new Vector2Int(-1, 0);
                    }

                    // y < 0
                    if (Mathf.Abs(tempDirection.y) > biggestDirectionValue)
                    {
                        biggestDirectionValue = Mathf.Abs(tempDirection.y);
                        nextPosition = currentPos + new Vector2Int(0, -1);
                    }

                    currentPos = nextPosition;
                    iterationOutCounter++;
                }

                if (iterationOutCounter >= iterationOutCount)
                {
                    throw new System.TimeoutException("iterationOutCount overflow");
                }

                return coastCounter;
            }
        }

        private class PositionAStarRef
        {
            public Vector2Int m_position;
            public AStarNode m_aStarNode;

            public override int GetHashCode()
            {
                return m_position.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                return m_position.Equals(((PositionAStarRef)obj).m_position);
            }
        }

        public static List<Vector2Int> findPath(int[,] costMap, Vector2Int start, Vector2Int destination, List<Vector2Int> inputClosedList, int iterationOutCount = 100000)
        {
            //Debug.DrawRay(new Vector3(start.x -20, 0, start.y - 20), Vector3.up*2, Color.red);
            //Debug.DrawRay(new Vector3(destination.x -20, 0, destination.y - 20), Vector3.up*2, Color.blue);

            if (
                start.x >= costMap.GetLength(0) ||
                start.x < 0 ||
                start.y >= costMap.GetLength(1) ||
                start.y < 0
            )
            {
                throw new System.ArgumentOutOfRangeException("start outside of costmap");
            }

            if (
               destination.x >= costMap.GetLength(0) ||
               destination.x < 0 ||
               destination.y >= costMap.GetLength(1) ||
               destination.y < 0
            )
            {
                throw new System.ArgumentOutOfRangeException("destination outside of costmap");
            }

            List<PositionAStarRef> openList = new List<PositionAStarRef>();
            openList.Add(new PositionAStarRef() { m_position = start, m_aStarNode = new AStarNode(null, start, costMap, destination, 0) });

            List<PositionAStarRef> closedList = new List<PositionAStarRef>();
            for (int i = 0; i < inputClosedList.Count; i++)
            {
                closedList.Add(new PositionAStarRef() { m_position = inputClosedList[i] });
            }


            PositionAStarRef destinationNode = null;

            int iterationOutCounter = 0;
            PositionAStarRef bestCoastNode;
            int bestCoast;
            Vector2Int tempPos = new Vector2Int();
            PositionAStarRef tempAStarRef = new PositionAStarRef();
            int tempCost;
            int tempIndex;

            while (openList.Count > 0 && iterationOutCounter < iterationOutCount)
            {
                bestCoast = int.MaxValue;
                bestCoastNode = null;

                //string DEBUG_Allnode = "";

                // find best node
                for (int i = 0; i < openList.Count; i++)
                {
                    if (openList[i].m_aStarNode.fCoast < bestCoast)
                    {
                        bestCoast = openList[i].m_aStarNode.fCoast;
                        bestCoastNode = openList[i];
                    }

                    //DEBUG_Allnode += openList[i].m_aStarNode.fCoast + "," + openList[i].m_position + "; ";
                }

                //Debug.Log("bestnode: " + bestCoastNode.m_aStarNode.fCoast +"," + bestCoastNode.m_position + "; All nodes: " + DEBUG_Allnode);

                // move to close list
                openList.Remove(bestCoastNode);
                closedList.Add(bestCoastNode);

                if (bestCoastNode.m_position.Equals(destination))
                {
                    destinationNode = bestCoastNode;
                    break;
                }

                // add new open list entries
                for (int i = 0; i < 4; i++)
                {
                    switch (i)
                    {
                        case 0: // up
                            {
                                tempPos.x = bestCoastNode.m_position.x;
                                tempPos.y = bestCoastNode.m_position.y + 1;
                                break;
                            }
                        case 1: // right
                            {
                                tempPos.x = bestCoastNode.m_position.x + 1;
                                tempPos.y = bestCoastNode.m_position.y;
                                break;
                            }
                        case 2: // bot
                            {
                                tempPos.x = bestCoastNode.m_position.x;
                                tempPos.y = bestCoastNode.m_position.y - 1;
                                break;
                            }
                        case 3: // left
                            {
                                tempPos.x = bestCoastNode.m_position.x - 1;
                                tempPos.y = bestCoastNode.m_position.y;
                                break;
                            }
                    }

                    tempAStarRef.m_position = tempPos;

                    if (
                        tempPos.x >= costMap.GetLength(0) ||
                        tempPos.x < 0 ||
                        tempPos.y >= costMap.GetLength(1) ||
                        tempPos.y < 0 ||
                        closedList.Contains(tempAStarRef)
                        )
                    {
                        continue;
                    }
                    else
                    {
                        tempCost = bestCoastNode.m_aStarNode.gCost + costMap[tempPos.x, tempPos.y];
                        tempIndex = openList.IndexOf(tempAStarRef);

                        if (tempIndex == -1) // -1 = not in list
                        {
                            openList.Add(
                                new PositionAStarRef()
                                {
                                    m_position = new Vector2Int(tempPos.x, tempPos.y),
                                    m_aStarNode = new AStarNode(bestCoastNode, tempPos, costMap, destination, tempCost)
                                }
                                );
                        }
                        else
                        {
                            if (openList[tempIndex].m_aStarNode.gCost > tempCost)
                            {
                                openList[tempIndex].m_aStarNode.updatePredecessorAndCost(bestCoastNode, tempCost);
                            }
                        }
                    }
                }

                iterationOutCounter++;
            }

            if (iterationOutCounter >= iterationOutCount)
            {
                throw new System.TimeoutException("iterationOutCount overflow");
            }

            iterationOutCounter = 0;
            List<Vector2Int> pathToDestination = new List<Vector2Int>();
            tempAStarRef = destinationNode;

            while (tempAStarRef != null && iterationOutCounter < iterationOutCount)
            {
                pathToDestination.Add(tempAStarRef.m_position);
                //Debug.Log("gCost:" +tempAStarRef.m_aStarNode.gCost);
                tempAStarRef = tempAStarRef.m_aStarNode.predecessor;
                iterationOutCounter++;
            }

            if (iterationOutCounter >= iterationOutCount)
            {
                throw new System.TimeoutException("iterationOutCount overflow");
            }

            pathToDestination.Reverse();

            return pathToDestination;
        }
    }
}