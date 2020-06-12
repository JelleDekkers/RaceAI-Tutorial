using UnityEngine;

namespace PathFinding
{
    [System.Serializable]
    public class Node : INode
    {
        public int ID { get; private set; }
        public int[] Connections
        {
            get { return connections; }
            set { connections = value; }
        }
        public Vector3 Position { get; private set; }
        public Vector3 Forward { get; private set; }

        public int[] connections;

        public Node(Vector3 position, Vector3 forward, int ID)
        {
            this.ID = ID;
            Position = position;
            Forward = forward;
            connections = new int[0];
        }

        public Node(Vector3 position, Vector3 forward, Node[] neighbours)
        {
            Position = position;
            Forward = forward;

            connections = new int[neighbours.Length];
            for (int i = 0; i < neighbours.Length; i++)
            {
                connections[i] = neighbours[i].ID;
            }
        }
    }
}