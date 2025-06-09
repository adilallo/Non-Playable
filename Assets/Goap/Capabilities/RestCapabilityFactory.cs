using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using NonPlayable.Goap.Sensors;

namespace NonPlayable.Goap.Capabilities
{
    public class RestCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var cap = new CapabilityBuilder("Rest");

            cap.AddGoal<RestGoal>()
               .AddCondition<Fatigue>(Comparison.GreaterThanOrEqual, 75)
               .SetBaseCost(1);

            cap.AddAction<RestAction>()
               .AddEffect<Fatigue>(EffectType.Decrease)
               .SetTarget<RestTarget>()
               .SetRequiresTarget(true)
               .SetStoppingDistance(0.2f);

            cap.AddTargetSensor<RestTargetSensor>()
               .SetTarget<RestTarget>();

            cap.AddWorldSensor<FatigueSensor>()
                .SetKey<Fatigue>();

            return cap.Build();
        }
    }
}
