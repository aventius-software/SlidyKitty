using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;

namespace SlidyKitty.Code.Map;

internal class HillDrawSystem : EntityDrawSystem
{
    private readonly OrthographicCamera _camera;
    private readonly ContentManager _contentManager;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ShapeDrawingService _shapeDrawingService;

    private ComponentMapper<HillComponent> _hillMapper = default!;
    private Effect _terrainShader = default!;
    private ComponentMapper<Transform2> _transformMapper = default!;

    public HillDrawSystem(
        OrthographicCamera camera,
        ContentManager contentManager,
        GraphicsDevice graphicsDevice,
        ShapeDrawingService shapeDrawingService) : base(Aspect.All(
        typeof(HillComponent),
        typeof(Transform2)))
    {
        _camera = camera;
        _contentManager = contentManager;
        _graphicsDevice = graphicsDevice;
        _shapeDrawingService = shapeDrawingService;
    }

    public override void Initialize(IComponentMapperService mapperService)
    {
        // Get component mappers
        _hillMapper = mapperService.GetMapper<HillComponent>();
        _transformMapper = mapperService.GetMapper<Transform2>();

        // Load our custom terrain shader which we will use when drawing nice
        // patterns on the terrain quads for our hills.
        _terrainShader = _contentManager.Load<Effect>("Shaders/dirt and grass");
    }

    public override void Draw(GameTime gameTime)
    {
        // Configure the shader before each draw run so that it has the latest camera position
        ConfigureShader();

        foreach (var entityId in ActiveEntities)
        {
            // Get our entities components
            var hillComponent = _hillMapper.Get(entityId);
            var transformComponent = _transformMapper.Get(entityId);

            // Draw this hill
            DrawHill(hillComponent, transformComponent);
        }
    }

    private void ConfigureShader()
    {
        // Set the view and projection matrices on the shader. The view matrix is based on the
        // camera's position and orientation, and the projection matrix is an orthographic
        // projection based on the viewport size. We multiply these together to get a combined
        // view-projection matrix which we can use in our shader to transform our terrain
        // vertices from world space into screen space.
        var view = _camera.GetViewMatrix();
        var projection = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, 0, 1);

        _terrainShader.Parameters["ViewProjection"].SetValue(view * projection);
        _terrainShader.Parameters["Scale"].SetValue(0.05f);
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
        }
    }
}
