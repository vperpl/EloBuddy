namespace Flowers_Ryze.MyCommon
{
    #region 

    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;

    using Flowers_Ryze.MyBase;

    using System;

    #endregion

    internal class MyMenuManager
    {
        internal static void Initializer()
        {
            try
            {
                MyLogic.Menu = MainMenu.AddMenu("Flowers Ryze", "Flowers Ryze");
                {
                    MyLogic.Menu.AddGroupLabel("Made by NightMoon");
                    MyLogic.Menu.AddSeparator();
                    MyLogic.Menu.AddGroupLabel("For Yukki Request");
                }

                MyLogic.ComboMenu = MyLogic.Menu.AddSubMenu(":: Combo Settings", "FlowersRyze.ComboMenu");
                {
                    MyLogic.ComboMenu.AddGroupLabel("-- Q Settings");
                    MyLogic.ComboMenu.Add("FlowersRyze.ComboMenu.Q", new CheckBox("Use Q"));
                    MyLogic.ComboMenu.Add("FlowersRyze.ComboMenu.QSmart", new CheckBox("Use Q| Smart Ignore Collsion"));

                    MyLogic.ComboMenu.AddGroupLabel("-- W Settings");
                    MyLogic.ComboMenu.Add("FlowersRyze.ComboMenu.W", new CheckBox("Use W"));

                    MyLogic.ComboMenu.AddGroupLabel("-- E Settings");
                    MyLogic.ComboMenu.Add("FlowersRyze.ComboMenu.E", new CheckBox("Use E"));

                    MyLogic.ComboMenu.AddGroupLabel("-- Mode Settings");
                    MyLogic.ComboMenu.Add("FlowersRyze.ComboMenu.Mode", new ComboBox(
                        "Combo Mode: ", new[] {"Smart", "Shield", "Burst"}));
                    MyLogic.ComboMenu.Add("FlowersRyze.ComboMenu.ModeKey", new KeyBind(
                        "Switch Mode Key: ", false, KeyBind.BindTypes.HoldActive, 'G')).OnValueChange +=
                        delegate (ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs Args)
                        {
                            if (Args.NewValue)
                            {
                                switch (MyLogic.ComboMenu["FlowersRyze.ComboMenu.Mode"].Cast<ComboBox>().CurrentValue)
                                {
                                    case 0:
                                        MyLogic.ComboMenu["FlowersRyze.ComboMenu.Mode"].Cast<ComboBox>().CurrentValue = 1;
                                        break;
                                    case 1:
                                        MyLogic.ComboMenu["FlowersRyze.ComboMenu.Mode"].Cast<ComboBox>().CurrentValue = 2;
                                        break;
                                    case 2:
                                        MyLogic.ComboMenu["FlowersRyze.ComboMenu.Mode"].Cast<ComboBox>().CurrentValue = 0;
                                        break;
                                }
                            }
                        };
                    MyLogic.ComboMenu.Add("FlowersRyze.ComboMenu.ShieldHP", new Slider(
                        "Smart Mode Auto Shield| When Player HealthPercent <= x%", 60));

                    MyLogic.ComboMenu.AddGroupLabel("-- Other Settings");
                    MyLogic.ComboMenu.Add("FlowersRyze.ComboMenu.Ignite", new CheckBox("Use Ignite"));
                    MyLogic.ComboMenu.Add("FlowersRyze.ComboMenu.DisableAttack", new ComboBox(
                        "Disable Attack in Combo: ", new[] {"Smart Mode", "Always", "No"}));
                }

                MyLogic.HarassMenu = MyLogic.Menu.AddSubMenu(":: Harass Settings", "FlowersRyze.HarassMenu");
                {
                    MyLogic.HarassMenu.AddGroupLabel("-- Q Settings");
                    MyLogic.HarassMenu.Add("FlowersRyze.HarassMenu.Q", new CheckBox("Use Q"));

                    MyLogic.HarassMenu.AddGroupLabel("-- W Settings");
                    MyLogic.HarassMenu.Add("FlowersRyze.HarassMenu.W", new CheckBox("Use W", false));

                    MyLogic.HarassMenu.AddGroupLabel("-- E Settings");
                    MyLogic.HarassMenu.Add("FlowersRyze.HarassMenu.E", new CheckBox("Use E"));

                    MyLogic.HarassMenu.AddGroupLabel("-- Mana Settings");
                    MyLogic.HarassMenu.Add("FlowersRyze.HarassMenu.Mana", new Slider(
                        "When Player ManaPercent >= x%", 60, 1, 99));
                }

                MyLogic.ClearMenu = MyLogic.Menu.AddSubMenu(":: Clear Settings", "FlowersRyze.ClearMenu");
                {
                    MyLogic.ClearMenu.AddGroupLabel("-- LaneClear Settings");
                    MyLogic.ClearMenu.Add("FlowersRyze.ClearMenu.LaneClearQ", new CheckBox("Use Q"));
                    MyLogic.ClearMenu.Add("FlowersRyze.ClearMenu.LaneClearW", new CheckBox("Use W"));
                    MyLogic.ClearMenu.Add("FlowersRyze.ClearMenu.LaneClearE", new CheckBox("Use E"));
                    MyLogic.ClearMenu.Add("FlowersRyze.ClearMenu.LaneClearMana", new Slider(
                        "When Player ManaPercent >= x%", 30, 1, 99));

                    MyLogic.ClearMenu.AddGroupLabel("-- JungleClear Settings");
                    MyLogic.ClearMenu.Add("FlowersRyze.ClearMenu.JungleClearQ", new CheckBox("Use Q"));
                    MyLogic.ClearMenu.Add("FlowersRyze.ClearMenu.JungleClearW", new CheckBox("Use W"));
                    MyLogic.ClearMenu.Add("FlowersRyze.ClearMenu.JungleClearE", new CheckBox("Use E"));
                    MyLogic.ClearMenu.Add("FlowersRyze.ClearMenu.JungleClearMana", new Slider(
                        "When Player ManaPercent >= x%", 30, 1, 99));
                }

                MyLogic.LastHitMenu = MyLogic.Menu.AddSubMenu(":: LastHit Settings", "FlowersRyze.LastHitMenu");
                {
                    MyLogic.LastHitMenu.AddGroupLabel( "-- Q Settings");
                    MyLogic.LastHitMenu.Add("FlowersRyze.LastHitMenu.LastHitQ", new CheckBox("Use Q"));
                    MyLogic.LastHitMenu.Add("FlowersRyze.LastHitMenu.LastHitMana", new Slider(
                        "When Player ManaPercent >= x%", 30, 1, 99));
                }

                MyLogic.KillStealMenu = MyLogic.Menu.AddSubMenu(":: KillSteal Settings", "FlowersRyze.KillStealMenu");
                {
                    MyLogic.KillStealMenu.AddGroupLabel("-- Q Settings");
                    MyLogic.KillStealMenu.Add("FlowersRyze.KillStealMenu.Q", new CheckBox("Use Q"));

                    MyLogic.KillStealMenu.AddGroupLabel("-- W Settings");
                    MyLogic.KillStealMenu.Add("FlowersRyze.KillStealMenu.W", new CheckBox("Use W"));

                    MyLogic.KillStealMenu.AddGroupLabel("-- E Settings");
                    MyLogic.KillStealMenu.Add("FlowersRyze.KillStealMenu.E", new CheckBox("Use E"));
                }

                MyLogic.MiscMenu = MyLogic.Menu.AddSubMenu(":: Misc Settings", "FlowersRyze.MiscMenu");
                {
                    MyManaManager.AddFarmToMenu(MyLogic.MiscMenu);
                    MyLogic.MiscMenu.AddGroupLabel("-- W Settings");
                    MyLogic.MiscMenu.Add("FlowersRyze.MiscMenu.WMelee", new CheckBox("Auto W| Anti Melee"));
                    MyLogic.MiscMenu.Add("FlowersRyze.MiscMenu.WRengar", new CheckBox("Auto W| Anti Rengar"));
                    MyLogic.MiscMenu.Add("FlowersRyze.MiscMenu.WKhazix", new CheckBox("Auto W| Anti Khazix"));
                }

                MyLogic.DrawMenu = MyLogic.Menu.AddSubMenu(":: Draw Settings", "FlowersRyze.DrawMenu");
                {
                    MyLogic.DrawMenu.AddGroupLabel("-- Spell Range");
                    MyLogic.DrawMenu.Add("FlowersRyze.DrawMenu.Q", new CheckBox("Draw Q Range", false));
                    MyLogic.DrawMenu.Add("FlowersRyze.DrawMenu.W", new CheckBox("Draw W Range", false));
                    MyLogic.DrawMenu.Add("FlowersRyze.DrawMenu.E", new CheckBox("Draw E Range", false));
                    MyLogic.DrawMenu.Add("FlowersRyze.DrawMenu.R", new CheckBox("Draw R Range", false));

                    MyLogic.DrawMenu.AddGroupLabel("-- Logic Status");
                    MyLogic.DrawMenu.Add("FlowersRyze.DrawMenu.Combo", new CheckBox("Draw Combo Status"));
                    MyManaManager.AddDrawToMenu(MyLogic.DrawMenu);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MyMenuManager.Initializer." + ex);
            }
        }
    }
}