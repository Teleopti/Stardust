using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AnalysisServicesManager
{
	public class ParseDataViewInfoFromXml
	{
		public List<RelationalTable> ExtractDataViewInfo(string filePath)
		{
			var reader = XmlReader.Create(filePath);
			XmlDocument doc = new XmlDocument();
			doc.Load(reader);
			XmlNode entries = doc.ChildNodes[0];
			var listOfExtracted = new List<RelationalTable>();
			foreach (XmlNode node in entries.ChildNodes)
			{

				if (node.Name == "DataTable")
					listOfExtracted.Add(lookForDataTable(node));
			}
			return listOfExtracted;
		}

		private RelationalTable lookForDataTable(XmlNode nodes)
		{
			var extractedValue = new RelationalTable();
			foreach (XmlNode node in nodes.ChildNodes)
			{
				if (node.Name == "DbTableName")
					extractedValue.DbTableName = node.InnerText;
				if (node.Name == "DbSchemaName")
					extractedValue.DbSchemaName = node.InnerText;
				if (node.Name == "TableType")
					extractedValue.TableType = node.InnerText;
				if (node.Name == "CommandText")
					extractedValue.CommandText = node.InnerText;
				if (node.Name == "Constraints")
					ListOfContraint(node.ChildNodes, extractedValue);
			}
			return extractedValue;
		}

		private void ListOfContraint(XmlNodeList contraintNodes, RelationalTable extractedValue)
		{
			List<TeleoptiContraints> con = new List<TeleoptiContraints>();
			foreach (XmlNode coontraints in contraintNodes)
			{
				con.Add(extratContraintData(coontraints));
			}
			extractedValue.ListOfConstraints = con;
		}

		private TeleoptiContraints extratContraintData(XmlNode coontraints)
		{
			var con = new TeleoptiContraints();
			foreach (XmlNode contraintNode in coontraints.ChildNodes)
			{
				if (contraintNode.Name == "fkTableName")
					con.FkTableName = contraintNode.InnerText;
				if (contraintNode.Name == "fkColumName")
					con.FkColumName = contraintNode.InnerText;
				if (contraintNode.Name == "pkTableName")
					con.PkTableName = contraintNode.InnerText;
				if (contraintNode.Name == "pkColumName")
					con.PkColumName = contraintNode.InnerText;
			}
			return con;
		}
	}
}
