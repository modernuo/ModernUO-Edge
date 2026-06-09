// Source: ServUO Scripts/Services/Astronomy/ConstellationInfo.cs
using System;
using ModernUO.Serialization;

namespace Server.Engines.Astronomy
{
    [PropertyObject]
    [SerializationGenerator(0)]
    public partial class ConstellationInfo
    {
        [SerializableField(0)]
        [SerializedCommandProperty(AccessLevel.GameMaster)]
        private int _identifier;

        [SerializableField(1)]
        [SerializedCommandProperty(AccessLevel.GameMaster)]
        private TimeCoordinate _timeCoordinate;

        [SerializableField(2)]
        [SerializedCommandProperty(AccessLevel.GameMaster)]
        private int _coordRA;

        [SerializableField(3)]
        [SerializedCommandProperty(AccessLevel.GameMaster)]
        private double _coordDEC;

        [SerializableField(4)]
        [SerializedCommandProperty(AccessLevel.GameMaster)]
        private string _name;

        [SerializableField(5)]
        [SerializedCommandProperty(AccessLevel.GameMaster)]
        private Mobile _discoveredBy;

        [SerializableField(6)]
        [SerializedCommandProperty(AccessLevel.GameMaster)]
        private DateTime _discoveredOn;

        [SerializableField(7, setter: "private")]
        private StarPosition[] _starPositions;

        [CommandProperty(AccessLevel.GameMaster)]
        public bool HasBeenDiscovered => _discoveredOn != DateTime.MinValue;

        public ConstellationInfo()
        {
        }

        public ConstellationInfo(TimeCoordinate p, int coordRA, double coordDEC, StarPosition[] starInfo)
        {
            _timeCoordinate = p;
            _coordRA = coordRA;
            _coordDEC = coordDEC;
            _starPositions = starInfo;
            _identifier = -1;
        }

        public override string ToString() => "...";

        public static StarPosition[] RandomStarPositions()
        {
            var amount = Utility.RandomMinMax(4, 7);
            var positions = new StarPosition[amount];

            for (var i = 0; i < amount; i++)
            {
                positions[i] = new StarPosition
                {
                    ImageID = Utility.RandomMinMax(0x668, 0x67D),
                    X = Utility.RandomMinMax(180, 450),
                    Y = Utility.RandomMinMax(150, 400)
                };
            }

            return positions;
        }
    }

    [SerializationGenerator(0)]
    public partial class StarPosition
    {
        [SerializableField(0)]
        private int _imageID;

        [SerializableField(1)]
        private int _x;

        [SerializableField(2)]
        private int _y;
    }
}
