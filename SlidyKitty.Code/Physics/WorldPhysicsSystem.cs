using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace SlidyKitty.Code.Physics;

internal class WorldPhysicsSystem : EntityUpdateSystem
{
    private const float _gravity = 9.8f;
    private const float _pixelsPerMetre = 32f;

    private readonly PhysicsService _physicsService;

    public WorldPhysicsSystem(PhysicsService physicsService) : base(Aspect.All(
        typeof(RigidBodyComponent),
        typeof(Transform2)))
    {
        _physicsService = physicsService;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // For this 'simulation' we'll start with normal earth gravity
        _physicsService.World.Gravity = new Vector2(0, _gravity);

        // For the physics simulation to work correctly we need to indicate how many pixels
        // on the screen correspond to how many simulation units. So 'X' number of pixels
        // for 1 metre in the physics simulation.
        _physicsService.SetDisplayUnitToSimUnitRatio(_pixelsPerMetre);
    }

    public override void Update(GameTime gameTime)
    {
        // Simulate our physics stuff ;-)
        _physicsService.World.Step(gameTime.GetElapsedSeconds());
    }
}
