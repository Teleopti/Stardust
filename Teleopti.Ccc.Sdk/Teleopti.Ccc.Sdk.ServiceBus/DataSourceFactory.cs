using System;
using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public static class DataSourceFactory
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (DataSourceFactory));

				public static SdkDataSourceResult GetDataSource(IDataSourceForTenant dataSourceForTenant, string tenant, IRepositoryFactory repositoryFactory)
				{
					var ret = new SdkDataSourceResult();
		    var dataSource = dataSourceForTenant.Tenant(tenant);
		    if (dataSource == null)
		    {
					Logger.ErrorFormat("No datasource matching the name {0} was found.", tenant);
					throw new ArgumentNullException("tenant",
																					string.Format(CultureInfo.InvariantCulture,
																												"No datasource matching the name {0} was found.",
																												tenant));
		    }
					ret.SuperUser = new LoadUserUnauthorized().LoadFullPersonInSeperateTransaction(dataSource.Application, SuperUser.Id_AvoidUsing_This);
					ret.DataSource = dataSource;
					return ret;
				}
    }

	public class SdkDataSourceResult
	{
		public IDataSource DataSource { get; set; }
		public IPerson SuperUser { get; set; }
	}
}
