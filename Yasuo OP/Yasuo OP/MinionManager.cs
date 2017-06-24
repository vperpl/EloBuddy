using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using static Yasuo_OP.Helper;
using static Yasuo_OP.SpellsManager;

namespace Yasuo_OP
{
    public static class MinionManager
    {
        public static Obj_AI_Base GetBestEnemy(this Obj_AI_Base target)
        {
            return
                ObjectManager.Get<Obj_AI_Base>()
                    .Where(m => m.IsEnemy && m.IsValidTarget())
                    .OrderBy(m => m.Distance(target))
                    .ThenByDescending(m => m.Distance(Me))
                    .ThenByDescending(m => m.Distance(Game.CursorPos))
                    .FirstOrDefault(m => m.IsValidTarget(E.Range) && !m.HasEBuff());
        }
    }
}
