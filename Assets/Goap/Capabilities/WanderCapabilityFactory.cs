using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using Storeroom.Goap.Sensors;
using UnityEngine;

namespace Storeroom.Goap.Capabilities
{
    public class WanderCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var cap = new CapabilityBuilder("Wander");

            cap.AddGoal<WanderGoal>()
               .AddCondition<IsIdle>(Comparison.GreaterThanOrEqual, 1)
               .SetBaseCost(1);

            cap.AddAction<WanderAction>()
               .AddEffect<IsIdle>(EffectType.Increase)
               .SetTarget<WanderTarget>()
               .SetStoppingDistance(0.1f);

            cap.AddTargetSensor<NavMeshWanderTargetSensor>()
               .SetTarget<WanderTarget>();

            return cap.Build();
        }
    }

}
