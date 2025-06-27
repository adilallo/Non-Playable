using UnityEngine;

namespace NonPlayable.Goap.Behaviours
{
    /// <summary>
    /// Holds vital stats that sensors & actions read/write. Replace the simple
    /// tick‑up logic with your own later (animation events, LLM responses, etc.).
    /// </summary>
    public class DataBehaviour : MonoBehaviour
    {
        [Header("Rest")]
        [Tooltip("Fatigue rate of this character")]
        public float fatigueRate = 1f;
        [Tooltip("Rest rate of this character")]
        public float restRate = 1f;
        [Tooltip("How long this character can rest before leaving")]
        public float restThreshold = 25f;
        
        [Header("Work")]
        [Tooltip("Debt rate of this character")]
        public float debtRate = 1f;
        [Tooltip("Work rate of this character")]
        public float workRate = 1f;
        [Tooltip("How long this character can work before leaving")]
        public float workThreshold = 25f;

        [Header("Eat")]
        [Tooltip("Hunger rate of this character")]
        public float hungerRate = 1f;
        [Tooltip("Eat rate of this character")]
        public float eatRate = 1f;
        [Tooltip("How long this character can go without eating before leaving")]
        public float eatThreshold = 10f;


        [Header("Vitals (0‑100)")]
        public float debt = 0f;
        public float fatigue = 0f;
        public float hunger = 0f;
    }
}