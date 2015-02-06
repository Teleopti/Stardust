using System;
using System.Xml.Linq;

namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IParseNhibFile
	{
		Tuple<string, DataSourceConfiguration> CreateDataSourceConfiguration(XDocument xDocument);
	}
}