using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Extensions;
using SlidyKitty.Code.Physics;
using System;

namespace SlidyKitty.Code.Map;

internal class HillUpdateSystem : EntityUpdateSystem
{
    private const int _hillHeight = 4000;
    private const int _maxDownhillSteepness = -35;
    private const int _maxDownhillSteepnessForFlatHills = -10;
    private const int _maxUphillSteepness = 35;
    private const int _maxUphillSteepnessForFlatHills = 10;
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

        var hillWidth = _numberOfSegmentsPerHill * _segmentWidth;

        // Create some initial hills to start with, we will add more as the game scrolls in the update method
        for (var numberOfInitialHills = 0; numberOfInitialHills < 5; numberOfInitialHills++)
        {
            // Set the position of the hill based on the number of initial hills we have already created, so
            // they are spaced out correctly. We start at zero and then add the width of each hill (number of
            // segments * segment width) for each hill we create
            var position = new Vector2(-hillWidth, 0) + new Vector2(numberOfInitialHills * hillWidth, 0);

            // We can optionally specify the steepness of the hill, if we don't then a random steepness will be
            // generated for it. For the first hill we want to specify a downward slope so that we have some
            // initial momentum when the game starts, for the rest of the hills we'll just generate a random
            // steepness
            int? steepness = null;

            // Start with a hill that slopes downwards so we have some initial momentum when the game starts
            if (numberOfInitialHills == 0)
                steepness = _maxDownhillSteepness;

            // Add a new hill at this position
            AddNewHill(position, steepness);
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
            if (_camera.IsPositionOffCameraToTheLeft(hillPosition))
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
    /// <param name="position">World position of the hill</param>
    /// <param name="steepness">Optional hill steepness, if null specified then a random amount is used</param>
    private void AddNewHill(Vector2 position, int? steepness = null)
    {
        // Create an entity for the hill
        var entity = CreateEntity();

        // Determine the steepness of the hill, if a specific steepness is provided then use that, otherwise
        // generate a random steepness for the hill within the defined min and max steepness values
        var hillSteepness = steepness ?? _random.Next(_maxDownhillSteepness, _maxUphillSteepness);

        // We don't want too many flat hills, so if the steepness is between a certain range, for
        // example -10 and 10 then we will make it a bit more extreme
        if (hillSteepness > _maxDownhillSteepnessForFlatHills && hillSteepness < _maxUphillSteepnessForFlatHills)
        {
            // Make the hill a bit steeper instead of too flat...
            hillSteepness = hillSteepness < 0 ? _maxDownhillSteepnessForFlatHills : _maxUphillSteepnessForFlatHills;
        }

        // The hill factory will add the components to it and set it up correctly
        _hillFactory.CreateHill(
            entity: entity,
            position: position,
            offsetY: 0,
            startingAngle: 0,
            numberOfSegments: _numberOfSegmentsPerHill,
            segmentWidth: _segmentWidth,
            steepness: hillSteepness,
            height: _hillHeight);
    }
}
