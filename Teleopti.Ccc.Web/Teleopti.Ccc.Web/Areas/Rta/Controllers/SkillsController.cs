using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Skill;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview)]
	public class SkillsController : ApiController
	{
		private readonly SkillViewModelBuilder _viewModelBuilder;

		public SkillsController(SkillViewModelBuilder viewModelBuilder)
		{
			_viewModelBuilder = viewModelBuilder;
		}

		[UnitOfWork, HttpGet, Route("api/Skills")]
		public virtual IHttpActionResult Index()
		{
			return Ok(_viewModelBuilder.Build());
		}
	}
}