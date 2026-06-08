using System;
using Server;
using Server.Engines.Astronomy;
using Xunit;

namespace UOContent.Tests;

public class ConstellationInfoTests
{
    [Fact]
    public void Serialize_RoundTrips_AllFields()
    {
        var stars = new[]
        {
            new ConstellationInfo.StarPosition { ImageID = 0x668, X = 200, Y = 180 },
            new ConstellationInfo.StarPosition { ImageID = 0x670, X = 300, Y = 250 }
        };

        var original = new ConstellationInfo(TimeCoordinate.Midnight, 12, 45.6, stars)
        {
            Identifier = 7,
            Name = "Test Constellation"
        };

        var writer = new BufferWriter(true);
        original.Serialize(writer);

        var buffer = new byte[writer.Position];
        writer.Buffer.AsSpan(0, (int)writer.Position).CopyTo(buffer);

        var reader = new BufferReader(buffer);
        var loaded = new ConstellationInfo(reader);

        Assert.Equal(original.Identifier, loaded.Identifier);
        Assert.Equal(original.TimeCoordinate, loaded.TimeCoordinate);
        Assert.Equal(original.CoordRA, loaded.CoordRA);
        Assert.Equal(original.CoordDEC, loaded.CoordDEC);
        Assert.Equal(original.Name, loaded.Name);
        Assert.Equal(stars.Length, loaded.StarPositions.Length);
        Assert.Equal(stars[1].ImageID, loaded.StarPositions[1].ImageID);
        Assert.Equal(buffer.Length, reader.Position);
    }
}
