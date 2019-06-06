using System;

namespace GLEngineMobile
{
	public class GLLine
	{
		public GLVector LineVector { get; set; }
		public GLPoint Position { get; set; }
		
		public static GLLine CrateFromPoints(GLPoint A,GLPoint B)
		{
			var line = new GLLine();
			line.Position = A;
			line.LineVector = new GLVector(A,B);
			
			return line;
		}
		
		public GLPoint PointByParam(double t)
		{
			if (t==double.MinValue)
				return null;
			
			return new GLPoint(	Position.X+t*LineVector.X,
								Position.Y+t*LineVector.Y,
								Position.Z+t*LineVector.Z);
		}
		
		
		/// A			t in <0;1>			B
		///  --------------------------------
		/// 				^ I 
		/// 				|
		/// 				| s in <0;1>
		/// 				|
		/// 				|
		/// 				|P
		///
		/// Line: Point A,B; Vec U = B - A
		/// 	X = Ax + t*Ux
		/// 	Y = Ay + t*Uy
		/// 	Z = Az + t*Uz
		///
		/// Perpendicular: Point P,I; Vec V = I - P
		/// 	X = Px + s*Vx
		/// 	Y = Py + s*Vy
		/// 	Z = Pz + s*Vz
		///
		///	U and V are perpendicular (U.V = 0)
		///	
		///		UxVx+UyVy+UzVz = 0
		/// 	-------------------
		///
		/// Param s = 1 at intersection I
		///
		public GLPoint IntersectionWithPerpendicularThroughPoint(GLPoint P)
		{
			var t= IntersectionWithPerpendicularThroughPointParamTValue(P);		
			
			return this.PointByParam(t);
		}
		

		public double IntersectionWithPerpendicularThroughPointParamTValue(GLPoint P)
		{
			var U = LineVector;
			var A = Position;
			
			var denominator = U.X*U.X+U.Y*U.Y+U.Z*U.Z;
			
			if (denominator == 0)
				return double.MinValue;
				
				var t = (U.X*(P.X-A.X)+U.Y*(P.Y-A.Y)+U.Z*(P.Z-A.Z))/denominator;
				
				return t;
		}		
	}
}

