using System.Collections.Generic;
using System.Xml.Serialization;

namespace AnalysisServicesManager
{
	public class RelationalTable
	{
		public string DbSchemaName { get; set; }
		public string DbTableName { get; set; }
		public string TableType { get; set; }
		public string CommandText { get; set; }

		[XmlArrayItem(ElementName = "Constraint")]
		public List<TeleoptiContraints> Constraints { get; set; }
	}
}