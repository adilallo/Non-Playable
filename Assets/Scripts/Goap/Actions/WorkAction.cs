using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using NonPlayable.Goap.Behaviours;
using UnityEngine;

namespace NonPlayable.Goap
{
    [GoapId("Work-29ef2528-d99b-469f-adf0-bbfd7ba41d10")]
    public class WorkAction : GoapActionBase<WorkAction.Data>
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
            stats.debt = Mathf.Max(
                0f,
                stats.debt - stats.workRate * context.DeltaTime
            );

            return stats.debt > stats.workThreshold
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