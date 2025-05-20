using UnityEngine;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using UnityEngine.UI;
using Storeroom.LLM;
using System.Collections.Generic;
using System.Threading;
using System;

namespace Storeroom.Goap
{
    public class HumorBrain : MonoBehaviour
    {
        [Header("Thinking")]
        [SerializeField] private LLMPromptTable _prompts;
        [SerializeField] private LLMSentenceLimiter _limiter;
        [SerializeField] private Text _thoughtText;

        [Header("Throttle")]
        [Tooltip("Seconds before the SAME Action can trigger another thought")]
        [SerializeField] float minInterval = 8f;

        private AgentBehaviour _agent;
        private GoapActionProvider _provider;
        private GoapBehaviour _goap;
        static GoapBehaviour _cachedGoap;

        CancellationTokenSource cts;
        readonly Dictionary<string, float> lastFired = new();
        bool busy = false;

        private void Awake()
        {
            if (_cachedGoap == null)
                _cachedGoap = FindFirstObjectByType<GoapBehaviour>();
            this._goap = _cachedGoap;
            this._agent = this.GetComponent<AgentBehaviour>();
            this._provider = this.GetComponent<GoapActionProvider>();

            if (this._provider.AgentTypeBehaviour == null)
                this._provider.AgentType = this._goap.GetAgentType("HumorAgent");

            _agent.Events.OnActionStart += action => OnActionStart(action);
        }

        void OnEnable() => cts = new CancellationTokenSource();
        void OnDisable() { cts.Cancel(); cts.Dispose(); }

        private void OnDestroy()
        {
            _agent.Events.OnActionStart -= action => OnActionStart(action);
        }

        private void Start()
        {
            GetComponent<GoapActionProvider>()
                         .RequestGoal<WanderGoal>();
        }

        private async void OnActionStart(object actionObj)
        {
            if (busy || _prompts == null || _limiter == null || _thoughtText == null)
                return;

            string id = actionObj.GetType().Name;

            if (!_prompts.TryGetPrompt(id, out var prompt))
                return;

            if (lastFired.TryGetValue(id, out var t) && Time.time - t < minInterval)
                return;

            lastFired[id] = Time.time;
            busy = true;

            try
            {
                string trimmed = await _limiter.ChatLimited(prompt);
                _thoughtText.text = trimmed;
            }
            catch (OperationCanceledException) { /* play-mode ended */ }
            finally { busy = false; }
        }
    }
}