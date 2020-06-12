using UnityEngine;

namespace RacerAI.Input
{
    public class AgentInput : ICarInput
    {
        private readonly SteeringAgent agent;

        public AgentInput(SteeringAgent agent)
        {
            this.agent = agent;
        }

        public Vector2 GetInput()
        {
            Vector3 desiredAcceleration = agent.GetDesiredAcceleration();
            Vector3 convertedAcceleration = agent.transform.InverseTransformDirection(desiredAcceleration.normalized);

            return new Vector2(convertedAcceleration.x, convertedAcceleration.z);
        }
    }
}