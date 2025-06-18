using MoreMountains.Feedbacks;
using NonPlayable.Goap;
using System.Collections;
using UnityEngine;

namespace NonPlayable.UI
{
    [RequireComponent(typeof(HumorBrain))]
    public class ThoughtBubbleTrigger : MonoBehaviour
    {
        [SerializeField] public MMF_Player player;
        [SerializeField] public MMF_Player textFXPlayer;
        [SerializeField] private float _visibleDuration = 8f;

        private HumorBrain brain;
        private Coroutine _hideRoutine;

        void Awake()
        {
            brain = GetComponent<HumorBrain>();
        }

        void OnEnable() => brain.OnThoughtReady.AddListener(HandleThought);
        void OnDisable() => brain.OnThoughtReady.RemoveListener(HandleThought);

        private void HandleThought(HumorBrain b)
        {
            if (_hideRoutine != null) StopCoroutine(_hideRoutine);

            player.ResetAllCooldowns();
            textFXPlayer.ResetAllCooldowns();
            player.Initialization(true);
            textFXPlayer.Initialization(true);
            player.PlayFeedbacks();
            textFXPlayer.PlayFeedbacks();

            _hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(_visibleDuration);
            player.StopFeedbacks();
            _hideRoutine = null;
        }
    }
}


