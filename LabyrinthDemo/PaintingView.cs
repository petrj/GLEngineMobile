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

namespace GLEngineMobileLabyrinthDemo
{
    class PaintingView : AndroidGameView
	{
        GLPoint _fingerTapCoordinates = new GLPoint();
        GLPoint _finger2TapCoordinates = new GLPoint();
        private TapCrossMoveEvent _lastTapCrossMove;
        private TapCrossMoveEvent _lastTapSideMove;
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

            Matrix4 m = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, Width / (float)Height, 1f, 200.0f);

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

            GL.Enable(All.Blend);
            GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);            
            
            var labyrinth = (_scene.GetObjectByName("labyrinth") as GLLabyrinthObj);
            if (labyrinth.Polygons.Count == 0)
                Restart();

            SetupCamera();			
		}

        public void Restart()
        {
            GLTextureAdmin.UnLoadGLTextures();

            GLTextureAdmin.AddTexturesFromResource(Context, new string[]
            {
                "labBottom", "labBottomF", "labBottomL", "labBottomS" ,
                "labTop", "labTopF", "labTopL", "labTopS" ,
                "labWall0", "labWall1", "labWall2", "labWall3" ,
                "labWallF", "labWallL", "labWallS" ,
                "money", "moneySmall", "blue0", "blue1"
            });

            var labyrinth = (_scene.GetObjectByName("labyrinth") as GLLabyrinthObj);
            labyrinth.Generate(Context);

            _scene.Observer.Position = labyrinth.LabPointToScenePoint(labyrinth.StartPos);
            _scene.Observer.Rotation = new GLVector(0, 180, 0);

            _lastTapCrossMove = null;
            _lastTapSideMove = null;

            UpdateDisplays();
        }

        protected override void OnUnload(EventArgs e)
        {

        }

        public float PxFromDp(float dp)
        {
            return dp * Context.Resources.DisplayMetrics.Density;
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
                          e.GetX() - (Width - crossSize) - marginSize,
                          e.GetY() - (Height - crossSize));
                }
                else
                {
                    // left half => side move event
                    _lastTapSideMove = TapCrossMoveEvent.GetTapMoveEvent(
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
                    // left half => side move event
                    _lastTapSideMove = null;
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
                    //_scene.Observer.Rotation.Y += _lastTapCrossMove.Right/5f;
                    _scene.Go(DirectionEnum.Right, _lastTapCrossMove.Right / 50f);
                }
                if (_lastTapCrossMove.Left > 10)
                {
                    // _scene.Observer.Rotation.Y -= _lastTapCrossMove.Left /5f;                     
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

            if (_lastTapSideMove != null)
            {
                if (_lastTapSideMove.Right > 10)
                {
                    //_scene.Go(DirectionEnum.Right, _lastTapSideMove.Right / 50f);
                    _scene.Observer.Rotation.Y += _lastTapSideMove.Right / 10f;
                }
                if (_lastTapSideMove.Left > 10)
                {
                    //_scene.Go(DirectionEnum.Left, _lastTapSideMove.Left / 50f);
                    _scene.Observer.Rotation.Y -= _lastTapSideMove.Left / 10f;
                }
            }

            var labyrinth = (_scene.GetObjectByName("labyrinth") as GLLabyrinthObj);

            var nearestBonusItem = labyrinth.GetNearestBonusItem(_scene.Observer.Position);
            if (nearestBonusItem != null)
            {
                var dist = _scene.Observer.Position.DistanceToPoint(nearestBonusItem.Position);
                if (dist < labyrinth.TileWidth)
                {
                    labyrinth.PickUpBonusItem(nearestBonusItem);
                    UpdateDisplays();
                }
            }
            
            var finishPosition = labyrinth.LabPointToScenePoint(labyrinth.EndPos);
            var distToFinish = _scene.Observer.Position.DistanceToPoint(finishPosition);
            if (distToFinish < labyrinth.TileWidth && !labyrinth.Locked)
            {
                labyrinth.Level++;                
                Restart();
            }         

            Render();
        }

        private void UpdateDisplays()
        {
            var labyrinth = (_scene.GetObjectByName("labyrinth") as GLLabyrinthObj);

                /*
             if (DebugDisplayLabel != null)
             {
                 DebugDisplayLabel.Text = $"Position: {_scene.Observer.Position.ToShortString()} Rotation: : {_scene.Observer.Rotation.ToShortString()}";
             }
             */

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
                CenterDisplayLabel.Text = labyrinth.Locked ? "Find all dollars" : "Find exit";
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
