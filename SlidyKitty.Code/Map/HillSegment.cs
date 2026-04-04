namespace SlidyKitty.Code.Map;

internal struct HillSegment
{
    /// <summary>
    /// The start and end Y offset of this segment, these are relative to the 
    /// position of the hill's transform component.
    /// </summary>
    public float StartYOffset, EndYOffset;
}
