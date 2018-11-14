using System.Web.Http;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class AuthenticateController : ApiController
	{
		private readonly IApplicationAuthentication _applicationAuthentication;
		private readonly IIdentityAuthentication _identityAuthentication;
		private readonly IIdAuthentication _idAuthentication;
		private readonly ILogLogonAttempt _logLogonAttempt;


		public AuthenticateController(IApplicationAuthentication applicationAuthentication, 
													IIdentityAuthentication identityAuthentication,
													IIdAuthentication idAuthentication,
													ILogLogonAttempt logLogonAttempt)
		{
			_applicationAuthentication = applicationAuthentication;
			_identityAuthentication = identityAuthentication;
			_idAuthentication = idAuthentication;
			_logLogonAttempt = logLogonAttempt;
		}

		[HttpPost, Route("Authenticate/ApplicationLogon")]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual IHttpActionResult ApplicationLogon([FromBody]ApplicationLogonModel applicationLogonModel)
		{
			var res = _applicationAuthentication.Logon(applicationLogonModel.UserName, applicationLogonModel.Password);
			_logLogonAttempt.SaveAuthenticateResult(applicationLogonModel.UserName, res.PersonId, res.Success);
			return Ok(res);
		}

		[HttpPost, Route("Authenticate/IdentityLogon")]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual IHttpActionResult IdentityLogon([FromBody]IdentityLogonModel identityLogonModel)
		{
			var res = _identityAuthentication.Logon(identityLogonModel.Identity);
			_logLogonAttempt.SaveAuthenticateResult(identityLogonModel.Identity, res.PersonId, res.Success);
			return Ok(res);
		}

		[HttpPost, Route("Authenticate/IdLogon")]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual IHttpActionResult IdLogon([FromBody]IdLogonModel idLogonModel)
		{
			var res = _idAuthentication.Logon(idLogonModel.Id);
			// don't think we should do this here because this is called after the actual logon
			//_logLogonAttempt.SaveAuthenticateResult(idLogonModel.Id.ToString(), res.PersonId, res.Success);
			return Ok(res);
		}
	}
}