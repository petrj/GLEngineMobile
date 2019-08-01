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

            //GL.TexEnv(All.TextureEnv, All.TextureEnvMode, (float)All.Modulate);            

            // setup texture parameters
            GL.TexParameterx(All.Texture2D, All.TextureMagFilter, (int)All.Linear);            
            GL.TexParameterx(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
            GL.TexParameterx(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameterx(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);

            Android.Opengl.GLUtils.TexImage2D((int)All.Texture2D, 0, bmp, 0);
        }

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

