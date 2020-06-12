using UnityEngine;

namespace RacerAI
{
    [RequireComponent(typeof(Rigidbody))]
    public class RigidbodyMovementController : MonoBehaviour
    {
        /// <summary>
        /// How far the character should look below him for ground to stay grounded to
        /// </summary>
        [SerializeField] private float groundCheckDistance = 0.1f;

        /// <summary>
        /// The sphere cast mask that determines what layers should be consider the ground
        /// </summary>
        [SerializeField] private LayerMask groundCheckMask = Physics.DefaultRaycastLayers;

        [SerializeField] private float slopeLimit = 80f;
        [SerializeField] private new Rigidbody rigidbody = null;
        [SerializeField] private float colliderRadius = 1;
        [SerializeField] private Vector3 colliderPosition = Vector3.zero;

        /// <summary>
        /// The current ground normal for this character. 
        /// </summary>
        private Vector3 wallNormal = Vector3.zero;

        /// <summary>
        /// The current movement plane normal for this character.
        /// </summary>
        private Vector3 movementNormal = Vector3.up;

        /* Make the spherecast offset slightly bigger than the max allowed collider overlap. This was
        * known as Physics.minPenetrationForPenalty and had a default value of 0.05f, but has since
        * been removed and supposedly replaced by Physics.defaultContactOffset/Collider.contactOffset.
        * My tests show that as of Unity 5.3.0f4 this is not %100 true and Unity still seems to be 
        * allowing overlaps of 0.05f somewhere internally. So I'm setting my spherecast offset to be
        * slightly bigger than 0.05f 
        */
        private const float spherecastOffset = 0.051f;

        public Rigidbody Rigidbody => rigidbody;

        /// <summary>
        /// The radius for the current game object
        /// </summary>
        public float ColliderRadius => colliderRadius;

        /// <summary>
        /// The maximum slope the character can climb in degrees
        /// </summary>
        public float SlopeLimit => slopeLimit;

        /// <summary>
        /// The position that should be used for most movement AI code. For 2D chars the position will 
        /// be on the X/Y plane. For 3D grounded characters the position is on the X/Z plane. For 3D
        /// flying characters the position is in full 3D (X/Y/Z).
        /// </summary>
        public Vector3 Position => new Vector3(rigidbody.position.x, 0, rigidbody.position.z);

        /// <summary>
        /// Gets the position of the collider (which can be offset from the transform position).
        /// </summary>
        public Vector3 ColliderPosition => transform.TransformPoint(colliderPosition) + rigidbody.position - transform.position;

        /// <summary>
        /// The velocity that should be used for movement AI code. For 3D grounded characters this velocity will be on the X/Z plane but will be
        /// applied on whatever plane the character is currently moving on. For 3D flying characters the velocity will be in full 3D (X/Y/Z).
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                Vector3 dir = rigidbody.velocity;
                dir.y = 0;
                float mag = Vector3.ProjectOnPlane(rigidbody.velocity, movementNormal).magnitude;
                return dir.normalized * mag;
            }
            set
            {
                // If the char is not on the ground then then we will move along the x/z
                // plane and keep any y movement we already have
                if (rigidbody.useGravity)
                {
                    value.y = rigidbody.velocity.y;
                    rigidbody.velocity = value;
                }
                // Else only move along the ground plane
                else
                {
                    rigidbody.velocity = DirOnPlane(value, movementNormal) * value.magnitude;
                }

                LimitMovementOnSteepSlopes();
            }
        }

        /// <summary>
        /// The actual velocity of the underlying unity rigidbody.
        /// </summary>
        public Vector3 RealVelocity
        {
            get
            {
                return rigidbody.velocity;
            }
            set
            {
                rigidbody.velocity = value;
            }
        }
        public Quaternion Rotation
        {
            get
            {
                return rigidbody.rotation;
            }
            set
            {
                rigidbody.MoveRotation(value);
            }
        }
        public float AngularVelocity
        {
            get
            {
                return rigidbody.angularVelocity.y;
            }
            set
            {
                rigidbody.angularVelocity = new Vector3(0, value, 0);
            }
        }
        public float RotationInRadians => rigidbody.rotation.eulerAngles.y * Mathf.Deg2Rad;
        public Vector3 RotationAsVector => SteeringHelper.OrientationToVector(RotationInRadians);

        public void Reset()
        {
            Velocity = Vector3.zero;
            AngularVelocity = 0;
            RealVelocity = Vector3.zero;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            wallNormal = Vector3.zero;
            movementNormal = Vector3.zero;
        }

        private void FixedUpdate()
        {
            bool shouldFollowGround = !rigidbody.useGravity || rigidbody.velocity.y <= 0;

            /* Reset to default values */
            wallNormal = Vector3.zero;
            movementNormal = Vector3.up;
            rigidbody.useGravity = true;

            RaycastHit downHit;

            // Start the ray with a small offset of 0.1f from inside the character. The
            // transform.position of the characer is assumed to be at the base of the character. 
            if (shouldFollowGround && SphereCast(Vector3.down, out downHit, groundCheckDistance, groundCheckMask.value))
            {
                if (IsWall(downHit.normal))
                {
                    /* Get vector pointing down the wall */
                    Vector3 rightSlope = Vector3.Cross(downHit.normal, Vector3.down);
                    Vector3 downSlope = Vector3.Cross(rightSlope, downHit.normal).normalized;

                    float remainingDist = groundCheckDistance - downHit.distance;

                    RaycastHit downWallHit;

                    /* If we found ground that we would have hit if not for the wall then follow it */
                    if (remainingDist > 0 && SphereCast(downSlope, out downWallHit, remainingDist, groundCheckMask.value) && !IsWall(downWallHit.normal))
                    {
                        Vector3 newPos = rigidbody.position + (downSlope * downWallHit.distance);
                        OnGroundDetected(downWallHit.normal, newPos);
                    }

                    /* If we are close enough to the hit to be touching it then we are on the wall */
                    if (downHit.distance <= 0.01f)
                    {
                        wallNormal = downHit.normal;
                    }
                }
                /* Else we've found walkable ground */
                else
                {
                    Vector3 newPos = rigidbody.position + (Vector3.down * downHit.distance);
                    OnGroundDetected(downHit.normal, newPos);
                }
            }

            LimitMovementOnSteepSlopes();
        }

        private bool SphereCast(Vector3 dir, out RaycastHit hitInfo, float dist, int layerMask, Vector3 planeNormal = default(Vector3))
        {
            dir.Normalize();

            /* Make sure we use the collider's origin for our cast (which can be different
                * then the transform.position).
                *
                * Also if we are given a planeNormal then raise the origin a tiny amount away
                * from the plane to avoid problems when the given dir is just barely moving  
                * into the plane (this can occur due to floating point inaccuracies when the 
                * dir is calculated with cross products) */
            Vector3 origin = ColliderPosition + (planeNormal * 0.001f);

            /* Start the ray with a small offset from inside the character, so it will
                * hit any colliders that the character is already touching. */
            origin += -spherecastOffset * dir;

            float maxDist = (spherecastOffset + dist);

            if (Physics.SphereCast(origin, ColliderRadius, dir, out hitInfo, maxDist, layerMask))
            {
                /* Remove the small offset from the distance before returning*/
                hitInfo.distance -= spherecastOffset;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnGroundDetected(Vector3 normal, Vector3 newPos)
        {
            movementNormal = normal;
            rigidbody.useGravity = false;
            rigidbody.MovePosition(newPos);

            /* Reproject the velocity onto the ground plane in case the ground plane has changed this frame.
                * Make sure to multiple by the movement velocity's magnitude, rather than the actual velocity
                * since we could have been falling and now found ground so all the downward y velocity is not
                * part of our movement speed. Technically I am projecting the actual velocity onto the ground
                * plane rather than finding the real movement velocity's speed.*/
            rigidbody.velocity = DirOnPlane(rigidbody.velocity, movementNormal) * Velocity.magnitude;
        }

        private bool IsWall(Vector3 surfNormal)
        {
            /* If the normal of the surface is greater then our slope limit then its a wall */
            return Vector3.Angle(Vector3.up, surfNormal) > slopeLimit;
        }

        private void LimitMovementOnSteepSlopes()
        {
            Vector3 startVelocity = rigidbody.velocity;

            /* If we are currently on a wall then limit our movement */
            if (wallNormal != Vector3.zero && IsMovingInto(rigidbody.velocity, wallNormal))
            {
                rigidbody.velocity = LimitVelocityOnWall(rigidbody.velocity, wallNormal);
            }
            /* Else we have no wall or we are moving away from the wall so we will no longer be touching it */
            else
            {
                wallNormal = Vector3.zero;
            }

            /* Check if we are moving into a wall */
            for (int i = 0; i < 2; i++)
            {
                Vector3 direction = rigidbody.velocity.normalized;
                float dist = rigidbody.velocity.magnitude * Time.deltaTime;

                Vector3 origin = ColliderPosition;

                //if (i == 0)
                //{
                //    Debug.DrawRay(origin + Vector3.up * 0.05f, direction, new Color(0.953f, 0.898f, 0.961f), 0f, false);
                //}
                //else if (i == 1)
                //{
                //    Debug.DrawRay(origin + Vector3.up * 0.05f, direction, new Color(0.612f, 0.153f, 0.69f), 0f, false);
                //}
                //else
                //{
                //    Debug.DrawRay(origin + Vector3.up * 0.05f, direction, new Color(0.29f, 0.078f, 0.549f), 0f, false);
                //}

                RaycastHit hitInfo;

                /* Spherecast in the direction we are moving and check if we will hit a wall. Also check that we are
                    * in fact moving into the wall (it seems that it is possible to clip the corner of a wall even 
                    * though the char/spherecast is moving away from the wall) */
                if (SphereCast(direction, out hitInfo, dist, groundCheckMask.value, movementNormal) && IsWall(hitInfo.normal)
                    && IsMovingInto(direction, hitInfo.normal))
                {
                    /* Move up to the on coming wall */
                    float moveUpDist = Mathf.Max(0, hitInfo.distance);
                    rigidbody.MovePosition(rigidbody.position + (direction * moveUpDist));

                    Vector3 projectedVel = LimitVelocityOnWall(rigidbody.velocity, hitInfo.normal);
                    Vector3 projectedStartVel = LimitVelocityOnWall(startVelocity, hitInfo.normal);

                    /* If we have a previous wall. And if the latest velocity is moving into the previous wall or if 
                        * our starting velocity projected onto this new wall is moving into the previous wall then stop
                        * movement */
                    if (wallNormal != Vector3.zero && (IsMovingInto(projectedVel, wallNormal) || IsMovingInto(projectedStartVel, wallNormal)))
                    {
                        Vector3 vel = Vector3.zero;
                        if (rigidbody.useGravity)
                        {
                            vel.y = rigidbody.velocity.y;
                        }
                        rigidbody.velocity = vel;

                        break;
                    }
                    /* Else move along the wall */
                    else
                    {
                        rigidbody.velocity = projectedVel;

                        /* Make this wall the previous wall */
                        wallNormal = hitInfo.normal;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private bool IsMovingInto(Vector3 dir, Vector3 normal)
        {
            return Vector3.Angle(dir, normal) > 90f;
        }

        private Vector3 LimitVelocityOnWall(Vector3 velocity, Vector3 planeNormal)
        {
            Vector3 rightSlope = Vector3.Cross(planeNormal, Vector3.down);

            if (!rigidbody.useGravity)
            {
                /* Make sure the direction against the wall is dictated by the X/Z direction of the
                    * character and the wall normal. So even when the character's ground normal changes
                    * the direction it is moving against the wall is not changed.
                    *
                    * Also make sure the magnitude of the movement along the wall is also dictated by
                    * the X/Y direction of the character and the wall normal. This will is needed to 
                    * keep the change in magnitude in sync with the change in direction. */

                float mag = velocity.magnitude;

                velocity.y = 0;

                /* Scale the original magnitude by how parallel the X/Z movement is to the wall's left/right direction */
                mag *= Mathf.Abs(Mathf.Cos(Vector3.Angle(velocity, rightSlope) * Mathf.Deg2Rad));

                Vector3 groundPlaneIntersection = Vector3.Cross(movementNormal, planeNormal);
                velocity = Vector3.Project(velocity, rightSlope);
                velocity = Vector3.Project(velocity, groundPlaneIntersection).normalized * mag;
            }
            else
            {
                /* Get vector pointing down the slope) */
                Vector3 downSlope = Vector3.Cross(rightSlope, planeNormal);

                /* Keep any downward movement (like gravity) */
                float yComponent = Mathf.Min(0f, rigidbody.velocity.y);

                /* Project the remaining movement on to the wall */
                Vector3 newVel = rigidbody.velocity;
                newVel.y = 0;
                newVel = Vector3.ProjectOnPlane(newVel, planeNormal);

                /* If the remaining movement is moving up the wall then make it only go left/right.
                    * I believe this will be true for all  ramp walls but false for all ceiling walls */
                if (Vector3.Angle(downSlope, newVel) > 90f)
                {
                    newVel = Vector3.Project(newVel, rightSlope);
                }

                /* Add the downward movement back in and make sure we are still moving along the wall
                    * so future sphere casts won't hit this wall */
                newVel.y = yComponent;
                newVel = Vector3.ProjectOnPlane(newVel, planeNormal);

                velocity = newVel;
            }

            return velocity;
        }

        /// <summary>
        /// Creates a vector that maintains x/z direction but lies on the plane.
        /// </summary>
        private Vector3 DirOnPlane(Vector3 vector, Vector3 planeNormal)
        {
            Vector3 newVel = vector;
            newVel.y = (-planeNormal.x * vector.x - planeNormal.z * vector.z) / planeNormal.y;
            return newVel.normalized;
        }

        /// <summary>
        /// Rotates the rigidbody to angle (given in degrees)
        /// </summary>
        /// <param name="angle"></param>
        public void Rotate(float angle)
        {
            rigidbody.MoveRotation(Quaternion.Euler(new Vector3(0f, angle, 0f)));
        }

        /// <summary>
        /// Converts the vector by setting y to 0. 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector3 ConvertVector(Vector3 v)
        {
            v.y = 0;
            return v;
        }

        private void OnDrawGizmos()
        {
            Vector3 origin = ColliderPosition;
            Debug.DrawLine(origin, origin + (Velocity.normalized), Color.red, 0f, false);
            Debug.DrawLine(origin, origin + (RealVelocity.normalized), Color.green, 0f, false);
            Debug.DrawLine(origin, origin + (wallNormal), Color.yellow, 0f, false);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            Matrix4x4 prevMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(-transform.position), transform.rotation, transform.lossyScale);
            Gizmos.DrawWireSphere(transform.position + colliderPosition, colliderRadius);
            Gizmos.matrix = prevMatrix;
        }
    }
}