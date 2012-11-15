using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory
{
	public class WindowsDataSourcesViewModelFactory : IDataSourcesViewModelFactory
	{
		private readonly IDataSourcesProvider _dataSourcesProvider;

		public WindowsDataSourcesViewModelFactory(IDataSourcesProvider dataSourcesProvider)
		{
			_dataSourcesProvider = dataSourcesProvider;
		}

		public IEnumerable<DataSourceViewModelNew> DataSources()
		{
			return _dataSourcesProvider.RetrieveDatasourcesForWindows()
				.Select(x => new DataSourceViewModelNew
					{
						Name = x.DataSourceName,
						Type = "windows"
					});
		}
	}
}