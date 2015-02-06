using System.Xml.Linq;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IParseNhibFile
	{
		DataSourceConfiguration CreateDataSourceConfiguration(XDocument xDocument);
	}
}