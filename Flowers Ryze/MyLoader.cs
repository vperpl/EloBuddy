namespace Flowers_Ryze
{
    #region 

    using EloBuddy;
    using EloBuddy.SDK.Events;

    #endregion

    internal class MyLoader
    {
        public static void Main()
        {
            Loading.OnLoadingComplete += Args =>
            {
                if (ObjectManager.Player.Hero != Champion.Ryze)
                {
                    return;
                }

                var RyzeLoader = new MyBase.MyChampions();
            };
        }
    }
}
