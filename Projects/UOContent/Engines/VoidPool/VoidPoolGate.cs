using ModernUO.Serialization;
using Server.Items;

namespace Server.Engines.VoidPool;

[SerializationGenerator(0)]
public partial class VoidPoolGate : Static
{
    public VoidPoolGate() : base(0xF6C)
    {
        Light = LightType.Circle300;
    }

    [AfterDeserialization(false)]
    private void AfterDeserialization()
    {
        Delete();
    }
}
