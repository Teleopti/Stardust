using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class Authenticator : IIdentityLogon
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IRepositoryFactory _repositoryFactory;

		public Authenticator(IDataSourcesProvider dataSourceProvider,
									ITokenIdentityProvider tokenIdentityProvider,
									IRepositoryFactory repositoryFactory)
		{
			_dataSourceProvider = dataSourceProvider;
			_tokenIdentityProvider = tokenIdentityProvider;
			_repositoryFactory = repositoryFactory;
		}

		public AuthenticateResult LogonWindowsUser(string dataSourceName)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				IPerson foundUser;
				var winAccount = _tokenIdentityProvider.RetrieveToken();
				if (_repositoryFactory.CreatePersonRepository(uow).TryFindIdentityAuthenticatedPerson(winAccount.UserIdentifier,
					out foundUser))
				{
					return new AuthenticateResult { Successful = true, Person = foundUser, DataSource = dataSource };
				}
			}

			return null;
		}

		public AuthenticateResult LogonApplicationIdentityUser(string dataSourceName)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var account = _tokenIdentityProvider.RetrieveToken();
				var foundUser = _repositoryFactory.CreatePersonRepository(uow).TryFindBasicAuthenticatedPerson(account.UserIdentifier);
				if (foundUser != null)
				{
					return new AuthenticateResult { Successful = true, Person = foundUser, DataSource = dataSource };
				}
			}

			return null;
		}
	}
}