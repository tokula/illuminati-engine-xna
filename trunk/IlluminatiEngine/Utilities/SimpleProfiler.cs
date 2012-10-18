using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;


namespace IlluminatiEngine.Utilities
{

    public static class SimpleProfiler
    {
        public static void Initialize(bool millisecondTimer, Game game, SpriteFont spriteFont)
        {
            m_millisecondTimer = millisecondTimer;
            m_profileDictionary = new Dictionary<string, ProfileInformation>();
            m_profileDictionary[m_totalID] = new ProfileInformation(m_totalID, m_millisecondTimer);

            m_texture = new Texture2D(game.GraphicsDevice, s_textureWidth, s_textureHeight);
            ScreenPosition = new Vector2(0, 30);
            m_spriteBatch = new SpriteBatch(game.GraphicsDevice);
            m_spriteFont = spriteFont;
        }

        public static void Cleanup()
        {
            Enabled = false;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////	

        public static void StartUpdate()
        {
            StartProfileBlock(m_totalID);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////	

        public static void EndUpdate()
        {
            EndProfileBlock(m_totalID);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////	

        public static void Draw(GameTime gameTime)
        {
            if (Enabled)
            {
                //SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState, Matrix.Identity);
                int numEntries = m_profileDictionary.Count;
                int ystep = s_textureHeight / numEntries;
                int counter = 0;

                //int fontHeight = m_spriteFont.

                ProfileInformation totalPI = m_profileDictionary[m_totalID];

                //unsafe
                {
                    uint[] textureData = new uint[s_textureWidth * s_textureHeight];
                    //Array.Clear(textureData, 0, textureData.Length);

                    m_texture.GetData<uint>(textureData);
                    foreach (String key in m_profileDictionary.Keys)
                    {
                        ProfileInformation currentPi = m_profileDictionary[key];
                        Vector2 lineDims = m_spriteFont.MeasureString(currentPi.debugInfo());
                        float scaler = totalPI.LastTime != 0 ? (float)((double)currentPi.LastTime / (double)totalPI.LastTime) : 1.0f;

                        // seems to have a bug where individual values exceed total so clamp.
                        scaler = Math.Min(1f, scaler);


                        int scaledWidth = (int)(scaler * s_textureWidth);

                        Color colour = Color.White;
                        switch (counter % 3)
                        {
                            case 0:
                                colour = Color.Red;
                                break;
                            case 1:
                                colour = Color.Yellow;
                                break;
                            case 2:
                                colour = Color.Green;
                                break;
                        }

                        int xpos = (int)ScreenPosition.X;
                        int ypos = (int)ScreenPosition.Y + (counter * ystep);
                        for (int y = 0; y < ystep; ++y)
                        {
                            for (int x = 0; x < scaledWidth; ++x)
                            {
                                int index = (ystep * counter * s_textureWidth) + (s_textureWidth * y) + x;
                                textureData[index] = colour.PackedValue;
                            }
                            
                            for (int x = scaledWidth; x < s_textureWidth;++x )
                            {
                                int index = (ystep * counter * s_textureWidth) + (s_textureWidth * y) + x;
                                textureData[index] = Color.Gray.PackedValue;
                            }

                        }
                        counter++;
                    }
                    m_texture.SetData(textureData);
                }
                SpriteBatch.Begin();
                SpriteBatch.Draw(m_texture, ScreenPosition, Color.White);
                counter = 0;
                foreach (string key in m_profileDictionary.Keys)
                {
                    int xpos = (int)ScreenPosition.X;
                    int ypos = (int)ScreenPosition.Y + (counter * ystep);
                    String debugInfo = m_profileDictionary[key].debugInfo();
                    Vector2 lineDims = m_spriteFont.MeasureString(debugInfo);


                    SpriteBatch.DrawString(m_spriteFont, debugInfo, new Vector2(xpos, ypos), Color.Black);
                    ++counter;
                }
                SpriteBatch.End();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////	

        public static void StartProfileBlock(String id)
        {
            if (Enabled)
            {
                ProfileInformation info = FindProfileBlock(id);
                if (info == null)
                {
                    info = new ProfileInformation(id, m_millisecondTimer);
                    m_profileDictionary[id] = info;
                }
                info.start();
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////	

        public static void EndProfileBlock(String id)
        {
            if (Enabled)
            {
                ProfileInformation info = FindProfileBlock(id);
                //System.Diagnostics.Debug.Assert(info != null, "Trying to close a block that wasn't opened");
                // stopped asserts here as with this toggleable we can miss an update
                if (info != null)
                {
                    info.stop();
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////	

        private static ProfileInformation FindProfileBlock(String id)
        {
            ProfileInformation result = null;
            if (m_profileDictionary.ContainsKey(id))
            {
                result = m_profileDictionary[id];
            }
            return result;
        }



        public static Vector2 ScreenPosition
        {
            get { return m_screenPosition; }
            set { m_screenPosition = value; }
        }

        public static bool Enabled
        {
            get { return m_enabled; }
            set { m_enabled = value; }
        }

        public static string Id
        {
            get { return m_id; }
            set { m_id = value; }
        }

        public static SpriteBatch SpriteBatch
        {
            get { return m_spriteBatch; }
        }

        private static string m_id;
        private static bool m_enabled;
        private static Vector2 m_screenPosition;
        private static SpriteBatch m_spriteBatch;
        private static SpriteFont m_spriteFont;



        private static bool m_millisecondTimer;
        private static Dictionary<String, ProfileInformation> m_profileDictionary;
        private static int s_textureWidth = 400;
        private static int s_textureHeight = 200;
        private static Texture2D m_texture;
        private static String m_totalID = "--TOTAL--";
    }
    ///////////////////////////////////////////////////////////////////////////////////////////////	
    // Simple class to keep track of number of calls to see how certain sections are behaving.
    class ProfileInformation
    {
        public ProfileInformation(String id, bool millisecondTimer)
        {
            m_id = id;
            m_minTime = long.MaxValue;
            m_maxTime = long.MinValue;
            m_lastTime = 0;
            m_avgTime = 0;
            m_numCalls = 0;
            m_open = false;
            m_stopWatch = new Stopwatch();
            m_millisecondTimer = millisecondTimer;
        }

        public void start()
        {
            System.Diagnostics.Debug.Assert(m_open == false, "Trying to re-open a currently open block");
            m_open = true;
            m_stopWatch.Start();
        }

        public void stop()
        {
            //System.Diagnostics.Debug.Assert(m_open == true, "Trying to close a currently closed block");
            m_stopWatch.Stop();
            //update(m_stopWatch.ElapsedMilliseconds);
            update(m_millisecondTimer ? m_stopWatch.ElapsedMilliseconds : m_stopWatch.ElapsedTicks);
            m_stopWatch.Reset();
            m_open = false;
        }

        private void update(long elapsed)
        {
            m_lastTime = elapsed;
            m_totalTime += elapsed;
            m_minTime = Math.Min(m_minTime, elapsed);
            m_maxTime = Math.Max(m_maxTime, elapsed);
            m_numCalls++;
            m_avgTime = m_totalTime / m_numCalls;
        }

        public String debugInfo()
        {
            return String.Format("{0} Last[{1}] Min[{2}] Avg[{3}] Max [{4}] NC [{5}]", m_id, m_lastTime, m_minTime, m_avgTime, m_maxTime, m_numCalls);
        }

        public long LastTime
        {
            get { return m_lastTime; }
        }

        public long MinTime
        {
            get { return m_minTime; }
        }

        public long MaxTime
        {
            get { return m_maxTime; }
        }

        public long AvgTime
        {
            get { return m_avgTime; }
        }

        public long NumCalls
        {
            get { return m_numCalls; }
        }


        String m_id;
        long m_lastTime;
        long m_minTime;
        long m_maxTime;
        long m_avgTime;
        long m_numCalls;
        long m_totalTime; // yep this could loop but don't mind too much.

        Stopwatch m_stopWatch;
        bool m_open;  // Till I get a better solution this will enforce non recursive calls.
        bool m_millisecondTimer;
    }

}


