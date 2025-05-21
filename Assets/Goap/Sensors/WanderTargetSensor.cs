using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace Storeroom.Goap.Sensors
{
    public class WanderTargetSensor : LocalTargetSensorBase
    {
        // 30 × 30 world bounds
        private static readonly Bounds World = new(new Vector3(0, 0, 0), new Vector3(30, 0, 30));

        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent,
                                      IComponentReference refs,
                                      ITarget previous)
        {
            var p = Random.insideUnitCircle * 5f;
            var pos = agent.Transform.position + new Vector3(p.x, 0, p.y);

            // keep inside bounds
            pos = World.Contains(pos) ? pos : World.ClosestPoint(pos);

            return previous is PositionTarget pt ? pt.SetPosition(pos)
                                                 : new PositionTarget(pos);
        }

        public override void Update()
        {
        }
    }
}
