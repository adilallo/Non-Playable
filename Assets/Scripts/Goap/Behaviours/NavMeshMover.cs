using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace NonPlayable.Goap
{
    [RequireComponent(typeof(AgentBehaviour))]
    public class NavMeshMover : MonoBehaviour, IAgentDistanceObserver
    {
        [SerializeField] public NavMeshAgent nav; 
        AgentBehaviour agent;

        void Awake()
        {
            agent = GetComponent<AgentBehaviour>();
            agent.DistanceObserver = this;
        }

        IEnumerator Start()
        {
            yield return null;

            // if we’re not already on the NavMesh, find the nearest point
            if (!nav.isOnNavMesh &&
                NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
            {
                nav.Warp(hit.position);
            }
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
            if (!Application.isPlaying || !nav.isOnNavMesh)
            {
                return;
            }

            nav.ResetPath();
            nav.isStopped = true;
        }

        private void ResumeAgent()
        {
            if (!Application.isPlaying || !nav.isOnNavMesh)
            {
                return;
            }

            nav.isStopped = false;
        }
    }
}

