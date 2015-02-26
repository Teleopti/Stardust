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
		private readonly IFindTenantAndPersonIdForIdentity _findTenantAndPersonIdForIdentity;

		public Authenticator(IDataSourcesProvider dataSourceProvider,
									ITokenIdentityProvider tokenIdentityProvider,
									IRepositoryFactory repositoryFactory,
									IFindTenantAndPersonIdForIdentity findTenantAndPersonIdForIdentity)
		{
			_dataSourceProvider = dataSourceProvider;
			_tokenIdentityProvider = tokenIdentityProvider;
			_repositoryFactory = repositoryFactory;
			_findTenantAndPersonIdForIdentity = findTenantAndPersonIdForIdentity;
		}

		public AuthenticateResult LogonIdentityUser()
		{
			var token = _tokenIdentityProvider.RetrieveToken();
			var personInfo = _findTenantAndPersonIdForIdentity.Find(token.UserIdentifier);
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(personInfo.Tenant);

			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var foundAppUser = _repositoryFactory.CreatePersonRepository(uow).LoadOne(personInfo.PersonId);
				return new AuthenticateResult { Successful = true, Person = foundAppUser, DataSource = dataSource };
			}
		}
	}
}