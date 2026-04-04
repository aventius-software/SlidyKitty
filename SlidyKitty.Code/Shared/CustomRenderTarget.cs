using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace SlidyKitty.Code.Shared;

internal class CustomRenderTarget
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;

    private Color? _clearScreenColour = null;
    private Rectangle _destinationRectangle;
    private RenderTarget2D _renderTarget = default!;

    public CustomRenderTarget(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
    }

    public void Begin()
    {
        _graphicsDevice.SetRenderTarget(_renderTarget);

        if (_clearScreenColour is not null)
            _graphicsDevice.Clear(_clearScreenColour.Value);
    }

    public void Draw()
    {
        // Done using render target to draw
        _graphicsDevice.SetRenderTarget(null);

        // Draw the render target to the 'real' screen now
        _spriteBatch.Begin();
        _spriteBatch.Draw(_renderTarget, _destinationRectangle, Color.White);
        _spriteBatch.End();
    }

    public void InitialiseRenderDestination(int virtualScreenWidth, int virtualScreenHeight, Color? clearScreenColour = null)
    {
        // Create our 'virtual' render target
        _renderTarget = new RenderTarget2D(_graphicsDevice, virtualScreenWidth, virtualScreenHeight);

        // Set the screen 'clear' colour (if we're clearing the screen at all)
        if (clearScreenColour is not null)
            _clearScreenColour = clearScreenColour.Value;

        // Now setup scaling so everything looks right no matter the real resolution
        var screenSize = _graphicsDevice.Viewport.Bounds.Size;

        var scaleX = (float)screenSize.X / _renderTarget.Width;
        var scaleY = (float)screenSize.Y / _renderTarget.Height;
        var scale = Math.Min(scaleX, scaleY);

        var destinationWidth = (int)(_renderTarget.Width * scale);
        var destinationHeight = (int)(_renderTarget.Height * scale);

        var destinationX = (screenSize.X - destinationWidth) / 2;
        var destinationY = (screenSize.Y - destinationHeight) / 2;

        _destinationRectangle = new Rectangle(destinationX, destinationY, destinationWidth, destinationHeight);
    }
}
