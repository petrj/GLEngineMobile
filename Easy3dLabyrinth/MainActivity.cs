using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content.PM;
using Android.Views;
using LoggerService;
using System;

namespace Easy3DLabyrinth
{    
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/Easy3DLabyrinth",
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden, LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            // Inflate our UI from its XML layout description
            SetContentView(Resource.Layout.main);

            SupportActionBar.Hide();            
            //Window.Attributes.ScreenBrightness = 1f;  // override system display brightness

            var paintingView = FindViewById<PaintingView>(Resource.Id.paintingview);
            paintingView.DebugDisplayLabel = FindViewById<TextView>(Resource.Id.debugDisplayTextView);
            paintingView.LeftDisplayLabel = FindViewById<TextView>(Resource.Id.leftDisplayTextView);
            paintingView.CenterDisplayLabel = FindViewById<TextView>(Resource.Id.centerDisplayTextView);
            paintingView.RightDisplayLabel = FindViewById<TextView>(Resource.Id.rightDisplayTextView);            

            Logger.InitLoggerService(new BasicLoggingService());
            Logger.Info("Starting Labyrinth demo");
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            var paintingView = FindViewById<PaintingView>(Resource.Id.paintingview);

            paintingView.OnKeyboarUp(new KeyboardEvent()
            {
                Event = e,
                Key = keyCode,
                Time = DateTime.Now
            });

            return base.OnKeyUp(keyCode, e);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            var paintingView = FindViewById<PaintingView>(Resource.Id.paintingview);         
            
            paintingView.OnKeyboardDown(new KeyboardEvent()
            {
                Event = e,
                Key = keyCode,
                Time = DateTime.Now
            });

#if DEBUG
            /*
            var display = FindViewById<TextView>(Resource.Id.debugDisplayTextView);
            if (display != null)
            {               
                   display.Text = $"Key: KeyCode: {keyCode}";              
            }
            */
#endif

            if ((keyCode == Keycode.Del) || (keyCode == Keycode.ForwardDel))
            {
                paintingView.NewLevel();
            }

            if ((keyCode == Keycode.DpadCenter) || (keyCode == Keycode.Space))
            {
                var cross = FindViewById<ImageView>(Resource.Id.cross);
                if (cross != null)
                {
                    if (cross.Visibility == ViewStates.Visible)                    
                        cross.Visibility = ViewStates.Invisible;                    
                    else if (cross.Visibility == ViewStates.Invisible)                    
                        cross.Visibility = ViewStates.Visible;                    
                }
                var sidemove = FindViewById<ImageView>(Resource.Id.sidemove);
                if (sidemove != null)
                {
                    if (sidemove.Visibility == ViewStates.Visible)
                        sidemove.Visibility = ViewStates.Invisible;
                    else if (sidemove.Visibility == ViewStates.Invisible)
                        sidemove.Visibility = ViewStates.Visible;
                }
            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}