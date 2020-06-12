using UnityEngine;

namespace PathFinding
{
    public interface INode
    {
        int ID { get; }
        int[] Connections { get; set; }
        Vector3 Position { get; }
        Vector3 Forward { get; }
    }
}