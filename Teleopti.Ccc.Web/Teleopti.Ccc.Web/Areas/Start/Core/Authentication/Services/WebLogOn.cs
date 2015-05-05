﻿using System;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class WebLogOn : IWebLogOn
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ILogOnOff _logOnOff;
		private readonly IApplicationData _applicationData;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IPrincipalAuthorization _principalAuthorization;

		public WebLogOn(ILogOnOff logOnOff,
										IApplicationData applicationData,
		                IRepositoryFactory repositoryFactory,
		                ISessionSpecificDataProvider sessionSpecificDataProvider,
		                IRoleToPrincipalCommand roleToPrincipalCommand,
		                ICurrentTeleoptiPrincipal currentTeleoptiPrincipal,
		                IPrincipalAuthorization principalAuthorization)
		{
			_logOnOff = logOnOff;
			_applicationData = applicationData;
			_repositoryFactory = repositoryFactory;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_principalAuthorization = principalAuthorization;
		}

		public void LogOn(string dataSourceName, Guid businessUnitId, Guid personId)
		{
			var dataSource = _applicationData.Tenant(dataSourceName);
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRep = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRep.Get(personId);
				var businessUnit = _repositoryFactory.CreateBusinessUnitRepository(uow).Get(businessUnitId);
				_logOnOff.LogOn(dataSource, person, businessUnit);
				var principal = _currentTeleoptiPrincipal.Current();
				_roleToPrincipalCommand.Execute(principal, dataSource.Application, personRep);
				_repositoryFactory.CreateApplicationFunctionRepository(uow).LoadAll();

				var allowed =	_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb) ||
								_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere) ||
								_principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AccessToReports);

				if (!allowed)
					throw new PermissionException("You (" + person.Name + ") don't have permission to access the web portal.");
			}

			var sessionSpecificData = new SessionSpecificData(businessUnitId, dataSourceName, personId);
			_sessionSpecificDataProvider.StoreInCookie(sessionSpecificData);
		}
	}
}