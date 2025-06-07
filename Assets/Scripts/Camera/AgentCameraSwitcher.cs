using System.Collections.Generic;
using UnityEngine;
using NonPlayable.Goap;

namespace NonPlayable.Cam
{
    /// <summary>
    /// Manages a set of cameras—only one is active at a time.
    /// Call ShowCamera(index) to pick which one renders.
    /// </summary>
    public class AgentCameraSwitcher : MonoBehaviour
    {
        [SerializeField] List<HumorBrain> brains;
        Camera current;

        void Awake()
        {
            foreach (var b in brains)
            {
                if (b.MyCamera != null)
                    b.MyCamera.enabled = false;

                b.OnThoughtReady.AddListener(OnThoughtReady);
            }
        }

        void OnThoughtReady(NonPlayable.Goap.HumorBrain brain)
        {
            var cam = brain.MyCamera;
            if (cam == null || cam == current) return;

            if (current != null)
                current.enabled = false;

            cam.enabled = true;
            current = cam;
        }
    }
}
