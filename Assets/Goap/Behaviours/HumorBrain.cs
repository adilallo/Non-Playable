using UnityEngine;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Agent.Core;
using UnityEngine.UI;
using NonPlayable.LLM;
using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine.Events;
using System.Collections;

namespace NonPlayable.Goap
{
    public class HumorBrain : MonoBehaviour
    {
        [Header("Thinking")]
        [SerializeField] private LLMPromptTable _prompts;
        [SerializeField] private LLMSentenceLimiter _limiter;
        [SerializeField] private Text _thoughtText;

        [Header("Throttle")]
        [Tooltip("Seconds before the SAME Action can trigger another thought after the first one")]
        [SerializeField] private float minInterval = 24f;
        [Tooltip("Spacing between initial fire times")]
        [SerializeField] private float initialOffsetSpacing = 8f;

        [SerializeField] private Camera myCamera;
        public Camera MyCamera => myCamera;

        [Header("Rest")]
        [SerializeField] private Transform _restPoint;
        public Transform RestPoint => _restPoint;
        [Serializable] public class ThoughtEvent : UnityEvent<HumorBrain> { }
        public ThoughtEvent OnThoughtReady = new ThoughtEvent();

        private AgentBehaviour _agent;
        private GoapActionProvider _provider;
        private GoapBehaviour _goap;
        static GoapBehaviour _cachedGoap;
        private CancellationTokenSource _cts;

        private static int _brainCount;
        private int _brainIndex;
        private float _initialDelay;
        private bool _busy = false;
        
        private void Awake()
        {
            _brainIndex = _brainCount++;
            _initialDelay = initialOffsetSpacing * (_brainIndex + 1);

            if (_cachedGoap == null)
                _cachedGoap = FindFirstObjectByType<GoapBehaviour>();
            this._goap = _cachedGoap;
            this._agent = this.GetComponent<AgentBehaviour>();
            this._provider = this.GetComponent<GoapActionProvider>();

            if (this._provider.AgentTypeBehaviour == null)
                this._provider.AgentType = this._goap.GetAgentType("HumorAgent");
        }

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
        }

        private void OnDisable() 
        { 
            _cts.Cancel(); 
            _cts.Dispose(); 
        }

        private void Start()
        {
            _provider.RequestGoal<WanderGoal, RestGoal>();
            StartCoroutine(ThoughtLoop());
        }

        private IEnumerator ThoughtLoop()
        {
            yield return new WaitForSeconds(_initialDelay);

            while (true)
            {
                if (!_busy && _prompts != null && _limiter != null && _thoughtText != null)
                {
                    var currentAction = _agent.ActionState.Action;
                    if (currentAction != null)
                    {
                        var id = currentAction.GetType().Name;
                        if (_prompts.TryGetPrompt(id, out var prompt))
                        {
                            _ = FireThought(id, prompt);
                        }
                    }
                }

                yield return new WaitForSeconds(minInterval);
            }
        }

        private async System.Threading.Tasks.Task FireThought(string id, string prompt)
        {
            _busy = true;
            try
            {
                var trimmed = await _limiter.ChatLimited(prompt);
                _thoughtText.text = trimmed;
                OnThoughtReady.Invoke(this);
                Debug.Log($"Prompt: {id} – {prompt}", this);
            }
            catch (OperationCanceledException) { /* play-mode ended */ }
            finally { _busy = false; }
        }
    }
}