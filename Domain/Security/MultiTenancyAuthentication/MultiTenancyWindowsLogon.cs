using System;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public interface IMultiTenancyWindowsLogon
	{
		AuthenticationResult Logon(ILogonModel logonModel);
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

		public AuthenticationResult Logon(ILogonModel logonModel)
		{
			var allAppContainers =
				logonModel.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Application)
					.ToList();

			var userId = _windowsUserProvider.UserName;
			var domain = _windowsUserProvider.DomainName;
			logonModel.UserName = domain + "\\" + userId;
			var result = _authenticationQuerier.TryIdentityLogon(logonModel.UserName);
			if (!result.Success)
				return new AuthenticationResult
				{
					Successful = false,
					HasMessage = true,
					Message = result.FailReason			
				};

			var dataSourceName = result.Tennant;
			var personId = result.PersonId;

			logonModel.SelectedDataSourceContainer = allAppContainers.FirstOrDefault(d => d.DataSourceName.Equals(dataSourceName));
			// if null error
			if (logonModel.SelectedDataSourceContainer == null)
				return new AuthenticationResult
				{
					Successful = false
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
	}
}