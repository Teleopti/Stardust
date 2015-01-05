using System.Collections.Generic;
using System.Xml.Serialization;

namespace AnalysisServicesManager
{
	[XmlRoot("DatasourceViewDefinition")]
	public class DatasourceViewDefinition
	{
		[XmlElement(ElementName = "DataTable")]
		public List<RelationalTable> DataTables { get; set; }
	}
}