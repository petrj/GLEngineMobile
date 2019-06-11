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
    public class GLEllipse : GLObj
    {
        public Color FillColor { get; set; }

        public double RadiusMajor { get; set; }
        public double RadiusMinor { get; set; }

        // unit vectors of a plane
        public GLVector U;
        public GLVector V;

        public GLEllipse()
        {
            FillColor = Color.White;

            RadiusMajor = 10;
            RadiusMinor = 5;

            U = new GLVector(1, 0, 0);
            V = new GLVector(0, 0, 1);
        }

        public GLEllipse(string name)
            : this()
        {
            Name = name;
        }

        public GLEllipse(string name, double radiusMajor, double radiusMinor, GLVector vecX, GLVector vecY)
            : this(name)
        {
            RadiusMajor = radiusMajor;
            RadiusMinor = radiusMinor;

            U = new GLVector(vecX);
            V = new GLVector(vecY);
        }

        public GLPoint GetPositionOfAngle(double angle)
        {
            var a = (angle) * Math.PI / 180.0;

            var x = RadiusMajor * Math.Cos(a) * U.X + RadiusMinor * Math.Sin(a) * V.X;
            var y = RadiusMajor * Math.Cos(a) * U.Y + RadiusMinor * Math.Sin(a) * V.Y;
            var z = RadiusMajor * Math.Cos(a) * U.Z + RadiusMinor * Math.Sin(a) * V.Z;

            return new GLPoint(x, y, z);
        }

        public GLPoint GetPositionOfAngleFocused(double angle)
        {
            var a = (angle) * Math.PI / 180.0;
            var excentricity = Math.Sqrt(Math.Pow(RadiusMajor, 2) - Math.Pow(RadiusMinor, 2));
            var point270 = GetPositionOfAngle(270);

            if (RadiusMajor != 0)
            {
                var vec = new GLVector(Center, point270);
                var Fx = new GLVector(excentricity / RadiusMajor * vec.X,
                                    excentricity / RadiusMajor * vec.Y,
                                    excentricity / RadiusMajor * vec.Z);

            }

            var x = Center.X + RadiusMajor * Math.Cos(a) * U.X + RadiusMinor * Math.Sin(a) * V.X;
            var y = Center.Y + RadiusMajor * Math.Cos(a) * U.Y + RadiusMinor * Math.Sin(a) * V.Y;
            var z = Center.Z + RadiusMajor * Math.Cos(a) * U.Z + RadiusMinor * Math.Sin(a) * V.Z;

            return new GLPoint(x, y, z);
        }

		public GLPoint GetPositionOf2DAngle(double angle)
        {
            var a = (angle) * Math.PI / 180.0;

			var x = Position.X + RadiusMajor * Math.Cos (a);
			var z = Position.Z + RadiusMajor * Math.Sin (a);
			return new GLPoint (x, Position.Y, z);
        }

        public override void Render()
        {
            base.BeforeRender();
            GL.Disable(All.Texture2D);
            GL.Color4(FillColor.R, FillColor.G, FillColor.B, FillColor.A);

            GL.EnableClientState(All.VertexArray);

            GL.Color4(FillColor.R, FillColor.G, FillColor.B, FillColor.A);            

            GLPoint lastPoint = new GLPoint();

            var vertexCoords = new float[216]; // 36 lines, 2 points for every line, 3 floats for every point

            for (var i = 0; i <= 36; i++)
            {
                var p = GetPositionOfAngleFocused(i * 10);

                if (i != 0)
                {
                    var vIndex = (i - 1)*6;

                    vertexCoords[vIndex+0] = (float)p.X;
                    vertexCoords[vIndex+1] = (float)p.Y;
                    vertexCoords[vIndex+2] = (float)p.Z;

                    vertexCoords[vIndex+3] = (float)lastPoint.X;
                    vertexCoords[vIndex+4] = (float)lastPoint.Y;
                    vertexCoords[vIndex+5] = (float)lastPoint.Z;
                }

                lastPoint = p;
            }

            unsafe
            {
                fixed (float* pv = vertexCoords)
                {
                    GL.VertexPointer(3, All.Float, 0, new IntPtr(pv));
                    GL.DrawArrays(All.Lines, 0, 72);
                    GL.Finish();
                }
            }

            GL.Enable(All.Texture2D);

            GL.DisableClientState(All.VertexArray);
         
            base.AfterRender();
        }

        public override void LoadFromXmlElement(Context context, XmlElement element)
        {
            base.LoadFromXmlElement(context, element);

            if (element.HasAttribute("radiusMajor"))
                RadiusMajor = Convert.ToDouble(element.GetAttribute("radiusMajor"));

            if (element.HasAttribute("radiusMinor"))
                RadiusMinor = Convert.ToDouble(element.GetAttribute("radiusMinor"));

            var vecUEl = element.SelectSingleNode("./vector[@name='U']") as XmlElement;
            if (vecUEl != null)
            {
                U.LoadFromXmlElement(vecUEl);
            }

            var vecVEl = element.SelectSingleNode("./vector[@name='V']") as XmlElement;
            if (vecVEl != null)
            {
                V.LoadFromXmlElement(vecVEl);
            }
        }
    }
}