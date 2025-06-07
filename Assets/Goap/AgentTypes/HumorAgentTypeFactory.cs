using NonPlayable.Goap.Capabilities;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;

namespace NonPlayable.Goap.AgentType
{
    public class HumorAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("HumorAgent");
            factory.AddCapability<WanderCapabilityFactory>();
            factory.AddCapability<RestCapabilityFactory>();
            return factory.Build();
        }
    }
}