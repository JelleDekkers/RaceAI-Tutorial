using RacerAI.SteeringBehaviour;
using UnityEngine;
using RacerAI.Input;

namespace RacerAI
{
    public enum ControlType
    {
        Player,
        AI,
        GeneticBrain
    }

    [RequireComponent(typeof(SteeringController))]
    public class CarController : MonoBehaviour
    {
        public delegate void ControlSwitchedEventHandler(bool isPlayer);
        public ControlSwitchedEventHandler ControlSwitchedEvent;

        public Vector3 TargetPosition { get; private set; }

        [SerializeField] private bool isPlayerControlled = false;

        [SerializeField, HideInInspector] private SteeringController steering = null;
        [SerializeField, HideInInspector] private SteeringAgent steeringAgent = null;

        private ICarInput inputController;

        private void OnValidate()
        {
            steering = GetComponent<SteeringController>();
            steeringAgent = GetComponent<SteeringAgent>();

            if (Application.isPlaying)
                SetInput(isPlayerControlled);
        }

        private void Awake()
        {
            SetInput(isPlayerControlled);
        }

        private void SetInput(bool player)
        {
            if (player && inputController is PlayerInput)
                return;
            else if (!player && inputController is AgentInput)
                return;

            isPlayerControlled = player;
            steeringAgent.enabled = !player;

            if(player)
                inputController = new PlayerInput();
            else
                inputController = new AgentInput(steeringAgent);

            ControlSwitchedEvent?.Invoke(player);
        }

        private void FixedUpdate()
        {
            Vector3 input = inputController.GetInput();
            input.z = input.y;
            input.y = 0;

            TargetPosition = transform.position + transform.TransformDirection(input);
            steering.Steer(steering.Arrive(TargetPosition));
            steering.FaceMovementDirection();
        }

#if UNITY_EDITOR
        public void OnGUI()
        {
            if (UnityEditor.Selection.activeGameObject == gameObject)
                GUI.Label(new Rect(10, 10, 200, 20), "Velocity: " + steering.PhysicsController.Velocity.magnitude);
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, TargetPosition);
            Gizmos.DrawSphere(TargetPosition, 0.5f);
        }
#endif
    }
}