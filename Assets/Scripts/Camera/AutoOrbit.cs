using UnityEngine;
using Unity.Cinemachine;

namespace Storeroom.Cinemachine
{
    /// <summary>
    /// Adds a constant horizontal spin to a Cinemachine Camera
    /// that is using the Orbital Follow body.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class AutoOrbit : MonoBehaviour
    {
        [Tooltip("Degrees per second around the target.")]
        public float speed = 20f;

        CinemachineCamera _vcam;
        CinemachineOrbitalFollow _orbital;

        void Awake()
        {
            _vcam = GetComponent<CinemachineCamera>();

            _orbital = _vcam.GetComponent<CinemachineOrbitalFollow>();
            if (_orbital == null)
                Debug.LogError(
                  "AutoOrbit requires a CinemachineVirtualCamera\n" +
                  "with its Body set to 'Orbital Follow'."
                );
        }

        void LateUpdate()
        {
            if (_orbital == null)
                return;

            _orbital.HorizontalAxis.Value += speed * Time.deltaTime;

            var min = _orbital.HorizontalAxis.Range.x;
            var max = _orbital.HorizontalAxis.Range.y;
            var span = max - min;

            if (_orbital.HorizontalAxis.Value > max)
                _orbital.HorizontalAxis.Value -= span;
            else if (_orbital.HorizontalAxis.Value < min)
                _orbital.HorizontalAxis.Value += span;
        }
    }
}
