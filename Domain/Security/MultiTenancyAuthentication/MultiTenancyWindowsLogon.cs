using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IMultiTenancyWindowsLogon
	{
		AuthenticationResult Logon(ILogonModel logonModel, IApplicationData applicationData, string userAgent);
	}

	public class MultiTenancyWindowsLogon : IMultiTenancyWindowsLogon
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAuthenticationQuerier _authenticationQuerier;
		private readonly IWindowsUserProvider _windowsUserProvider;

		public MultiTenancyWindowsLogon(IRepositoryFactory repositoryFactory, IAuthenticationQuerier authenticationQuerier,
			IWindowsUserProvider windowsUserProvider)
		{
			_repositoryFactory = repositoryFactory;
			_authenticationQuerier = authenticationQuerier;
			_windowsUserProvider = windowsUserProvider;
		}

		public AuthenticationResult Logon(ILogonModel logonModel, IApplicationData applicationData, string userAgent)
		{
			var userId = _windowsUserProvider.UserName;
			var domain = _windowsUserProvider.DomainName;
			logonModel.UserName = domain + "\\" + userId;
			var result = _authenticationQuerier.TryIdentityLogon(logonModel.UserName, userAgent);
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
			if (logonModel.SelectedDataSourceContainer == null)
				return new AuthenticationResult
				{
					Successful = false,
					HasMessage = true,
					Message = string.Format(Resources.CannotFindDataSourceWithName, dataSourceName)
				};

			using (var uow = logonModel.SelectedDataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var person = _repositoryFactory.CreatePersonRepository(uow).LoadOne(personId);
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
			var datasource = applicationData.CreateAndAddDataSource(dataSourceName, nhibConfig.ApplicationNHibernateConfig, nhibConfig.AnalyticsConnectionString);

			return new DataSourceContainer(datasource, _repositoryFactory, null, AuthenticationTypeOption.Application);
		}
	}
}