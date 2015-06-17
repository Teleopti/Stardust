using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	static class TenantCreator
	{
		public static List<ITenantName> TenantNames(string xmlDirectory)
		{
			var dataSources = new List<ITenantName>();
			
				foreach (string file in Directory.GetFiles(xmlDirectory, "*.nhib.xml"))
				{
					XElement element = XElement.Load(file);
					if (element.Name != "datasource")
					{
						continue;
						//throw new DataSourceException(@"Missing <dataSource> in file " + file);
					}
					var dataSourceName = element.Descendants().ElementAt(1).Attribute("name").Value;
					dataSources.Add(new TenantName{DataSourceName = dataSourceName});
				}
			
			return dataSources;
		}
	}

	public class TenantName:ITenantName
	{
		public string DataSourceName { get; set; }
	}
}
