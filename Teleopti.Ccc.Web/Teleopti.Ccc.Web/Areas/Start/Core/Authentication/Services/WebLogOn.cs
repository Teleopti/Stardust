﻿using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class WebLogOn : IWebLogOn
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ILogOnOff _logOnOff;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		private readonly IRoleToPrincipalCommand _roleToPrincipalCommand;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		private readonly IAuthorization _authorization;

		public WebLogOn(
			ILogOnOff logOnOff,
			IDataSourceForTenant dataSourceForTenant,
			IRepositoryFactory repositoryFactory,
			ISessionSpecificDataProvider sessionSpecificDataProvider,
			IRoleToPrincipalCommand roleToPrincipalCommand,
			ICurrentTeleoptiPrincipal currentTeleoptiPrincipal,
			IAuthorization authorization
			)
		{
			_logOnOff = logOnOff;
			_dataSourceForTenant = dataSourceForTenant;
			_repositoryFactory = repositoryFactory;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
			_roleToPrincipalCommand = roleToPrincipalCommand;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
			_authorization = authorization;
		}

		public void LogOn(string dataSourceName, Guid businessUnitId, Guid personId, string tenantPassword, bool isPersistent, bool isLogonFromBrowser)
		{
			var dataSource = _dataSourceForTenant.Tenant(dataSourceName);
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var personRep = _repositoryFactory.CreatePersonRepository(uow);
				var person = personRep.Get(personId);
				var businessUnit = _repositoryFactory.CreateBusinessUnitRepository(uow).Get(businessUnitId);
				_logOnOff.LogOnWithoutClaims(dataSource, person, businessUnit);
				var principal = _currentTeleoptiPrincipal.Current();
				_roleToPrincipalCommand.Execute(principal, dataSource.Application, personRep);

				// why just load all but discard any result from the server?
				_repositoryFactory.CreateApplicationFunctionRepository(uow).LoadAll();

				var allowed =	_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb) ||
								_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere) ||
								_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AccessToReports) ||
								_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage);

				if (!allowed)
					throw new PermissionException("You (" + person.Name + ") don't have permission to access the web portal.");
			}

			var sessionSpecificData = new SessionSpecificData(businessUnitId, dataSourceName, personId, tenantPassword);
			_sessionSpecificDataProvider.StoreInCookie(sessionSpecificData, isPersistent, isLogonFromBrowser);
			_sessionSpecificDataProvider.RemoveAuthBridgeCookie();
		}
	}
}