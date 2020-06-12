using System.Collections.Generic;
using System.Linq;

namespace RacerAI
{
    public class RankManager : MonoBehaviourSingleton<RankManager>
    {
        private Dictionary<ProgressManager, LevelPositionData> playerPositionDataPair;

        protected override void Awake()
        {
            base.Awake();
            playerPositionDataPair = new Dictionary<ProgressManager, LevelPositionData>();
        }

        private void Update()
        {
            CalculatePlayerLevelPositions();
        }

        private void CalculatePlayerLevelPositions()
        {
            // set level progress for each player
            foreach (var pair in playerPositionDataPair)
            {
                playerPositionDataPair[pair.Key].progress = pair.Key.PathProgress;
            }

            // sort players based on progress
            var sortedPlayers = from pair in playerPositionDataPair
                                orderby pair.Value.progress descending
                                select pair;

            // set ranks in order of sortedPlayers
            int i = 0;
            foreach (var pair in sortedPlayers)
            {
                pair.Value.rank = i;
                i++;
            }
        }

        public void AddPlayer(ProgressManager player)
        {
            playerPositionDataPair.Add(player, new LevelPositionData());
        }

        public void RemovePlayer(ProgressManager player)
        {
            playerPositionDataPair.Remove(player);
        }

        public int GetRank(ProgressManager player)
        {
            return playerPositionDataPair[player].rank;
        }

        public class LevelPositionData
        {
            public float progress = 0;
            public int rank = 0;
        }
    }
}