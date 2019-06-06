using System;
using System.Xml.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Android.Content;
using LoggerService;

namespace GLEngineMobile
{
	public static class GLTextureAdmin
	{
		public static Dictionary<string,GLTexture>Textures { get; set; }		

		static GLTextureAdmin()
		{
			Textures = new Dictionary<string, GLTexture>();
		}

		public static void UnLoadGLTextures()
		{
			foreach (KeyValuePair<string,GLTexture> kvp in Textures)
            {
            	kvp.Value.Unload();
           	}
		}

		public static GLTexture GetTextureByName(string name)
		{
			if (Textures.ContainsKey(name))
			{
				return Textures[name];
			} else
			return null;
		}

		public static GLTexture AddTextureFromResource(Context context, string name)
		{
            if (Textures.ContainsKey(name))
            {
                Logger.Info($"Texture {name} already loaded");
                return Textures[name];
            }

            var tex = GLTexture.CreateFromResource(context, name);
            Textures.Add(name, tex);

            return tex;
        }
	}
}

