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
			var userId = _windowsUserProvider.UserName;
			var domain = _windowsUserProvider.DomainName;
			logonModel.UserName = domain + "\\" + userId;
			// fejkar att vi fått datasource från web service
			// we should not have any "windows" datasources now
			var allAppContainers =
				logonModel.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Application)
					.ToList();
			var dataSourceName = "Teleopti WFM";
			var personId = new Guid("10957AD5-5489-48E0-959A-9B5E015B2B5C");

			logonModel.SelectedDataSourceContainer = allAppContainers.Where(d => d.DataSourceName.Equals(dataSourceName)).FirstOrDefault();
			// if null error
			using (var uow = logonModel.SelectedDataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
			{
				logonModel.SelectedDataSourceContainer.SetUser(_repositoryFactory.CreatePersonRepository(uow).LoadOne(personId));
			}

			return new AuthenticationResult
			{
				Person = logonModel.SelectedDataSourceContainer.User,
				Successful = true
			};
		}
	}
}