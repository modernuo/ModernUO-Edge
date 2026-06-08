// Source: ServUO Scripts/Services/Astronomy/AstronomySystem.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Items;
using Server.Logging;

namespace Server.Engines.Astronomy
{
    public enum TimeCoordinate
    {
        FiveToEight,
        NineToEleven,
        Midnight,
        OneToFour,
        Day
    }

    public class AstronomySystem : GenericPersistence
    {
        private static readonly ILogger logger = LogFactory.GetLogger(typeof(AstronomySystem));

        // era gate (CLAUDE.md rule 11): Astronomy ~Pub 86; Core.TOL is the conservative modern-content flag
        public static bool Enabled = Core.TOL;

        public static readonly int MaxConstellations = 1000;
        public static readonly int MaxRA = 24;
        public static readonly double MaxDEC = 90;

        public static AstronomySystem Instance { get; private set; }

        public static int LoadedConstellations { get; set; }
        public static List<ConstellationInfo> Constellations { get; set; } = new();
        public static List<Tuple<int, int>> InterstellarObjects { get; set; } = new();
        public static List<int> DiscoveredConstellations { get; set; } = new();

        public AstronomySystem() : base("Astronomy", 30)
        {
        }

        public static void Configure()
        {
            if (!Enabled)
            {
                return;
            }

            Instance = new AstronomySystem();
            BuildInterstellarObjects();
        }

        public static void Initialize()
        {
            if (Enabled && LoadedConstellations < MaxConstellations)
            {
                CreateConstellations(MaxConstellations - LoadedConstellations);
            }
        }

        private static void BuildInterstellarObjects()
        {
            InterstellarObjects.Clear();

            for (var i = 0x68D; i <= 0x693; i++) { InterstellarObjects.Add(new Tuple<int, int>(i, 1158514)); } // comets
            for (var i = 0x69F; i <= 0x6A6; i++) { InterstellarObjects.Add(new Tuple<int, int>(i, 1158734)); } // felucca
            for (var i = 0x6A7; i <= 0x6AE; i++) { InterstellarObjects.Add(new Tuple<int, int>(i, 1158735)); } // trammel
            for (var i = 0x6AF; i <= 0x6BC; i++) { InterstellarObjects.Add(new Tuple<int, int>(i, 1158736)); } // galaxy
            for (var i = 0x6BD; i <= 0x6CD; i++) { InterstellarObjects.Add(new Tuple<int, int>(i, 1158737)); } // planet
        }

        private static void CreateConstellations(int amount)
        {
            var next = TimeCoordinate.FiveToEight;

            if (LoadedConstellations > 0)
            {
                if (Constellations.Where(c => c.TimeCoordinate == TimeCoordinate.FiveToEight).Count() > Constellations.Where(c => c.TimeCoordinate == TimeCoordinate.NineToEleven).Count())
                {
                    next = TimeCoordinate.NineToEleven;
                }
                else if (Constellations.Where(c => c.TimeCoordinate == TimeCoordinate.NineToEleven).Count() > Constellations.Where(c => c.TimeCoordinate == TimeCoordinate.Midnight).Count())
                {
                    next = TimeCoordinate.Midnight;
                }
                else if (Constellations.Where(c => c.TimeCoordinate == TimeCoordinate.Midnight).Count() > Constellations.Where(c => c.TimeCoordinate == TimeCoordinate.OneToFour).Count())
                {
                    next = TimeCoordinate.OneToFour;
                }
            }

            for (var i = 0; i < amount; i++)
            {
                int ra;
                double dec;

                do
                {
                    ra = Utility.RandomMinMax(0, MaxRA);
                    dec = Utility.RandomMinMax(0, (int)MaxDEC) + Utility.RandomList(.2, .4, .6, .8, .0);
                }
                while (CheckExists(next, ra, dec));

                var info = new ConstellationInfo(next, ra, dec, ConstellationInfo.RandomStarPositions());
                Constellations.Add(info);

                info.Identifier = Constellations.Count - 1;

                next = next switch
                {
                    TimeCoordinate.FiveToEight => TimeCoordinate.NineToEleven,
                    TimeCoordinate.NineToEleven => TimeCoordinate.Midnight,
                    TimeCoordinate.Midnight => TimeCoordinate.OneToFour,
                    _ => TimeCoordinate.FiveToEight
                };
            }

            LoadedConstellations = Constellations.Count;
        }

        public static void ResetConstellations()
        {
            Constellations.Clear();
            LoadedConstellations = 0;

            CreateConstellations(MaxConstellations);
            logger.Information("Reset Constellations!");
        }

        public static ConstellationInfo GetConstellation(int id)
        {
            return Constellations.FirstOrDefault(info => info.Identifier == id);
        }

        public static ConstellationInfo GetConstellation(TimeCoordinate p, int ra, double dec)
        {
            return Constellations.FirstOrDefault(c => c.TimeCoordinate == p && c.CoordRA == ra && c.CoordDEC == dec);
        }

        private static bool CheckExists(TimeCoordinate p, int ra, double dec)
        {
            return Constellations.Any(c => c.TimeCoordinate == p && c.CoordRA == ra && c.CoordDEC == dec);
        }

        public static bool CheckNameExists(string name)
        {
            return Constellations.Any(c => !string.IsNullOrEmpty(c.Name) && c.Name.ToLower() == name.ToLower());
        }

        public static TimeCoordinate GetTimeCoordinate(IEntity e)
        {
            Clock.GetTime(e.Map, e.X, e.Y, out var hours, out _, out _);

            if (hours >= 17 && hours < 21)
            {
                return TimeCoordinate.FiveToEight;
            }

            if (hours >= 21 && hours < 24)
            {
                return TimeCoordinate.NineToEleven;
            }

            if ((hours >= 24 && hours < 1) || hours == 0)
            {
                return TimeCoordinate.Midnight;
            }

            if (hours >= 1 && hours <= 4)
            {
                return TimeCoordinate.OneToFour;
            }

            return TimeCoordinate.Day;
        }

        public static int RandomSkyImage(Mobile m)
        {
            return RandomSkyImage(GetTimeCoordinate(m));
        }

        public static int RandomSkyImage(TimeCoordinate timeCoordinate)
        {
            return timeCoordinate switch
            {
                TimeCoordinate.FiveToEight => 0x67F,
                TimeCoordinate.NineToEleven => Utility.RandomMinMax(0x680, 0x682),
                TimeCoordinate.Midnight => 0x686,
                TimeCoordinate.OneToFour => Utility.RandomMinMax(0x683, 0x685),
                _ => 0x67E
            };
        }

        public static Tuple<int, int> GetRandomInterstellarObject()
        {
            return InterstellarObjects[Utility.Random(InterstellarObjects.Count)];
        }

        public static int TimeCoordinateLocalization(TimeCoordinate timeCoordinate)
        {
            return timeCoordinate switch
            {
                TimeCoordinate.NineToEleven => 1158507,
                TimeCoordinate.Midnight => 1158508,
                TimeCoordinate.OneToFour => 1158509,
                _ => 1158506
            };
        }

        public static void AddDiscovery(ConstellationInfo info)
        {
            if (!DiscoveredConstellations.Contains(info.Identifier))
            {
                DiscoveredConstellations.Add(info.Identifier);
            }
        }

        public override void Serialize(IGenericWriter writer)
        {
            writer.Write(0); // version
            writer.Write(Constellations.Count);

            foreach (var info in Constellations)
            {
                info.Serialize(writer);
            }
        }

        public override void Deserialize(IGenericReader reader)
        {
            reader.ReadInt(); // version
            LoadedConstellations = reader.ReadInt();

            for (var i = 0; i < LoadedConstellations; i++)
            {
                Constellations.Add(new ConstellationInfo(reader));
            }
        }

        // test-only hooks
        public static AstronomySystem InstanceForTest => Instance ??= new AstronomySystem();

        public static void ResetForTest()
        {
            Constellations.Clear();
            DiscoveredConstellations.Clear();
            LoadedConstellations = 0;

            if (InterstellarObjects.Count == 0)
            {
                BuildInterstellarObjects();
            }

            _ = InstanceForTest;
        }

        public static void GenerateForTest(int amount) => CreateConstellations(amount);
    }
}
