using System;
using System.IO;
using System.Drawing;
using System.Xml;
using System.Collections.Generic;
using GL=OpenTK.Graphics.ES11.GL;
using TK=OpenTK.Graphics;


namespace GLEngineMobile
{
    /// <summary>
    /// GL object. (basic object)
    /// </summary>
    public class GLObj
    {
        public string Name { get; set; }

        public GLPoint Position { get; set; } // vector of rotation in all axis 		
        public GLVector Rotation { get; set; } // vector of rotation in all axis

        public GLObj(string name)
        {
            Name = name;
            Rotation = new GLVector(0, 0, 0);
            Position = new GLPoint(0, 0, 0);
        }

        public GLObj()
        {
            Rotation = new GLVector(0, 0, 0);
            Position = new GLPoint(0, 0, 0);
        }

        public string DisplayName
        {
            get
            {
                if (String.IsNullOrEmpty(Name))
                    return String.Empty;

                var res = "";
                if (Name.Length > 0) res += Name.Substring(0, 1).ToUpper();
                if (Name.Length > 1) res += Name.Substring(1).ToLower();

                return res;
            }
        }

        public static double Distance(GLObj A, GLObj B)
        {
            if (A != null && B != null)
            {
                var vec = new GLVector(A.Position, B.Position);
                return vec.Length;
            }
            else
                return 0;
        }

        public virtual void LoadFromXmlElement(XmlElement element)
        {
            if (element.HasAttribute("name"))
                Name = element.Attributes["name"].Value;


            var pointEl = element.SelectSingleNode("./point[@name='Position']") as XmlElement;
            if (pointEl != null)
            {
                Position.LoadFromXmlElement(pointEl);
            }

            var rotEl = element.SelectSingleNode("./vector[@name='Rotation']") as XmlElement;
            if (rotEl != null)
            {
                Rotation.LoadFromXmlElement(rotEl);
            }
        }

        public virtual double DistanceToPoint(GLPoint point)
        {
            var dist = Position.DistanceToPoint(point);

            return dist;
        }

        public virtual void Magnify(double ratio)
        {
        }

        public virtual void Move(double x, double y, double z)
        {
            Position.X += x;
            Position.Y += y;
            Position.Z += z;
        }

        public virtual void Move(GLVector vec)
        {
            Move((float)vec.X, (float)vec.Y, (float)vec.Z);
        }

        public virtual void SetTexture(string name)
        {
        }

        /// <summary>
        /// Actions before render 
        /// - push position
        /// - move to Position
        /// - rotatation 
        /// </summary>
        public virtual void BeforeRender()
        {
            GL.PushMatrix();

            GL.Translate((float)Position.X, (float)Position.Y, (float)Position.Z); // object position 

            var c = Center;

            if (!Rotation.IsZero)
            {
                GL.Translate((float)-c.X, (float)-c.Y, (float)-c.Z);
                GL.Rotate((float)Rotation.X, 1, 0, 0);
                GL.Rotate((float)Rotation.Y, 0, 1, 0);
                GL.Rotate((float)Rotation.Z, 0, 0, 1);
            }
        }

        /// <summary>
        /// Actions after render 
        /// - finish rotatation 
        /// - pop position

        /// </summary>
        public virtual void AfterRender()
        {
            if (!Rotation.IsZero)
            {
                var c = Center;
                GL.Translate((float)+c.X, (float)+c.Y, (float)+c.Z);
            }

            GL.PopMatrix();
        }

        public virtual void Render()
        {
            BeforeRender();

            // nothing to render

            AfterRender();
        }

        public virtual GLPoint UpperLeft
        {
            get
            {
                return Position;
            }
        }

        public virtual GLPoint BottomRight
        {
            get
            {
                return Position;
            }
        }

        public virtual GLPoint Center
        {
            get
            {
                return Position;
            }
        }

        public virtual double Length
        {
            get
            {
                return 0;
            }
        }

        public virtual double XSize
        {
            get
            {
                return 0;
            }
        }

        public virtual double YSize
        {
            get
            {
                return 0;
            }
        }

        public virtual double ZSize
        {
            get
            {
                return 0;
            }
        }
    }

}

