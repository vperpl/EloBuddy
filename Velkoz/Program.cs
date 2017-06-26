#region LICENSE

// Copyright 2014 - 2014 LeagueSharp
// Program.cs is part of Velkoz Assembly.
// 
// Velkoz Assembly is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Velkoz Assembly is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Velkoz Assembly. If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace Velkoz
{
    internal class Program
    {
        public const string ChampionName = "Velkoz";

        public static Spell.Skillshot Q;
        public static Spell.Skillshot QSplit;
        public static Spell.Skillshot QDummy;
        public static Spell.Skillshot W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;

        public static bool QToggleState => Q.Handle.Name.Equals("VelkozQSplitActivate");

        public static SpellSlot IgniteSlot;

        public static MissileClient QMissile;

        //Menu
        public static Menu Config, ComboMenu, HarassMenu, FarmingMenu,  JungleClearMenu, MiscMenu, DrawingsMenu;

        private static AIHeroClient Player;
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName)
                return;
            
            Q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, 250, 1300, 50)
            {
                AllowedCollisionCount = 0
            };
            QSplit = new Spell.Skillshot(SpellSlot.Q, 1100, SkillShotType.Linear, 250, 2100, 55)
            {
                AllowedCollisionCount = 0
            };
            QDummy = new Spell.Skillshot(SpellSlot.Q, (uint)Math.Sqrt(Math.Pow(Q.Range, 2) + Math.Pow(QSplit.Range, 2)), SkillShotType.Linear, 250, int.MaxValue, 55)
            {
                AllowedCollisionCount = int.MaxValue
            };
            W = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Linear, 250, 1700, 85)
            {
                AllowedCollisionCount = int.MaxValue
            };
            E = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Circular, 500, 1500, 100)
            {
                AllowedCollisionCount = int.MaxValue
            };
            R = new Spell.Skillshot(SpellSlot.R, 1550, SkillShotType.Linear, 300, int.MaxValue, 1)
            {
                AllowedCollisionCount = int.MaxValue
            };

            IgniteSlot = Player.GetSpellSlotFromName("SummonerDot");
            
            //Create the menu
            Config = MainMenu.AddMenu(ChampionName, ChampionName);

            ComboMenu = Config.AddSubMenu("Combo", "Combo");
            ComboMenu.Add("UseQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("UseWCombo", new CheckBox("Use W"));
            ComboMenu.Add("UseECombo", new CheckBox("Use E"));
            ComboMenu.Add("UseRCombo", new CheckBox("Use R"));
            ComboMenu.Add("UseIgniteCombo", new CheckBox("Use Ignite"));

            //Harass menu:
            HarassMenu = Config.AddSubMenu("Harass", "Harass");
            HarassMenu.Add("UseQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("UseWHarass", new CheckBox("Use W", false));
            HarassMenu.Add("UseEHarass", new CheckBox("Use E", false));
            HarassMenu.Add("HarassActiveT", new KeyBind("Harass (toggle)!", false, KeyBind.BindTypes.PressToggle, 'Y'));

            //Farming menu:
            FarmingMenu = Config.AddSubMenu("Farm", "Farm");
            FarmingMenu.Add("UseQFarm", new CheckBox("Use Q", false));
            FarmingMenu.Add("UseWFarm", new CheckBox("Use W", false));
            FarmingMenu.Add("UseEFarm", new CheckBox("Use E", false));

            //JungleFarm menu:
            JungleClearMenu = Config.AddSubMenu("JungleFarm", "JungleFarm");
            JungleClearMenu.Add("UseQJFarm", new CheckBox("Use Q"));
            JungleClearMenu.Add("UseWJFarm", new CheckBox("Use W"));
            JungleClearMenu.Add("UseEJFarm", new CheckBox("Use E"));

            //Misc
            MiscMenu = Config.AddSubMenu("Misc", "Misc");
            MiscMenu.Add("InterruptSpells", new CheckBox("Interrupt spells"));
            MiscMenu.AddGroupLabel("Dont use R on");

            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.Team != Player.Team))
            {
                MiscMenu.Add("DontUlt" + enemy.BaseSkinName, new CheckBox(enemy.BaseSkinName, false));

            }
            //Drawings menu:
            DrawingsMenu = Config.AddSubMenu("Drawings", "Drawings");
            DrawingsMenu.Add("QRange", new CheckBox("Draw Q range", false));
            DrawingsMenu.Add("WRange", new CheckBox("Draw W range", false));
            DrawingsMenu.Add("ERange", new CheckBox("Draw E range", false));
            DrawingsMenu.Add("RRange", new CheckBox("Draw R range", false));

            //Add the events we are going to use:
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            EloBuddy.SDK.Events.Interrupter.OnInterruptableSpell += Interrupter2_OnInterruptableTarget;
            GameObject.OnCreate += Obj_SpellMissile_OnCreate;
            Spellbook.OnUpdateChargeableSpell += Spellbook_OnUpdateChargedSpell;
            Chat.Print(ChampionName + " Loaded!");
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Base sender, EloBuddy.SDK.Events.Interrupter.InterruptableSpellEventArgs args)
        {
            if (!MiscMenu["InterruptSpells"].Cast<CheckBox>().CurrentValue)
                return;

            E.Cast(sender);
        }

        private static void Obj_SpellMissile_OnCreate(GameObject sender, EventArgs args)
        {
            if (!(sender is MissileClient))
                return;

            var missile = (MissileClient)sender;

            if (missile.SpellCaster != null && missile.SpellCaster.IsValid && missile.SpellCaster.IsMe &&
                missile.SData.Name.Equals("VelkozQMissile", StringComparison.InvariantCultureIgnoreCase))
            {
                QMissile = missile;
            }
        }

        static void Spellbook_OnUpdateChargedSpell(Spellbook sender, SpellbookUpdateChargeableSpellEventArgs args)
        {
            if (sender.Owner.IsMe)
            {
                args.Process =
                        !(Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) &&
                          ComboMenu["UseRCombo"].Cast<CheckBox>().CurrentValue);
            }
        }

        private static void Combo()
        {
            Orbwalker.DisableAttacking = Q.IsReady() || W.IsReady() || E.IsReady();

            UseSpells(ComboMenu["UseQCombo"].Cast<CheckBox>().CurrentValue, ComboMenu["UseWCombo"].Cast<CheckBox>().CurrentValue,
                ComboMenu["UseECombo"].Cast<CheckBox>().CurrentValue, ComboMenu["UseRCombo"].Cast<CheckBox>().CurrentValue,
                ComboMenu["UseIgniteCombo"].Cast<CheckBox>().CurrentValue);
        }

        private static void Harass()
        {
            UseSpells(HarassMenu["UseQHarass"].Cast<CheckBox>().CurrentValue, HarassMenu["UseWHarass"].Cast<CheckBox>().CurrentValue,
                HarassMenu["UseEHarass"].Cast<CheckBox>().CurrentValue, false, false);
        }

        private static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0d;

            if (Q.IsReady() && Q.GetPrediction(enemy).CollisionObjects.Length == 0)
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += W.Handle.Ammo *
                          Player.GetSpellDamage(enemy, SpellSlot.W);

            if (E.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            if (IgniteSlot != SpellSlot.Unknown && Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
                damage += Player.GetSummonerSpellDamage(enemy, DamageLibrary.SummonerSpells.Ignite);

            if (R.IsReady())
                damage += 7 * Player.GetSpellDamage(enemy, SpellSlot.R) / 10;

            return (float)damage;
        }

        private static void UseSpells(bool useQ, bool useW, bool useE, bool useR, bool useIgnite)
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            var qDummyTarget = TargetSelector.GetTarget(QDummy.Range, DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(R.Range, DamageType.Magical);
            
            if (useW && wTarget != null && W.IsReady())
            {
                W.Cast(wTarget);
                return;
            }

            if (useE && eTarget != null && E.IsReady())
            {
                E.Cast(eTarget);
                return;
            }

            if (useQ && qTarget != null && Q.IsReady() && !QToggleState)
            {
                if (QCast(qTarget))
                    return;
            }

            if (qDummyTarget != null && useQ && Q.IsReady() && !QToggleState)
            {
                if (qTarget != null) qDummyTarget = qTarget;
                QDummy.CastDelay = (int)(.25f + Q.Range / Q.Speed * 1000 + QSplit.Range / QSplit.Speed * 1000)*1000;

                var predictedPos = QDummy.GetPrediction(qDummyTarget);

                if (predictedPos.HitChance >= HitChance.High)
                {
                    for (var i = -1; i < 1; i = i + 2)
                    {
                        var alpha = 28 * (float)Math.PI / 180;
                        var cp = ObjectManager.Player.ServerPosition.To2D() +
                                 (predictedPos.CastPosition.To2D() - ObjectManager.Player.ServerPosition.To2D()).Rotated
                                     (i * alpha);
                        
                        if (GetCollision(Player.ServerPosition.To2D(), new List<Vector2> { cp },
                            Q.Width, Q.Speed, Q.CastDelay / 1000f).Count == 0 &&
                            GetCollision(cp, new List<Vector2> { predictedPos.CastPosition.To2D() },
                            QSplit.Width, QSplit.Speed, QSplit.CastDelay / 1000f).Count == 0)
                        {
                            QCast(cp.To3DWorld());
                            return;
                        }
                    }
                }
            }

            if (qTarget != null && useIgnite && IgniteSlot != SpellSlot.Unknown &&
                Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                if (Player.Distance(qTarget) < 650 && GetComboDamage(qTarget) > qTarget.Health)
                {
                    Player.Spellbook.CastSpell(IgniteSlot, qTarget);
                }
            }

            if (useR && rTarget != null && R.IsReady() &&
                Player.GetSpellDamage(rTarget, SpellSlot.R) / 10 * (Player.Distance(rTarget) < (R.Range - 500) ? 10 : 6) > rTarget.Health &&
                
                (LastCastedSpell.LastCastPacketSent.Slot != SpellSlot.R ||
                 Core.GameTickCount - LastCastedSpell.LastCastPacketSent.Tick > 350))
            {
                R.Cast(rTarget);
            }
        }
        public static  List<Obj_AI_Base> GetCollision(Vector2 from, List<Vector2> to, float width, float speed, float delay, float delayOverride = -1)
        {
            return Pradiction.Collision.GetCollision(
                to.Select(h => h.To3D()).ToList(),
                new Pradiction.PredictionInput
                {
                    From = from.To3D(),
                    Type = Pradiction.SkillshotType.SkillshotLine,
                    Radius = width,
                    Delay = delayOverride > 0 ? delayOverride : delay,
                    Speed = speed
                });
        }

        private static float QLastCastAttemptT;

        private static bool QCast()
        {
            QLastCastAttemptT = Core.GameTickCount;

            return Player.Spellbook.CastSpell(SpellSlot.Q);
        }

        private static bool QCast(Vector3 position)
        {
            QLastCastAttemptT = Core.GameTickCount;

            return Q.Cast(position);
        }

        private static bool QCast(Obj_AI_Base target)
        {
            QLastCastAttemptT = Core.GameTickCount;

            return Q.Cast(target);
        }

        private static void Farm()
        {
            if (!Orbwalker.CanMove)
                return;

            var rangedMinionsE =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition,
                    E.Range + E.Width).Where(x => x.IsRanged).ToList();

            var allMinionsW =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.ServerPosition,
                    W.Range).ToList();

            var useQ = FarmingMenu["UseQFarm"].Cast<CheckBox>().CurrentValue;
            var useW = FarmingMenu["UseWFarm"].Cast<CheckBox>().CurrentValue;
            var useE = FarmingMenu["UseEFarm"].Cast<CheckBox>().CurrentValue;


            if (useQ && allMinionsW.Count > 0 && !QToggleState && Q.IsReady())
            {
                Q.Cast(allMinionsW[0]);
            }

            if (useW && W.IsReady())
            {
                var wPos = W.GetBestLinearCastPosition(allMinionsW);
              
                if (wPos.HitNumber >= 3)
                    W.Cast(wPos.CastPosition);
            }

            if (useE && E.IsReady())
            {
                var ePos = E.GetBestCircularCastPosition(rangedMinionsE);
                if (ePos.HitNumber >= 3)
                    E.Cast(ePos.CastPosition);
            }
        }

        private static void JungleFarm()
        {
            var useQ = JungleClearMenu["UseQJFarm"].Cast<CheckBox>().CurrentValue;
            var useW = JungleClearMenu["UseWJFarm"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["UseEJFarm"].Cast<CheckBox>().CurrentValue;
            
            var mobs = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.ServerPosition, W.Range)
                .OrderByDescending(x => x.Health).ToList();

            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (useQ && !QToggleState && Q.IsReady())
                    QCast(mob);

                if (useW && W.IsReady())
                    W.Cast(mob);

                if (useE && E.IsReady())
                    E.Cast(mob);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Player.IsChannelingImportantSpell())
            {
                var endPoint = new Vector2();
                foreach (var obj in ObjectManager.Get<GameObject>())
                {
                    if (obj != null && obj.IsValid && obj.Name.Contains("Velkoz_") &&
                        obj.Name.Contains("_R_Beam_End"))
                    {
                        endPoint = Player.ServerPosition.To2D() +
                                   R.Range * (obj.Position - Player.ServerPosition).To2D().Normalized();
                        break;
                    }
                }

                if (endPoint.IsValid())
                {
                    var targets =
                        ObjectManager.Get<AIHeroClient>()
                            .Where(h => h.IsValidTarget(R.Range, true))
                            .Where(
                                enemy =>
                                {
                                    return enemy.ServerPosition.To2D().Distance(Player.ServerPosition.To2D(), endPoint, true, false) < 370;
                                }).Cast<Obj_AI_Base>()
                            .ToList();

                    if (targets.Count > 0)
                    {
                        var target = targets.OrderBy(t => t.Health / Player.GetSpellDamage(t, SpellSlot.Q)).ToList()[0];
                        EloBuddy.Player.UpdateChargeableSpell(SpellSlot.R, target.ServerPosition, false, false);
                    }
                    else
                    {
                        EloBuddy.Player.UpdateChargeableSpell(SpellSlot.R, Game.CursorPos, false, false);
                    }
                }

                return;
            }

            if (QMissile != null && QMissile.IsValid && QToggleState)
            {
                var qMissilePosition = QMissile.Position.To2D();
                var perpendicular = (QMissile.EndPosition - QMissile.StartPosition).To2D().Normalized().Perpendicular();

                var lineSegment1End = qMissilePosition + perpendicular * QSplit.Range;
                var lineSegment2End = qMissilePosition - perpendicular * QSplit.Range;

                var potentialTargets =
                    EntityManager.Heroes.Enemies.Where(
                        h =>
                            h.IsValidTarget() &&
                            h.ServerPosition.To2D().Distance(qMissilePosition, QMissile.EndPosition.To2D(), true, false) < 700)
                        .Cast<Obj_AI_Base>()
                        .ToList();

                QSplit.RangeCheckSource = qMissilePosition.To3DWorld();
                QSplit.SourcePosition = qMissilePosition.To3D();

                foreach (
                    var enemy in
                        EntityManager.Heroes.Enemies
                            .Where(
                                h =>
                                    h.IsValidTarget() &&
                                    (potentialTargets.Count == 0 ||
                                     h.NetworkId == potentialTargets.OrderBy(t => t.Health / Player.GetSpellDamage(t, SpellSlot.Q)).ToList()[0].NetworkId) &&
                                    (h.ServerPosition.To2D().Distance(qMissilePosition, QMissile.EndPosition.To2D(), true, false) > Q.Width + h.BoundingRadius)))
                {
                    var prediction = QSplit.GetPrediction(enemy);

                    var d1 = prediction.UnitPosition.To2D().Distance(qMissilePosition, lineSegment1End, true, false);
                    var d2 = prediction.UnitPosition.To2D().Distance(qMissilePosition, lineSegment2End, true, false);

                    if (prediction.HitChance >= HitChance.Medium && (d1 < QSplit.Width + enemy.BoundingRadius || d2 < QSplit.Width + enemy.BoundingRadius))
                    {
                        QCast();
                    }
                }
            }

            Orbwalker.DisableAttacking = false;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            else
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) || HarassMenu["HarassActiveT"].Cast<KeyBind>().CurrentValue)
                    Harass();

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                    Farm();

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                    JungleFarm();
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Q.IsReady() && DrawingsMenu["QRange"].Cast<CheckBox>().CurrentValue)
                Q.DrawRange(Color.DeepSkyBlue);
            if (W.IsReady() && DrawingsMenu["WRange"].Cast<CheckBox>().CurrentValue)
                W.DrawRange(Color.DeepSkyBlue);
            if (E.IsReady() && DrawingsMenu["ERange"].Cast<CheckBox>().CurrentValue)
                E.DrawRange(Color.DeepSkyBlue);
            if (R.IsReady() && DrawingsMenu["RRange"].Cast<CheckBox>().CurrentValue)
                R.DrawRange(Color.DeepSkyBlue);
        }
    }
}
