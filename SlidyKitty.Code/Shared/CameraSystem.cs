using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Physics;
using SlidyKitty.Code.Player;

namespace SlidyKitty.Code.Shared;

internal class CameraSystem : EntityProcessingSystem
{
    private readonly OrthographicCamera _camera;

    private Vector2 _positionToTrack;
    private ComponentMapper<RigidBodyComponent> _rigidBodyMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public CameraSystem(OrthographicCamera camera) : base(Aspect.All(
        typeof(PlayerComponent),
        typeof(RigidBodyComponent),
        typeof(Transform2)))
    {
        _camera = camera;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Get component mapper services
        _rigidBodyMapper = mapperService.GetMapper<RigidBodyComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();

        // Set zoom limits just in case
        _camera.MinimumZoom = 0.5f; // Restrict zoom out to 50%
        _camera.MaximumZoom = 2f;   // Restrict zoom in to 200%

        // Initialize the position to track to the camera's current position
        _positionToTrack = _camera.Position;
    }

    public override void Process(GameTime gameTime, int entityId)
    {
        // Get the components for the player entity
        var rigidBodyComponent = _rigidBodyMapper.Get(entityId);
        var transformComponent = _transformMapper.Get(entityId);

        // Interpolate the camera position for smooth movement between position
        // changes, using a simple linear interpolation (lerp). This gives us a
        // kind of an acceleration/deccelaration effect for the camera movement
        // as it tracks towards the players position.        
        _positionToTrack = Vector2.Lerp(_positionToTrack, transformComponent.Position, 0.25f);

        // Update camera position to follow player
        _camera.LookAt(transformComponent.Position);

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
