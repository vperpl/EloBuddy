using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Color = System.Drawing.Color;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using SharpDX.IO;

namespace Tyler1
{
    internal class Program
    {
        private static List<string> noobchamps = new List<string>
        {
            "Ahri",
            "Anivia",
            "Annie",
            "Ashe",
            "Azir",
            "Brand",
            "Caitlyn",
            "Cassiopeia",
            "Corki",
            "Draven",
            "Ezreal",
            "Graves",
            "Jinx",
            "Kalista",
            "Karma",
            "Karthus",
            "Katarina",
            "Kennen",
            "KogMaw",
            "Leblanc",
            "Kindred",
            "Lucian",
            "Lux",
            "Malzahar",
            "MasterYi",
            "MissFortune",
            "Orianna",
            "Quinn",
            "Sivir",
            "Syndra",
            "Talon",
            "Teemo",
            "Tristana",
            "TwistedFate",
            "Twitch",
            "Varus",
            "Vayne",
            "Veigar",
            "Velkoz",
            "Viktor",
            "Xerath",
            "Zed",
            "Ziggs",
            "Soraka",
            "Akali",
            "Diana",
            "Ekko",
            "Fiddlesticks",
            "Fiora",
            "Fizz",
            "Heimerdinger",
            "Illaoi",
            "Jayce",
            "Kassadin",
            "Kayle",
            "KhaZix",
            "Kindred",
            "Lissandra",
            "Mordekaiser",
            "Nidalee",
            "Riven",
            "Shaco",
            "Vladimir",
            "Yasuo",
            "Zilean"
        };

        /// <summary>
        /// Those buffs make the target either unkillable or a pain in the ass to kill, just wait until they end
        /// </summary>
        private static List<string> UndyingBuffs = new List<string>
        {
            "JudicatorIntervention",
            "UndyingRage",
            "FerociousHowl",
            "ChronoRevive",
            "ChronoShift",
            "lissandrarself",
            "kindredrnodeathbuff"
        };

        private static Menu Menu;
        public static CheckBox AutoCatch;
        public static CheckBox CatchOnlyCloseToMouse;
        public static Slider MaxDistToMouse;
        public static CheckBox OnlyCatchIfSafe;
        public static Slider MinQLaneclearManaPercent;
        public static Menu EMenu;
        public static CheckBox ECombo;
        public static CheckBox EGC;
        public static CheckBox EInterrupt;
        public static Menu RMenu;
        public static CheckBox RKS;
        public static CheckBox RKSOnlyIfCantAA;
        public static Slider RIfHit;
        public static CheckBox WCombo;
        public static CheckBox UseItems;
        public static Menu DrawingMenu;
        public static CheckBox DrawAXELocation;
        public static CheckBox DrawAXECatchRadius;
        public static CheckBox DrawAXELine;
        public static Slider DrawAXELineWidth;
        public static Color DrawingCololor => Color.Red;
        private static readonly AIHeroClient Player = ObjectManager.Player;
        private static Spell.Active Q, W;
        private static Spell.Skillshot E, R;
        private static Item BOTRK;
        private static Item Bilgewater;
        private static Item Yomamas;
        private static Item Mercurial;
        private static Item QSS;
        public static Color color = Color.DarkOrange;
        public static float MyRange = 550f;
        private static CheckBox R1vs1;

        private static Dictionary<int, GameObject> Reticles;

        private static int AxesCount
        {
            get
            {
                var data = Player.GetBuff("dravenspinningattack");
                if (data == null || data.Count == -1)
                {
                    return 0;
                }
                return data.Count == 0 ? 1 : data.Count;
            }
        }

        private static int TotalAxesCount
        {
            get
            {
                return (ObjectManager.Player.HasBuff("dravenspinning") ? 1 : 0)
                       + (ObjectManager.Player.HasBuff("dravenspinningleft") ? 1 : 0) + Reticles.Count;
            }
        }

        private static void Main()
        {
            Loading.OnLoadingComplete += Load;
        }

        private static void Load(EventArgs args)
        {
            Core.DelayAction(() =>
            {
                if (ObjectManager.Player.CharData.BaseSkinName != "Draven")
                    return;

                InitSpells();
                FinishLoading();
                Reticles = new Dictionary<int, GameObject>();
                GameObject.OnCreate += OnCreate;
                GameObject.OnDelete += OnDelete;
            }, 1500);
        }

        private static void OnDelete(GameObject sender, EventArgs args)
        {
            var itemToDelete = Reticles.FirstOrDefault(ret => ret.Value.NetworkId == sender.NetworkId);
            if (itemToDelete.Key != 0)
            {
                Reticles.Remove(itemToDelete.Key);
            }
        }

        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name.Equals("Draven_Base_Q_reticle_self.troy") && !sender.IsDead)
            {
                Reticles.Add(Core.GameTickCount, sender);
            }
        }

        private static void InitSpells()
        {
            Q = new Spell.Active(SpellSlot.Q);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Linear, 250, 1400, 130);
            R = new Spell.Skillshot(SpellSlot.E, 3000, SkillShotType.Linear, 250, 2000, 160);

            BOTRK = new Item(3153, 550);
            Bilgewater = new Item(3144, 550);
            Yomamas = new Item(3142, 400);
            Mercurial = new Item(3139, 22000);
            QSS = new Item(3140, 22000);
        }

        private static void FinishLoading()
        {
            Drawing.OnDraw += Draw;
            Game.OnUpdate += OnUpdate;

            Gapcloser.OnGapcloser += OnGapcloser;
            Interrupter.OnInterruptableSpell += OnInterruptableTarget;

            Core.DelayAction(() => MyRange = Player.GetAutoAttackRange(), 3000);

            Menu = MainMenu.AddMenu("Tyler1.EXE", "Tyler1");

            AutoCatch = Menu.Add("tyler1auto", new CheckBox("Auto catch axes?"));
            CatchOnlyCloseToMouse = Menu.Add("tyler1onlyclose", new CheckBox("Catch only axes close to mouse?"));
            MaxDistToMouse = Menu.Add("tyler1maxdist", new Slider("Max axe distance to mouse", 500, 250, 1250));
            OnlyCatchIfSafe = Menu.Add("tyler1safeaxes", new CheckBox("Only catch axes if safe (anti melee)", false));
            MinQLaneclearManaPercent = Menu.Add("tyler1QLCMana", new Slider("Min Mana Percent for Q Laneclear", 60));

            EMenu = Menu.AddSubMenu("E Settings: ", "tyler1E");
            ECombo = EMenu.Add("tyler1ECombo", new CheckBox("Use E in Combo"));
            EGC = EMenu.Add("tyler1EGC", new CheckBox("Use E on Gapcloser"));
            EInterrupt = EMenu.Add("tyler1EInterrupt", new CheckBox("Use E to Interrupt"));

            RMenu = Menu.AddSubMenu("R Settings: ", "tyler1R");
            RKS = RMenu.Add("tyler1RKS", new CheckBox("Use R to steal kills"));
            RKSOnlyIfCantAA = RMenu.Add("tyler1RKSOnlyIfCantAA", new CheckBox("Use R KS only if can't AA"));
            RIfHit = RMenu.Add("tyler1RIfHit", new Slider("Use R if it will hit X enemies", 2, 1, 5));
            R1vs1 = RMenu.Add("tyler1R1v1", new CheckBox("Always use R in 1v1"));

            WCombo = Menu.Add("tyler1WCombo", new CheckBox("Use W in Combo?"));
            UseItems = Menu.Add("tyler1Items", new CheckBox("Use Items?"));

            DrawingMenu = Menu.AddSubMenu("R Draw Settings: ", "tyler1DrawSettings");
            DrawAXECatchRadius = DrawingMenu.Add("tyler1AxeCatchDraw", new CheckBox("Draw Axe Catch Radius"));
            DrawAXELocation = DrawingMenu.Add("tyler1AxeLocationDraw", new CheckBox("Draw Axe Location"));
            DrawAXELine = DrawingMenu.Add("tyler1AxeLineDraw", new CheckBox("Draw Line to Axe Position"));
            DrawAXELineWidth = DrawingMenu.Add("tyler1AxeLineDrawWidth", new Slider("Line Width: {0}", 8, 10, 1));
        }

        private static void OnUpdate(EventArgs args)
        {
            try
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                    Farm();

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && target != null)
                {
                    Combo();
                    RCombo();
                }

                CatchAxes();
                KS();

                if (W.IsReady() && Player.HasBuffOfType(BuffType.Slow) && target.Distance(ObjectManager.Player) <= MyRange)
                    W.Cast();

                R1V1(target);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void R1V1(AIHeroClient target)
        {
            if (R1vs1.CurrentValue && target != null && target.IsHPBarRendered && target.Distance(ObjectManager.Player) < 650 && !ShouldntUlt(target))
            {
                if (target.HealthPercent > ObjectManager.Player.HealthPercent && (target.MaxHealth <= ObjectManager.Player.MaxHealth + 300 ||
                     noobchamps.Contains(target.CharData.BaseSkinName)))
                {
                    var pred = R.GetPrediction(target);

                    if (pred.HitChance >= HitChance.High)
                    {
                        R.Cast(pred.UnitPosition);
                    }
                }
            }
        }

        private static bool ShouldntUlt(AIHeroClient target)
        {
            if (target == null || !target.IsHPBarRendered)
                return true;

            if (UndyingBuffs.Any(target.HasBuff))
                return true;

            if (target.CharData.BaseSkinName == "Blitzcrank" && !target.HasBuff("BlitzcrankManaBarrierCD") && !target.HasBuff("ManaBarrier"))
            {
                return true;
            }

            return target.CharData.BaseSkinName == "Sivir" && target.HasBuffOfType(BuffType.SpellShield) || target.HasBuffOfType(BuffType.SpellImmunity);
        }

        private static void RCombo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);

            if (target != null && target.IsHPBarRendered && !target.IsDead && !target.IsZombie)
            {
                R.CastIfItWillHit(RIfHit.CurrentValue);
            }
        }

        private static void Farm()
        {
            if (ObjectManager.Player.ManaPercent < MinQLaneclearManaPercent.CurrentValue)
                return;


            if (ObjectManager.Get<Obj_AI_Minion>()
                    .Any(m => m.IsHPBarRendered && m.IsEnemy && m.Distance(ObjectManager.Player) < MyRange))
            {
                if (TotalAxesCount < 2) Q.Cast();
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            if (target.Distance(Player) < MyRange + 100)
            {
                if (TotalAxesCount < 2) Q.Cast();
                if (WCombo.CurrentValue && W.IsReady() && !Player.HasBuff("dravenfurybuff")) W.Cast();
            }
            if (ECombo.CurrentValue && E.IsReady() && target.IsValidTarget(750))
            {
                var pred = E.GetPrediction(target);
                if (pred.HitChance >= HitChance.High)
                    E.Cast(pred.UnitPosition);
            }

            if (UseItems.CurrentValue)
            {
                if (target.IsValidTarget(MyRange))
                {
                    if (Yomamas.IsReady()) Yomamas.Cast();
                    if (Bilgewater.IsReady()) Bilgewater.Cast(target);
                    if (BOTRK.IsReady()) BOTRK.Cast(target);
                }
                //QSS
                if (Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Fear) ||
                    Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Taunt) ||
                    Player.HasBuffOfType(BuffType.Blind))
                {
                    if (Mercurial.IsReady()) Core.DelayAction(() => Mercurial.Cast(), 100);
                    if (QSS.IsReady()) Core.DelayAction(() => QSS.Cast(), 100);
                }
            }
        }

        private static void CatchAxes()
        {
            Vector3 Mouse = Game.CursorPos;

            if (!ObjectManager.Get<GameObject>().Any(x => x.Name.Equals("Draven_Base_Q_reticle_self.troy") && !x.IsDead) || !AutoCatch.CurrentValue)
            {
                Orbwalker.DisableMovement = false;
            }
            if (AutoCatch.CurrentValue)
            {
                
                foreach (
                    var reticle in
                        Reticles
                            .Where(
                                x => !x.Value.IsDead &&
                                     (!x.Value.Position.IsUnderEnemyTurret() ||
                                      (Mouse.IsUnderEnemyTurret() && ObjectManager.Player.IsUnderEnemyTurret())))
                            .OrderBy(ret => ret.Key))
                {
                    var AXE = reticle.Value;
                    if (OnlyCatchIfSafe.CurrentValue &&
                        EntityManager.Heroes.Enemies.Count(
                            e => e.IsHPBarRendered && e.IsMelee && e.ServerPosition.Distance(AXE.Position) < 350) >= 1)
                    {
                        break;
                    }
                    if (CatchOnlyCloseToMouse.CurrentValue && AXE.Distance(Mouse) > MaxDistToMouse.CurrentValue)
                    {
                        Orbwalker.DisableMovement = false;

                        if (EntityManager.Heroes.Enemies.Count(
                            e => e.IsHPBarRendered && e.IsMelee && e.ServerPosition.Distance(AXE.Position) < 350) >= 1)
                        {
                            //user probably doesn't want to go there, try the next reticle
                            break;
                        }
                        //maybe user just has potato reaction time
                        return;
                    }
                    if (AXE.Distance(Player.ServerPosition) > 80 && Orbwalker.CanMove)
                    {
                        Orbwalker.MoveTo(AXE.Position.Randomize());
                        Orbwalker.DisableMovement = true;
                    }
                    if (AXE.Distance(Player.ServerPosition) <= 80)
                    {
                        Orbwalker.DisableMovement = false;
                    }
                }
            }
        }


        /// <summary>
        /// Will need to add an actual missile check for the axes in air instead of brosciencing
        /// </summary>
        private static void KS()
        {
            if (!RKS.CurrentValue)
                return;

            foreach (
                var enemy in
                    EntityManager.Heroes.Enemies.Where(
                        e =>
                            e.IsHPBarRendered && e.Distance(ObjectManager.Player) < 3000 &&
                            (e.Distance(ObjectManager.Player) > MyRange + 150 || !RKSOnlyIfCantAA.CurrentValue)))
            {
                if (enemy.Health < Player.GetSpellDamage(enemy, SpellSlot.R) && !ShouldntUlt(enemy))
                {
                    var pred = R.GetPrediction(enemy);
                    if (pred.HitChance >= HitChance.High)
                    {
                        R.Cast(pred.UnitPosition);
                    }
                }
            }
        }

        private static void Draw(EventArgs args)
        {
            if (ObjectManager.Player.IsDead)
                return;

            var reticles = ObjectManager.Get<GameObject>().Where(x => x.Name.Equals("Draven_Base_Q_reticle_self.troy") && !x.IsDead).ToArray();

            if (reticles.Any())
            {
                var PlayerPosToScreen = Drawing.WorldToScreen(ObjectManager.Player.Position);

                if (DrawAXELocation.CurrentValue)
                    foreach (var AXE in reticles)
                    {
                        EloBuddy.SDK.Rendering.Circle.Draw(DrawingCololor.ToSharpDX(), 140, DrawAXELineWidth.CurrentValue, AXE.Position);
                    }

                Drawing.DrawLine(PlayerPosToScreen, Drawing.WorldToScreen(reticles[0].Position), DrawAXELineWidth.CurrentValue, DrawingCololor);

                if (DrawAXELine.CurrentValue)
                    for (int i = 0; i < reticles.Length; i++)
                    {
                        if (i < reticles.Length - 1)
                        {
                            Drawing.DrawLine(Drawing.WorldToScreen(reticles[i].Position),
                                Drawing.WorldToScreen(reticles[i + 1].Position), DrawAXELineWidth.CurrentValue, DrawingCololor);
                        }
                    }
                if (DrawAXECatchRadius.CurrentValue)
                    if (CatchOnlyCloseToMouse.CurrentValue && MaxDistToMouse.CurrentValue < 700 && ObjectManager.Get<GameObject>().Any(x => x.Name.Equals("Draven_Base_Q_reticle_self.troy") && !x.IsDead))
                    {
                        EloBuddy.SDK.Rendering.Circle.Draw(DrawingCololor.ToSharpDX(), MaxDistToMouse.CurrentValue, DrawAXELineWidth.CurrentValue, Game.CursorPos);
                    }
            }
        }

        private static void OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (EGC.CurrentValue && E.IsReady() && gapcloser.Sender.Distance(ObjectManager.Player) < 800)
            {
                var pred = E.GetPrediction(gapcloser.Sender);

                if (pred.HitChance > HitChance.High)
                {
                    E.Cast(pred.UnitPosition);
                }
            }
        }

        private static void OnInterruptableTarget(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs gapcloser)
        {
            if (EInterrupt.CurrentValue && E.IsReady() && sender.Distance(ObjectManager.Player) < 950)
            {
                E.Cast(sender.Position);
            }
        }
    }
}