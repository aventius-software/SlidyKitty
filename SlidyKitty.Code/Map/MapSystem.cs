using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using System;
using System.Collections.Generic;

namespace SlidyKitty.Code.Map;

internal class MapSystem : UpdateSystem, IDrawSystem
{
    private const int _hillHeight = 1000;
    private const int _maxSteepness = 35;
    private const int _minSteepness = -35;
    private const int _numberOfSegmentsPerHill = 36;
    private const int _segmentWidth = 32;

    private readonly OrthographicCamera _camera;
    private readonly ContentManager _contentManager;
    private readonly HillService _hillService;

    private List<Hill> _hills = [];
    private readonly Random _random = new();
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

        // Create some initial hills to start with, we will add more as
        // the game goes on in the update method
        _hills = _hillService.CreateHills(
            startingPosition: Vector2.Zero,
            numberOfHills: 10,
            height: _hillHeight,
            numberOfSegmentsPerHill: _numberOfSegmentsPerHill,
            segmentWidth: _segmentWidth,
            minSteepness: _minSteepness,
            maxSteepness: _maxSteepness);

        base.Initialize(world);
    }

    public override void Update(GameTime gameTime)
    {
        // Check each hill to see if it has gone off the left of the screen, and if so then we remove it and
        // add a new one to the right of the screen so we have an 'infinite' scrolling hill effect
        for (int hill = 0; hill < _hills.Count; hill++)
        {
            // Has the leftmost hill gone off the left of the camera?
            if (IsHillOffCameraToTheLeft(_hills[hill]))
            {
                // Yes, ok, first remove the hill from the hill service
                // so it can dispose of any resources correctly (i.e. physics)
                _hillService.RemoveHillPhysicsBody(_hills[hill]);

                // Next, remove the hill from our list of hills so it's no longer drawn
                _hills.RemoveAt(hill);

                // Finally, add a new hill to the end of the list
                var lastHill = _hills[^1];
                var position = new Vector2(lastHill.Segments[^1].End.X, lastHill.Segments[^1].End.Y);

                var newHill = _hillService.CreateHill(
                    position: position,
                    numberOfSegments: _numberOfSegmentsPerHill,
                    segmentWidth: _segmentWidth,
                    steepness: _random.Next(_minSteepness, _maxSteepness),
                    height: _hillHeight,
                    startingAngle: lastHill.EndAngle);

                _hills.Add(newHill);
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
