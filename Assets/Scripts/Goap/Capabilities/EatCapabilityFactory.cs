using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using NonPlayable.Goap.Sensors;
using System;

namespace NonPlayable.Goap.Capabilities
{
    public class EatCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var cap = new CapabilityBuilder("Eat");

            cap.AddGoal<EatGoal>()
               .AddCondition<Hunger>(Comparison.SmallerThanOrEqual, 75)
               .SetBaseCost(1);

            cap.AddAction<EatAction>()
               .AddEffect<Hunger>(EffectType.Decrease)
               .SetTarget<EatTarget>()
               .SetRequiresTarget(true)
               .SetStoppingDistance(0.2f);

            cap.AddTargetSensor<EatTargetSensor>()
               .SetTarget<EatTarget>();

            cap.AddWorldSensor<HungerSensor>()
                .SetKey<Hunger>();

            return cap.Build();
        }
    }
}
