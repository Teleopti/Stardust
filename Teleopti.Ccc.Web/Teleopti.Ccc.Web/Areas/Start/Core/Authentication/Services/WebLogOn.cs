using System;
using System.Threading;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class WebLogOn : IWebLogOn
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ILogOnOff _logOnOff;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;

		public WebLogOn(ILogOnOff logOnOff, 
						IDataSourcesProvider dataSourceProvider, 
						IRepositoryFactory repositoryFactory, 
						ISessionSpecificDataProvider sessionSpecificDataProvider,
						IRoleToPrincipalCommand roleToPrincipalCommand)
		{
			_logOnOff = logOnOff;
			_dataSourceProvider = dataSourceProvider;
			_repositoryFactory = repositoryFactory;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
	    	_roleToPrincipalCommand = roleToPrincipalCommand;
		}

		public void LogOn(Guid businessUnitId, string dataSourceName, Guid personId, AuthenticationTypeOption authenticationType)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);
			using(var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRep = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRep.Get(personId);
				var businessUnit = _repositoryFactory.CreateBusinessUnitRepository(uow).Get(businessUnitId);
				_logOnOff.LogOn(dataSource, person, businessUnit, authenticationType);
				var principal = (TeleoptiPrincipal) Thread.CurrentPrincipal;
				_roleToPrincipalCommand.Execute(principal, uow, personRep);
				checkWebPermission(principal, person);
			}

			var sessionSpecificData = new SessionSpecificData(businessUnitId, dataSourceName, personId, authenticationType);
			_sessionSpecificDataProvider.Store(sessionSpecificData);
		}

		private static void checkWebPermission(TeleoptiPrincipal principal, IPerson person)
		{
			var allowed = principal.PrincipalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb);
			if (!allowed)
				throw new PermissionException("You (" + person.Name + ") don't have permission to access the web portal.");
		}
	}
}