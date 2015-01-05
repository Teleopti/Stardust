using System.Xml.Serialization;

namespace AnalysisServicesManager
{
	public class TeleoptiContraints
	{
		[XmlElement(ElementName = "fkTableName")]
		public string FkTableName { get; set; }

		[XmlElement(ElementName = "fkColumName")]
		public string FkColumName { get; set; }

		[XmlElement(ElementName = "pkTableName")]
		public string PkTableName { get; set; }

		[XmlElement(ElementName = "pkColumName")]
		public string PkColumName { get; set; }
	}
}