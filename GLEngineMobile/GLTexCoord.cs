using System;
using System.Collections.Generic;
using System.Text;

namespace GLEngineMobile
{
    public class GLTexCoord
    {
        public GLTexCoord(double x, double y)
        {
            X = x;
            Y = y;
        }

        public GLTexCoord()
        {
        }

        public double X { get; set; }
        public double Y { get; set; }
    }
}
