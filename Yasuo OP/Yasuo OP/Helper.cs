using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using static Yasuo_OP.SpellsManager;

namespace Yasuo_OP
{
    public static class Helper
    {
        public static AIHeroClient Me => Player.Instance;

        public static bool HasEBuff(this Obj_AI_Base target)
        {
            return target.HasBuff("yasuodashwrapper");
        }

        public static bool IsKnockedUp(this Obj_AI_Base target)
        {
            return target.HasBuffOfType(BuffType.Knockup) || target.HasBuffOfType(BuffType.Knockback);
        }

        public static bool HasQ3()
        {
            return Me.HasBuff("yasuoq3w");
        }

        public static bool HasShield()
        {
            return Me.Mana >= 100;
        }

        public static int ECount()
        {
            var count = Me.GetBuffCount("yasuodashscalar");
            return count > 0 ? count : 0;
        }

        public static Obj_AI_Base GetNearestTower()
        {
            return EntityManager.Turrets.Enemies.FirstOrDefault(t => t.IsValid && !t.IsDead);
        }

        public static bool IsUnderTower(this Vector3 position)
        {
            return
                EntityManager.Turrets.Enemies.Where(a => a.Health > 0 && !a.IsDead).Any(a => a.Distance(position) <= 1100);
        }

        public static int GetLowestKnockupTime()
        {
            return
                (int) EntityManager.Heroes.Enemies.Where(a => a.IsKnockedUp() && a.IsValidTarget(R.Range)).Select(a =>
                {
                    var buff = a.Buffs.FirstOrDefault(b => b.IsKnockup || b.IsKnockback);
                    return buff != null ? (buff.EndTime - Game.Time)*1000 : 0;
                }).OrderBy(a => a).FirstOrDefault();
        }

        public static bool IsSafeToE(this Obj_AI_Base target)
        {
            var position = target.GetPosAfterE();

            if (position.IsUnderTower()) return false;

            if (position.CountEnemiesInRange(900) >= 3) return false;

            if (position.CountEnemiesInRange(900) >= 2 && Me.HealthPercent < 40) return false;

            return true;
        }
    }
}
