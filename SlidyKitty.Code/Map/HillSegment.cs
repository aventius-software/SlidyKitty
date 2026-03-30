using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace SlidyKitty.Code.Map;

internal struct HillSegment
{
    public Vector2 Start, End;
    public Body Body;
}
