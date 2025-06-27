using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using NonPlayable.Goap.Behaviours;

namespace NonPlayable.Goap.Sensors
{
    [GoapId("FatigueSensor-a4e9cf7d-09e2-4ae9-8831-1e8267d59c02")]
    public class FatigueSensor : LocalWorldSensorBase
    {
        public override void Created()
        {
            // Initialization logic if needed
        }

        public override void Update()
        {
            // Update logic if needed
        }
        public override SenseValue Sense(IActionReceiver agent, IComponentReference refs)
        {
            var data = refs.GetCachedComponent<DataBehaviour>();
            return data == null ? 0 : (int)data.fatigue;
        }
    }
}