using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RacerAI.SteeringBehaviour
{
    [Serializable]
    public class PlayerAvoidanceSteeringBehaviour
    {
        [SerializeField] private TriggerSensor sensor = null;
        /// <summary>
        /// How much space can be between two characters before they are considered colliding
        /// </summary>
        [SerializeField] private float collisionDistanceOffset = 0.05f;
        /// <summary>
        /// The max angle that a player is considered to be in front of me and to start avoiding
        /// </summary>
        [SerializeField, Range(0, 180)] private float maxInFrontAngle = 45f;
        [SerializeField] private Transform agentTransform = null;

        private SteeringAgent agent;
        private SteeringController steering;
        private RigidbodyMovementController controller;
        private List<RigidbodyMovementController> targetsInFront;

        public void Init(SteeringAgent agent, SteeringController steering, RigidbodyMovementController controller)
        {
            this.agent = agent;
            this.steering = steering;
            this.controller = controller;
        }

        public Vector3 GetAcceleration()
        {
            targetsInFront = GetPlayersInFront(sensor.DetectedObstacles.ToList());

            Vector3 overtakeAcceleration = GetAccelerationForPlayerAvoidance(targetsInFront);
            Vector3 acceleration = Vector3.zero;
            if (overtakeAcceleration.magnitude > SteeringAgent.MIN_STEERING_MAGNITUDE)
            {
                acceleration = overtakeAcceleration;
            }

            return acceleration;
        }

        private List<RigidbodyMovementController> GetPlayersInFront(List<RigidbodyMovementController> allTargets)
        {
            targetsInFront = new List<RigidbodyMovementController>();
            for (int i = 0; i < allTargets.Count; i++)
            {
                float angle = Vector3.Angle(agent.transform.forward, allTargets[i].transform.position - agent.transform.position);
                if (angle <= maxInFrontAngle)
                {
                    targetsInFront.Add(allTargets[i]);
                    allTargets.Remove(allTargets[i]);
                }
            }
            return targetsInFront;
        }

        private Vector3 GetAccelerationForPlayerAvoidance(ICollection<RigidbodyMovementController> targets)
        {
            Vector3 acceleration = Vector3.zero;

            // 1. Find the target that the character will collide with first
            // The first collision time
            float shortestTime = float.PositiveInfinity;

            // The first target that will collide and other data that we will need and can avoid recalculating
            RigidbodyMovementController firstTarget = null;
            float firstMinSeparation = 0;
            float firstDistance = 0;
            float firstRadius = 0;
            Vector3 firstRelativePos = Vector3.zero;
            Vector3 firstRelativeVel = Vector3.zero;

            foreach (RigidbodyMovementController target in targets)
            {
                // Calculate the time to collision
                Vector3 relativePos = controller.ColliderPosition - target.ColliderPosition;
                Vector3 relativeVel = controller.RealVelocity - target.RealVelocity;
                float distance = relativePos.magnitude;
                float relativeSpeed = relativeVel.magnitude;

                if (relativeSpeed == 0)
                    continue;

                float timeToCollision = -1 * Vector3.Dot(relativePos, relativeVel) / (relativeSpeed * relativeSpeed);

                // Check if they will collide at all
                Vector3 separation = relativePos + relativeVel * timeToCollision;
                float minSeparation = separation.magnitude;

                if (minSeparation > controller.ColliderRadius + target.ColliderRadius + collisionDistanceOffset)
                    continue;

                // Check if its the shortest
                if (timeToCollision > 0 && timeToCollision < shortestTime)
                {
                    shortestTime = timeToCollision;
                    firstTarget = target;
                    firstMinSeparation = minSeparation;
                    firstDistance = distance;
                    firstRelativePos = relativePos;
                    firstRelativeVel = relativeVel;
                    firstRadius = target.ColliderRadius;
                }
            }

            // 2. Calculate the steering
            // If we have no target then exit
            if (firstTarget == null)
                return acceleration;

            // If we are going to collide with no separation or if we are already colliding then steer based on current position
            if (firstMinSeparation <= 0 || firstDistance < controller.ColliderRadius + firstRadius + collisionDistanceOffset)
            {
                acceleration = controller.ColliderPosition - firstTarget.ColliderPosition;
            }
            // Else calculate the future relative position 
            else
            {
                acceleration = firstRelativePos + firstRelativeVel * shortestTime;
            }

            // Avoid the target
            acceleration = controller.ConvertVector(acceleration);
            acceleration.Normalize();
            acceleration *= steering.maxAcceleration;

            return acceleration;
        }

        public void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!agentTransform)
                return;

            UnityEditor.Handles.color = new Color(0, 0, 0.7f, 0.2f);
            Vector3 adjustedForwardRotation = Quaternion.Euler(0, -maxInFrontAngle, 0) * agentTransform.forward;
            UnityEditor.Handles.DrawSolidArc(agentTransform.position, Vector3.up, adjustedForwardRotation, maxInFrontAngle * 2, 3);
#endif
        }
    }
}