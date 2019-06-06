using System;
using System.Xml;

namespace GLEngineMobile
{
	public enum GLAxisEnum
	{
		X,
		Y,
		Z
	}

	public class GLPoint
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		
		public string Name { get; set; }

        public static GLPoint ZeroPoint
		{
			get
			{
				return new GLPoint(0,0,0);	
			}
		}
		
		public static GLPoint CopyFrom(GLPoint point)
		{
			return new GLPoint(point.X,point.Y,point.Z);			
		}
		
		public GLPoint Clone()
		{
			return new GLPoint(X,Y,Z);
		}		
	
		public GLPoint ()
		{
						
		}
		
		public GLPoint (string name)
		{
			Name = name;
		}
		
		public GLPoint (GLPoint p)
		{
			Name = "";
			X = p.X;
			Y = p.Y;
			Z = p.Z;
		}		
		
		public GLPoint PointAdded(GLPoint P)
		{
			return new GLPoint(X+P.X,Y+P.Y,Z+P.Z);
		}
		
		public GLPoint PointSubtracted(GLPoint P)
		{
			return new GLPoint(X-P.X,Y-P.Y,Z-P.Z);
		}		
		
		public double DistanceToPoint(GLPoint P)
		{
			// √( x2 + y2 + z2)   
			var denominator = Math.Pow((P.X-X),2)+Math.Pow((P.Y-Y),2)+Math.Pow((P.Z-Z),2);			
			if (denominator == 0)
				return 0; // zero vector -the same point
			
			return Math.Sqrt(denominator);
		}
		
		public GLPoint (double x,double y,double z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		
        /*
		public void WriteToLog()
		{
		 	var s = String.IsNullOrEmpty(Name) ? "" : Name;
		 	s = s.PadRight(15);
		 	s += "[ " + X.ToString("N2").PadLeft(8);
		 	s += "; " + Y.ToString("N2").PadLeft(8);
		 	s += "; " + Z.ToString("N2").PadLeft(8);
		 	s += " ]";
		 	
			Logger.WriteToLog(s);
		}
        */
		
		public void Move(double x,double y,double z)
		{
			X += x;
			Y += y;
			Z += z;
		}
		
		public void Move(GLVector vec)
		{
			X += vec.X;
			Y += vec.Y;
			Z += vec.Z;
		}		
		
		public void LoadFromXmlElement(XmlElement element)
		{			
			if (element.HasAttribute("x"))			
				X = Convert.ToDouble(element.GetAttribute("x"),System.Globalization.CultureInfo.InvariantCulture);			
				
			if (element.HasAttribute("y"))			
				Y = Convert.ToDouble(element.GetAttribute("y"),System.Globalization.CultureInfo.InvariantCulture);				
				
			if (element.HasAttribute("z"))			
				Z = Convert.ToDouble(element.GetAttribute("z"),System.Globalization.CultureInfo.InvariantCulture);
				
			if (element.HasAttribute("name"))			
				Name = element.GetAttribute("name");
		}
		
		public override string ToString()
		{
			return (Name == null ? "" : Name) + " [" + X.ToString("N2")+" ; " + Y.ToString("N2")+" ; " + Z.ToString("N2") + "]";
		}
		
		public string ToShortString()
		{
			return "[" + X.ToString("#0")+";" + Y.ToString("#0")+";" + Z.ToString("#0") + "]";
		}

		public static double DegToRad(double angle)
		{
			return angle * Math.PI / 180.0;
		}

		public GLPoint RotatedByAxis(GLAxisEnum axis, double radius, double angle)
		{        	
			var rotatedPoint = new GLPoint (this);
			var angleRad = DegToRad (angle);

			switch (axis) 
			{
				case GLAxisEnum.X:

					rotatedPoint.Y = Y * Math.Cos (angleRad) - Z * Math.Sin(angleRad);
					rotatedPoint.Z = Y * Math.Sin (angleRad) + Z * Math.Cos(angleRad);
					break;

				case GLAxisEnum.Y:

					rotatedPoint.X = X * Math.Cos (angleRad) - Z * Math.Sin(angleRad);
					rotatedPoint.Z = X * Math.Sin (angleRad) + Z * Math.Cos(angleRad);
					break;

				case GLAxisEnum.Z:
					rotatedPoint.X = X * Math.Cos (angleRad) - Y * Math.Sin(angleRad);
					rotatedPoint.Y = X * Math.Sin (angleRad) + Y * Math.Cos(angleRad);
					break;

			}

			return rotatedPoint;
		}

		public static GLPoint GetMovedPointByAngle(GLPoint P,double stepSize, double angle, bool forward)
		{        	
			var rotatedPoint = new GLPoint(0,P.Y,0);

			rotatedPoint.X = P.X + stepSize*Math.Cos( (angle-90)*Math.PI/180.0 );
			rotatedPoint.Z = P.Z + stepSize*Math.Sin( (angle-90)*Math.PI/180.0 );

			var viewVec = new GLVector(rotatedPoint,P);

			var movedPoint = new GLPoint(P.X,P.Y,P.Z);

			if (forward)
			{
				movedPoint.X += 1 * viewVec.X;
				movedPoint.Y += 1 * viewVec.Y;
				movedPoint.Z += 1 * viewVec.Z;
			}  else
			{
				movedPoint.X -= 1 * viewVec.X;
				movedPoint.Y -= 1 * viewVec.Y;
				movedPoint.Z -= 1 * viewVec.Z;
			}

			return movedPoint;
		}
		
	}
}

