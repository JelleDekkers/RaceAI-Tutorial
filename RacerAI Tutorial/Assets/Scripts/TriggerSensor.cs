using System.Collections.Generic;
using UnityEngine;

namespace RacerAI
{
    [RequireComponent(typeof(Collider))]
    public class TriggerSensor : MonoBehaviour
    {
        public HashSet<RigidbodyMovementController> DetectedObstacles { get; private set; }

        private void Awake()
        {
            DetectedObstacles = new HashSet<RigidbodyMovementController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.transform.parent != null && other.transform.parent.TryGetComponent(out RigidbodyMovementController player))
                DetectedObstacles.Add(player);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.transform.parent != null && other.transform.parent.TryGetComponent(out RigidbodyMovementController player))
            {
                if(DetectedObstacles.Contains(player))
                    DetectedObstacles.Remove(player);
            }
        }
    }
}