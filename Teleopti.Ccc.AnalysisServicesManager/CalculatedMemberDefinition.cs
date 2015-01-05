using System.Collections.Generic;
using System.Xml.Serialization;

namespace AnalysisServicesManager
{
	[XmlRoot("CalculatedMemberDefinition")]
	public class CalculatedMemberDefinition
	{
		[XmlElement("CalculatedMember")]
		public List<CalculatedMember> CalculatedMembers { get; set; }
	}
}