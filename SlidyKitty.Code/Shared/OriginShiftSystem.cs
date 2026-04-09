using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Physics;

namespace SlidyKitty.Code.Shared;

internal class OriginShiftSystem : EntityUpdateSystem
{
    private const float Threshold = 1500f;

    private readonly OrthographicCamera _camera;
    private readonly OriginShiftService _originShiftService;
    private readonly PhysicsService _physicsService;

    private ComponentMapper<RigidBodyComponent> _rigidBodyMapper = default!;

    public OriginShiftSystem(OrthographicCamera camera, OriginShiftService originShiftService, PhysicsService physicsService) : base(Aspect.All(
        typeof(RigidBodyComponent),
        typeof(Transform2)))
    {
        _camera = camera;
        _originShiftService = originShiftService;
        _physicsService = physicsService;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _rigidBodyMapper = mapperService.GetMapper<RigidBodyComponent>();
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
        _originShiftService.SetShift(new Vector2(Threshold, 0));

        if (_camera.Position.X > Threshold)
        {
            // Move the camera back towards the origin by the threshold amount. We're not bothered
            // by the camera being a bit off from the origin, we just want to make sure it doesn't get
            // too far away. So we can just move it back by the threshold amount, which will keep it
            // within a reasonable distance from the origin. This is also key for the terrain shader
            // to work correctly, as it uses the shift value to calculate how to repeat the noise
            // texture across the terrain. Essentially our terrain shader needs to  repeat the noise texture
            // every 'threshold' units across the terrain to look nice and not 'jump' whenever a shift happens
            var offset = new Vector2(-Threshold, 0);

            // Camera back nearer to zero
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
            }
        }
    }
}
