using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SlidyKitty.Code.Map;

internal class ShapeDrawingService
{
    private readonly BasicEffect _basicEffect;
    private readonly GraphicsDevice _graphicsDevice;

    private Matrix? _cameraTransformationMatrix;
    private Effect? _customShader;

    public ShapeDrawingService(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _graphicsDevice.RasterizerState = RasterizerState.CullNone;

        // We'll use the BasicEffect 'shader' to draw stuff by default if no custom shader is supplied
        _basicEffect = new BasicEffect(_graphicsDevice);
    }

    /// <summary>
    /// Start a batch ready for shape drawing
    /// </summary>
    /// <param name="cameraTransformationMatrix">The transform matrix to use from a camera (optional).</param>
    /// <param name="customShader">A custom effect (optional).</param>
    public void BeginBatch(Matrix? cameraTransformationMatrix = null, Effect? customShader = null)
    {
        // Set/save the transformation matrix
        _cameraTransformationMatrix = cameraTransformationMatrix;

        // Set the shader we'll use, if a custom shader is not being used we just use BasicEffect
        if (customShader is null) UseDefaultShader();
        else SetCustomShader(customShader);
    }

    /// <summary>
    /// Draw a filled quad, will use the custom shader if one has been set, otherwise it will use 
    /// BasicEffect. The quad is drawn as 2 triangles, so the vertices are ordered in a way to make 
    /// sure the triangles are wound correctly for backface culling (even though we have backface 
    /// culling turned off, it's still good practice to wind the triangles correctly).
    /// </summary>
    /// <param name="colour">The colour to use to draw the quad.</param>
    /// <param name="topLeftX">The top left most X position of the quad.</param>
    /// <param name="topLeftY">The top left most Y position of the quad.</param>
    /// <param name="topRightX">The top right most X position of the quad.</param>
    /// <param name="topRightY">The top right most Y position of the quad.</param>
    /// <param name="bottomRightX">The bottom right most X position of the quad.</param>
    /// <param name="bottomRightY">The bottom right most Y position of the quad.</param>
    /// <param name="bottomLeftX">The bottom left most X position of the quad.</param>
    /// <param name="bottomLeftY">The bottom left most Y position of the quad.</param>
    public void DrawFilledQuadrilateral(
        Color colour,
        int topLeftX, int topLeftY,
        int topRightX, int topRightY,
        int bottomRightX, int bottomRightY,
        int bottomLeftX, int bottomLeftY)
    {
        // Coordinates
        var vertices = new VertexPositionColor[6];

        // First triangle
        var triangle1TopLeft = new VertexPositionColor
        {
            Position = new Vector3(topLeftX, topLeftY, 0f),
            Color = colour
        };

        var triangle1TopRight = new VertexPositionColor
        {
            Position = new Vector3(topRightX, topRightY, 0f),
            Color = colour
        };

        var triangle1BottomRight = new VertexPositionColor
        {
            Position = new Vector3(bottomRightX, bottomRightY, 0f),
            Color = colour
        };

        // Second triangle
        var triangle2TopLeft = new VertexPositionColor
        {
            Position = new Vector3(topLeftX, topLeftY, 0f),
            Color = colour
        };

        var triangle2BottomRight = new VertexPositionColor
        {
            Position = new Vector3(bottomRightX, bottomRightY, 0f),
            Color = colour
        };

        var triangle2BottomLeft = new VertexPositionColor
        {
            Position = new Vector3(bottomLeftX, bottomLeftY, 0f),
            Color = colour
        };

        _graphicsDevice.RasterizerState = RasterizerState.CullNone;

        // First triangle
        vertices[0] = triangle1TopLeft;
        vertices[1] = triangle1BottomRight;
        vertices[2] = triangle1TopRight;

        // Second triangle
        vertices[3] = triangle2TopLeft;
        vertices[4] = triangle2BottomLeft;
        vertices[5] = triangle2BottomRight;

        // Draw...        
        var passes = _customShader is null ? _basicEffect.CurrentTechnique.Passes : _customShader.CurrentTechnique.Passes;

        foreach (var pass in passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
        }
    }

    /// <summary>
    /// Draw a simple line. Will use the custom shader if one has been set, otherwise it 
    /// will use BasicEffect. The line is drawn as a single line primitive.
    /// </summary>
    /// <param name="start">The starting position of the line.</param>
    /// <param name="end">The ending position of the line.</param>
    /// <param name="colour">The colour to use when drawing the line.</param>
    public void DrawLine(Vector2 start, Vector2 end, Color colour)
    {
        // Coordinates
        var vertices = new VertexPositionColor[2];

        // First triangle
        vertices[0].Position = new Vector3(start.X, start.Y, 0f);
        vertices[0].Color = colour;
        vertices[1].Position = new Vector3(end.X, end.Y, 0f);
        vertices[1].Color = colour;

        // Draw...
        var passes = _customShader is null ? _basicEffect.CurrentTechnique.Passes : _customShader.CurrentTechnique.Passes;

        foreach (var pass in passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }
    }

    /// <summary>
    /// End shape drawing batch, resets transformation matrix and any custom shader
    /// </summary>
    public void EndBatch()
    {
        _cameraTransformationMatrix = null;
        _customShader = null;
    }

    /// <summary>
    /// Set a custom shader to be used, passing NULL will reset the shader
    /// </summary>
    /// <param name="customShader">Custom effect shader to use (optional).</param>
    public void SetCustomShader(Effect? customShader)
    {
        if (customShader is null) UseDefaultShader();
        else UseCustomShader(customShader);
    }

    /// <summary>
    /// Sets up the service to use the specified shader
    /// </summary>
    /// <param name="customShader">Custom effect shader to use.</param>
    private void UseCustomShader(Effect customShader)
    {
        _customShader = customShader;

        var cameraUp = Vector3.Transform(Vector3.Down, Matrix.CreateRotationZ(0));
        var world = _cameraTransformationMatrix is null ? Matrix.Identity : (Matrix)_cameraTransformationMatrix;
        var view = _cameraTransformationMatrix is null ? Matrix.CreateLookAt(Vector3.Forward, Vector3.Zero, cameraUp) : Matrix.Identity;
        var projection = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, 0, 1);

        if (_cameraTransformationMatrix is null) projection *= Matrix.CreateScale(1, -1, 1);

        _customShader.Parameters["World"].SetValue(world);
        _customShader.Parameters["View"].SetValue(view);
        _customShader.Parameters["Projection"].SetValue(projection);
    }

    /// <summary>
    /// Sets up the service to use BasicEffect shader for drawing
    /// </summary>
    private void UseDefaultShader()
    {
        _customShader = null;

        var cameraUp = Vector3.Transform(Vector3.Down, Matrix.CreateRotationZ(0));
        var world = _cameraTransformationMatrix is null ? Matrix.Identity : (Matrix)_cameraTransformationMatrix;
        var view = _cameraTransformationMatrix is null ? Matrix.CreateLookAt(Vector3.Forward, Vector3.Zero, cameraUp) : Matrix.Identity;
        var projection = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, 0, 1);

        if (_cameraTransformationMatrix is null) projection *= Matrix.CreateScale(1, -1, 1);

        _basicEffect.World = world;
        _basicEffect.View = view;
        _basicEffect.Projection = projection;

        _basicEffect.TextureEnabled = false;
        _basicEffect.VertexColorEnabled = true;
    }
}