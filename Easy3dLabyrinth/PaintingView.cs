using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using GLEngineMobile;
using LoggerService;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.Android;
using System;
using System.Threading.Tasks;

namespace Easy3DLabyrinth
{
    class PaintingView : AndroidGameView
	{
        GLPoint _fingerTapCoordinates = new GLPoint();
        GLPoint _finger2TapCoordinates = new GLPoint();
        private TapCrossMoveEvent _lastTapCrossMove;
        private TapCrossMoveEvent _lastTapRotateMove;
        private GLScene _scene;
        public TextView DebugDisplayLabel { get; set; }
        public TextView LeftDisplayLabel { get; set; }
        public TextView CenterDisplayLabel { get; set; }
        public TextView RightDisplayLabel { get; set; }

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

            Logger.InitLoggerService(new BasicLoggingService());

            var labyrinth = new GLLabyrinthObj();
            labyrinth.Name = "labyrinth";

            _scene.Objects.Add(labyrinth);

            Resize += delegate
            {
				SetupCamera ();
			};

            Run(30); // fps
            RenderFrame += PaintingView_RenderFrame;


        }

        private void SetupCamera()
        {
            GL.Viewport(0, 0, Width, Height);
            // setup projection matrix
            GL.MatrixMode(All.Projection);

            Matrix4 m = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1f, 600.0f);

            float[] perspective_m = new float[16];

            int i = 0;
            perspective_m[i + 0] = m.Row0.X; perspective_m[i + 1] = m.Row0.Y;
            perspective_m[i + 2] = m.Row0.Z; perspective_m[i + 3] = m.Row0.W;
            i += 4;

            perspective_m[i + 0] = m.Row1.X; perspective_m[i + 1] = m.Row1.Y;
            perspective_m[i + 2] = m.Row1.Z; perspective_m[i + 3] = m.Row1.W;
            i += 4;

            perspective_m[i + 0] = m.Row2.X; perspective_m[i + 1] = m.Row2.Y;
            perspective_m[i + 2] = m.Row2.Z; perspective_m[i + 3] = m.Row2.W;
            i += 4;

            perspective_m[i + 0] = m.Row3.X; perspective_m[i + 1] = m.Row3.Y;
            perspective_m[i + 2] = m.Row3.Z; perspective_m[i + 3] = m.Row3.W;

            GL.LoadMatrix(perspective_m);
        }

        // This method is called everytime the context needs
        // to be recreated. Use it to set any egl-specific settings
        // prior to context creation
        protected override void CreateFrameBuffer ()
		{
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
            GL.ShadeModel(All.Smooth);
            //GL.ShadeModel(All.Flat);
            GL.ClearColor (1f, 1f, 1f, 1f);
            GL.Color4(1f, 1f, 1f, 1f);

            GL.ClearDepth (1.0f);
            GL.Enable (All.DepthTest);
            GL.DepthFunc (All.Lequal);
            GL.Disable(All.Blend);

            GL.Enable(All.Lighting);
            GL.Enable(All.Light0);
            GL.Enable(All.Light1);

            var labyrinth = (_scene.GetObjectByName("labyrinth") as GLLabyrinthObj);
            if (labyrinth.Polygons.Count == 0)
                NewLevel();

            SetupCamera();
		}

        public void NewLevel()
        {
            GLTextureAdmin.UnLoadGLTextures();

            GLTextureAdmin.AddTexturesFromResource(Context, new string[]
            {
                "blue0", "blue1", "labBottomF", "labBottomL", "labBottomS" ,
                "labTop", "labTopF", "labTopL", "labTopS" ,
                "labWall0", "labWall1", "labWall2", "labWall3" ,
                "labWallF", "labWallL", "labWallS" ,
                "money", "moneySmall", "blue0", "labBottom"
            });

            var labyrinth = (_scene.GetObjectByName("labyrinth") as GLLabyrinthObj);

            int moves;
            int items;

            if (labyrinth.Level <= 5) // 1 .. 5 level => 5 * 2 .. 25 * 10
            {
                moves = labyrinth.Level * 5;
                items = labyrinth.Level * 2;
            } else
            if (labyrinth.Level <= 20) // 6 .. 20 level =>  26 * 11 .. 40 * 25
            {
                moves = 20 + labyrinth.Level;
                items = 5 + labyrinth.Level;
            } else
            {
                moves = 40;
                items = 25;
            }

            labyrinth.Generate(Context, moves, items);

            _scene.Observer.Position = labyrinth.LabPointToScenePoint(labyrinth.StartPos);
            _scene.Observer.Rotation = new GLVector(0, 180, 0);

            _lastTapCrossMove = null;
            _lastTapRotateMove = null;

            UpdateDisplays();
        }

        protected override void OnUnload(EventArgs e)
        {

        }

        public float PxFromDp(float dp)
        {
            return dp * Context.Resources.DisplayMetrics.Density;
        }

        public void OnKeyboarUp(KeyboardEvent e)
        {

        }

        public void OnKeyboardDown(KeyboardEvent e)
        {
            var speed = 5;

            switch (e.Key)
            {
                case Keycode.DpadUp:
                case Keycode.W:
                    _scene.Go(DirectionEnum.Forward, speed);
                    break;
                case Keycode.DpadDown:
                case Keycode.S:
                    _scene.Go(DirectionEnum.Backward, speed);
                    break;
                case Keycode.DpadRight:
                case Keycode.D:
                    _scene.Observer.Rotation.Y += speed;
                    break;
                case Keycode.DpadLeft:
                case Keycode.A:
                    _scene.Observer.Rotation.Y -= speed;
                    break;
                case Keycode.ButtonL1:
                case Keycode.ButtonL2:
                case Keycode.Z:
                    _scene.Go(DirectionEnum.Left, speed);
                    break;
                case Keycode.ButtonR1:
                case Keycode.ButtonR2:
                case Keycode.X:
                    _scene.Go(DirectionEnum.Right, speed);
                    break;
            }
        }

        public override bool OnTouchEvent (MotionEvent e)
	    {
            var crossSize = Convert.ToInt32(PxFromDp(135));
            var marginSize = Convert.ToInt32(PxFromDp(20));

            // 1 finger for every half of screen

            if (
                (e.Action == MotionEventActions.Down  ||
                e.Action == MotionEventActions.Move  ||
                e.Action == MotionEventActions.Pointer1Down ||
                e.Action == MotionEventActions.Pointer2Down)
                )
            {
                if (e.GetX() > Width / 2f)
                {
                    // right half => cross tap event

                    _lastTapCrossMove = TapCrossMoveEvent.GetTapMoveEvent(
                          crossSize,
                          crossSize,
                           e.GetX() - (Width - crossSize - marginSize),
                          e.GetY() - (Height - crossSize));
                }
                else
                {
                    // left half => rotation
                    _lastTapRotateMove = TapCrossMoveEvent.GetTapMoveEvent(
                          crossSize,
                          crossSize,
                          e.GetX() - marginSize,
                          e.GetY() - (Height - crossSize));
                }

                return true;
            } else
            if (e.Action == MotionEventActions.Up ||
                e.Action == MotionEventActions.Pointer1Up ||
                e.Action == MotionEventActions.Pointer2Up)
            {
                if (e.GetX() > Width / 2f)
                {
                    // right half => cross tap event

                    _lastTapCrossMove = null;
                }
                else
                {
                    // left half => no rotation
                    _lastTapRotateMove = null;
                }

                return true;
            } else
            {
                return base.OnTouchEvent(e);
            }
		}


        private void PaintingView_RenderFrame(object sender, FrameEventArgs e)
        {

            if (_lastTapCrossMove != null)
            {
                if (_lastTapCrossMove.Right > 10)
                {
                    _scene.Go(DirectionEnum.Right, _lastTapCrossMove.Right / 50f);
                }
                if (_lastTapCrossMove.Left > 10)
                {
                    _scene.Go(DirectionEnum.Left, _lastTapCrossMove.Left / 50f);
                }
                if (_lastTapCrossMove.Top > 10)
                {
                    _scene.Go(DirectionEnum.Forward, _lastTapCrossMove.Top/10f);
                }
                if (_lastTapCrossMove.Bottom > 10)
                {
                    _scene.Go(DirectionEnum.Backward, _lastTapCrossMove.Bottom / 10f);
                }
            }

            if (_lastTapRotateMove != null)
            {
                if (_lastTapRotateMove.Right > 10)
                {
                    _scene.Observer.Rotation.Y += _lastTapRotateMove.Right / 10f;
                }
                if (_lastTapRotateMove.Left > 10)
                {
                    _scene.Observer.Rotation.Y -= _lastTapRotateMove.Left / 10f;
                }
            }

            var labyrinth = (_scene.GetObjectByName("labyrinth") as GLLabyrinthObj);

            var newLevel = labyrinth.CheckPosition(_scene.Observer.Position);
            UpdateDisplays(newLevel ? "Loading new level ..." : null);
            if (newLevel)
            {
                NewLevel();
            }


            // adding light above Observer

            GL.Light(All.Light0, All.Position, _scene.Observer.Position.ToFloatArray());

            GL.Light(All.Light0, All.Ambient, new float[] { 1f, 1f, 1f, 1f });
            GL.Light(All.Light0, All.Diffuse, new float[] { 1f, 1f, 1f, 1f });
            GL.Light(All.Light0, All.Specular, new float[] { 1f, 1f, 1f, 1f });

            // adding light in front of Observer
            var pBefore = GLPoint.GetMovedPointByAngle(_scene.Observer.Position, labyrinth.TileWidth, _scene.Observer.Rotation.Y, true);
            GL.Light(All.Light1, All.Position, pBefore.ToFloatArray());

            GL.Light(All.Light1, All.Ambient, new float[] { 1f, 1f, 1f, 1f });
            GL.Light(All.Light1, All.Diffuse, new float[] { 1f, 1f, 1f, 1f });
            GL.Light(All.Light1, All.Specular, new float[] { 1f, 1f, 1f, 1f });

            GL.Material(All.Front, All.Ambient, new float[] { 1f, 1f, 1f });
            GL.Material(All.Front, All.Diffuse, new float[] { 1f, 1f, 1f });
            GL.Material(All.Front, All.Specular, new float[] { 1f, 1f, 1f });
            GL.Material(All.Front, All.Shininess, 128f);


            Render();
        }

        private void UpdateDisplays(string centerDisplayText = null)
        {
            var labyrinth = (_scene.GetObjectByName("labyrinth") as GLLabyrinthObj);

            if (LeftDisplayLabel != null)
            {
                LeftDisplayLabel.Text = $"{labyrinth.FinishCount}/{labyrinth.BonusItemsCount}";
            }
            if (RightDisplayLabel != null)
            {
                RightDisplayLabel.Text = $"Level {labyrinth.Level}";
            }
            if (CenterDisplayLabel != null)
            {
                if (centerDisplayText == null)
                {
                    CenterDisplayLabel.Text = labyrinth.Locked ? "Find all diamonds" : "Find exit";
                } else
                {
                    CenterDisplayLabel.Text = centerDisplayText;
                }
            }
        }

        void Render ()
		{
            MakeCurrent();

            _scene.Render();

            SwapBuffers();
        }

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}

        protected override void Dispose(bool disposing)
        {
            GLTextureAdmin.UnLoadGLTextures();
            base.Dispose(disposing);
        }
    }
}
