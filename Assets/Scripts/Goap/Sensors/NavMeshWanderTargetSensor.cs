using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace NonPlayable.Goap.Sensors
{
    /// <summary>
    /// Picks the next wander position somewhere on the NavMesh, biased
    /// toward continuing in roughly the same direction so the agent
    /// doesn’t ping-pong.
    /// </summary>
    [GoapId("NavMeshWanderTargetSensor-0af7b2c8-a313-4f18-88f3-40c0702a81d9")]
    public class NavMeshWanderTargetSensor : LocalTargetSensorBase
    {
        // --------------------------------------------------------------------
        // static data – shared by every instance
        // --------------------------------------------------------------------
        private static readonly Bounds NavBounds;
        private const int MaxTries = 16;          // tries per sample
        private const float SampleDist = 2f;    // clamp to mesh
        private const float MinStep = 6f;       // how far at least
        private const float MaxStep = 32f;      // how far at most
        private const float DirJitter = 15f;      // deg of allowed turn

        // compute bounds of the baked NavMesh once when the class is first touched
        static NavMeshWanderTargetSensor()
        {
            var tri = NavMesh.CalculateTriangulation();
            if (tri.vertices.Length == 0)
            {
                // fallback to a 0-sized bounds around world origin
                NavBounds = new Bounds(Vector3.zero, Vector3.zero);
                Debug.LogWarning("[NavMeshWander] Could not read NavMesh triangulation; " +
                                 "bounds default to (0,0,0). Make sure your NavMesh is baked.");
                return;
            }

            var b = new Bounds(tri.vertices[0], Vector3.zero);
            for (int i = 1; i < tri.vertices.Length; i++)
                b.Encapsulate(tri.vertices[i]);

            NavBounds = b;
        }

        // --------------------------------------------------------------------
        // instance methods
        // --------------------------------------------------------------------
        public override ITarget Sense(IActionReceiver agent,
                                      IComponentReference _,
                                      ITarget previous)
        {
            // -------------------------------------------
            // 1) figure out a preferred direction
            // -------------------------------------------
            Vector3 currentPos = agent.Transform.position;

            // If we had a previous wander target, keep the heading with a bit of jitter.
            Vector3 dir =
                previous is PositionTarget pt
                    ? (pt.Position - currentPos).normalized
                    : Random.insideUnitSphere.WithY(0).normalized;

            // add ±DirJitter degrees of random yaw so we do not walk perfectly straight
            dir = Quaternion.Euler(0,
                                   Random.Range(-DirJitter, DirJitter),
                                   0) * dir;

            // -------------------------------------------
            // 2) try a few distances along that direction
            // -------------------------------------------
            for (int i = 0; i < MaxTries; i++)
            {
                float step = Random.Range(MinStep, MaxStep);
                Vector3 guess = currentPos + dir * step;

                // clamp guess into NavMesh bounding box
                guess = NavBounds.Contains(guess) ? guess : NavBounds.ClosestPoint(guess);

                // snap to mesh
                if (!NavMesh.SamplePosition(guess, out var hit, SampleDist, NavMesh.AllAreas))
                    continue;

                // path test
                var path = new NavMeshPath();
                if (NavMesh.CalculatePath(currentPos, hit.position, NavMesh.AllAreas, path) &&
                    path.status == NavMeshPathStatus.PathComplete)
                {
                    // reuse PositionTarget if possible
                    return previous is PositionTarget prev ? prev.SetPosition(hit.position)
                                                            : new PositionTarget(hit.position);
                }

                // failed → pick a fresh random direction next iteration
                dir = Random.insideUnitSphere.WithY(0).normalized;
            }

            // fallback to staying put
            return previous is PositionTarget pPrev
                     ? pPrev
                     : new PositionTarget(currentPos);
        }

        public override void Created() { }
        public override void Update() { }
    }

    // small helper so we can write "Random.insideUnitSphere.WithY(0)"
    internal static class VecExt
    {
        public static Vector3 WithY(this Vector3 v, float y)
            => new Vector3(v.x, y, v.z);
    }
}
