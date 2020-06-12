using UnityEngine;

namespace RacerAI
{
    public class ProgressManager : MonoBehaviour
    {
        public string Name => name;
        public float PathProgress { get; private set; }
        public float Laps { get; private set; }

        private PathManager pathManager;

        private void Start()
        {
            pathManager = PathManager.Instance;
        }

        private void Update()
        {
            PathProgress = pathManager.Road.path.GetClosestProgressOnPath(transform.position);
        }
    }
}