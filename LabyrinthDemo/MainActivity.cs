using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content.PM;
using Android.Views;
using LoggerService;


namespace GLEngineMobileLabyrinthDemo
{    
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/LabyrinthDemoIcon",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Inflate our UI from its XML layout description
            SetContentView(Resource.Layout.main);

            var paintingView = FindViewById<PaintingView>(Resource.Id.paintingview);
            paintingView.DebugDisplayLabel = FindViewById<TextView>(Resource.Id.debugDisplayTextView);
         
            Logger.InitLoggerService(new BasicLoggingService());
            Logger.Info("Starting labyrinth demo");
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}