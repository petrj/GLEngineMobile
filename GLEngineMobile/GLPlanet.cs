using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using OpenTK;
using Android.Content.Res;
using Android.Content;
using Android.Graphics;

namespace GLEngineMobile
{
	public class GLPlanet  : GLSphere
	{

		public bool ShowRing { get;set; }
		public double RingSize { get;set; }
		public string RingTextureName { get;set; }
		public Color RingFillColor { get;set; }

		//public GLSphere Planet  { get;set; }
		public GLObject Ring  { get;set; }

		public List<GLSphere> Satellites { get;set; }

        public GLPlanet()
		{
			ShowRing = false;
			RingSize = 2;
			Name = "planet";

			RingTextureName = null;
			RingFillColor = Color.Gray;

			Texture  = null;

			Satellites = new List<GLSphere>();
		}

		public GLPlanet(string name, string textureName)
		:this()
		{
			Name = name;
			Texture  = GLTextureAdmin.GetTextureByName(textureName);
		}
	
		public void GenerateRingPolygon()
		{
			Ring = new GLObject();

			GLTexture ringTexture = null;

			if (RingTextureName != null)
				ringTexture = GLTextureAdmin.GetTextureByName(RingTextureName);

			var angleStep = 360.0/(double)Slices;

			var c = new GLPoint(0,0,0);

			for (double i=0;i<=360;i += angleStep)
			{
				double ax = (Radius+1)*Math.Cos(i*Math.PI/180.0)+c.X;
				double az = (Radius+1)*Math.Sin(i*Math.PI/180.0)+c.Z;

				double bx = (Radius+1+RingSize)*Math.Cos(i*Math.PI/180.0)+c.X;
				double bz = (Radius+1+RingSize)*Math.Sin(i*Math.PI/180.0)+c.Z;

				double cx = (Radius+1+RingSize)*Math.Cos((i+angleStep)*Math.PI/180.0)+c.X;
				double cz = (Radius+1+RingSize)*Math.Sin((i+angleStep)*Math.PI/180.0)+c.Z;

				double dx = (Radius+1)*Math.Cos((i+angleStep)*Math.PI/180.0)+c.X;
				double dz = (Radius+1)*Math.Sin((i+angleStep)*Math.PI/180.0)+c.Z;

				var polygon = new GLPolygon();
			 	polygon.FillColor = RingFillColor;
			 	polygon.Texture = ringTexture;

				polygon.Points.Add(new GLPoint(ax,c.Y,az));
				polygon.Points.Add(new GLPoint(bx,c.Y,bz));
				polygon.Points.Add(new GLPoint(cx,c.Y,cz));
				polygon.Points.Add(new GLPoint(dx,c.Y,dz));

				Ring.Polygons.Add(polygon);
			}

		}

		public override void Render ()
		{
           
			//var timeAngle = ( (DateTime.Now.Second + DateTime.Now.Millisecond/1000.0) * 360/10.0);
			foreach (var satelitte in Satellites)
			{
				//satelitteAndHisEllipse.Key.OrbitAngle = timeAngle;
				//satelitteAndHisEllipse.Key.Position = satelitteAndHisEllipse.Value.GetPositionOfAngleFocused(timeAngle);
				//timeAngle+=70;

				// move ellipse center to planet
				// center of ellipse is [0;0;0]
				var moveVec = new GLVector(Position,satelitte.Orbit.Center);

				//satelitteAndHisEllipse.Value.Render();
				satelitte.Move(moveVec);
				//satelitteAndHisEllipse.Key.Rotation.Z = -timeAngle;
				satelitte.Render();
			}

           
         
			if (ShowRing)
			{
				if (Ring == null)
				 GenerateRingPolygon();

				 Ring.Position = Position;
				 Ring.Rotation = new GLVector(-30, 0, 20);

				Ring.Render();
			}
        
          
			base.Render();
		}

		public override void LoadFromXmlElement(Context context, XmlElement element)
		{
			base.LoadFromXmlElement(context, element);

			var ringEl = element.SelectSingleNode("./ring") as XmlElement;
			if (ringEl != null)
			{
				ShowRing = ringEl.HasAttribute("shown") && ringEl.Attributes["shown"].Value.ToLower()  == "true" ? true : false;

				if (ringEl.HasAttribute("size"))
					RingSize = Convert.ToDouble(ringEl.Attributes["size"].Value);

				if (ringEl.HasAttribute("texture"))
					RingTextureName = ringEl.Attributes["texture"].Value;

				//if (ringEl.HasAttribute("color"))
					//RingFillColor = System.Drawing.ColorTranslator.FromHtml(ringEl.Attributes["color"].Value);
			}

			var sattelitesEl = element.SelectNodes("./satellite");
			if (sattelitesEl != null)
			{
				foreach (XmlElement moonElement in sattelitesEl)
	            {
	            	var moon = new GLSphere();
	            	moon.LoadFromXmlElement(context, moonElement);
                    Satellites.Add(moon);
                }
			}
		}
	}
}

