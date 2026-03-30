using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;

namespace SlidyKitty.Code.Map;

internal struct HillSegment
{
    /// <summary>
    /// The physics body for this segment, this is used to check for collisions 
    /// with the player and other physics objects
    /// </summary>
    public Body Body;

    /// <summary>
    /// The start and end position of this segment, this is used to draw the segment 
    /// and also to determine if the segment has gone off the left of the camera
    /// </summary>
    public Vector2 Start, End;
}
