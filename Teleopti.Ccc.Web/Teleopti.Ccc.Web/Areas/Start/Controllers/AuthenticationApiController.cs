using System.Web.Http;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class AuthenticationApiController : ApiController
	{
		private readonly IBusinessUnitsViewModelFactory _businessUnitViewModelFactory;
		private readonly IIdentityLogon _identityLogon;
		private readonly ILogLogonAttempt _logLogonAttempt;
		private readonly IWebLogOn _webLogon;
		private readonly IDataSourceForTenant _dataSourceForTenant;
		private readonly IToggleManager _toggleManager;

		public AuthenticationApiController(IBusinessUnitsViewModelFactory businessUnitViewModelFactory,
																					IIdentityLogon identityLogon,
																					ILogLogonAttempt logLogonAttempt,
																					IWebLogOn webLogon,
																					IDataSourceForTenant dataSourceForTenant,
																					IToggleManager toggleManager)
		{
			_businessUnitViewModelFactory = businessUnitViewModelFactory;
			_identityLogon = identityLogon;
			_logLogonAttempt = logLogonAttempt;
			_webLogon = webLogon;
			_dataSourceForTenant = dataSourceForTenant;
			_toggleManager = toggleManager;
		}

		[HttpGet,Route("start/authenticationapi/businessunits")]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual IHttpActionResult BusinessUnits()
		{
			var result = _identityLogon.LogonIdentityUser();
			if (!result.Successful)
				return errorMessage("Unknown error");
			var businessUnits = _businessUnitViewModelFactory.BusinessUnits(result.DataSource, result.Person);
			return Ok(businessUnits);
		}

		[HttpPost, Route("start/authenticationapi/logon")]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual IHttpActionResult Logon([FromBody]ApiLogonInputModel model)
		{
			var result = _identityLogon.LogonIdentityUser();
			_logLogonAttempt.SaveAuthenticateResult(string.Empty, result.PersonId(), result.Successful);
			if (!result.Successful)
				return errorMessage(Resources.LogOnFailedInvalidUserNameOrPassword);
			try
			{
				_webLogon.LogOn(result.DataSource.DataSourceName, model.BusinessUnitId, result.Person.Id.Value, result.TenantPassword, result.IsPersistent);
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

			var isToggleOpen = _toggleManager.IsEnabled(Toggles.MyTimeWeb_KeepUrlAfterLogon_34762);
			return Ok(new { MyTimeWeb_KeepUrlAfterLogon_34762 = isToggleOpen });
		}

		private IHttpActionResult errorMessage(string message)
		{
			ModelState.AddModelError("Error", message);
			return BadRequest(ModelState);
		}
	}
}