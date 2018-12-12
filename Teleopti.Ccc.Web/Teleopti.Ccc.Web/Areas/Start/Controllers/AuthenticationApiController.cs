using System.Web.Http;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class AuthenticationApiController : ApiController
	{
		private readonly IBusinessUnitsViewModelFactory _businessUnitViewModelFactory;
		private readonly IIdentityLogon _identityLogon;
		private readonly ILogLogonAttempt _logLogonAttempt;
		private readonly IWebLogOn _webLogon;
		private readonly IDataSourceForTenant _dataSourceForTenant;

		public AuthenticationApiController(IBusinessUnitsViewModelFactory businessUnitViewModelFactory,
			IIdentityLogon identityLogon,
			ILogLogonAttempt logLogonAttempt,
			IWebLogOn webLogon,
			IDataSourceForTenant dataSourceForTenant)
		{
			_businessUnitViewModelFactory = businessUnitViewModelFactory;
			_identityLogon = identityLogon;
			_logLogonAttempt = logLogonAttempt;
			_webLogon = webLogon;
			_dataSourceForTenant = dataSourceForTenant;
		}

		[HttpGet, Route("start/authenticationapi/businessunits")]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual IHttpActionResult BusinessUnits()
		{
			var result = _identityLogon.LogonIdentityUser();
			if (!result.Successful)
				return Ok("NoUserFound");
			var businessUnits = _businessUnitViewModelFactory.BusinessUnits(result.DataSource, result.Person);
			return Ok(businessUnits);
		}

		[HttpPost, Route("start/authenticationapi/logon")]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual IHttpActionResult Logon([FromBody] ApiLogonInputModel model)
		{
			var result = _identityLogon.LogonIdentityUser();
			_logLogonAttempt.SaveAuthenticateResult(string.Empty, result.PersonId(), result.Successful);
			if (!result.Successful)
				return errorMessage(Resources.LogOnFailedInvalidUserNameOrPassword);

			try
			{
				_webLogon.LogOn(result.DataSource.DataSourceName, model.BusinessUnitId, result.Person,
					result.TenantPassword, result.IsPersistent, model.IsLogonFromBrowser);
			}
			catch (PermissionException)
			{
				return errorMessage(Resources.InsufficientPermissionForWeb);
			}
			catch (InvalidLicenseException)
			{
				_dataSourceForTenant.RemoveDataSource(result.DataSource.DataSourceName);
				return errorMessage(Resources.TeleoptiProductActivationKeyException);
			}

			return Ok(new { });
		}

		private IHttpActionResult errorMessage(string message)
		{
			ModelState.AddModelError("Error", message);
			return BadRequest(ModelState);
		}
	}
}