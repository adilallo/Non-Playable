using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Storeroom.Goap.Sensors
{
    [GoapId("NavMeshWanderTargetSensor-0af7b2c8-a313-4f18-88f3-40c0702a81d9")]
    public class NavMeshWanderTargetSensor : LocalTargetSensorBase
    {
        const float Radius = 6f;           // wander radius around the agent

        public override void Created()
        {

        }

        public override ITarget Sense(IActionReceiver agent,
                                      IComponentReference _,
                                      ITarget previous)
        {
            var p2D = Random.insideUnitCircle * Radius;               // ⟵ 2-D, no Y jitter
            var guess = agent.Transform.position + new Vector3(p2D.x, 0, p2D.y);

            if (NavMesh.SamplePosition(guess, out var hit, 1.5f, NavMesh.AllAreas))
            {
                var pos = hit.position;
                return previous is PositionTarget pt ? pt.SetPosition(pos)
                                                     : new PositionTarget(pos);
            }

            return previous is PositionTarget p ? p:
                new PositionTarget(agent.Transform.position);
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
