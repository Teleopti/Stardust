using System.Linq;
using System.Xml.Linq;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	//tested from ReadNHibFilesTest
	public class ParseNhibFile : IParseNhibFile
	{
		private readonly XNamespace nhibNs = "urn:nhibernate-configuration-2.2";

		public DataSourceConfiguration CreateDataSourceConfiguration(XDocument xDocument)
		{
			var datasourceElement = xDocument.Root;
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