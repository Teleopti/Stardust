using System.Xml;
using System.Xml.Serialization;

namespace AnalysisServicesManager
{
	public class ParseDataFromXml<T>
	{
		public T Parse(string filePath)
		{
			var serializer = new XmlSerializer(typeof(T));
			T def;
			using (var xmlTextReader = new XmlTextReader(filePath))
			{
				def = (T)serializer.Deserialize(xmlTextReader);
			}
			return def;
		}
	}
}
