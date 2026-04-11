using SlidyKitty.Code;
using SlidyKitty.Code.Shared;

var gameSettings = new GameSettings
{
    UseCurrentDisplayMode = false,
    VirtualResolution = new(1920, 1080)
};

using var game = new GameMain(gameSettings);
game.Run();
