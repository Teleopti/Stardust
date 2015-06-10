using System;
using System.Xml.Linq;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IParseNhibFile
	{
		Tuple<string, DataSourceConfiguration> CreateDataSourceConfiguration(XDocument xDocument);
	}
}