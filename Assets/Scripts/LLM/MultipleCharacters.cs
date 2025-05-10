using UnityEngine;
using UnityEngine.UI;
using LLMUnity;

/// <summary>
/// Scene controller for two independent NPC chat panels.
/// Add this script to an empty GameObject and wire
/// both character prefabs, input fields and text outputs.
/// </summary>
public class MultipleCharacters : MonoBehaviour
{
    [Header("NPC #1")]
    public LLMCharacter llmCharacter1;
    public InputField playerText1;
    public Text aiText1;

    [Header("NPC #2")]
    public LLMCharacter llmCharacter2;
    public InputField playerText2;
    public Text aiText2;

    LLMCharactersInteraction int1, int2;

    void Start()
    {
        int1 = new LLMCharactersInteraction(playerText1, aiText1, llmCharacter1);
        int2 = new LLMCharactersInteraction(playerText2, aiText2, llmCharacter2);
        int1.Start(); int2.Start();
    }

    /* ---------- optional UI buttons ---------- */
    public void CancelRequests()
    {
        llmCharacter1.CancelRequests(); int1.ReplyDone();
        llmCharacter2.CancelRequests(); int2.ReplyDone();
    }
    public void ExitGame() => Application.Quit();

    /* ---------- editor hint ---------- */
    void OnValidate()
    {
        if (!llmCharacter1.remote && llmCharacter1.llm != null &&
            string.IsNullOrEmpty(llmCharacter1.llm.model))
        {
            Debug.LogWarning($"Assign a model in {llmCharacter1.llm.gameObject.name}");
        }
    }
}
