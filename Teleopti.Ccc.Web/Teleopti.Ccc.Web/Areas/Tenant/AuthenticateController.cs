using System.Web.Mvc;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Ccc.Web.Areas.Tenant.Model;

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
		public virtual JsonResult ApplicationLogon(ApplicationLogonModel applicationLogonModel)
		{
			var res = _applicationAuthentication.Logon(applicationLogonModel.UserName, applicationLogonModel.Password);
			var authResult = hackForNow_whenRemovingOldCodeWeDontNeedAuthenticateResultHere_ButWeCanChangeTheInterfaceToAcceptPrimitives(res);
			_logLogonAttempt.SaveAuthenticateResult(applicationLogonModel.UserName, authResult);
			return Json(res);
		}

		[HttpPost]
		[TenantUnitOfWork]
		public virtual JsonResult IdentityLogon(IdentityLogonModel identityLogonModel)
		{
			var res = _identityAuthentication.Logon(identityLogonModel.Identity);
			var authResult = hackForNow_whenRemovingOldCodeWeDontNeedAuthenticateResultHere_ButWeCanChangeTheInterfaceToAcceptPrimitives(res);
			_logLogonAttempt.SaveAuthenticateResult(identityLogonModel.Identity, authResult);
			return Json(res);
		}

		//TODO: tenant - this should be removed
		private static AuthenticateResult hackForNow_whenRemovingOldCodeWeDontNeedAuthenticateResultHere_ButWeCanChangeTheInterfaceToAcceptPrimitives(ApplicationAuthenticationResult res)
		{
			var tempPerson = new Person();
			tempPerson.SetId(res.PersonId);
			return new AuthenticateResult {Person = tempPerson, Successful = res.Success};
		}
	}
}