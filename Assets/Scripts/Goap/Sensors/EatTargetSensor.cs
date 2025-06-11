using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace NonPlayable.Goap.Sensors
{
    public class EatTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent,
                                      IComponentReference _,
                                      ITarget previous)
        {
            var pos = agent.Transform.position;

            if (previous is PositionTarget pt)
                return pt.SetPosition(pos);

            return new PositionTarget(pos);
        }

        public override void Update()
        {
        }
    }
}
