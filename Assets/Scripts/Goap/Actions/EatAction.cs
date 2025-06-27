using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using NonPlayable.Goap.Behaviours;
using UnityEngine;

namespace NonPlayable.Goap
{
    [GoapId("Eat-c2038e75-c0b9-491b-aad7-24eab0388134")]
    public class EatAction : GoapActionBase<EatAction.Data>
    {
        // This method is called when the action is created
        // This method is optional and can be removed
        public override void Created()
        {
        }

        // This method is called every frame before the action is performed
        // If this method returns false, the action will be stopped
        // This method is optional and can be removed
        public override bool IsValid(IActionReceiver agent, Data data)
        {
            return true;
        }

        // This method is called when the action is started
        // This method is optional and can be removed
        public override void Start(IMonoAgent agent, Data data)
        {
        }

        // This method is called once before the action is performed
        // This method is optional and can be removed
        public override void BeforePerform(IMonoAgent agent, Data data)
        {
        }

        // This method is called every frame while the action is running
        // This method is required
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            var stats = data.Stats;
            stats.hunger = Mathf.Max(
                0f,
                stats.hunger - stats.eatRate * context.DeltaTime
            );

            return stats.hunger > stats.eatThreshold
                ? ActionRunState.Continue
                : ActionRunState.Completed;
        }

        // This method is called when the action is completed
        // This method is optional and can be removed
        public override void Complete(IMonoAgent agent, Data data)
        {
        }

        // This method is called when the action is stopped
        // This method is optional and can be removed
        public override void Stop(IMonoAgent agent, Data data)
        {
        }

        // This method is called when the action is completed or stopped
        // This method is optional and can be removed
        public override void End(IMonoAgent agent, Data data)
        {
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            [GetComponent] 
            public DataBehaviour Stats { get; set; }
            [GetComponent]
            public HumorBrain Brain { get; set; }
        }
    }
}