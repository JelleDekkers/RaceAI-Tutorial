using UnityEngine;

namespace RacerAI.SteeringBehaviour
{
    [System.Serializable]
    public class ObstacleAvoidanceBehaviour
    {
        private struct GenericCastHit
        {
            public Vector3 point;
            public Vector3 normal;

            public GenericCastHit(RaycastHit h)
            {
                point = h.point;
                normal = h.normal;
            }
        }

        private const float MIN_VELOCITY = 0.005f;
        private const float MIN_COLLISION_ANGLE = 165;

        [SerializeField] private LayerMask castMask = Physics.DefaultRaycastLayers;
        /// <summary>
        /// The distance away from the collision that we wish go
        /// </summary>
        [SerializeField] private float wallAvoidDistance = 0.5f;
        [SerializeField] private float mainWhiskerLength = 1.25f;
        [SerializeField] private float sideWhiskerLength = 0.701f;
        [SerializeField] private float sideWhiskerAngle = 45f;

        [Header("Debugging")]
        [SerializeField] private bool drawGizmos = true;

        private SteeringController steering;
        private RigidbodyMovementController controller;

        public void Init(SteeringController steering, RigidbodyMovementController controller)
        {
            this.steering = steering;
            this.controller = controller;
        }

        public Vector3 GetAcceleration()
        {
            if (!DetectedObstacle(controller.transform.forward, out GenericCastHit hit))
                return Vector3.zero;

            // Create a target away from the obstacle to seek
            Vector3 targetPostition = hit.point + hit.normal * wallAvoidDistance;

            // If velocity and the collision normal are parallel then move the target a bit to the left or right of the normal
            float angle = Vector3.Angle(controller.Velocity, hit.normal);
            if (angle > MIN_COLLISION_ANGLE)
            {
                // Add some perpendicular displacement to the target position propotional to the angle between the wall normal
                // and facing dir and propotional to the wall avoidance distance (with 2f being a magic constant that feels good) 
                Vector3 perpendicular = new Vector3(-hit.normal.z, hit.normal.y, hit.normal.x);
                targetPostition += (perpendicular * Mathf.Sin((angle - MIN_COLLISION_ANGLE) * Mathf.Deg2Rad) * 2f * wallAvoidDistance);
            }

            return steering.Seek(targetPostition, steering.maxAcceleration);
        }

        private bool DetectedObstacle(Vector3 facingDir, out GenericCastHit firstHit)
        {
            facingDir = controller.ConvertVector(facingDir).normalized;

            Vector3[] dirs = new Vector3[3];
            dirs[0] = facingDir;

            float orientation = SteeringHelper.VectorToOrientation(facingDir);

            dirs[1] = SteeringHelper.OrientationToVector(orientation + sideWhiskerAngle * Mathf.Deg2Rad);
            dirs[2] = SteeringHelper.OrientationToVector(orientation - sideWhiskerAngle * Mathf.Deg2Rad);

            return CastWhiskers(dirs, out firstHit);
        }

        private bool CastWhiskers(Vector3[] dirs, out GenericCastHit firstHit)
        {
            firstHit = new GenericCastHit();
            bool foundObstacle = false;

            for (int i = 0; i < dirs.Length; i++)
            {
                float dist = (i == 0) ? mainWhiskerLength : sideWhiskerLength;

                if (DetectCollider(dirs[i], out GenericCastHit hit, dist))
                {
                    foundObstacle = true;
                    firstHit = hit;
                    break;
                }
            }

            return foundObstacle;
        }

        private bool DetectCollider(Vector3 direction, out GenericCastHit hit, float distance = 1)
        {
            bool result;
            RaycastHit h;
            Vector3 origin = controller.ColliderPosition;

            //result = Physics.Raycast(origin, direction, out h, distance, castMask.value);
            result = Physics.SphereCast(origin, (controller.ColliderRadius * 0.5f), direction, out h, distance, castMask.value);

            if(drawGizmos)
                Debug.DrawLine(origin, origin + direction * distance, Color.yellow);

            hit = new GenericCastHit(h);

            //// If the normal is less than our slope limit then we've hit the ground and not a wall */
            //float angle = Vector3.Angle(Vector3.up, hit.normal);

            //if (angle < controller.SlopeLimit)
            //{
            //    hit.normal = controller.ConvertVector(hit.normal);
            //    result = false;
            //}

            return result;
        }
    }
}