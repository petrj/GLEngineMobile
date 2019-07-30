using System;
using System.Xml.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using GL = OpenTK.Graphics.ES11.GL;
using TK = OpenTK.Graphics;
using System.Drawing;
using System.Text;
using Android.Graphics;
using OpenTK.Graphics.ES11;
using Android.Content;

namespace GLEngineMobile
{
	public class GLTexture : IDisposable
	{
        private Bitmap _bmp;

        public int TexHandle { get; private set; }
		public string Name { get;set; }

        public GLTexture()
        {
            CreateTexHandle();
        }

        public static GLTexture CreateFromResource(Context context, string name)
        {
            var tex = new GLTexture();
            tex.LoadFromResource(context, name);
            return tex;
        }

        private void CreateTexHandle()
        {
            int[] textureIds = new int[1];
            GL.GenTextures(2, textureIds);
            TexHandle = textureIds[0];
        }

        public void LoadFromResource(Context context, string name)
        {            
            var resourceId = context.Resources.GetIdentifier(name, "drawable", context.PackageName);

            var bmp = BitmapFactory.DecodeResource(context.Resources, resourceId);

            Name = name;
            LoadFromImage(bmp);
        }

		public void LoadFromImage(Bitmap bmp)
        {
            _bmp = bmp;

            GL.BindTexture(All.Texture2D, TexHandle);
            
            // setup texture parameters
            GL.TexParameterx(All.Texture2D, All.TextureMagFilter, (int)All.Linear);            
            GL.TexParameterx(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
            GL.TexParameterx(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameterx(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);            

            Android.Opengl.GLUtils.TexImage2D((int)All.Texture2D, 0, bmp, 0);
        }

        /*

		public void LoadFromFile(string fileName)
		{
			if (File.Exists(fileName))
			{
				Img = new Bitmap(fileName);
				Name = Path.GetFileNameWithoutExtension(fileName);
			}
		}
        */

    
        /*
		public static GLTexture CreateFromText(string text,Font font, Brush foreground, Brush background, int minWidth = 6)
		{
			SizeF size;
			var testBmp = new Bitmap(1,1);
			using(var g = Graphics.FromImage(testBmp) )
			{
				// var correting text

				var correctedText = new StringBuilder();

				for (var i=0; i<text.Length; i++) 
				{
					if (text.Substring (i, 1) != Environment.NewLine) 
					{
						correctedText.Append ('W');
					} else
						correctedText.AppendLine ();
				}

				size = g.MeasureString(correctedText.ToString(),font);
				if (size.Width<minWidth) size.Width = minWidth;
			}

			var sichr = 5;

			var tex = new GLTexture();

			var bmp = new Bitmap(Convert.ToInt32(size.Width+sichr),Convert.ToInt32(size.Height+sichr));

				using(var g = Graphics.FromImage(bmp) )
				{
					var rectf = new RectangleF(0, 0, bmp.Width, bmp.Height);
					g.FillRectangle(background,rectf);
				    g.DrawString(text,font,foreground,rectf);
				}

				tex.Img = bmp;
				tex.Name = "someText";

			return tex;

		}
        */

		public void Unload()
		{
            int[] textureIds = new int[1] { TexHandle };
            GL.DeleteTextures(1, textureIds);
		}        

        #region IDisposable implementation

        public void Dispose ()
		{
            if (_bmp != null)
                _bmp.Dispose();

            Unload();
        }

		#endregion      
    }

}

