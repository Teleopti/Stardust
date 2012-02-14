using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class Authenticator : IAuthenticator
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly IWindowsAccountProvider _windowsAccountProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IFindApplicationUser _findApplicationUser;

		public Authenticator(IDataSourcesProvider dataSourceProvider,
									IWindowsAccountProvider windowsAccountProvider,
									IRepositoryFactory repositoryFactory,
									IFindApplicationUser findApplicationUser)
		{
			_dataSourceProvider = dataSourceProvider;
			_windowsAccountProvider = windowsAccountProvider;
			_repositoryFactory = repositoryFactory;
			_findApplicationUser = findApplicationUser;
		}


		public AuthenticateResult AuthenticateWindowsUser(string dataSourceName)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				IPerson foundUser;
				var winAccount = _windowsAccountProvider.RetrieveWindowsAccount();
				if (_repositoryFactory.CreatePersonRepository(uow).TryFindWindowsAuthenticatedPerson(winAccount.DomainName,
																	winAccount.UserName,
																	out foundUser))
				{
					return new AuthenticateResult { Successful = true, Person = foundUser, DataSource = dataSource };
				}
			}

			return null;
		}

		public AuthenticateResult AuthenticateApplicationUser(string dataSourceName, string userName, string password)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var authResult = _findApplicationUser.CheckLogOn(uow, userName, password);
				return new AuthenticateResult { DataSource = dataSource, Person = authResult.Person, Successful = authResult.Successful };
			}

		}
	}
}