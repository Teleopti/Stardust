using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebSettings)]
	public class SkillController : ApiController
	{
		private readonly IActivityProvider _activityProvider;

		public SkillController(IActivityProvider activityProvider)
		{
			_activityProvider = activityProvider;
		}

		[HttpGet, Route("api/Forecasting/Activities"), UnitOfWork]
		public virtual IEnumerable<ActivityViewModel> Activities()
		{
			return _activityProvider.GetAll();
		}
	}
}