using Microsoft.Xna.Framework;

namespace SlidyKitty.Code.Shared;

internal class OriginShiftService
{
    public Vector2 Shift => _shift;
    private Vector2 _shift = Vector2.Zero;

    public void SaveShift(Vector2 shift)
    {
        _shift = shift;
    }
}
