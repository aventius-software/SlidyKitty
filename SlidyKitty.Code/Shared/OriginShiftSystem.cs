using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Physics;

namespace SlidyKitty.Code.Shared;

internal class OriginShiftSystem : EntityUpdateSystem
{
    private const float Threshold = 1000f;

    private readonly OrthographicCamera _camera;
    private readonly PhysicsService _physicsService;

    private ComponentMapper<RigidBodyComponent> _rigidBodyMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public OriginShiftSystem(OrthographicCamera camera, PhysicsService physicsService) : base(Aspect.All(
        typeof(RigidBodyComponent),
        typeof(Transform2)))
    {
        _camera = camera;
        _physicsService = physicsService;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _rigidBodyMapper = mapperService.GetMapper<RigidBodyComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();
    }

    public override void Update(GameTime gameTime)
    {
        // If the camera has moved too far from the origin, we need to shift
        // everything back towards the origin. Since we're creating an infinite
        // scroller, the player will always be moving to the right, so we only
        // need to check the X position of the camera. If we didn't do this, the
        // camera position (and all game objects) would eventually be enormously
        // far from the origin which would or could cause all sorts of weird
        // issues with rendering and physics.
        if (_camera.Position.X > Threshold)
        {
            // Calculate how much we need to shift the world back towards the origin
            var offset = new Vector2(-_camera.Position.X, 0);

            // Camera back near zero
            _camera.Move(offset);

            // Shift everything in the world. Since we're using physics, we need to shift
            // the physics bodies and the transform components separately, otherwise the
            // physics bodies would be in the wrong place compared to the transform components
            // which would cause all sorts of weird issues! We are later using another ECS
            // system to update the transform components based on the physics bodies, but 
            // we're shifting both here to make sure they stay in sync and we don't end up
            // with any weird issues where the physics bodies are in the wrong place compared
            // to the transform components.
            foreach (var entityId in ActiveEntities)
            {
                var rigidBodyComponent = _rigidBodyMapper.Get(entityId);
                rigidBodyComponent.Body.Position += _physicsService.ToSimUnits(offset);

                var transformComponent = _transformMapper.Get(entityId);
                transformComponent.Position += offset;
            }
        }
    }
}
