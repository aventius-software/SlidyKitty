using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using nkast.Aether.Physics2D.Collision.Shapes;
using SlidyKitty.Code.Physics;
using System;
using System.Collections.Generic;

namespace SlidyKitty.Code.Map;

internal class HillFactory
{
    private readonly PhysicsService _physicsService;

    public HillFactory(PhysicsService physicsService)
    {
        _physicsService = physicsService;
    }

    /// <summary>
    /// This method creates a hill which is made up of the specified number of segments. The
    /// more segments, the smoother the 'curve' of the hill. Each segment is also created with
    /// a physics body with an 'edge' shape (effectively just a line), this is so we can figure
    /// out the sliding and friction physics between the ground and player. The end position of 
    /// each segment is calculated using a sine wave to create a smooth curve. The steepness 
    /// parameter controls how 'steep' the hill is (i.e. how much the Y coordinate changes per 
    /// segment) and the height parameter specifies how far down the hill extends (used for drawing 
    /// the terrain quad). The starting angle allows you to specify the initial angle of the first 
    /// segment so that consecutive hills can continue from each other smoothly.
    /// </summary>
    /// <param name="position">The position to place this hill at.</param>
    /// <param name="numberOfSegments">The number of segments which make up a hill.</param>
    /// <param name="segmentWidth">The width in pixels of an individual segment.</param>
    /// <param name="steepness">How steep this hill should be.</param>
    /// <param name="height">The height or depth of this hill.</param>
    /// <param name="startingAngle">The starting angle to use when creating this hill.</param>
    /// <returns>A complete hill.</returns>
    public Entity CreateHill(
        Entity entity,
        Vector2 position,
        float offsetY,
        float startingAngle,
        int numberOfSegments,
        int segmentWidth,
        int steepness,
        int height)
    {
        // Reserve an array to store the hill segment
        var hillSegments = new List<HillSegment>();

        // Per hill we gradually increase our angle per segment to cover full 360 degrees
        var angleIncrement = MathHelper.ToRadians(360f / numberOfSegments);

        // Starting Y position
        var startOffsetY = offsetY;

        // Create a single physics body for the entire hill, we will attach
        // multiple 'edge' fixtures to this body to form the segments of the hill. We
        // do this because we want the entire hill to be a single physics body so that
        // the player can slide across it smoothly without any 'jumps' in physics when
        // moving from one segment to the next. If we created separate physics bodies
        // for each segment then we would have to worry about how to transition the
        // player between them and it would likely result in a less smooth experience.
        var body = _physicsService.World.CreateBody(_physicsService.ToSimUnits(position));

        // Now, generate requested number of segments for this hill
        for (var segmentIndex = 0; segmentIndex < numberOfSegments; segmentIndex++)
        {
            // If this is NOT the first segment of the hill, we use
            // the Y coordinate of the previous segment
            if (segmentIndex > 0)
                startOffsetY = hillSegments[segmentIndex - 1].EndYOffset;

            // Add a physics 'edge' fixture to the main physics body, just for this segment. First
            // we calculate the start and end positions of this segment relative to the hill's position
            var relativePositionX = segmentIndex * segmentWidth;
            var relativePositionY = startOffsetY;
            var relativePosition = _physicsService.ToSimUnits(new Vector2(relativePositionX, relativePositionY));

            // The end position of this segment is calculated using a sine wave to create a smooth curve. Note
            // that we need to use the starting angle so consecutive hills continue the curve smoothly!
            var angle = startingAngle + segmentIndex * angleIncrement;
            var endOffsetY = startOffsetY + (MathF.Sin(angle) * steepness);

            // Now work out the end position of this segment relative to the hill's position
            var relativePositionEndX = (segmentIndex + 1) * segmentWidth;
            var relativePositionEndY = endOffsetY;
            var relativePositionEnd = _physicsService.ToSimUnits(new Vector2(relativePositionEndX, relativePositionEndY));

            // Finally create an edge fixture for this segment using the start and end positions we calculated, and
            // attach it to the main physics body for this hill
            var edgeShape = new EdgeShape(relativePosition, relativePositionEnd);
            var edgeFixture = body.CreateFixture(edgeShape);

            // Set its friction to some very low value (to make it slippery)
            edgeFixture.Friction = 0.01f;

            // Add this segment to the list...
            hillSegments.Add(new HillSegment
            {
                StartYOffset = startOffsetY,
                EndYOffset = endOffsetY
            });
        }

        // Compute the end angle based on the final segment slope so we can use it if we
        // want to make another hill continue a smooth curve when transitioning from this hill
        var lastSegment = hillSegments[numberOfSegments - 1];
        var deltaY = lastSegment.EndYOffset - lastSegment.StartYOffset;
        var endAngle = MathF.Atan2(deltaY, 0);

        // Add a transform component to specify the position of the hill in the world
        var transformComponent = new Transform2(position);
        entity.Attach(transformComponent);

        // We also need a rigid body component to attach the physics body we created for this hill
        var rigidBodyComponent = new RigidBodyComponent
        {
            Body = body
        };

        entity.Attach(rigidBodyComponent);

        // Finally we attach a hill component which contains all (non-physics) information about this
        // hill (e.g. its segments, height, end angle etc.) which we will use when drawing the hill        
        entity.Attach(new HillComponent
        {
            EndAngle = endAngle,
            Height = height,
            Segments = hillSegments,
            SegmentWidth = segmentWidth
        });

        return entity;
    }

    /// <summary>
    /// Delete a hill by removing all of its segments' physics bodies from the physics world. This is important
    /// otherwise the physics bodies will still exist and interact with the player even if we are no longer drawing 
    /// the hill (e.g. if we want to remove hills that are off screen). Note that we don't need to do anything 
    /// to 'remove' the drawn hill as it will simply not be drawn anymore when we call 'DrawHill' with a different 
    /// set of hills.
    /// </summary>
    /// <param name="rigidBodyComponent">The rigid body component of the hill we want to remove.</param>
    public void RemoveHillPhysicsBody(RigidBodyComponent rigidBodyComponent)
    {
        _physicsService.World.Remove(rigidBodyComponent.Body);
    }
}
