using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace PKHeX.Core.Tests.PKM;

public class Gen7ExpansionTests
{
    [Fact]
    public void PersonalTableUsesExpansionGeometry()
    {
        PersonalTable.USUMExpansion.MaxSpeciesID.Should().Be(Gen7Expansion.MaxSpeciesID);
        PersonalTable.USUMExpansion.Count.Should().Be(1330);
        PersonalTable.USUMExpansion[1025].FormCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void PersonalAbilitiesPreserveNinthBit()
    {
        var info = new PersonalInfo7(new byte[PersonalInfo7.SIZE])
        {
            Ability1 = 311,
            Ability2 = 312,
            AbilityH = 316,
        };

        info.Ability1.Should().Be(311);
        info.Ability2.Should().Be(312);
        info.AbilityH.Should().Be(316);
        info.Write()[0x53].Should().Be(7);

        info.Ability2 = 42;
        info.Ability1.Should().Be(311);
        info.Ability2.Should().Be(42);
        info.AbilityH.Should().Be(316);
        info.Write()[0x53].Should().Be(5);
    }

    [Fact]
    public void PK7AbilityRoundTripsWithAbilityNumber()
    {
        var pk = new PK7
        {
            Ability = 316,
            AbilityNumber = 4,
        };

        var stored = new byte[pk.SIZE_STORED];
        pk.WriteDecryptedDataStored(stored);
        var reopened = new PK7(stored);

        reopened.Ability.Should().Be(316);
        reopened.AbilityNumber.Should().Be(4);

        reopened.Ability = 42;
        reopened.AbilityNumber.Should().Be(4);
        reopened.Ability.Should().Be(42);
    }

    [Fact]
    public void MoveTablesAndFilteringUseExpansionData()
    {
        MoveInfo.GetPP(EntityContext.Gen7, 742).Should().Be(5);
        MoveInfo.GetPP(EntityContext.Gen7, 920).Should().Be(10);
        MoveInfo.GetType(920, EntityContext.Gen7).Should().Be(15);
        Gen7Expansion.IsMoveAllowed(742).Should().BeTrue();
        Gen7Expansion.IsMoveAllowed(743).Should().BeFalse();
        Gen7Expansion.IsMoveAllowed(757).Should().BeFalse();
        Gen7Expansion.IsMoveAllowed(919).Should().BeTrue();
        Gen7Expansion.IsMoveAllowed(920).Should().BeTrue();
    }

    [Fact]
    public void ItemsUseExpansionNamesAndPouches()
    {
        Gen7Expansion.GeneralItemIDs.Length.Should().Be(79);
        Gen7Expansion.IsItemID(505).Should().BeTrue();
        Gen7Expansion.IsItemID(959).Should().BeFalse();
        Gen7Expansion.IsItemID(1023).Should().BeTrue();
        var strings = GameInfo.GetStrings("en");
        var names = strings.GetItemStrings(EntityContext.Gen7, GameVersion.US);
        names[505].Should().Be("Golurkite");
        names[995].Should().Be("Raichunite X");
        names[1023].Should().NotBeNullOrEmpty();
        ItemStorage7USUM.General.IndexOf((ushort)995).Should().BeGreaterThanOrEqualTo(0);
        ItemStorage7USUM.General.IndexOf(Gen7Expansion.ReinsOfUnity).Should().Be(-1);
        ItemStorage7USUM.Key.IndexOf(Gen7Expansion.ReinsOfUnity).Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void AbilityNamesIncludeExpansionEntries()
    {
        var names = GameInfo.GetStrings("en").abilitylist;
        names.Length.Should().Be(Gen7Expansion.MaxAbilityID + 1);
        names[311].Should().Be("Piercing Drill");
        names[316].Should().Be("Fire Mane");
    }

    [Fact]
    public void FormNamesUseExpansionMappings()
    {
        var strings = GameInfo.GetStrings("en");
        var pikachu = FormConverter.GetFormList(25, strings.Types, strings.forms, EntityContext.Gen7);
        var zygarde = FormConverter.GetFormList(718, strings.Types, strings.forms, EntityContext.Gen7);
        var terapagos = FormConverter.GetFormList(1024, strings.Types, strings.forms, EntityContext.Gen7);

        pikachu[8].Should().Be("World Cap Pikachu");
        zygarde[5].Should().Be("Mega Complete Zygarde");
        terapagos[2].Should().Be("Stellar Terapagos");
    }

    [Fact]
    public void PK7LegalityIsSuppressed()
    {
        var result = new LegalityAnalysis(new PK7());
        result.Valid.Should().BeTrue();
        result.Parsed.Should().BeFalse();
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public void ExpandedMoveListContainsOnlyRetainedMoves()
    {
        var sav = new SAV7USUM();
        var source = new GameDataSource(GameInfo.GetStrings("en"));
        var filtered = new FilteredGameDataSource(sav, source);
        var ids = filtered.Moves.Select(z => z.Value).ToArray();

        ids.Should().Contain(742);
        ids.Should().Contain(920);
        ids.Should().NotContain(743);
        ids.Should().NotContain(757);
    }
}
