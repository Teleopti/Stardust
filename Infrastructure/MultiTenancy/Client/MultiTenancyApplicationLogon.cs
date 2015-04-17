using NHibernate.Cfg;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public class MultiTenancyApplicationLogon : IMultiTenancyApplicationLogon
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAuthenticationQuerier _authenticationQuerier;

		public MultiTenancyApplicationLogon(IRepositoryFactory repositoryFactory, IAuthenticationQuerier authenticationQuerier)
		{
			_repositoryFactory = repositoryFactory;
			_authenticationQuerier = authenticationQuerier;
		}

		public AuthenticationResult Logon(LogonModel logonModel, IApplicationData applicationData, string userAgent)
		{
			var result = _authenticationQuerier.TryApplicationLogon(new ApplicationLogonClientModel{UserName = logonModel.UserName, Password = logonModel.Password}, userAgent);
			if (!result.Success)
				return new AuthenticationResult
				{
					Successful = false,
					HasMessage = true,
					Message = result.FailReason
				};

			var dataSourceName = result.Tenant;
			var personId = result.PersonId;
			var dataSourceCfg = result.DataSourceConfiguration;

			logonModel.SelectedDataSourceContainer = getDataSorce(dataSourceName, dataSourceCfg, applicationData);

			using (var uow = logonModel.SelectedDataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var person = _repositoryFactory.CreatePersonRepository(uow).LoadPersonAndPermissions(personId);
				logonModel.SelectedDataSourceContainer.SetUser(person);
				logonModel.SelectedDataSourceContainer.LogOnName = logonModel.UserName;
			}

			return new AuthenticationResult
			{
				Person = logonModel.SelectedDataSourceContainer.User,
				Successful = true
			};
		}

		private IDataSourceContainer getDataSorce(string dataSourceName, DataSourceConfig nhibConfig, IApplicationData applicationData)
		{
			applicationData.MakeSureDataSourceExists(dataSourceName, nhibConfig.ApplicationNHibernateConfig, nhibConfig.AnalyticsConnectionString);
			return new DataSourceContainer(applicationData.DataSource(dataSourceName), _repositoryFactory, null, AuthenticationTypeOption.Application);
		}
	}
}