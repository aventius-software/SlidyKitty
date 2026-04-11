using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using SlidyKitty.Code;
using SlidyKitty.Code.Shared;
using AndroidGraphics = Android.Graphics;

namespace SlidyKitty.Android
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@drawable/icon",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.UserLandscape,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]
    public class Activity : AndroidGameActivity
    {
        private GameMain _game;
        private View _view;

        protected override void OnCreate(Bundle bundle)
        {            
            base.OnCreate(bundle);

            var gameSettings = new GameSettings
            {
                UseCurrentDisplayMode = true
            };

            _game = new GameMain(gameSettings);
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

            // Create the ad view, load an ad and add it to the container. This is just AdMob's TEST ad unit for
            // for demonstration purposes, you'll need to sign up to AdMob and create your own ad unit to show real
            // ads in your app (which can be a nightmare just trying to get an account). Also note that you'll need
            // to use these in the 'AndroidManifest.xml' files (see the 'Configurations' folder). For more infor, see
            // the links below:
            // https://developers.google.com/admob/android/test-ads#sample_ad_units
            // https://developers.google.com/admob/android/quick-start#import_the_mobile_ads_sdk
            var bannerAd = new AdView(this)
            {               
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
