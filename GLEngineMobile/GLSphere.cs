using System;
using System.Xml;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using TK = OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using Android.Graphics;
using Android.Content;

namespace GLEngineMobile
{
	public class GLSphere : GLObj
	{
		public GLVector RotationSpeed { get;set; }
		public double Radius { get; set; }
		public int Slices { get; set; }
		public int Stacks { get; set; }
		public GLTexture Texture { get; set; }

		public GLEllipse Orbit { get; set; }
		public double OrbitSpeed  { get; set; }
		public double _orbitAngle = 0;

		private List<GLPolygon> Polygons { get; set; }

        public double OrbitAngle
		{
			get
			{
				return _orbitAngle;
			}
			set
			{
				_orbitAngle = value;
				Position = (Orbit as GLEllipse).GetPositionOfAngle(value);
			}
		}

		public GLSphere()
		{
			RotationSpeed = new GLVector(0,0,0.5);
			Radius = 2;
			Slices = 20;
			Stacks = 20;
			Texture = null;
			Orbit = new GLEllipse();
			OrbitAngle = 270;
			OrbitSpeed = 0.5;

			Polygons = new List<GLPolygon> ();
		}

		public void Rotate(GLVector vec)
		{
			Rotation.X += vec.X;
			Rotation.X += vec.X % 360;
			Rotation.Y += vec.Y;
			Rotation.Y += vec.Y % 360;
			Rotation.Z += vec.Z;
			Rotation.Z += vec.Z % 360;
		}

		public GLSphere (string name)
			:this()
		{
			Name = name;
		}

		public override void SetTexture(string name)
		{
			Texture = GLTextureAdmin.GetTextureByName(name);
		}

		public override void BeforeRender ()
		{
			GL.PushMatrix();

			GL.Translate((float)Position.X, (float)Position.Y, (float)Position.Z); 

			if (!Rotation.IsZero)
			{
				GL.Rotate((float)Rotation.Y,0,1,0);
				GL.Rotate((float)Rotation.X,1,0,0);
				GL.Rotate((float)Rotation.Z,0,0,1);
			}
		}

		// https://www.codeproject.com/Articles/18160/Spherical-Coordinates-in-C
		private GLPoint Spherical(double r, double theta, double phi)
		{
			var  pt = new GLPoint();
			double snt = (double)Math.Sin(theta * Math.PI / 180.0);
			double cnt = (double)Math.Cos(theta * Math.PI / 180.0);
			double snp = (double)Math.Sin(phi * Math.PI / 180.0);
			double cnp = (double)Math.Cos(phi * Math.PI / 180.0);
			pt.X = r * snt * cnp;
			pt.Y = r * cnt;
			pt.Z = -r * snt * snp;
			//pt.W = 1;
			return pt;
		}

		public void CreatePolygons()
		{
			Polygons.Clear ();

			for (var j=0;j<=Slices;j++)
			{
				var angleJ = j * 360.0 / (double)Slices;
				var angleJ2 = (j+1) * 360.0 / (double)Slices;

				for (var i=0;i<=Stacks;i++)
				{
					var angleI = i * 180.0 / (double)Stacks;
					var angleI2 = (i+1) * 180.0 / (double)Stacks;

					var A = Spherical (Radius, angleI, angleJ);
					var B = Spherical (Radius, angleI, angleJ2);
					var C = Spherical (Radius, angleI2, angleJ2);
					var D = Spherical (Radius, angleI2, angleJ);

					var polygon = new GLPolygon (A, B, C, D);
					polygon.Texture = Texture;

					var textCoordLeft = (j) * (1.0 / Slices);
					var textCoordRight = (j+1) * (1.0 / Slices);

					var textCoordTop = (i) * (1.0 / Stacks);
					var textCoordBottom = (i+1) * (1.0 / Stacks);

                    var coords = new List<GLTexCoord>();

                    coords.Add(new GLTexCoord(textCoordLeft, textCoordTop));
                    coords.Add(new GLTexCoord(textCoordRight, textCoordTop));
                    coords.Add(new GLTexCoord(textCoordRight, textCoordBottom));
                    coords.Add(new GLTexCoord(textCoordLeft, textCoordBottom));
                    

                    polygon.SetPolygonTexCoords(coords);

                    Polygons.Add (polygon);
				}
			}
		}

		public override void Render ()
		{
			BeforeRender();
	
			if (Polygons.Count == 0)
				CreatePolygons ();

            foreach (var pol in Polygons)
            {
                pol.Render();
            } 

			AfterRender ();
		}

		public override void AfterRender ()
		{
			GL.PopMatrix();
		}

		public override void Magnify(double ratio)
		{
			Radius = Radius*ratio;
			foreach (var pol in Polygons)
				pol.Magnify (ratio);
		}

		public override void LoadFromXmlElement(Context context, XmlElement element)
		{
			base.LoadFromXmlElement(context, element);

			if (element.HasAttribute("radius"))
				Radius = Convert.ToDouble(element.GetAttribute("radius"));

			if (element.HasAttribute("slices"))
				Slices = Convert.ToInt32(element.GetAttribute("slices"));

			if (element.HasAttribute("stacks"))
				Stacks = Convert.ToInt32(element.GetAttribute("stacks"));

			if (element.HasAttribute("texture"))
				SetTexture(element.GetAttribute("texture"));

			if (element.HasAttribute("orbitAngle"))
				OrbitAngle = Convert.ToDouble(element.GetAttribute("orbitAngle"));

			var rotSpeedEl = element.SelectSingleNode("./vector[@name='RotationSpeed']") as XmlElement;
			if (rotSpeedEl != null)
			{
				RotationSpeed.LoadFromXmlElement(rotSpeedEl);
			}

			var orbitEllipseElement = element.SelectSingleNode("./ellipse[@name='Orbit']") as XmlElement;
			if (orbitEllipseElement != null)
			{
				Orbit.LoadFromXmlElement(context, orbitEllipseElement);
			}		
		}
	}

}
