using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathFinding
{
    public class Graph
    {
        public const float ANGLE_PENALTY_MODIFIER = 0.1f;

        public INode[] AllNodes { get; private set; }
        public INode Start { get; private set; }
        public INode Finish { get; private set; }

        /// <summary>
        /// Gets the shortest path from the start Node to the end Node.
        /// </summary>
        public virtual Path FindShortestPath(INode from, INode to, INode[] allNodes)
        {
            Start = from;
            Finish = to;
            AllNodes = allNodes;

            if (from == null || to == null)
                throw new ArgumentNullException();

            Path path = new Path();

            // If the start and end are same node, we can return the start node
            if (from == to)
            {
                path.nodes.Add(from);
                return path;
            }

            // The list of unvisited nodes
            List<INode> unvisited = new List<INode>();

            // Previous nodes in optimal path from source
            Dictionary<INode, INode> previous = new Dictionary<INode, INode>();

            // The calculated distances, set all to Infinity at start, except the start Node
            Dictionary<INode, float> allDistances = new Dictionary<INode, float>();

            for (int i = 0; i < AllNodes.Length; i++)
            {
                INode node = AllNodes[i];
                unvisited.Add(node);
                allDistances.Add(node, float.MaxValue);
            }

            // Set the starting Node distance to zero
            allDistances[from] = 0f;
            while (unvisited.Count != 0)
            {
                // Ordering the unvisited list by distance, smallest distance at start and largest at end
                unvisited = unvisited.OrderBy(node => allDistances[node]).ToList();

                // Getting the Node with smallest distance
                INode currentNode = unvisited[0];

                // Remove the current node from unvisisted list
                unvisited.Remove(currentNode);

                // When the current node is equal to the end node, then we can break and return the path
                if (currentNode == to)
                {
                    // Construct the shortest path
                    while (previous.ContainsKey(currentNode))
                    {
                        // Insert the node onto the final result
                        path.nodes.Insert(0, currentNode);

                        // Traverse from start to end
                        currentNode = previous[currentNode];
                    }

                    // Insert the source onto the final result
                    path.nodes.Insert(0, currentNode);
                    break;
                }

                // Looping through the Node connections (neighbors) and where the connection (neighbor) is available at unvisited list
                for (int i = 0; i < currentNode.Connections.Length; i++)
                {
                    INode neighbor = AllNodes[currentNode.Connections[i]];

                    // Getting the distance between the current node and the connection (neighbor)
                    float cost = GetCost(currentNode, neighbor);

                    // The distance from start node to this connection (neighbor) of current node
                    float alt = allDistances[currentNode] + cost;

                    // A shorter path to the connection (neighbor) has been found
                    if (alt < allDistances[neighbor])
                    {
                        allDistances[neighbor] = alt;
                        previous[neighbor] = currentNode;
                    }
                }
            }

            path.CalculateTotalDistance();
            return path;
        }

        /// <summary>
        /// Retrieves the cost of travelling between 2 nodes
        /// Cost is calculated using the distance between the nodes and their angle
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public float GetCost(INode from, INode to)
        {
            float distance =  Vector3.Distance(from.Position, to.Position);
            float angle = Vector3.Angle(to.Forward, from.Position - to.Position);
            float cost = distance + ((180 - angle) * ANGLE_PENALTY_MODIFIER);

            return cost;
        }
    }
}