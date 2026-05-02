using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended.Input;

namespace SlidyKitty.Code.Shared;

internal class InputService
{    
    private KeyboardStateExtended _keyboardState;    
    private bool _tapped = false;
    private bool _touching = false;
    private TouchCollection _touchPanelState;

    public bool SlideControlPressed
    {
        get
        {
            return _tapped || Keyboard.GetState().IsKeyDown(Keys.Space);
        }
    }

    public bool IsJumpPressed
    {
        get
        {
            return Keyboard.GetState().IsKeyDown(Keys.Space)
                || GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed
                || _touching;
        }
    }

    public bool IsPausePressed
    {
        get
        {
            return _keyboardState.IsKeyDown(Keys.P);
        }
    }

    public void Initialise()
    {
        _tapped = false;
    }

    public void Update()
    {
        //_gamePadState = GamePad.GetState(PlayerIndex.One);
        _keyboardState = KeyboardExtended.GetState();
        _touchPanelState = TouchPanel.GetState();

        if (_touchPanelState.Count > 0)//.Any(x => x.State == TouchLocationState.Pressed))
        {
            _touching = true;
            _tapped = true;
        }
        else _touching = false;// if (_touchPanelState.Any(x => x.State == TouchLocationState.Released)) _touching = false;
    }
}
