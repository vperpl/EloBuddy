using System;
using EloBuddy.SDK.Events;

namespace Yasuo_OP
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            try
            {
                SpellsManager.Load();
                ModeManager.Load();
                DashManager.Load();
                Evader.Load();
                MenuOP.Load();
                DrawManager.Load();
            }
            catch (Exception exp)
            {
                Console.Write(exp);
            }
        }
    }
}
