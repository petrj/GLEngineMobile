using System;
using System.Xml;

namespace GLEngineMobile
{
	public class GLVector
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		
		string Name  { get; set; }
		
		public GLVector ()
		{
			
		}	
		
		public double AngleToVec(GLVector v)
		{
			var vec1den = Math.Pow(X,2)+Math.Pow(Y,2)+Math.Pow(Z,2);
			var vec2den = Math.Pow(v.X,2)+Math.Pow(v.Y,2)+Math.Pow(v.Z,2);
			
			vec1den = Math.Sqrt(vec1den);
			vec2den = Math.Sqrt(vec2den);
			
			var denominator = vec1den*vec2den;
			
			if (denominator == 0)
			return 0;
			
			var numerator = Math.Abs(X*v.X+Y*v.Y+Z*v.Z);
			
			var cosfee = numerator/denominator;
			
			return Math.Acos(cosfee)*(180/Math.PI);
		}
		
		public GLVector (double x,double y,double z)
		{
			X = x;
			Y = y;
			Z = z;
		}		
		
		public GLVector Clone()
		{
			return new GLVector(X,Y,Z);
		}
		
		public GLVector (GLPoint A,GLPoint B)
		{
			X = A.X-B.X;
			Y = A.Y-B.Y;
			Z = A.Z-B.Z;
		}	
		
		public GLVector (GLVector vec)
		{
			X = vec.X;
			Y = vec.Y;
			Z = vec.Z;
		}			
		
		public GLVector UnitVector
		{
			get
			{
				var l = Length;
				if (l == 0)
				return null;
				
				return new GLVector(X/l,Y/l,Z/l);
			}
		}		
		
		
		public bool IsZero
		{
			get
			{
				return (X == 0) && (Y==0) && (Z==0);
			}
		}
		
		public double Length
		{
			get
			{
				if (IsZero) 
				return 0;
				
				return Math.Sqrt(Math.Pow(X,2)+Math.Pow(Y,2)+Math.Pow(Z,2));
			}			
		}
		
		public override string ToString ()
		{
			return "[" + X.ToString("N2")+" ; " + Y.ToString("N2")+" ; " + Z.ToString("N2") + "]";
		}		
		
		public static GLVector CrossProduct(GLVector u,GLVector v)
		{
			return new GLVector(u.Y*v.Z-u.Z*v.Y,u.Z*v.X-u.X*v.Z,u.X*v.Y-u.Y*v.X);
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
	}
}

