using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Points;

public class CleanUpBritanniaData : PointsSystem
{
    public static CleanUpBritanniaData Instance { get; private set; }
    public static bool Enabled { get; private set; }

    public override PointsType Loyalty => PointsType.CleanUpBritannia;
    public override TextDefinition Name => null;
    public override bool AutoAdd => true;
    public override double MaxPoints => double.MaxValue;
    public override bool ShowOnLoyaltyGump => false;

    // Points table: built once at class load; single-threaded server, no lock needed.
    private static readonly Dictionary<Type, double> _entries = new()
    {
        // Decorative
        [typeof(DecorativeTopiary)] = 2.0,

        // Fishing
        [typeof(FabledFishingNet)] = 2500.0,
        [typeof(MessageInABottle)] = 100.0,
        [typeof(SOS)] = 100.0,
        [typeof(Rope)] = 1600.0,
        [typeof(SpecialFishingNet)] = 250.0,

        // Mining
        [typeof(IronIngot)] = 0.10,
        [typeof(DullCopperIngot)] = 0.50,
        [typeof(ShadowIronIngot)] = 0.75,
        [typeof(CopperIngot)] = 1.0,
        [typeof(BronzeIngot)] = 1.50,
        [typeof(GoldIngot)] = 2.50,
        [typeof(AgapiteIngot)] = 5.0,
        [typeof(VeriteIngot)] = 8.50,
        [typeof(ValoriteIngot)] = 10.0,
        [typeof(Amber)] = 0.30,
        [typeof(Citrine)] = 0.30,
        [typeof(Ruby)] = 0.30,
        [typeof(Tourmaline)] = 0.30,
        [typeof(Amethyst)] = 0.30,
        [typeof(Emerald)] = 0.30,
        [typeof(Sapphire)] = 0.30,
        [typeof(StarSapphire)] = 0.30,
        [typeof(Diamond)] = 0.30,
        [typeof(BlueDiamond)] = 25.0,
        [typeof(FireRuby)] = 25.0,
        [typeof(PerfectEmerald)] = 25.0,
        [typeof(DarkSapphire)] = 25.0,
        [typeof(Turquoise)] = 25.0,
        [typeof(EcruCitrine)] = 25.0,
        [typeof(WhitePearl)] = 25.0,

        // Lumberjacking
        [typeof(Board)] = 0.05,
        [typeof(OakBoard)] = 0.10,
        [typeof(AshBoard)] = 0.25,
        [typeof(YewBoard)] = 0.50,
        [typeof(HeartwoodBoard)] = 1.0,
        [typeof(BloodwoodBoard)] = 2.0,
        [typeof(FrostwoodBoard)] = 3.0,
        [typeof(BarkFragment)] = 1.60,
        [typeof(LuminescentFungi)] = 2.0,
        [typeof(SwitchItem)] = 3.0,
        [typeof(ParasiticPlant)] = 6.0,
        [typeof(BrilliantAmber)] = 62.0,

        // Fletching
        [typeof(Arrow)] = 0.05,
        [typeof(Bolt)] = 0.05,

        // Tailoring
        [typeof(Leather)] = 0.10,
        [typeof(SpinedLeather)] = 0.50,
        [typeof(HornedLeather)] = 1.0,
        [typeof(BarbedLeather)] = 2.0,

        // BOD Rewards
        [typeof(Sandals)] = 2.0,
        [typeof(LeatherGlovesOfMining)] = 50.0,
        [typeof(StuddedGlovesOfMining)] = 100.0,
        [typeof(RingmailGlovesOfMining)] = 500.0,

        // ArtifactRarity 1 Stealable Artifacts
        [typeof(RockArtifact)] = 5.0,
        [typeof(SkullCandleArtifact)] = 5.0,
        [typeof(BottleArtifact)] = 5.0,
        [typeof(DamagedBooksArtifact)] = 5.0,
        [typeof(Basket1Artifact)] = 5.0,
        [typeof(Basket2Artifact)] = 5.0,
        [typeof(Basket3NorthArtifact)] = 5.0,
        [typeof(Basket3WestArtifact)] = 5.0,

        // ArtifactRarity 2 Stealable Artifacts
        [typeof(StretchedHideArtifact)] = 15.0,
        [typeof(BrazierArtifact)] = 15.0,
        [typeof(Basket4Artifact)] = 15.0,
        [typeof(Basket5NorthArtifact)] = 15.0,
        [typeof(Basket5WestArtifact)] = 15.0,
        [typeof(Basket6Artifact)] = 15.0,
        [typeof(ZenRock1Artifact)] = 15.0,

        // ArtifactRarity 3 Stealable Artifacts
        [typeof(LampPostArtifact)] = 25.0,
        [typeof(BooksNorthArtifact)] = 25.0,
        [typeof(BooksWestArtifact)] = 25.0,
        [typeof(BooksFaceDownArtifact)] = 25.0,
        [typeof(BowlsVerticalArtifact)] = 25.0,
        [typeof(FanWestArtifact)] = 25.0,
        [typeof(FanNorthArtifact)] = 25.0,
        [typeof(Sculpture1Artifact)] = 25.0,
        [typeof(Sculpture2Artifact)] = 25.0,
        [typeof(TeapotWestArtifact)] = 25.0,
        [typeof(TeapotNorthArtifact)] = 25.0,
        [typeof(TowerLanternArtifact)] = 25.0,
        [typeof(Urn1Artifact)] = 25.0,
        [typeof(Urn2Artifact)] = 25.0,
        [typeof(ZenRock2Artifact)] = 25.0,
        [typeof(ZenRock3Artifact)] = 25.0,
        // JugsOfGoblinRotgutArtifact omitted — in omit-list
        // MysteriousSupperArtifact omitted — in omit-list

        // ArtifactRarity 4 Stealable Artifacts
        [typeof(BowlArtifact)] = 50.0,
        [typeof(BowlsHorizontalArtifact)] = 50.0,
        [typeof(CupsArtifact)] = 50.0,
        [typeof(TripleFanWestArtifact)] = 50.0,
        [typeof(TripleFanNorthArtifact)] = 50.0,
        [typeof(Painting1WestArtifact)] = 50.0,
        [typeof(Painting1NorthArtifact)] = 50.0,
        [typeof(Painting2WestArtifact)] = 50.0,
        [typeof(Painting2NorthArtifact)] = 50.0,
        [typeof(SakeArtifact)] = 50.0,
        // StolenBottlesOfLiquor1Artifact omitted — in omit-list
        // StolenBottlesOfLiquor2Artifact omitted — in omit-list
        // BottlesOfSpoiledWine1Artifact omitted — in omit-list
        // NaverysWeb1Artifact omitted — in omit-list
        // NaverysWeb2Artifact omitted — in omit-list

        // ArtifactRarity 5 Stealable Artifacts
        [typeof(Painting3Artifact)] = 100.0,
        [typeof(SwordDisplay1WestArtifact)] = 100.0,
        [typeof(SwordDisplay1NorthArtifact)] = 100.0,
        // DyingPlantArtifact omitted — in omit-list
        // LargePewterBowlArtifact omitted — in omit-list
        // NaverysWeb3Artifact omitted — in omit-list
        // NaverysWeb4Artifact omitted — in omit-list
        // NaverysWeb5Artifact omitted — in omit-list
        // NaverysWeb6Artifact omitted — in omit-list
        // BloodySpoonArtifact omitted — in omit-list
        // RemnantsOfMeatLoafArtifact omitted — in omit-list
        // HalfEatenSupperArtifact omitted — in omit-list
        [typeof(BackpackArtifact)] = 100.0,
        [typeof(BloodyWaterArtifact)] = 100.0,
        [typeof(EggCaseArtifact)] = 100.0,
        [typeof(GruesomeStandardArtifact)] = 100.0,
        [typeof(SkinnedGoatArtifact)] = 100.0,
        [typeof(StuddedLeggingsArtifact)] = 100.0,
        [typeof(TarotCardsArtifact)] = 100.0,

        // ArtifactRarity 6 Stealable Artifacts
        [typeof(Painting4WestArtifact)] = 200.0,
        [typeof(Painting4NorthArtifact)] = 200.0,
        [typeof(SwordDisplay2WestArtifact)] = 200.0,
        [typeof(SwordDisplay2NorthArtifact)] = 200.0,
        // LargeDyingPlantArtifact omitted — in omit-list
        // GargishLuckTotemArtifact omitted — in omit-list
        // BookOfTruthArtifact omitted — in omit-list
        // GargishTraditionalVaseArtifact omitted — in omit-list
        // GargishProtectiveTotemArtifact omitted — in omit-list
        // BottlesOfSpoiledWine2Artifact omitted — in omit-list
        // BatteredPanArtifact omitted — in omit-list
        // RustedPanArtifact omitted — in omit-list

        // ArtifactRarity 7 Stealable Artifacts
        [typeof(FlowersArtifact)] = 350.0,
        // GargishBentasVaseArtifact omitted — in omit-list
        // GargishPortraitArtifact omitted — in omit-list
        // GargishKnowledgeTotemArtifact omitted — in omit-list
        // GargishMemorialStatueArtifact omitted — in omit-list
        // StolenBottlesOfLiquor3Artifact omitted — not in Edge (additional omission)
        // BottlesOfSpoiledWine3Artifact omitted — in omit-list
        // DriedUpInkWellArtifact omitted — in omit-list
        // FakeCopperIngotsArtifact omitted — in omit-list
        [typeof(CocoonArtifact)] = 350.0,
        [typeof(StuddedTunicArtifact)] = 350.0,

        // ArtifactRarity 8 Stealable Artifacts
        [typeof(Painting5WestArtifact)] = 750.0,
        [typeof(Painting5NorthArtifact)] = 750.0,
        [typeof(DolphinLeftArtifact)] = 750.0,
        [typeof(DolphinRightArtifact)] = 750.0,
        [typeof(SwordDisplay3SouthArtifact)] = 750.0,
        [typeof(SwordDisplay3EastArtifact)] = 750.0,
        [typeof(SwordDisplay4WestArtifact)] = 750.0,
        // PushmePullyuArtifact omitted — in omit-list
        // StolenBottlesOfLiquor4Artifact omitted — not in Edge (additional omission)
        // RottedOarsArtifact omitted — in omit-list
        // PricelessTreasureArtifact omitted — in omit-list
        [typeof(SkinnedDeerArtifact)] = 750.0,

        // ArtifactRarity 9 Stealable Artifacts
        [typeof(Painting6WestArtifact)] = 1400.0,
        [typeof(Painting6NorthArtifact)] = 1400.0,
        [typeof(ManStatuetteSouthArtifact)] = 1400.0,
        [typeof(ManStatuetteEastArtifact)] = 1400.0,
        [typeof(SwordDisplay4NorthArtifact)] = 1400.0,
        [typeof(SwordDisplay5WestArtifact)] = 1400.0,
        [typeof(SwordDisplay5NorthArtifact)] = 1400.0,
        // TyballsFlaskStandArtifact omitted — in omit-list
        // BlockAndTackleArtifact omitted — in omit-list
        [typeof(LeatherTunicArtifact)] = 1400.0,
        [typeof(SaddleArtifact)] = 1400.0,

        // ArtifactRarity 10
        [typeof(TitansHammer)] = 2750.0,
        [typeof(ZyronicClaw)] = 2750.0,
        [typeof(InquisitorsResolution)] = 2750.0,
        [typeof(BladeOfTheRighteous)] = 2750.0,
        [typeof(LegacyOfTheDreadLord)] = 2750.0,
        [typeof(TheTaskmaster)] = 2750.0,

        // Virtue Artifacts
        // TenthAnniversarySculpture omitted — in omit-list
        // MapOfTheKnownWorld omitted — in omit-list
        // AnkhPendant omitted — in omit-list
        // DragonsEnd omitted — in omit-list
        // JaanasStaff omitted — in omit-list
        // KatrinasCrook omitted — in omit-list
        // LordBlackthornsExemplar omitted — in omit-list
        // SentinelsGuard omitted — in omit-list
        // CompassionArms omitted — in omit-list
        // JusticeBreastplate omitted — in omit-list
        [typeof(ValorGauntlets)] = 1500.0,
        [typeof(SpiritualityHelm)] = 1500.0,
        // HonestyGorget omitted — in omit-list
        // HonorLegs omitted — in omit-list
        // SacrificeSollerets omitted — in omit-list

        // Minor Artifacts (ML/Peerless/Tokuno)
        [typeof(CandelabraOfSouls)] = 100.0,
        [typeof(GhostShipAnchor)] = 100.0,
        [typeof(GoldBricks)] = 100.0,
        [typeof(PhillipsWoodenSteed)] = 100.0,
        [typeof(SeahorseStatuette)] = 100.0,
        [typeof(ShipModelOfTheHMSCape)] = 100.0,
        [typeof(AdmiralsHeartyRum)] = 100.0,
        [typeof(AlchemistsBauble)] = 100.0,
        [typeof(ArcticDeathDealer)] = 100.0,
        [typeof(BlazeOfDeath)] = 100.0,
        [typeof(BurglarsBandana)] = 100.0,
        [typeof(CaptainQuacklebushsCutlass)] = 100.0,
        [typeof(CavortingClub)] = 100.0,
        [typeof(DreadPirateHat)] = 100.0,
        [typeof(EnchantedTitanLegBone)] = 100.0,
        [typeof(GwennosHarp)] = 100.0,
        [typeof(IolosLute)] = 100.0,
        [typeof(LunaLance)] = 100.0,
        [typeof(NightsKiss)] = 100.0,
        [typeof(NoxRangersHeavyCrossbow)] = 100.0,
        [typeof(PolarBearMask)] = 100.0,
        [typeof(VioletCourage)] = 100.0,
        [typeof(GlovesOfThePugilist)] = 100.0,
        [typeof(PixieSwatter)] = 100.0,
        [typeof(WrathOfTheDryad)] = 100.0,
        [typeof(StaffOfPower)] = 100.0,
        [typeof(OrcishVisage)] = 100.0,
        [typeof(BowOfTheJukaKing)] = 100.0,
        [typeof(ColdBlood)] = 100.0,
        // CreepingVine omitted — in omit-list
        // ForgedPardon omitted — in omit-list
        // ManaPhasingOrb omitted — in omit-list
        // RunedSashOfWarding omitted — in omit-list
        // SurgeShield omitted — in omit-list
        [typeof(HeartOfTheLion)] = 100.0,
        [typeof(ShieldOfInvulnerability)] = 100.0,
        [typeof(AegisOfGrace)] = 100.0,
        [typeof(BladeDance)] = 100.0,
        [typeof(BloodwoodSpirit)] = 100.0,
        [typeof(Bonesmasher)] = 100.0,
        [typeof(Boomstick)] = 100.0,
        [typeof(BrightsightLenses)] = 100.0,
        [typeof(FeyLeggings)] = 100.0,
        [typeof(FleshRipper)] = 100.0,
        [typeof(HelmOfSwiftness)] = 100.0,
        [typeof(PadsOfTheCuSidhe)] = 100.0,
        [typeof(QuiverOfRage)] = 100.0,
        [typeof(QuiverOfElements)] = 100.0,
        [typeof(RaedsGlory)] = 100.0,
        [typeof(RighteousAnger)] = 100.0,
        [typeof(RobeOfTheEclipse)] = 100.0,
        [typeof(RobeOfTheEquinox)] = 100.0,
        [typeof(SoulSeeker)] = 100.0,
        [typeof(TalonBite)] = 100.0,
        [typeof(TotemOfVoid)] = 100.0,
        [typeof(WildfireBow)] = 100.0,
        [typeof(Windsong)] = 100.0,
        [typeof(CrimsonCincture)] = 100.0,
        // DreadFlute omitted — in omit-list
        // DreadsRevenge omitted — in omit-list
        [typeof(MelisandesCorrodedHatchet)] = 100.0,
        // AlbinoSquirrelImprisonedInCrystal omitted — in omit-list
        [typeof(GrizzledMareStatuette)] = 100.0,
        // GrizzleGauntlets omitted — in omit-list
        // GrizzleGreaves omitted — in omit-list
        // GrizzleHelm omitted — in omit-list
        // GrizzleTunic omitted — in omit-list
        // GrizzleVambraces omitted — in omit-list
        // ParoxysmusSwampDragonStatuette omitted — in omit-list
        // ScepterOfTheChief omitted — in omit-list
        // CrystallineRing omitted — in omit-list
        // MarkOfTravesty omitted — in omit-list
        // ImprisonedDog omitted — in omit-list
        [typeof(AncientFarmersKasa)] = 100.0,
        [typeof(AncientSamuraiDo)] = 100.0,
        [typeof(AncientUrn)] = 100.0,
        [typeof(ArmsOfTacticalExcellence)] = 100.0,
        [typeof(BlackLotusHood)] = 100.0,
        [typeof(ChestOfHeirlooms)] = 100.0,
        [typeof(DaimyosHelm)] = 100.0,
        [typeof(DemonForks)] = 100.0,
        [typeof(TheDestroyer)] = 100.0,
        [typeof(DragonNunchaku)] = 100.0,
        [typeof(Exiler)] = 100.0,
        [typeof(FluteOfRenewal)] = 100.0,
        [typeof(GlovesOfTheSun)] = 100.0,
        [typeof(HanzosBow)] = 100.0,
        [typeof(HonorableSwords)] = 100.0,
        [typeof(LegsOfStability)] = 100.0,
        [typeof(LeurociansMempoOfFortune)] = 100.0,
        [typeof(PeasantsBokuto)] = 100.0,
        [typeof(PilferedDancerFans)] = 100.0,
        [typeof(TomeOfEnlightenment)] = 100.0,

        // Stygian Abyss Artifacts
        // AbyssalBlade omitted — in omit-list
        // AnimatedLegsoftheInsaneTinker omitted — in omit-list
        // AxeOfAbandon omitted — in omit-list
        // AxesOfFury omitted — in omit-list
        // BansheesCall omitted — in omit-list
        // BasiliskHideBreastplate omitted — in omit-list
        // BladeOfBattle omitted — in omit-list
        // BouraTailShield omitted — in omit-list
        // BreastplateOfTheBerserker omitted — in omit-list
        // BurningAmber omitted — in omit-list
        // CastOffZombieSkin omitted — in omit-list
        // CavalrysFolly omitted — in omit-list
        // ChannelersDefender omitted — in omit-list
        // ClawsOfTheBerserker omitted — in omit-list
        // DeathsHead omitted — in omit-list
        // DefenderOfTheMagus omitted — in omit-list
        // DemonBridleRing omitted — in omit-list
        // DemonHuntersStandard omitted — in omit-list
        // DragonHideShield omitted — in omit-list
        // DragonJadeEarrings omitted — in omit-list
        // DraconisWrath omitted — in omit-list
        // EternalGuardianStaff omitted — in omit-list
        // FallenMysticsSpellbook omitted — in omit-list
        // GiantSteps omitted — in omit-list
        // IronwoodCompositeBow omitted — in omit-list
        // JadeWarAxe omitted — in omit-list
        // LegacyOfDespair omitted — in omit-list
        // Lavaliere omitted — in omit-list
        // LifeSyphon omitted — in omit-list
        // Mangler omitted — in omit-list
        // MantleOfTheFallen omitted — in omit-list
        // MysticsGarb omitted — in omit-list
        // NightEyes omitted — in omit-list
        // ObsidianEarrings omitted — in omit-list
        // PetrifiedSnake omitted — in omit-list
        // PillarOfStrength omitted — in omit-list
        // ProtectoroftheBattleMage omitted — in omit-list
        // RaptorClaw omitted — in omit-list
        // ResonantStaffofEnlightenment omitted — in omit-list
        // ShroudOfTheCondemned omitted — in omit-list
        // GargishSignOfOrder omitted — in omit-list
        // HumanSignOfOrder omitted — in omit-list
        // GargishSignOfChaos omitted — in omit-list
        // HumanSignOfChaos omitted — in omit-list
        // Slither omitted — in omit-list
        // SpinedBloodwormBracers omitted — in omit-list
        // StandardOfChaos omitted — in omit-list
        // StandardOfChaosG omitted — in omit-list
        // StaffOfShatteredDreams omitted — in omit-list
        // StoneDragonsTooth omitted — in omit-list
        // StoneSlithClaw omitted — in omit-list
        // StormCaller omitted — in omit-list
        // SwordOfShatteredHopes omitted — in omit-list
        // SummonersKilt omitted — in omit-list
        // Tangle1 omitted — in omit-list
        // TheImpalersPick omitted — in omit-list
        // TorcOfTheGuardians omitted — in omit-list
        // TokenOfHolyFavor omitted — in omit-list
        // VampiricEssence omitted — in omit-list
        // Venom omitted — in omit-list
        // VoidInfusedKilt omitted — in omit-list
        // WallOfHungryMouths omitted — in omit-list

        // Tokuno Major Artifacts
        [typeof(DarkenedSky)] = 2500.0,
        [typeof(KasaOfTheRajin)] = 2500.0,
        [typeof(RuneBeetleCarapace)] = 2500.0,
        [typeof(Stormgrip)] = 2500.0,
        [typeof(SwordOfTheStampede)] = 2500.0,
        [typeof(SwordsOfProsperity)] = 2500.0,
        [typeof(TheHorselord)] = 2500.0,
        [typeof(TomeOfLostKnowledge)] = 2500.0,
        [typeof(WindsEdge)] = 2500.0,

        // Major Artifacts
        [typeof(TheDryadBow)] = 5500.0,
        [typeof(RingOfTheElements)] = 5500.0,
        [typeof(ArcaneShield)] = 5500.0,
        [typeof(SerpentsFang)] = 5500.0,
        [typeof(OrnamentOfTheMagician)] = 5500.0,
        [typeof(BoneCrusher)] = 5500.0,
        [typeof(OrnateCrownOfTheHarrower)] = 5500.0,
        [typeof(HuntersHeaddress)] = 5500.0,
        [typeof(DivineCountenance)] = 5500.0,
        [typeof(BraceletOfHealth)] = 5500.0,
        [typeof(Aegis)] = 5500.0,
        [typeof(AxeOfTheHeavens)] = 5500.0,
        [typeof(HelmOfInsight)] = 5500.0,
        [typeof(Frostbringer)] = 5500.0,
        [typeof(StaffOfTheMagi)] = 5500.0,
        [typeof(TheDragonSlayer)] = 5500.0,
        [typeof(BreathOfTheDead)] = 5500.0,
        [typeof(HolyKnightsBreastplate)] = 5500.0,
        [typeof(TunicOfFire)] = 5500.0,
        [typeof(ShadowDancerLeggings)] = 5500.0,
        [typeof(VoiceOfTheFallenKing)] = 5500.0,
        [typeof(TheBeserkersMaul)] = 5500.0,
        [typeof(HatOfTheMagi)] = 5500.0,
        [typeof(BladeOfInsanity)] = 5500.0,
        [typeof(JackalsCollar)] = 5500.0,
        [typeof(SpiritOfTheTotem)] = 5500.0,

        // Artifacts
        [typeof(PendantOfTheMagi)] = 35.0,

        // Replicas
        [typeof(TatteredAncientMummyWrapping)] = 5000.0,
        [typeof(WindSpirit)] = 5000.0,
        // GauntletsOfAnger omitted — in omit-list
        [typeof(GladiatorsCollar)] = 5000.0,
        [typeof(OrcChieftainHelm)] = 5000.0,
        // ShroudOfDeceit omitted — in omit-list
        [typeof(AcidProofRobe)] = 5000.0,
        [typeof(ANecromancerShroud)] = 5000.0,
        [typeof(CaptainJohnsHat)] = 5000.0,
        [typeof(CrownOfTalKeesh)] = 5000.0,
        [typeof(DetectiveBoots)] = 5000.0,
        [typeof(EmbroideredOakLeafCloak)] = 5000.0,
        // JadeArmband omitted — in omit-list
        [typeof(LieutenantOfTheBritannianRoyalGuard)] = 5000.0,
        // MagicalDoor omitted — in omit-list
        // RoyalGuardInvestigatorsCloak omitted — in omit-list
        [typeof(SamaritanRobe)] = 5000.0,
        [typeof(TheMostKnowledgePerson)] = 5000.0,
        [typeof(TheRobeOfBritanniaAri)] = 5000.0,
        [typeof(DjinnisRing)] = 5000.0,
        [typeof(BraveKnightOfTheBritannia)] = 5000.0,
        [typeof(Calm)] = 5000.0,
        [typeof(FangOfRactus)] = 5000.0,
        [typeof(OblivionsNeedle)] = 5000.0,
        [typeof(Pacify)] = 5000.0,
        [typeof(Quell)] = 5000.0,
        [typeof(RoyalGuardSurvivalKnife)] = 5000.0,
        [typeof(Subdue)] = 5000.0,
        // Asclepius omitted — in omit-list
        // BracersofAlchemicalDevastation omitted — in omit-list
        // GargishAsclepius omitted — in omit-list
        // GargishBracersofAlchemicalDevastation omitted — in omit-list
        // HygieiasAmulet omitted — in omit-list
        // ScrollofValiantCommendation omitted — in omit-list

        // Easter
        [typeof(EasterEggs)] = 2.0,
        [typeof(JellyBeans)] = 1.0,

        // Miscellaneous
        // ParrotItem omitted — in omit-list
        [typeof(Gold)] = 0.01,
        [typeof(RedScales)] = 0.10,
        [typeof(YellowScales)] = 0.10,
        [typeof(BlackScales)] = 0.10,
        [typeof(GreenScales)] = 0.10,
        [typeof(WhiteScales)] = 0.10,
        [typeof(BlueScales)] = 0.10,
        [typeof(Bottle)] = 0.25,
        [typeof(OrcishKinMask)] = 100.0,
        [typeof(PottedPlantDeed)] = 15000.0,
        [typeof(BagOfSending)] = 250.0,
        [typeof(Cauldron)] = 200.0,
        [typeof(ChampionSkull)] = 1000.0,
        [typeof(ClockworkAssembly)] = 50.0,
        // ConjurersTrinket omitted — in omit-list
        // CorgulsHandbookOnMysticism omitted — in omit-list
        // CrownOfArcaneTemperament omitted — in omit-list
        [typeof(DeadWood)] = 1.0,
        // DustyPillow omitted — in omit-list
        [typeof(EndlessDecanter)] = 10.0,
        [typeof(EternallyCorruptTree)] = 1000.0,
        [typeof(ExcellentIronMaiden)] = 50.0,
        [typeof(ExecutionersCap)] = 1.0,
        // Flowstone omitted — in omit-list
        [typeof(GlacialStaff)] = 500.0,
        // GrapeVine omitted — in omit-list
        [typeof(GrobusFur)] = 20.0,
        [typeof(HorseShoes)] = 200.0,
        [typeof(JocklesQuicksword)] = 2.0,
        [typeof(MangledHeadOfDreadhorn)] = 1000.0,
        // MedusaBlood omitted — in omit-list
        // MedusaDarkScales omitted — in omit-list
        // MedusaLightScales omitted — in omit-list
        [typeof(ContestMiniHouseDeed)] = 6500.0,
        // MysticsGuard omitted — not in Edge (additional omission)
        [typeof(PowerCrystal)] = 100.0,
        [typeof(PristineDreadHorn)] = 1000.0,
        [typeof(ProspectorsTool)] = 3.0,
        [typeof(RecipeScroll)] = 10.0,
        [typeof(SwampTile)] = 5000.0,
        // TastyTreat omitted — in omit-list
        // TatteredAncientScroll omitted — in omit-list
        [typeof(ThorvaldsMedallion)] = 250.0,
        [typeof(TribalBerry)] = 10.0,
        [typeof(TunicOfGuarding)] = 2.0,
        // UndeadGargHorn omitted — in omit-list
        // UntranslatedAncientTome omitted — in omit-list
        [typeof(WallBlood)] = 5000.0,
        [typeof(Whip)] = 200.0,
        // BalmOfSwiftness omitted — in omit-list
        [typeof(TaintedMushroom)] = 1000.0,
        // GoldenSkull omitted — not in Edge (additional omission)
        [typeof(RedSoulstone)] = 15000.0,
        [typeof(BlueSoulstone)] = 15000.0,
        [typeof(SoulStone)] = 15000.0,
        // HornOfPlenty omitted — in omit-list
        // KepetchWax omitted — in omit-list
        // SlithEye omitted — in omit-list
        [typeof(SoulstoneFragment)] = 500.0,
        [typeof(WhiteClothDyeTub)] = 300.0,
        // Lodestone omitted — in omit-list
        // FeyWings omitted — in omit-list
        [typeof(StoutWhip)] = 3.0,
        // PlantClippings omitted — in omit-list
        // BasketOfRolls omitted — in omit-list
        // Yeast omitted — in omit-list
        [typeof(ValentinesCard)] = 50.0,
        // MetallicClothDyeTub omitted — in omit-list

        // Treasure Hunting
        [typeof(Lockpick)] = 0.10,
    };

    public CleanUpBritanniaData() : base("CleanUpBritannia", 12)
    {
    }

    public static void Configure()
    {
        Enabled = ServerConfiguration.GetSetting("cleanupbritannia.enabled", false);

        if (Enabled)
        {
            Instance = new CleanUpBritanniaData();
        }
    }

    public static void Enable()
    {
        if (Enabled)
        {
            return;
        }

        Instance ??= new CleanUpBritanniaData();
        Instance.Register();
        Enabled = true;
        ServerConfiguration.SetSetting("cleanupbritannia.enabled", true);
    }

    public static void Disable()
    {
        if (!Enabled)
        {
            return;
        }

        Instance?.Unregister();
        Enabled = false;
        ServerConfiguration.SetSetting("cleanupbritannia.enabled", false);
    }

    public static double GetPoints(Item item)
    {
        // CUB-DEFER: IVvVItem branch — interface not present in Edge (backlog: add VvV system)
        // if (item is IVvVItem vvv && vvv.IsVvVItem) { return 0; }

        var type = item.GetType();

        if (_entries.TryGetValue(type, out var points))
        {
            if (item is SOS { IsAncient: true })
            {
                points = 2500;
            }

            if (item.Stackable)
            {
                points *= item.Amount;
            }

            return points;
        }

        switch (item)
        {
            case RunicHammer hammer:
                return RunicHammerPoints(hammer);
            case RunicSewingKit sewing:
                return RunicSewingPoints(sewing);
            case PowerScroll ps:
                return ps.Value switch
                {
                    105 => 50,
                    110 => 100,
                    115 => 500,
                    120 => 2500,
                    _ => 0
                };
            case ScrollofTranscendence sot:
                return sot.Value / 0.1 * 2;
            // CUB-DEFER: Bait branch — Bait type not present in Edge (backlog: add fishing bait system)
            case TreasureMap tmap:
                return tmap.Level switch
                {
                    1 => 100,
                    2 => 250,
                    3 => 750,
                    4 => 1000,
                    _ => 50
                };
            case MonsterStatuette ms when ms.Type == MonsterStatuetteType.Slime:
                return 5000;
            case BasePigmentsOfTokuno pigments:
                return 500 * pigments.UsesRemaining;
        }

        // CUB-DEFER: ICombatEquipment/GetPointsForEquipment branch — deferred; Edge has no Imbuing.GetTotalWeight (backlog: add imbuing weight calc)

        // Shipwrecked floor: items not in table and not matching a special case score 100 if shipwrecked.
        if (item.LootType != LootType.Blessed && item is IShipwreckedItem { IsShipwreckedItem: true })
        {
            return 100;
        }

        return 0;
    }

    private static double RunicHammerPoints(RunicHammer hammer) =>
        hammer.Resource switch
        {
            CraftResource.DullCopper => 5 * hammer.UsesRemaining,
            CraftResource.ShadowIron => 10 * hammer.UsesRemaining,
            CraftResource.Copper => 25 * hammer.UsesRemaining,
            CraftResource.Bronze => 100 * hammer.UsesRemaining,
            CraftResource.Gold => 250 * hammer.UsesRemaining,
            CraftResource.Agapite => 1000 * hammer.UsesRemaining,
            CraftResource.Verite => 4000 * hammer.UsesRemaining,
            CraftResource.Valorite => 8000 * hammer.UsesRemaining,
            _ => 0
        };

    private static double RunicSewingPoints(RunicSewingKit sewing) =>
        sewing.Resource switch
        {
            CraftResource.SpinedLeather => 10 * sewing.UsesRemaining,
            CraftResource.HornedLeather => 100 * sewing.UsesRemaining,
            CraftResource.BarbedLeather => 400 * sewing.UsesRemaining,
            _ => 0
        };

    public override void SendMessage(PlayerMobile from, double old, double points, bool quest)
    {
        // Your Clean Up Britannia point total is now ~1_VALUE~!
        from.SendLocalizedMessage(1151281, $"{(int)GetPoints(from)}");
    }

    public override void Serialize(IGenericWriter writer)
    {
        base.Serialize(writer);
        writer.Write(0); // version
    }

    public override void Deserialize(IGenericReader reader)
    {
        base.Deserialize(reader);
        reader.ReadInt(); // version
    }
}
