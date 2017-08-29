namespace Flowers_Ryze.MyCommon
{
    #region 

    using EloBuddy;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    using System;
    using System.Drawing;

    #endregion

    internal class MyManaManager
    {
        internal static bool SpellFarm { get; set; } = true;
        internal static bool SpellHarass { get; set; } = true;

        internal static void AddFarmToMenu(Menu mainMenu)
        {
            try
            {
                if (mainMenu != null)
                {
                    mainMenu.AddGroupLabel("-- Spell Farm Logic");
                    mainMenu.Add("MyManaManager.SpellFarm", new CheckBox("Use Spell To Farm(Mouse Scrool)"));
                    mainMenu.Add("MyManaManager.SpellHarass",
                        new KeyBind("Use Spell To Harass(In Clear Mode)", true, KeyBind.BindTypes.PressToggle, 'H'));

                    Game.OnWndProc += delegate (WndEventArgs Args)
                    {
                        try
                        {
                            if (Args.Msg == 0x20a)
                            {
                                mainMenu["MyManaManager.SpellFarm"].Cast<CheckBox>().CurrentValue =
                                    !mainMenu["MyManaManager.SpellFarm"].Cast<CheckBox>().CurrentValue;
                                SpellFarm = mainMenu["MyManaManager.SpellFarm"].Cast<CheckBox>().CurrentValue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error in MyManaManager.OnWndProcEvent." + ex);
                        }
                    };

                    Game.OnTick += delegate
                    {
                        SpellFarm = mainMenu["MyManaManager.SpellFarm"].Cast<CheckBox>().CurrentValue;
                        SpellHarass = mainMenu["MyManaManager.SpellHarass"].Cast<KeyBind>().CurrentValue;
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyManaManager.AddFarmToMenu." + ex);
            }
        }

        internal static void AddDrawToMenu(Menu mainMenu)
        {
            try
            {
                if (mainMenu != null)
                {
                    mainMenu.AddGroupLabel("-- Spell Farm Logic");
                    mainMenu.Add("MyManaManager.DrawSpelFarm", new CheckBox("Draw Spell Farm Status"));
                    mainMenu.Add("MyManaManager.DrawSpellHarass", new CheckBox("Draw Soell Harass Status"));

                    Drawing.OnDraw += delegate
                    {
                        try
                        {
                            if (ObjectManager.Player.IsDead)
                            {
                                return;
                            }

                            if (mainMenu["MyManaManager.DrawSpelFarm"].Cast<CheckBox>().CurrentValue)
                            {
                                var MePos = Drawing.WorldToScreen(ObjectManager.Player.Position);

                                Drawing.DrawText(MePos[0] - 57, MePos[1] + 48, Color.FromArgb(242, 120, 34),
                                    "Spell Farm:" + (SpellFarm ? "On" : "Off"));
                            }

                            if (mainMenu["MyManaManager.DrawSpellHarass"].Cast<CheckBox>().CurrentValue)
                            {
                                var MePos = Drawing.WorldToScreen(ObjectManager.Player.Position);

                                Drawing.DrawText(MePos[0] - 57, MePos[1] + 68, Color.FromArgb(242, 120, 34),
                                    "Spell Harass:" + (SpellHarass ? "On" : "Off"));
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error in MyManaManager.OnRender." + ex);
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyManaManager.AddDrawToMenu." + ex);
            }
        }
    }
}