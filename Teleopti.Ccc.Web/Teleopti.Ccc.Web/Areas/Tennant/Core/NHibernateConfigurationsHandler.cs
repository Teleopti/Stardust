using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public interface INHibernateConfigurationsHandler
	{
		string GetConfigForName(string dataSourceName);
	}

	public class NHibernateConfigurationsHandler : INHibernateConfigurationsHandler
	{
		private readonly ISettings _settings;
		readonly IList<IDataSourceHolder> _dataSources = new List<IDataSourceHolder>();

		public NHibernateConfigurationsHandler(ISettings settings)
		{
			_settings = settings;
		}

		public string GetConfigForName(string dataSourceName)
		{
			if (!_dataSources.Any())
				loadDataSources();
			foreach (var dataSourceHolder in _dataSources.Where(dataSourceHolder => dataSourceHolder.DataSourceName.Equals(dataSourceName)))
			{
				return dataSourceHolder.DataSourceConfig;
			}
			return "";
		}

		private void loadDataSources()
		{
			var nhibPath = _settings.nhibConfPath();

			foreach (string file in Directory.GetFiles(nhibPath, "*.nhib.xml"))
			{
				XElement element = XElement.Load(file);
				if (element.Name != "datasource")
					continue;

				_dataSources.Add(new DataSourceHolder
				{
					DataSourceName = element.Descendants().ElementAt(1).Attribute("name").Value,
					DataSourceConfig = Encryption.EncryptStringToBase64(element.ToString(),
						EncryptionConstants.Image1,
						EncryptionConstants.Image2)
				});
			}
		}
	}

	interface IDataSourceHolder
	{
		string DataSourceName { get; set; }
		string DataSourceConfig { get; set; }
	}

	class DataSourceHolder : IDataSourceHolder
	{
		public string DataSourceName { get; set; }
		public string DataSourceConfig { get; set; }
	}
}