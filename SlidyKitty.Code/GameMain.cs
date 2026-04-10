using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;
using SlidyKitty.Code.Extensions;
using SlidyKitty.Code.Map;
using SlidyKitty.Code.Physics;
using SlidyKitty.Code.Screens;
using SlidyKitty.Code.Shared;
using System;
using System.Reflection;

namespace SlidyKitty.Code;

/// <summary>
/// Slidy Kitty is a cute Tiny Wings style game built using Monogame and Monogame.Extended. The 
/// code is open source and available on GitHub at https://github.com/aventius-software/SlidyKitty
/// </summary>
public class GameMain : Game
{
    // Set the 'virtual' resolution which we'll scale later to whatever screen size
    private const int _targetFps = 119;
    private const int _virtualResolutionWidth = 1920, _virtualResolutionHeight = 1080;

    private readonly GraphicsDeviceManager _graphics;
    private readonly ScreenManager _screenManager;

    private CustomRenderTarget _customRenderTarget = default!;
    private IServiceProvider _serviceProvider = default!;

    public GameMain()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        // Set initial video config
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;

        // Set fixed timestep
        IsFixedTimeStep = true;
        InactiveSleepTime = TimeSpan.Zero;

        // If we want a different target fps from the default (which in Monogame is 60), then
        // we need to set the target 'time elapsed' we want for the specified target fps        
        TargetElapsedTime = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond / _targetFps));

        // No vsync
        _graphics.SynchronizeWithVerticalRetrace = false;

        // Apply changes
        _graphics.ApplyChanges();

        // Add the Monogame.Extended screen manager as per normal...
        _screenManager = new ScreenManager();
        Components.Add(_screenManager);
    }

    private ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // The Monogame Extended screen manager needs the actual 'Game' object, so
        // we'll register it here so we can inject it later on
        services.AddSingleton<Game>(this);

        // We're just going to use a single content manager and inject it as a service
        // whenever we need it. Since we won't use multiple content managers, this way
        // it will cache assets and we just write 'load' as normal to load textures for
        // example and if we load an existing texture, we'll get the cached version ;-)
        services.AddSingleton(Content);

        // Register the graphics device too, so we can also register sprite batch
        // and the container will inject the graphics device for us ;-)
        services.AddSingleton(GraphicsDevice);

        // We're ONLY going to use a single sprite batch, so we register here and then
        // we can inject the SpriteBatch service into various other classes eventually
        // via constructor injection
        services.AddSingleton<SpriteBatch>();

        // Add the Monogame Extended screen manager too
        services.AddSingleton(_screenManager);

        // We'll add our custom render target service so we can use our virtual resolution
        // but scale correctly to all different screen sizes easily        
        services.AddSingleton<CustomRenderTarget>(options =>
        {
            var service = new CustomRenderTarget(GraphicsDevice, options.GetRequiredService<SpriteBatch>());
            service.InitialiseRenderDestination(_virtualResolutionWidth, _virtualResolutionHeight);            

            return service;
        });

        // Setup our camera and viewport, see link to documentation below
        // https://www.monogameextended.net/docs/features/camera/orthographic-camera/
        services.AddSingleton<OrthographicCamera>(options =>
        {
            // Setup a viewport adapter to handle different screen sizes/aspect ratios
            var viewportAdapter = new BoxingViewportAdapter(
                Window,
                GraphicsDevice,
                _virtualResolutionWidth,
                _virtualResolutionHeight);

            return new OrthographicCamera(viewportAdapter);
        });

        // Add core game services
        services.AddSingleton<PhysicsService>();
        services.AddSingleton<ShapeDrawingService>();
        services.AddSingleton<HillFactory>();
        services.AddSingleton<OriginShiftService>();

        // Add our ECS world        
        services.AddSingleton<WorldBuilder>();

        // This adds all our Monogame.Extended ECS systems (which are in this assembly)
        services.AddAllImplementationsAsSelf<ISystem>(ServiceLifetime.Singleton, Assembly.GetExecutingAssembly());

        // Now add all our screens (which are in this assembly)
        services.AddAllImplementationsAsSelf<Screen>(ServiceLifetime.Singleton, Assembly.GetExecutingAssembly());

        return services.BuildServiceProvider();
    }

    protected override void Draw(GameTime gameTime)
    {
        // Tell the system we want to render to the custom render target
        _customRenderTarget.Begin();

        // Draw all registered drawable game components
        base.Draw(gameTime);

        // Finally, draw the render target to the screen
        _customRenderTarget.Draw();
    }

    protected override void Initialize()
    {
        // Create service collection (not using the Monogame 'container' as it cannot do
        // constructor injection), so instead we're using the standard Microsoft container ;-)        
        _serviceProvider = ConfigureServices();

        // Initialize the screen manager with the service provider so it can resolve screens
        base.Initialize();

        // Don't forget to set our custom render target service so we can use it in the
        // draw method to render to our virtual resolution
        _customRenderTarget = _serviceProvider.GetRequiredService<CustomRenderTarget>();
    }

    protected override void LoadContent()
    {
        // Show the starting screen, which will be the main menu screen in this case
        var startingScreen = _serviceProvider.GetRequiredService<GamePlayScreen>();
        _screenManager.ShowScreen(startingScreen);
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardExtended.Update();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || KeyboardExtended.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // All update logic is now handled by the screen management service        
        base.Update(gameTime);
    }
}
