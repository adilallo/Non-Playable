using UnityEngine;
using UnityEngine.AI;
using Unity.Cinemachine;

namespace NonPlayable.Cinemachine
{
    /// <summary>
    /// Top-down camera that glides between agents, random spots, and
    /// occasional wide shots, staying within Terrain/NavMesh bounds and
    /// outside a designated mountain collider.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class AutoExplore : MonoBehaviour
    {
        // ───── Inspector ─────────────────────────────────────────────
        [Header("Bounds Detection")]
        public bool DeriveFromTerrain = true;
        public bool DeriveFromNavMesh = false;
        public Transform GroundRoot;              // parent holding Terrain tiles

        [Header("Agents / POIs")]
        public Transform[] PointsOfInterest;

        [Tooltip("Transform at board centre used for wide pull-outs")]
        public Transform BoardCentre;

        [Range(0f, 1f)] public float WideShotBias = 0.1f;

        [Header("Baseline Orbit")]
        public float MinRadius = 8f;
        public float MaxRadius = 14f;

        [Header("Altitude")]
        public float MinHeight = 12f;
        public float MaxHeight = 20f;

        [Header("Elevation (Vertical Axis)")]
        public float MinVerticalDeg = 0f;
        public float MaxVerticalDeg = 45f;

        [Header("Timing")]
        public Vector2 DwellTime = new(6, 12);

        [Header("Noise")]
        public float NoiseScale = 0.2f;
        public float NoiseSpeed = 0.02f;

        [Header("Smoothing")]
        public float SmoothTime = 1f;
        // ─────────────────────────────────────────────────────────────

        /// <summary>External systems can force-focus a world position.</summary>
        public void MarkInterest(Vector3 position) => _nextInterest = position;

        // ───── Private state ────────────────────────────────────────
        CinemachineCamera _vcam;
        CinemachineOrbitalFollow _orbit;
        Transform _followTarget;

        float _phase;
        float _nextDecisionTime;
        Vector3 _targetFocus;
        float _targetRadius;
        float _targetHeight;
        float _targetHeading;
        float _targetVertical;
        Vector3? _nextInterest;

        Vector3 _focusVelocity;
        float _radiusVelocity;
        float _heightVelocity;
        float _headingVelocity;
        float _verticalVelocity;

        Vector2 _boardMin;
        Vector2 _boardMax;
        // ─────────────────────────────────────────────────────────────

        void Awake()
        {
            _vcam = GetComponent<CinemachineCamera>();
            _orbit = _vcam.GetComponent<CinemachineOrbitalFollow>();

            if (_orbit == null)
            {
                Debug.LogError($"{nameof(AutoExplore)} requires an Orbital Follow body.");
                enabled = false;
                return;
            }

            // dummy target the vcam will orbit
            GameObject dummy = new GameObject("AutoExploreTarget");
            _followTarget = dummy.transform;
            _vcam.Follow = _followTarget;

            // seed targets
            _targetRadius = Mathf.Lerp(MinRadius, MaxRadius, 0.5f);
            _targetHeight = Mathf.Lerp(MinHeight, MaxHeight, 0.5f);
            _targetHeading = _orbit.HorizontalAxis.Value;
            _targetFocus = Vector3.zero;

            _orbit.Radius = _targetRadius;
            _orbit.TargetOffset = new Vector3(0f, _targetHeight, 0f);
            _orbit.VerticalAxis.Range = new Vector2(MinVerticalDeg, MaxVerticalDeg);

            _targetVertical = Mathf.Lerp(MinVerticalDeg, MaxVerticalDeg, 0.5f);
            _orbit.VerticalAxis.Value = _targetVertical;

            _nextDecisionTime = Time.time + Random.Range(DwellTime.x, DwellTime.y);

            InitBoardBounds();
        }

        void LateUpdate()
        {
            DecideIfWeNeedANewTarget();
            ApplySmoothing();
            ApplyPerlinDrift();
        }

        // ───── Bounds detection ─────────────────────────────────────
        void InitBoardBounds()
        {
            // 1) Terrain tiles
            if (DeriveFromTerrain && GroundRoot != null)
            {
                Terrain[] terrains = GroundRoot.GetComponentsInChildren<Terrain>();
                if (terrains.Length > 0)
                {
                    float minX = float.PositiveInfinity, minZ = float.PositiveInfinity;
                    float maxX = float.NegativeInfinity, maxZ = float.NegativeInfinity;

                    foreach (Terrain t in terrains)
                    {
                        Vector3 pos = t.transform.position;
                        Vector3 size = t.terrainData.size;
                        minX = Mathf.Min(minX, pos.x);
                        minZ = Mathf.Min(minZ, pos.z);
                        maxX = Mathf.Max(maxX, pos.x + size.x);
                        maxZ = Mathf.Max(maxZ, pos.z + size.z);
                    }

                    _boardMin = new(minX, minZ);
                    _boardMax = new(maxX, maxZ);
                    return;
                }
            }

            // 2) NavMesh
            if (DeriveFromNavMesh)
            {
                NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
                if (tri.vertices.Length > 0)
                {
                    float minX = tri.vertices[0].x, maxX = tri.vertices[0].x;
                    float minZ = tri.vertices[0].z, maxZ = tri.vertices[0].z;

                    foreach (Vector3 v in tri.vertices)
                    {
                        if (v.x < minX) minX = v.x;
                        if (v.x > maxX) maxX = v.x;
                        if (v.z < minZ) minZ = v.z;
                        if (v.z > maxZ) maxZ = v.z;
                    }

                    _boardMin = new(minX, minZ);
                    _boardMax = new(maxX, maxZ);
                    return;
                }
            }

            // 3) Fallback 80×80 centred at origin
            _boardMin = new(-40f, -40f);
            _boardMax = new(40f, 40f);
        }

        // ───── Decision logic ───────────────────────────────────────
        void DecideIfWeNeedANewTarget()
        {
            if (Time.time < _nextDecisionTime && _nextInterest == null) return;

            _targetFocus = _nextInterest ?? PickNextFocus();
            _nextInterest = null;
            _targetHeight = Random.Range(MinHeight, MaxHeight);
            _targetRadius = Random.Range(MinRadius, MaxRadius);
            _targetHeading = Random.Range(0f, 360f);
            _targetVertical = Random.Range(MinVerticalDeg, MaxVerticalDeg);

            _nextDecisionTime = Time.time + Random.Range(DwellTime.x, DwellTime.y);
        }

        Vector3 PickNextFocus()
        {
            // 1) Wide pull-out?
            if (BoardCentre != null && Random.value < WideShotBias)
            {
                _targetRadius = MaxRadius;
                _targetHeight = MaxHeight;
                _targetVertical = MaxVerticalDeg;          // optional: camera tilts up for the vista
                _nextDecisionTime = Time.time + DwellTime.y;   // hold the shot to max dwell
                return BoardCentre.position;
            }

            // 2) Choose one of the supplied POIs (agents or other)
            if (PointsOfInterest != null && PointsOfInterest.Length > 0)
            {
                int safety = 8;
                while (safety-- > 0)
                {
                    Transform t = PointsOfInterest[Random.Range(0, PointsOfInterest.Length)];
                    if (t != null)
                    {
                        Vector3 pos = ClampToBoard(t.position);
                        if (InBoard(pos)) return pos;
                    }
                }
            }

            // 3) Fallback: stay where we are (no random board spots)
            return _followTarget.position;
        }

        // ───── Utility ──────────────────────────────────────────────
        bool InBoard(Vector3 p) =>
            p.x >= _boardMin.x && p.x <= _boardMax.x &&
            p.z >= _boardMin.y && p.z <= _boardMax.y;

        Vector3 ClampToBoard(Vector3 p) => new(
            Mathf.Clamp(p.x, _boardMin.x, _boardMax.x),
            p.y,
            Mathf.Clamp(p.z, _boardMin.y, _boardMax.y));

        // ───── Motion helpers ───────────────────────────────────────
        void ApplySmoothing()
        {
            _followTarget.position = Vector3.SmoothDamp(_followTarget.position,
                                                        _targetFocus,
                                                        ref _focusVelocity,
                                                        SmoothTime);

            _orbit.Radius = Mathf.SmoothDamp(_orbit.Radius,
                                             _targetRadius,
                                             ref _radiusVelocity,
                                             SmoothTime);

            float newY = Mathf.SmoothDamp(_orbit.TargetOffset.y,
                                          _targetHeight,
                                          ref _heightVelocity,
                                          SmoothTime);
            _orbit.TargetOffset = new Vector3(0f, newY, 0f);

            float heading = Mathf.SmoothDampAngle(_orbit.HorizontalAxis.Value,
                                                  _targetHeading,
                                                  ref _headingVelocity,
                                                  SmoothTime);
            _orbit.HorizontalAxis.Value = heading;

            float vertical = Mathf.SmoothDamp(_orbit.VerticalAxis.Value,
                                                  _targetVertical,
                                                  ref _verticalVelocity,
                                                  SmoothTime);

            _orbit.VerticalAxis.Value = vertical;
        }

        void ApplyPerlinDrift()
        {
            _phase += NoiseSpeed * Time.deltaTime;
            float delta = (Mathf.PerlinNoise(_phase, 0f) - 0.5f) * 2f * NoiseScale;
            _orbit.HorizontalAxis.Value += delta;

            float min = _orbit.HorizontalAxis.Range.x;
            float max = _orbit.HorizontalAxis.Range.y;

            if (_orbit.HorizontalAxis.Value > max) _orbit.HorizontalAxis.Value = min;
            else if (_orbit.HorizontalAxis.Value < min) _orbit.HorizontalAxis.Value = max;
        }
    }
}
