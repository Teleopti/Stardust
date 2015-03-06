using System.Web.Mvc;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Tenant.Core;

namespace Teleopti.Ccc.Web.Areas.Tenant
{
	public class AuthenticateController : Controller
	{
		private readonly IApplicationAuthentication _applicationAuthentication;
		private readonly IIdentityAuthentication _identityAuthentication;
		private readonly ILogLogonAttempt _logLogonAttempt;


		public AuthenticateController(IApplicationAuthentication applicationAuthentication, 
													IIdentityAuthentication identityAuthentication,
													ILogLogonAttempt logLogonAttempt)
		{
			_applicationAuthentication = applicationAuthentication;
			_identityAuthentication = identityAuthentication;
			_logLogonAttempt = logLogonAttempt;
		}

		[HttpPost]
		[TenantUnitOfWork]
		public virtual JsonResult ApplicationLogon(string userName, string password)
		{
			var res = _applicationAuthentication.Logon(userName, password);
			var authResult = hackForNow_whenRemovingOldCodeWeDontNeedAuthenticateResultHere_ButWeCanChangeTheInterfaceToAcceptPrimitives(res);
			_logLogonAttempt.SaveAuthenticateResult(userName, authResult);
			return Json(res);
		}

		[HttpPost]
		[TenantUnitOfWork]
		public virtual JsonResult IdentityLogon(string identity)
		{
			var res = _identityAuthentication.Logon(identity);
			var authResult = hackForNow_whenRemovingOldCodeWeDontNeedAuthenticateResultHere_ButWeCanChangeTheInterfaceToAcceptPrimitives(res);
			_logLogonAttempt.SaveAuthenticateResult(identity, authResult);
			return Json(res);
		}

		private static AuthenticateResult hackForNow_whenRemovingOldCodeWeDontNeedAuthenticateResultHere_ButWeCanChangeTheInterfaceToAcceptPrimitives(ApplicationAuthenticationResult res)
		{
			var tempPerson = new Person();
			tempPerson.SetId(res.PersonId);
			return new AuthenticateResult {Person = tempPerson, Successful = res.Success};
		}

	}
}