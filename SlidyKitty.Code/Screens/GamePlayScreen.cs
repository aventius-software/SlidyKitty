using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;
using SlidyKitty.Code.Map;
using SlidyKitty.Code.Physics;
using SlidyKitty.Code.Player;
using SlidyKitty.Code.Shared;

namespace SlidyKitty.Code.Screens;

internal class GamePlayScreen : Screen
{
    private readonly CameraSystem _cameraSystem;
    private readonly MapSystem _mapSystem;
    private readonly PauseScreen _pauseScreen;
    private readonly PhysicsService _physicsService;
    private readonly PhysicsSystem _physicsSystem;
    private readonly PlayerControlSystem _playerControlSystem;
    private readonly PlayerSpawnSystem _playerSpawnSystem;
    private readonly ScreenManager _screenManager;
    private readonly SpriteDrawingSystem _spriteDrawingSystem;

    private World? _world;

    public GamePlayScreen(
        CameraSystem cameraSystem,        
        MapSystem mapSystem,
        PauseScreen pauseScreen,
        PhysicsService physicsService,
        PhysicsSystem physicsSystem,
        PlayerControlSystem playerControlSystem,
        PlayerSpawnSystem playerSpawnSystem,
        ScreenManager screenManager,
        SpriteDrawingSystem spriteDrawingSystem)
    {
        _cameraSystem = cameraSystem;
        _mapSystem = mapSystem;
        _pauseScreen = pauseScreen;
        _physicsService = physicsService;
        _physicsSystem = physicsSystem;
        _playerControlSystem = playerControlSystem;
        _playerSpawnSystem = playerSpawnSystem;
        _screenManager = screenManager;
        _spriteDrawingSystem = spriteDrawingSystem;
    }

    public override void Draw(GameTime gameTime) => _world?.Draw(gameTime);

    public override void LoadContent()
    {        
        // Add systems        
        _world = new WorldBuilder()

            // Add the physics system first so it can initialise the physics world
            // and so that it updates the physics world before any other systems need to use it
            .AddSystem(_physicsSystem)
            .AddSystem(_mapSystem)

            // Add various initialisation systems, these will run once            
            .AddSystem(_playerSpawnSystem)
            
            // Add player control system
            .AddSystem(_playerControlSystem)

            // Add sprite drawing system last so it draws everything else on top of the map and player
            .AddSystem(_spriteDrawingSystem)

            // Add the camera system
            .AddSystem(_cameraSystem)

            // Build the ECS world ;-)
            .Build();

        base.LoadContent();
    }

    public override void UnloadContent()
    {
        // We need to reset the physics world and dispose of the ECS world when we unload the
        // content for this screen, otherwise when we come back to this screen the physics world
        // will still have all the bodies in it...
        _physicsService.ResetWorld();

        // ...and the ECS world will still have all the entities
        // in it which will cause all sorts of weird issues!
        _world?.Dispose();

        // Bye...
        base.UnloadContent();
    }

    public override void Update(GameTime gameTime)
    {
        // Update the world, this will run all the systems in the world which
        // will update the game state and do all the drawing
        _world?.Update(gameTime);

        // Check to see if the pause button was pressed, if it was we
        // show the pause screen (which will pause the game until it's closed)
        var keyboardState = KeyboardExtended.GetState();

        // Pause?
        if (keyboardState.WasKeyPressed(Keys.P))
        {
            // When the game is paused, we still draw the gameplay screen
            // but we don't update anything (i.e. paused ;-)
            DrawWhenInactive = true;
            UpdateWhenInactive = false;

            // Now show the pause screen...
            _screenManager.ShowScreen(_pauseScreen);
        }
    }
}
