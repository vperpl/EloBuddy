using System;
using EloBuddy.SDK;
using SharpDX;

namespace Tyler1
{
    using System.Linq;

    public static class Utils
    {
        private static Random _rand = new Random();

        public static Vector3 Randomize(this Vector3 v)
        {
            return new Vector2(v.X + _rand.Next(-10, 10), v.Y + _rand.Next(-10, 10)).To3DWorld();
        }

        public static bool IsUnderEnemyTurret(this Vector3 vector)
        {
            return EntityManager.Turrets.Enemies.Any(x => x.IsValidTarget(900, true, vector));
        }

        public static bool IsUnderEnemyTurret(this EloBuddy.Obj_AI_Base vector)
        {
            return EntityManager.Turrets.Enemies.Any(x => x.IsValidTarget(900, true, vector.Position));
        }
    }
}
