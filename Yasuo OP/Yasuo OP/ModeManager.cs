using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using static Yasuo_OP.Helper;
using static Yasuo_OP.SpellsManager;

namespace Yasuo_OP
{
    class ModeManager
    {
        public static void Load()
        {
            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            var orbMode = Orbwalker.ActiveModesFlags;

            #region Ult

            var targetR =
                EntityManager.Heroes.Enemies.OrderBy(e => e.Health)
                    .ThenBy(e => e.CountEnemiesInRange(600))
                    .ThenBy(e => e.FlatPhysicalDamageMod)
                    .FirstOrDefault(e => e.IsValidTarget(R.Range) && !e.IsInvulnerable);

            if (targetR != null && GetLowestKnockupTime() <= 250 + Game.Ping)
            {
                targetR.CastR();
            }

            #endregion Ult

            if (orbMode.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                var target = TargetSelector.GetTarget(GetQRange(), DamageType.Physical);

                if (target == null) return;

                if (!Me.IsDashing())
                {
                    target.CastQ();
                }
                else if (DashManager.GetPlayerPosition().IsInRange(target, 375))
                {
                    target.CastQ();
                }


                if (!target.IsInRange(Me, Me.GetAutoAttackRange() + 80))
                {
                    if (target.IsInRange(Me, E.Range))
                    {
                        target.CastE();
                    }
                    else
                    {
                        target.GetBestEnemy().CastE();
                    }
                }
            }

            if (orbMode.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                var target = TargetSelector.GetTarget(GetQRange(), DamageType.Physical);

                target.CastQ();
            }

            if (orbMode.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                //Normal Laneclear(safer)
                if (Me.CountEnemiesInRange(1200) >= 1)
                {
                    if (!Me.IsDashing())
                    {
                        var minionQ =
                            EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Health)
                                .FirstOrDefault(
                                    m =>
                                        m.IsValidTarget(GetQRange()) &&
                                        Prediction.Health.GetPrediction(m,
                                            Q.CastDelay + (E.IsReady() ? 150 : 0) + Game.Ping) <=
                                        m.GetDamage(SpellSlot.Q) + (E.IsReady() ? m.GetDamage(SpellSlot.E) : 0f));

                        if(!HasQ3()) minionQ?.CastQ();

                        var minionE =
                            EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Health)
                                .FirstOrDefault(
                                    m =>
                                        m.IsValidTarget(E.Range) &&
                                        Prediction.Health.GetPrediction(m, 150 + Game.Ping) <= m.GetDamage(SpellSlot.E));
                        minionE?.CastE();
                    }
                }
                //Faster Laneclear(faster, not safe)
                else
                {
                    if (!Me.IsDashing())
                    {
                        //LastHit
                        var minionQ =
                            EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Health)
                                .FirstOrDefault(
                                    m =>
                                        m.IsValidTarget(GetQRange()) &&
                                        Prediction.Health.GetPrediction(m,
                                            Q.CastDelay + (E.IsReady() ? 150 : 0) + Game.Ping) <=
                                        m.GetDamage(SpellSlot.Q) + (E.IsReady() && m.IsSafeToE() ? m.GetDamage(SpellSlot.E) : 0f));

                        minionQ?.CastQ();

                        var minionE =
                            EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Health)
                                .FirstOrDefault(
                                    m =>
                                        m.IsValidTarget(E.Range) &&
                                        Prediction.Health.GetPrediction(m, 150 + Game.Ping) <= m.GetDamage(SpellSlot.E));
                        minionE?.CastE();
                        //Lasthit
                    }

                    if (Me.CountAllyMinionsInRange(700) >= 5)
                    {
                        //Fast
                        var minionEKinda =
                            EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Health)
                                .FirstOrDefault(
                                    m =>
                                        m.IsValidTarget(E.Range) &&
                                        m.GetPosAfterE().CountEnemyMinionsInRange(QCircleRange) >= 1 &&
                                        Prediction.Health.GetPrediction(m, + E.CastDelay + Game.Ping) <= + m.GetDamage(SpellSlot.E));

                        minionEKinda?.CastE();

                        var minionQFast =
                            EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Health)
                                .FirstOrDefault(
                                    m =>
                                        m.IsValidTarget(Me.IsDashing() ? QCircleRange : (int) Q.Range) &&
                                        DashManager.GetPlayerPosition().CountEnemyMinionsInRange(QCircleRange) >= 1);
                        minionQFast?.CastQ();
                        //Fast
                    }

                    if (Me.CountAllyMinionsInRange(700) <= 1)
                    {
                        //Fast
                        var minionEFast =
                            EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Health)
                                .FirstOrDefault(
                                    m =>
                                        m.IsValidTarget(E.Range) &&
                                        m.GetPosAfterE().CountEnemyMinionsInRange(QCircleRange) >= 1);

                        minionEFast?.CastE();

                        var minionQFast =
                            EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Health)
                                .FirstOrDefault(
                                    m =>
                                        m.IsValidTarget(Me.IsDashing() ? QCircleRange : (int) Q.Range) &&
                                        DashManager.GetPlayerPosition().CountEnemyMinionsInRange(QCircleRange) >= 1);
                        minionQFast?.CastQ();
                        //Fast
                    }

                }
            }

            if (orbMode.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                if (!Me.IsDashing())
                {
                    var minionQ =
                        EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Health)
                            .FirstOrDefault(
                                m =>
                                    m.IsValidTarget(Q.Range) &&
                                    Prediction.Health.GetPrediction(m,
                                        Q.CastDelay + (E.IsReady() ? 250 : 0) + Game.Ping) <=
                                    m.GetDamage(SpellSlot.Q) + (E.IsReady() ? m.GetDamage(SpellSlot.E) : 0f));

                    minionQ?.CastQ();

                    var minionE =
                        EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(m => m.Health)
                            .FirstOrDefault(
                                m =>
                                    m.IsValidTarget(E.Range) &&
                                    Prediction.Health.GetPrediction(m, 250 + Game.Ping) <= m.GetDamage(SpellSlot.E));
                    minionE?.CastE();
                }
            }

            if (orbMode.HasFlag(Orbwalker.ActiveModes.Flee))
            {

            }
        }
    }
}
