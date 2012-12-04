using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory
{
	public class DataSourcesViewModelFactory : IDataSourcesViewModelFactory
	{
		private readonly IEnumerable<IAuthenticationType> _authenticationTypes;

		public DataSourcesViewModelFactory(IEnumerable<IAuthenticationType> authenticationTypes)
		{
			_authenticationTypes = authenticationTypes;
		}

		public IEnumerable<DataSourceViewModel> DataSources()
		{
			return (from t in _authenticationTypes
					let dataSources = t.DataSources()
					where dataSources != null
			        from s in dataSources
			        select new DataSourceViewModel
			                {
			                    Name = s.DataSourceName,
			                    Type = t.TypeString
			                })
			    .ToArray();
		}
	}
}