namespace Flowers_Ryze.MyCommon
{
    #region

    using EloBuddy;
    using EloBuddy.SDK;
    using EloBuddy.SDK.Enumerations;

    using Flowers_Ryze.MyBase;

    using System;

    #endregion

    internal class MySpellManager
    {
        internal static void Initializer()
        {
            try
            {
                MyLogic.Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, int.MaxValue, 50, DamageType.Magical)
                {
                    AllowedCollisionCount = 0,
                    MinimumHitChance = HitChance.High
                };

                MyLogic.W = new Spell.Targeted(SpellSlot.W, 615, DamageType.Magical)
                {
                    CastDelay = 350
                };

                MyLogic.E = new Spell.Targeted(SpellSlot.E, 600, DamageType.Magical)
                {
                    CastDelay = 500
                };

                MyLogic.R = new Spell.Skillshot(SpellSlot.R, 1500, SkillShotType.Circular, 2500, int.MaxValue, 475, DamageType.True)
                {
                    AllowedCollisionCount = int.MaxValue
                };

                MyLogic.IgniteSlot = ObjectManager.Player.GetSpellSlotFromName("summonerdot");

                if (MyLogic.IgniteSlot != SpellSlot.Unknown)
                {
                    MyLogic.Ignite = new Spell.Targeted(MyLogic.IgniteSlot, 600, DamageType.True);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MySpellManager.Initializer." + ex);
            }
        }
    }
}