using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace NonPlayable.Goap.Sensors
{
    public class RestTargetSensor : LocalTargetSensorBase
    {
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

            if (previous == null)
            {
                var i = Random.Range(0, points.Length);
                return new PositionTarget(points[i].position);
            }

            return previous;
        }

        public override void Update()
        {
        }
    }
}
