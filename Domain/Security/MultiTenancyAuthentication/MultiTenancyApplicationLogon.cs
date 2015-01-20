using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
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

		public AuthenticationResult Logon(ILogonModel logonModel)
		{
			var allAppContainers =
				logonModel.DataSourceContainers.Where(d => d.AuthenticationTypeOption == AuthenticationTypeOption.Application)
					.ToList();
			var result = _authenticationQuerier.TryLogon(logonModel.UserName, logonModel.Password);
			if (!result.Success)
				return new AuthenticationResult
				{
					Successful = false
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