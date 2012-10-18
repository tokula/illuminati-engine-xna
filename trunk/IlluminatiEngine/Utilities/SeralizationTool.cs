#if !XBOX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;

namespace SerializationXNA
{
    public class SerializableBase<T>
    {
        /// <summary>
        /// ctor
        /// </summary>
        public SerializableBase() { }


        /// <summary>
        /// Method to serialize the object
        /// </summary>
        /// <returns></returns>
        public static byte[] Serialize<T>(T objectInsatnce) where T : class
        {
            DataContractSerializer formatter = new DataContractSerializer(typeof(T));

            MemoryStream memStream = new MemoryStream();
            formatter.WriteObject(memStream, objectInsatnce);

            byte[] buffer = memStream.ToArray();
            memStream.Close();



            return buffer;

        }

        /// <summary>
        /// Method to deserialize the object from a byte array
        /// </summary>
        /// <param name="buffer">byte array holding the serialized object</param>
        /// <returns>Deserialized instance of the object</returns>
        public static T Deserialize<T>(byte[] buffer) where T : class
        {
            DataContractSerializer fomratter = new DataContractSerializer(typeof(T));
            MemoryStream memStream = new MemoryStream(buffer);

            T retVal = (T)fomratter.ReadObject(memStream);

            memStream.Close();

            return retVal;
        }
    }
}
#endif