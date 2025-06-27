using MoreMountains.Feedbacks;
using NonPlayable.Goap;
using UnityEngine;

namespace NonPlayable.UI
{
    [RequireComponent(typeof(HumorBrain))]
    public class ThoughtBubbleTrigger : MonoBehaviour
    {
        [SerializeField] public MMF_Player player;
        [SerializeField] public MMF_Player textFXPlayer;

        private HumorBrain brain;

        void Awake()
        {
            brain = GetComponent<HumorBrain>();
        }

        void OnEnable() => brain.OnThoughtReady.AddListener(HandleThought);
        void OnDisable() => brain.OnThoughtReady.RemoveListener(HandleThought);

        private void HandleThought(HumorBrain b)
        {
            textFXPlayer.StopFeedbacks();
            player.ResetAllCooldowns();
            this.textFXPlayer.ResetAllCooldowns();
            player.Initialization(true);
            this.textFXPlayer.Initialization(true);
            player.PlayFeedbacks();
            this.textFXPlayer.PlayFeedbacks();
        }
    }
}


