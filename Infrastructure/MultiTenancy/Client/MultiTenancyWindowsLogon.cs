using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IMultiTenancyWindowsLogon
	{
		//AuthenticationResult Logon(ILogonModel logonModel, string userAgent);
		AuthenticationResult Logon(LogonModel logonModel, IApplicationData applicationData, string userAgent);
		bool CheckWindowsIsPossible();
	}

	public class MultiTenancyWindowsLogon : IMultiTenancyWindowsLogon
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAuthenticationQuerier _authenticationQuerier;
		private readonly IWindowsUserProvider _windowsUserProvider;
		//private readonly IDataSourcesFactory _dataSourcesFactory;

		public MultiTenancyWindowsLogon(IRepositoryFactory repositoryFactory, IAuthenticationQuerier authenticationQuerier,
			IWindowsUserProvider windowsUserProvider) //, IDataSourcesFactory dataSourcesFactory)
		{
			_repositoryFactory = repositoryFactory;
			_authenticationQuerier = authenticationQuerier;
			_windowsUserProvider = windowsUserProvider;
			//_dataSourcesFactory = dataSourcesFactory;
		}

		//public AuthenticationResult Logon(ILogonModel logonModel, string userAgent)
		//{
		//	var userId = _windowsUserProvider.UserName;
		//	var domain = _windowsUserProvider.DomainName;
		//	logonModel.UserName = domain + "\\" + userId;
		//	var result = _authenticationQuerier.TryIdentityLogon(logonModel.UserName, userAgent);
		//	if (!result.Success)
		//		return new AuthenticationResult
		//		{
		//			Successful = false,
		//			HasMessage = true,
		//			Message = result.FailReason,
		//			PasswordPolicy = result.PasswordPolicy		
		//		};

		//	var dataSourceName = result.Tenant;
		//	var personId = result.PersonId;
		//	var dataSourceCfg = result.DataSourceConfiguration;

		//	dataSourceCfg.ApplicationNHibernateConfig[Environment.SessionFactoryName] = dataSourceName;
		//	var datasource = _dataSourcesFactory.Create(dataSourceCfg.ApplicationNHibernateConfig,
		//		dataSourceCfg.AnalyticsConnectionString);
		//	logonModel.SelectedDataSourceContainer = new DataSourceContainer(datasource, _repositoryFactory, null, AuthenticationTypeOption.Application);
		//	if (logonModel.SelectedDataSourceContainer == null)
		//		return new AuthenticationResult
		//		{
		//			Successful = false,
		//			HasMessage = true,
		//			Message = string.Format(Resources.CannotFindDataSourceWithName, dataSourceName),
		//			PasswordPolicy = result.PasswordPolicy
		//		};

		//	using (var uow = logonModel.SelectedDataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
		//	{
		//		var person = _repositoryFactory.CreatePersonRepository(uow).LoadOne(personId);
		//		logonModel.SelectedDataSourceContainer.SetUser(person);
		//		logonModel.SelectedDataSourceContainer.LogOnName = logonModel.UserName;
		//	}

		//	return new AuthenticationResult
		//	{
		//		Person = logonModel.SelectedDataSourceContainer.User,
		//		Successful = true,
		//		PasswordPolicy = result.PasswordPolicy
		//	};
		//}

		public AuthenticationResult Logon(LogonModel logonModel, IApplicationData applicationData, string userAgent)
		{
			var userId = _windowsUserProvider.UserName;
			var domain = _windowsUserProvider.DomainName;
			logonModel.UserName = domain + "\\" + userId;
			var result = _authenticationQuerier.TryIdentityLogon(new IdentityLogonClientModel{Identity = logonModel.UserName}, userAgent);
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

		public bool CheckWindowsIsPossible()
		{
			var userId = _windowsUserProvider.UserName;
			var domain = _windowsUserProvider.DomainName;

			return _authenticationQuerier.TryIdentityLogon(new IdentityLogonClientModel{Identity = domain + "\\" + userId}, "").Success;
		}

		private IDataSourceContainer getDataSorce(string dataSourceName, DataSourceConfig nhibConfig, IApplicationData applicationData)
		{
			applicationData.MakeSureDataSourceExists(dataSourceName, nhibConfig.ApplicationNHibernateConfig, nhibConfig.AnalyticsConnectionString);
			return new DataSourceContainer(applicationData.DataSource(dataSourceName), _repositoryFactory, null, AuthenticationTypeOption.Application);
		}
	}
}