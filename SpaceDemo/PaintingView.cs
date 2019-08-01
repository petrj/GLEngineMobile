using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using OpenTK;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System.Collections.Generic;
using GLEngineMobile;
using LoggerService;
using Android.Content.Res;
using System.IO;
using Android.Widget;
using System.Threading.Tasks;

namespace GLEngineMobileSpaceDemo
{
	class PaintingView : AndroidGameView
	{
        GLPoint _fingerTapCoordinates = new GLPoint();
        GLPoint _finger2TapCoordinates = new GLPoint();        
        bool _zoom = false;
        int width, height;
        GLScene _scene;
        public TextView RotationLabel { get; set; }

        public PaintingView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize ();
		}
		public PaintingView (IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
			: base (handle, transfer)
		{
			Initialize ();
		}

		private void Initialize ()
		{
            _scene = new GLScene();         

            //_scene.Objects.Add(new GLAxis());
            _scene.Objects.Add(new GLStarSpace() { Count = 200 });            

            Resize += delegate 
            {
				height = Height;
				width = Width;
				SetupCamera ();
			};

            Run(20); // 20 fps

            RenderFrame += PaintingView_RenderFrame;            
        }

        private void PaintingView_RenderFrame(object sender, FrameEventArgs e)
        {
            Render();
        }

        // This method is called everytime the context needs
        // to be recreated. Use it to set any egl-specific settings
        // prior to context creation
        protected override void CreateFrameBuffer ()
		{
			GLContextVersion = GLContextVersion.Gles1_1;

			// the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
			try
            {
				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();

				return;
			} catch (Exception ex)
            {
				Logger.Error(ex);
			}

			// Fallback modes
			// If the first attempt at initializing the surface with a default graphics
			// mode fails, then the app can try different configurations. Devices will
			// support different modes, and what is valid for one might not be valid for
			// another. If all options fail, you can set all values to 0, which will
			// ask for the first available configuration the device has without any
			// filtering.
			// After a successful call to base.CreateFrameBuffer(), the GraphicsMode
			// object will have its values filled with the actual values that the
			// device returned.


			// This is a setting that asks for any available 16-bit color mode with no
			// other filters. It passes 0 to the buffers parameter, which is an invalid
			// setting in the default OpenTK implementation but is valid in some
			// Android implementations, so the AndroidGraphicsMode object allows it.
			try
            {
				Logger.Debug("Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode (16, 0, 0, 0, 0, false);

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex)
            {
                Logger.Error(ex);
			}

			// this is a setting that doesn't specify any color values. Certain devices
			// return invalid graphics modes when any color level is requested, and in
			// those cases, the only way to get a valid mode is to not specify anything,
			// even requesting a default value of 0 would return an invalid mode.
			try {
                Logger.Debug("Loading with no Android settings");
				GraphicsMode = new AndroidGraphicsMode (0, 4, 0, 0, 0, false);

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex)
            {
                Logger.Error(ex);
			}
			throw new Exception ("Context create error, aborting");
		}

		protected override void OnLoad (EventArgs e)
		{              
            //GL.ShadeModel (All.Flat);
            GL.ShadeModel(All.Smooth);
            GL.ClearColor(0, 0, 0, 1);
            GL.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));

            GL.ClearDepth(1.0f);
            GL.Enable(All.DepthTest);
            GL.DepthFunc(All.Lequal);

            GLTextureAdmin.UnLoadGLTextures ();
            GLTextureAdmin.AddTexturesFromResource(Context, new string[]
            { "earth", "moon", "tex", "darkgray", "f_spot", "pattern", "borg" });

            _scene.LoadFromAndroidAsset(Context, "scene.xml");

            var ent = _scene.GetObjectByName("Enterprise") as GLSpaceShip;
            ent.MoveToCenter();
            ent.Magnify(0.12);
            ent.OrbitEllipse.RadiusMajor = 6;
            ent.OrbitEllipse.RadiusMinor = 5;
            ent.OrbitAngle = 90;
           
            SetupCamera();			
		}

		void SetupCamera ()
		{
			width = Width;
			height = Height;

			GL.Viewport(0, 0, width, height);
			// setup projection matrix
			GL.MatrixMode(All.Projection);
			GL.LoadIdentity();

			// gluPerspective
			Matrix4 m = Matrix4.CreatePerspectiveFieldOfView (ToRadians (45.0f), (float)width / (float)height, 1.0f, 100.0f);
			float [] perspective_m = new float [16];

			int i = 0;
			perspective_m [i + 0] = m.Row0.X; perspective_m [i + 1] = m.Row0.Y;
			perspective_m [i + 2] = m.Row0.Z; perspective_m [i + 3] = m.Row0.W;
			i += 4;

			perspective_m [i + 0] = m.Row1.X; perspective_m [i + 1] = m.Row1.Y;
			perspective_m [i + 2] = m.Row1.Z; perspective_m [i + 3] = m.Row1.W;
			i += 4;

			perspective_m [i + 0] = m.Row2.X; perspective_m [i + 1] = m.Row2.Y;
			perspective_m [i + 2] = m.Row2.Z; perspective_m [i + 3] = m.Row2.W;
			i += 4;

			perspective_m [i + 0] = m.Row3.X; perspective_m [i + 1] = m.Row3.Y;
			perspective_m [i + 2] = m.Row3.Z; perspective_m [i + 3] = m.Row3.W;

			GL.LoadMatrix (perspective_m);
		}
        
		public override bool OnTouchEvent (MotionEvent e)
		{   
            var x = e.GetX();
            var y = e.GetY();

            base.OnTouchEvent (e);

            if (e.Action == MotionEventActions.Down)
            {
                _fingerTapCoordinates.X = x;
                _fingerTapCoordinates.Y = y;

                Logger.Info($"Down: {x}:{y}");
            }           
            else
            if (e.Action == MotionEventActions.Pointer2Down)
            {
                // second finger down 

                Logger.Info($"Pointer2Down: {x}:{y}");
            } else
			if (e.Action == MotionEventActions.Move)
            {
                if (e.PointerCount > 1)
                {
                    if (!_zoom)
                    {
                        _zoom = true;                        

                        _fingerTapCoordinates = new GLPoint(e.GetX(0), e.GetY(0), 0);
                        _finger2TapCoordinates = new GLPoint(e.GetX(1), e.GetY(1), 0);
                    }
                    else
                    {
                        Logger.Info($"Zoom:");

                        var originalDistance = _fingerTapCoordinates.DistanceToPoint(_finger2TapCoordinates);

                        var actualFinger1TapCoordinates = new GLPoint(e.GetX(0), e.GetY(0), 0);
                        var actualFinger2TapCoordinates = new GLPoint(e.GetX(1), e.GetY(1), 0);

                        var newDistance = actualFinger1TapCoordinates.DistanceToPoint(actualFinger2TapCoordinates);

                        if (originalDistance != 0)
                        {
                            var ratio = newDistance / originalDistance;

                            if (ratio < 0.9)
                            {
                                _scene.Magnify(0.9f);
                            }
                            else
                            if (ratio > 1.1)
                            {
                                _scene.Magnify(1.1f);
                            }

                            Logger.Info($"Move: PinchToZoom, originalDistance: {originalDistance:N2}, newDistance: {newDistance:N2}, ratio: {ratio}");
                        }
                    }
                }
                else if (!_zoom)
                {
                    Logger.Info($"Move:");
                    
                    float xdiff = ((float)_fingerTapCoordinates.X - x);
                    float ydiff = ((float)_fingerTapCoordinates.Y - y);

                    _scene.Observer.Rotation.X += ydiff;
                    _scene.Observer.Rotation.Y += xdiff;

                    _fingerTapCoordinates.X = x;
                    _fingerTapCoordinates.Y = y;
                }
			}

            if (e.Action == MotionEventActions.Up)
            {
                _zoom = false;
            }            

            return true;
		}

		protected override void OnUnload (EventArgs e)
		{
            
        }

		void Render ()
		{
            MakeCurrent();

            _scene.Render();

            var enterprise = _scene.GetObjectByName("Enterprise") as GLSpaceShip;
            enterprise.OrbitAngle -= 3;

            var planet = _scene.GetObjectByName("Earth") as GLPlanet;
            foreach (var satellite in planet.Satellites)
            {
                satellite.OrbitAngle = satellite.OrbitAngle + 2;
            }
            planet.Rotation.Y += 5;

            if (RotationLabel != null)
            {
                RotationLabel.Text = $"Rotation: : {_scene.Observer.Rotation.ToString()}";
            }

            SwapBuffers();
		}
		
		protected override void Dispose (bool disposing)
		{
            GLTextureAdmin.UnLoadGLTextures();
            base.Dispose (disposing);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);			
		}

		public static float ToRadians (float degrees)
		{
			//pi/180
			//FIXME: precalc pi/180
			return (float) (degrees * (System.Math.PI/180.0));
		}		
	}
}
