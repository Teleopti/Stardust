using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class Authenticator : IIdentityLogon
	{
		private readonly IApplicationData _applicationData;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IFindTenantAndPersonIdForIdentity _findTenantAndPersonIdForIdentity;

		public Authenticator(IApplicationData applicationData,
									ITokenIdentityProvider tokenIdentityProvider,
									IRepositoryFactory repositoryFactory,
									IFindTenantAndPersonIdForIdentity findTenantAndPersonIdForIdentity)
		{
			_applicationData = applicationData;
			_tokenIdentityProvider = tokenIdentityProvider;
			_repositoryFactory = repositoryFactory;
			_findTenantAndPersonIdForIdentity = findTenantAndPersonIdForIdentity;
		}

		public AuthenticateResult LogonIdentityUser()
		{
			var token = _tokenIdentityProvider.RetrieveToken();
			var personInfo = _findTenantAndPersonIdForIdentity.Find(token.UserIdentifier);
			if (personInfo == null)
				return new AuthenticateResult{Successful = false};

			var dataSource = _applicationData.DataSource(personInfo.Tenant);
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var foundAppUser = _repositoryFactory.CreatePersonRepository(uow).LoadOne(personInfo.PersonId);
				return new AuthenticateResult { Successful = true, Person = foundAppUser, DataSource = dataSource };
			}
		}
	}
}