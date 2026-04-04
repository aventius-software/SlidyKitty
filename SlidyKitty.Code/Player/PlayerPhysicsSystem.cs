using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Physics;
using SlidyKitty.Code.Shared;
using System;

namespace SlidyKitty.Code.Player;

internal class PlayerPhysicsSystem : EntityUpdateSystem
{    
    private const float _minimumCharacterVelocity = 2.5f;    
    private const float _slidePower = 6.5f;

    private readonly PhysicsService _physicsService;

    private ComponentMapper<CharacterComponent> _characterMapper = default!;
    private Vector2 _originalGravity;
    private ComponentMapper<RigidBodyComponent> _rigidBodyMapper = default!;

    public PlayerPhysicsSystem(PhysicsService physicsService) : base(Aspect.All(
        typeof(CharacterComponent),
        typeof(RigidBodyComponent)))
    {
        _physicsService = physicsService;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Get component mappers
        _characterMapper = mapperService.GetMapper<CharacterComponent>();
        _rigidBodyMapper = mapperService.GetMapper<RigidBodyComponent>();        

        // Save gravity so that we can reset it back to normal when the character is not in 'swift pose'
        _originalGravity = _physicsService.World.Gravity;
    }

    public override void Update(GameTime gameTime)
    {
        // Update any physics based entities
        foreach (var entityId in ActiveEntities)
        {
            // Get components
            var characterComponent = _characterMapper.Get(entityId);
            var rigidBodyComponent = _rigidBodyMapper.Get(entityId);            
            
            if (characterComponent.IsInSwiftPose)
            {
                // If the character is in 'swift pose' we just amplify gravity
                // and they will fall faster and slide faster ;-)                
                _physicsService.World.Gravity = new Vector2(0, _originalGravity.Y * _slidePower);
            }
            else
            {
                // Otherwise gravity is set back to normal
                _physicsService.World.Gravity = new Vector2(0, _originalGravity.Y);
            }

            // Check for minimum velocity as we want the character to keep slowly moving forward
            // even if they are heading up hill, otherwise they'd fall back down the hill...
            var velocity = rigidBodyComponent.Body.LinearVelocity;

            // Set 'X' axis velocity to our minimum value
            if (velocity.X < _minimumCharacterVelocity)
                rigidBodyComponent.Body.LinearVelocity = new Vector2(_minimumCharacterVelocity, velocity.Y);

            // Set the rotation angle depending on velocity, so that the character tends
            // downwards when 'diving' and upwards when 'launching' ;-)
            rigidBodyComponent.Body.Rotation = MathF.Atan2(velocity.Y, velocity.X);            
        }
    }
}
