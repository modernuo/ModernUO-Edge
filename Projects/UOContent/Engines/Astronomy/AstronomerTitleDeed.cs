// Source: ServUO Scripts/Services/Astronomy/AstronomerTitleDeed.cs
using ModernUO.Serialization;

namespace Server.Items;

[SerializationGenerator(0)]
public partial class AstronomerTitleDeed : BaseRewardTitleDeed
{
    public override TextDefinition Title => 1158523; // Astronomer

    [Constructible]
    public AstronomerTitleDeed()
    {
    }
}
