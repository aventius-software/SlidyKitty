using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.Extended.Screens;

namespace SlidyKitty.Code.Screens;

internal class PauseScreen : Screen
{
    private readonly ScreenManager _screenManager;

    public PauseScreen(ScreenManager screenManager)
    {
        _screenManager = screenManager;
    }

    public override void Draw(GameTime gameTime)
    {
        // TODO: draw some pause screen stuff?
    }

    public override void Update(GameTime gameTime)
    {
        var keyboardState = KeyboardExtended.GetState();

        // If the pause toggle was pressed, we unpause and return to the gameplay screen
        if (keyboardState.WasKeyPressed(Keys.P))
            _screenManager.CloseScreen();
    }
}
