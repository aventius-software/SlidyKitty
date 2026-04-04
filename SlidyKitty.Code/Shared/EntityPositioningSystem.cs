using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Physics;

namespace SlidyKitty.Code.Shared;

internal class EntityPositioningSystem : EntityUpdateSystem
{
    private readonly PhysicsService _physicsService;

    private ComponentMapper<RigidBodyComponent> _rigidBodyMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public EntityPositioningSystem(PhysicsService physicsService) : base(Aspect.All(
        typeof(RigidBodyComponent),
        typeof(Transform2)))
    {
        _physicsService = physicsService;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Get component mappers        
        _rigidBodyMapper = mapperService.GetMapper<RigidBodyComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();        
    }

    public override void Update(GameTime gameTime)
    {
        // Update any physics based entities
        foreach (var entityId in ActiveEntities)
        {
            // Get components            
            var rigidBodyComponent = _rigidBodyMapper.Get(entityId);
            var transformComponent = _transformMapper.Get(entityId);
            
            // Set the transform according to the entities currenty physics status, this
            // can then be used later on by the sprite drawing system to draw the sprite
            // in the correct position and rotation
            transformComponent.Position = _physicsService.ToDisplayUnits(rigidBodyComponent.Body.Position);
            transformComponent.Rotation = rigidBodyComponent.Body.Rotation;
        }
    }
}
