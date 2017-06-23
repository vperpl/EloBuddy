
using System.Collections.Generic;
using EloBuddy;

namespace Evade
{
    public static class SpellBlocker
    {
        public static List<SpellSlot> WhitelistedSpellSlots; 

        static SpellBlocker()
        {
            switch (ObjectManager.Player.ChampionName.ToLowerInvariant())
            {
                case "aatrox":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.R };
                    break;

                case "ahri":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "akali":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "alistar":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "amumu":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "anivia":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W};
                    break;

                case "annie":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E, SpellSlot.R };
                    break;

                case "ashe":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.R };
                    break;

                case "azir":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E, SpellSlot.R };
                    break;

                case "bard":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "blitzcrank":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "brand":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E, SpellSlot.R };
                    break;

                case "braum":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "caitlyn":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E };
                    break;

                case "cassiopeia":
                    WhitelistedSpellSlots = new List<SpellSlot>();
                    break;

                case "chogath":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "corki":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "darius":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.R };
                    break;

                case "diana":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "draven":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "drmundo":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "ekko":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E, SpellSlot.R };
                    break;

                case "elise":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "evelynn":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.R };
                    break;

                case "ezreal":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E };
                    break;

                case "fiddlesticks":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q };
                    break;

                case "fiora":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "fizz":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E };
                    break;

                case "galio":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "gangplank":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "garen":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.R };
                    break;

                case "gnar":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "gragas":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "graves":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E, SpellSlot.R };
                    break;

                case "hecarim":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "heimerdinger":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.R };
                    break;

                case "irelia":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "janna":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.E };
                    break;

                case "jarvaniv":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "jax":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "jayce":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "jinx":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.E };
                    break;

                case "kalista":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E, SpellSlot.R };
                    break;

                case "karma":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "karthus":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E };
                    break;

                case "kassadin":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "katarina":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "kayle":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "kennen":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "khazix":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E, SpellSlot.R };
                    break;

                case "kogmaw":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W };
                    break;

                case "leblanc":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "leesin":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "leona":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.R };
                    break;

                case "lissandra":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E, SpellSlot.R };
                    break;

                case "lucian":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E };
                    break;

                case "lulu":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "lux":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "malphite":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "malzahar":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E };
                    break;

                case "maokai":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "masteryi":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.E, SpellSlot.R };
                    break;

                case "missfortune":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W };
                    break;

                case "monkeyking":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "mordekaiser":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.R };
                    break;

                case "morgana":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E, SpellSlot.R };
                    break;

                case "nami":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "nasus":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.R };
                    break;

                case "nautilus":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "nidalee":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "nocturne":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "nunu":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W };
                    break;

                case "olaf":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "orianna":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E };
                    break;

                case "pantheon":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "poppy":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "quinn":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "rammus":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.R };
                    break;

                case "reksai":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "renekton":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "rengar":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W };
                    break;

                case "riven":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.E, SpellSlot.R };
                    break;

                case "rumble":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W };
                    break;

                case "ryze":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.R };
                    break;

                case "sejuani":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E };
                    break;

                case "shaco":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.R };
                    break;

                case "shen":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "shyvana":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "singed":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.R };
                    break;

                case "sion":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "sivir":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.E, SpellSlot.R };
                    break;

                case "skarner":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "sona":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "soraka":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "swain":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "syndra":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.R };
                    break;

                case "tahmkench":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "talon":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.E, SpellSlot.R };
                    break;

                case "taric":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.R };
                    break;

                case "teemo":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "thresh":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q };
                    break;

                case "tristana":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W };
                    break;

                case "trundle":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "tryndamere":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.E, SpellSlot.R };
                    break;

                case "twistedfate":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E };
                    break;

                case "twitch":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.R };
                    break;

                case "udyr":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "urgot":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "varus":
                    WhitelistedSpellSlots = new List<SpellSlot>();
                    break;

                case "vayne":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.R };
                    break;

                case "veigar":
                    WhitelistedSpellSlots = new List<SpellSlot>();
                    break;

                case "velkoz":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.E };
                    break;

                case "vi":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "viktor":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "vladimir":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.R };
                    break;

                case "volibear":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "warwick":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "xerath":
                    WhitelistedSpellSlots = new List<SpellSlot>();
                    break;

                case "xinzhao":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "yasuo":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "yorick":
                    WhitelistedSpellSlots = new List<SpellSlot>();
                    break;

                case "zac":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "zed":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "ziggs":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W };
                    break;

                case "zilean":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;

                case "zyra":
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.W };
                    break;

                default:
                    WhitelistedSpellSlots = new List<SpellSlot> { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
                    break;
            }
            
        }

        public static bool ShouldBlock(SpellSlot spellToCast)
        {
            if (spellToCast == SpellSlot.Summoner1 || spellToCast == SpellSlot.Summoner2)
            {
                return false;
            }

            if (WhitelistedSpellSlots.Contains(spellToCast))
            {
                return false;
            }

            if (spellToCast == SpellSlot.Q || spellToCast == SpellSlot.W || spellToCast == SpellSlot.E ||
                spellToCast == SpellSlot.R)
            {
                return true;  
            }

            return false;
        }
    }
}
