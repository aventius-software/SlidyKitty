using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using SlidyKitty.Code.Shared;

namespace SlidyKitty.Code.Map;

internal class HillDrawSystem : EntityDrawSystem
{
    private const int _groundLineShadowThickness = 10;
    private const int _groundLineThickness = 15;

    private readonly Vector3 _groundLineColour = new(
        (1.0f / 255.0f) * 40,
        (1.0f / 255.0f) * 140,
        (1.0f / 255.0f) * 40);

    private readonly float _groundLineColourAlpha = (1.0f / 255.0f) * 255;

    private readonly Vector3 _groundLineShadowColour = new(
        (1.0f / 255.0f) * 1,
        (1.0f / 255.0f) * 1,
        (1.0f / 255.0f) * 1);

    private readonly float _groundLineShadowColourAlpha = (1.0f / 255.0f) * 75;

    private readonly OrthographicCamera _camera;
    private readonly ContentManager _contentManager;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly OriginShiftService _originShiftService;
    private readonly ShapeDrawingService _shapeDrawingService;

    private ComponentMapper<HillComponent> _hillMapper = default!;
    private Effect _outlineShader = default!;
    private Effect _terrainShader = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public HillDrawSystem(
        OrthographicCamera camera,
        ContentManager contentManager,
        GraphicsDevice graphicsDevice,
        OriginShiftService originShiftService,
        ShapeDrawingService shapeDrawingService) : base(Aspect.All(
        typeof(HillComponent),
        typeof(Transform2)))
    {
        _camera = camera;
        _contentManager = contentManager;
        _graphicsDevice = graphicsDevice;
        _originShiftService = originShiftService;
        _shapeDrawingService = shapeDrawingService;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Get component mappers
        _hillMapper = mapperService.GetMapper<HillComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();

        // Load our shader which we'll use to draw the outlines of the terrain quads for our hills
        _outlineShader = _contentManager.Load<Effect>("Map/Ground Outline Shader");

        // Load our custom terrain shader which we will use when drawing nice
        // patterns on the terrain quads for our hills.        
        _terrainShader = _contentManager.Load<Effect>("Map/Default Terrain Shader");
    }

    public override void Draw(GameTime gameTime)
    {
        // Set the view and projection matrices on the shader. The view matrix is based on the
        // camera's position and orientation, and the projection matrix is an orthographic
        // projection based on the viewport size. We multiply these together to get a combined
        // view-projection matrix which we can use in our shader to transform our terrain
        // vertices from world space into screen space.
        var view = _camera.GetViewMatrix();
        var projection = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, 0, 1);
        var viewProjection = view * projection;

        // So that the shader knows about the camera's position and orientation, we give
        // the combined view-projection matrix to the shader.
        _terrainShader.Parameters["ViewProjection"].SetValue(viewProjection);

        // As we're using an origin shift technique to keep world/camera positions limited (otherwise
        // they'd grow enormous for an infinite world eventually causing all sorts of issues), then
        // we need to make sure that the shader uses a repeating noise texture in the shader to create
        // patterns on the terrain. So, if we basically have a 'tiled' shader, we need to tell the shader
        // how wide the terrain is in world units so that it can repeat the noise texture correctly across
        // the terrain). So we give the shader the shift value (which should always stay the same) as it
        // will use this to calculate how to repeat the noise texture across the terrain. This is important
        // to ensure that the noise pattern repeats correctly across the terrain as the camera moves.
        _terrainShader.Parameters["TileWidth"].SetValue(_originShiftService.Shift.X);

        // So that the shader knows about the camera's position and orientation, we give
        // the combined view-projection matrix to the shader.
        _outlineShader.Parameters["ViewProjection"].SetValue(viewProjection);

        // Draw all the hills
        foreach (var entityId in ActiveEntities)
        {
            // Get our entities components
            var hillComponent = _hillMapper.Get(entityId);
            var transformComponent = _transformMapper.Get(entityId);

            // Draw this hill
            DrawHill(hillComponent, transformComponent);
        }
    }

    /// <summary>
    /// Draw an individual hill by drawing each of its segments as a filled quadrilateral (to 
    /// represent the terrain) and a line (to represent the ground). Uses an optional custom shader 
    /// when drawing the terrain quads so that we can apply some kind of pattern to the terrain (e.g. to 
    /// make it look like grass or dirt). If no shader is provided then the terrain will just be drawn 
    /// as a flat coloured quad. We start with just the position of the hill's transform component and 
    /// then draw each segment of the hill 'relative' to that position. So if the transform position 
    /// is (100, 200) and the first hill segment has a startY of 50, then the start position of 
    /// that segment will be (100, 250). If the second segment has a startY of 60, then its start 
    /// position will be (100 + segmentWidth, 260), and so on for each segment of the hill.
    /// </summary>
    /// <param name="hill">The hill to draw</param>
    /// <param name="transform">The transform component of the hill's entity (used to determine 
    /// the starting position of the hill)</param>
    private void DrawHill(HillComponent hill, Transform2 transform)
    {
        // The starting position of the hill is determined by the position of the entity's transform
        // component. The hill segments are drawn relative to this starting position. So if the transform
        // position is (100, 200) and the first hill segment has a startY of 50, then the start position
        // of that segment will be (100, 250). If the second segment has a startY of 60, then its start
        // position will be (100 + segmentWidth, 260), and so on for each segment of the hill.
        var hillPosition = transform.Position;

        // Now draw each segment of the hill as a filled quadrilateral (for
        // the terrain) and a line (for the ground)
        for (var segmentIndex = 0; segmentIndex < hill.Segments.Count; segmentIndex++)
        {
            // For each segment we calculate the start and end positions of the terrain
            // quad. The start position is just the hill's starting position plus an offset
            // based on the segment's index multiplied by the segment width) and the segment's startY.
            var startingRelativeOffsetX = segmentIndex * hill.SegmentWidth;
            var startingRelativeOffsetY = hill.Segments[segmentIndex].StartYOffset;
            var startingRelativeOffset = new Vector2(startingRelativeOffsetX, startingRelativeOffsetY);
            var segmentStartPosition = hillPosition + startingRelativeOffset;

            // The end position is the hill's starting position plus an offset based on the
            // segment's index (plus 1) multiplied by the segment width and the segment's endY.
            var endingRelativeOffsetX = (segmentIndex + 1) * hill.SegmentWidth;
            var endingRelativeOffsetY = hill.Segments[segmentIndex].EndYOffset;
            var endingRelativeOffset = new Vector2(endingRelativeOffsetX, endingRelativeOffsetY);
            var segmentEndPosition = hillPosition + endingRelativeOffset;

            // Draw this terrain 'segment' which will be affected by our terrain shader. Ideall, the
            // shader should 'draw' some kind pattern on the terrain to make the terrain more interesting
            // than just a flat colour. If no shader is used then this will just draw a simple flat
            // coloured quad...
            _shapeDrawingService.DrawQuadrilateral(
                _terrainShader,
                x0: (int)segmentStartPosition.X, y0: (int)segmentStartPosition.Y,
                x1: (int)segmentEndPosition.X, y1: (int)segmentEndPosition.Y,
                x2: (int)segmentEndPosition.X, y2: hill.Height,
                x3: (int)segmentStartPosition.X, y3: hill.Height);

            // Draw a line to represent the ground for this segment of the hill.
            _outlineShader.Parameters["RGB"].SetValue(_groundLineColour);
            _outlineShader.Parameters["Alpha"].SetValue(_groundLineColourAlpha);

            _shapeDrawingService.DrawQuadrilateral(
                _outlineShader,
                x0: (int)segmentStartPosition.X, y0: (int)segmentStartPosition.Y,
                x1: (int)segmentEndPosition.X, y1: (int)segmentEndPosition.Y,
                x2: (int)segmentEndPosition.X, y2: (int)segmentEndPosition.Y + _groundLineThickness,
                x3: (int)segmentStartPosition.X, y3: (int)segmentStartPosition.Y + _groundLineThickness);

            // Draw a 'shadow' line to under the ground line for this segment of the hill.
            _outlineShader.Parameters["RGB"].SetValue(_groundLineShadowColour);
            _outlineShader.Parameters["Alpha"].SetValue(_groundLineShadowColourAlpha);

            _shapeDrawingService.DrawQuadrilateral(
                _outlineShader,
                x0: (int)segmentStartPosition.X, y0: (int)segmentStartPosition.Y + _groundLineThickness,
                x1: (int)segmentEndPosition.X, y1: (int)segmentEndPosition.Y + _groundLineThickness,
                x2: (int)segmentEndPosition.X, y2: (int)segmentEndPosition.Y + _groundLineThickness + _groundLineShadowThickness,
                x3: (int)segmentStartPosition.X, y3: (int)segmentStartPosition.Y + _groundLineThickness + _groundLineShadowThickness);
        }
    }
}
