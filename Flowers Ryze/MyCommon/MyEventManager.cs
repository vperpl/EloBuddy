namespace Flowers_Ryze.MyCommon
{
    #region

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;
    using EloBuddy.SDK.Menu.Values;
    using EloBuddy.SDK.Rendering;

    using Flowers_Ryze.MyBase;

    using System;
    using System.Drawing;
    using System.Linq;

    #endregion

    internal class MyEventManager : MyLogic
    {
        internal static void Initializer()
        {
            try
            {
                Game.OnUpdate += OnUpdate;
                Spellbook.OnCastSpell += OnCastSpell;
                GameObject.OnCreate += OnCreate;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
                Orbwalker.OnPreAttack += OnPreAttack;
                Drawing.OnDraw += OnDraw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.Initializer." + ex);
            }
        }

        private static void OnUpdate(EventArgs Args)
        {
            try
            {
                SetCoolDownTime();

                if (Me.IsDead || Me.IsRecalling())
                {
                    return;
                }

                KillStealEvent();

                if (Orbwalker.ModeIsActive(Orbwalker.ActiveModes.Combo))
                {
                    ComboEvent();
                }

                if (Orbwalker.ModeIsActive(Orbwalker.ActiveModes.Harass))
                {
                    HarassEvent();
                }

                if (Orbwalker.ModeIsActive(Orbwalker.ActiveModes.LaneClear) ||
                    Orbwalker.ModeIsActive(Orbwalker.ActiveModes.JungleClear))
                {
                    ClearEvent();
                }

                if (Orbwalker.ModeIsActive(Orbwalker.ActiveModes.LastHit))
                {
                    LastHitEvent();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnUpdate." + ex);
            }
        }

        private static void SetCoolDownTime()
        {
            try
            {
                R.Range = Me.Spellbook.GetSpell(SpellSlot.R).Level > 0
                    ? (uint)Me.Spellbook.GetSpell(SpellSlot.R).Level * 1500
                    : 0;

                QcdEnd = Me.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires;
                WcdEnd = Me.Spellbook.GetSpell(SpellSlot.W).CooldownExpires;
                EcdEnd = Me.Spellbook.GetSpell(SpellSlot.E).CooldownExpires;

                Qcd = Me.Spellbook.GetSpell(SpellSlot.Q).Level > 0 ? CheckCD(QcdEnd) : -1;
                Wcd = Me.Spellbook.GetSpell(SpellSlot.W).Level > 0 ? CheckCD(WcdEnd) : -1;
                Ecd = Me.Spellbook.GetSpell(SpellSlot.E).Level > 0 ? CheckCD(EcdEnd) : -1;

                CanShield = Orbwalker.ModeIsActive(Orbwalker.ActiveModes.Combo) &&
                            (ComboMenu["FlowersRyze.ComboMenu.Mode"].Cast<ComboBox>().CurrentValue == 0 &&
                             Me.HealthPercent() <=
                             ComboMenu["FlowersRyze.ComboMenu.ShieldHP"].Cast<Slider>().CurrentValue ||
                             ComboMenu["FlowersRyze.ComboMenu.Mode"].Cast<ComboBox>().CurrentValue == 1) &&
                            Me.Spellbook.GetSpell(SpellSlot.Q).Level > 0 && Me.Spellbook.GetSpell(SpellSlot.W).Level > 0 &&
                            Me.Spellbook.GetSpell(SpellSlot.E).Level > 0;

                if (!Orbwalker.ModeIsActive(Orbwalker.ActiveModes.Combo) ||
                    ComboMenu["FlowersRyze.ComboMenu.QSmart"].Cast<CheckBox>().CurrentValue == false ||
                    CanShield == false)
                {
                    Q.AllowedCollisionCount = 0;
                }
                else
                {
                    Q.AllowedCollisionCount = int.MaxValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.SetCoolDownTime." + ex);
            }
        }

        private static float CheckCD(float Expires)
        {
            try
            {
                var time = Expires - Game.Time;

                if (time < 0)
                {
                    time = 0;

                    return time;
                }

                return time;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.CheckCD." + ex);
                return -1;
            }
        }

        private static void KillStealEvent()
        {
            try
            {
                if (Me.CountEnemyChampionsInRange(Q.Range) == 0)
                {
                    return;
                }

                if (KillStealMenu["FlowersRyze.KillStealMenu.Q"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                {
                    foreach (
                        var target in
                        EntityManager.Heroes.Enemies.Where(
                            x =>
                                x.IsValidTarget(Q.Range) &&
                                x.Health < x.GetRealQDamage(true) &&
                                !x.IsUnKillable()))
                    {
                        if (target.IsValidTarget(Q.Range) && !target.IsUnKillable())
                        {
                            var qPred = Q.GetPrediction(target);

                            if (qPred.HitChance >= HitChance.High)
                            {
                                Q.Cast(qPred.UnitPosition);
                                return;
                            }
                        }
                    }
                }

                if (KillStealMenu["FlowersRyze.KillStealMenu.W"].Cast<CheckBox>().CurrentValue && W.IsReady())
                {
                    foreach (
                        var target in
                        EntityManager.Heroes.Enemies.Where(
                            x =>
                                x.IsValidTarget(W.Range) &&
                                x.Health < W.GetDamage(x) &&
                                !x.IsUnKillable()))
                    {
                        if (target.IsValidTarget(W.Range) && !target.IsUnKillable())
                        {
                            W.Cast(target);
                            return;
                        }
                    }
                }

                if (KillStealMenu["FlowersRyze.KillStealMenu.E"].Cast<CheckBox>().CurrentValue && E.IsReady())
                {
                    foreach (
                        var target in
                        EntityManager.Heroes.Enemies.Where(
                            x =>
                                x.IsValidTarget(E.Range) &&
                                x.Health < E.GetDamage(x) &&
                                !x.IsUnKillable()))
                    {
                        if (target.IsValidTarget(E.Range) && !target.IsUnKillable())
                        {
                            E.Cast(target);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.KillStealEvent." + ex);
            }
        }

        private static void ComboEvent()
        {
            try
            {
                var target =
                    TargetSelector.GetTarget(
                        EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(Q.Range) && !x.IsUnKillable()),
                        DamageType.Magical);

                if (target != null && target.IsValidTarget(Q.Range))
                {
                    if (ComboMenu["FlowersRyze.ComboMenu.Ignite"].Cast<CheckBox>().CurrentValue && IgniteSlot != SpellSlot.Unknown &&
                        Ignite.IsReady() && target.IsValidTarget(600) &&
                        (target.Health < MyExtraManager.GetComboDamage(target) && target.IsValidTarget(400) ||
                         target.Health < Me.GetIgniteDamage(target)))
                    {
                        Ignite.Cast(target);
                    }

                    if (Core.GameTickCount - LastCastTime > 500)
                    {
                        switch (ComboMenu["FlowersRyze.ComboMenu.Mode"].Cast<ComboBox>().CurrentValue)
                        {
                            case 0:
                                NormalCombo(target, ComboMenu["FlowersRyze.ComboMenu.Q"].Cast<CheckBox>().CurrentValue,
                                    ComboMenu["FlowersRyze.ComboMenu.W"].Cast<CheckBox>().CurrentValue,
                                    ComboMenu["FlowersRyze.ComboMenu.E"].Cast<CheckBox>().CurrentValue);
                                break;
                            case 1:
                                ShieldCombo(target, ComboMenu["FlowersRyze.ComboMenu.Q"].Cast<CheckBox>().CurrentValue,
                                    ComboMenu["FlowersRyze.ComboMenu.W"].Cast<CheckBox>().CurrentValue,
                                    ComboMenu["FlowersRyze.ComboMenu.E"].Cast<CheckBox>().CurrentValue);
                                break;
                            default:
                                BurstCombo(target, ComboMenu["FlowersRyze.ComboMenu.Q"].Cast<CheckBox>().CurrentValue,
                                    ComboMenu["FlowersRyze.ComboMenu.W"].Cast<CheckBox>().CurrentValue,
                                    ComboMenu["FlowersRyze.ComboMenu.E"].Cast<CheckBox>().CurrentValue);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.ComboEvent." + ex);
            }
        }

        private static void NormalCombo(AIHeroClient target, bool useQ, bool useW, bool useE)
        {
            try
            {
                if (target != null && target.IsValidTarget(Q.Range))
                {
                    if (Core.GameTickCount - LastCastTime > 500)
                    {
                        if (CanShield)
                        {
                            if (useQ && Q.IsReady() &&
                                (FullStack || NoStack ||
                                 HalfStack && !W.IsReady() && Wcd > 1 && !E.IsReady() && Ecd > 1) &&
                                target.IsValidTarget(Q.Range))
                            {
                                var qPred = Q.GetPrediction(target);

                                if (qPred.HitChance >= HitChance.High)
                                {
                                    Q.Cast(qPred.UnitPosition);
                                }
                            }

                            if (useW && W.IsReady() && (!FullStack || HaveShield) &&
                                target.IsValidTarget(W.Range) &&
                                (Ecd >= 2 || target.HasBuff("ryzee")))
                            {
                                W.Cast(target);
                            }

                            if (useE && E.IsReady() && (!FullStack || HaveShield) &&
                                target.IsValidTarget(E.Range))
                            {
                                if (NoStack)
                                {
                                    E.Cast(target);
                                }

                                var minions =
                                    ObjectManager.Get<Obj_AI_Minion>().Where(
                                            x => x.IsValidTarget(E.Range, true) && (x.IsMinion() || x.IsMob()))
                                        .Where(
                                            x =>
                                                x.Health < E.GetDamage(x) &&
                                                EntityManager.Heroes.Enemies.Any(a => a.Distance(x.Position) <= 290))
                                        .ToArray();

                                if (minions.Any())
                                {
                                    foreach (var minion in minions.Where(x => x.IsValidTarget(E.Range)).OrderByDescending(x => x.Distance(target)))
                                    {
                                        if (minion != null && minion.IsValidTarget(E.Range))
                                        {
                                            E.Cast(minion);
                                        }
                                    }
                                }
                                else if (target.IsValidTarget(E.Range))
                                {
                                    E.Cast(target);
                                }
                            }
                        }
                        else
                        {
                            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                            {
                                var qPred = Q.GetPrediction(target);

                                if (qPred.HitChance >= HitChance.High)
                                {
                                    Q.Cast(qPred.UnitPosition);
                                }
                            }

                            if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                            {
                                E.Cast(target);
                            }

                            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                            {
                                var qPred = Q.GetPrediction(target);

                                if (qPred.HitChance >= HitChance.High)
                                {
                                    Q.Cast(qPred.UnitPosition);
                                }
                            }

                            if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                            {
                                W.Cast(target);
                            }

                            if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                            {
                                var qPred = Q.GetPrediction(target);

                                if (qPred.HitChance >= HitChance.High)
                                {
                                    Q.Cast(qPred.UnitPosition);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.NormalCombo." + ex);
            }
        }

        private static void ShieldCombo(AIHeroClient target, bool useQ, bool useW, bool useE)
        {
            try
            {
                if (target != null && target.IsValidTarget(Q.Range))
                {
                    if (Core.GameTickCount - LastCastTime > 500)
                    {
                        if (useQ && Q.IsReady() &&
                            (FullStack || NoStack || HalfStack &&
                            !W.IsReady() && Wcd > 1 && !E.IsReady() && Ecd > 1) &&
                             target.IsValidTarget(Q.Range))
                        {
                            var qPred = Q.GetPrediction(target);

                            if (qPred.HitChance >= HitChance.High)
                            {
                                Q.Cast(qPred.UnitPosition);
                            }
                        }

                        if (useW && W.IsReady() && (!FullStack || HaveShield) &&
                            target.IsValidTarget(W.Range) &&
                            (Ecd >= 2 || target.HasBuff("ryzee")))
                        {
                            W.Cast(target);
                        }

                        if (useE && E.IsReady() && (!FullStack || HaveShield) &&
                            target.IsValidTarget(E.Range))
                        {
                            if (NoStack)
                            {
                                E.Cast(target);
                            }

                            var minions =
                                ObjectManager.Get<Obj_AI_Minion>().Where(
                                        x => x.IsValidTarget(E.Range, true) && (x.IsMinion() || x.IsMob()))
                                    .Where(
                                        x =>
                                            x.Health < E.GetDamage(x) &&
                                            EntityManager.Heroes.Enemies.Any(a => a.Distance(x.Position) <= 290))
                                    .ToArray();

                            if (minions.Any())
                            {
                                foreach (var minion in minions.Where(x => x.IsValidTarget(E.Range)).OrderByDescending(x => x.Distance(target)))
                                {
                                    if (minion != null && minion.IsValidTarget(E.Range))
                                    {
                                        E.Cast(minion);
                                    }
                                }
                            }
                            else if (target.IsValidTarget(E.Range))
                            {
                                E.Cast(target);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.ShieldCombo." + ex);
            }
        }

        private static void BurstCombo(AIHeroClient target, bool useQ, bool useW, bool useE)
        {
            try
            {
                if (target != null && target.IsValidTarget(Q.Range))
                {
                    if (Core.GameTickCount - LastCastTime > 500)
                    {
                        if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                        {
                            var qPred = Q.GetPrediction(target);

                            if (qPred.HitChance >= HitChance.High)
                            {
                                Q.Cast(qPred.UnitPosition);
                            }
                        }

                        if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                        {
                            E.Cast(target);
                        }

                        if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                        {
                            var qPred = Q.GetPrediction(target);

                            if (qPred.HitChance >= HitChance.High)
                            {
                                Q.Cast(qPred.UnitPosition);
                            }
                        }

                        if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                        {
                            W.Cast(target);
                        }

                        if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                        {
                            var qPred = Q.GetPrediction(target);

                            if (qPred.HitChance >= HitChance.High)
                            {
                                Q.Cast(qPred.UnitPosition);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.BurstCombo." + ex);
            }
        }

        private static void HarassEvent()
        {
            try
            {
                if (Me.ManaPercent() >= HarassMenu["FlowersRyze.HarassMenu.Mana"].Cast<Slider>().CurrentValue)
                {
                    if (HarassMenu["FlowersRyze.HarassMenu.Q"].Cast<CheckBox>().CurrentValue && Q.IsReady() && !FullStack)
                    {
                        var minions =
                             ObjectManager.Get<Obj_AI_Minion>()
                             .Where(x => x.IsValidTarget(Q.Range, true) && (x.IsMinion() || x.IsMob()))
                                .Where(
                                    x =>
                                        x.HasBuff("RyzeE") && x.Health < x.GetRealQDamage() &&
                                        EntityManager.Heroes.Enemies.Any(a => a.Distance(x) <= 290))
                                        .ToArray();

                        if (minions.Any())
                        {
                            foreach (var minion in minions.Where(x => x.IsValidTarget(Q.Range)))
                            {
                                if (minion != null && minion.IsValidTarget(Q.Range))
                                {
                                    var qPred = Q.GetPrediction(minion);

                                    if (qPred.HitChance >= HitChance.Medium)
                                    {
                                        Q.Cast(qPred.UnitPosition);
                                    }
                                }
                            }
                        }
                        else
                        {
                            var target =
                                TargetSelector.GetTarget(
                                    EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(Q.Range)),
                                    DamageType.Magical);

                            if (target != null && target.IsValidTarget(Q.Range))
                            {
                                var qPred = Q.GetPrediction(target);

                                if (qPred.HitChance >= HitChance.High)
                                {
                                    Q.Cast(qPred.UnitPosition);
                                }
                            }
                        }
                    }

                    if (HarassMenu["FlowersRyze.HarassMenu.E"].Cast<CheckBox>().CurrentValue && E.IsReady() && !HalfStack)
                    {
                        var minions =
                            ObjectManager.Get<Obj_AI_Minion>()
                                .Where(x => x.IsValidTarget(E.Range, true) && (x.IsMinion() || x.IsMob()))
                                .Where(
                                    x =>
                                        x.Health < E.GetDamage(x) &&
                                        EntityManager.Heroes.Enemies.Any(a => a.Distance(x.Position) <= 290))
                                .ToArray();

                        if (minions.Any())
                        {
                            foreach (var minion in minions.Where(x => x.IsValidTarget(E.Range)))
                            {
                                if (minion != null && minion.IsValidTarget(E.Range))
                                {
                                    E.Cast(minion);
                                }
                            }
                        }
                        else
                        {
                            var target =
                                TargetSelector.GetTarget(
                                    EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(E.Range)),
                                    DamageType.Magical);

                            if (target != null && target.IsValidTarget(E.Range))
                            {
                                E.Cast(target);
                            }
                        }
                    }

                    if (HarassMenu["FlowersRyze.HarassMenu.W"].Cast<CheckBox>().CurrentValue && W.IsReady() && !HalfStack)
                    {
                        var target =
                            TargetSelector.GetTarget(
                                EntityManager.Heroes.Enemies.Where(x => x.IsValidTarget(W.Range) && !x.HasBuff("RyzeE")),
                                DamageType.Magical);

                        if (target != null && target.IsValidTarget(W.Range))
                        {
                            W.Cast(target);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.HarassEvent." + ex);
            }
        }

        private static void ClearEvent()
        {
            try
            {
                if (MyManaManager.SpellHarass && Me.CountEnemyChampionsInRange(Q.Range) > 0)
                {
                    HarassEvent();
                }

                if (MyManaManager.SpellFarm)
                {
                    if (Orbwalker.ModeIsActive(Orbwalker.ActiveModes.LaneClear))
                    {
                        LaneClearEvent();
                    }

                    if (Orbwalker.ModeIsActive(Orbwalker.ActiveModes.JungleClear))
                    {
                        JungleClearEvent();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.ClearEvent." + ex);
            }
        }

        private static void LaneClearEvent()
        {
            try
            {
                if (Me.ManaPercent() >= ClearMenu["FlowersRyze.ClearMenu.LaneClearMana"].Cast<Slider>().CurrentValue)
                {
                    var minions =
                        EntityManager.MinionsAndMonsters.EnemyMinions.Where(x => x.IsValidTarget(Q.Range, true) && x.IsMinion()).ToArray();

                    if (minions.Any())
                    {
                        var eMinionsList = minions.Where(x => x.HasBuff("RyzeE")).ToArray();

                        if (eMinionsList.Any())
                        {
                            foreach (
                                var eMinion in
                                eMinionsList.Where(
                                    x =>
                                        EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                                            a =>
                                                a.IsValidTarget(300, true, x.ServerPosition) && a.IsMinion() &&
                                                a.NetworkId != x.NetworkId) >= 2))
                            {
                                if (eMinion != null && eMinion.IsValidTarget(Q.Range))
                                {
                                    if (ClearMenu["FlowersRyze.ClearMenu.LaneClearE"].Cast<CheckBox>().CurrentValue && 
                                        E.IsReady() && eMinion.IsValidTarget(E.Range))
                                    {
                                        E.Cast(eMinion);
                                        return;
                                    }

                                    if (ClearMenu["FlowersRyze.ClearMenu.LaneClearQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                                    {
                                        var qPred = Q.GetPrediction(eMinion);

                                        if (qPred.HitChance >= HitChance.High)
                                        {
                                            Q.Cast(qPred.UnitPosition);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var minion in minions)
                            {
                                if (ClearMenu["FlowersRyze.ClearMenu.LaneClearE"].Cast<CheckBox>().CurrentValue && E.IsReady())
                                {
                                    foreach (
                                        var eMinion in
                                        minions.Where(
                                                x =>
                                                EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                                                        a =>
                                                            a.IsValidTarget(300, true, x.ServerPosition) &&
                                                            a.IsMinion() && a.NetworkId != x.NetworkId) >= 2)
                                            .OrderByDescending(
                                                x =>
                                                    EntityManager.MinionsAndMonsters.EnemyMinions.Count(
                                                        a =>
                                                            a.IsValidTarget(300, true, x.ServerPosition) &&
                                                            a.IsMinion() && a.NetworkId != x.NetworkId)))
                                    {
                                        if (eMinion != null && eMinion.IsValidTarget(E.Range))
                                        {
                                            E.Cast(eMinion);
                                            return;
                                        }
                                    }
                                }

                                if (ClearMenu["FlowersRyze.ClearMenu.LaneClearQ"].Cast<CheckBox>().CurrentValue && Q.IsReady() &&
                                    minion.IsValidTarget(Q.Range) && minion.Health < minion.GetRealQDamage())
                                {
                                    var qPred = Q.GetPrediction(minion);

                                    if (qPred.HitChance >= HitChance.High)
                                    {
                                        Q.Cast(qPred.UnitPosition);
                                        return;
                                    }
                                }

                                if (ClearMenu["FlowersRyze.ClearMenu.LaneClearW"].Cast<CheckBox>().CurrentValue && W.IsReady() &&
                                    minion.IsValidTarget(W.Range) && minion.Health < W.GetDamage(minion))
                                {
                                    W.Cast(minion);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.LaneClearEvent." + ex);
            }
        }

        private static void JungleClearEvent()
        {
            try
            {
                if (Me.ManaPercent() >= ClearMenu["FlowersRyze.ClearMenu.JungleClearMana"].Cast<Slider>().CurrentValue)
                {
                    var mobs =
                        EntityManager.MinionsAndMonsters.Monsters.Where(x => x.IsValidTarget(Q.Range, true) && x.IsMob())
                            .OrderBy(x => x.MaxHealth)
                            .ToArray();

                    foreach (var mob in mobs.Where(x => x.IsValidTarget(Q.Range)).OrderBy(x => x.MaxHealth))
                    {
                        if (mob != null && mob.IsValidTarget(Q.Range))
                        {
                            if (ClearMenu["FlowersRyze.ClearMenu.JungleClearQ"].Cast<CheckBox>().CurrentValue &&
                                Q.IsReady() && mob.IsValidTarget(Q.Range))
                            {
                                Q.Cast(mob.ServerPosition);
                            }

                            if (ClearMenu["FlowersRyze.ClearMenu.JungleClearE"].Cast<CheckBox>().CurrentValue &&
                                E.IsReady() && mob.IsValidTarget(E.Range))
                            {
                                E.Cast(mob);
                            }

                            if (ClearMenu["FlowersRyze.ClearMenu.JungleClearW"].Cast<CheckBox>().CurrentValue && 
                                W.IsReady() && mob.IsValidTarget(W.Range))
                            {
                                W.Cast(mob);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.JungleClearEvent." + ex);
            }
        }

        private static void LastHitEvent()
        {
            try
            {
                if (Me.ManaPercent() >= LastHitMenu["FlowersRyze.LastHitMenu.LastHitMana"].Cast<Slider>().CurrentValue)
                {
                    if (LastHitMenu["FlowersRyze.LastHitMenu.LastHitQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                    {
                        var qMinions =
                            ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(Q.Range, true) && (x.IsMinion() || x.IsMob()))
                                .ToArray();

                        if (qMinions.Any())
                        {
                            foreach (var minion in qMinions.Where(x => x.IsValidTarget(Q.Range) && x.Health < Q.GetDamage(x)))
                            {
                                if (minion.IsValidTarget(Q.Range))
                                {
                                    var qPred = Q.GetPrediction(minion);

                                    if (qPred.HitChance >= HitChance.High)
                                    {
                                        Q.Cast(qPred.UnitPosition);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.LastHitEvent." + ex);
            }
        }

        private static void OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs Args)
        {
            try
            {
                if (sender.Owner.IsMe)
                {
                    if (Args.Slot == SpellSlot.Q || Args.Slot == SpellSlot.W || Args.Slot == SpellSlot.E)
                    {
                        LastCastTime = Core.GameTickCount;
                        Player.IssueOrder(GameObjectOrder.MoveTo, Me.ServerPosition.Extend(Game.CursorPos, 200).To3D());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnCastSpell." + ex);
            }
        }

        private static void OnCreate(GameObject sender,EventArgs Args)
        {
            try
            {
                if (W.IsReady())
                {
                    var Rengar = EntityManager.Heroes.Enemies.Find(heros => heros.ChampionName.Equals("Rengar"));
                    var Khazix = EntityManager.Heroes.Enemies.Find(heros => heros.ChampionName.Equals("Khazix"));

                    if (MiscMenu["FlowersRyze.MiscMenu.WRengar"].Cast<CheckBox>().CurrentValue && Rengar != null)
                    {
                        if (sender.Name == "Rengar_LeapSound.troy" && Rengar.IsValidTarget(W.Range))
                        {
                            W.Cast(Rengar);
                        }
                    }

                    if (MiscMenu["FlowersRyze.MiscMenu.WKhazix"].Cast<CheckBox>().CurrentValue && Khazix != null)
                    {
                        if (sender.Name == "Khazix_Base_E_Tar.troy" && Khazix.IsValidTarget(300f))
                        {
                            W.Cast(Khazix);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnCreate." + ex);
            }
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs Args)
        {
            try
            {
                if (Me.IsDead || sender == null || !sender.IsEnemy || 
                    !MiscMenu["FlowersRyze.MiscMenu.WMelee"].Cast<CheckBox>().CurrentValue || !W.IsReady())
                {
                    return;
                }

                if (Args.Target != null && Args.Target.IsMe && sender.Type == GameObjectType.AIHeroClient &&
                    sender.IsMelee && sender.IsValidTarget(W.Range) && !sender.HaveShiled())
                {
                    W.Cast(sender);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnProcessSpellCast." + ex);
            }
        }

        private static void OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs Args)
        {
            try
            {
                if (ComboMenu["FlowersRyze.ComboMenu.DisableAttack"].Cast<ComboBox>().CurrentValue == 2 ||
                    Orbwalker.ModeIsActive(Orbwalker.ActiveModes.Combo) || target == null ||
                    target.Type != GameObjectType.AIHeroClient ||
                    target.Health < Me.GetAutoAttackDamage((AIHeroClient)target))
                {
                    return;
                }

                switch (ComboMenu["FlowersRyze.ComboMenu.DisableAttack"].Cast<ComboBox>().CurrentValue)
                {
                    case 0:
                        if (W.IsReady() || E.IsReady())
                        {
                            Args.Process = false;
                        }
                        break;
                    case 1:
                        Args.Process = false;
                        break;
                    default:
                        Args.Process = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnPreAttack." + ex);
            }
        }

        private static void OnDraw(EventArgs Args)
        {
            try
            {
                if (Me.IsDead)
                {
                    return;
                }

                if (DrawMenu["FlowersRyze.DrawMenu.Q"].Cast<CheckBox>().CurrentValue && Q.IsReady())
                {
                    Circle.Draw(Color.FromArgb(251, 0, 133).ToSharpDX(), Q.Range, Me.Position);
                }

                if (DrawMenu["FlowersRyze.DrawMenu.W"].Cast<CheckBox>().CurrentValue && W.IsReady())
                {
                    Circle.Draw(Color.FromArgb(86, 0, 255).ToSharpDX(), R.Range, Me.Position);
                }

                if (DrawMenu["FlowersRyze.DrawMenu.E"].Cast<CheckBox>().CurrentValue && E.IsReady())
                {
                    Circle.Draw(Color.FromArgb(0, 136, 255).ToSharpDX(), E.Range, Me.Position);
                }

                if (DrawMenu["FlowersRyze.DrawMenu.R"].Cast<CheckBox>().CurrentValue && R.IsReady())
                {
                    Circle.Draw(Color.FromArgb(0, 255, 161).ToSharpDX(), R.Range, Me.Position);
                }

                if (DrawMenu["FlowersRyze.DrawMenu.Combo"].Cast<CheckBox>().CurrentValue)
                {
                    var MePos = Drawing.WorldToScreen(ObjectManager.Player.Position);

                    Drawing.DrawText(MePos[0] - 84, MePos[1] + 88, Color.FromArgb(242, 120, 34),
                        "Combo Mode(" +
                        new string(
                            System.Text.Encoding.Default.GetChars(
                                BitConverter.GetBytes(
                                    ComboMenu["FlowersRyze.ComboMenu.ModeKey"].Cast<KeyBind>().Keys.Item1))),
                        10);
                    Drawing.DrawText(MePos[0] + 19, MePos[1] + 88, Color.FromArgb(242, 120, 34),
                        "): " +
                        ComboMenu["FlowersRyze.ComboMenu.Mode"].Cast<ComboBox>().SelectedText);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyEventManager.OnRender." + ex);
            }
        }
    }
}