using PathCreation;
using PathCreation.Examples;
using PathFinding;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RacerAI
{
    public class PathManager : MonoBehaviourSingleton<PathManager>
    {
        private const float NODE_MIN_SPACING = 0.1f;

        public Graph RacingLineGraph { get; private set; }
        public PathCreator Road => road;
        public EndOfPathInstruction TrackType => trackType;
        public float RoadWidth => roadMesh.roadWidth;
        public PathCreator RacingLinePath => racingLinePath;

        [SerializeField] private PathCreator road = null;
        [SerializeField] private EndOfPathInstruction trackType = EndOfPathInstruction.Loop;
        [SerializeField] private RoadMeshCreator roadMesh = null;
        [SerializeField] private PathCreator racingLinePath;

        [Header("Racing Line Generator")]
        [SerializeField] private float nodeSpacing = 20;
        [SerializeField] private float nodesRoadEdgeOffset = 1f;

        protected override void Awake()
        {
            base.Awake();

            if (racingLinePath == null)
                GenerateRacingLinePath();
        }

        [Button("Generate Shortest Path")]
        public void GenerateRacingLinePath()
        {
            Path racingLine = CalculateShortestPath();
            racingLinePath = CreateSpline(racingLine.nodes.Select(node => node.Position).ToList(), "Racing Line");
        }

        public int GetNearestAnchorIndex(float pathProgress)
        {
            return (int)((RacingLinePath.bezierPath.NumAnchorPoints * pathProgress) + 1) % racingLinePath.bezierPath.NumAnchorPoints;
        }

        public Vector3 GetPointOnRacingLine(int anchorIndex)
        {
            return RacingLinePath.bezierPath.GetPoint(anchorIndex * 3 % racingLinePath.bezierPath.NumPoints);
        }

        public Vector3 GetAnchorDirection(int anchorIndex)
        {
            int index = (anchorIndex * 3 + 1) % racingLinePath.bezierPath.NumPoints;
            Vector3 point = RacingLinePath.bezierPath.GetPoint(index);

            index = (anchorIndex * 3 + 2) % racingLinePath.bezierPath.NumPoints;
            Vector3 handle = RacingLinePath.bezierPath.GetPoint(index);

            return handle - point;
        }

        private Path CalculateShortestPath()
        {
            INode[] nodes = GenerateNodes();
            RacingLineGraph = new Graph();

            // TODO: this final node should not be the destination point as it will never create a correct loop
            Path path = RacingLineGraph.FindShortestPath(nodes[0], nodes[nodes.Length - 1], nodes);

            List<Vector3> points = new List<Vector3>();
            foreach (INode node in path.nodes)
            {
                points.Add(node.Position);
            }

            return path;
        }

        private INode[] GenerateNodes()
        {
            VertexPath path = Road.path;
            List<INode> allNodes = new List<INode>();

            nodeSpacing = Mathf.Max(NODE_MIN_SPACING, nodeSpacing);
            float distance = 0;
            int nodesPerAnchor = 3;

            // Create all nodes
            while (distance < path.length)
            {
                Vector3 centre = path.GetPointAtDistance(distance, TrackType);
                Vector3 normal = path.GetNormalAtDistance(distance, TrackType);
                Vector3 forward = Road.path.GetDirectionAtDistance(distance, TrackType);

                distance += nodeSpacing;

                allNodes.Add(new Node(centre + normal * (RoadWidth - nodesRoadEdgeOffset) * -1, forward, allNodes.Count));
                allNodes.Add(new Node(centre, forward, allNodes.Count));
                allNodes.Add(new Node(centre + normal * (RoadWidth - nodesRoadEdgeOffset), forward, allNodes.Count));

                if (distance > path.length - nodeSpacing)
                    break;
            }

            // Fill node connections
            for (int i = 0; i < allNodes.Count; i++)
            {
                allNodes[i].Connections = new int[nodesPerAnchor];
                int neighbourIndexOffset = nodesPerAnchor - i % nodesPerAnchor;

                for (int j = 0; j < nodesPerAnchor; j++)
                {
                    int connectionIndex = (i + j + neighbourIndexOffset) % allNodes.Count;
                    allNodes[i].Connections[j] = allNodes[connectionIndex].ID;
                }
            }

            return allNodes.ToArray();
        }

        private PathCreator CreateSpline(List<Vector3> points, string name)
        {
            GameObject splineObject = new GameObject(name);
            splineObject.transform.position = transform.position;
            splineObject.transform.SetParent(transform);

            bool isClosedPath = TrackType == EndOfPathInstruction.Loop;
            BezierPath newPath = new BezierPath(points, isClosedPath);
            PathCreator racingLineSpline = splineObject.AddComponent<PathCreator>();

            racingLineSpline.bezierPath = newPath;
            racingLineSpline.bezierPath.GlobalNormalsAngle = Road.bezierPath.GlobalNormalsAngle;

            return racingLineSpline;
        }

        private void OnDrawGizmos()
        {
            if (RacingLinePath != null)
            {
                for (int i = 0; i < racingLinePath.bezierPath.NumAnchorPoints; i++)
                {
                    Vector3 point = racingLinePath.bezierPath.GetPoint(i * 3); 
                    UnityEditor.Handles.Label(point, i.ToString());
                }
            }
        }
    }
}