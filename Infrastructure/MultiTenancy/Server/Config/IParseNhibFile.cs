using System;
using System.Xml.Linq;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config
{
	public interface IParseNhibFile
	{
		Tuple<string, DataSourceConfiguration> CreateDataSourceConfiguration(XDocument xDocument);
	}
}