using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	//TODO: tenant remove me!
	public interface IDataSourcesProvider
	{
		IDataSource RetrieveDataSourceByName(string dataSourceName);
	}
}