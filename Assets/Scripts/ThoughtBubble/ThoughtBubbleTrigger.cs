using Storeroom.Goap;
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
            var bubble = bubbles.AddSpeechBubble(
                objectToFollow: b.transform,
                text: "\u200B",
                type: SpeechBubbleManager.SpeechbubbleType.THINKING,
                offset: new Vector3(0, verticalOffset, 0)
            );

            SetLayerRecursively(bubble.gameObject, LayerMask.NameToLayer("UI World"));
        }

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null)
                return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}


