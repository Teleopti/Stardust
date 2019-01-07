using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
	public class WebLogOn : IWebLogOn
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ILogOnOff _logOnOff;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly ISessionSpecificWfmCookieProvider _sessionSpecificWfmCookieProvider;
		private readonly IAuthorization _authorization;
		private readonly ITokenIdentityProvider _tokenIdentityProvider;

		public WebLogOn(
			ILogOnOff logOnOff,
			IDataSourceForTenant dataSourceForTenant,
			IRepositoryFactory repositoryFactory,
			ISessionSpecificWfmCookieProvider sessionSpecificWfmCookieProvider,
			IAuthorization authorization,
			ITokenIdentityProvider tokenIdentityProvider)
		{
			_logOnOff = logOnOff;
			_dataSourceForTenant = dataSourceForTenant;
			_repositoryFactory = repositoryFactory;
			_sessionSpecificWfmCookieProvider = sessionSpecificWfmCookieProvider;
			_authorization = authorization;
			_tokenIdentityProvider = tokenIdentityProvider;
		}

		public void LogOn(string dataSourceName, Guid businessUnitId, IPerson person, string tenantPassword, bool isPersistent, bool isLogonFromBrowser)
		{
			var dataSource = _dataSourceForTenant.Tenant(dataSourceName);
			using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
			{
				var businessUnit = _repositoryFactory.CreateBusinessUnitRepository(uow).Get(businessUnitId);
				_logOnOff.LogOn(dataSource, person, businessUnit);

				// why just load all but discard any result from the server?
				_repositoryFactory.CreateApplicationFunctionRepository(uow).LoadAll();

				var allowed =	_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb) ||
								_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.Anywhere) ||
								_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.AccessToReports) ||
								_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebPermissions);

				if (!allowed)
					throw new PermissionException("You (" + person.Name + ") don't have permission to access the web portal.");
			}

			var currentToken = _tokenIdentityProvider.RetrieveToken();
			_tokenIdentityProvider.GetHashCode();
			var sessionSpecificData = new SessionSpecificData(businessUnitId, dataSourceName, person.Id.GetValueOrDefault(), tenantPassword, currentToken.IsTeleoptiApplicationLogon);
			_sessionSpecificWfmCookieProvider.StoreInCookie(sessionSpecificData, isPersistent, isLogonFromBrowser, dataSourceName);
			_sessionSpecificWfmCookieProvider.RemoveAuthBridgeCookie();
		}
	}
}