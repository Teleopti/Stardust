using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public interface IDataSourcesProvider
	{
		IEnumerable<IDataSource> RetrieveDatasourcesForApplication();
		IEnumerable<IDataSource> RetrieveDatasourcesForWindows();
		IDataSource RetrieveDataSourceByName(string dataSourceName);
		IEnumerable<IDataSource> RetrieveDatasourcesForApplicationIdentityToken();
	}
}