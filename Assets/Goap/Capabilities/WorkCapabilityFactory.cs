using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using NonPlayable.Goap.Sensors;

namespace NonPlayable.Goap.Capabilities
{
    public class WorkCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var cap = new CapabilityBuilder("Work");

            cap.AddGoal<WorkGoal>()
               .AddCondition<Debt>(Comparison.SmallerThanOrEqual, 75)
               .SetBaseCost(1);

            cap.AddAction<WorkAction>()
               .AddEffect<Debt>(EffectType.Decrease)
               .SetTarget<WorkTarget>()
               .SetRequiresTarget(true)
               .SetStoppingDistance(0.2f);

            cap.AddTargetSensor<WorkTargetSensor>()
               .SetTarget<WorkTarget>();

            cap.AddWorldSensor<DebtSensor>()
                .SetKey<Debt>();

            return cap.Build();
        }
    }
}
