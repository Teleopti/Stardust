using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Core.RequestContext.Initialize
{
	public class SessionPrincipalFactory : ISessionPrincipalFactory
	{
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly IPrincipalFactory _principalFactory;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;
		private readonly IDataSourceScope _dataSource;

		public SessionPrincipalFactory(IDataSourceForTenant dataSourceForTenant,
			ISessionSpecificDataProvider sessionSpecificDataProvider,
			IRepositoryFactory repositoryFactory,
			IRoleToPrincipalCommand roleToPrincipalCommand,
			IPrincipalFactory principalFactory,
			ITokenIdentityProvider tokenIdentityProvider,
			IDataSourceScope dataSource)
		{
			_dataSourceForTenant = dataSourceForTenant;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_repositoryFactory = repositoryFactory;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_principalFactory = principalFactory;
			_tokenIdentityProvider = tokenIdentityProvider;
			_dataSource = dataSource;
		}

		public ITeleoptiPrincipal Generate()
		{
			var sessionData = _sessionSpecificDataProvider.GrabFromCookie();
			return sessionData == null ? null : createPrincipal(sessionData);
		}

		private ITeleoptiPrincipal createPrincipal(SessionSpecificData sessionData)
		{
			var dataSource = _dataSourceForTenant.Tenant(sessionData.DataSourceName);
			if (dataSource == null)
				return null;

			using (_dataSource.OnThisThreadUse(dataSource.DataSourceName))
			{
				ITeleoptiPrincipal principal;
				using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var personRepository = _repositoryFactory.CreatePersonRepository(uow);
					var person = personRepository.Load(sessionData.PersonId);

					var businessUnitRepository = _repositoryFactory.CreateBusinessUnitRepository(uow);
					var businessUnit = businessUnitRepository.Get(sessionData.BusinessUnitId);
					principal = makePrincipalAndHandleThatPersonMightNotExist(dataSource.Application, personRepository, dataSource, businessUnit, person);
				}
				return principal;
			}
		}

		private ITeleoptiPrincipal makePrincipalAndHandleThatPersonMightNotExist(IUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository, IDataSource dataSource, IBusinessUnit businessUnit, IPerson person)
		{
			try
			{
				var token = _tokenIdentityProvider.RetrieveToken();
				var principal = _principalFactory.MakePrincipal(person, dataSource, businessUnit, token?.OriginalToken);
				_roleToPrincipalCommand.Execute(principal, unitOfWorkFactory, personRepository);
				return principal;
			}
			catch (PersonNotFoundException)
			{
				return null;
			}
		}
	}
}