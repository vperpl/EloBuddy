// Copyright 2014 - 2014 Esk0r
// Collision.cs is part of Evade.
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
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using SharpDX;

#endregion

namespace Evade
{
    public enum CollisionObjectTypes
    {
        Minion,
        Champions,
        YasuoWall,
    }

    internal class FastPredResult
    {
        public Vector2 CurrentPos;
        public bool IsMoving;
        public Vector2 PredictedPos;
    }

    internal class DetectedCollision
    {
        public float Diff;
        public float Distance;
        public Vector2 Position;
        public CollisionObjectTypes Type;
        public Obj_AI_Base Unit;
    }

    internal static class Collision
    {
        private static int _wallCastT;
        private static Vector2 _yasuoWallCastedPos;

        public static void Init()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }


        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsValid || sender.Team != ObjectManager.Player.Team || args.SData.Name != "YasuoWMovingWall")
                return;

            _wallCastT = Utils.TickCount;
            _yasuoWallCastedPos = sender.ServerPosition.To2D();
        }

        public static FastPredResult FastPrediction(Vector2 from, Obj_AI_Base unit, int delay, int speed)
        {
            var tDelay = delay / 1000f + (from.Distance(unit) / speed);
            var d = tDelay * unit.MoveSpeed;
            var path = unit.Path.To2DList();
            if (path.PathLength() > d)
            {
                return new FastPredResult
                {
                    IsMoving = true,
                    CurrentPos = unit.ServerPosition.To2D(),
                    PredictedPos = path.CutPath((int) d)[0],
                };
            }
            return new FastPredResult
            {
                IsMoving = false,
                CurrentPos = path[path.Count - 1],
                PredictedPos = path[path.Count - 1],
            };
        }


        public static List<Vector2> CutPath(this List<Vector2> path, float distance)
        {
            var result = new List<Vector2>();
            var Distance = distance;
            if (distance < 0)
            {
                path[0] = path[0] + distance * (path[1] - path[0]).Normalized();
                return path;
            }

            for (var i = 0; i < path.Count - 1; i++)
            {
                var dist = path[i].Distance(path[i + 1]);
                if (dist > Distance)
                {
                    result.Add(path[i] + Distance * (path[i + 1] - path[i]).Normalized());
                    for (var j = i + 1; j < path.Count; j++)
                    {
                        result.Add(path[j]);
                    }

                    break;
                }
                Distance -= dist;
            }
            return result.Count > 0 ? result : new List<Vector2> { path.Last() };
        }
        /*

        /// <summary>
        ///     Returns the path of the unit appending the ServerPosition at the start, works even if the unit just entered fow.
        /// </summary>
        public static List<Vector2> GetWaypoints(this Obj_AI_Base unit)
        {
            var result = new List<Vector2>();

            if (unit.IsVisible)
            {
                result.Add(unit.ServerPosition.To2D());
                var path = unit.Path;
                if (path.Length > 0)
                {
                    var first = path[0].To2D();
                    if (first.Distance(result[0], true) > 40)
                    {
                        result.Add(first);
                    }

                    for (var i = 1; i < path.Length; i++)
                    {
                        result.Add(path[i].To2D());
                    }
                }
            }
            else if (WaypointTracker.StoredPaths.ContainsKey(unit.NetworkId))
            {
                var path = WaypointTracker.StoredPaths[unit.NetworkId];
                var timePassed = (Utils.TickCount - WaypointTracker.StoredTick[unit.NetworkId]) / 1000f;
                if (path.PathLength() >= unit.MoveSpeed * timePassed)
                {
                    result = CutPath(path, (int)(unit.MoveSpeed * timePassed));
                }
            }

            return result;
        }
        */

        /// <summary>
        ///     Internal class used to get the waypoints even when the enemy enters the fow of war.
        /// </summary>
        internal static class WaypointTracker
        {
            #region Static Fields

            public static readonly Dictionary<int, List<Vector2>> StoredPaths = new Dictionary<int, List<Vector2>>();

            public static readonly Dictionary<int, int> StoredTick = new Dictionary<int, int>();

            #endregion
        }

        public static Vector2 GetCollisionPoint(Skillshot skillshot)
        {
            var collisions = new List<DetectedCollision>();
            var from = skillshot.GetMissilePosition(0);
            skillshot.ForceDisabled = false;
            foreach (var cObject in skillshot.SpellData.CollisionObjects)
            {
                switch (cObject)
                {
                    case CollisionObjectTypes.Minion:

                        if (!Config.CollisionMenu["MinionCollision"].Cast<CheckBox>().CurrentValue)
                        {
                            break;
                        }
                        foreach (var minion in
                            EntityManager.MinionsAndMonsters.CombinedAttackable.Where(x => x.Distance(from) < 1200 && skillshot.Unit.Team == ObjectManager.Player.Team
                                    ? x.IsEnemy
                                    : x.IsAlly))
                        {
                            var pred = FastPrediction(
                                from, minion,
                                Math.Max(0, skillshot.SpellData.Delay - (Utils.TickCount - skillshot.StartTick)),
                                skillshot.SpellData.MissileSpeed);
                            var pos = pred.PredictedPos;
                            var w = skillshot.SpellData.RawRadius + (!pred.IsMoving ? (minion.BoundingRadius - 15) : 0) -
                                    pos.Distance(from, skillshot.End, true);
                            if (w > 0)
                            {
                                collisions.Add(
                                    new DetectedCollision
                                    {
                                        Position =
                                            pos.ProjectOn(skillshot.End, skillshot.Start).LinePoint +
                                            skillshot.Direction * 30,
                                        Unit = minion,
                                        Type = CollisionObjectTypes.Minion,
                                        Distance = pos.Distance(from),
                                        Diff = w,
                                    });
                            }
                        }

                        break;

                    case CollisionObjectTypes.Champions:
                        if (!Config.CollisionMenu["HeroCollision"].Cast<CheckBox>().CurrentValue)
                        {
                            break;
                        }
                        foreach (var hero in
                            ObjectManager.Get<AIHeroClient>()
                                .Where(
                                    h =>
                                        (h.IsValidTarget(1200, false) && h.Team == ObjectManager.Player.Team && !h.IsMe ||
                                         Config.TestOnAllies && h.Team != ObjectManager.Player.Team)))
                        {
                            var pred = FastPrediction(
                                from, hero,
                                Math.Max(0, skillshot.SpellData.Delay - (Utils.TickCount - skillshot.StartTick)),
                                skillshot.SpellData.MissileSpeed);
                            var pos = pred.PredictedPos;

                            var w = skillshot.SpellData.RawRadius + 30 - pos.Distance(from, skillshot.End, true);
                            if (w > 0)
                            {
                                collisions.Add(
                                    new DetectedCollision
                                    {
                                        Position =
                                            pos.ProjectOn(skillshot.End, skillshot.Start).LinePoint +
                                            skillshot.Direction * 30,
                                        Unit = hero,
                                        Type = CollisionObjectTypes.Minion,
                                        Distance = pos.Distance(from),
                                        Diff = w,
                                    });
                            }
                        }
                        break;

                    case CollisionObjectTypes.YasuoWall:
                        if (!Config.CollisionMenu["YasuoCollision"].Cast<CheckBox>().CurrentValue)
                        {
                            break;
                        }
                        if (
                            !ObjectManager.Get<AIHeroClient>()
                                .Any(
                                    hero =>
                                        hero.IsValidTarget(float.MaxValue, false) &&
                                        hero.Team == ObjectManager.Player.Team && hero.ChampionName == "Yasuo"))
                        {
                            break;
                        }
                        GameObject wall = null;
                        foreach (var gameObject in ObjectManager.Get<GameObject>())
                        {
                            if (gameObject.IsValid &&
                                System.Text.RegularExpressions.Regex.IsMatch(
                                    gameObject.Name, "_w_windwall.\\.troy",
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                wall = gameObject;
                            }
                        }
                        if (wall == null)
                        {
                            break;
                        }
                        var level = wall.Name.Substring(wall.Name.Length - 6, 1);
                        var wallWidth = (300 + 50 * Convert.ToInt32(level));


                        var wallDirection = (wall.Position.To2D() - _yasuoWallCastedPos).Normalized().Perpendicular();
                        var wallStart = wall.Position.To2D() + wallWidth / 2 * wallDirection;
                        var wallEnd = wallStart - wallWidth * wallDirection;
                        var wallPolygon = new Geometry.Rectangle(wallStart, wallEnd, 75).ToPolygon();
                        var intersection = new Vector2();
                        var intersections = new List<Vector2>();

                        for (var i = 0; i < wallPolygon.Points.Count; i++)
                        {
                            var inter =
                                wallPolygon.Points[i].Intersection(
                                    wallPolygon.Points[i != wallPolygon.Points.Count - 1 ? i + 1 : 0], from,
                                    skillshot.End);
                            if (inter.Intersects)
                            {
                                intersections.Add(inter.Point);
                            }
                        }

                        if (intersections.Count > 0)
                        {
                            intersection = intersections.OrderBy(item => item.Distance(from)).ToList()[0];
                            var collisionT = Utils.TickCount +
                                             Math.Max(
                                                 0,
                                                 skillshot.SpellData.Delay -
                                                 (Utils.TickCount - skillshot.StartTick)) + 100 +
                                             (1000 * intersection.Distance(from)) / skillshot.SpellData.MissileSpeed;
                            if (collisionT - _wallCastT < 4000)
                            {
                                if (skillshot.SpellData.Type != SkillShotType.SkillshotMissileLine)
                                {
                                    skillshot.ForceDisabled = true;
                                }
                                return intersection;
                            }
                        }

                        break;
                }
            }

            Vector2 result;
            if (collisions.Count > 0)
            {
                result = collisions.OrderBy(c => c.Distance).ToList()[0].Position;
            }
            else
            {
                result = new Vector2();
            }

            return result;
        }
    }
}