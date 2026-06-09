using System;
using Server;
using Server.Engines.Astronomy;
using Xunit;

namespace UOContent.Tests;

public class AstronomySystemTests
{
    [Fact]
    public void Persistence_RoundTrips_ConstellationList()
    {
        AstronomySystem.ResetForTest();
        AstronomySystem.GenerateForTest(5);
        var expectedCount = AstronomySystem.Constellations.Count;
        Assert.Equal(5, expectedCount);

        var instance = AstronomySystem.InstanceForTest;

        var writer = new BufferWriter(true);
        instance.Serialize(writer);

        var buffer = new byte[writer.Position];
        writer.Buffer.AsSpan(0, (int)writer.Position).CopyTo(buffer);

        AstronomySystem.ResetForTest();
        Assert.Empty(AstronomySystem.Constellations);

        var reader = new BufferReader(buffer);
        instance.Deserialize(reader);

        Assert.Equal(expectedCount, AstronomySystem.Constellations.Count);
        Assert.Equal(expectedCount, AstronomySystem.LoadedConstellations);
        Assert.Equal(buffer.Length, reader.Position);
    }
}
