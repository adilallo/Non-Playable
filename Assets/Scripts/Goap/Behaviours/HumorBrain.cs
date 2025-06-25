using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using NonPlayable.LLM;
using System;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace NonPlayable.Goap
{
    public enum Humor
    {
        Sanguine,
        Choleric,
        Melancholic,
        Phlegmatic
    }

    public class HumorBrain : MonoBehaviour
    {
        [Header("Thinking")]
        [SerializeField] private LLMPromptTable _prompts;
        [SerializeField] private LLMSentenceLimiter _limiter;
        [SerializeField] private TextMeshProUGUI _thoughtText;

        [Header("Throttle")]
        [Tooltip("Seconds before the SAME Action can trigger another thought after the first one")]
        [SerializeField] private float minInterval = 24f;
        [Tooltip("Spacing between initial fire times")]
        [SerializeField] private float initialOffsetSpacing = 8f;

        [SerializeField] private Camera myCamera;
        public Camera MyCamera => myCamera;

        [Header("Rest")]
        [SerializeField] private Transform[] _restPoints;
        public Transform[] RestPoints => _restPoints;

        [Header("Work")]
        [SerializeField] private Transform[] _workPoints;
        public Transform[] WorkPoints => _workPoints;

        [Header("Eat")]
        [SerializeField] private Transform[] _eatPoints;
        public Transform[] EatPoints => _eatPoints;

        [Serializable] public class ThoughtEvent : UnityEvent<HumorBrain> { }
        public Humor humor;
        public ThoughtEvent OnThoughtReady = new ThoughtEvent();
        public string LastActionId { get; private set; }

        private AgentBehaviour _agent;
        private GoapActionProvider _provider;
        private GoapBehaviour _goap;
        static GoapBehaviour _cachedGoap;

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
        }

        private void OnDisable() 
        { 
        }

        private void Start()
        {
            _provider.RequestGoal<WanderGoal, RestGoal, WorkGoal, EatGoal>();
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
                            LastActionId = id;
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