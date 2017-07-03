using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using SharpDX;
using static Yasuo_OP.Helper;
using System.Linq;
using static System.Math;

namespace Yasuo_OP
{
    public static class SpellsManager
    {
        public static Spell.Skillshot Q;
        private static Spell.Skillshot Q3;
        public static int QCircleRange;

        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Targeted R;
        public static int QRange;

        public static void Load()
        {
            Q = GetQ();
            Q3 = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 300, 1200, 90)
            {
                AllowedCollisionCount = int.MaxValue
            };

            QCircleRange = 375;

            W = new Spell.Active(SpellSlot.W, 400);
            E = new Spell.Targeted(SpellSlot.E, 475);
            R = new Spell.Targeted(SpellSlot.R, 1200);
        }

        public static int GetQRange()
        {
            return HasQ3() ? 1100 : 475;
        }

        private static int QSpeed()
        {
            return (int) (1/(1/0.5*Player.Instance.AttackSpeedMod));
        }

        private static Spell.Skillshot GetQ()
        {
            return new Spell.Skillshot(SpellSlot.Q, 475, SkillShotType.Linear, 325, QSpeed(), 55)
            {
                AllowedCollisionCount = int.MaxValue
            };
        }

        public static int EDelay()
        {
            return new[] { 1000, 900, 800, 700, 600 }[E.Level - 1];
        }

        #region Casts

        public static void CastQ(this Obj_AI_Base target)
        {
            var Q = GetQ();
            if (!Q.IsReady() || target == null || target.IsInvulnerable) return;

            if (HasQ3())
            {
                var pred = Q3.GetPrediction(target);
                if (pred.HitChancePercent >= 80)
                {
                    Q3.Cast(target);
                }
            }
            else
            {
                var pred = Q.GetPrediction(target);
                if (pred.HitChancePercent >= 80)
                {
                    Q.Cast(target);
                }
            }
        }

        public static void CastE(this Obj_AI_Base target)
        {
            if (!E.IsReady() || target == null || target.HasEBuff()) return;

            if (target.IsSafeToE()) E.Cast(target);
        }

        public static void CastR(this Obj_AI_Base target)
        {
            if (!R.IsReady() || target == null || !target.IsKnockedUp()) return;

            R.Cast(target);
        }

        #endregion Casts

        #region Damages


    public static float GetDamage(this Obj_AI_Base target, SpellSlot slot)
        {
            var dmg = 0f;

            var me = Player.Instance;
            var level = me.Spellbook.GetSpell(slot).Level - 1;

            if (!me.Spellbook.GetSpell(slot).IsReady || target == null)
            {
                return 0f;
            }

            if (slot == SpellSlot.Q)
            {
                dmg += new[] {20f, 40f, 60f, 80f, 100f}[level] + me.TotalAttackDamage;
            }
            else if (slot == SpellSlot.E)
            {
                var baseDamageArray = new[] {60, 70, 80, 90, 100};

                var eStackCount = me.Buffs.Count(b => b.DisplayName == "YasuoDashScalar");
                var eDmgScaling = Math.Min(1.5f, 1 + 0.25 * eStackCount);
                var baseDamageWithEStacks = baseDamageArray[level - 1] * eDmgScaling;

                var bonusAd = me.TotalAttackDamage - me.BaseAttackDamage;
                var extraAdDamage = 0.2f + bonusAd;
                var extraApDamage = 0.6f * me.FlatMagicDamageMod;

                var totalRawDamage = baseDamageWithEStacks + extraAdDamage + extraApDamage;

                return me.CalculateDamageOnUnit(target, DamageType.Magical, (float)totalRawDamage);
            }
            else if (slot == SpellSlot.R)
            {
                dmg += new[] {200f, 300f, 400f}[level] + me.FlatPhysicalDamageMod * 1.5f;
            }

            return dmg - 10f;
        }

        #endregion Damages
    }
}
