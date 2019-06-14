using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using OpenTK;
using Android.Content.Res;
using Android.Content;
using Android.Graphics;
using System.Globalization;

namespace GLEngineMobile
{
    public class GLCube : GLObject
    {
        private double _size = 0;

        public double Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                GeneratePolygons();
            }
        }

        private void GeneratePolygons()
        {
            Polygons.Clear();

            Polygons.Add(new GLPolygon(  // top
              new GLPoint(_size / 2, _size / 2, -_size / 2),
              new GLPoint(-_size / 2, _size / 2, -_size / 2),
              new GLPoint(-_size / 2, _size / 2, _size / 2),
              new GLPoint(_size / 2, _size / 2, _size / 2)
              ));
            Polygons.Add(new GLPolygon(  // bottom
                new GLPoint(_size / 2, -_size / 2, _size / 2),
                new GLPoint(-_size / 2, -_size / 2, _size / 2),
                new GLPoint(-_size / 2, -_size / 2, -_size / 2),
                new GLPoint(_size / 2, -_size / 2, -_size / 2)
                ));
            Polygons.Add(new GLPolygon(  // front
                new GLPoint(_size / 2, _size / 2, _size / 2),
                new GLPoint(-_size / 2, _size / 2, _size / 2),
                new GLPoint(-_size / 2, -_size / 2, _size / 2),
                new GLPoint(_size / 2, -_size / 2, _size / 2)
                ));
            Polygons.Add(new GLPolygon(  // back
                new GLPoint(_size / 2, -_size / 2, -_size / 2),
                new GLPoint(-_size / 2, -_size / 2, -_size / 2),
                new GLPoint(-_size / 2, _size / 2, -_size / 2),
                new GLPoint(_size / 2, _size / 2, -_size / 2)
                ));
            Polygons.Add(new GLPolygon(  // left
                new GLPoint(-_size / 2, _size / 2, _size / 2),
                new GLPoint(-_size / 2, _size / 2, -_size / 2),
                new GLPoint(-_size / 2, -_size / 2, -_size / 2),
                new GLPoint(-_size / 2, -_size / 2, _size / 2)
                ));
            Polygons.Add(new GLPolygon(  // right
                new GLPoint(_size / 2, _size / 2, -_size / 2),
                new GLPoint(_size / 2, _size / 2, _size / 2),
                new GLPoint(_size / 2, -_size / 2, _size / 2),
                new GLPoint(_size / 2, -_size / 2, -_size / 2)
                ));
        }

        public override void LoadFromXmlElement(Context context, XmlElement element)
        {
            base.LoadFromXmlElement(context, element);

            if (element.HasAttribute("size"))
                Size = Convert.ToDouble(element.GetAttribute("size"), CultureInfo.InvariantCulture);

            if (element.HasAttribute("texture"))
                SetTexture(element.GetAttribute("texture"));
        }
    }
}
