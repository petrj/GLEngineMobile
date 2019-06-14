using System;
using System.Xml;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using System.Collections.Generic;
using Android.Content;
using LoggerService;
using Android.Content.Res;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using OpenTK;
using Android.Graphics.Drawables;

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


		public void LoadTextures(Context context)
		{
            foreach (var f in typeof(Drawable).GetFields())
            {
                System.Diagnostics.Debug.WriteLine($"{f.Name} {f.GetValue(null)}");
            }

            GLTextureAdmin.AddTextureFromResource(context, "earth");
            GLTextureAdmin.AddTextureFromResource(context, "moon");
            GLTextureAdmin.AddTextureFromResource(context, "tex");
            GLTextureAdmin.AddTextureFromResource(context, "darkgray");
            GLTextureAdmin.AddTextureFromResource(context, "f_spot");
            GLTextureAdmin.AddTextureFromResource(context, "pattern");
            GLTextureAdmin.AddTextureFromResource(context, "borg");
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
            GL.Clear((int)All.ColorBufferBit | (int)All.DepthBufferBit);
            GL.MatrixMode(All.Modelview);
            GL.LoadIdentity();

            var op = Observer.Position;
            var or = Observer.Rotation;

            GL.Translate((float)op.X, (float)op.Y, (float)op.Z);
            GL.Rotate(-(float)or.X, 1, 0, 0);
            GL.Rotate(-(float)or.Y, 0, 1, 0);

            foreach (var obj in Objects)
			{
				obj.Render();									
			}
		}

        public void Magnify(double ratio)
        {
            foreach (var obj in Objects)
            {
                obj.Magnify(ratio);
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

        public void LoadFromAndroidAsset(Context context, string name)
        {
            var xmlDoc = GLObj.GetAssetXML(context, name);
            LoadFromXmlDocument(context, xmlDoc);
        }

        public void LoadFromXmlDocument(Context context, XmlDocument xmlDoc)
		{	
			var sceneNode = xmlDoc.SelectSingleNode("//scene");
			if(sceneNode != null)
			{
            
				var objs = sceneNode.SelectNodes ("./obj");
				foreach (XmlElement objElement in objs) 
				{
					if ((objElement.HasAttribute ("name")) && (objElement.GetAttribute ("name") == "Observer"))
                    {
						Observer.LoadFromXmlElement(context, objElement);
					} else 
					{                     
						var obj = new GLObject ();
						obj.LoadFromXmlElement(context, objElement);

						Objects.Add (obj);
					}
				}
                
                var planets = sceneNode.SelectNodes("./planet");
                foreach (XmlElement planetNode in planets)
                {
                    var planet = new GLPlanet();
                    planet.LoadFromXmlElement(context, planetNode);

                    Objects.Add(planet);
                }

                var spaceShips = sceneNode.SelectNodes("./spaceShip");
                foreach (XmlElement spaceShipNode in spaceShips)
                {
                    var spaceShip = new GLSpaceShip();
                    spaceShip.LoadFromXmlElement(context, spaceShipNode);

                    Objects.Add(spaceShip);
                }

                var cubeNodes = sceneNode.SelectNodes("./cube");
                foreach (XmlElement cubeNode in cubeNodes)
                {
                    var cube = new GLCube();
                    cube.LoadFromXmlElement(context, cubeNode);

                    Objects.Add(cube);
                }
            }
		}

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

