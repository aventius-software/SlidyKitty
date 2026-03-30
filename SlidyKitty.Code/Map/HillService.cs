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

    public Hill CreateHill(Vector2 position, int numberOfSegments, int segmentWidth, int steepness, int height, float startingAngle = 0f)
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

    public void DeleteHill(Hill hill)
    {
        foreach (var segment in hill.Segments)
            if (segment.Body is not null)
                _physicsService.World.Remove(segment.Body);
    }

    public void DrawHill(Hill hill, Effect? terrainShader = null)
    {
        // First draw the map (so that it will be under the character), we start
        // the shape drawing batch using current camera view matrix
        _shapeDrawingService.BeginBatch(_camera.GetViewMatrix());

        foreach (var segment in hill.Segments)
        {
            // Get the start and end coordinates of the segment
            var start = segment.Start;
            var end = segment.End;

            // Set shader when drawing terrain quad
            _shapeDrawingService.SetCustomShader(terrainShader);

            // Draw this terrain 'segment' which will be affected by our terrain shader. The
            // shader should 'draw' some kind pattern on the terrain. If no shader is used
            // then this will just be a simple flat coloured quad...
            _shapeDrawingService.DrawFilledQuadrilateral(
                colour: new Color(0, 0, 255, 255),
                topLeftX: (int)start.X, topLeftY: (int)start.Y,
                topRightX: (int)end.X - 0, topRightY: (int)end.Y,
                bottomRightX: (int)end.X - 0, bottomRightY: hill.Height,
                bottomLeftX: (int)start.X, bottomLeftY: hill.Height);

            // Disable any custom terrain shader
            _shapeDrawingService.SetCustomShader(null);

            // Draw ground line
            _shapeDrawingService.DrawLine(start, end, Color.White);
        }

        // Done drawing shapes
        _shapeDrawingService.EndBatch();
    }
}
