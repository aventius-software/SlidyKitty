using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using SlidyKitty.Code;
using AndroidGraphics = Android.Graphics;

namespace SlidyKitty.Android
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.FullUser,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class Activity : AndroidGameActivity
    {
        private GameMain _game;
        private View _view;

        protected override void OnCreate(Bundle bundle)
        {
            //base.OnCreate(bundle);

            //_game = new GameMain();
            //_view = _game.Services.GetService(typeof(View)) as View;

            //SetContentView(_view);
            //_game.Run();

            base.OnCreate(bundle);

            _game = new GameMain();
            _view = _game.Services.GetService(typeof(View)) as View;

            // A container to show the add at the top of the page            
            var adContainer = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };

            adContainer.SetGravity(GravityFlags.CenterHorizontal | GravityFlags.Top);

            // Need on some devices, not sure why
            adContainer.SetBackgroundColor(AndroidGraphics.Color.Transparent);

            // A layout to hold the ad container and game view
            var mainLayout = new FrameLayout(this);
            mainLayout.AddView(_view);
            mainLayout.AddView(adContainer);

            SetContentView(mainLayout);

            // Create the ad view, load an ad and add it to the container
            var bannerAd = new AdView(this)
            {
                // This is AdMob's test unit id, for more information see this
                // link https://developers.google.com/admob/android/test-ads 
                AdUnitId = "ca-app-pub-3940256099942544/9214589741",
                AdSize = AdSize.Banner
            };

            bannerAd.LoadAd(new AdRequest.Builder().Build());

            // Show the ad
            adContainer.AddView(bannerAd);

            // Run game as per normal and hopefully we should see the ad at the top of the page ;-)
            _game.Run();
        }
    }
}
