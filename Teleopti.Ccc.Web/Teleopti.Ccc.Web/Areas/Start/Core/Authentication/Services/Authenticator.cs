using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class Authenticator : IIdentityLogon
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IIdentityUserQuery _identityUserQuery;
		private readonly IApplicationUserTenantQuery _applicationUserTenantQuery;

		public Authenticator(IDataSourcesProvider dataSourceProvider,
									ITokenIdentityProvider tokenIdentityProvider,
									IRepositoryFactory repositoryFactory,
									IIdentityUserQuery identityUserQuery,
									IApplicationUserTenantQuery applicationUserTenantQuery)
		{
			_dataSourceProvider = dataSourceProvider;
			_tokenIdentityProvider = tokenIdentityProvider;
			_repositoryFactory = repositoryFactory;
			_identityUserQuery = identityUserQuery;
			_applicationUserTenantQuery = applicationUserTenantQuery;
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

		public AuthenticateResult LogonApplicationUser()
		{
			var account = _tokenIdentityProvider.RetrieveToken();
			var personInfo = _applicationUserTenantQuery.Find(account.UserIdentifier);
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(personInfo.Tenant);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var foundAppUser = _repositoryFactory.CreatePersonRepository(uow).LoadOne(personInfo.Id);
				return new AuthenticateResult { Successful = true, Person = foundAppUser, DataSource = dataSource };
			}
		}
	}
}