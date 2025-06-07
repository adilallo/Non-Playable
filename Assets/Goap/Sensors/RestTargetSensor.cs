using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace NonPlayable.Goap.Sensors
{
    /// <summary>Returns the position of the agent's personal RestPoint.</summary>
    [GoapId("RestTargetSensor-3c3f0e68-e7d5-4bcf-a0d4-f48d5e2d7b3")]
    public class RestTargetSensor : LocalTargetSensorBase
    {
        public override void Created()
        {
        }

        public override ITarget Sense(IActionReceiver agent,
                                      IComponentReference _,
                                      ITarget previous)
        {
            var brain = agent.Transform.GetComponent<HumorBrain>();
            if (brain == null || brain.RestPoint == null)
                return previous;

            var pos = brain.RestPoint.transform.position;
            Debug.Log($"RestTargetSensor: Sensed position {pos} for agent {agent.Transform.name}");

            if (previous is PositionTarget pt)
                return pt.SetPosition(pos);

            return new PositionTarget(pos);
        }

        public override void Update()
        {
        }
    }
}
