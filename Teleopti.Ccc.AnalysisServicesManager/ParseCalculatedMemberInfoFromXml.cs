using System.Collections.Generic;
using System.Xml;

namespace AnalysisServicesManager
{
	public class ParseCalculatedMemberInfoFromXml
	{
		public List<CalculatedMember> ExtractCalculatedMemberInfo(string filePath)
		{
			var reader = XmlReader.Create(filePath);
			XmlDocument doc = new XmlDocument();
			doc.Load(reader);

			XmlNode calculatedMemberDefinition = doc.GetElementsByTagName("CalculatedMemberDefinition")[0];

			var listOfExtracted = new List<CalculatedMember>();
			foreach (XmlNode node in calculatedMemberDefinition)
			{
				if (node.Name == "CalculatedMember")
					listOfExtracted.Add(lookForDataTable(node));
			}
			return listOfExtracted;
		}

		private CalculatedMember lookForDataTable(XmlNode nodes)
		{
			var extractedValue = new CalculatedMember();
			foreach (XmlNode node in nodes.ChildNodes)
			{
				if (node.Name == "MdxString")
					extractedValue.MdxString = node.InnerText;
			}

			return extractedValue;
		}
	}
}
