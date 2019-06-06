using System;
using System.Xml;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using System.Collections.Generic;
using Android.Content;
using LoggerService;

namespace GLEngineMobile
{
	public class GLScene : IDisposable
	{
		private DateTime _FullScreenSetTime;

		public List<GLObj> Objects { get; set; }

		public string FileName { get; set; }

		public GLObj Observer { get; set; }

		public string TexturesDirectory { get; set; }
		public string ObjectsDirectory { get; set; }

		public AutoPilotStateEnum State { get; set; }

		public GLScene ()
		{
			Observer = new GLObj("observer");
			Objects = new  List<GLObj>();
			TexturesDirectory = "tex";
			ObjectsDirectory = "obj";
			_FullScreenSetTime = DateTime.Now.AddSeconds (-60);
			State = AutoPilotStateEnum.Stoppped;
		}
	
		public void LoadObjects()
		{
			AddObjectsFromDir(ObjectsDirectory);       
		}

		public void LoadTextures(Context context)
		{
            GLTextureAdmin.AddTextureFromResource(context, "tex");
            GLTextureAdmin.AddTextureFromResource(context, "darkgray");
            GLTextureAdmin.AddTextureFromResource(context, "f_spot");
            GLTextureAdmin.AddTextureFromResource(context, "pattern");
		}

		public GLObj GetObjectByName(string name)
		{
			foreach (var obj in Objects)
			{
				if (obj.Name == name)
				{
					return obj;
				}
			}

			return null;
		}

		public void Render()
		{
			foreach (var obj in Objects)
			{
				obj.Render();									
			}
		}

		public void AddObjectsFromDir(string directory, bool recursive = false)
		{
			if (Directory.Exists(directory))
			{
				var files = Directory.GetFiles(directory, "*.xml", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
				foreach (var f in files)
				{
					var obj = new GLObject();
					obj.LoadFromFile(f);

					// moving to center
					var c = obj.Center;
					obj.Move(obj.Position.X-c.X,obj.Position.Y-c.Y,obj.Position.Z-c.Z);

					Objects.Add(obj);
				}
			}
		}

		public  GLPolygon NearestPolygon(GLPoint P)
		{
			GLPolygon nearestPolygon = null;

			var minDistance = double.MaxValue;
			var i = 0;
			foreach (var obj in Objects)
			{
				if (obj is GLObject)
				{
					var d = obj.DistanceToPoint(P);

					if (d == -1) 
					{
						// some error
						continue;
					}

					if (d<minDistance)
					{
						minDistance = d;
						nearestPolygon = (obj as GLObject).NearestPolygon (P);
					}
				} 
				i++;
			}			

			return nearestPolygon;
		} 

		public void ReLoad()
		{
			if ((!String.IsNullOrEmpty (FileName)) && (File.Exists(FileName)))
			{
				Load (FileName);
			}
		}

		public void Load(string fileName)
		{
			FileName = fileName;
			Objects = new List<GLObj>();

			var xmlDoc = new XmlDocument();

			xmlDoc.Load(fileName);

			var sceneNode = xmlDoc.SelectSingleNode("//scene");
			if(sceneNode != null)
			{
				var objs = sceneNode.SelectNodes ("./obj");
				foreach (XmlElement objElement in objs) 
				{
					if ((objElement.HasAttribute ("name")) && (objElement.GetAttribute ("name") == "Observer")) {
						Observer.LoadFromXmlElement (objElement);
					} else 
					{
						var obj = new GLObject ();
						obj.LoadFromXmlElement (objElement);

						Objects.Add (obj);					
					}
				}
			}
		}

        /*
		public void DoMoveOnMouse(MouseMoveEventArgs mouseEventArgs)
		{
			var xMove = mouseEventArgs.XDelta;

			if (xMove<0) Observer.Rotation.Y -= 1;
			if (xMove>0) Observer.Rotation.Y += 1;

			var yMove = mouseEventArgs.YDelta;

			if (yMove<0) Observer.Rotation.X += 1;
			if (yMove>0) Observer.Rotation.X -= 1;
		}

		public void DoSetFullScreenOnKeyboard(KeyboardDevice Keyboard, GameWindow window)		
		{
			if (Keyboard[Key.Enter] && (Keyboard[Key.AltLeft] || Keyboard[Key.AltRight]))
			{
				if ((DateTime.Now - _FullScreenSetTime).TotalSeconds > 1) 
				{
					_FullScreenSetTime = DateTime.Now;
					window.WindowState = window.WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;
				}
			}

		}
      
		public void DoMoveOnKeyboard(KeyboardDevice Keyboard)
		{
			// user moves:

			var availableActionKeys = new Dictionary<string,Key>() 
			{
				//{"RotateX+",Key.Keypad6},
				//{"RotateX-",Key.Keypad4},

				{"RotateXZ+",Key.Keypad7},
				{"RotateXZ-",Key.Keypad1},
				{"Home",Key.Home},									            	 		

				{"Up",Key.Keypad8},
				{"Down",Key.Keypad2},
				{"Forward",Key.Up},
				{"Backward",Key.Down},
				{"RotateLeft",Key.Left},
				{"RotateRight",Key.Right},

				{"Left2",Key.Keypad4},
				{"Right2",Key.Keypad6},

				{"Forward2",Key.W},
				{"Backward2",Key.S},
				{"Left",Key.A},
				{"Right",Key.D},

				{"Zoom+",Key.KeypadPlus},
				{"Zoom-",Key.KeypadMinus},

				{"Enter",Key.Enter},
			};										            	  

			foreach (var keyAction in availableActionKeys)
			{
				if (Keyboard[keyAction.Value])
				{								
					var action = keyAction.Key;

					switch (action)
					{

                        //case "RotateX+":
                        //Observer.Rotation.X += 5;
                        //break;
                        //case "RotateX-":
                        //Observer.Rotation.X -= 5;
                        //break;
                    
						case "RotateY+":
						Observer.Rotation.Y += 5;
						break;
						case "RotateY-":
						Observer.Rotation.Y -= 5;
						break;
						case "RotateXZ+":
						Observer.Rotation.X += 5;
						break;
						case "RotateXZ-":
						Observer.Rotation.X -= 5;
						break;		

						case "Forward":								
						case "Forward2":	
						Go(0);
						break;	

						case "Backward":
						case "Backward2":
						Go(1);
						break;	

						case "Up":
						Observer.Position.Y -= 1;							
						break;	

						case "Down":
						Observer.Position.Y += 1;							
						break;

						case "Left":
						case "Left2":
						Go(2);
						break;	

						case "Right":
						case "Right2":
						Go(3);
						break;

						case "RotateLeft":
						Observer.Rotation.Y -= 2;							
						break;		

						case "RotateRight":
						Observer.Rotation.Y += 2;							
						break;									

						case "Home":	
						ReLoad ();
						break;	

						case "Zoom+":
						//Scene.Magnify(1.1);						
						break;

						case "Zoom-":
						//Scene.Magnify(0.9);
						break;		

						case "Enter":
							
						break;			
					}
				}
			}
        }
        */


		/// <summary>
		/// Go the specified direction.
		/// </summary>
		/// <param name="direction">Direction.
		/// 0 .. forward (angle 0)
		/// 1 .. backward  (angle 0)
		/// 2 .. left  (angle -90)
		/// 3 .. right</param>  (angle +90)
		public void Go(int direction, double stepSize = 2)
		{
			GLPoint movedPoint = new GLPoint(0,0,0);
			switch (direction)
			{
				case 0:  movedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,stepSize,Observer.Rotation.Y,true);break;
				case 1:  movedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,stepSize,Observer.Rotation.Y,false);break;
				case 2:  movedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,stepSize,Observer.Rotation.Y-90,true);break;
				case 3:  movedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,stepSize,Observer.Rotation.Y+90,true);break;
			}

			//Observer.Position = movedPoint;


			var nearestPolygon = NearestPolygon (movedPoint);
			var dist =  nearestPolygon.DistanceToPoint(new GLPoint(-movedPoint.X,movedPoint.Y,-movedPoint.Z));

			//Logger.WriteToLog("Distance to labyrinth:" + dist);            
			if (dist>5)
			{
				Observer.Position = movedPoint;
			} else
			{
				// collision 		

				var angle = nearestPolygon.AngleToVec(new GLVector(Observer.Position,movedPoint));
				/*

				//Logger.WriteToLog("Angle to collision polygon: " + angle.ToString());			

				if ((angle<65) && (direction<2))
				{
					// sliding 	angle left
					GLPoint leftRotatedMovedPoint = new GLPoint(0,0,0);
					switch (direction)
					{
						case 0:  leftRotatedMovedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,3,Observer.Rotation.Y-angle,true);break;
						case 1:  leftRotatedMovedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,3,Observer.Rotation.Y-angle,false);break;		        		
					}

					var distLeft =  DistanceToPoint(new GLPoint(-leftRotatedMovedPoint.X,leftRotatedMovedPoint.Y+8,-leftRotatedMovedPoint.Z));
					if (distLeft>5)
					{
						observer.Position = leftRotatedMovedPoint;				
					} else
					{
						GLPoint rightRotatedMovedPoint = new GLPoint(0,0,0);
						switch (direction)
						{
							case 0:  rightRotatedMovedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,3,Observer.Rotation.Y+angle,true);break;
							case 1:  rightRotatedMovedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,3,Observer.Rotation.Y+angle,false);break;		        		
						}

						var distRight =  labyrinth.DistanceToPoint(new GLPoint(-rightRotatedMovedPoint.X,rightRotatedMovedPoint.Y+8,-rightRotatedMovedPoint.Z));
						if (distRight>5)
						{
							observer.Position = rightRotatedMovedPoint;				
						}
					}
				}
				*/
			}

		}

		#region IDisposable implementation

		public void Dispose ()
		{
			GLTextureAdmin.UnLoadGLTextures();
		}

		#endregion
	}

	public enum AutoPilotStateEnum
	{
		Stoppped = 0,
		MovingForward = 1,
		TurningRight = 2, 
		Turningleft = 3,
		Reverse = 4

	}
}

