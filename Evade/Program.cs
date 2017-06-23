// Copyright 2014 - 2014 Esk0r
// Program.cs is part of Evade.
// 
// Evade is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Evade is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Evade. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Collections.Generic;
using System.Linq;
using Evade.Pathfinding;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using Color = System.Drawing.Color;
using GamePath = System.Collections.Generic.List<SharpDX.Vector2>;

#endregion

namespace Evade
{
    internal class Program
    {
        public static SpellList<Skillshot> DetectedSkillshots = new SpellList<Skillshot>();

        private static bool _evading;

        public static bool NoSolutionFound;

        public static Vector2 EvadeToPoint;

        public static int LastWardJumpAttempt;

        public static Vector2 PreviousTickPosition;
        public static Vector2 PlayerPosition;

        public static string PlayerChampionName;

        private static readonly Random RandomN = new Random();
        private static int _lastSentMovePacketT;
        private static int _lastSentMovePacketT2;

        public static int LastSMovePacketT { get; } = 0;

        private static bool _forcePathFollowing;
        public static bool Evading
        {
            get { return _evading; } //
            set
            {
                if (value)
                {
                    _forcePathFollowing = true;
                    _lastSentMovePacketT = 0;
                    ObjectManager.Player.SendMovePacket(EvadePoint);
                }

                _evading = value;
            }
        }

        public static Vector2 EvadePoint { get; set; }

        private static void Main()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            PlayerChampionName = ObjectManager.Player.ChampionName;

            //Create the menu to allow the user to change the config.
            Config.CreateMenu();

            //Add the game events.
            Game.OnUpdate += Game_OnOnGameUpdate;
            Player.OnIssueOrder += Player_OnIssueOrder;
            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
            //Set up the OnDetectSkillshot Event.
            SkillshotDetector.OnDetectSkillshot += OnDetectSkillshot;
            SkillshotDetector.OnDeleteMissile += SkillshotDetectorOnOnDeleteMissile;

            //For skillshot drawing.
            Drawing.OnDraw += Drawing_OnDraw;

            //Ondash event.
            Dash.OnDash += Dash_OnDash;

            Benchmarking.Benchmark.Initialize();

            DetectedSkillshots.OnAdd += DetectedSkillshots_OnAdd;

            //Initialze the collision
            Collision.Init();
        }


        private static void DetectedSkillshots_OnAdd(object sender, EventArgs e)
        {
            Evading = false;
        }

        private static void SkillshotDetectorOnOnDeleteMissile(Skillshot skillshot, MissileClient missile)
        {
            if (skillshot.SpellData.SpellName != "VelkozQ")
                return;

            var spellData = SpellDatabase.GetByName("VelkozQSplit");
            var direction = skillshot.Direction.Perpendicular();

            if (DetectedSkillshots.Count(s => s.SpellData.SpellName == "VelkozQSplit") != 0)
                return;

            for (var i = -1; i <= 1; i = i + 2)
            {
                var skillshotToAdd = new Skillshot(
                    DetectionType.ProcessSpell, spellData, Utils.TickCount, missile.Position.To2D(),
                    missile.Position.To2D() + i * direction * spellData.Range, skillshot.Unit);
                DetectedSkillshots.Add(skillshotToAdd);
            }
        }

        private static void OnDetectSkillshot(Skillshot skillshot)
        {
            //Check if the skillshot is already added.
            var alreadyAdded = false;

            if (Config.MiscMenu["DisableFow"].Cast<CheckBox>().CurrentValue && !skillshot.Unit.IsVisible)
            {
                return;
            }
                
            foreach (var item in DetectedSkillshots)
            {
                if ((item.SpellData.SpellName == skillshot.SpellData.SpellName) &&
                    ((item.Unit.NetworkId == skillshot.Unit.NetworkId) &&
                     ((skillshot.Direction).AngleBetween(item.Direction) < 5) &&
                     ((skillshot.Start.Distance(item.Start) < 100) || (skillshot.SpellData.FromObjects.Length == 0))))
                {
                    alreadyAdded = true;
                }
            }

            //Check if the skillshot is from an ally.
            if ((skillshot.Unit.Team == ObjectManager.Player.Team) && !Config.TestOnAllies)
            {
                return;
            }

            //Check if the skillshot is too far away.
            if (skillshot.Start.Distance(PlayerPosition) >
                (skillshot.SpellData.Range + skillshot.SpellData.Radius + 1000) * 1.5)
            {
                return;
            }

            //Add the skillshot to the detected skillshot list.
            if (alreadyAdded && !skillshot.SpellData.DontCheckForDuplicates)
                return;

            //Multiple skillshots like twisted fate Q.
            if (skillshot.DetectionType == DetectionType.ProcessSpell)
            {
                if (skillshot.SpellData.MultipleNumber != -1)
                {
                    var originalDirection = skillshot.Direction;

                    for (var i = -(skillshot.SpellData.MultipleNumber - 1) / 2;
                        i <= (skillshot.SpellData.MultipleNumber - 1) / 2;
                        i++)
                    {
                        var end = skillshot.Start +
                                  skillshot.SpellData.Range *
                                  originalDirection.Rotated(skillshot.SpellData.MultipleAngle * i);
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                            skillshot.Unit);

                        DetectedSkillshots.Add(skillshotToAdd);
                    }
                    return;
                }

                if (skillshot.SpellData.SpellName == "UFSlash")
                {
                    skillshot.SpellData.MissileSpeed = 1600 + (int) skillshot.Unit.MoveSpeed;
                }

                if (skillshot.SpellData.SpellName == "SionR")
                {
                    skillshot.SpellData.MissileSpeed = (int)skillshot.Unit.MoveSpeed;
                }

                if (skillshot.SpellData.Invert)
                {
                    var newDirection = -(skillshot.End - skillshot.Start).Normalized();
                    var end = skillshot.Start + newDirection * skillshot.Start.Distance(skillshot.End);
                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start, end,
                        skillshot.Unit);
                    DetectedSkillshots.Add(skillshotToAdd);
                    return;
                }

                if (skillshot.SpellData.Centered)
                {
                    var start = skillshot.Start - skillshot.Direction * skillshot.SpellData.Range;
                    var end = skillshot.Start + skillshot.Direction * skillshot.SpellData.Range;
                    var skillshotToAdd = new Skillshot(
                        skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                        skillshot.Unit);
                    DetectedSkillshots.Add(skillshotToAdd);
                    return;
                }

                var aiHeroClient = skillshot.Unit as AIHeroClient;
                if ((aiHeroClient != null) && ((skillshot.SpellData.SpellName == "TaricE") && (aiHeroClient.ChampionName == "Taric")))
                {
                    var target = EntityManager.Heroes.AllHeroes.FirstOrDefault(h => (h.Team == skillshot.Unit.Team) && h.IsVisible && h.HasBuff("taricwleashactive"));
                    if (target != null)
                    {
                        var start = target.ServerPosition.To2D();
                        var direction = (skillshot.OriginalEnd - start).Normalized();
                        var end = start + direction * skillshot.SpellData.Range;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick,
                            start, end, target)
                        {
                            OriginalEnd = skillshot.OriginalEnd
                        };
                        DetectedSkillshots.Add(skillshotToAdd);
                    }
                }

                switch (skillshot.SpellData.SpellName)
                {
                    case "SyndraE":
                    case "syndrae5":
                        const int angle = 60;
                        var edge1 =
                            (skillshot.End - skillshot.Unit.ServerPosition.To2D()).Rotated(
                                -angle / 2f * (float) Math.PI / 180);
                        var edge2 = edge1.Rotated(angle * (float) Math.PI / 180);

                        var positions = new List<Vector2>();

                        var explodingQ = DetectedSkillshots.FirstOrDefault(s => s.SpellData.SpellName == "SyndraQ");

                        if (explodingQ != null)
                        {
                            positions.Add(explodingQ.End);
                        }

                        positions.AddRange(from minion in ObjectManager.Get<Obj_AI_Minion>() where (minion.Name == "Seed") && !minion.IsDead && ((minion.Team != ObjectManager.Player.Team) || Config.TestOnAllies) select ((Vector3) minion.ServerPosition).To2D());

                        foreach (var position in positions)
                        {
                            var v = position - skillshot.Unit.ServerPosition.To2D();

                            if ((!(edge1.CrossProduct(v) > 0)) || (!(v.CrossProduct(edge2) > 0)) || (!(position.Distance(skillshot.Unit) < 800)))
                                continue;

                            var start = position;
                            var end = skillshot.Unit.ServerPosition.To2D()
                                .Extend(
                                    position,
                                    skillshot.Unit.Distance(position) > 200 ? 1300 : 1000);

                            var startTime = skillshot.StartTick;

                            startTime += (int)(150 + skillshot.Unit.Distance(position) / 2.5f);
                            var skillshotToAdd = new Skillshot(
                                skillshot.DetectionType, skillshot.SpellData, startTime, start, end,
                                skillshot.Unit);
                            DetectedSkillshots.Add(skillshotToAdd);
                        }
                        return;
                    case "MalzaharQ":
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 400;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 400;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }
                    case "ZyraQ":
                    {
                        var start = skillshot.End - skillshot.Direction.Perpendicular() * 450;
                        var end = skillshot.End + skillshot.Direction.Perpendicular() * 450;
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, start, end,
                            skillshot.Unit);
                        DetectedSkillshots.Add(skillshotToAdd);
                        return;
                    }
                    case "DianaArc":
                    {
                        var skillshotToAdd = new Skillshot(
                            skillshot.DetectionType, SpellDatabase.GetByName("DianaArcArc"), skillshot.StartTick, skillshot.Start, skillshot.End,
                            skillshot.Unit);

                        DetectedSkillshots.Add(skillshotToAdd);
                    }
                        break;
                }

                if (skillshot.SpellData.SpellName == "ZiggsQ")
                {
                    var d1 = skillshot.Start.Distance(skillshot.End);
                    var d2 = d1 * 0.4f;
                    var d3 = d2 * 0.69f;


                    var bounce1SpellData = SpellDatabase.GetByName("ZiggsQBounce1");
                    var bounce2SpellData = SpellDatabase.GetByName("ZiggsQBounce2");

                    var bounce1Pos = skillshot.End + skillshot.Direction * d2;
                    var bounce2Pos = bounce1Pos + skillshot.Direction * d3;

                    bounce1SpellData.Delay =
                        (int) (skillshot.SpellData.Delay + d1 * 1000f / skillshot.SpellData.MissileSpeed + 500);
                    bounce2SpellData.Delay =
                        (int) (bounce1SpellData.Delay + d2 * 1000f / bounce1SpellData.MissileSpeed + 500);

                    var bounce1 = new Skillshot(
                        skillshot.DetectionType, bounce1SpellData, skillshot.StartTick, skillshot.End, bounce1Pos,
                        skillshot.Unit);
                    var bounce2 = new Skillshot(
                        skillshot.DetectionType, bounce2SpellData, skillshot.StartTick, bounce1Pos, bounce2Pos,
                        skillshot.Unit);

                    DetectedSkillshots.Add(bounce1);
                    DetectedSkillshots.Add(bounce2);
                }

                if (skillshot.SpellData.SpellName.Equals("ZiggsR", StringComparison.CurrentCultureIgnoreCase))
                {
                    skillshot.SpellData.Delay =
                        (int) (1500 + 1500 * skillshot.End.Distance(skillshot.Start) / skillshot.SpellData.Range);
                }

                if (skillshot.SpellData.SpellName.Equals("JarvanIVDragonStrike", StringComparison.CurrentCultureIgnoreCase))
                {
                    var endPos = new Vector2();

                    foreach (var s in DetectedSkillshots)
                    {
                        if ((s.Unit.NetworkId != skillshot.Unit.NetworkId) || (s.SpellData.Slot != SpellSlot.E))
                            continue;

                        var extendedE = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start,
                            skillshot.End + skillshot.Direction * 100, skillshot.Unit);
                        if (!extendedE.IsSafe(s.End))
                        {
                            endPos = s.End;
                        }
                        break;
                    }

                    foreach (var m in ObjectManager.Get<Obj_AI_Minion>())
                    {
                        if ((!m.BaseSkinName.Equals("jarvanivstandard", StringComparison.CurrentCultureIgnoreCase)) || (m.Team != skillshot.Unit.Team))
                            continue;

                        var extendedE = new Skillshot(
                            skillshot.DetectionType, skillshot.SpellData, skillshot.StartTick, skillshot.Start,
                            skillshot.End + skillshot.Direction * 100, skillshot.Unit);
                        if (!extendedE.IsSafe(m.Position.To2D()))
                        {
                            endPos = m.Position.To2D();
                        }
                        break;
                    }

                    if (endPos.IsValid())
                    {
                        skillshot = new Skillshot(DetectionType.ProcessSpell, SpellDatabase.GetByName("JarvanIVEQ"), Utils.TickCount, skillshot.Start, endPos, skillshot.Unit);
                        skillshot.End = endPos + 200 * (endPos - skillshot.Start).Normalized();
                        skillshot.Direction = (skillshot.End - skillshot.Start).Normalized();
                    }
                }
            }

            if (skillshot.SpellData.SpellName == "OriannasQ")
            {
                var skillshotToAdd = new Skillshot(
                    skillshot.DetectionType, SpellDatabase.GetByName("OriannaQend"), skillshot.StartTick, skillshot.Start, skillshot.End,
                    skillshot.Unit);

                DetectedSkillshots.Add(skillshotToAdd);
            }


            //Dont allow fow detection.
            if (skillshot.SpellData.DisableFowDetection && (skillshot.DetectionType == DetectionType.RecvPacket))
            {
                return;
            }
#if DEBUG
                Console.WriteLine(Utils.TickCount + "Adding new skillshot: " + skillshot.SpellData.SpellName);
#endif

            DetectedSkillshots.Add(skillshot);
        }

        private static void Game_OnOnGameUpdate(EventArgs args)
        {
            PlayerPosition = ObjectManager.Player.ServerPosition.To2D();
            
            //Set evading to false after blinking
            if (PreviousTickPosition.IsValid() &&
                (PlayerPosition.Distance(PreviousTickPosition) > 200))
            {
                Evading = false;
                EvadeToPoint = Vector2.Zero;
            }

            PreviousTickPosition = PlayerPosition;
            
            //Remove the detected skillshots that have expired.
            DetectedSkillshots.RemoveAll(skillshot => !skillshot.IsActive());

            //Trigger OnGameUpdate on each skillshot.
            foreach (var skillshot in DetectedSkillshots)
            {
                skillshot.Game_OnGameUpdate();
            }

            //Evading disabled
            if (!Config.Menu["Enabled"].Cast<KeyBind>().CurrentValue)
            {
                Evading = false;
                return;
            }

            if ((PlayerChampionName == "Olaf") && Config.MiscMenu["DisableEvadeForOlafR"].Cast<CheckBox>().CurrentValue && ObjectManager.Player.HasBuff("OlafRagnarok"))
            {
                Evading = false;
                return;
            }

            //Avoid sending move/cast packets while dead.
            if (ObjectManager.Player.IsDead)
            {
                Evading = false;
                EvadeToPoint = Vector2.Zero;
                return;
            }

            ////Avoid sending move/cast packets while channeling interruptable spells that cause hero not being able to move.
            //if (ObjectManager.Player.IsCastingInterruptableSpell(true))
            //{
            //    Evading = false;
            //    EvadeToPoint = Vector2.Zero;
            //    return;
            //}
            
            if (IsWindingUp(ObjectManager.Player, false))
            {
                Evading = false;
                return;
            }

            /*Avoid evading while stunned or immobile.*/
            //if (Utils.ImmobileTime(ObjectManager.Player) - Utils.TickCount > Game.Ping / 2 + 70)
            //{
            //    Evading = false;
            //    return;
            //}

            /*Avoid evading while dashing.*/
            //if (ObjectManager.Player.IsDashing())
            //{
            //    Evading = false;
            //    return;
            //}

            //Don't evade while casting R as sion
            if ((PlayerChampionName == "Sion") && ObjectManager.Player.HasBuff("SionR"))
            {
                return;
            }

            //Shield allies.
            foreach (var ally in ObjectManager.Get<AIHeroClient>())
            {
                if (!ally.IsValidTarget(1000))
                    continue;

                var shieldAlly = Config.ShieldingMenu["shield" + ally.ChampionName];

                if ((shieldAlly == null) || !shieldAlly.Cast<CheckBox>().CurrentValue)
                    continue;

                var allySafeResult = IsSafe(ally.ServerPosition.To2D());

                if (allySafeResult.IsSafeB)
                    continue;

                var dangerLevel = allySafeResult.SkillshotList.Select(skillshot => Config.SkillShotsMenu["DangerLevel" + skillshot.SpellData.MenuItemName].Cast<Slider>().CurrentValue).Concat(new[] {0}).Max();

                foreach (var evadeSpell in EvadeSpellDatabase.Spells)
                {
                    if (evadeSpell.IsShield && evadeSpell.CanShieldAllies &&
                        (ally.Distance(ObjectManager.Player.ServerPosition) < evadeSpell.MaxRange) &&
                        (dangerLevel >= evadeSpell.DangerLevel) &&
                        (ObjectManager.Player.Spellbook.CanUseSpell(evadeSpell.Slot) == SpellState.Ready) &&
                        IsAboutToHit(ally, evadeSpell.Delay))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ally);
                    }
                }
            }

            //Spell Shielded
            if (ObjectManager.Player.IsSpellShielded())
            {
                return;
            }

            var currentPath = ObjectManager.Player.Path.To2DList();
            var safeResult = IsSafe(PlayerPosition);
            var safePath = IsSafePath(currentPath, 100);

            
            NoSolutionFound = false;
            
            //Continue evading
            if (Evading && IsSafe(EvadePoint).IsSafeB)
            {
                if (safeResult.IsSafeB)
                {
                    //We are safe, stop evading.
                    Evading = false;
                }
                else
                {
                    if (Utils.TickCount - _lastSentMovePacketT > 1000/15)
                    {
                        _lastSentMovePacketT = Utils.TickCount;
                        ObjectManager.Player.SendMovePacket(EvadePoint);
                    }
                    return;
                }
            }
                //Stop evading if the point is not safe.
            else if (Evading)
            {
                Evading = false;
            }

            //The path is not safe.
            if (!safePath.IsSafe)
            {
                //Inside the danger polygon.
                if (!safeResult.IsSafeB)
                {
                    //Search for an evade point:
                    TryToEvade(safeResult.SkillshotList, EvadeToPoint.IsValid() ? EvadeToPoint : Game.CursorPos.To2D());
                }
            }

            /*FOLLOWPATH*/
            if (!NoSolutionFound && !Evading && EvadeToPoint.IsValid() && safeResult.IsSafeB)
            {
                if (EvadeSpellDatabase.Spells.Any(evadeSpell => (evadeSpell.Name == "Walking") && evadeSpell.Enabled))
                {
                    if (safePath.IsSafe && !_forcePathFollowing)
                    {
                        return;
                    }

                    if ((Utils.TickCount - _lastSentMovePacketT2 > 1000 / 15) || !PathFollower.IsFollowing)
                    {
                        _lastSentMovePacketT2 = Utils.TickCount;
                        
                        if (DetectedSkillshots.Count == 0)
                        {
                            if (ObjectManager.Player.Distance(EvadeToPoint) > 75)
                            {
                                ObjectManager.Player.SendMovePacket(EvadeToPoint);
                            }
                            return;
                        }

                        var path2 = ObjectManager.Player.GetPath(EvadeToPoint.To3D()).To2DList();
                        var safePath2 = IsSafePath(path2, 100);

                        if (safePath2.IsSafe)
                        {
                            if (ObjectManager.Player.Distance(EvadeToPoint) > 75)
                            {
                                ObjectManager.Player.SendMovePacket(EvadeToPoint);
                            }
                            return;
                        }

                        var candidate = Pathfinding.Pathfinding.PathFind(PlayerPosition, EvadeToPoint);
                        
                        if (candidate.Count == 0)
                        {
                            if (!safePath.Intersection.Valid && (currentPath.Count <= 1))
                            {
                                safePath = IsSafePath(path2, 100);
                            }

                            if (safePath.Intersection.Valid)
                            {
                                if (ObjectManager.Player.Distance(safePath.Intersection.Point) > 75)
                                {
                                    ObjectManager.Player.SendMovePacket(safePath.Intersection.Point);
                                    return;
                                }
                            }
                        }

                        PathFollower.Follow(candidate);

                        PathFollower.KeepFollowingPath(new EventArgs());
                        
                    }
                }
            }
        }

        internal static bool IsWindingUp(Obj_AI_Base unit, bool inclueAutoAttacking = true)
        {
            if (!inclueAutoAttacking && unit.Spellbook.IsAutoAttacking)
                return false;

            return unit.Spellbook.CastEndTime - Game.Time > 0;
        }

        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            if (sender.Owner.IsValid && sender.Owner.IsMe)
            {
                if (args.Slot == SpellSlot.Recall)
                {
                    EvadeToPoint = new Vector2();
                }
                
                if (Evading)
                {
                    var blockLevel = Config.MiscMenu["BlockSpells"].Cast<ComboBox>().SelectedIndex;

                    if (blockLevel == 0)
                    {
                        return;
                    }

                    var isDangerous = false;
                    foreach (var skillshot in DetectedSkillshots)
                    {
                        if (skillshot.Evade() && skillshot.IsDanger(PlayerPosition))
                        {
                            isDangerous =
                                Config.SkillShotsMenu["IsDangerous" + skillshot.SpellData.MenuItemName].Cast<CheckBox>()
                                    .CurrentValue;

                            if (isDangerous)
                            {
                                break;
                            }
                        }
                    }

                    if ((blockLevel == 1) && !isDangerous)
                    {
                        return;
                    }

                    args.Process = !SpellBlocker.ShouldBlock(args.Slot);
                }
            }
        }

        private static void Player_OnIssueOrder(Obj_AI_Base sender, PlayerIssueOrderEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if ((args.Order == GameObjectOrder.MoveTo) || (args.Order == GameObjectOrder.AttackTo))
            {
                EvadeToPoint.X = args.TargetPosition.X;
                EvadeToPoint.Y = args.TargetPosition.Y;
            }
            else
            {
                EvadeToPoint = Vector2.Zero;
            }

            if (DetectedSkillshots.Count == 0)
            {
                _forcePathFollowing = false;
            }


            //Don't block the movement packets if cant find an evade point.
            if (NoSolutionFound)
            {
                return;
            }
            
            //Evading disabled
            if (!Config.Menu["Enabled"].Cast<KeyBind>().CurrentValue)
            {
                return;
            }

            if (EvadeSpellDatabase.Spells.Any(evadeSpell => (evadeSpell.Name == "Walking") && !evadeSpell.Enabled))
            {
                return;
            }

            //Spell Shielded
            if (ObjectManager.Player.IsSpellShielded())
            {
                return;
            }

            if ((PlayerChampionName == "Olaf") && Config.MiscMenu["DisableEvadeForOlafR"].Cast<CheckBox>().CurrentValue &&
                ObjectManager.Player.HasBuff("OlafRagnarok"))
            {
                return;
            }

            var myPath =
                ObjectManager.Player.GetPath(
                    new Vector3(args.TargetPosition.X, args.TargetPosition.Y, ObjectManager.Player.ServerPosition.Z))
                    .To2DList();
            var safeResult = IsSafe(PlayerPosition);


            //If we are evading:
            if (Evading || !safeResult.IsSafeB)
            {
                var rcSafePath = IsSafePath(myPath, Config.EvadingRouteChangeTimeOffset);
                if (args.Order == GameObjectOrder.MoveTo)
                {
                    var willMove = false;

                    if (Evading &&
                        (Utils.TickCount - Config.LastEvadePointChangeT > Config.EvadePointChangeInterval))
                    {
                        //Update the evade point to the closest one:
                        var points = Evader.GetEvadePoints(-1, 0, false, true);
                        if (points.Count > 0)
                        {
                            var to = new Vector2(args.TargetPosition.X, args.TargetPosition.Y);
                            EvadePoint = to.Closest(points);
                            Evading = true;
                            Config.LastEvadePointChangeT = Utils.TickCount;
                            willMove = true;
                        }
                    }

                    //If the path is safe let the user follow it.
                    if (rcSafePath.IsSafe && IsSafe(myPath[myPath.Count - 1]).IsSafeB &&
                        (args.Order == GameObjectOrder.MoveTo))
                    {
                        EvadePoint = myPath[myPath.Count - 1];
                        Evading = true;
                        willMove = true;
                    }

                    if (!willMove)
                    {
                        _forcePathFollowing = true;
                    }
                }

                //Block the packets if we are evading or not safe.
                args.Process = false;
                return;
            }

            var safePath = IsSafePath(myPath, Config.CrossingTimeOffset);

            //Not evading, outside the skillshots.
            //The path is not safe, stop in the intersection point.
            if (!safePath.IsSafe && (args.Order != GameObjectOrder.AttackUnit))
            {
                if (safePath.Intersection.Valid)
                {
                    if (ObjectManager.Player.Distance(safePath.Intersection.Point) > 75)
                    {
                        _forcePathFollowing = true;
                        //ObjectManager.Player.SendMovePacket(safePath.Intersection.Point);
                    }
                }

                _forcePathFollowing = true;
                args.Process = false;
            }

            //AutoAttacks.
            if (safePath.IsSafe || (args.Order != GameObjectOrder.AttackUnit))
                return;

            var target = args.Target;
            if ((target == null) || !target.IsValid || (target.GetType() != typeof(Obj_AI_Base)) || !target.IsVisible)
                return;

            //Out of attack range.
            if (PlayerPosition.Distance(((Obj_AI_Base) target).ServerPosition) <  ObjectManager.Player.AttackRange + ObjectManager.Player.BoundingRadius + target.BoundingRadius)
                return;

            if (safePath.Intersection.Valid)
            {
                ObjectManager.Player.SendMovePacket(safePath.Intersection.Point);
            }
            args.Process = false;
        }


        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            if (sender.IsMe)
            {
                if (Config.PrintSpellData)
                {
                    Console.WriteLine(
                        Utils.TickCount + "DASH: Speed: " + e.Speed + " Width:" +
                        e.EndPos.Distance(e.StartPos));
                }

                EvadeToPoint = e.EndPos.To2D();
                //Utility.DelayAction.Add(args.Duration, delegate { Evading = false; });
            }
        }
        /// <summary>
        /// Returns true if the point is not inside the detected skillshots.
        /// </summary>
        public static IsSafeResult IsSafe(Vector2 point)
        {
            var result = new IsSafeResult {SkillshotList = new List<Skillshot>()};

            foreach (var skillshot in DetectedSkillshots.Where(skillshot => skillshot.Evade() && skillshot.IsDanger(point)))
            {
                result.SkillshotList.Add(skillshot);
            }

            result.IsSafeB = result.SkillshotList.Count == 0;

            return result;
        }

        /// <summary>
        /// Returns if the unit will get hit by skillshots taking the path.
        /// </summary>
        public static SafePathResult IsSafePath(GamePath path,
            int timeOffset,
            int speed = -1,
            int delay = 0,
            Obj_AI_Base unit = null)
        {
            var isSafe = true;
            var intersections = new List<FoundIntersection>();
            var intersection = new FoundIntersection();

            foreach (var skillshot in DetectedSkillshots)
            {
                if (!skillshot.Evade())
                    continue;

                var sResult = skillshot.IsSafePath(path, timeOffset, speed, delay, unit);
                isSafe = isSafe && sResult.IsSafe;

                if (sResult.Intersection.Valid)
                {
                    intersections.Add(sResult.Intersection);
                }
            }

            //Return the first intersection
            if (isSafe) return new SafePathResult(true, intersection);
            var intersetion = intersections.MinOrDefault(o => o.Distance);
            return new SafePathResult(false, intersetion.Valid ? intersetion : intersection);
        }

        /// <summary>
        /// Returns if you can blink to the point without being hit.
        /// </summary>
        public static bool IsSafeToBlink(Vector2 point, int timeOffset, int delay)
        {
            return DetectedSkillshots.Where(skillshot => skillshot.Evade()).All(skillshot => skillshot.IsSafeToBlink(point, timeOffset, delay));
        }

        /// <summary>
        /// Returns true if some detected skillshot is about to hit the unit.
        /// </summary>
        public static bool IsAboutToHit(Obj_AI_Base unit, int time)
        {
            time += 150;
            return DetectedSkillshots.Where(skillshot => skillshot.Evade()).Any(skillshot => skillshot.IsAboutToHit(time, unit));
        }

        private static void TryToEvade(IEnumerable<Skillshot> hitBy, Vector2 to)
        {
            var dangerLevel = hitBy.Select(skillshot => Config.SkillShotsMenu["DangerLevel" + skillshot.SpellData.MenuItemName].Cast<Slider>().CurrentValue).Concat(new[] {0}).Max();

            foreach (var evadeSpell in EvadeSpellDatabase.Spells)
            {
                if (!evadeSpell.Enabled || (evadeSpell.DangerLevel > dangerLevel))
                    continue;

                //SpellShields
                if (evadeSpell.IsSpellShield &&
                    (ObjectManager.Player.Spellbook.CanUseSpell(evadeSpell.Slot) == SpellState.Ready))
                {
                    if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);
                    }
                    //Let the user move freely inside the skillshot.
                    NoSolutionFound = true;
                    return;
                }

                //Walking
                if (evadeSpell.Name == "Walking")
                {
                    var points = Evader.GetEvadePoints();
                    if (points.Count > 0)
                    {
                        EvadePoint = to.Closest(points);
                        var nEvadePoint = EvadePoint.Extend(PlayerPosition, -100);
                        if (IsSafePath(
                            ObjectManager.Player.GetPath(nEvadePoint.To3D()).To2DList(),
                            Config.EvadingSecondTimeOffset, (int) ObjectManager.Player.MoveSpeed, 100).IsSafe)
                        {
                            EvadePoint = nEvadePoint;
                        }

                        Evading = true;
                        return;
                    }
                }

                if (evadeSpell.IsReady())
                {
                    //MovementSpeed Buff
                    if (evadeSpell.IsMovementSpeedBuff)
                    {
                        var points = Evader.GetEvadePoints((int) evadeSpell.MoveSpeedTotalAmount());

                        if (points.Count > 0)
                        {
                            EvadePoint = to.Closest(points);
                            Evading = true;

                            if (evadeSpell.IsSummonerSpell)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(
                                    evadeSpell.Slot, ObjectManager.Player);
                            }
                            else
                            {
                                ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);
                            }

                            return;
                        }
                    }

                    //Dashes
                    if (evadeSpell.IsDash)
                    {
                        //Targetted dashes
                        if (evadeSpell.IsTargetted) //Lesinga W.
                        {
                            var targets = Evader.GetEvadeTargets(
                                evadeSpell.ValidTargets, evadeSpell.Speed, evadeSpell.Delay, evadeSpell.MaxRange);

                            if (targets.Count > 0)
                            {
                                var closestTarget = Utils.Closest(targets, to);
                                EvadePoint = closestTarget.ServerPosition.To2D();
                                Evading = true;

                                if (evadeSpell.IsSummonerSpell)
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                }
                                else
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                }

                                return;
                            }
                            if (Utils.TickCount - LastWardJumpAttempt < 250)
                            {
                                //Let the user move freely inside the skillshot.
                                NoSolutionFound = true;
                                return;
                            }

                            if (evadeSpell.IsTargetted &&
                                evadeSpell.ValidTargets.Contains(SpellValidTargets.AllyWards) &&
                                Config.EvadeSpellMenu["WardJump" + evadeSpell.Name].Cast<CheckBox>().CurrentValue)
                            {
                                var wardSlot = GetWardSlot();
                                if (wardSlot != null)
                                {
                                    var points = Evader.GetEvadePoints(evadeSpell.Speed, evadeSpell.Delay);

                                    // Remove the points out of range
                                    points.RemoveAll(
                                        item => item.Distance(ObjectManager.Player.ServerPosition) > 600);

                                    if (points.Count > 0)
                                    {
                                        //Dont dash just to the edge:
                                        for (var i = 0; i < points.Count; i++)
                                        {
                                            var k =
                                                (int)
                                                (600 -
                                                 PlayerPosition.Distance(points[i]));

                                            k = k - new Random(Utils.TickCount).Next(k);
                                            var extended = points[i] +
                                                           k *
                                                           (points[i] - PlayerPosition)
                                                               .Normalized();
                                            if (IsSafe(extended).IsSafeB)
                                            {
                                                points[i] = extended;
                                            }
                                        }

                                        var ePoint = to.Closest(points);
                                        ObjectManager.Player.Spellbook.CastSpell(wardSlot.SpellSlot, ePoint.To3D());
                                        LastWardJumpAttempt = Utils.TickCount;
                                        //Let the user move freely inside the skillshot.
                                        NoSolutionFound = true;
                                        return;
                                    }
                                }
                            }
                        }
                        //Skillshot type dashes.
                        else
                        {
                            var points = Evader.GetEvadePoints(evadeSpell.Speed, evadeSpell.Delay);

                            // Remove the points out of range
                            points.RemoveAll(
                                item => item.Distance(ObjectManager.Player.ServerPosition) > evadeSpell.MaxRange);

                            //If the spell has a fixed range (Vaynes Q), calculate the real dashing location. TODO: take into account walls in the future.
                            if (evadeSpell.FixedRange)
                            {
                                for (var i = 0; i < points.Count; i++)
                                {
                                    points[i] = PlayerPosition
                                        .Extend(points[i], evadeSpell.MaxRange);
                                }

                                for (var i = points.Count - 1; i > 0; i--)
                                {
                                    if (!IsSafe(points[i]).IsSafeB)
                                    {
                                        points.RemoveAt(i);
                                    }
                                }
                            }
                            else
                            {
                                for (var i = 0; i < points.Count; i++)
                                {
                                    var k =
                                        (int)
                                        (evadeSpell.MaxRange -
                                         PlayerPosition.Distance(points[i]));
                                    k -= Math.Max(RandomN.Next(k) - 100, 0);
                                    var extended = points[i] +
                                                   k *
                                                   (points[i] - PlayerPosition)
                                                       .Normalized();
                                    if (IsSafe(extended).IsSafeB)
                                    {
                                        points[i] = extended;
                                    }
                                }
                            }

                            if (points.Count > 0)
                            {
                                EvadePoint = to.Closest(points);
                                Evading = true;

                                if (!evadeSpell.Invert)
                                {
                                    if (evadeSpell.RequiresPreMove)
                                    {
                                        ObjectManager.Player.SendMovePacket(EvadePoint);
                                        var theSpell = evadeSpell;

                                        Core.DelayAction(delegate
                                        {
                                            ObjectManager.Player.Spellbook.CastSpell(
                                                theSpell.Slot, EvadePoint.To3D());
                                        }, Game.Ping / 2 + 100);
                                    }
                                    else
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, EvadePoint.To3D());
                                    }
                                }
                                else
                                {
                                    var castPoint = PlayerPosition -
                                                    (EvadePoint - PlayerPosition);
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, castPoint.To3D());
                                }

                                return;
                            }
                        }
                    }

                    //Blinks
                    if (evadeSpell.IsBlink)
                    {
                        //Targetted blinks
                        if (evadeSpell.IsTargetted)
                        {
                            var targets = Evader.GetEvadeTargets(
                                evadeSpell.ValidTargets, int.MaxValue, evadeSpell.Delay, evadeSpell.MaxRange, true);

                            if (targets.Count > 0)
                            {
                                if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                                {
                                    var closestTarget = Utils.Closest(targets, to);
                                    EvadePoint = closestTarget.ServerPosition.To2D();
                                    Evading = true;

                                    if (evadeSpell.IsSummonerSpell)
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(
                                            evadeSpell.Slot, closestTarget);
                                    }
                                    else
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                    }
                                }

                                //Let the user move freely inside the skillshot.
                                NoSolutionFound = true;
                                return;
                            }
                            if (Utils.TickCount - LastWardJumpAttempt < 250)
                            {
                                //Let the user move freely inside the skillshot.
                                NoSolutionFound = true;
                                return;
                            }

                            if (evadeSpell.IsTargetted &&
                                evadeSpell.ValidTargets.Contains(SpellValidTargets.AllyWards) &&
                                Config.EvadeSpellMenu["WardJump" + evadeSpell.Name].Cast<CheckBox>().CurrentValue)
                            {
                                var wardSlot = GetWardSlot();
                                if (wardSlot != null)
                                {
                                    var points = Evader.GetEvadePoints(int.MaxValue, evadeSpell.Delay, true);

                                    // Remove the points out of range
                                    points.RemoveAll(
                                        item => item.Distance(ObjectManager.Player.ServerPosition) > 600);

                                    if (points.Count > 0)
                                    {
                                        //Dont blink just to the edge:
                                        for (var i = 0; i < points.Count; i++)
                                        {
                                            var k =
                                                (int)
                                                (600 -
                                                 PlayerPosition.Distance(points[i]));

                                            k = k - new Random(Utils.TickCount).Next(k);
                                            var extended = points[i] +
                                                           k *
                                                           (points[i] - PlayerPosition)
                                                               .Normalized();
                                            if (IsSafe(extended).IsSafeB)
                                            {
                                                points[i] = extended;
                                            }
                                        }

                                        var ePoint = to.Closest(points);
                                        ObjectManager.Player.Spellbook.CastSpell(wardSlot.SpellSlot, ePoint.To3D());
                                        LastWardJumpAttempt = Utils.TickCount;
                                        //Let the user move freely inside the skillshot.
                                        NoSolutionFound = true;
                                        return;
                                    }
                                }
                            }
                        }

                        //Skillshot type blinks.
                        else
                        {
                            var points = Evader.GetEvadePoints(int.MaxValue, evadeSpell.Delay, true);

                            // Remove the points out of range
                            points.RemoveAll(
                                item => item.Distance(ObjectManager.Player.ServerPosition) > evadeSpell.MaxRange);


                            //Dont blink just to the edge:
                            for (var i = 0; i < points.Count; i++)
                            {
                                var k =
                                    (int)
                                    (evadeSpell.MaxRange -
                                     PlayerPosition.Distance(points[i]));

                                k = k - new Random(Utils.TickCount).Next(k);
                                var extended = points[i] +
                                               k *
                                               (points[i] - PlayerPosition).Normalized();
                                if (IsSafe(extended).IsSafeB)
                                {
                                    points[i] = extended;
                                }
                            }


                            if (points.Count > 0)
                            {
                                if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                                {
                                    EvadePoint = to.Closest(points);
                                    Evading = true;
                                    if (evadeSpell.IsSummonerSpell)
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(
                                            evadeSpell.Slot, EvadePoint.To3D());
                                    }
                                    else
                                    {
                                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, EvadePoint.To3D());
                                    }
                                }

                                //Let the user move freely inside the skillshot.
                                NoSolutionFound = true;
                                return;
                            }
                        }
                    }

                    //Invulnerabilities, like Fizz's E
                    if (evadeSpell.IsInvulnerability)
                    {
                        if (evadeSpell.IsTargetted)
                        {
                            var targets = Evader.GetEvadeTargets(
                                evadeSpell.ValidTargets, int.MaxValue, 0, evadeSpell.MaxRange, true, false, true);

                            if (targets.Count > 0)
                            {
                                if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                                {
                                    var closestTarget = Utils.Closest(targets, to);
                                    EvadePoint = closestTarget.ServerPosition.To2D();
                                    Evading = true;
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, closestTarget);
                                }

                                //Let the user move freely inside the skillshot.
                                NoSolutionFound = true;
                                return;
                            }
                        }
                        else
                        {
                            if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                            {
                                if (evadeSpell.SelfCast)
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot);
                                }
                                else
                                {
                                    ObjectManager.Player.Spellbook.CastSpell(
                                        evadeSpell.Slot, ObjectManager.Player.ServerPosition);
                                }
                            }
                        }


                        //Let the user move freely inside the skillshot.
                        NoSolutionFound = true;
                        return;
                    }
                }

                //Zhonyas
                if ((evadeSpell.Name == "Zhonyas") && (CanUseItem("ZhonyasHourglass")))
                {
                    if (IsAboutToHit(ObjectManager.Player, 100))
                    {
                        UseItem("ZhonyasHourglass");
                    }

                    //Let the user move freely inside the skillshot.
                    NoSolutionFound = true;

                    return;
                }

                //Shields
                if (evadeSpell.IsShield &&
                    (ObjectManager.Player.Spellbook.CanUseSpell(evadeSpell.Slot) == SpellState.Ready))
                {
                    if (IsAboutToHit(ObjectManager.Player, evadeSpell.Delay))
                    {
                        ObjectManager.Player.Spellbook.CastSpell(evadeSpell.Slot, ObjectManager.Player);
                    }

                    //Let the user move freely inside the skillshot.
                    NoSolutionFound = true;
                    return;
                }
            }

            NoSolutionFound = true;
        }

        /// <summary>
        ///     Returns true if the player has the item and its not on cooldown.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static bool CanUseItem(int id)
        {
            foreach (var slot in ObjectManager.Player.InventoryItems.Where(slot => slot.Id == (ItemId)id))
            {
                return ObjectManager.Player.Spellbook.CanUseSpell((SpellSlot)(slot.Slot + (int)SpellSlot.Item1))
                       == SpellState.Ready;
            }

            return false;
        }

        public static bool CanUseItem(string name)
        {
            foreach (var slot in ObjectManager.Player.InventoryItems.Where(slot => slot.Name == name))
            {
                return ObjectManager.Player.Spellbook.CanUseSpell((SpellSlot)(slot.Slot + (int)SpellSlot.Item1))
                       == SpellState.Ready;
            }

            return false;
        }

        public static bool UseItem(string name, Obj_AI_Base target = null)
        {
            foreach (var slot in ObjectManager.Player.InventoryItems.Where(slot => slot.Name == name))
            {
                if (target != null)
                {
                    return ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot, target);
                }
                else
                {
                    return ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot);
                }
            }

            return false;
        }

        /// <summary>
        ///     Casts the item on the target.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static bool UseItem(int id, Obj_AI_Base target = null)
        {
            foreach (var slot in ObjectManager.Player.InventoryItems.Where(slot => slot.Id == (ItemId)id))
            {
                if (target != null)
                {
                    return ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot, target);
                }
                else
                {
                    return ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot);
                }
            }

            return false;
        }

        /// <summary>
        ///     Casts the item on a Vector2 position.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public static bool UseItem(int id, Vector2 position)
        {
            return UseItem(id,  position.To3D());
        }

        /// <summary>
        ///     Casts the item on a Vector3 position.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public static bool UseItem(int id, Vector3 position)
        {
            if (position != Vector3.Zero)
            {
                foreach (var slot in ObjectManager.Player.InventoryItems.Where(slot => slot.Id == (ItemId)id))
                {
                    return ObjectManager.Player.Spellbook.CastSpell(slot.SpellSlot, position);
                }
            }

            return false;
        }


        /// <summary>
        ///     Returns the ward slot.
        /// </summary>
        /// <returns></returns>
        public static InventorySlot GetWardSlot()
        {
            var wardIds = new[]
                              {
                                  2045, 2049, 2050, 2301, 2302, 2303, 3340, 3361, 3362, 3711, 1408, 1409, 1410, 1411, 2043
                              };
            return (from wardId in wardIds
                    where CanUseItem(wardId)
                    select ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId))
                .FirstOrDefault();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Config.DrawingsMenu["EnableDrawings"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            
            if (Config.MiscMenu["ShowEvadeStatus"].Cast<CheckBox>().CurrentValue)
            {
                var heropos = Drawing.WorldToScreen(ObjectManager.Player.Position);

                if (Config.Menu["Enabled"].Cast<KeyBind>().CurrentValue)
                {
                    Drawing.DrawText(heropos.X, heropos.Y, Color.White, "Evade: ON");
                }
                else
                {
                    Drawing.DrawText(heropos.X, heropos.Y, Color.Red, "Evade: OFF");
                }
            }

            var missileColor = Color.GreenYellow;
            
            //Draw the polygon for each skillshot.
            foreach (var skillshot in DetectedSkillshots)
            {
                skillshot.Draw(
                    (skillshot.Evade() && Config.Menu["Enabled"].Cast<KeyBind>().CurrentValue)
                        ? Color.White
                        : Color.MediumVioletRed, missileColor, 2);
            }

            if (Config.TestOnAllies)
            {
                var myPath = ObjectManager.Player.Path.To2DList();

                for (var i = 0; i < myPath.Count - 1; i++)
                {
                    var A = myPath[i];
                    var B = myPath[i + 1];
                    var SA = Drawing.WorldToScreen(A.To3D());
                    var SB = Drawing.WorldToScreen(B.To3D());
                     Drawing.DrawLine(SA.X, SA.Y, SB.X, SB.Y, 1, Color.White);
                }

                var evadePath = Pathfinding.Pathfinding.PathFind(PlayerPosition, Game.CursorPos.To2D());

                for (var i = 0; i < evadePath.Count - 1; i++)
                {
                    var A = evadePath[i];
                    var B = evadePath[i + 1];
                    var SA = Drawing.WorldToScreen(A.To3D());
                    var SB = Drawing.WorldToScreen(B.To3D());
                    Drawing.DrawLine(SA.X, SA.Y, SB.X, SB.Y, 1, Color.Red);
                }



                Drawing.DrawCircle(EvadePoint.To3D(), 300, Color.White);
                Drawing.DrawCircle(EvadeToPoint.To3D(), 300, Color.Red);
            }
        }

        public struct IsSafeResult
        {
            public bool IsSafeB;
            public List<Skillshot> SkillshotList;
        }
    }
}
