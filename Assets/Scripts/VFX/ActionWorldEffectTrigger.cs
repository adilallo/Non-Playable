using System;
using System.Collections.Generic;
using UnityEngine;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Agent.Core;
using MoreMountains.Feedbacks;

[RequireComponent(typeof(AgentBehaviour))]
public class ActionWorldEffectTrigger : MonoBehaviour
{
    private AgentBehaviour _agentBehaviour;

    [Serializable]
    public class Mapping
    {
        public string actionClass;
        public MMF_Player player;
    }

    [Tooltip("Map GOAP action class names to MMF_Players")]
    public Mapping[] mappings;

    Dictionary<string, MMF_Player> _map;

    void Awake()
    {
        _map = new Dictionary<string, MMF_Player>(mappings.Length);
        foreach (var m in mappings)
            if (m.player != null && !string.IsNullOrEmpty(m.actionClass))
                _map[m.actionClass] = m.player;

        _agentBehaviour = GetComponent<AgentBehaviour>();
    }

    private void OnEnable()
    {
        _agentBehaviour.Events.OnActionStart += OnActionStart;
        _agentBehaviour.Events.OnActionEnd += OnActionEnd;
    }

    void OnDestroy()
    {
        _agentBehaviour.Events.OnActionStart -= OnActionStart;
        _agentBehaviour.Events.OnActionEnd -= OnActionEnd;
    }

    private void OnActionStart(IAction action)
    {
        if (_map.TryGetValue(action.GetType().Name, out var fx))
        {
            fx.ResetAllCooldowns();
            fx.Initialization(true);
            fx.PlayFeedbacks();    
        }
    }

    private void OnActionEnd(IAction action)
    {
        if (_map.TryGetValue(action.GetType().Name, out var fx))
        {
            fx.StopFeedbacks();
        }
    }
}
