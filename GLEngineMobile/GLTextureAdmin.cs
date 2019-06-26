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
            Textures = new Dictionary<string, GLTexture>();
        }

		public static GLTexture GetTextureByName(string name)
		{
			if (Textures.ContainsKey(name.ToLower()))
			{
				return Textures[name.ToLower()];
			} else
			return null;
		}

        public static void AddTexturesFromResource(Context context, IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                AddTextureFromResource(context, name);
            }
        }

        public static GLTexture AddTextureFromResource(Context context, string name)
		{            
            if (Textures.ContainsKey(name.ToLower()))
            {
                Logger.Info($"Texture {name} already loaded");
                return Textures[name.ToLower()];
            }

            var tex = GLTexture.CreateFromResource(context, name.ToLower());
            Textures.Add(name.ToLower(), tex);

            return tex;
        }
	}
}

