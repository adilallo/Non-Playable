using UnityEngine;
using NonPlayable.Goap;
using Unity.Cinemachine;
using System.Collections.Generic;

namespace NonPlayable.Cam
{
    /// <summary>
    /// Keeps one Cinemachine Virtual Camera active and retargets it
    /// to whichever HumorBrain fires OnThoughtReady.
    /// Subscribes/unsubscribes automatically—no manual lists needed.
    /// </summary>
    public class AgentCameraSwitcher : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _vcam;
        [SerializeField] private Transform[] _humorAgents;

        private readonly Dictionary<Humor, Transform> _anchorByHumor = new();

        private HumorBrain[] _brains;

        private void Awake()
        {
            if (_vcam == null)
            {
                Debug.LogError($"[{nameof(AgentCameraSwitcher)}] no vcam assigned on {name}", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < _humorAgents.Length; i++)
            {
                if (_humorAgents[i])
                    _anchorByHumor[(Humor)i] = _humorAgents[i];
            }
        }

        private void OnEnable()
        {
            _brains = FindObjectsByType<HumorBrain>(FindObjectsSortMode.None);
            foreach (var brain in _brains)
                brain.OnThoughtReady.AddListener(HandleThoughtReady);
        }

        private void OnDisable()
        {
            if (_brains != null)
            {
                foreach (var brain in _brains)
                    brain.OnThoughtReady.RemoveListener(HandleThoughtReady);
            }
        }

        private void HandleThoughtReady(HumorBrain brain)
        {
            if (!_anchorByHumor.TryGetValue(brain.humor, out var tgt) || tgt == null)
            {
                Debug.LogWarning($"No anchor set for humor {brain.humor}", this);
                return;
            }

            _vcam.Follow = tgt;
            _vcam.LookAt = tgt;
        }
    }
}
