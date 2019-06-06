using System;
using System.Xml;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using TK = OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using GLEngineMobile;
using Android.Graphics;


namespace GLEngineMobile
{
	public class GLAxis : GLObj
	{
		public Color FillColor { get; set; }
	
		public GLAxis ()
		{
			Name = "axis";
			FillColor = Color.White;
		}
		
		public override void Render ()
		{
			base.BeforeRender();

            GL.Disable(All.Texture2D);
            GL.Color4(FillColor.R, FillColor.G, FillColor.B, FillColor.A);

            GL.EnableClientState(All.VertexArray);

            GL.Color4(FillColor.R,FillColor.G,FillColor.B, FillColor.A);
 		
            var vertexCoords = new float[18] 
            {
                -300, 0, 0,
                300, 0, 0,
                0, -300, 0,
                0, 300, 0,
                0,0,-300,
                0,0,300
            };

            unsafe
            {
                fixed (float* pv = vertexCoords)
                {
                    GL.VertexPointer(3, All.Float, 0, new IntPtr(pv));                    
                    GL.DrawArrays(All.Lines, 0, 6);                    
                    GL.Finish();
                }
            }

            GL.Enable(All.Texture2D);

            GL.DisableClientState(All.VertexArray);            

            base.AfterRender();
		}		
	}
}

