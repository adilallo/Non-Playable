using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace NonPlayable.Goap.Sensors
{
    public class RestTargetSensor : LocalTargetSensorBase
    {
        public override ISensorTimer Timer { get; } = SensorTimer.Always;
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent,
                                      IComponentReference references,
                                      ITarget previous)
        {
            var brain = references.GetCachedComponent<HumorBrain>();
            var points = brain?.RestPoints;

            if (points == null || points.Length == 0)
                return previous;

            int pick = Random.Range(0, points.Length);
            var pos = points[pick].position;

            return previous is PositionTarget p ? p.SetPosition(pos)
                                                : new PositionTarget(pos);
        }

        public override void Update()
        {
        }
    }
}
