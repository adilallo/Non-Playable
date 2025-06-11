using LLMUnity;
using UnityEngine;

namespace NonPlayable.LLM
{
    [DefaultExecutionOrder(-1000)]
    public class LLMUtility : MonoBehaviour
    {
        static bool IsServiceReady(LLMCharacter c) =>
    c != null && c.llm != null && c.llm.started && !c.llm.failed;

        void OnDisable() => CancelAll();
        void OnApplicationQuit() => CancelAll();

        static void CancelAll()
        {
            foreach (var llmChar in FindObjectsOfType<LLMCharacter>())
                if (IsServiceReady(llmChar))
                    llmChar.CancelRequests();

        }
    }

    static class LLMPrewarm
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static async void WarmAllLLMCharacters()
        {
            foreach (var c in Object.FindObjectsOfType<LLMCharacter>())
            {
                try { await c.Warmup(); }   
                catch { }                    
            }
        }
    }
}

