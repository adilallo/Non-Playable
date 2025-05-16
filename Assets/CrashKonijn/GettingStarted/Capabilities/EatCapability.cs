using CrashKonijn.Docs.GettingStarted.Actions;
using CrashKonijn.Docs.GettingStarted.Sensors;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using MoreMountains.Feel;

namespace CrashKonijn.Docs.GettingStarted.Capabilities
{
    public class EatCapability : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("EatCapability");

            builder.AddGoal<EatGoal>()
                .AddCondition<Hunger>(Comparison.SmallerThanOrEqual, 0);

            builder.AddAction<EatAction>()
                .AddEffect<Hunger>(EffectType.Decrease)
                .AddCondition<PearCount>(Comparison.GreaterThanOrEqual, 1)
                .SetRequiresTarget(false);

            return builder.Build();
        }
    }
}
