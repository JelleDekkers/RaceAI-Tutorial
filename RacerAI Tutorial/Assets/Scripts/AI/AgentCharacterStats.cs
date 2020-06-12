using UnityEngine;

namespace RacerAI
{
    [CreateAssetMenu(menuName = "AI Profile")]

    public class AgentCharacterStats : ScriptableObject
    {
        public float CorneringAggressionMultiplier => corneringAggressionMultiplier;
        [SerializeField, Range(0.5f, 5f)] private float corneringAggressionMultiplier = 1;

        public float PathFollowingRandomnessSpeed => pathFollowingRandomnessSpeed;
        [SerializeField] private float pathFollowingRandomnessSpeed = 1;

        public float PathFollowingRandomnessDistance => pathFollowingRandomnessDistance;
        [SerializeField] private float pathFollowingRandomnessDistance = 1;
    }
}