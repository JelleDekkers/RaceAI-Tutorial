using UnityEngine;

namespace RacerAI.SteeringBehaviour
{
    public class SteeringController : MonoBehaviour
    {
        public Vector3 TargetPosition { get; set; }
        public RigidbodyMovementController PhysicsController => physicsController;
        [SerializeField, HideInInspector] protected RigidbodyMovementController physicsController;

        [Header("Car Settings")]
        public float maxVelocity;
        public float handling;
        public float maxAcceleration;
        public float accelerationTime;
        public float steerLimit;

        [Header("Visuals Settings")]
        [SerializeField] private float turnSpeed = 20f;
        [SerializeField] private int smoothingSamplesAmount = 5;

        private void OnValidate()
        {
            physicsController = GetComponent<RigidbodyMovementController>();
        }

        /// <summary>
        /// Updates the velocity of the current game object by the given linear
        /// acceleration
        /// </summary>
        public void Steer(Vector3 linearAcceleration)
        {
            // Velocity is world velocity, so the first this is to convert it to a local velocity:
            Vector3 localAcceleration = transform.InverseTransformDirection(linearAcceleration);

            localAcceleration.x *= handling;
            localAcceleration.x = Mathf.Clamp(localAcceleration.x, -maxVelocity, maxVelocity);

            if (transform.InverseTransformDirection(physicsController.Velocity).z < steerLimit)
                localAcceleration.x *= transform.InverseTransformDirection(physicsController.Velocity).z / steerLimit;

            // Then convert it back to a world direction and assign it:
            localAcceleration = transform.TransformDirection(localAcceleration);

            physicsController.Velocity += localAcceleration * Time.deltaTime;

            if (physicsController.Velocity.magnitude > maxVelocity)
                physicsController.Velocity = physicsController.Velocity.normalized * maxVelocity;
        }

        /// <summary>
        /// A seek steering behavior. Will return the steering for the current game object to seek a given position
        /// </summary>
        public Vector3 Seek(Vector3 targetPosition, float maxSeekAccel)
        {
            Vector3 acceleration = physicsController.ConvertVector(targetPosition - transform.position);
            acceleration.Normalize();
            acceleration *= maxSeekAccel;

            return acceleration;
        }

        /// <summary>
        /// Makes the current game object look where it is going
        /// </summary>
        public void FaceMovementDirection()
        {
            LookAtDirection(physicsController.Velocity);
        }

        public void LookAtDirection(Vector3 direction)
        {
            direction.Normalize();

            /* If we have a non-zero direction then look towards that direction otherwise do nothing */
            if (direction.sqrMagnitude > 0.001f)
            {
                // Mulitply by -1 because counter clockwise on the y-axis is in the negative direction
                // Added 90 because it otherwise takes x as forward
                float targetRotation = (Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg) * -1 + 90;
                float rotation = Mathf.LerpAngle(physicsController.Rotation.eulerAngles.y, targetRotation, Time.deltaTime * turnSpeed);

                physicsController.Rotation = Quaternion.Euler(0, rotation, 0);
            }
            else
            {
                physicsController.Rotation = Quaternion.LookRotation(transform.forward);
            }
        }

        /// <summary>
        /// Returns the steering for a character so it arrives at the target
        /// </summary>
        public Vector3 Arrive(Vector3 targetPosition)
        {
            Debug.DrawLine(transform.position, targetPosition, Color.cyan);

            targetPosition = physicsController.ConvertVector(targetPosition);

            /* Get the right direction for the linear acceleration */
            Vector3 targetVelocity = targetPosition - physicsController.Position;

            /* Calculate the target speed, full speed at slowRadius distance and 0 speed at 0 distance */
            float targetSpeed = maxVelocity;

            /* Give targetVelocity the correct speed */
            targetVelocity.Normalize();
            targetVelocity *= targetSpeed;

            /* Calculate the linear acceleration we want */
            Vector3 acceleration = targetVelocity - physicsController.Velocity;

            /* Rather than accelerate the character to the correct speed in 1 second, 
             * accelerate so we reach the desired speed in timeToTarget seconds 
             * (if we were to actually accelerate for the full timeToTarget seconds). */
            acceleration *= 1 / accelerationTime;

            /* Make sure we are accelerating at max acceleration */
            if (acceleration.magnitude > maxAcceleration)
            {
                acceleration.Normalize();
                acceleration *= maxAcceleration;
            }

            return acceleration;
        }
    }
}

