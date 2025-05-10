using UnityEngine;
using LLMUnity;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Trims the model’s full reply down to N sentences,
/// where N is picked uniformly between minSentences and maxSentences
/// every time you call ChatLimited.
/// Attach to the same GameObject as an LLMCharacter.
/// </summary>
[RequireComponent(typeof(LLMCharacter))]
public class SentenceLimiter : MonoBehaviour
{
    [Header("Sentence range (inclusive)")]
    public int minSentences = 2;
    public int maxSentences = 5;

    [Header("Hard token ceiling (safety)")]
    public int tokenCeiling = 150;

    [Header("Forbidden words (logit‑bias)")]
    public string[] bannedWords =
        { "Lacan", "lacan", "Lacanian", "Lacanism" };

    LLMCharacter npc;

    async void Awake()
    {
        npc = GetComponent<LLMCharacter>();
        npc.numPredict = tokenCeiling;   // model can't run away
        npc.stream = false;          // we trim after generation
        await AddLogitBias();
    }

    /* ------------------------------------------------------------------
     *  Call this instead of npc.Chat(...)
     * ------------------------------------------------------------------ */
    public async void ChatLimited(
        string prompt,
        System.Action<string> onReady,
        System.Action onDone)
    {
        int target = Random.Range(minSentences, maxSentences + 1);

        string full = await npc.Chat(prompt);           // single shot
        string trimmed = TrimToSentences(full, target);

        onReady?.Invoke(trimmed);
        onDone?.Invoke();
    }

    /* ------------------------------------------------------------------ */
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

    async Task AddLogitBias()
    {
        await LLM.WaitUntilModelSetup();
        await npc.llm.WaitUntilReady();

        var bias = npc.logitBias ?? new Dictionary<int, string>();

        foreach (string word in bannedWords)
        {
            foreach (string variant in Variants(word))
            {
                foreach (int id in await TokensOf(variant))
                    bias[id] = "-100";          // strong negative bias
            }
        }
        npc.logitBias = bias;
        Debug.Log($"Logit‑bias: blocked {bias.Count} tokens on {npc.name}");
    }

    // All casings + with/without leading space
    IEnumerable<string> Variants(string w)
    {
        yield return w;
        yield return w.ToLower();
        yield return w.ToUpper();
        yield return char.ToUpper(w[0]) + w.Substring(1).ToLower();
        // with leading space (the common case inside a sentence)
        yield return " " + w;
        yield return " " + w.ToLower();
        yield return " " + w.ToUpper();
        yield return " " + char.ToUpper(w[0]) + w.Substring(1).ToLower();
    }

    // Tokenise helper that returns every token id for the string
    async Task<List<int>> TokensOf(string text)
    {
        string json = $"{{\"content\":\"{text}\"}}";
        List<int> ids = await npc.Tokenize(json);
        return ids ?? new List<int>();
    }
}
