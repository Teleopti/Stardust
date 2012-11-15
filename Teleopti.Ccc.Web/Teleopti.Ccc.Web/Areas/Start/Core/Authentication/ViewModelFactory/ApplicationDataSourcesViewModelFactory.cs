using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory
{
	public class ApplicationDataSourcesViewModelFactory : IDataSourcesViewModelFactory
	{
		private readonly IDataSourcesProvider _dataSourceProvider;

		public ApplicationDataSourcesViewModelFactory(IDataSourcesProvider dataSourceProvider)
		{
			_dataSourceProvider = dataSourceProvider;
		}

		public IEnumerable<DataSourceViewModelNew> DataSources()
		{
			return _dataSourceProvider.RetrieveDatasourcesForApplication()
				.Select(x => new DataSourceViewModelNew
					{
						Name = x.DataSourceName,
						Type = "application"
					});
		}
	}
}