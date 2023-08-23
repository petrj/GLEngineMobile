using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using GL = OpenTK.Graphics.ES11.GL;
using TK = OpenTK.Graphics;
using Android.Content;
using Android.Graphics;
using OpenTK.Graphics.ES11;

namespace GLEngineMobile
{
	public class GLStarSpace  : GLObj
	{
		private int _count = 0;

		public Color FillColor { get; set; }

		public GLVector SizeVec  { get; set; }

		public List<GLPoint> Stars;

		public int Count
		{
			get
			{
				return _count;
			}
			set
			{
				_count = value;


				if (Stars != null)
				{
					Stars.Clear();
				}

				Stars = new List<GLPoint>(value);
				for (var i=0;i<value;i++)
				 {
				 	Stars.Add(new GLPoint());
				 }

				RandomizePositions();
			}
		}

		public void MoveStarsX(double x)
		{
			foreach (var star in Stars)
			{
				star.Move(x,0,0);

				if ((star.X>Math.Abs(SizeVec.X)/2))
						star.X = star.X -Math.Abs(SizeVec.X);

				if ((star.X<-Math.Abs(SizeVec.X)/2))
						star.X = Math.Abs(SizeVec.X) - star.X;
			}
		}

		public GLPoint GetNewRandomStar(Random r = null)
		{
			if (r == null)
			 r = new Random();

			var star = new GLPoint();

			star.X = r.NextDouble() * Math.Abs(SizeVec.X)-Math.Abs(SizeVec.X)/2;
			star.Y = r.NextDouble() * Math.Abs(SizeVec.Y)-Math.Abs(SizeVec.Y)/2;
			star.Z = r.NextDouble() * Math.Abs(SizeVec.Z)-Math.Abs(SizeVec.Z)/2;

			return star;
		}


		private void RandomizePositions()
		{
			if (Stars == null)
				return;

			var r = new Random();

			foreach (var star in Stars)
			{
				var s = GetNewRandomStar(r);
				star.X = s.X;
				star.Y = s.Y;
				star.Z = s.Z;
			}
		}

		public GLStarSpace ()
		{
			Name = "StarSpace";
			FillColor = Color.White;
			Count = 0;
			SizeVec = new GLVector(10,5,10);
			Stars = new List<GLPoint>();
		}

		public override void Render()
		{
			base.BeforeRender();

            GL.Disable(All.Texture2D);
            GL.Color4(FillColor.R, FillColor.G, FillColor.B, FillColor.A);
            GL.Enable(All.Points);

            GL.EnableClientState(All.VertexArray);

            var vArray = new float[Stars.Count * 3];

            for (var i=0; i<Stars.Count;i++)
            {
                vArray[i * 3 + 0] = (float)(Stars[i].X + Position.X);
                vArray[i * 3 + 1] = (float)(Stars[i].Y + Position.Y);
                vArray[i * 3 + 2] = (float)(Stars[i].Z + Position.Z);
            }

            // pin the data, so that GC doesn't move them, while used
            // by native code
            unsafe
            {
                fixed (float* pv = vArray)
                {
                    GL.VertexPointer(3, All.Float, 0, new IntPtr(pv));
                    GL.DrawArrays(All.Points, 0, 4);
                    GL.Finish();
                }
            }

            GL.Enable(All.Texture2D);

            GL.DisableClientState(All.VertexArray);

            base.AfterRender();
		}
	}
}

