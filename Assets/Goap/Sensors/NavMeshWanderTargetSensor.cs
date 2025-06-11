using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace NonPlayable.Goap.Sensors
{
    [GoapId("NavMeshWanderTargetSensor-0af7b2c8-a313-4f18-88f3-40c0702a81d9")]
    public class NavMeshWanderTargetSensor : LocalTargetSensorBase
    {
        const float Radius = 12f;
        const int MaxTries = 12;
        const float SampleDist = 1.5f;

        public override void Created()
        {

        }

        public override ITarget Sense(IActionReceiver agent,
                                      IComponentReference _,
                                      ITarget previous)
        {
            var origin = agent.Transform.position;
            for (int i = 0; i < MaxTries; i++)
            {
                // pick a random point in XZ circle
                var p2D = Random.insideUnitCircle * Radius;
                var guess = origin + new Vector3(p2D.x, 0, p2D.y);

                // snap to navmesh
                if (!NavMesh.SamplePosition(guess, out var hit, SampleDist, NavMesh.AllAreas))
                    continue;

                // **ensure there is a complete path**
                var path = new NavMeshPath();
                if (NavMesh.CalculatePath(origin, hit.position, NavMesh.AllAreas, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    // Reuse previous instance if possible
                    if (previous is PositionTarget pt)
                        return pt.SetPosition(hit.position);

                    return new PositionTarget(hit.position);
                }
            }

            // Fallback: if we have a previous target, keep it; otherwise stay in place
            if (previous is PositionTarget prevPt)
                return prevPt;

            return new PositionTarget(origin);
        }

        public override void Update()
        {

        }

        bool RandomPointOnNavMesh(Vector3 center, float radius, out Vector3 result)
        {
            for (int i = 0; i < 10; i++)
            {
                var rand = center + Random.insideUnitSphere * radius;
                if (NavMesh.SamplePosition(rand, out var hit, 1.5f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }
            result = center;
            return false;
        }
    }
}
