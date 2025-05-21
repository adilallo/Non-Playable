using Storeroom.Goap.Capabilities;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace Storeroom.Goap.AgentType
{
    public class HumorAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("HumorAgent");
            factory.AddCapability<WanderCapabilityFactory>();
            return factory.Build();
        }
    }
}