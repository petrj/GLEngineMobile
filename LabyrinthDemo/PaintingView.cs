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

namespace GLEngineMobileLabyrinthDemo
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

            //_scene.Observer.Rotation.X = -45;
            //_scene.Observer.Rotation.Y = 20;

            //var elipse = new GLEllipse()
            //{
            //    RadiusMajor = 4,
            //    RadiusMinor = 4,
            //    FillColor = Color.Yellow
            //};

            var labyrinth = new GLLabyrinthObj();
            labyrinth.Name = "labyrinth";
            labyrinth.Generate();
            _scene.Observer.Position = labyrinth.LabPointToScenePoint(labyrinth.StartPos);
            _scene.Observer.Rotation = new GLVector(0, 0, 0);

            _scene.Objects.Add(labyrinth);

            Resize += delegate 
            {
				height = Height;
				width = Width;
				SetupCamera ();
			};

            //Render();

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
            GL.ClearColor (0, 0, 0, 1);

			GL.ClearDepth (1.0f);
			GL.Enable (All.DepthTest);
			GL.DepthFunc (All.Lequal);
            
            /*
			GL.Enable (All.CullFace);
			GL.CullFace (All.Back);

			GL.Hint (All.PerspectiveCorrectionHint, All.Nicest);
            GL.Enable(All.Texture2D);           
            */

            _scene.LoadTextures(Context);

            GLTextureAdmin.AddTextureFromResource(Context, "blue0");
            GLTextureAdmin.AddTextureFromResource(Context, "blue1");

            GLTextureAdmin.AddTextureFromResource(Context, "labBottom");
            GLTextureAdmin.AddTextureFromResource(Context, "labBottomF");
            GLTextureAdmin.AddTextureFromResource(Context, "labBottomL");
            GLTextureAdmin.AddTextureFromResource(Context, "labBottomS");

            GLTextureAdmin.AddTextureFromResource(Context, "labTop");
            GLTextureAdmin.AddTextureFromResource(Context, "labTopF");
            GLTextureAdmin.AddTextureFromResource(Context, "labTopL");
            GLTextureAdmin.AddTextureFromResource(Context, "labTopS");

            GLTextureAdmin.AddTextureFromResource(Context, "labWall0");
            GLTextureAdmin.AddTextureFromResource(Context, "labWall1");
            GLTextureAdmin.AddTextureFromResource(Context, "labWall2");
            GLTextureAdmin.AddTextureFromResource(Context, "labWall3");

            GLTextureAdmin.AddTextureFromResource(Context, "labWallF");
            GLTextureAdmin.AddTextureFromResource(Context, "labWallL");
            GLTextureAdmin.AddTextureFromResource(Context, "labWallS");

            GLTextureAdmin.AddTextureFromResource(Context, "money");
            GLTextureAdmin.AddTextureFromResource(Context, "moneySmall");

            SetupCamera();			
		}

		void SetupCamera ()
		{
			width = Width;
			height = Height;

			GL.Viewport(0, 0, width, height);
			// setup projection matrix
			GL.MatrixMode(All.Projection);
			//GL.LoadIdentity();

            // gluPerspective
            //Matrix4 m = Matrix4.CreatePerspectiveFieldOfView (ToRadians (45.0f), (float)width / (float)height, 1.0f, 10000.0f);
            Matrix4 m = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1f, 500.0f);

            
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
            
            //GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix (perspective_m);
		}

        public float PxFromDp(float dp)
        {
            return dp * Context.Resources.DisplayMetrics.Density;
        }

        public override bool OnTouchEvent (MotionEvent e)
	    {
            var tcme = TapCrossMoveEvent.GetTapMoveEvent(Convert.ToInt32(PxFromDp(135)), Convert.ToInt32(PxFromDp(135)), e.GetX(), e.GetY() - (Height - 135)); 
                        
            if (tcme != null)
            {                
                if (tcme.Right > 0)
                {
                    _scene.Observer.Rotation.X += 0.5;
                }
                if (tcme.Left > 0)
                {
                    _scene.Observer.Rotation.X -= 0.5;
                }
                if (tcme.Top > 0)
                {
                  
                }
                if (tcme.Bottom > 0)
                {
                    
                }

                return true;

            } else
            {
                return base.OnTouchEvent(e);
            }            
		}

		protected override void OnUnload (EventArgs e)
		{
            
        }

		void Render ()
		{
            MakeCurrent();

            _scene.Render();        
            
            if (RotationLabel != null)
            {
                RotationLabel.Text = $"Position: {_scene.Observer.Position.ToString()} Rotation: : {_scene.Observer.Rotation.ToString()}";
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
