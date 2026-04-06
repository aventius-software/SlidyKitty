using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Physics;
using System;

namespace SlidyKitty.Code.Map;

internal class HillUpdateSystem : EntityUpdateSystem
{
    private const int _hillHeight = 1000;
    private const int _maxSteepness = 35;
    private const int _minSteepness = -35;
    private const int _numberOfSegmentsPerHill = 64;
    private const int _segmentWidth = 16;

    private readonly OrthographicCamera _camera;
    private readonly HillFactory _hillFactory;

    private ComponentMapper<HillComponent> _hillMapper = default!;
    private readonly Random _random = new();
    private ComponentMapper<RigidBodyComponent> _rigidBodyMapper = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public HillUpdateSystem(OrthographicCamera camera, HillFactory hillFactory) : base(Aspect.All(
        typeof(HillComponent),
        typeof(Transform2)))
    {
        _camera = camera;
        _hillFactory = hillFactory;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Get component mappers
        _hillMapper = mapperService.GetMapper<HillComponent>();
        _rigidBodyMapper = mapperService.GetMapper<RigidBodyComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();

        // Create some initial hills to start with, we will add more as the game scrolls in the update method
        for (var numberOfInitialHills = 0; numberOfInitialHills < 5; numberOfInitialHills++)
        {
            // Set the position of the hill based on the number of initial hills we have already created, so
            // they are spaced out correctly. We start at zero and then add the width of each hill (number of
            // segments * segment width) for each hill we create
            var position = Vector2.Zero + new Vector2(numberOfInitialHills * _numberOfSegmentsPerHill * _segmentWidth, 0);

            // Add a new hill at this position
            AddNewHill(position);
        }
    }

    public override void Update(GameTime gameTime)
    {
        var numberOfHills = ActiveEntities.Count;

        foreach (var entityId in ActiveEntities)
        {
            // Get our entities components
            var hillComponent = _hillMapper.Get(entityId);
            var rigidBodyComponent = _rigidBodyMapper.Get(entityId);
            var transformComponent = _transformMapper.Get(entityId);

            // Get the position of the hill            
            var hillPosition = transformComponent.Position + new Vector2(hillComponent.HillWidth, 0);

            // Check if the hill is off the left of the camera, and if so then we remove it and
            // add a new one to the right of the screen so we have an 'infinite' scrolling hill effect
            if (IsPositionOffCameraToTheLeft(hillPosition))
            {
                // Yes, ok, first remove the rigid body for the hill 
                // so it can dispose of any resources correctly (i.e. physics)
                _hillFactory.RemoveHillPhysicsBody(rigidBodyComponent);

                // Next, remove the hill entity...
                DestroyEntity(entityId);

                // Add a new hill to the right of the screen (we use the position of the current hill
                // plus the width of the hill to determine where to place the new hill)
                var newHillPosition = transformComponent.Position + new Vector2(numberOfHills * hillComponent.HillWidth, 0);
                AddNewHill(newHillPosition);
            }
        }
    }

    /// <summary>
    /// Add a new hill to the world at the specified position. The hill factory will add the necessary components 
    /// to the new hill entity and set it up correctly (e.g. create the physics body, calculate the segments 
    /// etc.) based on the parameters we pass in here (e.g. position, height, steepness etc.)
    /// </summary>
    /// <param name="position"></param>
    private void AddNewHill(Vector2 position)
    {
        // Create an entity for the hill
        var entity = CreateEntity();

        // The hill factory will add the components to it and set it up correctly
        _hillFactory.CreateHill(
            entity: entity,
            position: position,
            offsetY: 0,
            startingAngle: 0,
            numberOfSegments: _numberOfSegmentsPerHill,
            segmentWidth: _segmentWidth,
            steepness: _random.Next(_minSteepness, _maxSteepness),
            height: _hillHeight);
    }

    /// <summary>
    /// Check if the specified position is off the left of the camera, we use this mostly for checking the
    /// position of a hill (plus its width) to determine when to remove it and add a new one to the right of 
    /// the screen. If its off the screen to the left then we can remove it. This allows us to have an infinite
    /// scrolling world which never ends without using up all our memory by creating new hills indefinitely.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>True or false depending on whether the specified position is off the screen to the left</returns>
    private bool IsPositionOffCameraToTheLeft(Vector2 position)
    {
        return position.X < _camera.BoundingRectangle.Left;
    }
}
