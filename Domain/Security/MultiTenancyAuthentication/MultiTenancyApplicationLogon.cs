using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication
{
	public class MultiTenancyApplicationLogon : IApplicationLogon
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
			var dataSourceName = result.Tennant;
			var personId = result.PersonId;

			logonModel.SelectedDataSourceContainer = allAppContainers.Where(d => d.DataSourceName.Equals(dataSourceName)).FirstOrDefault();
			// if null error
			using (var uow = logonModel.SelectedDataSourceContainer.DataSource.Application.CreateAndOpenUnitOfWork())
			{

				logonModel.SelectedDataSourceContainer.SetUser(_repositoryFactory.CreatePersonRepository(uow).LoadOne(personId));
				logonModel.SelectedDataSourceContainer.LogOnName = logonModel.UserName;
			}

			return new AuthenticationResult
			{
				Person = logonModel.SelectedDataSourceContainer.User,
				Successful = true
			};
		}

		public bool ShowDataSourceSelection { get { return false; }  }
	}
}