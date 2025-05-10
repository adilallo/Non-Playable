using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-3)]   // runs *before* LLMCharacter(-2)
public class LLMParagraphLimiter : MonoBehaviour
{
    public LLMUnity.LLMCharacter npc;
    [Range(1, 6)] public int maxSentences = 3;
    [Range(30, 150)] public int tokensPerReply = 80;

    void Awake()
    {
        if (npc == null) npc = GetComponent<LLMUnity.LLMCharacter>();

        npc.numPredict = tokensPerReply;

        // Build an inline GBNF and put it straight into grammarString
        npc.grammarString = BuildGrammar(maxSentences);

        // leave npc.grammar empty so package updates won’t overwrite us
    }

    string BuildGrammar(int n)
    {
        var reps = string.Join(" ", Enumerable.Repeat("sentence", n));
        return
$@"root     ::= {reps}
sentence ::= /[^.!?\n]+/ [.?!]";
    }
}
