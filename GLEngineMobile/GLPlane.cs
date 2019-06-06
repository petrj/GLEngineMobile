using System;

namespace GLEngineMobile
{
	/// <summary>
	/// Point-normal form
	/// </summary>
	public class GLPlane
	{
		public string Name { get; set; }
	
		public GLVector NormalVector { get; set; }
		public GLPoint Position { get; set; }
		public double D { get; set; }
		
		public bool PointInUpperHalfPlane(GLPoint P)
		{
			var n = NormalVector;
			var testInEq = -n.X*P.X-n.Y*P.Y-n.Z*P.Z - D;
			return testInEq>0;			
		}
		
		public GLPoint CrossWithLine(GLLine line)
		{		
			
			var n = NormalVector;
			var P = line.Position;
			
			var denominator = Math.Pow(n.X,2) + Math.Pow(n.Y,2) + Math.Pow(n.Z,2);
			if (denominator == 0)
			{
				return null;
			}
			
			var numerator = (-n.X*P.X-n.Y*P.Y-n.Z*P.Z-D);
			var t = numerator / denominator;
			
			var resPoint = new GLPoint(P.X + t*n.X,P.Y + t*n.Y,P.Z + t*n.Z);
			
			return resPoint;
		}	

		
		public static GLPlane CreateFromPoints(GLPoint A,GLPoint B,GLPoint C)
		{
			var u = new GLVector(A,B);						
			var v = new GLVector(B,C);

			return CreateFromVectorsAndPoint(u,v,A);
		}
		
		
		public static GLPlane CreateFromVectorsAndPoint(GLVector u,GLVector v,GLPoint P)
		{
			var plane = new GLPlane();
			plane.Name = "plane";
			
			plane.Position = P;
			
			var n = GLVector.CrossProduct(u,v);
			
			plane.NormalVector = n;
			
			plane.D = -n.X*P.X-n.Y*P.Y-n.Z*P.Z;
			
			return plane;
		}		
	}
}

