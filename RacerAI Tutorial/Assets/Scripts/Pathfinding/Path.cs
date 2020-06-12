using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PathFinding
{
    /// <summary>
    /// The calculated shortest path.
    /// </summary>
    [System.Serializable]
    public class Path
    {
        public List<INode> nodes;

        public float TotalDistance => totalDistance;
        protected float totalDistance = 0f;

        public Path()
        {
            nodes = new List<INode>();
        }

        /// <summary>
        /// Calculates the total distance for this generated path
        /// </summary>
        public virtual void CalculateTotalDistance()
        {
            List<INode> calculated = new List<INode>();
            totalDistance = 0f;

            for (int i = 0; i < nodes.Count; i++)
            {
                INode node = nodes[i];
                for (int j = 0; j < node.Connections.Length; j++)
                {
                    INode connection = nodes.Where(x => x.ID == node.Connections[j]) as INode;

                    // Don't calculate already calculated nodes
                    if (nodes.Contains(connection) && !calculated.Contains(connection))
                    {
                        // Calculating the distance between a node and connection when they are both available in path nodes list
                        totalDistance += Vector3.Distance(node.Position, connection.Position);
                    }
                }
                calculated.Add(node);
            }
        }

        public override string ToString()
        {
            return string.Format("Nodes: {0} \n Length: {1}", string.Join( ", ", nodes.Select(node => node.Position).ToArray()), totalDistance);
        }
    }
}