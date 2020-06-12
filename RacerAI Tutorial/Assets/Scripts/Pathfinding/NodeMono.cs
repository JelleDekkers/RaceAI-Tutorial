using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathFinding
{
    public class NodeMono : MonoBehaviour, INode
    {
        /// <summary>
        /// The neighbouring connections
        /// </summary>
        public virtual INode[] Connections
        {
            get { return connections.ToArray(); }
            set { Connections = value; }
        }
        [SerializeField] private List<NodeMono> connections = new List<NodeMono>();

        public Vector3 Position => transform.position;
        public Vector3 Forward => transform.forward;

        public int ID => id;
        [SerializeField, HideInInspector] private int id;

        int[] INode.Connections { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public INode this[int index] => connections[index];

        public void Setup(int id)
        {
            this.id = id;
        }

        private void OnValidate()
        {
            // Remove duplicate elements:
            connections = connections.Distinct().ToList();
        }
    }
}