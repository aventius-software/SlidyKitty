using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Physics;
using SlidyKitty.Code.Player;

namespace SlidyKitty.Code.Shared;

internal class CameraSystem : EntityProcessingSystem
{
    private readonly OrthographicCamera _camera;
    private readonly GraphicsDevice _graphicsDevice;

    private int _cameraOffsetX = 0;
    private ComponentMapper<RigidBodyComponent> _rigidBodyMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public CameraSystem(OrthographicCamera camera, GraphicsDevice graphicsDevice) : base(Aspect.All(
        typeof(PlayerComponent),
        typeof(RigidBodyComponent),
        typeof(Transform2)))
    {
        _camera = camera;
        _graphicsDevice = graphicsDevice;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Get component mapper services
        _rigidBodyMapper = mapperService.GetMapper<RigidBodyComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();

        // Set zoom limits just in case
        _camera.MinimumZoom = 0.5f; // Restrict zoom out to 50%
        _camera.MaximumZoom = 2f;   // Restrict zoom in to 200%

        // Set initial camera position to be centered on the player with an offset
        // so that the player is not exactly in the center of the screen, but slightly
        // to the left. Basically about 1/4 of the screen width to the right of the
        // player
        _cameraOffsetX = _graphicsDevice.Viewport.Width / 4;
    }

    public override void Process(GameTime gameTime, int entityId)
    {
        // Get the components for the player entity
        var rigidBodyComponent = _rigidBodyMapper.Get(entityId);
        var transformComponent = _transformMapper.Get(entityId);

        // Update camera position to follow player
        _camera.LookAt(new Vector2(transformComponent.Position.X + _cameraOffsetX, transformComponent.Position.Y));

        // Smooth zoom based on player speed
        float minZoomIn = 1.5f; // Closest zoom in (default, when moving slowest)
        float maxZoomOut = 1f;  // Furthest zoom out (when moving fastest)
        float minSpeed = 0.0f;  // Speed at which zoom is minZoom
        float maxSpeed = 10.0f; // Speed at which zoom is maxZoom

        float speed = rigidBodyComponent.Body.LinearVelocity.Length();
        float dt = MathHelper.Clamp((speed - minSpeed) / (maxSpeed - minSpeed), 0f, 1f);
        float targetZoom = MathHelper.Lerp(minZoomIn, maxZoomOut, dt);

        // Limit the maximum zoom change per second
        float maxZoomChangePerSecond = 0.25f;
        float maxZoomChange = maxZoomChangePerSecond * gameTime.GetElapsedSeconds();

        // Calculate the difference and clamp it
        float zoomDelta = targetZoom - _camera.Zoom;
        zoomDelta = MathHelper.Clamp(zoomDelta, -maxZoomChange, maxZoomChange);

        _camera.Zoom += zoomDelta;
    }
}
