using System;
using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public static class DataSourceFactory
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (DataSourceFactory));

				public static SdkDataSourceResult GetDataSource(IApplicationData applicationData, string tenant, IRepositoryFactory repositoryFactory)
				{
					var ret = new SdkDataSourceResult();
		    var dataSource = applicationData.DataSource(tenant);
		    if (dataSource == null)
		    {
					Logger.ErrorFormat("No datasource matching the name {0} was found.", tenant);
					throw new ArgumentNullException("tenant",
																					string.Format(CultureInfo.InvariantCulture,
																												"No datasource matching the name {0} was found.",
																												tenant));
		    }
				using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var systemId = SuperUser.Id_AvoidUsing_This;
					ret.SuperUser=repositoryFactory.CreatePersonRepository(uow).LoadPersonAndPermissions(systemId);
				}
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
