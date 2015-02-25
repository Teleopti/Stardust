using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class Authenticator : IIdentityLogon
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IIdentityUserQuery _identityUserQuery;

		public Authenticator(IDataSourcesProvider dataSourceProvider,
									ITokenIdentityProvider tokenIdentityProvider,
									IRepositoryFactory repositoryFactory,
									IIdentityUserQuery identityUserQuery)
		{
			_dataSourceProvider = dataSourceProvider;
			_tokenIdentityProvider = tokenIdentityProvider;
			_repositoryFactory = repositoryFactory;
			_identityUserQuery = identityUserQuery;
		}

		public AuthenticateResult LogonIdentityUser()
		{
			var winAccount = _tokenIdentityProvider.RetrieveToken();
			//TODO: tenant - use a simpler query here when joining these two methods. Tenant is enough to get back.
			var tenant = _identityUserQuery.FindUserData(winAccount.UserIdentifier).Tenant;
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(tenant);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				IPerson foundUser;
				if (_repositoryFactory.CreatePersonRepository(uow).TryFindIdentityAuthenticatedPerson(winAccount.UserIdentifier, out foundUser))
				{
					return new AuthenticateResult { Successful = true, Person = foundUser, DataSource = dataSource };
				}
			}

			return null;
		}

		public AuthenticateResult LogonApplicationUser(string dataSourceName)
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