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
        [Header("Vitals (0‑100)")]
        public float hunger = 0f;
        public float fatigue = 0f;
        public float bowel = 0f;

        [Header("Speeds (units per second)")]
        public float hungerRate = 5f;
        public float fatigueRate = 4f;
        public float bowelRate = 3f;

        private void Update()
        {
            float dt = Time.deltaTime;
            this.hunger = Mathf.Clamp(this.hunger + hungerRate * dt, 0f, 100f);
           // this.fatigue = Mathf.Clamp(this.fatigue + fatigueRate * dt, 0f, 100f);
            this.bowel = Mathf.Clamp(this.bowel + bowelRate * dt, 0f, 100f);
        }
    }
}