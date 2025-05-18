using UnityEngine;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;

namespace Storeroom.Goap
{
    public class HumorBrain : MonoBehaviour
    {
        private AgentBehaviour agent;
        private GoapActionProvider provider;
        private GoapBehaviour goap;

        private void Awake()
        {
            this.goap = FindObjectOfType<GoapBehaviour>();
            this.agent = this.GetComponent<AgentBehaviour>();
            this.provider = this.GetComponent<GoapActionProvider>();

            if (this.provider.AgentTypeBehaviour == null)
                this.provider.AgentType = this.goap.GetAgentType("HumorAgent");
        }
        private void Start()
        {
            GetComponent<GoapActionProvider>()
                         .RequestGoal<WanderGoal>();
        }     
    }
}