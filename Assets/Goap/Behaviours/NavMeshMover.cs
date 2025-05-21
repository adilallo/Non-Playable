using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace Storeroom.Goap
{
    [RequireComponent(typeof(AgentBehaviour), typeof(NavMeshAgent))]
    public class NavMeshMover : MonoBehaviour, IAgentDistanceObserver
    {
        NavMeshAgent nav; AgentBehaviour agent;

        void Awake()
        {
            nav = GetComponent<NavMeshAgent>();
            agent = GetComponent<AgentBehaviour>();
            agent.DistanceObserver = this;
        }
        void OnEnable()
        {
            agent.Events.OnTargetChanged += OnTarget;
            agent.Events.OnTargetInRange += _ => StopAgent(); // once we get “in range,” stop
            agent.Events.OnTargetNotInRange += _ => ResumeAgent(); // once we leave range, resume
            agent.Events.OnTargetLost += () => StopAgent(); // if the target vanishes, stop
        }
        void OnDisable()
        {
            agent.Events.OnTargetChanged -= OnTarget;
        }

        public float GetDistance(IMonoAgent _, ITarget t, IComponentReference __)
            => nav.pathPending || nav.pathStatus != NavMeshPathStatus.PathComplete
                  ? Mathf.Infinity
                  : nav.remainingDistance;

        void OnTarget(ITarget t, bool inRange)
        {
            if (t == null) { nav.ResetPath(); return; }
            ResumeAgent();
            nav.SetDestination(t.Position);
            nav.isStopped = inRange;
        }

        private void StopAgent()
        {
            nav.ResetPath();
            nav.isStopped = true;
        }

        private void ResumeAgent()
        {
            nav.isStopped = false;
        }
    }
}

