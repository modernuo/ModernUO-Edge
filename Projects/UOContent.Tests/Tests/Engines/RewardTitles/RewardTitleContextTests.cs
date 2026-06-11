using System;
using Server;
using Server.Engines.RewardTitles;
using Xunit;

namespace UOContent.Tests;

public class RewardTitleContextTests
{
    [Fact]
    public void Add_DedupesAndReports()
    {
        var ctx = new RewardTitleContext(null);

        Assert.True(ctx.Add(1158523));            // cliloc title "Astronomer"
        Assert.False(ctx.Add(1158523));           // duplicate cliloc -> false
        Assert.True(ctx.Add("Custom Title"));     // string title
        Assert.False(ctx.Add("Custom Title"));    // duplicate string -> false
        Assert.Equal(2, ctx.TitleList.Count);
    }

    [Fact]
    public void Select_ClampsAndHides()
    {
        var ctx = new RewardTitleContext(null);
        ctx.Add(1158523);

        Assert.Null(ctx.SelectedTitle);           // default -1 = hidden
        ctx.Select(0);
        Assert.Equal(1158523, ctx.SelectedTitle.Number);
        ctx.Select(-1);
        Assert.Null(ctx.SelectedTitle);           // hidden
        ctx.Select(99);                           // out of range -> ignored
        Assert.Null(ctx.SelectedTitle);
    }

    [Fact]
    public void Serialize_RoundTrips()
    {
        var ctx = new RewardTitleContext(null);
        ctx.Add(1158523);
        ctx.Add("Custom Title");
        ctx.Select(1);

        var writer = new BufferWriter(true);
        ctx.Serialize(writer);

        var buffer = new byte[writer.Position];
        writer.Buffer.AsSpan(0, (int)writer.Position).CopyTo(buffer);

        var reader = new BufferReader(buffer);
        var loaded = new RewardTitleContext(null);
        loaded.Deserialize(reader);

        Assert.Equal(2, loaded.TitleList.Count);
        Assert.Equal(1158523, loaded.TitleList[0].Number);
        Assert.Equal("Custom Title", loaded.TitleList[1].String);
        Assert.Equal("Custom Title", loaded.SelectedTitle.String);
        Assert.Equal(buffer.Length, reader.Position);
    }
}
