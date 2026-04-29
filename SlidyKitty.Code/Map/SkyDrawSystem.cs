using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace SlidyKitty.Code.Map;

internal class SkyDrawSystem : DrawSystem
{
    private readonly ContentManager _contentManager;
    private readonly SpriteBatch _spriteBatch;

    private Texture2D _skyTexture = default!;    

    public SkyDrawSystem(ContentManager contentManager, SpriteBatch spriteBatch)
    {
        _contentManager = contentManager;
        _spriteBatch = spriteBatch;
    }

    public override void Initialize(World world)
    {
        // Load the sky texture, this will be drawn as a background behind all other entities in the game
        _skyTexture = _contentManager.Load<Texture2D>("Map/Sky");

        base.Initialize(world);
    }

    public override void Draw(GameTime gameTime)
    {
        // Create a rectangle the size of the viewport to draw the sky texture on, this
        // will ensure the sky texture fills the entire background of the game
        var destinationRectangle = new Rectangle(0, 0, _spriteBatch.GraphicsDevice.Viewport.Width, _spriteBatch.GraphicsDevice.Viewport.Height);
        
        // Draw the sky texture as a background, this will/should be drawn before all other entities in the game
        _spriteBatch.Begin();
        _spriteBatch.Draw(texture: _skyTexture, destinationRectangle: destinationRectangle, color: Color.White);
        _spriteBatch.End();
    }
}
