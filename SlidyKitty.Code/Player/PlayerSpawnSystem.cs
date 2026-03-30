using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using SlidyKitty.Code.Physics;
using SlidyKitty.Code.Shared;

namespace SlidyKitty.Code.Player;

internal class PlayerSpawnSystem : EntitySystem
{
    private readonly ContentManager _contentManager;
    private readonly PhysicsService _physicsService;

    public PlayerSpawnSystem(ContentManager contentManager, PhysicsService physicsService) : base(Aspect.All(
        typeof(PlayerComponent)))
    {
        _contentManager = contentManager;
        _physicsService = physicsService;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Clear any existing player entities
        foreach (var entityId in ActiveEntities) DestroyEntity(entityId);

        // Load the sprite atlas for the player character
        var spriteAtlas = _contentManager.Load<Texture2DAtlas>("Characters/kitty-atlas");

        // Create the player entity and add the necessary components to it, such as a position
        // component, a sprite component, and a rigid body component for physics interactions.
        var entity = CreateEntity();
        entity.Attach(new CharacterComponent());
        entity.Attach(new PlayerComponent());
        entity.Attach(new RigidBodyComponent());
        entity.Attach(new SpriteComponent());
        entity.Attach(new Transform2());

        // Add the sprite component        
        var spriteComponent = entity.Get<SpriteComponent>();
        spriteComponent.Sprite = spriteAtlas.CreateSprite("kitty/001");

        // Create the physics body for the player
        var body = _physicsService.World.CreateBody(new Vector2(0, -5), 0, BodyType.Dynamic);
        body.Mass = 1f;
        body.FixedRotation = true;

        _ = body.CreateCircle(
            radius: _physicsService.ToSimUnits(spriteComponent.Sprite.Size.X / 2),            
            density: 1f,
            offset: Vector2.Zero);

        var rigidBodyComponent = entity.Get<RigidBodyComponent>();
        rigidBodyComponent.Body = body;
    }
}
