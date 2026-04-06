using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SlidyKitty.Code.Map;

internal class ShapeDrawingService
{    
    private readonly GraphicsDevice _graphicsDevice;

    public ShapeDrawingService(GraphicsDevice graphicsDevice)
    {        
        _graphicsDevice = graphicsDevice;
    }

    public void DrawQuadrilateral(Effect effect, int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
    {
        // Reserve an array to store the triangle coordinates that of the
        // 2 triangles that make up the quadrilateral. The first triangle
        // will be made up of the top left, bottom right and top right
        // corners of the quad, and the second triangle will be made up
        // of the top left, bottom left and bottom right corners of the
        // quad.
        var vertices = new VertexPosition[6];

        // Define the first triangle
        var triangle1TopLeft = new VertexPosition(new Vector3(x0, y0, 0f));
        var triangle1TopRight = new VertexPosition(new Vector3(x1, y1, 0f));
        var triangle1BottomRight = new VertexPosition(new Vector3(x2, y2, 0f));

        vertices[0] = triangle1TopLeft;
        vertices[1] = triangle1BottomRight;
        vertices[2] = triangle1TopRight;

        // Now the second triangle
        var triangle2TopLeft = new VertexPosition(new Vector3(x0, y0, 0f));
        var triangle2BottomRight = new VertexPosition(new Vector3(x2, y2, 0f));
        var triangle2BottomLeft = new VertexPosition(new Vector3(x3, y3, 0f));

        vertices[3] = triangle2TopLeft;
        vertices[4] = triangle2BottomLeft;
        vertices[5] = triangle2BottomRight;

        // Draw...
        _graphicsDevice.RasterizerState = RasterizerState.CullNone;

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
        }
    }
}
