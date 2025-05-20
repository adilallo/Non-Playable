using LLMUnity;
using UnityEngine;

namespace Storeroom.LLM
{
    [DefaultExecutionOrder(-1000)]
    public class LLMUtility : MonoBehaviour
    {
        void OnDisable() => CancelAll();
        void OnApplicationQuit() => CancelAll();

        static void CancelAll()
        {
            foreach (var llmChar in FindObjectsOfType<LLMCharacter>())
                llmChar.CancelRequests();

        }
    }

    static class LLMPrewarm
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void WarmAllLLMCharacters()
        {
            // Kick off warm-up asynchronously; no need to await
            foreach (var c in Object.FindObjectsOfType<LLMCharacter>())
                _ = c.Warmup();
        }
    }
}

