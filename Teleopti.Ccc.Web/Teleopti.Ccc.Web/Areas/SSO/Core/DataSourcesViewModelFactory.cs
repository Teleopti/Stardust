using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public class DataSourcesViewModelFactory : IDataSourcesViewModelFactory
	{
		private readonly IApplicationAuthenticationType _authenticationType;

		public DataSourcesViewModelFactory(IApplicationAuthenticationType authenticationType)
		{
			_authenticationType = authenticationType;
		}

		public IEnumerable<DataSourceViewModel> DataSources()
		{
			return _authenticationType.DataSources().Select(dataSource => new DataSourceViewModel
			{
				Name = dataSource.DataSourceName,
				Type = "application"
			});
		}
	}

	public interface IDataSourcesViewModelFactory
	{
		IEnumerable<DataSourceViewModel> DataSources();
	}
}