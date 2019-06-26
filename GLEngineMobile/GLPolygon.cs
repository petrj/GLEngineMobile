using System;
using System.Xml;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using TK=OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using GLEngineMobile;
using Android.Graphics;

namespace GLEngineMobile
{
	public class GLPolygon
	{
		public string Name { get; set; }
		
		public GLTexture Texture { get; set; }
        public Color FillColor { get; set; }
		
		public bool Visible { get; set; }
		
		public List<GLPoint> Points { get; set; }
        public List<GLTexCoord> TexCoord { get; set; }
	
		public GLPolygon ()
		{
			Points = new List<GLPoint>();
            TexCoord = new List<GLTexCoord>();
            FillColor = Color.Argb(255,255,255,255);
			Texture = null;	
			Visible = true;

			SetPolygonTexCoords();
		}

		public GLPolygon (GLPoint A,GLPoint B,GLPoint C,GLPoint D)
			:this()
		{
			Points.Add (A);
			Points.Add (B);
			Points.Add (C);
			Points.Add (D);
		}

        /// <summary>
        /// Default coords
        /// </summary>
        public void SetPolygonTexCoords()
        {
            SetPolygonTexCoords(new List<GLTexCoord>()
            {
                new GLTexCoord() { X = 0, Y = 0 },
                new GLTexCoord() { X = 0, Y = 1 },                
                new GLTexCoord() { X = 1, Y = 1 },
                new GLTexCoord() { X = 1, Y = 0 },                
            });
        }

        public void SetPolygonTexCoords(List<GLTexCoord> coords)
		{
            TexCoord.Clear();

            foreach (var coord in coords)
            {
                TexCoord.Add(coord);
            }			
		}
		
		public GLPolygon Clone()
		{
			var pol = new GLPolygon();
			pol.FillColor = FillColor;
			pol.Texture = Texture;
			pol.Visible = Visible;
			
			pol.Points = new List<GLPoint>();
			
			foreach (var point in Points)
			{
				pol.Points.Add(new GLPoint( point.X,point.Y,point.Z));
			}
			
			return pol;
		}
		
		public double AngleToVec(GLVector vec)
		{
			// using first three points for creating plane vectors: 
			if (Points.Count<3)
			{				
				return 0;
			}
			var plane =  GLPlane.CreateFromPoints(Points[0],Points[1],Points[2]);
			
			var angleToNormal = plane.NormalVector.AngleToVec(vec);
						
			return 90-angleToNormal;
		}
		
		public double DistanceToPoint(GLPoint P)
		{
			// using first three points for creating plane vectors: 
			if (Points.Count<3)
			{				
				return -1;
			}			
			
			var plane =  GLPlane.CreateFromPoints(Points[0],Points[1],Points[2]);
									
			// computing line (made by observer view) - plane cross 
			
			var viewLine = new GLLine();
			viewLine.Position = P;
			viewLine.LineVector = plane.NormalVector;
			
			var cross = plane.CrossWithLine(viewLine);
			
			// geting polygon center to determine right half-plane 
			var center = new GLPoint(
				(Points[0].X+Points[1].X+Points[2].X)/3,
				(Points[0].Y+Points[1].Y+Points[2].Y)/3,
				(Points[0].Z+Points[1].Z+Points[2].Z)/3);
						
			// creating polygon-edge planes:
			
			//Logger.WriteToLog("Generating edge planes and lines");
			
			var edgeLines = new List <GLLine>();
		
			var pointInsideEdgePlanes = true;
			for (var i=0;i<Points.Count;i++)
			{
				var A = Points[i];				
				var B = i+1 <= Points.Count-1 ? Points[i+1] : Points[0];				
				var u = new GLVector(A,B);
				
				var edgePlane = GLPlane.CreateFromVectorsAndPoint(u,plane.NormalVector,A);
				
				// testing center half-plane position
				var centerInUpperHalfPlane = edgePlane.PointInUpperHalfPlane(center);
				var crossInUpperHalfPlane = edgePlane.PointInUpperHalfPlane(cross);

				edgeLines.Add(GLLine.CrateFromPoints(A,B));	
				
				if (centerInUpperHalfPlane != crossInUpperHalfPlane)					
					{
						pointInsideEdgePlanes = false;
						break;						
					}
			}
			
			//Logger.WriteToLog(" cross: " + cross.ToString());			
						
			if (pointInsideEdgePlanes)
			{
				// point inside edge planes
				//Logger.WriteToLog(" point inside edge planes");				
				
				return cross.DistanceToPoint(P);		
			} else
			{
				//Logger.WriteToLog("detecting minimal distance to all edge lines");	
				
				var minDistance = double.MaxValue;
				
				foreach (var line in edgeLines)
				{				
					if (line.LineVector.IsZero)
						continue;

					var intersectionT = line.IntersectionWithPerpendicularThroughPointParamTValue(P);					
					//Logger.WriteToLog(" intersectionT param value: " + intersectionT.ToString());			
					
					if (intersectionT == double.MinValue) 
					{					
						return -1;									
					}
					
					if (intersectionT<0)
					{
						// distance to A
						
						var distanceToA = P.DistanceToPoint(line.Position);
						if (distanceToA<minDistance)
							minDistance = distanceToA;
						
					} else if (intersectionT>1)
					{
						// distance to B	
						var B = line.PointByParam(1);
						var distanceToB = P.DistanceToPoint(B);
						if (distanceToB<minDistance)
							minDistance = distanceToB;
						
					} else
					{
						// distance to perpendicular line intersection 
						
						var intersection = line.PointByParam(intersectionT);
						var distanceToLine = P.DistanceToPoint(intersection);
						if (distanceToLine<minDistance)
							minDistance = distanceToLine;
						
					}
				}		
				
				if (minDistance==double.MaxValue)
					return -1; 
				
				return minDistance;
			}		

		}
		
		public void Magnify(double ratio)
		{
			foreach (var p in Points)
			{
				p.X = p.X * ratio;				
				p.Y = p.Y * ratio;
				p.Z = p.Z * ratio;
			}		
		}
		
		public void Move(double x,double y,double z)
		{
			foreach (var p in Points)
			{
				p.Move(x,y,z);
			}		
		}

        public float[] VertexCoords
        {
            get
            {         
                var verticesList = new List<float>();
                foreach (var p in Points)
                {
                    verticesList.Add((float)p.X);
                    verticesList.Add((float)p.Y);
                    verticesList.Add((float)p.Z);
                }

                return verticesList.ToArray();
            }
        }


        public float[] TextureCoords
        {
            get
            {
                var coordsList = new List<float>();
                foreach (var coord in TexCoord)
                {
                    coordsList.Add((float)coord.X);
                    coordsList.Add((float)coord.Y);
                }

                return coordsList.ToArray();
            }
        }

        public void Render(All depthFunc = All.Lequal)
		{
			if (!Visible)
			return;
            
			GL.DepthFunc(depthFunc);           

            if (Texture != null)
			{
                GL.Enable(All.Texture2D);
                GL.Color4(Color.White.R, Color.White.G, Color.White.G, Color.White.A);  // viz http://stackoverflow.com/questions/5607471/previous-calls-to-gl-color3-are-making-my-texture-use-the-wrong-colors
                GL.BindTexture(All.Texture2D, Texture.TexHandle);
            } else
			{
                // only fill color
                GL.Disable(All.Texture2D);  
                GL.Color4(FillColor.R, FillColor.G, FillColor.B, FillColor.A);               
            }

            GL.EnableClientState(All.VertexArray);
            GL.EnableClientState(All.TextureCoordArray);

            // pin the data, so that GC doesn't move them, while used
            // by native code
            unsafe
            {
                fixed (float* pv = VertexCoords, pt = TextureCoords)
                {
                    GL.VertexPointer(3, All.Float, 0, new IntPtr(pv));
                    GL.TexCoordPointer(2, All.Float, 0, new IntPtr(pt));
                    GL.DrawArrays(All.TriangleFan, 0, 4);
                    GL.Finish();
                }
            }

            GL.Enable(All.Texture2D);

            GL.DisableClientState(All.VertexArray);
            GL.DisableClientState(All.TextureCoordArray);        
        }
		
		public static GLPolygon CreateFromPoints(IEnumerable<GLPoint> points)
		{
			var pol = new GLPolygon();
			
			foreach (var point in points) 
				pol.Points.Add(point);
				
			return pol;
		}	
		
		public void LoadFromXmlElement(XmlElement element)
		{			
			Points = new List<GLPoint>();			
		
			var allPoints = element.SelectNodes("point");
			if (allPoints != null)
			{	
				foreach (XmlElement pointElement in allPoints)
				{		
					var p = new GLPoint();
					p.LoadFromXmlElement(pointElement);
					Points.Add(p);					
				}
			}
			
			if (element.HasAttribute("texture"))
			{
				Name = element.GetAttribute("texture");
				Texture = GLTextureAdmin.GetTextureByName(Name);
			}
		}

        /*
		public void WriteToLog()
		{
			var res = "Polygon " + Name;
			res += Environment.NewLine;

			res += "Texture: " + Texture.Name;
			res += Environment.NewLine;

			foreach (var p in Points) 
			{
				res += p.ToString();
				res += Environment.NewLine;
			}

			Logger.WriteToLog(res);
		}	*/

	}
}
