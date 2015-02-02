using System;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
    public static class DataSourceFactory
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (DataSourceFactory));

        public static DataSourceContainer GetDesiredDataSource(IDataSourceProvider dataSourceProvider, string dataSource, IRepositoryFactory repositoryFactory)
        {
            DataSourceContainer dataSourceContainer =
                dataSourceProvider.DataSourceList().FirstOrDefault(d => d.DataSource.Application.Name == dataSource);
            
            if (dataSourceContainer == null)
            {
                Logger.ErrorFormat("No datasource matching the name {0} was found.",dataSource);
                throw new ArgumentNullException("dataSource",
                                                string.Format(CultureInfo.InvariantCulture,
                                                              "No datasource matching the name {0} was found.",
                                                              dataSource));
            }

	        using (var uow = dataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
	        {
		        var systemId = SuperUser.Id_AvoidUsing_This;
		        dataSourceContainer.SetUser(repositoryFactory.CreatePersonRepository(uow).LoadOne(systemId));
	        }
	        return dataSourceContainer;
        }
    }
}
