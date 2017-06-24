using EloBuddy;
using static Yasuo_OP.Helper;
using static Yasuo_OP.SpellsManager;

namespace Yasuo_OP
{
    internal class Evader
    {
        public static void Load()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var hero = sender as AIHeroClient;
            if(hero == null || hero.IsAlly)return;

            if (args.Target == Me)
            {
                W.Cast(args.Start);
            }
        }
    }
}
