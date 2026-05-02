using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using SlidyKitty.Code.Shared;

namespace SlidyKitty.Code.Screens;

internal class TitleScreen : Screen
{
    private const string GameName = "Slidy Kitty";
    private const float Gravity = 3200f;
    private const float JumpStrength = -1400f;
    
    private readonly ContentManager _contentManager;
    private readonly GamePlayScreen _gamePlayScreen;
    private readonly InputService _inputService;
    private readonly ScreenManager _screenManager;
    private readonly SpriteBatch _spriteBatch;

    private Texture2D _catTexture = default!;
    private SpriteFont _font = default!;
    private Vector2 _gameNameTextOrigin;
    private Vector2 _gameNameTextPosition;
    private Vector2 _gameNameTextSize;
    private Vector2 _position = new(200, 400);
    private Vector2 _velocity = new(200, Gravity);

    public TitleScreen(ScreenManager screenManager, SpriteBatch spriteBatch, InputService inputService, GamePlayScreen gamePlayScreen, ContentManager contentManager)
    {
        _screenManager = screenManager;
        _spriteBatch = spriteBatch;
        _inputService = inputService;
        _gamePlayScreen = gamePlayScreen;
        _contentManager = contentManager;
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw bouncing kitty
        _spriteBatch.Draw(_catTexture, _position, Color.White);

        // Draw title text
        _spriteBatch.DrawString(
            _font,
            GameName,
            position: _gameNameTextPosition,
            color: Color.White,
            rotation: 0f,
            origin: _gameNameTextOrigin,
            scale: 9.0f,
            effects: SpriteEffects.None,
            layerDepth: 1
        );

        var subText = "Hold slide button or screen to slide fast";
        var subTextSize = _font.MeasureString(subText);
        var subTextOrigin = new Vector2(subTextSize.X / 2, subTextSize.Y / 2);
        var subTextPosition = new Vector2(_gameNameTextPosition.X, _gameNameTextPosition.Y + 250);

        _spriteBatch.DrawString(
            _font,
            subText,
            position: subTextPosition,
            color: Color.White,
            rotation: 0f,
            origin: subTextOrigin,
            scale: 2.8f,
            effects: SpriteEffects.None,
            layerDepth: 1
        );

        var zsubText = "Longer touch = higher jump";
        var zsubTextSize = _font.MeasureString(zsubText);
        var zsubTextOrigin = new Vector2(zsubTextSize.X / 2, zsubTextSize.Y / 2);
        var zsubTextPosition = new Vector2(_gameNameTextPosition.X, subTextPosition.Y + 75);

        _spriteBatch.DrawString(
            _font,
            zsubText,
            position: zsubTextPosition,
            color: Color.White,
            rotation: 0f,
            origin: zsubTextOrigin,
            scale: 2.8f,
            effects: SpriteEffects.None,
            layerDepth: 1
        );

        _spriteBatch.End();
    }

    public override void LoadContent()
    {
        _font = _contentManager.Load<SpriteFont>("Title/Font");
        _catTexture = _contentManager.Load<Texture2D>("Characters/Kitty");
        
        // Setup string variable stuff
        _gameNameTextSize = _font.MeasureString(GameName);
        _gameNameTextOrigin = new Vector2(_gameNameTextSize.X / 2, _gameNameTextSize.Y / 2);
        _gameNameTextPosition = new Vector2(
            _spriteBatch.GraphicsDevice.Viewport.Width / 2, 
            (_spriteBatch.GraphicsDevice.Viewport.Height / 2) - 100);

        base.LoadContent();
    }

    public override void Update(GameTime gameTime)
    {
        // Poll control system
        _inputService.Update();

        // Play game if slide control is pressed
        if (_inputService.SlideControlPressed)
        {
            // Create a fade transition (black, 0.5 seconds)
            var fadeTransition = new FadeTransition(_spriteBatch.GraphicsDevice, Color.Black, 0.5f);

            _screenManager.ReplaceScreen(_gamePlayScreen, fadeTransition);
        }

        // Bounce our kitty around on the title screen
        var deltaTime = gameTime.GetElapsedSeconds();

        // Sort out frame rate independent movement
        if (_velocity.Y < 0 && _velocity.Y + (Gravity * deltaTime) >= 0)
        {
            float t = _velocity.Y / Gravity;
            float y = (-0.5f * Gravity * (t * t)) + (_velocity.Y * t);

            _velocity = new Vector2(_velocity.X, -y);
            _position += new Vector2(_velocity.X * deltaTime, _velocity.Y);
            _velocity += new Vector2(0, Gravity * deltaTime);
        }
        else
        {
            _velocity += new Vector2(0, Gravity * deltaTime * 0.5f);
            _position += _velocity * deltaTime;
            _velocity += new Vector2(0, Gravity * deltaTime * 0.5f);
        }

        // Has the kitty bounced out of the horizontal bounds?
        if (_position.X + _catTexture.Width > _spriteBatch.GraphicsDevice.Viewport.Width)
        {
            _position = new Vector2(_spriteBatch.GraphicsDevice.Viewport.Width - _catTexture.Width, _position.Y);
            _velocity = new Vector2(_velocity.X * -1, _velocity.Y);
        }
        else if (_position.X < 0)
        {
            _position = new Vector2(0, _position.Y);
            _velocity = new Vector2(_velocity.X * -1, _velocity.Y);
        }

        // Has the kitty reached the 'floor' (which in this case is the top of the title text)?
        if (_position.Y + _catTexture.Height > _gameNameTextPosition.Y)
        {
            _position = new Vector2(_position.X, _gameNameTextPosition.Y - _catTexture.Height);
            _velocity += new Vector2(0, JumpStrength);
        }
    }
}
