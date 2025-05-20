#if UNITY_EDITOR
using LLMUnity;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class LLMCancelOnExit
{
    static LLMCancelOnExit()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                foreach (var c in Object.FindObjectsOfType<LLMCharacter>())
                    c.CancelRequests();
            }
        };
    }
}
#endif
