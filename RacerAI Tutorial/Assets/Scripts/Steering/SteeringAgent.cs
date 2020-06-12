using RacerAI.SteeringBehaviour;
using UnityEngine;

namespace RacerAI
{
    [System.Serializable]
    [RequireComponent(typeof(SteeringController))]
    public class SteeringAgent : MonoBehaviour
    {
        public const float MIN_STEERING_MAGNITUDE = 0.005f;

        [SerializeField, InspectableSO] private AgentCharacterStats stats = null;
        [SerializeField, HideInInspector] private SteeringController steering = null;

        [Header("Agent Behaviours")]
        [SerializeField] private FollowPathSteeringBehaviour followPathBehaviour = null;
        [SerializeField] private ObstacleAvoidanceBehaviour obstacleAvoidanceBehaviour = null;
        [SerializeField] private PlayerAvoidanceSteeringBehaviour playerAvoidanceBehaviour = null;

        private void OnValidate()
        {
            steering = GetComponent<SteeringController>();
        }

        public void Start()
        {
            followPathBehaviour.Init(this, steering, steering.PhysicsController);
            playerAvoidanceBehaviour.Init(this, steering, steering.PhysicsController);
            obstacleAvoidanceBehaviour.Init(steering, steering.PhysicsController);

            followPathBehaviour.SetStats(stats.CorneringAggressionMultiplier, stats.PathFollowingRandomnessSpeed, stats.PathFollowingRandomnessDistance);
        }

        public void Update()
        {
            followPathBehaviour.Update();
        }

        public Vector3 GetDesiredAcceleration()
        {
            Vector3 acceleration = obstacleAvoidanceBehaviour.GetAcceleration();
            if (acceleration.magnitude > MIN_STEERING_MAGNITUDE)
            {
                Debug.DrawLine(transform.position, transform.position + acceleration, Color.red);
                return acceleration;
            }

            acceleration = playerAvoidanceBehaviour.GetAcceleration();
            if (acceleration.magnitude > MIN_STEERING_MAGNITUDE)
            {
                Debug.DrawLine(transform.position, transform.position + acceleration, Color.red);
                return acceleration;
            }

            acceleration = followPathBehaviour.GetAcceleration();
            return acceleration;
        }

        public void OnDrawGizmosSelected()
        {
            if (!enabled)
                return;

            followPathBehaviour.OnDrawGizmosSelected();
            playerAvoidanceBehaviour.OnDrawGizmosSelected();
        }
    }
}