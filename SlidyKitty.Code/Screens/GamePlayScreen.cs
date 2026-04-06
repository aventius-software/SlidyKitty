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
    private readonly EntityPositioningSystem _entityPositioningSystem;
    private readonly HillDrawSystem _hillDrawSystem;
    private readonly HillUpdateSystem _hillUpdateSystem;    
    private readonly OriginShiftSystem _originShiftSystem;
    private readonly PauseScreen _pauseScreen;
    private readonly PhysicsService _physicsService;    
    private readonly PlayerControlSystem _playerControlSystem;
    private readonly PlayerPhysicsSystem _playerPhysicsSystem;
    private readonly PlayerSpawnSystem _playerSpawnSystem;
    private readonly ScreenManager _screenManager;
    private readonly SpriteDrawingSystem _spriteDrawingSystem;
    private readonly WorldPhysicsSystem _worldPhysicsSystem;

    private World? _world;

    public GamePlayScreen(
        CameraSystem cameraSystem,
        EntityPositioningSystem entityPositioningSystem,
        HillDrawSystem hillDrawSystem,
        HillUpdateSystem hillUpdateSystem,        
        OriginShiftSystem originShiftSystem,
        PauseScreen pauseScreen,
        PhysicsService physicsService,        
        PlayerControlSystem playerControlSystem,
        PlayerPhysicsSystem playerPhysicsSystem,
        PlayerSpawnSystem playerSpawnSystem,
        ScreenManager screenManager,
        SpriteDrawingSystem spriteDrawingSystem,
        WorldPhysicsSystem worldPhysicsSystem)
    {
        _cameraSystem = cameraSystem;
        _entityPositioningSystem = entityPositioningSystem;
        _hillDrawSystem = hillDrawSystem;
        _hillUpdateSystem = hillUpdateSystem;        
        _originShiftSystem = originShiftSystem;
        _pauseScreen = pauseScreen;
        _physicsService = physicsService;        
        _playerControlSystem = playerControlSystem;
        _playerPhysicsSystem = playerPhysicsSystem;
        _playerSpawnSystem = playerSpawnSystem;
        _screenManager = screenManager;
        _spriteDrawingSystem = spriteDrawingSystem;
        _worldPhysicsSystem = worldPhysicsSystem;
    }

    public override void Draw(GameTime gameTime) => _world?.Draw(gameTime);

    public override void LoadContent()
    {        
        // Add systems        
        _world = new WorldBuilder()

            // Add our camera and origin shift systems
            .AddSystem(_cameraSystem)
            .AddSystem(_originShiftSystem)

            // Add our main game systems
            .AddSystem(_worldPhysicsSystem)
            .AddSystem(_playerPhysicsSystem)
            .AddSystem(_hillUpdateSystem)
            .AddSystem(_entityPositioningSystem)            
                        
            // Add various initialisation systems, these will run once            
            .AddSystem(_playerSpawnSystem)
            
            // Add player control system
            .AddSystem(_playerControlSystem)

            // Add our drawing systems, add sprite drawing system last so it
            // draws everything else on top of the background
            .AddSystem(_hillDrawSystem)
            .AddSystem(_spriteDrawingSystem)            

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
