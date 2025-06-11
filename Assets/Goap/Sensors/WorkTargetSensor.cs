using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace NonPlayable.Goap.Sensors
{
    public class WorkTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent,
                                      IComponentReference _,
                                      ITarget previous)
        {
            var brain = agent.Transform.GetComponent<HumorBrain>();
            if (brain == null || brain.WorkPoint == null)
                return previous;

            var pos = brain.WorkPoint.transform.position;

            if (previous is PositionTarget pt)
                return pt.SetPosition(pos);

            return new PositionTarget(pos);
        }

        public override void Update()
        {
        }
    }
}
