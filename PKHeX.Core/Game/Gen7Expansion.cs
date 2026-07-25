using System;

namespace PKHeX.Core;

public static class Gen7Expansion
{
    public const ushort MaxSpeciesID = 1025;
    public const ushort MaxMoveID = 920;
    public const int MaxAbilityID = 316;
    public const int MaxItemID = 1023;
    public const ushort RetailMaxSpeciesID = 807;
    public const ushort RetailMaxMoveID = 728;
    public const ushort ReinsOfUnity = 965;

    public static ReadOnlySpan<ushort> GeneralItemIDs =>
    [
        505, 506, 507, 508, 509, 510, 511, 512, 513, 514, 515, 516, 517, 518, 519, 520,
        960, 961, 962, 963, 964, 966, 967, 968, 969, 970, 971, 972, 973, 974, 975, 976,
        977, 978, 979, 980, 981, 982, 983, 984, 985, 986, 987, 988, 989, 990, 991, 992,
        993, 994, 995, 996, 997, 998, 999, 1000, 1001, 1002, 1003, 1004, 1005, 1006,
        1007, 1008, 1009, 1010, 1011, 1012, 1013, 1014, 1015, 1016, 1017, 1018, 1019,
        1020, 1021, 1022, 1023,
    ];

    public static bool IsItemID(int item) => item is >= 505 and <= 520 or >= 960 and <= 1023;

    public static bool IsMoveAllowed(ushort move)
    {
        if (move <= RetailMaxMoveID)
            return MoveInfo.IsMoveKnowable(move);
        return move is 742 or 920 or >= 744 and <= 756 or >= 775 and <= 919;
    }

    public static string[] ExpandAbilityNames(string[] source)
    {
        var result = source.Length > MaxAbilityID ? (string[])source.Clone() : new string[MaxAbilityID + 1];
        source.CopyTo(result, 0);
        result[311] = "Piercing Drill";
        result[312] = "Dragonize";
        result[313] = "Mega Sol";
        result[314] = "Spicy Spray";
        result[315] = "Eelevate";
        result[316] = "Fire Mane";
        return result;
    }

    public static string[] ExpandItemNames(string[] source)
    {
        var result = source.Length > MaxItemID ? (string[])source.Clone() : new string[MaxItemID + 1];
        source.CopyTo(result, 0);
        foreach (var (id, name) in ItemNames)
            result[id] = name;
        return result;
    }

    public static bool HasFormOverrides(ushort species) => species is
        25 or 26 or 36 or 52 or 58 or 59 or 71 or 77 or 78 or 79 or 80 or 83 or 100 or 101 or 110 or 121 or 122 or
        128 or 144 or 145 or 146 or 149 or 154 or 157 or 160 or 194 or 199 or 211 or 215 or 222 or 227 or 263 or
        264 or 358 or 359 or 398 or 445 or 448 or 478 or 483 or 484 or 485 or 491 or 500 or 503 or 530 or 545 or
        549 or 550 or 554 or 555 or 560 or 562 or 570 or 571 or 604 or 609 or 618 or 623 or 628 or 652 or 655 or
        658 or 668 or 670 or 678 or 687 or 689 or 691 or 701 or 705 or 706 or 713 or 718 or 724 or 740 or 768 or
        780 or 801 or 807 or 845 or 849 or 854 or 855 or 869 or 870 or 875 or 876 or 877 or 888 or 889 or 890 or
        892 or 893 or 898 or 901 or 902 or 905 or 916 or 925 or 931 or 952 or 964 or 970 or 978 or 982 or 998 or
        999 or 1012 or 1013 or 1017 or 1024;

    public static string[] GetFormList(ushort species, string[] gen7, string[] modern)
    {
        var count = PersonalTable.USUMExpansion[species].FormCount;
        if (count <= 1)
            return gen7;

        var result = new string[count];
        for (byte form = 0; form < count; form++)
        {
            var custom = GetFormName(species, form);
            if (custom is not null)
            {
                result[form] = custom;
                continue;
            }
            if (form < gen7.Length && !string.IsNullOrEmpty(gen7[form]))
            {
                result[form] = gen7[form];
                continue;
            }
            if (form < modern.Length && !string.IsNullOrEmpty(modern[form]))
            {
                result[form] = modern[form];
                continue;
            }
            result[form] = form == 0 ? string.Empty : $"Form {form}";
        }
        return result;
    }

    private static string? GetFormName(ushort species, byte form) => (species, form) switch
    {
        (25, 8) => "World Cap Pikachu",
        (26, 2) => "Mega Raichu X",
        (26, 3) => "Mega Raichu Y",
        (36, 1) => "Mega Clefable",
        (52, 2) => "Galarian Meowth",
        (58, 1) => "Hisuian Growlithe",
        (59, 1) => "Hisuian Arcanine",
        (71, 1) => "Mega Victreebel",
        (77, 1) => "Galarian Ponyta",
        (78, 1) => "Galarian Rapidash",
        (79, 1) => "Galarian Slowpoke",
        (80, 2) => "Galarian Slowbro",
        (83, 1) => "Galarian Farfetch'd",
        (100, 1) => "Hisuian Voltorb",
        (101, 1) => "Hisuian Electrode",
        (110, 1) => "Galarian Weezing",
        (121, 1) => "Mega Starmie",
        (122, 1) => "Galarian Mr. Mime",
        (128, 1) => "Paldean Tauros Combat",
        (128, 2) => "Paldean Tauros Blaze",
        (128, 3) => "Paldean Tauros Aqua",
        (144, 1) => "Galarian Articuno",
        (145, 1) => "Galarian Zapdos",
        (146, 1) => "Galarian Moltres",
        (149, 1) => "Mega Dragonite",
        (154, 1) => "Mega Meganium",
        (157, 1) => "Hisuian Typhlosion",
        (160, 1) => "Mega Feraligatr",
        (194, 1) => "Paldean Wooper",
        (199, 1) => "Galarian Slowking",
        (211, 1) => "Hisuian Qwilfish",
        (215, 1) => "Hisuian Sneasel",
        (222, 1) => "Galarian Corsola",
        (227, 1) => "Mega Skarmory",
        (263, 1) => "Galarian Zigzagoon",
        (264, 1) => "Galarian Linoone",
        (358, 1) => "Mega Chimecho",
        (359, 2) => "Mega Absol Z",
        (398, 1) => "Mega Staraptor",
        (445, 2) => "Mega Garchomp Z",
        (448, 2) => "Mega Lucario Z",
        (478, 1) => "Mega Froslass",
        (483, 1) => "Origin Forme Dialga",
        (484, 1) => "Origin Forme Palkia",
        (485, 1) => "Mega Heatran",
        (491, 1) => "Mega Darkrai",
        (500, 1) => "Mega Emboar",
        (503, 1) => "Hisuian Samurott",
        (530, 1) => "Mega Excadrill",
        (545, 1) => "Mega Scolipede",
        (549, 1) => "Hisuian Lilligant",
        (550, 2) => "White-Striped Basculin",
        (554, 1) => "Galarian Darumaka",
        (555, 2) => "Galarian Darmanitan",
        (555, 3) => "Galarian Darmanitan Zen",
        (560, 1) => "Mega Scrafty",
        (562, 1) => "Galarian Yamask",
        (570, 1) => "Hisuian Zorua",
        (571, 1) => "Hisuian Zoroark",
        (604, 1) => "Mega Eelektross",
        (609, 1) => "Mega Chandelure",
        (618, 1) => "Galarian Stunfisk",
        (623, 1) => "Mega Golurk",
        (628, 1) => "Hisuian Braviary",
        (652, 1) => "Mega Chesnaught",
        (655, 1) => "Mega Delphox",
        (658, 3) => "Mega Greninja",
        (668, 1) => "Mega Pyroar",
        (670, 6) => "Mega Eternal Flower Floette",
        (678, 2) => "Mega Meowstic Male",
        (678, 3) => "Mega Meowstic Female",
        (687, 1) => "Mega Malamar",
        (689, 1) => "Mega Barbaracle",
        (691, 1) => "Mega Dragalge",
        (701, 1) => "Mega Hawlucha",
        (705, 1) => "Hisuian Sliggoo",
        (706, 1) => "Hisuian Goodra",
        (713, 1) => "Hisuian Avalugg",
        (718, 5) => "Mega Complete Zygarde",
        (724, 1) => "Hisuian Decidueye",
        (740, 1) => "Mega Crabominable",
        (768, 1) => "Mega Golisopod",
        (780, 1) => "Mega Drampa",
        (801, 2) => "Mega Magearna",
        (801, 3) => "Mega Original Color Magearna",
        (807, 1) => "Mega Zeraora",
        (845, 1) => "Cramorant Gulping",
        (845, 2) => "Cramorant Gorging",
        (849, 1) => "Low Key Toxtricity",
        (854, 1) => "Antique Sinistea",
        (855, 1) => "Antique Polteageist",
        (869, 1) => "Alcremie Ruby Cream",
        (869, 2) => "Alcremie Matcha Cream",
        (869, 3) => "Alcremie Mint Cream",
        (869, 4) => "Alcremie Lemon Cream",
        (869, 5) => "Alcremie Salted Cream",
        (869, 6) => "Alcremie Ruby Swirl",
        (869, 7) => "Alcremie Caramel Swirl",
        (869, 8) => "Alcremie Rainbow Swirl",
        (870, 1) => "Mega Falinks",
        (875, 1) => "Eiscue Noice Face",
        (876, 1) => "Female Indeedee",
        (877, 1) => "Hangry Morpeko",
        (888, 1) => "Crowned Sword Zacian",
        (889, 1) => "Crowned Shield Zamazenta",
        (890, 1) => "Eternamax Eternatus",
        (892, 1) => "Rapid-Strike Urshifu",
        (893, 1) => "Dada Zarude",
        (898, 1) => "Ice Rider Calyrex",
        (898, 2) => "Shadow Rider Calyrex",
        (901, 1) => "Bloodmoon Ursaluna",
        (902, 1) => "Female Basculegion",
        (905, 1) => "Therian Enamorus",
        (916, 1) => "Female Oinkologne",
        (925, 1) => "Maushold Family of Three",
        (931, 1) => "Blue Squawkabilly",
        (931, 2) => "Yellow Squawkabilly",
        (931, 3) => "White Squawkabilly",
        (952, 1) => "Mega Scovillain",
        (964, 1) => "Palafin Hero",
        (970, 1) => "Mega Glimmora",
        (978, 1) => "Droopy Tatsugiri",
        (978, 2) => "Stretchy Tatsugiri",
        (978, 3) => "Mega Tatsugiri Curly",
        (978, 4) => "Mega Tatsugiri Droopy",
        (978, 5) => "Mega Tatsugiri Stretchy",
        (982, 1) => "Three-Segment Dudunsparce",
        (998, 1) => "Mega Baxcalibur",
        (999, 1) => "Roaming Gimmighoul",
        (1012, 1) => "Artisan Poltchageist",
        (1013, 1) => "Masterpiece Sinistcha",
        (1017, 1) => "Wellspring Ogerpon",
        (1017, 2) => "Hearthflame Ogerpon",
        (1017, 3) => "Cornerstone Ogerpon",
        (1024, 1) => "Terastal Terapagos",
        (1024, 2) => "Stellar Terapagos",
        _ => null,
    };

    private static readonly (ushort ID, string Name)[] ItemNames =
    [
        (960, "Booster Energy"),
        (961, "Eternatusite"),
        (962, "Tera Crystal"),
        (963, "Rusted Sword"),
        (964, "Rusted Shield"),
        (965, "Reins of Unity"),
        (966, "Adamant Crystal"),
        (967, "Lustrous Globe"),
        (968, "Wellspring Mask"),
        (969, "Hearthflame Mask"),
        (970, "Cornerstone Mask"),
        (971, "Galarica Cuff"),
        (972, "Galarica Wreath"),
        (973, "Tart Apple"),
        (974, "Sweet Apple"),
        (975, "Cracked Pot"),
        (976, "Chipped Pot"),
        (977, "Strawberry Sweet"),
        (978, "Berry Sweet"),
        (979, "Love Sweet"),
        (980, "Star Sweet"),
        (981, "Clover Sweet"),
        (982, "Flower Sweet"),
        (983, "Ribbon Sweet"),
        (984, "Darkness Scroll"),
        (985, "Scroll of Waters"),
        (986, "Black Augurite"),
        (987, "Peat Block"),
        (988, "Auspicious Armor"),
        (989, "Malicious Armor"),
        (990, "Leader's Crest"),
        (991, "Syrupy Apple"),
        (992, "Unremarkable Cup"),
        (993, "Masterpiece Cup"),
        (994, "Gimmighoul Coin"),
        (995, "Raichunite X"),
        (996, "Raichunite Y"),
        (997, "Clefablite"),
        (998, "Victreebelite"),
        (999, "Starminite"),
        (1000, "Dragoninite"),
        (1001, "Meganiumite"),
        (1002, "Feraligite"),
        (1003, "Skarmorite"),
        (1004, "Froslassite"),
        (1005, "Emboarite"),
        (1006, "Excadrite"),
        (1007, "Scolipite"),
        (1008, "Scraftinite"),
        (1009, "Eelektrossite"),
        (1010, "Chandelurite"),
        (1011, "Chesnaughtite"),
        (1012, "Delphoxite"),
        (1013, "Pyroarite"),
        (1014, "Malamarite"),
        (1015, "Barbaracite"),
        (1016, "Dragalgite"),
        (1017, "Hawluchanite"),
        (1018, "Drampanite"),
        (1019, "Falinksite"),
        (1020, "Chimechite"),
        (1021, "Staraptite"),
        (1022, "Heatranite"),
        (1023, "Darkranite"),
        (505, "Golurkite"),
        (506, "Crabominite"),
        (507, "Golisopite"),
        (508, "Zeraorite"),
        (509, "Scovillainite"),
        (510, "Baxcalibrite"),
        (511, "Glimmoranite"),
        (512, "Absolite Z"),
        (513, "Garchompite Z"),
        (514, "Lucarionite Z"),
        (515, "Greninjite"),
        (516, "Floettite"),
        (517, "Zygardite"),
        (518, "Meowsticite"),
        (519, "Magearnite"),
        (520, "Tatsugirinite"),
    ];
}
