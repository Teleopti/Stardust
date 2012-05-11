using System.Diagnostics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;

namespace Teleopti.Ccc.Web.Core.RequestContext
{
	public class SessionPrincipalFactory : ISessionPrincipalFactory
	{
		private readonly IDataSourcesProvider _dataSourcesProvider;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly IPrincipalFactory _principalFactory;

		public SessionPrincipalFactory(
			IDataSourcesProvider dataSourcesProvider,
			ISessionSpecificDataProvider sessionSpecificDataProvider,
			IRepositoryFactory repositoryFactory,
			IRoleToPrincipalCommand roleToPrincipalCommand,
			IPrincipalFactory principalFactory
			)
		{
			_dataSourcesProvider = dataSourcesProvider;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_repositoryFactory = repositoryFactory;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_principalFactory = principalFactory;
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
				var businessUnitRepository = _repositoryFactory.CreateBusinessUnitRepository(uow);

				var person = personRepository.Load(sessionData.PersonId);
				if (person == null)
					return null;

				var businessUnit = businessUnitRepository.Load(sessionData.BusinessUnitId);

				principal = _principalFactory.MakePrincipal(person, dataSource, businessUnit, sessionData.AuthenticationType);
				_roleToPrincipalCommand.Execute(principal, uow, personRepository);
			}

			return principal;
		}
	}
}