using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.SystemSetting
{
	public class SystemSettingController : ApiController
	{
		private readonly IAuthorization _authorization;

		public SystemSettingController(IAuthorization authorization)
		{
			_authorization = authorization;
		}


		[HttpGet, Route("api/SystemSetting/HasPermission"), UnitOfWork]
		public virtual bool HasSystemSettingPermission()
		{
			return _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.SystemSetting);
		}
	}
}