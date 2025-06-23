using UnityEngine;
using LLMUnity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using System.Linq;

namespace NonPlayable.LLM
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

        static bool IsServiceReady(LLMCharacter c) =>
    c != null && c.llm != null && c.llm.started && !c.llm.failed;

        LLMCharacter npc;

        private void Awake()
        {
            npc = GetComponent<LLMCharacter>();
            if (npc == null) { enabled = false; return; }
            npc.numPredict = tokenCeiling; 
            npc.stream = false;
        }

        void OnDisable()
        {

            if (IsServiceReady(npc))
                npc.CancelRequests();
        }

        public async Task<string> ChatLimited(string prompt)
        {

            int target = UnityEngine.Random.Range(minSentences, maxSentences + 1);

            Task<string> llmTask = npc.Chat(prompt);


            return TrimToSentences(await llmTask, target);
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
