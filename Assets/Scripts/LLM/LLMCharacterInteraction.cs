using UnityEngine.UI;
using LLMUnity;

public class LLMCharactersInteraction
{
    readonly InputField playerText;
    readonly Text aiText;
    readonly LLMCharacter npc;
    readonly SentenceLimiter limiter;

    public LLMCharactersInteraction(InputField p, Text t, LLMCharacter c)
    {
        playerText = p; aiText = t; npc = c;
        limiter = c.GetComponent<SentenceLimiter>();
    }

    public void Start()
    {
        playerText.onSubmit.AddListener(OnSubmit);
        playerText.Select();
    }

    void OnSubmit(string msg)
    {
        playerText.interactable = false;
        aiText.text = "...";

        if (limiter != null && limiter.enabled)      // use capped‑sentence mode
        {
            limiter.ChatLimited(
                msg,
                chunk => aiText.text = chunk,
                ReplyDone);
        }
        else                                           // fallback: unlimited
        {
            _ = npc.Chat(msg, txt => aiText.text = txt, ReplyDone);
        }
    }

    public void ReplyDone()
    {
        playerText.interactable = true;
        playerText.text = "";
        playerText.Select();
    }
}
