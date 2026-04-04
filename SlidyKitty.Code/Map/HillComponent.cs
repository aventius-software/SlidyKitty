using System.Collections.Generic;

namespace SlidyKitty.Code.Map;

internal class HillComponent
{
    /// <summary>
    /// The ending angle of the hill, this is used to make sure 
    /// that consecutive hills connect smoothly
    /// </summary>
    public float EndAngle;

    /// <summary>
    /// Defines the height of the hill, this is used to determine how high 
    /// the hill should be drawn and how far down the segments should be placed
    /// </summary>
    public int Height;

    /// <summary>
    /// The total width of the hill, this is calculated as the number of 
    /// segments multiplied by the segment width
    /// </summary>
    public int HillWidth => NumberOfSegments * SegmentWidth;

    /// <summary>
    /// The total number of segments that make up this hill
    /// </summary>
    public int NumberOfSegments => Segments.Count;

    /// <summary>
    /// Array of segments that make up this hill, each segment has a (relative)
    /// start and end position (or offset) and a physics fixture which attached
    /// to the main hill physics 'body' so we can check for collisions with the 
    /// player and other physics objects. In other words, the player will 'slide'
    /// over the segments of the hill and we use the physics fixtures to determine 
    /// player speed and ground friction etc... ;-)
    /// </summary>
    public List<HillSegment> Segments = [];

    /// <summary>
    /// Width of an individual segment of the hill, this is used to determine 
    /// how far apart the segments are spaced and how wide the hill is 
    /// overall (i.e. number of segments * segment width)
    /// </summary>
    public int SegmentWidth;
}
