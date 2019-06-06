using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content.PM;
using Android.Views;
using LoggerService;
using GLEngineMobileDemo;

namespace GLEngineMobileDemo
{    
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/app_texturedcube",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : AppCompatActivity
    {
        //View mMenuContainer, mSwitchTexture;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Inflate our UI from its XML layout description
            SetContentView(Resource.Layout.main);

            PaintingView glp = FindViewById<PaintingView>(Resource.Id.paintingview);

            /*
            // Find the views whose visibility will change
            mMenuContainer = FindViewById(Resource.Id.hidecontainer);
            mSwitchTexture = FindViewById(Resource.Id.switch_texture);
            //mSwitchTexture.Click += delegate { glp.SwitchTexture(); };

            // Find our buttons
            Button showButton = FindViewById<Button>(Resource.Id.show);
            Button hideButton = FindViewById<Button>(Resource.Id.hide);

            // Wire each button to a click listener
            showButton.Click += delegate { SetVisibility(ViewStates.Visible); };
            hideButton.Click += delegate { SetVisibility(ViewStates.Gone); };
            */

            Logger.InitLoggerService(new BasicLoggingService());
            Logger.Info("Starting activity");
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void SetVisibility(ViewStates state)
        {
            //mSwitchTexture.Visibility = state;
            //mMenuContainer.Visibility = state;
        }
    }
}