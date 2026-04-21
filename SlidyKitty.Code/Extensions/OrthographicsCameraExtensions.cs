using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace SlidyKitty.Code.Extensions;

internal static class OrthographicsCameraExtensions
{
    public static bool IsPositionOffCameraToTheLeft(this OrthographicCamera camera, Vector2 position, Vector2? parallaxFactor = null)
    {
        var factor = parallaxFactor ?? new Vector2(1, 1);
        return position.X < camera.BoundingRectangle.Left * factor.X;
    }
}
