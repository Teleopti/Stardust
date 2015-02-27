using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public class DataSourcesProvider : IDataSourcesProvider
	{
		private readonly IApplicationData _applicationData;

		public DataSourcesProvider(IApplicationData applicationData)
		{
			_applicationData = applicationData;
		}


		public IDataSource RetrieveDataSourceByName(string dataSourceName)
		{
			return _applicationData.DataSource(dataSourceName);
		}
	}
}