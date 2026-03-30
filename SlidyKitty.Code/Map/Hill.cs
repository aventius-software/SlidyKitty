namespace SlidyKitty.Code.Map;

internal struct Hill
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
    /// Array of segments that make up this hill, each segment has a start 
    /// and end position and a physics body so we can check for collisions 
    /// with the player and other physics objects
    /// </summary>
    public HillSegment[] Segments;
}
