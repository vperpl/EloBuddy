// Copyright 2014 - 2014 Esk0r
// Config.cs is part of Evade.
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
using System.Linq;
using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

#endregion

namespace Evade
{
    internal static class Config
    {
        public const bool PrintSpellData = false;
        public const bool TestOnAllies = false;
        public const int SkillShotsExtraRadius = 9;
        public const int SkillShotsExtraRange = 20;
        public const int GridSize = 10;
        public const int ExtraEvadeDistance = 15;
        public const int PathFindingDistance = 60;
        public const int PathFindingDistance2 = 35;

        public const int DiagonalEvadePointsCount = 7;
        public const int DiagonalEvadePointsStep = 20;

        public const int CrossingTimeOffset = 250;

        public const int EvadingFirstTimeOffset = 250;
        public const int EvadingSecondTimeOffset = 80;

        public const int EvadingRouteChangeTimeOffset = 250;

        public const int EvadePointChangeInterval = 300;
        public static int LastEvadePointChangeT = 0;

        public static Menu Menu { get; set; }
        public static Menu EvadeSpellMenu { get; set; }
        public static Menu SkillShotsMenu { get; set; }
        public static Menu ShieldingMenu { get; set; }
        public static Menu CollisionMenu { get; set; }
        public static Menu DrawingsMenu { get; set; }
        public static Menu MiscMenu { get; set; }

        public static void CreateMenu()
        {
            Menu = MainMenu.AddMenu("Evade", "Evadesharp");

            //Create the evade spells submenus.
            EvadeSpellMenu = Menu.AddSubMenu("Evade spells", "evadeSpells");
            foreach (var spell in EvadeSpellDatabase.Spells)
            {
                EvadeSpellMenu.AddLabel(spell.Name);
                EvadeSpellMenu.Add("DangerLevel" + spell.Name, new Slider("Danger level", spell.DangerLevel, 1, 5));

                if (spell.IsTargetted && spell.ValidTargets.Contains(SpellValidTargets.AllyWards))
                {
                    EvadeSpellMenu.Add("WardJump" + spell.Name, new CheckBox("WardJump"));
                    EvadeSpellMenu.AddSeparator(3);
                }
                EvadeSpellMenu.Add("Enabled" + spell.Name, new CheckBox("Enabled"));
            }

            //Create the skillshots submenus.
            SkillShotsMenu = Menu.AddSubMenu("Skillshots", "SkillshotsMenu");

            foreach (var hero in ObjectManager.Get<AIHeroClient>())
            {
                if (hero.Team != ObjectManager.Player.Team || Config.TestOnAllies)
                {
                    foreach (var spell in SpellDatabase.Spells)
                    {
                        if (string.Equals(spell.ChampionName, hero.ChampionName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            SkillShotsMenu.AddLabel(spell.MenuItemName);

                            SkillShotsMenu.Add("DangerLevel" + spell.MenuItemName,
                                new Slider("Danger level", spell.DangerValue, 1, 5));

                            SkillShotsMenu.Add("IsDangerous" + spell.MenuItemName, new CheckBox("Is Dangerous", spell.IsDangerous));
                            SkillShotsMenu.Add("Draw" + spell.MenuItemName, new CheckBox("Draw"));
                            SkillShotsMenu.Add("Enabled" + spell.MenuItemName, new CheckBox("Enabled", !spell.DisabledByDefault));
                        }
                    }
                }
            }

            ShieldingMenu = Menu.AddSubMenu("Ally shielding", "Shielding");

            foreach (var ally in ObjectManager.Get<AIHeroClient>())
            {
                if (ally.IsAlly && !ally.IsMe)
                {
                    ShieldingMenu.Add("shield" + ally.ChampionName, new CheckBox("Shield " + ally.ChampionName));
                }
            }

            CollisionMenu = Menu.AddSubMenu("Collision", "Collision");
            CollisionMenu.Add("MinionCollision", new CheckBox("Minion collision", false));
            CollisionMenu.Add("HeroCollision", new CheckBox("Hero collision", false));
            CollisionMenu.Add("YasuoCollision", new CheckBox("Yasuo wall collision"));
            CollisionMenu.Add("EnableCollision", new CheckBox("Enabled", false));

            DrawingsMenu = Menu.AddSubMenu("Drawings", "DrawingsMenu");

            DrawingsMenu.Add("EnableDrawings", new CheckBox("Enable Drawings"));


            MiscMenu = Menu.AddSubMenu("Misc", "Miscmenu");
            MiscMenu.Add("BlockSpells",
                new ComboBox("Block spells while evading", 1, "No", "Only dangerous", "Always"));
            MiscMenu.Add("DisableFow", new CheckBox("Disable fog of war dodging", false));
            MiscMenu.Add("ShowEvadeStatus", new CheckBox("Show Evade Status", false));

            if (Player.Instance.CharData.BaseSkinName == "Olaf")
            {
                MiscMenu.Add("DisableEvadeForOlafR", new CheckBox("Automatic disable Evade when Olaf's ulti is active!"));
            }

            Menu.Add("Enabled", new KeyBind("Enabled", true, KeyBind.BindTypes.PressToggle, 'K'));
            Menu.Add("OnlyDangerous", new KeyBind("Enabled", false, KeyBind.BindTypes.HoldActive, 32));
        }
    }
}