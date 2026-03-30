using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System.Collections.Generic;

namespace SlidyKitty.Code.Map;

internal class MapSystem : UpdateSystem, IDrawSystem
{
    private readonly OrthographicCamera _camera;
    private readonly ContentManager _contentManager;
    private readonly HillService _hillService;

    private List<Hill> _hills = [];
    private Effect _terrainShader = default!;

    public MapSystem(OrthographicCamera camera, ContentManager contentManager, HillService hillGeneratorService)
    {
        _camera = camera;
        _contentManager = contentManager;
        _hillService = hillGeneratorService;
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var hill in _hills)
            _hillService.DrawHill(hill, _terrainShader);
    }

    public override void Initialize(World world)
    {
        // Load our custom terrain shader which will give the terrain a basic
        // pattern instead of just having a flat coloured terrain
        _terrainShader = _contentManager.Load<Effect>("Shaders/terrain shader");

        // Create some hills to start with, we will add more as the game goes on in
        // the update method. For now we're just testing with a few hills to make sure
        // they are drawn correctly
        _hills = _hillService.CreateHills(Vector2.Zero, 18);

        base.Initialize(world);
    }

    public override void Update(GameTime gameTime)
    {
        // Check each hill to see if it has gone off the left of the screen, and if so then we remove it and
        // add a new one to the right of the screen so we have an 'infinite' scrolling hill effect
        for (int hill = 0; hill < _hills.Count; hill++)
        {
            if (IsHillOffCameraToTheLeft(_hills[hill]))
            {
                // First remove the hill from the service so it can dispose of any resources
                _hillService.DeleteHill(_hills[hill]);

                // The remove from our list of hills so it's no longer drawn
                _hills.RemoveAt(hill);
            }
        }
    }

    /// <summary>
    /// If the end of the last segment of the hill is off the left of the camera 
    /// then we can remove it as it's no longer visible
    /// </summary>
    /// <param name="hill"></param>
    /// <returns></returns>
    private bool IsHillOffCameraToTheLeft(Hill hill)
    {
        return hill.Segments[^1].End.X < _camera.BoundingRectangle.Left;
    }
}
