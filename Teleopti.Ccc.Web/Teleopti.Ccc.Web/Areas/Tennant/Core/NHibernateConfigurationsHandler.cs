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
		private readonly IPhysicalApplicationPath _physicalApplicationPath;
		private readonly IList<DataSourceHolder> _dataSources = new List<DataSourceHolder>();

		public NHibernateConfigurationsHandler(ISettings settings, IPhysicalApplicationPath physicalApplicationPath)
		{
			_settings = settings;
			_physicalApplicationPath = physicalApplicationPath;
		}

		public string GetConfigForName(string dataSourceName)
		{
			if (!_dataSources.Any())
				loadDataSources();
			foreach (var dataSourceHolder in _dataSources.Where(dataSourceHolder => dataSourceHolder.DataSourceName.Equals(dataSourceName)))
			{
				return dataSourceHolder.DataSourceConfig;
			}
			return string.Empty;
		}

		private void loadDataSources()
		{
			var nhibPath = _settings.nhibConfPath();
			var fullPathToNhibFolder = Path.Combine(_physicalApplicationPath.Get(), nhibPath);

			foreach (var file in Directory.GetFiles(fullPathToNhibFolder, "*.nhib.xml"))
			{
				var element = XElement.Load(file);
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

	public class DataSourceHolder
	{
		public string DataSourceName { get; set; }
		public string DataSourceConfig { get; set; }
	}
}