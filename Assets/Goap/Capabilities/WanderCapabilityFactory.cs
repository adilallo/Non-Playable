using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using NonPlayable.Goap.Sensors;

namespace NonPlayable.Goap.Capabilities
{
    public class WanderCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var cap = new CapabilityBuilder("Wander");

            cap.AddGoal<WanderGoal>()
               .AddCondition<Fatigue>(Comparison.GreaterThan, 75)
               .AddCondition<Debt>(Comparison.GreaterThan, 75)
               .SetBaseCost(2);

            cap.AddAction<WanderAction>()
                .AddCondition<Fatigue>(Comparison.SmallerThan, 75)
               .AddCondition<Debt>(Comparison.SmallerThan, 75)
               .AddEffect<Fatigue>(EffectType.Increase)
               .AddEffect<Debt>(EffectType.Increase)
               .SetTarget<WanderTarget>()
               .SetStoppingDistance(0.1f)
               .SetMoveMode(ActionMoveMode.PerformWhileMoving);

            cap.AddTargetSensor<NavMeshWanderTargetSensor>()
               .SetTarget<WanderTarget>();

            cap.AddWorldSensor<FatigueSensor>()
                .SetKey<Fatigue>();

            cap.AddWorldSensor<DebtSensor>()
                .SetKey<Debt>();

            return cap.Build();
        }
    }

}
