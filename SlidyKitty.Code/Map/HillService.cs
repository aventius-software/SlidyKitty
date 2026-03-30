using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using nkast.Aether.Physics2D.Collision.Shapes;
using SlidyKitty.Code.Physics;
using System;
using System.Collections.Generic;

namespace SlidyKitty.Code.Map;

internal class HillService
{
    private readonly OrthographicCamera _camera;
    private readonly PhysicsService _physicsService;
    private readonly ShapeDrawingService _shapeDrawingService;

    public HillService(OrthographicCamera camera, PhysicsService physicsService, ShapeDrawingService shapeDrawingService)
    {
        _camera = camera;
        _physicsService = physicsService;
        _shapeDrawingService = shapeDrawingService;
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
    public Hill CreateHill(
        Vector2 position,
        int numberOfSegments,
        int segmentWidth,
        int steepness,
        int height,
        float startingAngle = 0f)
    {
        // Reserve an array to store the hill segment
        var hillSegments = new HillSegment[numberOfSegments];

        // Set initial starting position
        var segmentX = position.X;
        var segmentY = position.Y;

        // Per hill we gradually increase our angle per segment to cover full 360 degrees
        var angleIncrement = MathHelper.ToRadians(360f / numberOfSegments);

        // Now, generate requested number of segments for this hill
        for (var segmentIndex = 0; segmentIndex < numberOfSegments; segmentIndex++)
        {
            // If this is NOT the first segment of the hill, we use
            // the Y coordinate of the previous segment
            if (segmentIndex > 0)
                segmentY = hillSegments[segmentIndex - 1].End.Y;

            // Calculate start coordinates for this segment
            var segmentStartPosition = new Vector2(segmentX, segmentY);

            // Now calculate the position of the end of the segment
            segmentX += segmentWidth;

            // Use the starting angle so consecutive hills continue the curve smoothly
            var angle = startingAngle + segmentIndex * angleIncrement;
            var offsetY = MathF.Sin(angle) * steepness;
            var segmentEndPosition = new Vector2(segmentX, segmentY + offsetY);

            // Create a physics body for this segment and attach an 'edge' (just a 'line' effectively)
            var body = _physicsService.World.CreateBody();
            var edgeFixture = body
                .CreateFixture(new EdgeShape(_physicsService.ToSimUnits(segmentStartPosition), _physicsService.ToSimUnits(segmentEndPosition)));

            // Set its friction to some very low value (to make it slippery)
            edgeFixture.Friction = 0.01f;

            // Add this segment to the list...
            hillSegments[segmentIndex] = new HillSegment
            {
                Start = segmentStartPosition,
                End = segmentEndPosition,
                Body = body
            };
        }

        // Compute the end angle based on the final segment slope so the next hill can continue from it
        var last = hillSegments[numberOfSegments - 1];
        var delta = last.End - last.Start;
        var endAngle = MathF.Atan2(delta.Y, delta.X);

        return new Hill
        {
            EndAngle = endAngle,
            Height = height,
            Segments = hillSegments
        };
    }

    /// <summary>
    /// A helper to create multiple hills in a row, each hill will continue from the end of 
    /// the previous one (hopefully smoothly). This is just a helper method which calls the 
    /// 'CreateHill' method multiple times and updates the starting position and angle for 
    /// each new hill based on the end position and angle of the previous hill.
    /// </summary>
    /// <param name="startingPosition">The starting position to place the first hill.</param>
    /// <param name="numberOfHills">The number of hills to create.</param>
    /// <param name="height">The height or depth of the hills</param>
    /// <param name="numberOfSegmentsPerHill">The number of segments which make up a single hill.</param>
    /// <param name="segmentWidth">The width in pixels of a single hill segment.</param>
    /// <param name="minSteepness">The minimum steepness a hill can be.</param>
    /// <param name="maxSteepness">The maximum steepness a hill can be.</param>
    /// <returns>A list of hill objects.</returns>
    public List<Hill> CreateHills(
        Vector2 startingPosition,
        int numberOfHills,
        int height = 1000,
        int numberOfSegmentsPerHill = 32,
        int segmentWidth = 16,
        int minSteepness = -25,
        int maxSteepness = 25)
    {
        var hills = new List<Hill>();
        var r = new Random();
        var position = startingPosition;

        for (var i = 0; i < numberOfHills; i++)
        {
            var hill = CreateHill(
                position: position,
                numberOfSegments: numberOfSegmentsPerHill,
                segmentWidth: segmentWidth,
                steepness: r.Next(minSteepness, maxSteepness),
                height: height,
                startingAngle: hills.Count > 0 ? hills[^1].EndAngle : 0f);

            hills.Add(hill);
            position = hills[^1].Segments[^1].End;
        }

        return hills;
    }

    /// <summary>
    /// Draw an individual hill by drawing each of its segments as a filled quadrilateral (to 
    /// represent the terrain) and a line (to represent the ground). Uses an option custom shader 
    /// when drawing the terrain quads so that we can apply some kind of pattern to the terrain (e.g. to 
    /// make it look like grass or dirt). If no shader is provided then the terrain will just be drawn as 
    /// a flat coloured quad. Note that the physics bodies for the hill segments are not drawn, they are 
    /// just used for calculating physics interactions with the player (e.g. sliding down the hill). The 
    /// physics bodies are effectively invisible and do not affect how the hill is drawn.
    /// </summary>
    /// <param name="hill"></param>
    /// <param name="terrainShader"></param>
    public void DrawHill(Hill hill, Effect? terrainShader = null)
    {
        // First draw the map (so that it will be under the character), we start
        // the shape drawing batch using current camera view matrix
        _shapeDrawingService.BeginBatch(_camera.GetViewMatrix());

        // Now draw each segment of the hill as a filled quadrilateral (for
        // the terrain) and a line (for the ground)
        foreach (var segment in hill.Segments)
        {
            // Get the start and end coordinates of the segment
            var segmentStartPosition = segment.Start;
            var segmentEndPosition = segment.End;

            // Set shader when drawing terrain quad
            _shapeDrawingService.SetCustomShader(terrainShader);

            // Draw this terrain 'segment' which will be affected by our terrain shader. The
            // shader should 'draw' some kind pattern on the terrain. If no shader is used
            // then this will just be a simple flat coloured quad...
            _shapeDrawingService.DrawFilledQuadrilateral(
                colour: new Color(0, 0, 255, 255),
                topLeftX: (int)segmentStartPosition.X, topLeftY: (int)segmentStartPosition.Y,
                topRightX: (int)segmentEndPosition.X - 0, topRightY: (int)segmentEndPosition.Y,
                bottomRightX: (int)segmentEndPosition.X - 0, bottomRightY: hill.Height,
                bottomLeftX: (int)segmentStartPosition.X, bottomLeftY: hill.Height);

            // Disable any custom terrain shader
            _shapeDrawingService.SetCustomShader(null);

            // Draw ground line
            _shapeDrawingService.DrawLine(segmentStartPosition, segmentEndPosition, Color.White);
        }

        // Done drawing shapes
        _shapeDrawingService.EndBatch();
    }

    /// <summary>
    /// Delete a hill by removing all of its segments' physics bodies from the physics world. This is important
    /// otherwise the physics bodies will still exist and interact with the player even if we are no longer drawing 
    /// the hill (e.g. if we want to remove hills that are off screen). Note that we don't need to do anything 
    /// to 'remove' the drawn hill as it will simply not be drawn anymore when we call 'DrawHill' with a different 
    /// set of hills.
    /// </summary>
    /// <param name="hill">The hill to delete</param>
    public void RemoveHillPhysicsBody(Hill hill)
    {
        foreach (var segment in hill.Segments)
            if (segment.Body is not null)
                _physicsService.World.Remove(segment.Body);
    }
}
