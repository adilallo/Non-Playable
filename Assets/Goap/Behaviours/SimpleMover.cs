using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using UnityEngine;

[RequireComponent(typeof(AgentBehaviour))]
public class SimpleMover : MonoBehaviour
{
    AgentBehaviour agent; ITarget target; bool move;
    private void Awake()
    {
        this.agent = this.GetComponent<AgentBehaviour>();
    }
    void OnEnable()
    {
        agent.Events.OnTargetChanged += (t, inRange) => { target = t; move = !inRange; };
        agent.Events.OnTargetInRange += _ => move = false;
        agent.Events.OnTargetNotInRange += _ => move = true;
        agent.Events.OnTargetLost += () => move = false;
    }
    void Update()
    {
        if (move && target != null)
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(target.Position.x, 0, target.Position.z),
                Time.deltaTime * 2);         // speed 2 m/s
    }
}