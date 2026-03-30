using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Shared;

namespace SlidyKitty.Code.Player;

internal class PlayerControlSystem : EntityProcessingSystem
{
    private ComponentMapper<CharacterComponent> _characterMapper = default!;
    
    public PlayerControlSystem() : base(Aspect.All(
        typeof(CharacterComponent),
        typeof(PlayerComponent))) { }

    public override void Initialize(IComponentMapperService mapperService)
    {
        _characterMapper = mapperService.GetMapper<CharacterComponent>();        
    }

    public override void Process(GameTime gameTime, int entityId)
    {
        // Get references for our components
        var characterComponent = _characterMapper.Get(entityId);

        // When the player presses space we want to go into 'swift' pose, to make
        // the player character fall faster and slide quicker ;-)
        characterComponent.IsInSwiftPose = Keyboard.GetState().IsKeyDown(Keys.Space);
    }    
}
