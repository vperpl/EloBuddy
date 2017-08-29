namespace Flowers_Ryze.MyBase
{
    #region

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Menu;

    #endregion

    internal class MyLogic
    {
        internal static Spell.Skillshot Q { get; set; }
        internal static Spell.Targeted W { get; set; }
        internal static Spell.Targeted E { get; set; }
        internal static Spell.Skillshot R { get; set; }
        internal static Spell.Targeted Ignite { get; set; }

        internal static SpellSlot IgniteSlot { get; set; } = SpellSlot.Unknown;

        internal static AIHeroClient Me = ObjectManager.Player;

        internal static Menu Menu { get; set; }
        internal static Menu ComboMenu { get; set; }
        internal static Menu HarassMenu { get; set; }
        internal static Menu ClearMenu { get; set; }
        internal static Menu LastHitMenu { get; set; }
        internal static Menu KillStealMenu { get; set; }
        internal static Menu MiscMenu { get; set; }
        internal static Menu DrawMenu { get; set; }

        internal static float Qcd { get; set; }
        internal static float QcdEnd { get; set; }
        internal static float Wcd { get; set; }
        internal static float WcdEnd { get; set; }
        internal static float Ecd { get; set; }
        internal static float EcdEnd { get; set; }
        internal static int LastCastTime { get; set; }
        internal static bool CanShield { get; set; }

        internal static bool HaveShield
            => ObjectManager.Player.HasBuff("RyzeQShield");

        internal static bool NoStack
            => ObjectManager.Player.HasBuff("ryzeqiconnocharge");

        internal static bool HalfStack
            => ObjectManager.Player.HasBuff("ryzeqiconhalfcharge");

        internal static bool FullStack
            => ObjectManager.Player.HasBuff("ryzeqiconfullcharge");
    }
}