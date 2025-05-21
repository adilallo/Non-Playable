using UnityEngine;
using LLMUnity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Linq;

namespace Storeroom.LLM
{
    /// <summary>
    /// Trims the model’s full reply down to N sentences,
    /// where N is picked uniformly between minSentences and maxSentences
    /// every time you call ChatLimited.
    /// Attach to the same GameObject as an LLMCharacter.
    /// </summary>
    [RequireComponent(typeof(LLMCharacter))]
    public class LLMSentenceLimiter : MonoBehaviour
    {
        [Header("Sentence range (inclusive)")]
        public int minSentences = 2;
        public int maxSentences = 5;

        [Header("Hard token ceiling (safety)")]
        public int tokenCeiling = 150;

        [Header("Forbidden words (logit‑bias)")]
        public string[] bannedWords =
            { "Lacan", "lacan", "Lacanian", "Lacanism" };

        static readonly Dictionary<string, Task<Dictionary<int, string>>> _biasTasks
            = new Dictionary<string, Task<Dictionary<int, string>>>();

        static bool IsServiceReady(LLMCharacter c) =>
    c != null && c.llm != null && c.llm.started && !c.llm.failed;

        LLMCharacter npc;
        CancellationTokenSource _initCts;
        Task _biasReady;

        private void Awake()
        {
            npc = GetComponent<LLMCharacter>();
            if (npc == null) { enabled = false; return; }
            npc.numPredict = tokenCeiling; 
            npc.stream = false;
            _initCts = new CancellationTokenSource();

            string modelKey = npc.llm?.model ?? "default-model";
            lock (_biasTasks)
            {
                if (!_biasTasks.TryGetValue(modelKey, out var t))
                    _biasTasks[modelKey] = t = BuildBiasDictionary(_initCts.Token);

                _biasReady = ApplyBiasAsync(t);  
            }

            _ = ApplyBiasAsync(_biasTasks[modelKey]);
        }

        void OnDisable()
        {
            _initCts.Cancel();

            if (IsServiceReady(npc))
                npc.CancelRequests();
        }

        public async Task<string> ChatLimited(string prompt)
        {
            await _biasReady;

            int target = UnityEngine.Random.Range(minSentences, maxSentences + 1);

            Task<string> llmTask = npc.Chat(prompt);

            if (await Task.WhenAny(llmTask, Task.Delay(-1, _initCts.Token)) != llmTask)
                throw new OperationCanceledException();

            return TrimToSentences(await llmTask, target);
        }

        async Task ApplyBiasAsync(Task<Dictionary<int, string>> biasTask)
        {
            try
            {
                var bias = await biasTask;
                npc.logitBias = bias;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Bias init failed: {e}");
            }
        }

        async Task<Dictionary<int, string>> BuildBiasDictionary(CancellationToken token)
        {
            await global::LLMUnity.LLM.WaitUntilModelSetup();
            if (npc == null || npc.llm == null) return new Dictionary<int, string>();
            if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

            await npc.llm.WaitUntilReady();
            token.ThrowIfCancellationRequested();

            // 1. expand all variants
            var variants = new HashSet<string>();
            foreach (string w in bannedWords)
                foreach (string v in Variants(w))
                    variants.Add(v);

            // 2. tokenise in parallel
            var tasks = variants.Select(v => TokensOf(v, token)).ToArray();

            await Task.WhenAll(tasks);

            // 3. build dictionary
            var dict = new Dictionary<int, string>();
            foreach (Task<List<int>> t in tasks)
                foreach (int id in t.Result)
                    dict[id] = "-100";

            return dict;
        }

        async Task<List<int>> TokensOf(string text, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            string json = $"{{\"content\":\"{text}\"}}";
            var ids = await npc.Tokenize(json);
            token.ThrowIfCancellationRequested();
            return ids ?? new List<int>();
        }

        static IEnumerable<string> Variants(string w)
        {
            yield return w;
            yield return w.ToLowerInvariant();
            yield return w.ToUpperInvariant();
            yield return char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant();
            // leading-space versions
            yield return " " + w;
            yield return " " + w.ToLowerInvariant();
            yield return " " + w.ToUpperInvariant();
            yield return " " + char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant();

            yield return "\n" + w;
            yield return "\n" + w.ToLowerInvariant();
        }

        string TrimToSentences(string text, int max)
        {
            int sentences = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '.' || c == '!' || c == '?')
                {
                    // treat as sentence end only if next char is whitespace or EOS
                    if (i == text.Length - 1 || char.IsWhiteSpace(text[i + 1]))
                        if (++sentences >= max) return text.Substring(0, i + 1).Trim();
                }
            }
            return text.Trim();
        }
    }

}
