using System;
using UnityEngine;
using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;

namespace NonPlayable.Goap.Behaviours
{
    /// <summary>
    /// Holds vital stats that sensors & actions read/write. Replace the simple
    /// tick‑up logic with your own later (animation events, LLM responses, etc.).
    /// </summary>
    public class DataBehaviour : MonoBehaviour
    {
        [Header("Personality")]
        [Tooltip("How many seconds this character sleeps when resting")]
        public float restDuration = 8f;
        [Tooltip("Fatigue rate of this character")]
        public float fatigueRate = 1f;

        [Header("Vitals (0‑100)")]
        public float hunger = 0f;
        public float fatigue = 0f;
        public float bowel = 0f;
    }
}