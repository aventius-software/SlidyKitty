using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Shared;
using System;

namespace SlidyKitty.Code.Physics;

internal class PhysicsSystem : EntityUpdateSystem
{
    private const float _gravity = 9.8f;
    private const float _minimumCharacterVelocity = 2.5f;
    private const float _pixelsPerMetre = 32f;
    private const float _slidePower = 4.5f;

    private readonly PhysicsService _physicsService;

    private ComponentMapper<CharacterComponent> _characterMapper = default!;
    private Vector2 _defaultGravity = new(0, _gravity);
    private ComponentMapper<RigidBodyComponent> _rigidBodyMapper = default!;
    private Vector2 _strongGravity = new(0, _gravity * _slidePower);
    private ComponentMapper<Transform2> _transformMapper = default!;

    public PhysicsSystem(PhysicsService physicsService) : base(Aspect.All(
        typeof(CharacterComponent),
        typeof(RigidBodyComponent),
        typeof(Transform2)))
    {
        _physicsService = physicsService;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Get component mappers
        _characterMapper = mapperService.GetMapper<CharacterComponent>();
        _rigidBodyMapper = mapperService.GetMapper<RigidBodyComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();

        // For this 'simulation' we'll start with normal earth gravity
        _physicsService.World.Gravity = _defaultGravity;

        // For the physics simulation to work correctly we need to indicate how many pixels
        // on the screen correspond to how many simulation units. So 'X' number of pixels
        // for 1 metre in the physics simulation.
        _physicsService.SetDisplayUnitToSimUnitRatio(_pixelsPerMetre);
    }

    public override void Update(GameTime gameTime)
    {
        // Update any physics based entities
        foreach (var entityId in ActiveEntities)
        {
            // Get components
            var characterComponent = _characterMapper.Get(entityId);
            var rigidBodyComponent = _rigidBodyMapper.Get(entityId);
            var transformComponent = _transformMapper.Get(entityId);

            if (characterComponent.IsInSwiftPose)
            {
                // If the character is in 'swift pose' we just amplify gravity
                // and they will fall faster and slide faster ;-)
                _physicsService.World.Gravity = _strongGravity;
            }
            else
            {
                // Otherwise gravity is set back to normal
                _physicsService.World.Gravity = _defaultGravity;
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

            // Set the transform according to the entities currenty physics status, this
            // can then be used later on by the sprite drawing system to draw the sprite
            // in the correct position and rotation
            transformComponent.Position = _physicsService.ToDisplayUnits(rigidBodyComponent.Body.Position);
            transformComponent.Rotation = rigidBodyComponent.Body.Rotation;
        }

        // Simulate our physics stuff ;-)
        _physicsService.World.Step(gameTime.GetElapsedSeconds());
    }
}
