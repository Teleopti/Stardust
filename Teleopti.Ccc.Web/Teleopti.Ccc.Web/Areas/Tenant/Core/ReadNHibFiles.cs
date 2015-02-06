using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Teleopti.Ccc.Web.Core.Startup.InitializeApplication;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public class ReadNHibFiles : IReadNHibFiles
	{
		private readonly ISettings _settings;
		private readonly IPhysicalApplicationPath _physicalApplicationPath;

		public ReadNHibFiles(ISettings settings, IPhysicalApplicationPath physicalApplicationPath)
		{
			_settings = settings;
			_physicalApplicationPath = physicalApplicationPath;
		}

		public IDictionary<string, DataSourceConfiguration> Read()
		{
			var ret = new Dictionary<string, DataSourceConfiguration>();
			var nhibPath = _settings.nhibConfPath();
			var fullPathToNhibFolder = Path.Combine(_physicalApplicationPath.Get(), nhibPath);
			foreach (var nhibFile in Directory.GetFiles(fullPathToNhibFolder, "*.nhib.xml"))
			{
				var dsCfg = createDataSourceConfiguration(nhibFile);
				ret[dsCfg.Tennant] = dsCfg;
			}
			return ret;
		}

		private DataSourceConfiguration createDataSourceConfiguration(string filename)
		{
			XNamespace nhibNs = "urn:nhibernate-configuration-2.2";
			var doc = XDocument.Load(filename);
			var datasourceElement = doc.Root;
			var analyticsConnstring = datasourceElement.Element("matrix").Element("connectionString").Value.Trim();
			var sessionFactoryElement = datasourceElement.Element(nhibNs + "hibernate-configuration").Element(nhibNs + "session-factory");
			var allAppProperties = sessionFactoryElement.Elements(nhibNs + "property").ToDictionary(x => x.Attribute("name").Value, x => x.Value.Trim());

			return new DataSourceConfiguration
			{
				Tennant = sessionFactoryElement.Attribute("name").Value,
				AnalyticsConnectionString = analyticsConnstring,
				ApplicationNHibernateConfig = allAppProperties
			};
		}
	}
}