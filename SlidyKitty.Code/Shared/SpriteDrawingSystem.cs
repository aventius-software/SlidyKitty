using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;

namespace SlidyKitty.Code.Shared;

internal class SpriteDrawingSystem : EntityDrawSystem
{
    private readonly OrthographicCamera _camera;
    private readonly SpriteBatch _spriteBatch;

    private ComponentMapper<SpriteComponent> _spriteMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public SpriteDrawingSystem(SpriteBatch spriteBatch, OrthographicCamera camera) : base(Aspect.All(
        typeof(SpriteComponent),
        typeof(Transform2)))
    {
        _spriteBatch = spriteBatch;
        _camera = camera;
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(
            sortMode: SpriteSortMode.Immediate,
            blendState: null,
            samplerState: SamplerState.PointClamp,
            depthStencilState: null,
            rasterizerState: null,
            effect: null,
            transformMatrix: _camera.GetViewMatrix());

        foreach (var entityId in ActiveEntities)
        {
            // Get components
            var spriteComponent = _spriteMapper.Get(entityId);
            var transformComponent = _transformMapper.Get(entityId);

            // Draw the sprite using the transform component for position, rotation and scale
            _spriteBatch.Draw(spriteComponent.Sprite, transformComponent);
        }

        _spriteBatch.End();
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _spriteMapper = mapperService.GetMapper<SpriteComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();
    }

    public void Update(GameTime gameTime)
    {
        foreach (var entityId in ActiveEntities)
        {
            // Get references for our components            
            var spriteComponent = _spriteMapper.Get(entityId);

            // Update animation if its an animated sprite
            if (spriteComponent.Sprite is AnimatedSprite sprite)
                sprite.Update(gameTime);
        }
    }
}
