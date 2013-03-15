using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace IlluminatiContentPipelineExtension
{
    /// <summary>
    /// Log Writer
    /// </summary>
    public static class LogWriter
    {
        /// <summary>
        /// Method to write to log file.
        /// </summary>
        /// <param name="data"></param>
        public static void WriteToLog(string data)
        {
            StreamWriter sw = new StreamWriter("IlluminatiContentPipeline.log", true);
            sw.WriteLine(string.Format("{0:dd-MM-yyyy HH:mm:ss} - {1}",DateTime.Now, data));
            sw.Close();
        }
    }
}
