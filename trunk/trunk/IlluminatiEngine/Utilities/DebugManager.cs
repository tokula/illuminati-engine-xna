using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using IlluminatiEngine.Interfaces;
using System.IO;

namespace IlluminatiEngine.Utilities
{
    
    public class DebugManager : IDisposable
    {
#if WINDOWS
        public DebugManager(string filename)
        {

            String directoryName = Path.GetDirectoryName(filename);
            DirectoryInfo di = Directory.CreateDirectory(directoryName);
            m_streamWriter = new StreamWriter(File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read));
        }

        
        public DebugManager(StreamWriter streamWriter)
        {
            m_streamWriter = streamWriter;
        }
         
        public void Dispose()
        {
            if (m_streamWriter != null)
            {
                m_streamWriter.Flush();
                m_streamWriter.Close();
                m_streamWriter = null;
            }
        }


        public void DumpDebugInfo(GameComponentCollection collection)
        {
            if (m_streamWriter != null)
            {
                StringBuilder stringBuilder = new StringBuilder();
                // timestamp
                stringBuilder.AppendLine("Timestamp : "+DateTime.Now.ToString());
                foreach (GameComponent gc in collection)
                {
                    IDebuggable i = gc as IDebuggable;
                    if (i != null)
                    {
                        i.DumpDebugInfo(stringBuilder);
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine("--------------------------------------------------------------------------");
                        stringBuilder.AppendLine();
                    }
                }

                m_streamWriter.Write(stringBuilder.ToString());
                m_streamWriter.Flush();
            }
        }

        private StreamWriter m_streamWriter;
#else
        public void Dispose(){}
#endif
    }

}
