using Storeroom.Goap;
using CrashKonijn.Goap.Runtime;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using CrashKonijn.Agent.Runtime;

namespace Storeroom.Goap
{
    [RequireComponent(typeof(NavMeshAgent), typeof(GoapActionProvider))]
    public class NavMeshStuckRecovery : MonoBehaviour
    {
        public float checkInterval = 2f;
        public float movementThreshold = 0.2f;

        NavMeshAgent nav;
        Vector3 lastPos;
        AgentBehaviour agent;

        void Awake()
        {
            nav = GetComponent<NavMeshAgent>();
            agent = GetComponent<AgentBehaviour>();
        }

        void OnEnable()
        {
            lastPos = transform.position;
            StartCoroutine(CheckStuck());
        }

        IEnumerator CheckStuck()
        {
            var provider = GetComponent<GoapActionProvider>();
            while (enabled)
            {
                yield return new WaitForSeconds(checkInterval);

                // If the planner has given us an unreachable path...
                if (nav.hasPath && nav.pathStatus != NavMeshPathStatus.PathComplete)
                {
                    Unstick(provider);
                }
                else
                {
                    // Or if we’ve barely moved despite a valid path
                    float dist = Vector3.Distance(transform.position, lastPos);
                    if (nav.hasPath && !nav.pathPending && dist < movementThreshold)
                        Unstick(provider);
                }

                lastPos = transform.position;
            }
        }

        void Unstick(GoapActionProvider provider)
        {
            nav.ResetPath();
            Debug.LogWarning($"[StuckRecovery] {name} fell off the NavMesh or got wedged—replanning.");
            agent.StopAction(true);
            provider.RequestGoal<WanderGoal>();
        }
    }
}