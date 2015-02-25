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
			var personInfo = _identityUserQuery.FindUserData(winAccount.UserIdentifier);
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(personInfo.Tenant);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var foundAppUser =  _repositoryFactory.CreatePersonRepository(uow).LoadOne(personInfo.Id);
				return new AuthenticateResult { Successful = true, Person = foundAppUser, DataSource = dataSource };
			}
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