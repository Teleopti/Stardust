using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.RequestContext.Initialize
{
	public class SessionPrincipalFactory : ISessionPrincipalFactory
	{
		private readonly IDataSourcesProvider _dataSourcesProvider;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly IPrincipalFactory _principalFactory;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;

		public SessionPrincipalFactory(IDataSourcesProvider dataSourcesProvider, ISessionSpecificDataProvider sessionSpecificDataProvider, IRepositoryFactory repositoryFactory, IRoleToPrincipalCommand roleToPrincipalCommand, IPrincipalFactory principalFactory, ITokenIdentityProvider tokenIdentityProvider)
		{
			_dataSourcesProvider = dataSourcesProvider;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_repositoryFactory = repositoryFactory;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_principalFactory = principalFactory;
			_tokenIdentityProvider = tokenIdentityProvider;
		}

		public ITeleoptiPrincipal Generate()
		{


			var sessionData = _sessionSpecificDataProvider.GrabFromCookie();
			return sessionData == null ? null : createPrincipal(sessionData);
		}

		private ITeleoptiPrincipal createPrincipal(SessionSpecificData sessionData)
		{
			var dataSource = _dataSourcesProvider.RetrieveDataSourceByName(sessionData.DataSourceName);
			if (dataSource == null)
				return null;

			ITeleoptiPrincipal principal;
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRepository = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRepository.Load(sessionData.PersonId);

				var businessUnitRepository = _repositoryFactory.CreateBusinessUnitRepository(uow);
				var businessUnit = businessUnitRepository.Load(sessionData.BusinessUnitId);

				principal = makePrincipalAndHandleThatPersonMightNotExist(dataSource.Application, personRepository, dataSource, businessUnit, person);
			}

			return principal;
		}

		private ITeleoptiPrincipal makePrincipalAndHandleThatPersonMightNotExist(IUnitOfWorkFactory unitOfWorkFactory, IPersonRepository personRepository, IDataSource dataSource, IBusinessUnit businessUnit, IPerson person)
		{
			try
			{
				var token = _tokenIdentityProvider.RetrieveToken();
				var principal = _principalFactory.MakePrincipal(person, dataSource, businessUnit, token == null ? null : token.OriginalToken);
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