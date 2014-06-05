using System;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class WebLogOn : IWebLogOn
	{
		private readonly IDataSourcesProvider _dataSourceProvider;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ILogOnOff _logOnOff;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IPrincipalAuthorization _principalAuthorization;

		public WebLogOn(ILogOnOff logOnOff,
		                IDataSourcesProvider dataSourceProvider,
		                IRepositoryFactory repositoryFactory,
		                ISessionSpecificDataProvider sessionSpecificDataProvider,
		                IRoleToPrincipalCommand roleToPrincipalCommand,
		                ICurrentTeleoptiPrincipal currentTeleoptiPrincipal,
		                IPrincipalAuthorization principalAuthorization)
		{
			_logOnOff = logOnOff;
			_dataSourceProvider = dataSourceProvider;
			_repositoryFactory = repositoryFactory;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_principalAuthorization = principalAuthorization;
		}

		public void LogOn(string dataSourceName, Guid businessUnitId, Guid personId)
		{
			var dataSource = _dataSourceProvider.RetrieveDataSourceByName(dataSourceName);
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRep = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRep.Get(personId);
				var businessUnit = _repositoryFactory.CreateBusinessUnitRepository(uow).Get(businessUnitId);
				_logOnOff.LogOn(dataSource, person, businessUnit);
				var principal = _currentTeleoptiPrincipal.Current();
				_roleToPrincipalCommand.Execute(principal, dataSource.Application, personRep);

				var allowed = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MobileReports) ||
							   _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb) ||
							   _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere);

				if (!allowed)
					throw new PermissionException("You (" + person.Name + ") don't have permission to access the web portal.");
			}

			var sessionSpecificData = new SessionSpecificData(businessUnitId, dataSourceName, personId);
			_sessionSpecificDataProvider.StoreInCookie(sessionSpecificData);
		}
	}
}