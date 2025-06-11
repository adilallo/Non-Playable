using NonPlayable.Goap;
using System.Collections.Generic;
using UnityEngine;
using VikingCrew.Tools.UI;

namespace NonPlayable.UI
{
    [RequireComponent(typeof(HumorBrain))]
    public class ThoughtBubbleTrigger : MonoBehaviour
    {
        [SerializeField] float verticalOffset = 1.5f;

        private SpeechBubbleManager bubbles;
        private HumorBrain brain;

        readonly Dictionary<string, SpeechBubbleManager.SpeechbubbleType> _bubbleLookup
        = new()
        {
            { "WanderAction", SpeechBubbleManager.SpeechbubbleType.NORMAL  },
            { "EatAction",    SpeechBubbleManager.SpeechbubbleType.ANGRY   },
            { "WorkAction",   SpeechBubbleManager.SpeechbubbleType.SERIOUS },
            { "RestAction",   SpeechBubbleManager.SpeechbubbleType.THINKING},
        };

        static readonly Dictionary<string, Color> _tintLookup = new()
        {
            { "WanderAction", new Color32(255, 255, 255, 255) }, // white keeps prefab look
            { "EatAction",    new Color32(255, 197,   0, 255) }, // warm yellow
            { "WorkAction",   new Color32( 66, 135, 245, 255) }, // blue
            { "RestAction",   new Color32(158,  85, 247, 255) }, // lavender
        };

        void Awake()
        {
            brain = GetComponent<HumorBrain>();
        }

        private void Start()
        {
            bubbles = SpeechBubbleManager.Instance;
        }

        void OnEnable() => brain.OnThoughtReady.AddListener(HandleThought);
        void OnDisable() => brain.OnThoughtReady.RemoveListener(HandleThought);

        private void HandleThought(HumorBrain b)
        {
            string lastAction = b.LastActionId;

            if (!_bubbleLookup.TryGetValue(lastAction, out var bubbleType))
                bubbleType = SpeechBubbleManager.SpeechbubbleType.NORMAL;

            if (!_tintLookup.TryGetValue(lastAction, out var tint))
                tint = Color.white;

            var bubble = bubbles.AddSpeechBubble(
                objectToFollow: b.transform,
                text: "\u200B",
                color: tint,
                type: bubbleType,
                offset: new Vector3(0, verticalOffset, 0)
            );
        }
    }
}


