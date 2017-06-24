using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using SharpDX;

namespace Yasuo_OP
{
    public static class DashManager
    {
        private static int startTime;
        private static int endTime;
        private static Vector3 startPosition;
        private static Vector3 endPosition;

        public static void Load()
        {
            Dash.OnDash += Dash_OnDash;
        }

        private static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            var hero = sender as AIHeroClient;
            if (hero == null || !hero.IsMe) return;

            startTime = e.StartTick;
            endTime = e.EndTick;
            startPosition = e.StartPos;
            endPosition = e.EndPos;
        }

        public static Vector3 GetPlayerPosition(int time = 0)
        {
            if (Player.Instance.IsDashing() && endTime < Environment.TickCount + time)
            {
                return
                    startPosition.Extend(endPosition,
                        475*
                        ((endTime - (Environment.TickCount + time))/
                         ((startTime - endTime) == 0 ? 1 : startTime - endTime))).To3D();
            }
            return Player.Instance.Position;
        }

        public static Vector3 GetPosAfterE(this Obj_AI_Base target)
        {
            return Player.Instance.Position.Extend(Prediction.Position.PredictUnitPosition(target, 250), 475).To3D();
        }
    }
}
