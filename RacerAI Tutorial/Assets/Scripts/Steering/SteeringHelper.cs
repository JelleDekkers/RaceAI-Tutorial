using UnityEngine;

namespace RacerAI
{
    public static class SteeringHelper
    {
        /// <summary>
        /// Returns the given orientation (in radians) as a unit vector
        /// </summary>
        /// <param name="orientation">the orientation in radians</param>
        /// <param name="is3DGameObj">is the orientation for a 3D game object or a 2D game object</param>
        /// <returns></returns>
        public static Vector3 OrientationToVector(float orientation)
        {
            // Mulitply the orientation by -1 because counter clockwise on the y-axis is in the negative
            // direction, but Cos And Sin expect clockwise orientation to be the positive direction 
            return new Vector3(Mathf.Cos(-orientation), 0, Mathf.Sin(-orientation));
        }

        /// <summary>
        /// Gets the orientation of a vector as radians. For 3D it gives the orienation around the Y axis.
        /// For 2D it gaves the orienation around the Z axis.
        /// </summary>
        /// <param name="direction">the direction vector</param>
        /// <param name="is3DGameObj">is the direction vector for a 3D game object or a 2D game object</param>
        /// <returns>orientation in radians</returns>
        public static float VectorToOrientation(Vector3 direction)
        {
            // Mulitply by -1 because counter clockwise on the y-axis is in the negative direction 
            return -1 * Mathf.Atan2(direction.z, direction.x);

        }
    }
}