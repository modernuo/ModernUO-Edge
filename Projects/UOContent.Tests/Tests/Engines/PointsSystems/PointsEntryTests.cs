using System;
using Server;
using Server.Engines.Points;
using Xunit;

namespace UOContent.Tests;

public class PointsEntryTests
{
    [Fact]
    public void Serialize_RoundTrips_Points()
    {
        var entry = new PointsEntry(null, 1234.5);

        var writer = new BufferWriter(true);
        entry.Serialize(writer);

        var buffer = new byte[writer.Position];
        writer.Buffer.AsSpan(0, (int)writer.Position).CopyTo(buffer);

        var reader = new BufferReader(buffer);
        var loaded = new PointsEntry(null);
        loaded.Deserialize(reader);

        Assert.Equal(1234.5, loaded.Points);
        Assert.Equal(buffer.Length, reader.Position);
    }
}
