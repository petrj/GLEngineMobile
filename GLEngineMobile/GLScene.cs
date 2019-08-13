using System;
using System.Xml;
using System.Collections.Generic;
using Android.Content;
using OpenTK.Graphics.ES11;

namespace GLEngineMobile
{
    public class GLScene : IDisposable
	{
        private DateTime _FullScreenSetTime;
        private const float DefaultDistance = 6;

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
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.MatrixMode(All.Modelview);
            GL.LoadIdentity();

            var op = Observer.Position;
            var or = Observer.Rotation;

            GL.Rotate((float)or.X, 1, 0, 0);
            GL.Rotate((float)or.Y, 0, 1, 0);
            GL.Rotate((float)or.Z, 0, 0, 1);

            GL.Translate(-(float)op.X, -(float)op.Y, -(float)op.Z);

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
		public void Go(DirectionEnum direction, double stepSize = 2)
		{
			GLPoint movedPoint = new GLPoint(0,0,0);
			switch (direction)
			{
				case DirectionEnum.Forward:  movedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,stepSize,Observer.Rotation.Y,true);break;
				case DirectionEnum.Backward:  movedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,stepSize,Observer.Rotation.Y,false);break;
				case DirectionEnum.Left:  movedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,stepSize,Observer.Rotation.Y-90,true);break;
				case DirectionEnum.Right:  movedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,stepSize,Observer.Rotation.Y+90,true);break;
			}

			var nearestPolygon = NearestPolygon (movedPoint);
            var dist = nearestPolygon.DistanceToPoint(movedPoint);

            //L/ogger.Debug("Distance to labyrinth:" + dist);            

			if (dist> DefaultDistance)
			{
				Observer.Position = movedPoint;
			} else
			{
				// collision 		

				var angle = nearestPolygon.AngleToVec(new GLVector(Observer.Position,movedPoint));

				if ((angle<65) && ((int)direction<2))
				{
					// sliding left

					GLPoint leftRotatedMovedPoint = new GLPoint(0,0,0);
					switch (direction)
					{
						case DirectionEnum.Forward:  leftRotatedMovedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,3,Observer.Rotation.Y-angle,true);break;
						case DirectionEnum.Backward:  leftRotatedMovedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,3,Observer.Rotation.Y-angle,false);break;		        		
					}

                    var nearestPolygonToLeft = NearestPolygon(leftRotatedMovedPoint);
                    var distLeft = nearestPolygonToLeft.DistanceToPoint(leftRotatedMovedPoint);

					if (distLeft> DefaultDistance)
					{
						Observer.Position = leftRotatedMovedPoint;				
					} else
					{
                        // sliding right

                        GLPoint rightRotatedMovedPoint = new GLPoint(0,0,0);
						switch (direction)
						{
							case DirectionEnum.Forward:  rightRotatedMovedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,3,Observer.Rotation.Y+angle,true);break;
							case DirectionEnum.Backward:  rightRotatedMovedPoint = GLPoint.GetMovedPointByAngle(Observer.Position,3,Observer.Rotation.Y+angle,false);break;		        		
						}

                        var nearestPolygonToRight = NearestPolygon(rightRotatedMovedPoint);

                        var distRight = nearestPolygonToRight.DistanceToPoint(rightRotatedMovedPoint);
						if (distRight> DefaultDistance)
						{
                            Observer.Position = rightRotatedMovedPoint;				
						}
					}
				}
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

