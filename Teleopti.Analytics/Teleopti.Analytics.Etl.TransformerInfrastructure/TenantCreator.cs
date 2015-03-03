using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
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

		public static List<IDataSource> DataSources(string xmlDirectory, IRepositoryFactory repositoryFactory, DataSourcesFactory dataSourcesFactory)
		{
			var dataSources = new List<IDataSource>();

			foreach (string file in Directory.GetFiles(xmlDirectory, "*.nhib.xml"))
			{
				XElement element = XElement.Load(file);
				if (element.Name != "datasource")
				{
					continue;
					//throw new DataSourceException(@"Missing <dataSource> in file " + file);
				}
				
				IDataSource dataSource;
				if (dataSourcesFactory.TryCreate(element, out dataSource))
				{
					dataSource.AuthenticationTypeOption = AuthenticationTypeOption.Application | AuthenticationTypeOption.Windows;
					dataSources.Add(dataSource);
				}
			}

			return dataSources;
		}
	}

	public class TenantName:ITenantName
	{
		public string DataSourceName { get; set; }
	}
}
