using MLAgents;
using MLAgents.Sensors;
using RacerAI.SteeringBehaviour;
using UnityEngine;

namespace RacerAI.ML
{
    public class CarAgent : Agent
    {
        private const int ACTION_INPUT_VERTICAL = 0;
        private const int ACTION_INPUT_HORIZONTAL = 1;

        [SerializeField] private Transform spawnPoint = null;
        [SerializeField] private bool showGUI = false;

        [SerializeField, HideInInspector] private ProgressManager progressManager = null;
        [SerializeField, HideInInspector] private SteeringController steering = null;

        [Header("Rewards")]
        [SerializeField] private float progressionReward = 0.0001f;
        [SerializeField] private float velocityReward = 0.001f;
        [SerializeField] private float stepPenalty = 0.0001f;
        [SerializeField] private float finishReward = 1f;
        [SerializeField] private float wallCollisionPenalty = -0.1f;

        [Header("Raycast Values")]
        [SerializeField] private LayerMask mask = new LayerMask();
        [SerializeField] private float rayLength = 1.25f;
        [SerializeField] private float sideRayAngle = 45f;

        private Vector2 input;
        private float previousProgress;

        private int frame = 0;

        private void OnValidate()
        {
            progressManager = GetComponent<ProgressManager>();
            steering = GetComponent<SteeringController>();
        }

        private void Start()
        {
            OnEpisodeBegin();   
        }

        private void FixedUpdate()
        {
            CarControl();

            if(UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                EndEpisode();
            }

            frame++;
        }

        public override void OnEpisodeBegin()
        {
            steering.PhysicsController.Reset();
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            previousProgress = progressManager.PathProgress;
            input = new Vector2();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // TODO: opponents

            sensor.AddObservation(progressManager.PathProgress);
            sensor.AddObservation(steering.PhysicsController.Velocity);

            DetectEnvironment(ref sensor);
        }

        public override void OnActionReceived(float[] vectorAction)
        {
            input.y = vectorAction[ACTION_INPUT_VERTICAL];
            input.x = vectorAction[ACTION_INPUT_HORIZONTAL];

            AddReward(stepPenalty);
            AddReward(steering.PhysicsController.Velocity.magnitude * velocityReward);
            AddReward((progressManager.PathProgress - previousProgress) * progressionReward);
            //maxStep += (int)((progressManager.PathProgress - previousProgress) * progressionStepIncrease);

            previousProgress = progressManager.PathProgress;
        }

        public override float[] Heuristic()
        {
            float[] action = new float[2];
            action[ACTION_INPUT_VERTICAL] = UnityEngine.Input.GetAxis("Vertical");
            action[ACTION_INPUT_HORIZONTAL] = UnityEngine.Input.GetAxis("Horizontal");

            return action;
        }

        private void CarControl()
        {
            Vector3 input = new Vector3(this.input.x, 0, this.input.y);
            Vector3 targetPosition = transform.position + transform.TransformDirection(input);

            steering.Steer(steering.Arrive(targetPosition));
            steering.FaceMovementDirection();
        }

        private void OnCollisionEnter(Collision collision)
        {
            SetReward(wallCollisionPenalty);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.isTrigger)
                return;

            OnFinishEnter();
        }

        public void OnFinishEnter()
        {
            SetReward(finishReward);
            EndEpisode();
        }
        
        private void DetectEnvironment(ref VectorSensor sensor)
        {
            Vector3[] directions = GetRayDirections();

            for (int i = 0; i < directions.Length; i++)
            {
                bool hit = CastRay(transform.position, directions[i], rayLength, out float distance);
                sensor.AddObservation(hit);
                sensor.AddObservation(distance);
            }
        }

        private Vector3[] GetRayDirections()
        {
            Vector3[] rayDirections = new Vector3[5];
            float forwardAngle = SteeringHelper.VectorToOrientation(transform.forward);

            rayDirections[0] = transform.forward;
            rayDirections[1] = SteeringHelper.OrientationToVector(forwardAngle + sideRayAngle * Mathf.Deg2Rad);
            rayDirections[2] = SteeringHelper.OrientationToVector(forwardAngle + (sideRayAngle / 2) * Mathf.Deg2Rad);
            rayDirections[3] = SteeringHelper.OrientationToVector(forwardAngle - (sideRayAngle / 2) * Mathf.Deg2Rad);
            rayDirections[4] = SteeringHelper.OrientationToVector(forwardAngle - sideRayAngle * Mathf.Deg2Rad);

            return rayDirections;
        }

        private bool CastRay(Vector3 origin, Vector3 direction, float rayLength, out float distanceToCollision)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, rayLength))
            {
                distanceToCollision = hitInfo.distance / direction.magnitude;
                Debug.DrawRay(origin, direction * hitInfo.distance, Color.red);

                return true;
            }
            else
            {
                distanceToCollision = hitInfo.distance / direction.magnitude;
                Debug.DrawRay(origin, direction * rayLength, Color.green);

                return false;
            }
        }

        private void OnGUI()
        {
            if (!showGUI)
                return;

            GUILayout.BeginVertical();
            GUILayout.Label("Velocity: " + steering.PhysicsController.Velocity.magnitude);
            GUILayout.Label("Cumulative Reward: " + GetCumulativeReward());
            GUILayout.Label("Steps: " + StepCount);
            GUILayout.Label("Progress: " + progressManager.PathProgress);
            GUILayout.EndVertical();
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying)
                return;

            Gizmos.color = Color.white;
            Vector3[] rayDirections = GetRayDirections();

            foreach (Vector3 direction in rayDirections)
            {
                Gizmos.DrawLine(transform.position, transform.position + direction * rayLength);
            }
        }
    }
}