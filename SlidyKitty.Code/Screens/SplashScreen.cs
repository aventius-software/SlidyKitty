using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace SlidyKitty.Code.Screens;

internal class SplashScreen : Screen
{
    private readonly ContentManager _contentManager;
    private readonly GamePlayScreen _gamePlayScreen;
    private readonly ScreenManager _screenManager;
    private readonly SpriteBatch _spriteBatch;

    private float _displayTimer = 1.5f;
    private SpriteFont _font = default!;
    private Texture2D _logo = default!;
    private float _scale = 0.5f;

    public SplashScreen(ContentManager contentManager, ScreenManager screenManager, SpriteBatch spriteBatch, GamePlayScreen gamePlayScreen)
    {
        _contentManager = contentManager;
        _screenManager = screenManager;
        _spriteBatch = spriteBatch;
        _gamePlayScreen = gamePlayScreen;
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);

        var width = 2 * _scale * (_logo.Width / 2);
        var height = 2 * _scale * (_logo.Height / 2);
        var x = _spriteBatch.GraphicsDevice.Viewport.Width / 2 - (width / 1);
        var y = _spriteBatch.GraphicsDevice.Viewport.Height / 2 - (height / 1);

        _spriteBatch.Begin();

        _spriteBatch.Draw(
            texture: _logo,
            position: new Vector2(x, y),
            sourceRectangle: null,
            color: Color.White,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: _scale * 2,
            effects: SpriteEffects.None,
            layerDepth: 0
        );

        var logoSubText = "presents";
        var logoSubTextSize = _font.MeasureString(logoSubText);
        var logoSubTextOrigin = new Vector2(logoSubTextSize.X / 2, logoSubTextSize.Y / 2);
        var logoSubTextPosition = new Vector2(
            _spriteBatch.GraphicsDevice.Viewport.Width / 2,
            _spriteBatch.GraphicsDevice.Viewport.Height / 2 + height + (45 * _scale));

        _spriteBatch.DrawString(
            spriteFont: _font,
            text: logoSubText,
            position: logoSubTextPosition,
            color: Color.White,
            rotation: 0f,
            origin: logoSubTextOrigin,
            scale: _scale * 4,
            effects: SpriteEffects.None,
            layerDepth: 0
        );

        _spriteBatch.End();
    }

    public override void LoadContent()
    {
        _font = _contentManager.Load<SpriteFont>("Splash/Font");
        _logo = _contentManager.Load<Texture2D>("Splash/Aventius");

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        var deltaTime = gameTime.GetElapsedSeconds();

        if (_displayTimer > 0) _displayTimer -= deltaTime / 2;
        else if (_scale > 0) _scale -= deltaTime / 2;
        else
        {
            // Create a fade transition (black, 0.5 seconds)
            var fadeTransition = new FadeTransition(_spriteBatch.GraphicsDevice, Color.Black, 0.5f);

            // Change to the GamePlayScreen with the fade transition
            _screenManager.ShowScreen(_gamePlayScreen, fadeTransition);
        }
    }
}
