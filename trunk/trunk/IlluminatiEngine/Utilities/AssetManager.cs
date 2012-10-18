using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace IlluminatiEngine
{
    public enum BaseAssets
    {
        BlankTexture,
    }

    public class AssetManager : GameComponent, IAssetManager
    {   
        ContentManager cm;
        ContentManager gameCM;

        Dictionary<string, Texture2D> Texture2D = new Dictionary<string, Texture2D>();
        Dictionary<string, Texture3D> Texture3D = new Dictionary<string, Texture3D>();
        Dictionary<string, TextureCube> TextureCubes = new Dictionary<string, TextureCube>();
        Dictionary<string, Effect> Effects = new Dictionary<string, Effect>();
        Dictionary<string, Song> Songs = new Dictionary<string, Song>();
        Dictionary<string, SoundEffect> SoundEffects = new Dictionary<string, SoundEffect>();
        Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>();
        Dictionary<string, Model> Models = new Dictionary<string, Model>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="game">Calling Game</param>
        public AssetManager(Game game) : this(game, false) { }
        public AssetManager(Game game, bool noneService)
            : base(game)
        {
            if (!noneService)
            {
                game.Components.Add(this);
                game.Services.AddService(typeof(IAssetManager), this);
            }

            gameCM = new ContentManager(Game.Services, "Content");
            cm = new ContentManager(game.Services, "IlluminatiEngineContent");
        }

        /// <summary>
        /// Adds an asset to the manager
        /// </summary>
        /// <typeparam name="T">Asset type</typeparam>
        /// <param name="key">Key for assset (normaly assets content path)</param>
        /// <param name="asset">The asset to add</param>
        public void AddAsset<T>(string key, object asset) where T : class
        {
            if (typeof(T) == typeof(Texture2D) && !Texture2D.ContainsKey(key))
                Texture2D.Add(key, (Texture2D)asset);

            if (typeof(T) == typeof(Texture3D) && !Texture3D.ContainsKey(key))
                Texture3D.Add(key, (Texture3D)asset);

            if (typeof(T) == typeof(Effect) && !Effects.ContainsKey(key))
                Effects.Add(key, (Effect)asset);

            if (typeof(T) == typeof(Song) && !Songs.ContainsKey(key))
                Songs.Add(key, (Song)asset);

            if (typeof(T) == typeof(SoundEffect) && !SoundEffects.ContainsKey(key))
                SoundEffects.Add(key, (SoundEffect)asset);

            if (typeof(T) == typeof(SpriteFont) && !Fonts.ContainsKey(key))
                Fonts.Add(key, (SpriteFont)asset);

            if (typeof(T) == typeof(Model) && !Models.ContainsKey(key))
                Models.Add(key, (Model)asset);

            if (typeof(T) == typeof(TextureCube) && !TextureCubes.ContainsKey(key))
                TextureCubes.Add(key, (TextureCube)asset);
        }

        /// <summary>
        /// Gets an asset from the manager
        /// </summary>
        /// <typeparam name="T">Type of asset to get</typeparam>
        /// <param name="key">Asset to get</param>
        /// <returns>The required object. If key is not in the manager, it is loaded and the object passet back </returns>
        public T GetAsset<T>(string key) where T : class
        {
            object returnObj = null;

            if (typeof(T) == typeof(Texture2D) && Texture2D.ContainsKey(key))
                returnObj = Texture2D[key];

            if (typeof(T) == typeof(Texture3D) && Texture3D.ContainsKey(key))
                returnObj = Texture3D[key];

            if (typeof(T) == typeof(Effect) && Effects.ContainsKey(key))
                returnObj = Effects[key];

            if (returnObj == null && typeof(T) == typeof(Song) && Songs.ContainsKey(key))
                returnObj = Songs[key];

            if (returnObj == null && typeof(T) == typeof(SoundEffect) && SoundEffects.ContainsKey(key))
                returnObj = SoundEffects[key];

            if (returnObj == null && typeof(T) == typeof(SpriteFont) && Fonts.ContainsKey(key))
                returnObj = Fonts[key];

            if (returnObj == null && typeof(T) == typeof(Model) && Models.ContainsKey(key))
                returnObj = Models[key];

            if (returnObj == null && typeof(T) == typeof(TextureCube) && TextureCubes.ContainsKey(key))
                returnObj = TextureCubes[key];

            if (returnObj == null)
            {                
                try
                {
                    AddAsset<T>(key, gameCM.Load<T>(key));
                }
                catch(Exception e)
                {
                    // Try the native content..
                    AddAsset<T>(key, cm.Load<T>(key));                        
                }
                returnObj = GetAsset<T>(key);  
            }

            return (T)returnObj;
        }
        public T GetAsset<T>(BaseAssets key) where T : class
        {
            object returnObj = null;

            if (typeof(T) == typeof(Texture2D) && Texture2D.ContainsKey(key.ToString()))
                returnObj = Texture2D[key.ToString()];

            if (typeof(T) == typeof(Texture3D) && Texture3D.ContainsKey(key.ToString()))
                returnObj = Texture3D[key.ToString()];

            if (typeof(T) == typeof(Effect) && Effects.ContainsKey(key.ToString()))
                returnObj = Effects[key.ToString()];

            if (returnObj == null && typeof(T) == typeof(Song) && Songs.ContainsKey(key.ToString()))
                returnObj = Songs[key.ToString()];

            if (returnObj == null && typeof(T) == typeof(SoundEffect) && SoundEffects.ContainsKey(key.ToString()))
                returnObj = SoundEffects[key.ToString()];

            if (returnObj == null && typeof(T) == typeof(SpriteFont) && Fonts.ContainsKey(key.ToString()))
                returnObj = Fonts[key.ToString()];

            if (returnObj == null && typeof(T) == typeof(Model) && Models.ContainsKey(key.ToString()))
                returnObj = Models[key.ToString()];

            if(returnObj == null && typeof(T) == typeof(TextureCube) && TextureCubes.ContainsKey(key.ToString()))
                returnObj = TextureCubes[key.ToString()];

            if (returnObj == null)
            {
                switch(key)
                {
                    case BaseAssets.BlankTexture:
                        Texture2D bt = new Microsoft.Xna.Framework.Graphics.Texture2D(Game.GraphicsDevice, 1, 1);
                        bt.SetData<Color>(new Color[] { Color.Black });
                        AddAsset<T>(key.ToString(), bt);
                        break;
                }                
            }

            return (T)returnObj;
        }
        public void ClearAssets()
        {
            Texture2D.Clear();
            Texture3D.Clear();
            Effects.Clear();
            Songs.Clear();
            SoundEffects.Clear();
            Fonts.Clear();
            Models.Clear();
            TextureCubes.Clear();

            gameCM.Unload();
        }
    }
}
