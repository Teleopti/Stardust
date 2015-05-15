using System;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Client
{
	public interface IMultiTenancyWindowsLogon
	{
		AuthenticationResult Logon(LogonModel logonModel, string userAgent);
		bool CheckWindowsIsPossible();
	}

	public class MultiTenancyWindowsLogon : IMultiTenancyWindowsLogon
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAuthenticationQuerier _authenticationQuerier;
		private readonly IWindowsUserProvider _windowsUserProvider;
		private readonly Func<IApplicationData> _applicationData;

		public MultiTenancyWindowsLogon(IRepositoryFactory repositoryFactory, IAuthenticationQuerier authenticationQuerier,
			IWindowsUserProvider windowsUserProvider, Func<IApplicationData> applicationData) 
		{
			_repositoryFactory = repositoryFactory;
			_authenticationQuerier = authenticationQuerier;
			_windowsUserProvider = windowsUserProvider;
			_applicationData = applicationData;
		}

		public AuthenticationResult Logon(LogonModel logonModel, string userAgent)
		{
			logonModel.UserName = _windowsUserProvider.Identity();
			var result = _authenticationQuerier.TryLogon(new IdentityLogonClientModel{Identity = logonModel.UserName}, userAgent);
			if (!result.Success)
				return new AuthenticationResult
				{
					Successful = false,
					HasMessage = true,
					Message = result.FailReason
				};

			var dataSourceName = result.Tenant;
			var personId = result.PersonId;

			logonModel.SelectedDataSourceContainer = new DataSourceContainer(_applicationData().Tenant(dataSourceName), AuthenticationTypeOption.Application);

			using (var uow = logonModel.SelectedDataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				var person = _repositoryFactory.CreatePersonRepository(uow).LoadPersonAndPermissions(personId);
				logonModel.SelectedDataSourceContainer.SetUser(person);
			}

			return new AuthenticationResult
			{
				Person = logonModel.SelectedDataSourceContainer.User,
				Successful = true
			};
		}

		public bool CheckWindowsIsPossible()
		{
			return _authenticationQuerier.TryLogon(new IdentityLogonClientModel{Identity = _windowsUserProvider.Identity()}, "").Success;
		}
	}
}