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
               .AddCondition<Fatigue>(Comparison.SmallerThan, 75)
               .SetBaseCost(1);

            cap.AddAction<WanderAction>()
               .AddEffect<Fatigue>(EffectType.Increase)
               .SetTarget<WanderTarget>()
               .SetStoppingDistance(0.1f);

            cap.AddTargetSensor<NavMeshWanderTargetSensor>()
               .SetTarget<WanderTarget>();

            cap.AddWorldSensor<FatigueSensor>()
                .SetKey<Fatigue>();

            return cap.Build();
        }
    }

}
