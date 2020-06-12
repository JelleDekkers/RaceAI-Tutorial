using PathFinding;
using System;
using UnityEngine;

namespace RacerAI.SteeringBehaviour
{
    [Serializable]
    public class FollowPathSteeringBehaviour
    {
        public Vector3 TargetPosition { get; private set; }
        public Vector3 TargetVelocity { get; private set; }

        /// <summary>
        /// The extra offset thats added to the current path position to keep moving along the path
        /// </summary>
        [SerializeField] private float predictionStep = 8f;
        [SerializeField] private ProgressManager self = null;

        private float corneringAggressionMultiplier = 1;
        private float pathFollowingRandomnessSpeed = 1;
        private float pathFollowingRandomnessDistance = 1;

        private SteeringAgent agent;
        private SteeringController steering;
        private RigidbodyMovementController physicsController;
        private PathManager pathManager;

        private Vector3 nearestAnchorPoint;
        private float racingLineProgress;

        public void Init(SteeringAgent agent, SteeringController steering, RigidbodyMovementController physicsController)
        {
            this.agent = agent;
            this.steering = steering;
            this.physicsController = physicsController;
            pathManager = PathManager.Instance;

            TargetPosition = agent.transform.position + agent.transform.forward;
            racingLineProgress = pathManager.RacingLinePath.path.GetClosestProgressOnPath(agent.transform.position);
        }

        public void SetStats(float cornering, float followRandomnessSpeed, float followRandomnessDistance)
        {
            corneringAggressionMultiplier = cornering;
            pathFollowingRandomnessSpeed = followRandomnessSpeed;
            pathFollowingRandomnessDistance = followRandomnessDistance;
        }

        public void Update()
        {
            racingLineProgress = pathManager.RacingLinePath.path.GetClosestProgressOnPath(agent.transform.position);
            steering.TargetPosition = TargetPosition;
        }

        public Vector3 GetAcceleration()
        {
            float distanceTravelled = racingLineProgress * pathManager.RacingLinePath.path.length;
            float futureDistance = (distanceTravelled + predictionStep) % pathManager.RacingLinePath.path.length;

            TargetPosition = pathManager.RacingLinePath.path.GetPointAtDistance(futureDistance, pathManager.TrackType);
            TargetPosition = physicsController.ConvertVector(TargetPosition);

            // adds randomness on horizontal axis
            float pathRandomOffset = Mathf.Sin(Mathf.PingPong(Time.time * pathFollowingRandomnessSpeed, pathFollowingRandomnessDistance));
            TargetPosition += agent.transform.TransformDirection(new Vector3(pathRandomOffset, 0, 0));

            // get the angle between the nearest and next anchor point
            int nearestAnchorIndex = pathManager.GetNearestAnchorIndex(self.PathProgress);
            nearestAnchorPoint = pathManager.GetPointOnRacingLine(nearestAnchorIndex);
            Vector3 nearestForward = pathManager.GetAnchorDirection(nearestAnchorIndex);
            Vector3 nextForward = pathManager.GetAnchorDirection(nearestAnchorIndex + 1);
            float nodeAngle = Vector3.Angle(nearestForward, nextForward);

            // Calculate desired velocity adjusted to incoming corners angle
            float desiredCornerVelocity = steering.maxVelocity / nodeAngle;
            float targetSlowDownDistance = physicsController.Velocity.magnitude / desiredCornerVelocity / steering.handling / corneringAggressionMultiplier;

            // Get the right direction for the linear acceleration
            Vector3 targetVelocity = TargetPosition - physicsController.Position;

            // Calculate the target speed, full speed at slowRadius distance and 0 speed at 0 distance
            float targetSpeed;
            if (targetVelocity.magnitude <= targetSlowDownDistance)
                targetSpeed = steering.maxVelocity * (targetVelocity.magnitude / targetSlowDownDistance);
            else
                targetSpeed = steering.maxVelocity;

            // Give targetVelocity the correct speed 
            targetVelocity.Normalize();
            targetVelocity *= targetSpeed;
            TargetVelocity = targetVelocity;

            // Calculate the linear acceleration we want 
            Vector3 acceleration = targetVelocity - physicsController.Velocity;

            // Rather than accelerate the character to the correct speed in 1 second, accelerate so we reach the desired speed in timeToTarget seconds 
            acceleration *= 1 / steering.accelerationTime;

            // Limit acceleration to max:
            if (acceleration.magnitude > steering.maxAcceleration)
            {
                acceleration.Normalize();
                acceleration *= steering.maxAcceleration;
            }

            return acceleration;
        }

        public void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || agent == null)
                return;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(TargetPosition, 1f);
            Gizmos.DrawLine(agent.transform.position, TargetPosition);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(nearestAnchorPoint, 0.5f);
        }
    }
}