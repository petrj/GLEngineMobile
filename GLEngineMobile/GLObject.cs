using System;
using System.IO;
using System.Drawing;
using System.Xml;
using System.Collections.Generic;
using OpenTK;
using Android.Content.Res;
using Android.Content;

namespace GLEngineMobile
{
	public class GLObject : GLObj
	{		
		public List<GLPolygon> Polygons { get; set; } = new List<GLPolygon>();

        public static GLObject CreateFromPolygon(GLPolygon pol, string name)
        {
            var res = new GLObject()
            {
                Name = name
            };
            res.Polygons.Add(pol);

            return res;
        }
		
		public override void SetTexture(string name)
		{
			var tex = GLTextureAdmin.GetTextureByName(name);
			foreach (var pol in Polygons)
			{
				pol.Texture = tex;
			}	
		}
		
		public void ChangeTexture(string name, string newName)
		{
			var tex = GLTextureAdmin.GetTextureByName(newName);
			foreach (var pol in Polygons)
			{
				if (pol.Texture.Name == name)
					pol.Texture = tex;
			}	
		}		
		
		public override void Magnify(double ratio)
		{
			foreach (var pol in Polygons)
			{
				pol.Magnify(ratio);
			}		
		}		
		
		public override void Move(double x,double y,double z)		
		{
			foreach (var pol in Polygons)
			{
				pol.Move(x,y,z);
			}		
		}

		public void MoveToCenter()
		{
			// moving to center
			var c = Center;
			Move(-c.X,-c.Y,-c.Z);
		}
		
		public override void Render()
		{		
			base.BeforeRender();
		
			foreach (var polygon in Polygons)
			{			
				polygon.Render();
			}
			
			base.AfterRender();
		}
		
		public GLPolygon FirstPolygon
		{
			get
			{
				if ((Polygons != null) && (Polygons.Count>0))
					return Polygons[0];					
				
				return null;
			}
		}		
		
		public override GLPoint UpperLeft
		{
			get
			{
				var point = new GLPoint("UpperLeft");
				foreach (var pol in Polygons)
				{
					foreach (var p in pol.Points)
					{
						if (point.X>p.X) point.X = p.X;					
						if (point.Y>p.Y) point.Y = p.Y;
						if (point.Z>p.Z) point.Z = p.Z;
					}
				}
				
				return point;
			}
		}

		public override double DistanceToPoint (GLPoint point)
		{
			if (Polygons == null || Polygons.Count == 0)
				return base.DistanceToPoint(point);

			var pol = NearestPolygon (point);
			var dist = pol.DistanceToPoint (point);

			return dist;
		}

		public GLPolygon NearestPolygon(GLPoint P)
		{
			//Logger.WriteToLog("Detecting nearest polygon to point: " + P.ToString());			
			
			GLPolygon nearestPolygon = null;
			
			var minDistance = double.MaxValue;
			var i = 0;
			foreach (var pol in Polygons)
			{
				var d = pol.DistanceToPoint(P);
				
				if (d<minDistance)
				{
					minDistance = d;
					nearestPolygon = pol;
				}
				
				//Logger.WriteToLog(String.Format("Distance to polygon {0}: {1} ",i,d.ToString("#0.00")));							
				
				i++;
			}			
			
			return nearestPolygon;
		}
		/*
		public double DistanceToPoint(GLPoint P)
		{		
			var pointMoved = P.PointSubtracted(Position);
			
			var nearestPolygon = NearestPolygon(pointMoved);
			return nearestPolygon == null ? double.MaxValue : nearestPolygon.DistanceToPoint(pointMoved);
		}*/
		
		public override GLPoint BottomRight
		{
			get
			{
				var point = new GLPoint("BottomRight");
				foreach (var pol in Polygons)
				{
					foreach (var p in pol.Points)
					{
						if (point.X<p.X) point.X = p.X;					
						if (point.Y<p.Y) point.Y = p.Y;
						if (point.Z<p.Z) point.Z = p.Z;
					}
				}
				
				return point;
			}
		}
		
		public override GLPoint Center
		{
			get
			{
				var a = UpperLeft;
				var b = BottomRight;
				
				var c = new GLPoint("Center");
				
				c.X = a.X + (b.X-a.X)/2.0;
				c.Y = a.Y +  (b.Y-a.Y)/2.0;
				c.Z = a.Z + (b.Z-a.Z)/2.0;
				
				return c;				
			}
		}
		
		public override double Length
		{
			get
			{
				var a = UpperLeft;
				var b = BottomRight;
				var z = Math.Pow(a.X-b.X,2.0) + Math.Pow(a.Y-b.Y,2.0) + Math.Pow(a.Z-b.Z,2.0);
				if (z == 0)
				return 0;
				
				return Math.Sqrt(z);
			}
		}
		
		public override double XSize
		{
			get
			{
				var a = UpperLeft;
				var b = BottomRight;				
				return Math.Abs(b.X-a.X);
			}
		}
		
		public override double YSize
		{
			get
			{
				var a = UpperLeft;
				var b = BottomRight;				
				return Math.Abs(b.Y-a.Y);
			}
		}		
		
		public override double ZSize
		{
			get
			{
				var a = UpperLeft;
				var b = BottomRight;				
				return Math.Abs(b.Z-a.Z);
			}
		}		

		public static GLObject CreateFromPolygons(IEnumerable<GLPolygon> polygons)
		{
			var obj = new GLObject();
			
			foreach (var pol in polygons) 
				obj.Polygons.Add(pol);
				
			return obj;
		}		
		
		public void LoadFromjObject(GLObject obj)
		{
			Position = obj.Position.Clone();
			Rotation = Rotation.Clone();
			Polygons = new List<GLPolygon>();
			foreach (var polygon in obj.Polygons)
			{
				Polygons.Add(polygon.Clone());
			}
		}

        public override void LoadFromXmlElement(Context context, XmlElement element)
        {
            base.LoadFromXmlElement(context, element);

            var polygonNodes = element.SelectNodes("./polygon");
            foreach (XmlElement polygonElement in polygonNodes)
            {
                var pol = new GLPolygon();
                pol.LoadFromXmlElement(polygonElement);
                Polygons.Add(pol);
            }
        }
    }
}

