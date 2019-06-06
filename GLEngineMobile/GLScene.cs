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
            GLTextureAdmin.AddTextureFromResource(context, "earth"); 
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

        public void Magnify(double ratio)
        {
            foreach (var obj in Objects)
            {
                obj.Magnify(ratio);
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

