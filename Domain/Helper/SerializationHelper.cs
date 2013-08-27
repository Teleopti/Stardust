using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Teleopti.Ccc.Domain.Helper
{
    public static class SerializationHelper
    {
        /// <summary>
        /// Serializes the object as XML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The obj.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-07-07
        /// </remarks>
        public static string SerializeAsXml<T>(T obj)
        {
            return SerializeAsXml(typeof(T), obj);
        }

        /// <summary>
        /// Serializes the specified object with type as XML.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-18
        /// </remarks>
        public static string SerializeAsXml(Type type,object value)
        {
            string xmlString;
            using (var memoryStream = new MemoryStream())
            {
				var xs = new XmlSerializer(type);
                using (var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                {
                    xs.Serialize(xmlTextWriter, value);
                    xmlString = UTF8ByteArrayToString(((MemoryStream)xmlTextWriter.BaseStream).ToArray());
                }
            }   
            // Lose any whitespaces or crap characters that XML Serializer produces.
            if (xmlString.Substring(0, 1) != "<")
            {
                int startPos = xmlString.IndexOf("<", StringComparison.OrdinalIgnoreCase);
                xmlString = xmlString.Substring(startPos, xmlString.Length - startPos);
            }
            return xmlString;
        }

        /// <summary>
        /// Deserializes the specified content from XML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-07-07
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T Deserialize<T>(string xml)
        {
            return (T)Deserialize(typeof(T),xml);
        }

        /// <summary>
        /// Deserializes the specified type from xml content.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-18
        /// </remarks>
        public static object Deserialize(Type type, string xml)
        {
            var xs = new XmlSerializer(type);
            object value;
            using (var memoryStream = new MemoryStream(StringToUTF8ByteArray(xml)))
            {
                value = xs.Deserialize(memoryStream);
            }
            return value;
        }

        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">Unicode Byte Array to be converted to String</param>
        /// <returns>String converted from Unicode Byte Array</returns>
        private static string UTF8ByteArrayToString(byte[] characters)
        {
            var encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization
        /// </summary>
        /// <param name="pXmlString"></param>
        /// <returns></returns>
        private static byte[] StringToUTF8ByteArray(string pXmlString)
        {
            var encoding = new UTF8Encoding();
            byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }

        /// <summary>
        /// Deserializes the specified data with binary formatter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-18
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static T Deserialize<T>(byte[] data)
        {
            return (T)Deserialize(data);
        }

        /// <summary>
        /// Deserializes the specified data with binary formatter.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-18
        /// </remarks>
        public static object Deserialize(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Serializes as binary.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-11-18
        /// </remarks>
        public static byte[] SerializeAsBinary(object value)
        {
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, value);
                return stream.ToArray();
            }
        }
    }
}
