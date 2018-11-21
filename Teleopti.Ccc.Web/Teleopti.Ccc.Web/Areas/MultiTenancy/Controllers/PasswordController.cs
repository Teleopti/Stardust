using System.Web.Http;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class PasswordController : ApiController
	{
		private readonly IPasswordManager _passwordManager;
		private const string Error_TokenInvalid = "Error_TokenInvalid";
		private const string Error_RequestFailed = "Error_RequestFailed";

		public PasswordController(IPasswordManager passwordManager)
		{
			_passwordManager = passwordManager;
		}

		[TenantUnitOfWork]
		[HttpPost,Route("ChangePassword/Modify")]
		[NoTenantAuthentication]
		public virtual void Modify([FromBody]ChangePasswordModel model)
		{
			_passwordManager.Modify(model.PersonId, model.OldPassword, model.NewPassword);
		}

		[EnabledBy(Toggles.Wfm_User_Password_Reset_74957)]
		[TenantUnitOfWork]
		[HttpPost, Route("ChangePassword/Reset")]
		[NoTenantAuthentication]
		public virtual IHttpActionResult Reset([FromBody]PasswordResetModel model)
		{
			var resetSuccess = _passwordManager.Reset(model?.NewPassword, model?.ResetToken);

			var resultModel = new BaseResultModel();
			if (resetSuccess == true || model == null)
			{
				resultModel.Errors.Add(Error_RequestFailed);
			}

			return Ok(resultModel);
		}

		[EnabledBy(Toggles.Wfm_User_Password_Reset_74957)]
		[TenantUnitOfWork]
		[HttpPost, Route("ChangePassword/RequestReset")]
		[NoTenantAuthentication]
		public virtual IHttpActionResult RequestReset([FromBody] RequestPasswordResetModel model)
		{
			var resultModel = new BaseResultModel();
			// Will this be handeled gracefully with loadbalancers? (peek TenantAdminInfoController)
			var vpRoot = RequestContext.VirtualPathRoot;
			var reqUri = RequestContext.Url.Request.RequestUri;
			var baseUri = $"{reqUri.Scheme}://{reqUri.Authority}{vpRoot}";

			var apiServiceCalledOk = _passwordManager.SendResetPasswordRequest(model?.UserIdentifier, baseUri);
			if (!apiServiceCalledOk || model == null)
			{
				resultModel.Errors.Add(Error_RequestFailed);
			}

			return Ok(resultModel);
		}

		[EnabledBy(Toggles.Wfm_User_Password_Reset_74957)]
		[TenantUnitOfWork]
		[HttpPost, Route("ChangePassword/ValidateToken")]
		[NoTenantAuthentication]
		public virtual IHttpActionResult ValidateToken([FromBody]PasswordResetModel model)
		{
			var tokenValid = _passwordManager.ValidateResetToken(model?.ResetToken);
			var resultModel = new BaseResultModel();

			if (!tokenValid || model == null)
			{
				resultModel.Errors.Add(Error_TokenInvalid);
			}

			return Ok(resultModel);
		}
	}
}