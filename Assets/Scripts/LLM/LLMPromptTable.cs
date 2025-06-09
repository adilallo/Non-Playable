using System;
using System.Collections.Generic;
using UnityEngine;

namespace NonPlayable.LLM
{
    [CreateAssetMenu(menuName = "LLM/LLM Prompt Table")]
    public class LLMPromptTable : ScriptableObject
    {
        [Serializable]
        public struct Row
        {
            public string actionId;
            [TextArea(3, 10)] public string prompt;
        }
        public List<Row> rows;

        private Dictionary<string, string> _dict;
        public bool TryGetPrompt(string id, out string prompt)
        {
            _dict ??= BuildDict();
            return _dict.TryGetValue(id, out prompt);
        }

        Dictionary<string, string> BuildDict()
        {
            var d = new Dictionary<string, string>();
            foreach (var r in rows) if (!string.IsNullOrEmpty(r.actionId))
                    d[r.actionId] = r.prompt;
            return d;
        }
    }
}