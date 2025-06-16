using UnityEngine;
using Unity.Cinemachine;

namespace NonPlayable.Cinemachine
{
    /// <summary>
    /// Top-down camera that glides between agents’ positions
    /// and occasional random board spots.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class AutoExplore : MonoBehaviour
    {
        // ───── User-tunable fields ─────────────────────────────────────
        [Header("Agents / POIs")]
        [Tooltip("Drag your 4 agent GameObjects here")]
        public Transform[] pointsOfInterest;

        [Range(0f, 1f)]
        [Tooltip("0 = ignore agents, 1 = always choose an agent")]
        public float agentBias = 0.7f;

        [Header("Baseline orbit")]
        public float minRadius = 8f;
        public float maxRadius = 14f;

        [Header("Altitude")]
        public float minHeight = 12f;
        public float maxHeight = 20f;

        [Header("Timing")]
        public Vector2 dwellTime = new(6, 12);

        [Header("Noise")]
        public float noiseScale = 0.2f;
        public float noiseSpeed = 0.02f;

        [Header("Smoothing")]
        public float smoothTime = 1.0f;
        // ───────────────────────────────────────────────────────────────

        /// <summary>External systems can force-focus a specific world position.</summary>
        public void MarkInterest(Vector3 position) => _nextInterest = position;

        // ─── Private state ────────────────────────────────────────────
        CinemachineCamera _vcam;
        CinemachineOrbitalFollow _orbit;
        Transform _followTarget;

        float _phase;
        float _nextDecisionTime;

        Vector3 _targetFocus;
        float _targetRadius;
        float _targetHeight;
        float _targetHeading;
        Vector3? _nextInterest;

        Vector3 _focusVelocity;
        float _radiusVelocity;
        float _heightVelocity;
        float _headingVelocity;
        // ───────────────────────────────────────────────────────────────

        void Awake()
        {
            _vcam = GetComponent<CinemachineCamera>();
            _orbit = _vcam.GetComponent<CinemachineOrbitalFollow>();

            if (_orbit == null)
            {
                Debug.LogError($"{nameof(AutoExplore)} needs an Orbital Follow body.");
                enabled = false;
                return;
            }

            // dummy follow target (camera orbits this)
            var dummy = new GameObject("AutoExploreTarget");
            _followTarget = dummy.transform;
            _followTarget.position = Vector3.zero;
            _vcam.Follow = _followTarget;

            // initialise glide targets
            _targetFocus = _followTarget.position;
            _targetRadius = Mathf.Lerp(minRadius, maxRadius, 0.5f);
            _targetHeight = Mathf.Lerp(minHeight, maxHeight, 0.5f);
            _targetHeading = _orbit.HorizontalAxis.Value;

            _orbit.Radius = _targetRadius;
            _orbit.TargetOffset = new Vector3(0f, _targetHeight, 0f);

            _nextDecisionTime = Time.time + Random.Range(dwellTime.x, dwellTime.y);
        }

        void LateUpdate()
        {
            DecideIfWeNeedANewTarget();
            ApplySmoothing();
            ApplyPerlinDrift();
        }

        // ─────────── decision & motion helpers ───────────────────────
        void DecideIfWeNeedANewTarget()
        {
            if (Time.time < _nextDecisionTime && _nextInterest == null) return;

            _targetFocus = _nextInterest ?? PickNextFocus();
            _nextInterest = null;
            _targetHeight = Random.Range(minHeight, maxHeight);
            _targetRadius = Random.Range(minRadius, maxRadius);
            _targetHeading = Random.Range(0f, 360f);

            _nextDecisionTime = Time.time + Random.Range(dwellTime.x, dwellTime.y);
        }

        Vector3 PickNextFocus()
        {
            bool useAgent = pointsOfInterest != null &&
                            pointsOfInterest.Length > 0 &&
                            Random.value < agentBias;

            if (useAgent)
            {
                // choose a random agent whose Transform is still alive
                int safety = 10; // avoid potential infinite loop
                while (safety-- > 0)
                {
                    int idx = Random.Range(0, pointsOfInterest.Length);
                    Transform t = pointsOfInterest[idx];
                    if (t != null)
                        return t.position;
                }
            }

            // fallback: random board spot
            return new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
        }

        void ApplySmoothing()
        {
            _followTarget.position = Vector3.SmoothDamp(
                _followTarget.position, _targetFocus,
                ref _focusVelocity, smoothTime);

            _orbit.Radius = Mathf.SmoothDamp(
                _orbit.Radius, _targetRadius,
                ref _radiusVelocity, smoothTime);

            float newY = Mathf.SmoothDamp(
                _orbit.TargetOffset.y, _targetHeight,
                ref _heightVelocity, smoothTime);
            _orbit.TargetOffset = new Vector3(0f, newY, 0f);

            float heading = Mathf.SmoothDampAngle(
                _orbit.HorizontalAxis.Value, _targetHeading,
                ref _headingVelocity, smoothTime);
            _orbit.HorizontalAxis.Value = heading;
        }

        void ApplyPerlinDrift()
        {
            _phase += noiseSpeed * Time.deltaTime;
            float delta = (Mathf.PerlinNoise(_phase, 0f) - 0.5f) * 2f * noiseScale;
            _orbit.HorizontalAxis.Value += delta;

            float min = _orbit.HorizontalAxis.Range.x;
            float max = _orbit.HorizontalAxis.Range.y;
            if (_orbit.HorizontalAxis.Value > max) _orbit.HorizontalAxis.Value = min;
            if (_orbit.HorizontalAxis.Value < min) _orbit.HorizontalAxis.Value = max;
        }
    }
}
