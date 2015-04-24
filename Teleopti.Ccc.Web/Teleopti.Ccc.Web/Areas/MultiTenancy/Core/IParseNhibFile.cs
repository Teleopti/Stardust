using System;
using System.Xml.Linq;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IParseNhibFile
	{
		Tuple<string, DataSourceConfiguration> CreateDataSourceConfiguration(XDocument xDocument);
	}
}